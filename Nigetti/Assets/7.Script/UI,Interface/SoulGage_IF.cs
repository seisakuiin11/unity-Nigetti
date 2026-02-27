using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SoulGage_IF : MonoBehaviour
{
    [Header("テキスト")]
    [SerializeField] RectTransform soul;
    [SerializeField] TextMeshProUGUI numText;
    [Header("対象先")]
    [SerializeField] RectTransform soul_pos;
    [SerializeField] TextMeshProUGUI textData;

    // Update is called once per frame
    void Update()
    {
        soul.position = soul_pos.position;
        numText.text = textData.text;
    }
}
