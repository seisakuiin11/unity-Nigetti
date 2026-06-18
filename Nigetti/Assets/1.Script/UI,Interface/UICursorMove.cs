using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICursorMove : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    enum BtnType
    {
        NONE,
        ADIO,
        DISP
    }
    [SerializeField] BtnType type;
    [SerializeField] GameObject adioMenu;
    [SerializeField] GameObject dispMenu;
    UISoundScript AudioPlayer;
    EventSystem uiSystem;

    void Awake()
    {
        uiSystem = EventSystem.current;
        AudioPlayer = FindAnyObjectByType<UISoundScript>();
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(EnterSE);
    }

    public void OnSelect(BaseEventData eventData)
    {
        AudioPlayer.SelectSEPlay();
        if (type == BtnType.NONE) return;
        else if (type == BtnType.ADIO)
        {
            adioMenu.SetActive(true);
            dispMenu.SetActive(false);
        }
        else if (type == BtnType.DISP) {
            adioMenu.SetActive(false);
            dispMenu.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiSystem.SetSelectedGameObject(this.gameObject);
    }

    public void EnterSE()
    {
        AudioPlayer.EnterSEPlay();
    }
}
