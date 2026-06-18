using Fusion;
using UnityEngine;

public class KyonsiSkill : ZittaiSkillBase
{
    [SerializeField] NetworkPrefabRef billPrefab;

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

            anim.Play("SkillNone");
            anim.SetTrigger("Skill");
            audioP.SkillSound(charaNum);

            if (Object.HasStateAuthority) skillState = ActionState.NONE;
        }
        else if (skillState != skillStateOld && skillStateOld == ActionState.ACTION)
        {
            skillStateOld = ActionState.NONE;
        }

        // リセット
        if (reset && skillStateOld != ActionState.NONE)
        {
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

        skillState = ActionState.STANBY;
    }

    /// <summary>
    /// ボタンを放したとき
    /// </summary>
    public override void ReleaseButton()
    {
        if (skillState != ActionState.STANBY) return;

        skillState = ActionState.ACTION;

        if (skillUI_d != null) skillUI_d.UISkill2((skillCount-1) / (float)MaxSkillCount, 0f, true); // アイコン変化 グレー化はしない

        if (!Object.HasStateAuthority) return;
        // ホストのみ

        skillCount--;

        // お札の生成
        NetworkObject networkBillObject;
        Vector3 pos = new Vector3(this.gameObject.transform.position.x, -0.5f, this.gameObject.transform.position.z) + transform.forward;
        networkBillObject = Runner.Spawn(billPrefab, pos, Quaternion.identity, PlayerRef.None);
        networkBillObject.GetComponent<Rigidbody>().AddForce(transform.forward * 700);
        networkBillObject.transform.rotation = transform.rotation;

        // フリーモードなら
        if (freeMode && skillCount <= 0) skillCount = MaxSkillCount;

        // スキルのクールタイム設定
        if (skillCount > 0)
            SkillCoolTimer = TickTimer.CreateFromSeconds(Runner, SkillCoolTime);

    }

    /// <summary>
    /// スキルのリセット
    /// </summary>
    public override void ResetSkill()
    {
        base.ResetSkill();
    }
}
