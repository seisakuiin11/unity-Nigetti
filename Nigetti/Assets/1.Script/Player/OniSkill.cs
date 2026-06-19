using ComonData;
using Fusion;
using UnityEngine;

public class OniSkill : ZittaiSkillBase
{
    [SerializeField] GameObject attackCollider;
    [SerializeField] Vector3 upScaleAttackCol = new Vector3(1.35f, 1.35f, 1.35f);
    [SerializeField] float speed = 8f;
    [SerializeField] float buffTime = 8f;
    [SerializeField] float debuffTime = 2f;

    Vector3 defultScale;
    TickTimer BuffTimer;
    TickTimer DebuffTimer;

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
        defultScale = attackCollider.transform.localScale;
    }

    /// <summary>
    /// アップデート処理 (Host)
    /// </summary>
    public override void UpdateMethod()
    {
        if (!Object.HasStateAuthority) return;

        if (BuffTimer.Expired(Runner)) { Debuff(); BuffTimer = TickTimer.None; }

        if (DebuffTimer.Expired(Runner)) { End(); DebuffTimer = TickTimer.None; }

        if(SkillCoolTimer.Expired(Runner)) SkillCoolTimer = TickTimer.None;
    }

    /// <summary>
    /// 描画系
    /// </summary>
    public override void RenderMethod()
    {
        if(skillState != skillStateOld && skillStateOld == ActionState.NONE)
        {
            skillStateOld = ActionState.ACTION;

            anim.Play("SkillNone");
            anim.SetInteger("Skill", 1);
            effects.EffectFollowType(EffectNum.BUFF, 8);
            audioP.SkillSound(charaNum);
        }

        if(skillState != skillStateOld && skillStateOld == ActionState.ACTION)
        {
            skillStateOld = ActionState.END;

            anim.SetInteger("Skill", 2);
            effects.EffectFollowType(EffectNum.DEBUFF, 2);
            audioP.JiangshiHitSound();
        }

        if(skillState != skillStateOld && skillStateOld == ActionState.END)
        {
            skillStateOld = ActionState.NONE;

            anim.SetInteger("Skill", 0);

            // フリーモード時のUI
            if (freeMode && skillUI_d != null) skillUI_d.UISkill2(0f, SkillCoolTime, false);
        }

        // リセット
        if (reset && skillStateOld != ActionState.NONE)
        {
            anim.SetInteger("Skill", 0);
            anim.Play("SkillNone");

            skillStateOld = ActionState.NONE;
            reset = false;
        }
    }

    /// <summary>
    /// ボタンを押したとき
    /// </summary>
    public override void PressButton()
    {
        if (skillCount <= 0 || SkillCoolTimer.IsRunning) return;
        if (BuffTimer.IsRunning || DebuffTimer.IsRunning) return;

        skillState = ActionState.ACTION;

        if (!Object.HasStateAuthority) return;

        skillCount--;

        Buff();

        if (freeMode) skillCount = MaxSkillCount;

        if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false); // アイコングレー化
    }

    /// <summary>
    /// ボタンを放したとき
    /// </summary>
    public override void ReleaseButton()
    {
        if (skillState != ActionState.STANBY) return;
    }

    /// <summary>
    /// スキルのリセット
    /// </summary>
    public override void ResetSkill()
    {
        base.ResetSkill();

        BuffTimer = TickTimer.None;
        DebuffTimer = TickTimer.None;

        attackCollider.transform.localScale = defultScale;
    }

    // ======================================================================================

    void Buff()
    {
        player.ChangeSpeed(speed, buffTime);
        attackCollider.transform.localScale = upScaleAttackCol;
        BuffTimer = TickTimer.CreateFromSeconds(Runner, buffTime);
    }

    void Debuff()
    {
        skillState = ActionState.END;

        player.DisableRunning(debuffTime);
        attackCollider.transform.localScale = defultScale;
        DebuffTimer = TickTimer.CreateFromSeconds(Runner, debuffTime);
    }

    void End()
    {
        skillState = ActionState.NONE;

        SkillCoolTimer = TickTimer.CreateFromSeconds(Runner, SkillCoolTime);
    }
}
