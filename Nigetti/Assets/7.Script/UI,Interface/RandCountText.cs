using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RandCountText : MonoBehaviour
{
    const float MAX_TIME = 0.1f;
    [SerializeField] Image name_p1;
    [SerializeField] Image name_p2;
    [SerializeField] Image num_p1;
    [SerializeField] Image num_p2;
    [Header("UI‘fÞ")]
    [SerializeField] Sprite name_me;
    [SerializeField] Sprite name_em;
    [SerializeField] Sprite[] numSprite;
    int count = 0;
    float deltaTime = 0;
    bool startGame = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (startGame) return;
        deltaTime += Time.deltaTime;
        if (deltaTime > MAX_TIME)
        {
            deltaTime = 0;
            
            num_p1.sprite = numSprite[count];
            count = (count + 1) % 2;
            num_p2.sprite = numSprite[count];
        }
    }

    public void SetName(int playNum)
    {
        startGame = false;

        if(playNum == 0)
        {
            name_p1.sprite = name_me;
            name_p2.sprite = name_em;
        }
        else if(playNum == 1)
        {
            name_p1.sprite = name_em;
            name_p2.sprite = name_me;
        }
    }

    public void StartGameText(int numP1, int numP2)
    {
        num_p1.sprite = numSprite[numP1 == 1 ? 0 : 1];
        num_p2.sprite = numSprite[numP2 == 1 ? 0 : 1];
        startGame = true;
    }
}
