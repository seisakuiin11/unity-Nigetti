using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushScript : MonoBehaviour
{
    [SerializeField] GameObject thisObject;
    bool canRush;
    // Update is called once per frame
    void Update()
    {

        canRush = false;
        PosReset();
        while (canRush == false)
        {

            bool hit = CapsuleCheck();
            if (hit)
            {
                thisObject.transform.localPosition = new Vector3(thisObject.transform.localPosition.x, thisObject.transform.localPosition.y, thisObject.transform.localPosition.z - 0.05f);
                canRush = true;
            
            }
            else
            {
                if (thisObject.transform.localPosition.z <= 4f)
                {
                    thisObject.transform.localPosition = new Vector3(thisObject.transform.localPosition.x, thisObject.transform.localPosition.y, thisObject.transform.localPosition.z + 0.05f);
                }
                else
                {
                    canRush = true;
                }
            }


        }


    }

    public void PosReset()
    {
        thisObject.transform.localPosition = new Vector3(0, -0.5f, 0);

    }
    public bool CapsuleCheck()
    {
        LayerMask layerMask = LayerMask.GetMask("Default");
        Vector3 meStartPos = new Vector3(thisObject.transform.position.x, thisObject.transform.position.y - 0.2f, thisObject.transform.position.z);
        Vector3 meEndPos = new Vector3(thisObject.transform.position.x, thisObject.transform.position.y + 0.2f, thisObject.transform.position.z);
        float radius = 0.2f;
        bool hitcheck = Physics.CheckCapsule(meStartPos, meEndPos, radius, layerMask);
        return hitcheck;
    }
  
}
