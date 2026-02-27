using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTimer : MonoBehaviour
{
    const sbyte ZITTAI = 1, RETAI = -1;

    [Header("参照対象")]
    [SerializeField] Image timerBack;
    [SerializeField] Image timerGage;
    [SerializeField] Image timerIcon;
    [SerializeField] Animator changeCount;
    [Header("素材")]
    [SerializeField] Sprite backOni;
    [SerializeField] Sprite backNige;
    [SerializeField] Sprite iconOni;
    [SerializeField] Sprite iconNige;
    float maxTime;
    float nowTime;
    bool triger = false;
    bool startGame = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        startGame = false;
        changeCount.GetComponent<RectTransform>().localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (!startGame) return;
        nowTime -= Time.deltaTime;
        float fill = nowTime / maxTime;
        timerGage.fillAmount = fill;
        // 切り替わり3秒前 コール
        if (nowTime <= 3 && !triger)
        {
            triger = true;
            changeCount.SetTrigger("Count");
        }
    }

    /// <param name="mode">ジッタイorレータイ</param>
    /// <param name="time">開始時の時間</param>
    public void ChangeUI(int mode, float time)
    {
        if(mode == RETAI)
        {
            timerBack.sprite = backNige;
            timerIcon.sprite = iconNige;
        }
        else if(mode == ZITTAI)
        {
            timerBack.sprite = backOni;
            timerIcon.sprite = iconOni;
        }
        maxTime = time;
        nowTime = time;
        triger = false;
        changeCount.Play("None");
    }

    public void SetFlag()
    {
        startGame = true;
    }
}
