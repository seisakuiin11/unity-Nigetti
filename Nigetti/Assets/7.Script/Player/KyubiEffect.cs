using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KyubiEffect : MonoBehaviour
{
    [SerializeField, Header("‰ñ“]‘¬“x")]
    float speed;
    float animTime = 0f;
    int soulNum;
    int soulNum_old = 3;
    GameObject[] soulList;

    void Awake()
    {
        soulList = new GameObject[3];
        for(int i = 0; i < this.transform.childCount; i++)
        {
            soulList[i] = this.transform.GetChild(i).gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        animTime -= Time.deltaTime;
        soulNum = 1 + (int)animTime;
        if( soulNum < soulList.Length && soulNum != soulNum_old)
        {
            soulList[soulNum].SetActive(false);
            soulNum_old = soulNum;
        }
        if (animTime < 0f)
        {
            animTime = 0f;
            soulList[soulNum].SetActive(false);
            this.gameObject.SetActive(false);
        }
        else
        {
            this.transform.Rotate(0f, speed * Time.deltaTime, 0f);
        }
    }

    public void PlayAnim(float time)
    {
        this.gameObject.SetActive(true);
        animTime = time;
        foreach (var soul in soulList)
        {
            soul.SetActive(true);
        }
    }
}
