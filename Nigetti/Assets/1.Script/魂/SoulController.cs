using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulController : NetworkBehaviour
{
    [Networked]
    public SoulCreater soul_d {  get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Nige") //プレイヤー(逃げ)なら
        {
            other.gameObject.GetComponent<PlayerController>().SetAddSoul(1); //魂の保有数Up
            soul_d.SoulCountUp(this.Object); //魂の生成
            NetworkObject networkObject = this.GetComponent<NetworkObject>();
            Runner.Despawn(networkObject); //魂の破壊
        }
        
    }
}
