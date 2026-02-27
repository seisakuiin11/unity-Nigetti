using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharcterDirecter : NetworkBehaviour
{
    const int ZITTAI = 1, RETAI = -1;

    public SkinnedMeshRenderer[] char_zt;
    public SkinnedMeshRenderer[] char_rt;
    public RuntimeAnimatorController[] anim;
    [SerializeField] GameObject[] char_skin_z;
    [SerializeField] GameObject[] char_skin_r;
    [SerializeField] Material[] inv_char_m;
    [SerializeField] GameObject[] char_tpSkin;

    GameObject skin_now;
    Material skin_now_m;
    GameObject tpSkin;
    int mode = ZITTAI;

    // Start is called before the first frame update
    void Start()
    {

    }

    public GameObject ChangeSkin(int charNum, int turn)
    {
        if(skin_now != null) skin_now.SetActive(false);
        if(tpSkin != null) tpSkin.SetActive(false);
        // レータイ
        if(turn == -1) skin_now = char_skin_r[charNum];
        // ジッタイ
        else if(turn == 1) skin_now = char_skin_z[charNum];
        skin_now.SetActive(true);
        skin_now_m = inv_char_m[charNum];
        tpSkin = char_tpSkin[charNum];
        tpSkin.SetActive(true);
        mode = ZITTAI;

        return skin_now;
    }

    public void InvisivleSkin(int Mode)
    {
        if (mode == Mode) return;
        mode = Mode;
        Material material = skin_now.GetComponentInChildren<SkinnedMeshRenderer>().material;
        skin_now.GetComponentInChildren<SkinnedMeshRenderer>().material = skin_now_m;
        skin_now_m = material;
    }

    public Animator Anim(int charNum)
    {
        return skin_now.GetComponent<Animator>();
    }
}
