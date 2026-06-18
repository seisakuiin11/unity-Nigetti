using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SoulCreater : NetworkBehaviour
{
    [SerializeField] int SOULMAX;
    [SerializeField] Transform SOULspos1;
    [SerializeField] Transform SOULspos2;
    Transform SOULspos;
    [SerializeField] private NetworkPrefabRef soulsPrefab;
    [SerializeField] GameObject soulEffect;
    List<Vector3> soulsPosition = new List<Vector3>();
    List<NetworkObject> soulObjects = new List<NetworkObject>();
    UISoundScript SE;

    private void Awake()
    {
        SE = FindAnyObjectByType<UISoundScript>();
    }

    public void SoulCreate(int stage)
    {
        if (!Runner.IsServer) return;
        SOULspos = null;
        if (stage == 1) SOULspos = SOULspos1;
        else if (stage == 2) SOULspos = SOULspos2;
        for (int i = 0; i < SOULspos.childCount; i++)
        {
            soulsPosition.Add(SOULspos.GetChild(i).transform.position);
        }
        //魂のポジションリストをシャッフル
        for (int i = 0; i < soulsPosition.Count; i++)
        {
            var pos = soulsPosition[i];
            var num = Random.Range(0, soulsPosition.Count - 1);
            soulsPosition[i] = soulsPosition[num];
            soulsPosition[num] = pos;
        }

        //魂の生成
        for (int i = 0; i < SOULMAX; i++)
        {
            NetworkObject soul = Runner.Spawn(soulsPrefab, soulsPosition[0], Quaternion.identity, PlayerRef.None);
            soul.GetComponent<SoulController>().soul_d = this;
            soulObjects.Add(soul);
            soulsPosition.RemoveAt(0);
        }
    }
    public void SoulDestroy()
    {
        for(int i = 0; i < soulObjects.Count; i++)
        {
            //soulsPosition.Add(soulObjects[i].transform.position);
            Runner.Despawn(soulObjects[i]);
        }
        soulObjects.Clear();
        soulsPosition.Clear();
    }
    public void SoulCountUp(NetworkObject mine)
    {
        RpcSoulEffect(mine.transform.position);
        soulsPosition.Add(mine.transform.position);
        soulObjects.Remove(mine);
        NetworkObject soul = Runner.Spawn(soulsPrefab, soulsPosition[0], Quaternion.identity, Object.StateAuthority);
        soul.GetComponent<SoulController>().soul_d = this;
        soulObjects.Add(soul);
        soulsPosition.RemoveAt(0);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    void RpcSoulEffect(Vector3 pos)
    {
        GameObject ef = Instantiate(soulEffect, pos, Quaternion.identity);
        SE.SoulSEPlay();
        StartCoroutine(DestroyEffect(ef));
    }
    IEnumerator DestroyEffect(GameObject effect)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(effect);
    }
}
