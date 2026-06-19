using Fusion;
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
            Debug.Log("‚Åƒoƒt");
            other.GetComponent<PlayerController>().DisableRunning(3f);
        }
        if (!count) Runner.Despawn(obj); // ‚¨ŽD‚Ì”j‰ó
        count = true;
    }
}
