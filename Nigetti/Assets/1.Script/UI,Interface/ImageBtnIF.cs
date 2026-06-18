using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageBtnIF : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] Image btn;
    [SerializeField] Sprite notSelect_img;
    [SerializeField] Sprite select_img;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelect(BaseEventData data)
    {
        btn.sprite = select_img;
    }

    public void OnDeselect(BaseEventData data)
    {
        btn.sprite = notSelect_img;
    }
}
