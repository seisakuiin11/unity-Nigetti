using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComonData
{
    public enum Scene
    {
        LOBBY = 0,
        SELECT,
        BATTLE,
        RESULT,
        MAXNUM
    }

    public enum State
    {
        INIT = 0,
        UPDATA,
        END,
        MAXNUM
    }

    public enum EffectNum
    {
        TOUCH = 0,
        TP,
        SMOKE,
        YUREI,
        BUFF,
        DEBUFF,
        SOUL,
        ONICHANGE,
        NIGECHANGE,
        NONE
    }
}