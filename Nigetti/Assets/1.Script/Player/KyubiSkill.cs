using ComonData;
using Fusion;
using UnityEngine;

public class KyubiSkill : ZittaiSkillBase
{
    [SerializeField,Header("魂の削除継続時間")] float deleteTime = 5f;
    [SerializeField,Header("継続時間エフェクト")] KyubiEffect kyubiEffect;

    TickTimer SoulDeleteTimer;
    GameObject[] souls;

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
        if (!Object.HasStateAuthority) return;

        if (SoulDeleteTimer.Expired(Runner))
        {
            SoulDeleteTimer = TickTimer.None;
            SkillCoolTimer = TickTimer.CreateFromSeconds(Runner, SkillCoolTime);
            skillState = ActionState.END;
        }

        if (SkillCoolTimer.Expired(Runner)) SkillCoolTimer = TickTimer.None;
    }

    /// <summary>
    /// 描画系
    /// </summary>
    public override void RenderMethod()
    {
        if(skillState != skillStateOld && skillStateOld == ActionState.NONE)
        {
            skillStateOld = ActionState.STANBY;
        }

        if(skillState != skillStateOld && skillStateOld == ActionState.STANBY)
        {
            skillStateOld = ActionState.ACTION;

            // 削除 (非表示)
            SoulDelete();
        }

        if(skillState != skillStateOld && skillStateOld == ActionState.ACTION)
        {
            skillStateOld = ActionState.END;

            // 再表示
            SoulReactive();

            if (Object.HasStateAuthority) skillState = ActionState.NONE;
        }
        else if (skillState != skillStateOld && skillStateOld == ActionState.END)
        {
            skillStateOld = ActionState.NONE;

            // フリーモード時のUI
            if (freeMode && skillUI_d != null) skillUI_d.UISkill2(0f, SkillCoolTime, false);
        }

        // リセット
        if (reset && skillStateOld != ActionState.NONE)
        {
            // 魂を削除していたら
            if (skillStateOld == ActionState.ACTION) SoulReactive();

            skillStateOld = ActionState.NONE;
            reset = false;
        }
    }

    /// <summary>
    /// ボタンを押したとき
    /// </summary>
    public override void PressButton()
    {
        if (skillCount <= 0 || SoulDeleteTimer.IsRunning || SkillCoolTimer.IsRunning) return;

        skillState = ActionState.STANBY;
    }

    /// <summary>
    /// ボタンを放したとき
    /// </summary>
    public override void ReleaseButton()
    {
        if (skillState != ActionState.STANBY) return;

        skillState = ActionState.ACTION;

        if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false); // アイコングレー化

        if (!Object.HasStateAuthority) return;

        skillCount--;

        AllPlayerDisableSkill();
        // 魂の削除 (見えなくする)
        SoulDeleteTimer = TickTimer.CreateFromSeconds(Runner, deleteTime);

        if (freeMode) skillCount = MaxSkillCount;
    }

    /// <summary>
    /// スキルのリセット
    /// </summary>
    public override void ResetSkill()
    {
        base.ResetSkill();
    }

    // ===========================================================================

    void AllPlayerDisableSkill()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var _player in players)
            if(_player != player) _player.DisableSkill(deleteTime);
    }

    void SoulDelete()
    {
        souls = GameObject.FindGameObjectsWithTag("Soul");
        if (souls.Length <= 0) return;

        foreach (GameObject soul in souls)
        {
            soul.SetActive(false);
            effects.EffectFixedType(EffectNum.SOUL, 0, soul.transform.position);
        }
        kyubiEffect.PlayAnim(deleteTime); // 九尾の後ろに魂を表示 (残り時間を表現)
        audioP.SkillSound(charaNum);
    }

    void SoulReactive()
    {
        if (souls == null) return;

        foreach (GameObject soul in souls)
        {
            soul.SetActive(true);
            effects.EffectFixedType(EffectNum.SOUL, 0, soul.transform.position);
        }

        souls = null;
        audioP.SkillSound(charaNum);
    }
}
