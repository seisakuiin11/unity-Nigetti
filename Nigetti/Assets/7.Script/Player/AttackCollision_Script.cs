using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class AttackCorri : NetworkBehaviour
{
    bool Onlry = true;

    public void OnTriggerEnter(Collider other)
    {
        if (Onlry && other.tag == "Nige")
        {
            Onlry = false;
            this.gameObject.SetActive(false);
            other.GetComponent<PlayerController>().Damage();
            Task.Run(async() =>
            {
                await Task.Delay(200);
                Onlry = true;
            });
        }
    }
}