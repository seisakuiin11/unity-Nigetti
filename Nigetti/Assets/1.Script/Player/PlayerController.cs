using ComonData;
using Fusion;
using Fusion.Addons.KCC;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    const sbyte ZITTAI = 1, RETAI = -1;

    enum ActionState : sbyte
    {
        NONE,
        STANBY,
        ACTION,
        END,
    }

    [Networked] int charaNum { get; set; }                  // 0:幽霊 1:キョンシー 2:九尾 3:鬼
    [Networked] int turn { get; set; }                      // 実体 or 零体
    [Networked] int soulCount { get; set; }                 // 魂の所持数
    [Networked] float RunSpeed { get; set; }                // 走るときのスピード
    [Networked] TickTimer NotChangeTurnTimer { get; set; }  // 実体零体の切り替えができない時間
    [Networked] TickTimer StunTimer { get; set; }           // 移動できない時間
    [Networked] NetworkBool IsPlaying { get; set; }         // 操作可能か 移動可能か
    [Networked] TickTimer WalkTimer { get; set; }           // 走れない時間(歩くことになる)
    [Networked] ActionState attackState { get; set; }       // 攻撃状態管理
    [Networked] TickTimer NotAttackTimer { get; set; }      // 攻撃できない時間
    [Networked] ActionState teleportState { get; set; }     // テレポート状態管理
    [Networked] TickTimer NotTeleportTimer { get; set; }    // テレポートできない時間
    [Networked] ActionState invisibleState { get; set; }    // 透明化状態管理
    [Networked] TickTimer InvisibleTimer { get; set; }      // 透明化時間
    [Networked] TickTimer NotInvisibleTimer { get; set; }   // 透明化できない時間
    [Networked] NetworkBool isMoveFlag { get; set; }        // 動いているかフラグ（アニメーション用）
    [Networked] Vector3 tpPos1 { get; set; }                // テレポートポジション（プレイヤーの過去位置）
    [Networked] Vector3 tpPos2 { get; set; }                // テレポートポジション（プレイヤーのテレポート先）

    [SerializeField] float RotationSpeed = 20f;             // 振り返る時の回転速度
    [SerializeField] float RunSpeedZittai = 6.5f;           // 走る時のスピード
    [SerializeField] float RunSpeedRetai = 5f;              // 走る時のスピード
    [SerializeField] float WalkSpeed = 2f;                  // 歩く時のスピード
    [SerializeField] float ChangeTurnCoolTime = 0.5f;       // 実体零体の切り替えクールタイム
    [SerializeField] float StunTime = 5f;                   // 動けない時間
    [SerializeField] float AttackCoolTime = 1.5f;           // 攻撃クールタイム
    [SerializeField] float AttackAnimTime = 0.5f;           // 攻撃アニメーション時間
    [SerializeField] float TeleportCoolTime = 2f;           // テレポートクールタイム
    [SerializeField] int SoulCostTeleport = 1;              // 魂消費 テレポート
    [SerializeField] float InvisibleTime = 2f;              // 透明化時間
    [SerializeField] float InvisibleCoolTime = 4f;          // 透明化クールタイム
    [SerializeField] int SoulCostInvisible = 2;             // 魂消費 透明化
    [SerializeField] float FreeModeSkillCoolTime = 0.2f;    // フリーモード時のスキルクールタイム
    [SerializeField] GameObject skin;
    [SerializeField] Animator anim;
    [SerializeField] GameObject attack_c;
    [SerializeField] GameObject tpObj_c;
    [SerializeField] GameObject[] tpObj_obj;
    [SerializeField] EffectScript effects;
    [SerializeField] ZittaiSkillBase[] zittaiSkills;

    bool freeMode;                  // 自由にプレイできるか (実体零体の切り替えの自由,クールタイムの短縮)
    bool menuOpen;                  // UI操作中か
    int charaNumOld = -1;           // キャラID ローカル
    int turnOld;                    // 実体零体 ローカル
    ActionState attackStateOld;     // 攻撃状態管理 ローカル
    ActionState teleportStateOld;   // テレポート状態管理 ローカル
    ActionState invisibleStateOld;  // 透明化状態管理 ローカル
    bool attackAnimPlayFlag;        // 攻撃アニメーションを再生中かどうか
    TickTimer AttackAnimTimer;      // 攻撃コライダーが存在する時間
    TickTimer ChangeSpeedTimer;     // スピード変化時間
    float saveSpeed;                // スピード保存用
    Action OnHit;                   // 攻撃が当たっときに呼ぶ処理

    KCC _kcc;
    ZittaiSkillBase zittaiSkill;
    CharcterDirecter charcters;
    PlayerAudioScript audioP;
    UISkillDirecter skillUI_d;

    /* ----------------------------スタート処理---------------------------------- */
    // 生成されたときに呼ばれる
    public override void Spawned()
    {
        _kcc = GetComponent<KCC>();
        charcters = GetComponent<CharcterDirecter>();
        audioP = GetComponent<PlayerAudioScript>();
        zittaiSkill = zittaiSkills[0];
        zittaiSkill.ResetSkill();
        attack_c.SetActive(false);

        if (Object.HasInputAuthority) // 操作者本人なら
        {
            // UIの設定
            skillUI_d = FindAnyObjectByType<UISkillDirecter>();
            skillUI_d.Init();
        }
    }
    void Awake()
    {

    }

    /* ----------------------- アップデート処理 ----------------------------------- */
    public override void FixedUpdateNetwork()
    {
        // 入力情報をもとにした各種アクション ------------------
        if(GetInput(out NetworkInputData data))
        {
            //移動
            MoveAction(data);

            //実体の処理
            if (turn == ZITTAI) ZittaiAction(data);
            //零体の処理
            else if (turn == RETAI) RetaiAction(data);

            // フリーモード時の処理
            FreeModeAction(data);
        }

        // 実体スキル
        zittaiSkill.UpdateMethod();

        // TickTimer 計算系 -----------------------------------
        if (!Object.HasStateAuthority) return;

        // 操作可能か 移動可能か
        IsPlaying = IsCanMove();

        // 実体零体切り替えクールタイム
        if(NotChangeTurnTimer.Expired(Runner)) NotChangeTurnTimer = TickTimer.None;

        // スタン時間
        if(StunTimer.Expired(Runner)) StunTimer = TickTimer.None;

        // 強制歩き継続時間
        if(WalkTimer.Expired(Runner)) WalkTimer = TickTimer.None;

        // コライダー生存時間
        if (AttackAnimTimer.Expired(Runner)) { attack_c.SetActive(false); AttackAnimTimer = TickTimer.None; }

        // 攻撃クールタイム
        if(NotAttackTimer.Expired(Runner)) NotAttackTimer = TickTimer.None;

        // テレポートクールタイム
        if(NotTeleportTimer.Expired(Runner)) NotTeleportTimer = TickTimer.None;

        // 透明化中
        if(InvisibleTimer.Expired(Runner)) { InvisibleEnd(); InvisibleTimer = TickTimer.None; }

        // 透明化クールタイム
        if(NotInvisibleTimer.Expired(Runner)) NotInvisibleTimer = TickTimer.None;

        // スピード変化時間
        if (ChangeSpeedTimer.Expired(Runner)) { RunSpeed = saveSpeed; ChangeSpeedTimer = TickTimer.None; }
    }

    /* ----------------------------- 描画関係処理 --------------------------------------- */
    public override void Render()
    {
        // 移動アニメーション -------------------------------------
        anim.SetBool("move", isMoveFlag);

        // 走る歩くアニメーション　スピード切り替え ---------------
        anim.SetBool("Run", !WalkTimer.IsRunning);

        // 攻撃アニメーション (振り上げ) --------------------------
        if (attackState != attackStateOld && attackStateOld == ActionState.NONE)
        {
            attackStateOld = ActionState.STANBY;
            anim.SetBool("Armup", true);
        }
        // 攻撃アニメーション (振り下ろし)
        else if (attackState != attackStateOld && attackStateOld == ActionState.STANBY)
        {
            attackStateOld = ActionState.ACTION;

            anim.SetBool("Armup", false);
            effects.EffectFollowType(EffectNum.TOUCH, 0);
            audioP.AttackSound();

            attackAnimPlayFlag = true;
            // アニメーション再生終了時
            Task.Run(async () => {
                await Task.Delay((int)(AttackAnimTime * 1000)); // ミリ秒変換
                attackAnimPlayFlag = false;
            });

            // Hostが状態を戻す
            if (Object.HasStateAuthority) attackState = ActionState.NONE;
        }
        else if(attackState != attackStateOld && attackStateOld == ActionState.ACTION)
        {
            attackStateOld = ActionState.NONE;
        }

        // テレポートエフェクト (準備) ----------------------------
        if(teleportState != teleportStateOld && teleportStateOld == ActionState.NONE)
        {
            teleportStateOld = ActionState.STANBY;

            // 予測着地点
            tpObj_c.SetActive(true);
            //相手に見えないように
            if (!Object.HasInputAuthority)
                foreach (var obj in tpObj_obj) obj.SetActive(false);
        }
        // テレポートエフェクト (実行)
        else if(teleportState != teleportStateOld && teleportStateOld == ActionState.STANBY)
        {
            teleportStateOld = ActionState.ACTION;

            tpObj_c.SetActive(false);
            audioP.NigeSkillSound(0);
            effects.EffectFixedType(EffectNum.TP, 0f, tpPos1);
            effects.EffectFixedType(EffectNum.TP, 0f, tpPos2);

            // Hostが状態を戻す
            if (Object.HasStateAuthority) teleportState = ActionState.NONE;
        }
        else if (teleportState != teleportStateOld && teleportStateOld == ActionState.ACTION)
        {
            teleportStateOld = ActionState.NONE;
        }

        //透明化エフェクト (開始) ---------------------------------
        if (invisibleState != invisibleStateOld && invisibleStateOld == ActionState.NONE)
        {
            invisibleStateOld = ActionState.ACTION;

            // 操作者本人かどうか
            if (Object.HasInputAuthority) charcters.InvisivleSkin(true);
            else skin.SetActive(false);
            audioP.NigeSkillSound(1);
            effects.EffectFollowType(EffectNum.SMOKE, 0);

        }
        // 透明化エフェクト (終了)
        else if (invisibleState != invisibleStateOld && invisibleStateOld == ActionState.ACTION)
        {
            invisibleStateOld = ActionState.END;

            // 操作者本人かどうか
            if (Object.HasInputAuthority) charcters.InvisivleSkin(false);
            else skin.SetActive(true);

            if (Object.HasStateAuthority) invisibleState = ActionState.NONE;
        }
        else if (invisibleState != invisibleStateOld && invisibleStateOld == ActionState.END)
        {
            invisibleStateOld = ActionState.NONE;
        }

        // 実体スキル 描画系
        zittaiSkill.RenderMethod();

        //キャラクターの見た目切り替え -----------------------------
        if(charaNum != charaNumOld)
        {
            charaNumOld = charaNum;
            CharaChange();       // キャラを変える
        }

        //実体零体切り替え -----------------------------------------
        if(turn != turnOld)
        {
            turnOld = turn;
            SkinChange();       // 見た目を変える
        }
    }
    /* ========================================================================== */

    public void Init(int _charaNum, int _soul, bool _freeMode)
    {
        charaNum = _charaNum;
        turn = ZITTAI;
        soulCount = _soul;
        RunSpeed = RunSpeedZittai;
        freeMode = _freeMode;

        // すべてのスキルの初期化
        foreach (var skill in zittaiSkills) skill.Init(this, _freeMode);
    }

    // Get Set ------------------------------------------------------
    // 動けるかどうか
    bool IsCanMove()
    {
        // スタン状態
        if(StunTimer.IsRunning) return false;
        // メニューを開いている
        if(menuOpen) return false;

        return true;
    }

    /// <summary>
    /// メニュー画面を表示中かどうか (動けなくするかどうか)
    /// </summary>
    public void SetMenuOpenFlag(bool flag) => RPC_OpenMenu(flag);

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_OpenMenu(bool flag) => menuOpen = flag;

    /// <summary>
    /// 魂の所持数に加算する
    /// </summary>
    public void SetAddSoul(int Soul) { soulCount += Soul; }
    /// <summary>
    /// 魂の所持数を返す
    /// </summary>
    public int GetSoul() { return soulCount; }

    /// <summary>
    /// 現在のターンを返す 実体or零体 鬼or逃げ
    /// </summary>
    public int GetTurn() { return turn; }

    /// <summary>
    /// キャラクターIDを返す
    /// </summary>
    public int GetCharNum() { return charaNum; }

    /// <summary>
    /// タッチした時の処理を追加する
    /// </summary>
    /// <param name="onHitAction"></param>
    public void AddOnHitAction(Action onHitAction) => OnHit += onHitAction;

    /// <summary>
    /// タッチした時の処理を削除する
    /// </summary>
    /// <param name="onHitAction"></param>
    public void RemoveOnHitAction(Action onHitAction) => OnHit -= onHitAction;

    // ------------------------------------------------------------

    // 入力からプレイヤーの移動
    void MoveAction(NetworkInputData _data)
    {
        // 動けないなら
        if (!IsPlaying)
        {
            _kcc.SetInputDirection(Vector3.zero); // 固定 動かない
            return;
        }

        //進行方向 取得
        var direction = _data.direction.normalized;

        // キャラクターの回転処理(進行方向を向く)
        if (direction != Vector3.zero)
        {
            if(Object.HasStateAuthority) isMoveFlag = true;
            // 滑らかな回転を実現
            var currentRotation = _kcc.Data.TransformRotation;
            var targetRotation = Quaternion.LookRotation(direction);
            var nextRotation = Quaternion.Lerp(currentRotation, targetRotation, RotationSpeed * Runner.DeltaTime);
            _kcc.SetLookRotation(nextRotation.eulerAngles);
        }
        else if (Object.HasStateAuthority) isMoveFlag = false;

        // 移動
        float speed = WalkTimer.IsRunning ? WalkSpeed : RunSpeed;
        _kcc.SetInputDirection(direction * speed);
    }

    // 実体時の入力処理
    void ZittaiAction(NetworkInputData data)
    {
        if (!IsPlaying) return;

        //攻撃 (押したとき)
        if (data.attackStanby && !NotAttackTimer.IsRunning)
        {
            if (Object.HasStateAuthority) attackState = ActionState.STANBY;
        }
        // 攻撃 (放したとき)
        if (data.attackFlag && attackState == ActionState.STANBY)
        {
            Attack();
        }

        //スキル
        if (data.skillStanby)
        {
            zittaiSkill.PressButton();
        }
        if (data.skillFlag)
        {
            zittaiSkill.ReleaseButton();
        }
    }

    // 零体時の入力処理
    void RetaiAction(NetworkInputData data)
    {
        if (!IsPlaying) return;

        //テレポート
        if (data.attackStanby && !NotTeleportTimer.IsRunning)
        {
            if (Object.HasStateAuthority) teleportState = ActionState.STANBY;
        }
        if (data.attackFlag && teleportState == ActionState.STANBY)
        {
            Teleport();
        }

        //透明化 (押したとき)
        if (data.skillStanby && !InvisibleTimer.IsRunning && !NotInvisibleTimer.IsRunning)
        {
            Invisible();
        }
    }

    // 攻撃 捕まえる
    void Attack()
    {
        if (Object.HasStateAuthority)
        {
            attackState = ActionState.ACTION;

            // クールタイム設定
            NotAttackTimer = TickTimer.CreateFromSeconds(Runner, AttackCoolTime);
            // コライダー アクティブ可
            attack_c.SetActive(true);
        }

        // UI
        if (skillUI_d != null) skillUI_d.UISkill1(0f, AttackCoolTime, false);

        // アニメーション再生終了後、コライダー 非アクティブ化
        AttackAnimTimer = TickTimer.CreateFromSeconds(Runner, AttackAnimTime);
    }

    // テレポート実行
    void Teleport()
    {
        // Y軸 同一化
        Vector3 pos = tpObj_c.transform.position;
        pos.y = transform.position.y;

        tpPos1 = transform.position; // プレイヤーの過去位置
        tpPos2 = pos;                // テレポート先の位置

        // インターバル時間 設定
        float time = freeMode ? FreeModeSkillCoolTime : TeleportCoolTime;

        // ポジションを共有してから、状態変更
        if (Object.HasStateAuthority) teleportState = ActionState.ACTION;

        // TP
        _kcc.SetPosition(pos);

        // 魂消費 クールタイム設定
        if (Object.HasStateAuthority)
        {
            soulCount -= SoulCostTeleport;
            NotTeleportTimer = TickTimer.CreateFromSeconds(Runner, time);
        }

        // UI
        if (skillUI_d != null) skillUI_d.UISkill1(0f, time, false);
    }

    // 透明化
    void Invisible()
    {
        if (Object.HasStateAuthority)
        {
            invisibleState = ActionState.ACTION;

            // 魂消費 透明化継続時間設定
            soulCount -= SoulCostInvisible;
            InvisibleTimer = TickTimer.CreateFromSeconds(Runner, InvisibleTime);
        }

        // UI
        if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false);
    }
    // 透明化 終了
    void InvisibleEnd()
    {
        // 使用できない時間を設定
        float time = freeMode ? FreeModeSkillCoolTime : InvisibleCoolTime;

        if (Object.HasStateAuthority)
        {
            invisibleState = ActionState.END;

            // クールタイム設定
            NotInvisibleTimer = TickTimer.CreateFromSeconds(Runner, time);
        }

        // UI処理
        if (skillUI_d != null) skillUI_d.UISkill2(0f, time, false);
    }

    // ロビーでの入力処理
    void FreeModeAction(NetworkInputData data)
    {
        if (!freeMode) return;
        if(!IsPlaying) return;

        // 実体零体切り替え
        if (data._change != 0 && !NotChangeTurnTimer.IsRunning)
        {
            ChangeTurn();
            NotChangeTurnTimer = TickTimer.CreateFromSeconds(Runner, ChangeTurnCoolTime);
        }
    }

    /// <summary>
    /// 実体零体を切り替える
    /// </summary>
    public void ChangeTurn()
    {
        if (!Object.HasStateAuthority) return;

        SkillReset();
        turn = turn == ZITTAI ? RETAI : ZITTAI;
        RunSpeed = turn == ZITTAI ? RunSpeedZittai : RunSpeedRetai;
    }

    /// <summary>
    /// キャラクターを変える
    /// </summary>
    /// <param name="CharNum">キャラクターID</param>
    public void ChangeCharacter(int CharNum)
    {
        if (!Object.HasStateAuthority) return;

        ActionReset();
        charaNum = CharNum;
    }

    /// <summary>
    /// ダメージを与えた
    /// </summary>
    public void GiveHit(int takeSoul)
    {
        if (freeMode) return;

        if (!Object.HasStateAuthority) return;

        ChangeTurn();
        SetAddSoul(takeSoul); // 魂を奪う

        OnHit?.Invoke();
    }

    /// <summary>
    /// ダメージを受けた 鬼と逃げが入れ替わる
    /// </summary>
    public void Damage(int giveSoul)
    {
        if (freeMode) return;

        if (!Object.HasStateAuthority) return;
        // ホストなら 自身と相手の実体零体を切り替える

        ChangeTurn();
        SetAddSoul(-giveSoul); // 魂を取られる
        SetStunTime(); // 動けなくする 5秒
        RPC_StunCountUI(); // カウントダウンを表示する
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_StunCountUI()
    {
        skillUI_d.CountDown();
    }

    /// <summary>
    /// 行動制限 数秒動けなくなる
    /// </summary>
    public void SetStunTime(float time = 0f)
    {
        if (!Object.HasStateAuthority) return;

        if (time <= 0f) time = StunTime;

        StunTimer = TickTimer.CreateFromSeconds(Runner, time);
    }

    /// <summary>
    /// 走れなくする
    /// </summary>
    /// <param name="time">継続時間</param>
    public void DisableRunning(float time)
    {
        if (!Object.HasStateAuthority) return;

        WalkTimer = TickTimer.CreateFromSeconds(Runner, time);
        RPC_EffectPlay(EffectNum.DEBUFF, time);
    }

    /// <summary>
    /// スキルを使用できなくする
    /// </summary>
    /// <param name="time"></param>
    public void DisableSkill(float time)
    {
        if (!Object.HasStateAuthority) return;

        SkillReset();

        // テレポート クールタイム中
        NotTeleportTimer = TickTimer.CreateFromSeconds(Runner, time);

        // 透明化 クールタイム中
        NotInvisibleTimer = TickTimer.CreateFromSeconds(Runner, time);
    }

    /// <summary>
    /// 指定時間、スピードを変える
    /// </summary>
    /// <param name="_speed"></param>
    /// <param name="_time"></param>
    public void ChangeSpeed(float _speed, float _time)
    {
        saveSpeed = RunSpeed;
        RunSpeed = _speed;
        ChangeSpeedTimer = TickTimer.CreateFromSeconds(Runner, _time);
    }

    // テレポート&透明化の効果,クールタイムをリセットする
    void SkillReset()
    {
        // ホストのみがこの先のタイマーリセットを行う
        if(!Object.HasStateAuthority) return;

        RPC_SkillResetRender();

        // テレポート クールタイム中
        if (NotTeleportTimer.IsRunning) NotTeleportTimer = TickTimer.None;

        // 透明化 クールタイム中
        if (NotInvisibleTimer.IsRunning) NotInvisibleTimer = TickTimer.None;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SkillResetRender()
    {
        // テレポート 準備中
        if (teleportState == ActionState.STANBY)
        {
            teleportStateOld = ActionState.NONE;

            tpObj_c.SetActive(false);

            // Hostが状態を戻す
            if (Object.HasStateAuthority) teleportState = ActionState.NONE;
        }

        // 透明化中
        if (invisibleState == ActionState.ACTION)
        {
            invisibleStateOld = ActionState.NONE;

            // 操作者本人かどうか
            if (Object.HasInputAuthority) charcters.InvisivleSkin(false);
            else skin.SetActive(true);

            // Hostが状態を戻す
            if (Object.HasStateAuthority) { InvisibleTimer = TickTimer.None; invisibleState = ActionState.NONE; }
        }

        // UIのリセット
        if(skillUI_d != null) skillUI_d.ResetUI();
    }

    // 各種状態のリセット
    void ActionReset()
    {
        if (!Object.HasStateAuthority) return;

        RPC_ActionResetRender();

        // 走れない
        if (WalkTimer.IsRunning) WalkTimer = TickTimer.None;

        // 攻撃クールタイム中
        if (NotAttackTimer.IsRunning) NotAttackTimer = TickTimer.None;

        // テレポート&透明化 リセット
        SkillReset();

        // 鬼固有スキル リセット
        zittaiSkill.ResetSkill();
    }

    // 各種状態のリセット
    [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
    void RPC_ActionResetRender()
    {
        //攻撃リセット
        if (attackState == ActionState.STANBY)
        {
            attackStateOld = ActionState.NONE;

            anim.SetBool("Armup", false);
            anim.Play("None");

            if (Object.HasStateAuthority) attackState = ActionState.NONE;
        }

        // テレポート&透明化 リセット
        RPC_SkillResetRender();

        //UIリセット
        if (skillUI_d != null)
        {
            skillUI_d.UISkill1(1f, 0f, true);
            skillUI_d.UISkill2(1f, 0f, true);
        }
    }

    //キャラクターチェンジ
    void CharaChange() // 見た目の変更
    {
        skin = charcters.ChangeSkin(charaNum, turn);
        anim = charcters.Anim(charaNum);

        zittaiSkill = zittaiSkills[charaNum];
        zittaiSkill.ResetSkill();
    }

    // 実体零体の見た目の変更
    void SkinChange()
    {
        // 攻撃 (振り上げ中)
        if (attackState == ActionState.STANBY)
        {
            attackStateOld = ActionState.NONE;

            // Hostが状態を戻す
            if (Object.HasStateAuthority) attackState = ActionState.NONE;
        }
        // 振り下ろし中なら、見た目が切り替わった後もアニメーションを継続するための準備
        bool attackCheck = false;
        float passTime = 0;
        if (attackAnimPlayFlag)
        {
            attackCheck = true;
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(1);
            passTime = state.normalizedTime;
        }

        // 見た目の変更 -------------------------------------------------------------------------
        gameObject.tag = turn == ZITTAI ? "Oni" : "Nige";

        audioP.NigeSkillSound(1);
        effects.EffectFollowType(EffectNum.SMOKE, 0);
        effects.EffectFollowType(turn == ZITTAI ? EffectNum.ONICHANGE : EffectNum.NIGECHANGE, 0); // 自分が鬼か逃げかを認知するエフェクト

        // 見た目の適応
        CharaChange();

        // UI
        if (skillUI_d != null) skillUI_d.UIChange(turn);

        // 振り下ろし途中だったら、途中から再生する
        if (attackCheck)
        {
            anim.SetBool("Armup", false);
            anim.Play("hurisage", 1, passTime);
        }
        else // 振り下ろし途中じゃないなら
        {
            anim.SetBool("Armup", false);
            anim.Play("None");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_EffectPlay(EffectNum effect, float time)
    {
        effects.EffectFollowType(effect, time);

        if (effect == EffectNum.DEBUFF) audioP.JiangshiHitSound();
    }
}
