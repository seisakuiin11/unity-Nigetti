using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;
using Fusion;

public class cameraController : NetworkBehaviour
{
        // Start is called before the first frame update
    public override void Spawned()
    {
        if(Object.HasInputAuthority) //自分のプレイしている世界だったら行う(他人が入ってきたときにその人の処理を行わないため)
        {
            var cameraTarget = FindObjectOfType<CinemachineFreeLook>();
            float speed = PlayerPrefs.GetFloat("CAMERASPEED", 150f);
            cameraTarget.m_XAxis.m_MaxSpeed = speed;
            name = "avatar" + Random.Range(0, 50);
            //カメラが追従するターゲットを代入
            cameraTarget.Follow = this.transform;
            cameraTarget.LookAt = this.transform.Find("hed");
        }
    }
}
