using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    //入力した値を渡すためのデータ箱
    public Vector3 direction;
    public bool stanby;
    public bool runCheck;
    public bool attackFlag;
    public bool attackStanby;
    public int _change;
    public int _char_change;
    public bool skillStanby;
    public bool skillFlag;
}