using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountSoundScript : MonoBehaviour
{
    public int count = 0;
    int count_old = 0;
    UISoundScript sound;

    private void Awake()
    {
        sound = FindAnyObjectByType<UISoundScript>();
    }

    private void Update()
    {
        if(count != 0 && count != count_old)
        {
            sound.CountSEPlay();
            count_old = count;
        }
        else if(count != count_old) count_old = count;
    }

    public void CountSEPlay()
    {
        sound.CountSEPlay();
    }
}
