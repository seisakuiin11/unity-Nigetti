using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ComonData;

public class EffectScript : MonoBehaviour
{
    [System.Serializable]
    struct EffectData
    {
        public GameObject efg;
        public float time;
    }
    [Header("6:Soul, 7:ChangeOni, 8:ChangeNige")]
    [Header("0:Touch, 1:Tp, 2:Smoke, 3:Yurei, 4:Buff, 5:Debuff")]
    [SerializeField] IDictionary<EffectNum, EffectData> effects;
    List<EffectData> efgsFl;
    List<EffectData> efgsFx;

    // Start is called before the first frame update
    void Start()
    {
        efgsFl = new List<EffectData>();
        efgsFx = new List<EffectData>();
    }

    // Update is called once per frame
    void Update()
    {
        // 追従式エフェクトの経過時間
        CountTimeFollow();
        // 固定式エフェクトの経過時間
        CountTimeFixed();
    }

    /* ===================================================================================== */

    public void ResetEffect()
    {
        // 継続中のエフェクトが無ければ、何もしない
        if(efgsFl.Count == 0) return;
        for (int i = efgsFl.Count - 1; i >= 0; i--)
        {
            // 継続中のエフェクトを中断する
            efgsFl[i].efg.SetActive(false);
            efgsFl.RemoveAt(i);
        }
    }

    public void EffectFollowType(EffectNum type, float time)
    {
        // エフェクトをアクティブ化
        effects[type].efg.SetActive(true);
        // エフェクトの非アクティブ化
        EffectData data = new EffectData() { efg = effects[type].efg };
        if (time == 0) data.time = effects[type].time; // 経過時間の代入
        else { data.time = time; }
        efgsFl.Add(data);
    }
    public void EffectFixedType(EffectNum type, float time, Vector3 pos)
    {
        // エフェクトをアクティブ化
        GameObject efg = Instantiate(effects[type].efg, pos, Quaternion.identity);
        efg.SetActive(true);
        // エフェクトの非アクティブ化
        EffectData data = new EffectData() { efg = efg };
        if (time == 0) data.time = effects[type].time; // 経過時間の代入
        else { data.time = time; }
        efgsFx.Add(data);
    }

    void CountTimeFollow()
    {
        if (efgsFl.Count ==  0) return;

        for (int i = efgsFl.Count - 1; i >= 0; i--)
        {
            // 経過時間を代入する
            EffectData data = efgsFl[i];
            data.time -= Time.deltaTime;
            efgsFl[i] = data;
            if (efgsFl[i].time > 0) continue; // 経過時間内なら次へ

            // 経過時間を過ぎていたら
            efgsFl[i].efg.SetActive(false);
            efgsFl.RemoveAt(i);
        }
    }

    void CountTimeFixed()
    {
        if (efgsFx.Count == 0) return;

        for (int i = efgsFx.Count - 1; i >= 0; i--)
        {
            // 経過時間を代入する
            EffectData data = efgsFx[i];
            data.time -= Time.deltaTime;
            efgsFx[i] = data;
            if (efgsFx[i].time > 0) continue; // 経過時間内なら次へ

            // 経過時間を過ぎていたら
            Destroy(efgsFx[i].efg);
            efgsFx.RemoveAt(i);
        }
    }
}
