using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillCollisionScript : NetworkBehaviour
{
    [SerializeField]NetworkObject obj;
    bool count;
    private void Start()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Nige")
        {

            other.GetComponent<SkillController>().SpeedDown();
        }
        if (!count) Runner.Despawn(obj); //ç∞ÇÃîjâÛ
        count = true;
    }
}
