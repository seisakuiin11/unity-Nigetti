using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharIcon : MonoBehaviour
{
    [SerializeField] Image[] charImageP1;
    [SerializeField] Image[] charImageP2;
    [SerializeField] Sprite[] charSprite_r;
    [SerializeField] Sprite[] charSprite_z;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCharIcon(int charNum_p1, int charNum_p2)
    {
        charImageP1[0].sprite = charSprite_r[charNum_p1];
        charImageP1[1].sprite = charSprite_z[charNum_p1];
        charImageP2[0].sprite = charSprite_r[charNum_p2];
        charImageP2[1].sprite = charSprite_z[charNum_p2];
    }
}
