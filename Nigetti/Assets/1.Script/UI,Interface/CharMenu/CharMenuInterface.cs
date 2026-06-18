using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharMenuInterface : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    CharMenuManager charMenu;
    UISoundScript AudioPlayer;
    EventSystem uiSystem;
    int num;

    void Awake()
    {
        charMenu = FindAnyObjectByType<CharMenuManager>();
        AudioPlayer = FindAnyObjectByType<UISoundScript>();
        uiSystem = FindAnyObjectByType<EventSystem>();
        num = this.gameObject.transform.GetSiblingIndex();
    }

    public void OnSelect(BaseEventData eventData)
    {
        charMenu.SetCharNum(num);
        AudioPlayer.SelectSEPlay();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiSystem.SetSelectedGameObject(this.gameObject);
    }
}
