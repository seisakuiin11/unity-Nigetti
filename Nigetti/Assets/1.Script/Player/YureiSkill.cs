using ComonData;
using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;

public class YureiSkill : ZittaiSkillBase
{
    [SerializeField, Header("突進スピード")] float rushSpeed = 300f;
    [SerializeField, Header("アニメーション 回転")] float animTime_rotate = 0.8f;
    [SerializeField, Header("アニメーション 突進")] float animTime_rush = 0.5f;
    [SerializeField, Header("アニメーション 着地")] float animTime_land = 0.4f;
    [SerializeField] GameObject skillObj;
    [SerializeField] GameObject[] skillObj_visual;
    [SerializeField] GameObject skillCollider;
    [SerializeField] KCC _kcc;

    [Networked] TickTimer RushAction { get; set; }
    [Networked] ActionState animState { get; set; }
    [Networked] Vector3 direction { get; set; }


    ActionState animStateOld;

    // int charaNum;
    // int MaxSkillCount;
    // bool freeMode;
    // float SkillCoolTime;
    // TickTimer SkillCoolTimer;
    // int skillCount;
    // ActionState skillState;
    // ActionState skillStateOld;
    // PlayerController player;
    // PlayerAudioScript audioP;
    // UISkillDirecter skillUI_d;
    // bool reset;


    /// <summary>
    /// 初期化
    /// </summary>
    public override void Init(PlayerController _player, bool _freeMode)
    {
        base.Init(_player, _freeMode);
    }

    /// <summary>
    /// アップデート処理 (Host)
    /// </summary>
    public override void UpdateMethod()
    {
        // 突進
        if (RushAction.IsRunning) Rush();

        if (!Object.HasStateAuthority) return;

        // 突進 各セクション処理
        if (RushAction.Expired(Runner)) SkillAction();

        // スキルクールタイム
        if (SkillCoolTimer.Expired(Runner))
        {
            SkillCoolTimer = TickTimer.None;
        }
    }

    /// <summary>
    /// 描画系
    /// </summary>
    public override void RenderMethod()
    {
        // 準備
        if(skillState != skillStateOld && skillStateOld == ActionState.NONE)
        {
            skillStateOld = ActionState.STANBY;

            // 予測オブジェ
            skillObj.SetActive(true);

            // 敵に予測が見えないようにする
            if (!Object.HasInputAuthority)
                foreach (var obj in skillObj_visual) obj.SetActive(false);
        }

        // 開始
        else if(skillState != skillStateOld && skillStateOld == ActionState.STANBY)
        {
            skillStateOld = ActionState.ACTION;

            skillObj.SetActive(false);

            // ホストが終了処理
            if (Object.HasStateAuthority) skillState = ActionState.NONE;
        }
        else if (skillState != skillStateOld && skillStateOld == ActionState.ACTION)
        {
            skillStateOld = ActionState.NONE;
        }

        // 回転
        if (animState != animStateOld && animStateOld == ActionState.NONE)
        {
            animStateOld = ActionState.STANBY;

            anim.Play("SkillNone");
            anim.SetBool("Skill", true);
            audioP.SkillSound(charaNum);
        }

        // 突進
        else if (animState != animStateOld && animStateOld == ActionState.STANBY)
        {
            animStateOld = ActionState.ACTION;

            effects.EffectFollowType(EffectNum.YUREI, 0);
        }

        // 着地
        else if (animState != animStateOld && animStateOld == ActionState.ACTION)
        {
            animStateOld = ActionState.END;

            anim.SetBool("Skill", false);
        }
        else if(animState != animStateOld && animStateOld == ActionState.END)
        {
            animStateOld = ActionState.NONE;

            // フリーモード時のUI
            if (freeMode && skillUI_d != null) skillUI_d.UISkill2(0f, SkillCoolTime, false);
        }

        // リセット
        if (reset && skillStateOld != ActionState.NONE)
        {
            anim.SetBool("Skill", false);
            anim.Play("SkillNone");
            skillStateOld = ActionState.NONE;
            animStateOld = ActionState.NONE;
            reset = false;
        }
    }

    /// <summary>
    /// ボタンを押したとき
    /// </summary>
    public override void PressButton()
    {
        if (skillCount <= 0 || SkillCoolTimer.IsRunning) return;

        skillState = ActionState.STANBY;
    }

    /// <summary>
    /// ボタンを放したとき
    /// </summary>
    public override void ReleaseButton()
    {
        if (skillState != ActionState.STANBY) return;

        skillState = ActionState.ACTION;

        // アニメーションに合わせた処理
        SkillAction();

        if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false); // アイコングレー化
    }

    /// <summary>
    /// スキルのリセット
    /// </summary>
    public override void ResetSkill()
    {
        base.ResetSkill();
        animState = ActionState.NONE;
        RushAction = TickTimer.None;
        skillCollider.SetActive(false);
    }

    // ======================================================================================
    // 各セクションごとの処理
    void SkillAction()
    {
        if (!Object.HasStateAuthority) return;

        // 回転
        if (animState == ActionState.NONE)
        {
            animState = ActionState.STANBY;
            skillCount--;

            RushAction = TickTimer.CreateFromSeconds(Runner, animTime_rotate);
        }
        // 突進
        else if (animState == ActionState.STANBY)
        {
            animState = ActionState.ACTION;

            skillCollider.SetActive(true); // 当たり判定
            direction = player.transform.forward; // 前方に突進
            player.SetStunTime(animTime_rush); // PlayerControllerの移動処理を無効化する

            RushAction = TickTimer.CreateFromSeconds(Runner, animTime_rush);
        }
        // 着地
        else if (animState == ActionState.ACTION)
        {
            animState = ActionState.END;

            skillCollider.SetActive(false);

            RushAction = TickTimer.CreateFromSeconds(Runner, animTime_land);
        }
        // モーション終了
        else if(animState == ActionState.END)
        {
            animState = ActionState.NONE;

            // フリーモードなら
            if (freeMode)
            {
                skillCount = MaxSkillCount;
                SkillCoolTimer = TickTimer.CreateFromSeconds(Runner, SkillCoolTime);
            }

            RushAction = TickTimer.None;
        }
    }

    // 突進
    void Rush()
    {
        if (animState != ActionState.ACTION) return;

        Vector3 MoveVelocity = rushSpeed * direction;
        _kcc.AddExternalVelocity(MoveVelocity);
    }
}
