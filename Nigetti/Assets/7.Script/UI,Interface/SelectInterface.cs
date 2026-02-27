using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectInterface : MonoBehaviour, ISelectHandler
{
    SelectDirecter select_d;
    UISoundScript AudioPlayer;
    int num;

    void Awake()
    {
        select_d = FindAnyObjectByType<SelectDirecter>();
        AudioPlayer = FindAnyObjectByType<UISoundScript>();
        num = this.gameObject.transform.GetSiblingIndex();
    }

    public void OnSelect(BaseEventData eventData)
    {
        select_d.SetStageNum(num);
        AudioPlayer.SelectSEPlay();
    }
}
