using UnityEngine;
using Fusion;

public class ZittaiSkillBase : NetworkBehaviour
{
    protected enum ActionState : sbyte
    {
        NONE,
        STANBY,
        ACTION,
        END,
    }

    [SerializeField, Tooltip("キャラID")]
    protected int charaNum;
    [SerializeField, Tooltip("鬼になったときに使える最大回数")]
    protected int MaxSkillCount = 1;
    [SerializeField, Tooltip("フリーモード時のスキルクールタイム")]
    protected float SkillCoolTime = 0.2f;
    [SerializeField] protected Animator anim;
    [SerializeField] protected EffectScript effects;

    [Networked] protected int skillCount { get; set; }
    [Networked] protected ActionState skillState { get; set; }
    [Networked] protected TickTimer SkillCoolTimer { get; set; }
    [Networked] protected bool reset {  get; set; }
    [Networked] protected bool freeMode { get; set; }

    protected ActionState skillStateOld;
    protected PlayerController player;
    protected PlayerAudioScript audioP;
    protected UISkillDirecter skillUI_d;


    public override void Spawned()
    {
        audioP = GetComponent<PlayerAudioScript>();

        if (!Object.HasInputAuthority) return;

        skillUI_d = FindAnyObjectByType<UISkillDirecter>();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public virtual void Init(PlayerController _player, bool _freeMode)
    {
        player = _player;
        freeMode = _freeMode;
        skillCount = MaxSkillCount;
    }

    /// <summary>
    /// アップデート処理
    /// </summary>
    public virtual void UpdateMethod() { }

    /// <summary>
    /// 描画系
    /// </summary>
    public virtual void RenderMethod() { }

    /// <summary>
    /// ボタンを押したとき
    /// </summary>
    public virtual void PressButton() { }

    /// <summary>
    /// ボタンを放したとき
    /// </summary>
    public virtual void ReleaseButton() { }

    /// <summary>
    /// スキルのリセット
    /// </summary>
    public virtual void ResetSkill()
    {
        if (!Object.HasStateAuthority) return;

        reset = true;
        skillCount = MaxSkillCount;
        SkillCoolTimer = TickTimer.None;
        skillState = ActionState.NONE;
    }
}
