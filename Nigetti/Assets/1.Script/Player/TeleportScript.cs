using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    [SerializeField] GameObject thisObject;
    [SerializeField] GameObject target;
    bool canTp;
    Vector3 pos;

    void Awake()
    {
        pos = thisObject.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        canTp = false;
        PosReset();
        while (canTp == false)
        {
            bool hit = CapsuleCheck();
            if (hit)
            {
                if (thisObject.transform.localPosition.z >= -0.05f)
                {
                    thisObject.transform.localPosition = new Vector3(thisObject.transform.localPosition.x, thisObject.transform.localPosition.y, thisObject.transform.localPosition.z - 0.05f);
                }
            }
            else
            {
                canTp = true;
            }

        }
    }

    public void PosReset()
    {
        thisObject.transform.localPosition = pos;

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
    private void OnDrawGizmos()
    {
        // カプセルの開始点と終了点
        Vector3 start = new Vector3(thisObject.transform.position.x, thisObject.transform.position.y - 0.2f, thisObject.transform.position.z); // 開始点の位置を取得
        Vector3 end = new Vector3(thisObject.transform.position.x, thisObject.transform.position.y + 0.2f, thisObject.transform.position.z);// 終了点の位置を計算
        float radius = 0.2f;
        // カプセルの可視化
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);
        Gizmos.DrawLine(start + Vector3.up * radius, end + Vector3.up * radius);
        Gizmos.DrawLine(start - Vector3.up * radius, end - Vector3.up * radius);
    }
}

