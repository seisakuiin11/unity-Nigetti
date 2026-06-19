using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class AttackCorri : NetworkBehaviour
{
    [SerializeField, Header("ìGÇ©ÇÁíDÇ§ç∞")] int takeSoul = 1;
    [SerializeField] PlayerController player;

    bool Onlry = true;

    public void OnTriggerEnter(Collider other)
    {
        if (Onlry && other.tag == "Nige")
        {
            Debug.Log("çUåÇ");

            Onlry = false;
            this.gameObject.SetActive(false);
            player.GiveHit(takeSoul);
            other.GetComponent<PlayerController>().Damage(takeSoul);
            Task.Run(async() =>
            {
                await Task.Delay(200);
                Onlry = true;
            });
        }
    }
}