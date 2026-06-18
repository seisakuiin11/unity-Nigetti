using UnityEngine;
using UnityEngine.UI;

public class UISkillDirecter : MonoBehaviour
{
    const int ONI = 1, NIGE = -1;
    [Header("おにUI")]
    [SerializeField] GameObject oniUI;
    [SerializeField] Image o_attackIcon;
    [SerializeField] Image o_skillIcon;
    [SerializeField] Sprite[] skillIcons;
    [SerializeField] Animator countTime_an;
    [Header("にげUI")]
    [SerializeField] GameObject nigeUI;
    [SerializeField] Image n_skillIcon1;
    [SerializeField] Image n_skillIcon2;
    [Header("UI素材")]
    [SerializeField] Sprite oni_s;
    [SerializeField] Sprite nige_s;

    float skill1Time = 0;
    float skill1Time_Const;
    float skill2Time = 0;
    float skill2Time_Const;
    Image skill1Icon;
    Image skill2Icon;

    void Awake()
    {
        UIChange(ONI);
    }
    void Update()
    {
        if(skill1Time_Const != 0)
        {
            skill1Time += Time.deltaTime;
            if(skill1Time >= skill1Time_Const)
            {
                skill1Time_Const = 0;
                skill1Time = 0;
                skill1Icon.fillAmount = 1f;
                skill1Icon.color = Color.white;
            }
            else
            {
                skill1Icon.fillAmount = skill1Time / skill1Time_Const;
                skill1Icon.color = new Color32(140,140,140,255);
            }
        }
        if (skill2Time_Const != 0)
        {
            skill2Time += Time.deltaTime;
            if (skill2Time >= skill2Time_Const)
            {
                skill2Time_Const = 0;
                skill2Time = 0;
                skill2Icon.fillAmount = 1f;
                skill2Icon.color = Color.white;
            }
            else
            {
                skill2Icon.fillAmount = skill2Time / skill2Time_Const;
                skill2Icon.color = new Color32(140, 140, 140, 255);
            }
        }
        // 切り替わりゲージ
        
    }

    public void Init()
    {
        countTime_an.Play("None");
        countTime_an.transform.localScale = Vector3.zero;
    }

    public void UIChange(int mode)
    {
        if(mode == ONI)
        {
            oniUI.SetActive(true);
            nigeUI.SetActive(false);
            skill1Icon = o_attackIcon;
            skill2Icon = o_skillIcon;

            ResetUI();
        }
        else if(mode == NIGE)
        {
            oniUI.SetActive(false);
            nigeUI.SetActive(true);
            skill1Icon = n_skillIcon1;
            skill2Icon = n_skillIcon2;

            ResetUI();
        }
    }

    public void UICharChange(int charNum)
    {
        o_skillIcon.sprite = skillIcons[charNum];
    }

    public void UISkill1(float amount, float time, bool Flag)
    {
        skill1Time_Const = time;
        skill1Time = 0;
        skill1Icon.fillAmount = amount;
        skill1Icon.color = new Color32(140, 140, 140, 255);
        if (Flag) skill1Icon.color = Color.white;
    }
    public void UISkill2(float amount, float time, bool Flag)
    {
        skill2Time_Const = time;
        skill2Time = 0;
        skill2Icon.fillAmount = amount;
        skill2Icon.color = new Color32(140, 140, 140, 255);
        if (Flag) skill2Icon.color = Color.white;
    }

    /// <summary>
    /// カウントダウン 動けない時間
    /// </summary>
    public void CountDown()
    {
        countTime_an.SetTrigger("Play");
    }

    /// <summary>
    /// 元の状態に戻す
    /// </summary>
    public void ResetUI()
    {
        skill1Icon.fillAmount = 1f;
        skill1Icon.color = Color.white;
        skill2Icon.fillAmount = 1f;
        skill2Icon.color = Color.white;
    }
}
