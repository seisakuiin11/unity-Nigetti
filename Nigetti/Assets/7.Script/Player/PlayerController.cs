using UnityEngine;
using Fusion;
using Fusion.Addons.KCC;
using System.Collections;
using ComonData;

public class PlayerController : NetworkBehaviour
{
    const sbyte ZITTAI = 1, RETAI = -1;
    const sbyte NONE = 0, INV = 1;

    [Networked] int sceneType { get; set; } //現在のシーン  ロビー or バトル
    [Networked] int playNum { get; set; } //Host:0 or Client:1
    [Networked] int charNum { get; set; } //0:幽霊 1:キョンシー 2:九尾 3:鬼
    [Networked] int turn { get; set; } //実体 or 零体
    [Networked] int life { get; set; } //ライフ
    [Networked] int speed { get; set; } //走る時の倍率　100%で考える
    [Networked] int soulCount { get; set; } //魂の所持数
    [Networked] NetworkBool ChangeTurnFlag { get; set; }
    [Networked] TickTimer ChangeTurn_time { get; set; }
    [Networked] float oniGage { get; set; } // 鬼の切り替わり
    [Networked] NetworkBool oniGageFlag { get; set; }
    [Networked] Vector3 direction { get; set; } //現在の移動ベクトル
    [Networked] NetworkBool moveFlag { get; set; } //操作できるか
    [Networked] TickTimer moveFlag_time { get; set; }
    [Networked] NetworkBool runFlag { get; set; } //走れるか
    [Networked] TickTimer runFlag_time { get; set; }
    [Networked] NetworkBool attackFlag { get; set; } //攻撃できるか
    [Networked] NetworkBool attackStanby { get; set; }
    [Networked] NetworkBool attack { get; set; }
    [Networked] TickTimer attackFlag_time { get; set; }
    [Networked] NetworkBool tpFlag { get; set; } //テレポートが使えるか
    [Networked] NetworkBool tpStanby { get; set; }
    [Networked] TickTimer tpFlag_time { get; set; }
    [Networked] NetworkBool invFlag { get; set; } //透明化が使えるか
    [Networked] int invisibleState { get; set; }
    int invisibleState_old;
    [Networked] TickTimer invisible { get; set; }
    [Networked] TickTimer invFlag_time { get; set; }
    [Networked, OnChangedRender(nameof(WaitCount))] NetworkBool waitFlag { get; set; } // 動けない時間表示
    [Networked, OnChangedRender(nameof(Effect))] EffectNum effectNum { get; set; }
    [Networked] Vector3 savePos { get; set; }
    [Networked] Vector3 saveTPPos { get; set; }

    [SerializeField] float RotationSpeed = 20f;
    [SerializeField] int RunSpeed = 5; // 走る時のスピード
    [SerializeField] int WalkSpeed = 2; // 歩く時のスピード
    [SerializeField] float ChangeTime = 20f; // 切り替わり時間
    [SerializeField] GameObject skin;
    [SerializeField] Animator anim;
    [SerializeField] GameObject attack_c;
    [SerializeField] GameObject tpObj_c;
    [SerializeField] GameObject[] tpObj_obj;
    [SerializeField] EffectScript effects;
    Vector3 _moveVelocity;
    Vector3 savePos_old;
    Vector3 saveTPPos_old;
    bool onlyAttack;
    bool onlyAnim;
    int charNum_old;
    int turn_old;
    PlayerController enemy;
    
    KCC _kcc;
    SkillController skills;
    CharcterDirecter charcters;
    PlayerAudioScript audioP;
    UISkillDirecter skillUI_d;
    SkinnedMeshRenderer zittai;
    SkinnedMeshRenderer retai;

    /* ----------------------------スタート処理---------------------------------- */
    public override void Spawned()
    {
        //_cc = GetComponent<NetworkCharacterController>();
        _kcc = GetComponent<KCC>();
        skills = GetComponent<SkillController>();
        charcters = GetComponent<CharcterDirecter>();
        audioP = GetComponent<PlayerAudioScript>();
        if (Object.HasInputAuthority)
        {
            skillUI_d = FindAnyObjectByType<UISkillDirecter>();
            skillUI_d.SetAnimFlag(false);
            skillUI_d.Init();
        }
        effectNum = EffectNum.NONE;
    }
    void Awake()
    {
        charNum_old = -1;
        turn_old = 0;
    }
    void Start()
    {
        
    }
    /* ----------------------- アップデート処理 ----------------------------------- */
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData data)) //入力反映
        {
            if (moveFlag) //操作可能なら
            {
                //進行方向 取得
                data.direction.Normalize();
                direction = data.direction;

                if(turn == ZITTAI) //実体の処理
                {
                    //攻撃
                    if(data.attackStanby && attackFlag)
                    {
                        attackStanby = true;
                        //runFlag = false;
                    }
                    if(data.attackFlag && attackStanby)
                    {
                        attackStanby = false;
                        attackFlag = false;
                        Attack();
                    }
                    //スキル
                    if(data.skillStanby)
                    {
                        skills.StanbySkill(charNum, sceneType);
                    }
                    if(data.skillFlag)
                    {
                        skills.UseSkill(charNum, sceneType);
                    }
                }
                else if(turn == RETAI) //零体の処理
                {
                    //テレポート
                    if(data.attackStanby && tpFlag)
                    {
                        tpStanby = true;
                        tpObj_c.SetActive(true);
                        if (!Object.HasInputAuthority) { //相手に見えないように
                            //if (tpObj_c.TryGetComponent<Renderer>(out var skin)) skin.enabled = false;
                            foreach(var obj  in tpObj_obj) obj.SetActive(false);
                        }
                    }
                    if(data.attackFlag && tpStanby)
                    {
                        tpStanby = false;
                        tpFlag = false;
                        tpObj_c.SetActive(false);
                        Teleport();
                    }
                    //透明化
                    if(data.skillStanby && invFlag)
                    {
                        invFlag = false;
                        Invisible();
                    }
                }

                //ロビー内限定処理
                if(sceneType == (int)Scene.LOBBY)
                {// 実体零体切り替え
                    if (data._change != 0 && !ChangeTurnFlag) {
                        ChangeTurnFlag = true;
                        ChangeTurn_time = TickTimer.CreateFromSeconds(Runner, 0.5f);
                        SetTurn();
                    }
                }

            }
            else
            {// 移動できない
                direction = Vector3.zero;
            }
            //移動
            MoveAction(direction);
        }

        // 切り替わり 時間管理 通常の切り替わり
        if(turn == ZITTAI && oniGageFlag)
        {
            oniGage += Runner.DeltaTime;
            if (sceneType == (int)Scene.LOBBY && oniGage >= ChangeTime - 5.2f)
            {
                oniGage = ChangeTime - 5f;
            }
            else if (oniGage >= ChangeTime)
            {
                SetTurn();
                if (skillUI_d != null && sceneType == (int)Scene.BATTLE) skillUI_d.SetOniGage(turn, ChangeTime);
            }
        }
        else if (turn == RETAI && oniGageFlag)
        {
            oniGage -= Runner.DeltaTime;
            if (sceneType == (int)Scene.LOBBY && oniGage <= 5.2f)
            {
                oniGage = 5f;
            }
            else if (oniGage <= 0)
            {
                SetTurn();
                if (skillUI_d != null && sceneType == (int)Scene.BATTLE) skillUI_d.SetOniGage(turn, ChangeTime);
            }
        }

        //TickTimer 時間経過処理
        if (ChangeTurn_time.Expired(Runner))
        {
            ChangeTurnFlag = false;
            ChangeTurn_time = TickTimer.None;
        }
        //行動
        if (moveFlag_time.Expired(Runner))
        {
            waitFlag = false;
            moveFlag = true;
            moveFlag_time = TickTimer.None;
        }
        //走法
        if(runFlag_time.Expired(Runner))
        {
            runFlag = true;
            runFlag_time = TickTimer.None;
        }
        else if(runFlag_time.IsRunning) runFlag = false;
        //攻撃
        if (attackFlag_time.Expired(Runner))
        {
            attackFlag = true;
            attackFlag_time = TickTimer.None;
        }
        //テレポート
        if(tpFlag_time.Expired(Runner))
        {
            tpFlag = true;
            tpFlag_time = TickTimer.None;
        }
        //透明化
        if (invisible.Expired(Runner))
        {
            invisibleState = NONE;
            invisible = TickTimer.None;
            if (Object.HasStateAuthority)
            {
                if(sceneType == (int)Scene.LOBBY)
                {
                    invFlag_time = TickTimer.CreateFromSeconds(Runner, 0.2f);
                }
                else if(sceneType == (int)Scene.BATTLE)
                {
                    invFlag_time = TickTimer.CreateFromSeconds(Runner, 5.0f);
                }
            }
            if (sceneType == (int)Scene.LOBBY) // UI処理
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 0.2f, false);
            }
            else if (sceneType == (int)Scene.BATTLE)
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 5f, false);
            }
        }
        if(invFlag_time.Expired(Runner))
        {
            invFlag = true;
            invFlag_time = TickTimer.None;
        }
    }
    void Update()
    {
        
    }
    /* -----------------------------描画関係処理--------------------------------------- */
    public override void Render()
    {
        //移動アニメーション
        if(direction != Vector3.zero)
        {
            anim.SetBool("move", true);
        }
        else {
            anim.SetBool("move", false);
        }
        //走る歩くアニメーション　スピード切り替え
        if (runFlag && moveFlag)
        {
            anim.SetBool("Run", true);
        }
        else if(moveFlag) {
            anim.SetBool("Run", false);
        }
        //攻撃アニメーション
        if (attackStanby)
        {
            anim.SetBool("Armup", true);
        }
        if (attack && !onlyAttack)
        {
            onlyAttack = true;
            audioP.AttackSound();
            StartCoroutine(AttackAction());
        }
        //透明化アニメーション
        if (invisibleState == INV && invisibleState != invisibleState_old)
        {
            invisibleState_old = invisibleState;
            audioP.NigeSkillSound(1);
            if (Object.HasInputAuthority) charcters.InvisivleSkin(RETAI);
            else skin.SetActive(false);
        }
        if (invisibleState == NONE && invisibleState != invisibleState_old)
        {
            invisibleState_old = invisibleState;
            if (Object.HasInputAuthority) charcters.InvisivleSkin(ZITTAI);
            else skin.SetActive(true);
        }

        //キャラクターの見た目切り替え
        if(charNum != charNum_old)
        {
            charNum_old = charNum;
            ResetAction();
            CharChange();
        }
        //実体零体切り替え
        if(turn != turn_old)
        {
            //moveFlag = false;
            turn_old = turn;
            //ResetAction();
            TurnChange();
            //moveFlag = true;
        }
    }
    /* ========================================================================== */
    //初期化
    public void Init(Scene Scene, int PlayNum, int CharNum, int Turn, int Speed, int Soul, bool MoveFlag)
    {
        sceneType = (int)Scene;
        playNum = PlayNum;
        charNum = CharNum;
        turn = Turn;
        life = 3;
        speed = Speed;
        soulCount = Soul;
        moveFlag = MoveFlag;
        runFlag = true;
        attackFlag = true;
        tpFlag = true;
        invFlag = true;

        Rpc_SetData(playNum, this);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void Rpc_SetData(int PlayNum, PlayerController mine)
    {
        // 鬼ゲージの設定
        if (turn == RETAI) oniGage = ChangeTime; // 零体なら
        else if (turn == ZITTAI) oniGage = 0; // 実体なら
        if (skillUI_d != null && sceneType == (int)Scene.BATTLE) {
            skillUI_d.SetOniGage(turn, ChangeTime);
        }

        if (sceneType == (int)Scene.LOBBY) // ロビーに限り、両者(p1,p2)にデータを格納する
        {
            LobbyDirecter directer_l = FindAnyObjectByType<LobbyDirecter>();
            directer_l.GetPlayer(PlayNum, mine);
            oniGageFlag = true;
        }
    }

    // 相手のデータを格納
    public void SetEnemy(PlayerController Enemy)
    {
        enemy = Enemy;
        skills.SetEnemy(enemy, sceneType);
    }

    public void SetSpeed(int Speed)
    {
        speed = Speed;
    }
    public int GetSpeed()
    {
        return speed;
    }
    public int GetLife()
    {
        return life;
    }
    public void SetAddSoul(int Soul)
    {
        soulCount += Soul;
    }
    public int GetSoul()
    {
        return soulCount;
    }
    public int GetTurn()
    {
        return turn;
    }
    public void SetOniGageFlag(bool Flag)
    {
        oniGageFlag = Flag;
    }

    // 入力からプレイヤーの移動
    void MoveAction(Vector3 Direction)
    {
        var MoveVelocity = Direction; // 進行方向
        float acceleration = 0;
        if (MoveVelocity != Vector3.zero)
        {
            // 滑らかな回転を実現
            var currentRotation = _kcc.Data.TransformRotation;
            var targetRotation = Quaternion.LookRotation(Direction);
            var nextRotation = Quaternion.Lerp(currentRotation, targetRotation, RotationSpeed * Runner.DeltaTime);
            _kcc.SetLookRotation(nextRotation.eulerAngles);

            acceleration = runFlag ? RunSpeed : WalkSpeed;
        }
        else
        {
            acceleration = 100f;
        }
        // 走る:鬼逃げ別 歩く:鬼逃げ共通
        MoveVelocity = (runFlag ? RunSpeed * speed * 0.01f : WalkSpeed) * Direction;
        //_moveVelocity = Vector3.Lerp(_moveVelocity, MoveVelocity, acceleration * Runner.DeltaTime);
        _kcc.SetInputDirection(MoveVelocity * Runner.DeltaTime);
    }

    //行動制限 動けないようにする
    public void SetMoveFlag(bool Flag, float time)
    {
        moveFlag = Flag;
        if(Object.HasStateAuthority && time > 0)
        {
            waitFlag = true;
            moveFlag_time = TickTimer.CreateFromSeconds(Runner, time);
        }
    }
    // 行動制限 走れなくする
    public void SetRunFlag(bool Flag, float time)
    {
        runFlag = Flag;
        if (Object.HasStateAuthority && time > 0)
        {
            runFlag_time = TickTimer.CreateFromSeconds(Runner, time);
        }
    }

    //攻撃 ダメージ
    void Attack()
    {
        attack = true;
        if(Object.HasStateAuthority) attackFlag_time = TickTimer.CreateFromSeconds(Runner, 1.5f);
        effectNum = EffectNum.TOUCH;
        // UI処理
        if (skillUI_d != null) skillUI_d.UISkill1(0f, 1.5f, false);
    }
    IEnumerator AttackAction()
    {
        anim.SetBool("Armup", false);
        attack_c.SetActive(true);
        yield return new WaitForSeconds(0.02f);
        attack = false;
        yield return new WaitForSeconds(0.5f);
        attack_c.SetActive(false);
        onlyAttack = false;
        if(Object.HasStateAuthority) runFlag = true;
    }
    public void Damage()
    {
        if (sceneType != (int)Scene.BATTLE) return;
        if (Object.HasStateAuthority) {
            //life--;
            SetTurn();
            SetAddSoul(-1);
            enemy.SetTurn();
            enemy.SetAddSoul(1);
            SetMoveFlag(false, 5f);
        }
        //effectNum = 2;
    }

    //テレポート
    void Teleport()
    {
        if (Object.HasStateAuthority)
        {
            savePos = this.transform.position;
            saveTPPos = tpObj_c.transform.position;
            Vector3 pos = new Vector3(saveTPPos.x, savePos.y, saveTPPos.z);
            saveTPPos = pos; // Y軸 同一化
            effectNum = EffectNum.TP;
            _kcc.SetPosition(pos);
            if(sceneType == (int)Scene.LOBBY)
            {
                tpFlag_time = TickTimer.CreateFromSeconds(Runner, 0.2f);
            }
            else if(sceneType == (int)Scene.BATTLE)
            {
                soulCount -= 1;
                tpFlag_time = TickTimer.CreateFromSeconds(Runner, 2f);
            }
        }
        // UI処理
        if(sceneType == (int)Scene.LOBBY)
        {
            if (skillUI_d != null) skillUI_d.UISkill1(0f, 0.2f, false);
        }
        else if(sceneType == (int)Scene.BATTLE)
        {
            if (skillUI_d != null) skillUI_d.UISkill1(0f, 2f, false);
        }
    }

    //透明化
    void Invisible()
    {
        effectNum = EffectNum.SMOKE;
        invisibleState = INV;
        if (Object.HasStateAuthority)
        {
            if (sceneType == (int)Scene.BATTLE) soulCount -= 2;
            invisible = TickTimer.CreateFromSeconds(Runner, 2.0f);
        }
        if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false);
    }

    //キャラクターチェンジ
    public void SetCharNum(int CharNum)
    {
        charNum = CharNum;
    }
    public int GetCharNum()
    {
        return charNum;
    }
    void CharChange() // 見た目の変更
    {
        skin = charcters.ChangeSkin(charNum, turn);
        anim = charcters.Anim(charNum);
        skills.SetAnim(anim);
    }

    //実体零体切り替え  捕まり切り替わるとき
    public void SetTurn()
    {
        if (!Object.HasStateAuthority) return;
        if(turn == ZITTAI) // 実体なら
        {
            turn = RETAI;
            oniGage = ChangeTime;
            //speed = 100;
        }
        else if(turn == RETAI) // 零体なら
        {
            turn = ZITTAI;
            oniGage = 0;
            //speed = 130;
        }
    }
    void TurnChange() // 見た目の変更  turnトリガー
    {
        // 各種見た目のリセット
        // テレポート
        tpStanby = false;
        tpObj_c.SetActive(false);
        if (!tpFlag) {
            tpFlag_time = TickTimer.None;
            tpFlag = true;
        }
        // 透明化
        if (invisibleState == INV) { // 透明化中
            invisible = TickTimer.None;
            invisibleState = NONE;
            invisibleState_old = invisibleState;
            if (Object.HasInputAuthority) charcters.InvisivleSkin(ZITTAI);
            else skin.SetActive(true);
        }
        if (!invFlag) {
            invFlag_time = TickTimer.None;
            invFlag = true;
        }
        // 攻撃
        if (attackStanby)
        {
            attackStanby = false;
            runFlag = true;
        }
        bool attackCheck = false;
        float passTime = 0;
        if (onlyAttack)
        {
            attackCheck = true;
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(1);
            passTime = state.normalizedTime;
        }

        // スキルのリセット（幽霊は継続,その他書き換え
        skills.RsetSkill();

        // 見た目の変更
        speed = turn == ZITTAI ? 130 : 100;
        gameObject.tag = turn == ZITTAI ? "Oni" : "Nige";
        audioP.NigeSkillSound(1);
        effects.EffectFollowType(EffectNum.SMOKE, 0);
        effects.EffectFollowType(turn == ZITTAI ? EffectNum.ONICHANGE : EffectNum.NIGECHANGE, 0); // 自分が鬼か逃げかを認知するエフェクト
        skin = charcters.ChangeSkin(charNum, turn);
        anim = charcters.Anim(charNum);
        skills.SetAnim(anim);
        if (skillUI_d != null) skillUI_d.UIChange(turn);
        if (skillUI_d != null && sceneType == (int)Scene.BATTLE) skillUI_d.SetOniGage(turn, ChangeTime);
        if (attackCheck)
        {
            anim.SetBool("Armup", false);
            anim.Play("hurisage", 1, passTime);
        }
        else
        {
            anim.SetBool("Armup", false);
            anim.Play("None");
        }
    }

    //各種アクション,見た目のリセット
    void ResetAction()
    {
        runFlag = true;
        //攻撃リセット
        if (attackStanby)
        {
            attackStanby = false;
            anim.SetBool("Armup", false);
            anim.Play("None");
        }
        if(!attackFlag)
        {
            attackFlag = true;
            attackFlag_time = TickTimer.None;
        }
        //スキルリセット
        skills.RsetSkill();
        //テレポートリセット
        if(tpStanby)
        {
            tpStanby = false;
            tpObj_c.SetActive(false);
        }
        if (!tpFlag)
        {
            tpFlag = true;
            tpFlag_time = TickTimer.None;
        }
        //透明化リセット
        if(invisible.IsRunning)
        {
            if (Object.HasInputAuthority) charcters.InvisivleSkin(ZITTAI);
            else skin.SetActive(true);
            invisible = TickTimer.None;
        }
        if (!invFlag)
        {
            invFlag = true;
            invFlag_time = TickTimer.None;
        }
        //UIリセット
        if(skillUI_d != null)
        {
            skillUI_d.UISkill1(1f, 0f, true);
            skillUI_d.UISkill2(1f, 0f, true);
        }
    }

    //各種エフェクト 値が変わったときに呼ばれる
    void Effect()
    {
        if (effectNum == EffectNum.NONE) return;
        StartCoroutine(ResetEffectNum()); // バッファ

        if (effectNum == EffectNum.TP) { // テレポートのエフェクトは追従しない
            if (savePos == savePos_old && saveTPPos == saveTPPos_old) return;
            savePos_old = savePos;
            saveTPPos_old = saveTPPos;
            audioP.NigeSkillSound(0);
            effects.EffectFixedType(effectNum, 0, savePos); // 現在地
            effects.EffectFixedType(effectNum, 0, saveTPPos); // TP先
        }
        else effects.EffectFollowType(effectNum, 0);
    }
    IEnumerator ResetEffectNum()
    {
        yield return new WaitForSeconds(0.33f);
        effectNum = EffectNum.NONE;
    }

    void WaitCount()
    {
        if (skillUI_d != null) skillUI_d.CountDown(waitFlag);
    }

    // 鬼スキル 再使用可能化
    public void SkillRevival()
    {
        skills.SetSkillFlag(true, 0.1f);
    }

    public void KyubiSkill()
    {
        tpFlag = false;
        tpFlag_time = TickTimer.CreateFromSeconds(Runner, 5f);
        invFlag = false;
        invFlag_time = TickTimer.CreateFromSeconds(Runner, 5f);
    }
}
