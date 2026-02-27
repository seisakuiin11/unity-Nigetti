using Fusion;
using Fusion.Addons.KCC;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ComonData;

public class SkillController : NetworkBehaviour
{
    const sbyte YUREI = 0, KYONSI = 1, KYUBI = 2, ONI = 3;
    const sbyte NONE = 0, START = 1, WIP = 2, END = 3, RESET = 4;

    [SerializeField] Animator anim;
    [SerializeField] GameObject attackCol;
    [SerializeField] GameObject YUREI_SkillPlace, YUREI_SkillObj, YUREI_Skill_Col;
    [SerializeField] GameObject[] yureiSkill_obj;
    [SerializeField] NetworkPrefabRef billPrefab;
    [SerializeField] EffectScript effects;
    [SerializeField] KyubiEffect kyubiEffect;

    [Networked] int charNum { get; set; }
    [Networked] int gameMode { get; set; }
    [Networked] int process { get; set; }
    [Networked] int skillCount { get; set; }
    [Networked] NetworkBool speedDownFlag { get; set; }
    [Networked] NetworkBool skillFlag { get; set; } //スキルが使えるか
    [Networked] NetworkBool skillStanby { get; set; }
    [Networked] TickTimer skillFlag_time { get; set; }
    [Networked] private TickTimer YureiRushTime { get; set; } // 幽霊スキルの前進
    [Networked] private TickTimer SoulDeleteTime { get; set; } // 魂消失時間
    [Networked] private TickTimer OniSkillTime { get; set; } // バフタイム
    [Networked] private TickTimer OniDownTime { get; set; } // デバフタイム

    
    int speedSave;
    Vector3 vec3;
    GameObject[] souls;
    KCC _kcc;
    PlayerController player, enemy;
    PlayerAudioScript audioP;
    UISkillDirecter skillUI_d;
    bool onlyAnim;
    bool reset;

    public override void Spawned()
    {
        player = GetComponent<PlayerController>();
        _kcc = GetComponent<KCC>();
        audioP = GetComponent<PlayerAudioScript>();
        if(Object.HasInputAuthority) skillUI_d = FindAnyObjectByType<UISkillDirecter>();
        process = NONE;
        reset = false;
    }
    public override void FixedUpdateNetwork()
    {
        if (skillFlag_time.Expired(Runner))
        {
            skillFlag = true;
            skillFlag_time = TickTimer.None;
            switch (charNum)
            {
                case 0:
                    if (skillCount > 0) skillCount = 0; break;
                case 1:
                    if (skillCount > 4) skillCount = 0; break;
                case 2:
                    if (skillCount > 0) skillCount = 0; break;
                case 3:
                    if (skillCount > 0) skillCount = 0; break;
            }
        }

        // 幽霊スキル
        if (YureiRushTime.Expired(Runner))//幽霊スキルのインターバル時間が切れた
        {
            YureiRushTime = TickTimer.None;
            YUREI_Skill_Col.SetActive(false);
            if (Object.HasStateAuthority)
            {
                process = END;
                player.SetMoveFlag(true, 0);
                if (gameMode == (int)Scene.LOBBY)
                {
                    skillFlag_time = TickTimer.CreateFromSeconds(Runner, 1f);
                }
                else if (gameMode == (int)Scene.BATTLE)
                {

                }
            }
            // UI操作
            if (gameMode == (int)Scene.LOBBY)
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 1f, false);
            }
            else if (gameMode == (int)Scene.BATTLE)
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 0f, false);
            }
        }
        else if (YureiRushTime.IsRunning && Object.HasStateAuthority)
        {
            _kcc.SetInputDirection(vec3 * 300f * Runner.DeltaTime);
        }

        //時間経過後の処理
        if (SoulDeleteTime.Expired(Runner))//九尾スキルの効果時間が切れた
        {
            process = END;
            SoulDeleteTime = TickTimer.None;
            if (Object.HasStateAuthority)
            {
                if(gameMode == (int)Scene.LOBBY)
                {
                    skillFlag_time = TickTimer.CreateFromSeconds(Runner, 0.5f);
                }
                else if (gameMode == (int)Scene.BATTLE)
                {

                }
            }
            // UI操作
            if (gameMode == (int)Scene.LOBBY)
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 0.5f, false);
            }
            else if (gameMode == (int)Scene.BATTLE)
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 0f, false);
            }
        }

        // 鬼スキル
        if (OniSkillTime.Expired(Runner))//鬼スキルの効果時間が切れた
        {
            process = WIP;
            OniSkillTime = TickTimer.None;
            if (Object.HasStateAuthority)
            {
                // 走れないようにする
                player.SetRunFlag(false, 3f);
                OniDownTime = TickTimer.CreateFromSeconds(Runner, 3f);
            }
        }

        if (OniDownTime.Expired(Runner))//鬼スキルのダウンタイムが切れた
        {
            process = END;
            OniDownTime = TickTimer.None;
            player.SetSpeed(speedSave);

            if (Object.HasStateAuthority)
            {
                if(gameMode == (int)Scene.LOBBY)
                {
                    skillFlag_time = TickTimer.CreateFromSeconds(Runner, 0.5f);
                }
                else if (gameMode == (int)Scene.BATTLE)
                {

                }
            }
            // UI操作
            if (gameMode == (int)Scene.LOBBY)
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 0.5f, false);
            }
            else if (gameMode == (int)Scene.BATTLE)
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 0f, false);
            }
        }
    }
    /* ------------------------------ 描画関係 ------------------------------------ */
    public override void Render()
    {
        switch (charNum)
        {
            case YUREI: // ゆーれい
                if (onlyAnim) break;
                if(process == START) // 回転
                {
                    onlyAnim = true;
                    anim.Play("SkillNone");
                    anim.SetBool("Skill", true);
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                else if(process == WIP) // 前進
                {
                    onlyAnim = true;
                    effects.EffectFollowType(EffectNum.YUREI, 0);
                    audioP.SkillSound(charNum);
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                else if(process == END) // 着地
                {
                    onlyAnim = true;
                    anim.SetBool("Skill", false);
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                break;
            case KYONSI: // キョンシー
                if(onlyAnim) break;
                if (process == START) // 投げる
                {
                    onlyAnim = true;
                    anim.Play("SkillNone");
                    anim.SetTrigger("Skill");
                    audioP.SkillSound(charNum);
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                break;
            case KYUBI: // 九尾
                if(onlyAnim) break;
                if (process == START) // 魂を消す
                {
                    onlyAnim = true;
                    souls = GameObject.FindGameObjectsWithTag("Soul");
                    if (souls.Length > 0)
                    {
                        foreach (GameObject soul in souls)
                        {
                            soul.SetActive(false);
                            effects.EffectFixedType(EffectNum.SOUL, 0, soul.transform.position);
                        }
                        kyubiEffect.PlayAnim(5); // 九尾の後ろに魂を表示 (残り時間を表現)
                        audioP.SkillSound(charNum);
                    }
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                else if (process == END) // 魂を戻す
                {
                    onlyAnim = true;
                    if (souls != null)
                    {
                        foreach (GameObject soul in souls)
                        {
                            soul.SetActive(true);
                            effects.EffectFixedType(EffectNum.SOUL, 0, soul.transform.position);
                        }
                    }
                    souls = null;
                    audioP.SkillSound(charNum);
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                break;
            case ONI: // 鬼
                if (onlyAnim) break;
                if (process == START) // スピードアップ
                {
                    onlyAnim = true;
                    attackCol.transform.localScale = new Vector3(1.35f, 1.35f, 1.35f);
                    anim.Play("SkillNone");
                    anim.SetInteger("Skill", 1);
                    effects.EffectFollowType(EffectNum.BUFF, 8);
                    audioP.SkillSound(charNum);
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                else if (process == WIP) // スピードダウン
                {
                    onlyAnim = true;
                    attackCol.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    anim.SetInteger("Skill", 2);
                    effects.EffectFollowType(EffectNum.DEBUFF, 2);
                    audioP.JiangshiHitSound();
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                else if (process == END) // 元のスピード
                {
                    onlyAnim = true;
                    anim.SetInteger("Skill", 0);
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        process = NONE;
                        onlyAnim = false;
                    });
                }
                break;
            default: break;
        }

        if (speedDownFlag)
        {
            effects.EffectFollowType(EffectNum.DEBUFF, 2);
            audioP.JiangshiHitSound();
            speedDownFlag = false;
        }

        if(process == RESET)
        {
            anim.Play("SkillNone");
            switch (charNum)
            {
                case YUREI:
                    anim.SetBool("Skill", false);
                    break;
                case ONI:
                    anim.SetInteger("Skill", 0);
                    break;
                default: break;
            }
            if (souls != null)
            {
                foreach (GameObject soul in souls)
                {
                    soul.SetActive(true);
                }
            }
            souls = null;
            attackCol.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            process = NONE;
        }
    }

    /* ================================================================================= */
    
    public void SetEnemy(PlayerController Enemy, int scene)
    {
        enemy = Enemy;
        gameMode = scene;
        Debug.Log(gameMode);
    }

    // スキルの準備状態
    public void StanbySkill(int CharNum, int scene)
    {
        if (skillFlag)
        {
            skillStanby = true;
            gameMode = scene;
            Skills(true, CharNum);
        }
    }

    // スキル使用
    public void UseSkill(int CharNum, int scene)
    {
        if (skillStanby)
        {
            skillStanby = false;
            skillFlag = false;
            Skills(false, CharNum);
        }
    }

    // スキルのリセット
    public void RsetSkill()
    {
        process = RESET;
        reset = true;
        if (skillStanby)
        {
            skillStanby = false;
        }
        if (!skillFlag)
        {
            skillFlag = true;
            skillFlag_time = TickTimer.None;
        }
        // 幽霊
        YUREI_SkillObj.SetActive(false);
        YUREI_Skill_Col.SetActive(false);
        if (Object.HasStateAuthority && YureiRushTime.IsRunning)
        {
            player.SetMoveFlag(true, 0);
            YureiRushTime = TickTimer.None;
        }
        // キョンシー
        // 九尾
        SoulDeleteTime = TickTimer.None;
        // 鬼
        if (Object.HasStateAuthority && (OniSkillTime.IsRunning || OniDownTime.IsRunning))
        {
            player.SetRunFlag(true, 0);
            player.SetSpeed(speedSave);
        }
        OniSkillTime = TickTimer.None;
        OniDownTime = TickTimer.None;

        skillCount = 0;
        effects.ResetEffect();
    }

    // スキルクールタイム
    public void SetSkillFlag(bool Flag, float time)
    {
        if (Object.HasStateAuthority)
        {
            skillFlag = Flag;
            skillFlag_time = TickTimer.None;
            if(time > 0)
            {
                skillFlag_time = TickTimer.CreateFromSeconds(Runner, time);
            }
        }
        if (skillUI_d != null) skillUI_d.UISkill2(0f, time, false);
    }

    public void SetAnim(Animator Anim)
    {
        anim = Anim;
    }

    /* ========================================= 各種スキル ============================================== */

    void Skills(bool Press,int CharNum)
    {
        charNum = CharNum;
        switch (charNum)
        {
            case YUREI:
                YureiSkill(Press, CharNum);
                break;

            case KYONSI:
                JiangshiSkill(Press, CharNum);
                break;
            case KYUBI:
                KyubiSkill(Press, CharNum);
                break;
            case ONI:
                OniSkill(Press, CharNum);
                break;
            default: break;
        }
    }

    /*---------------------- ↑スキル発動 ----↓幽霊のスキル ------------------------*/

    public void YureiSkill(bool press, int CharNum)
    {
        if (press)//押している最中
        {
            // 予測オブジェ
            YUREI_SkillObj.SetActive(true);
            if (!Object.HasInputAuthority)
            {   // 敵に予測が見えないようにする
                foreach(var obj in yureiSkill_obj) obj.SetActive(false);
                if (YUREI_SkillObj.TryGetComponent<Renderer>(out var skin)) skin.enabled = false;
                var skin_cn = YUREI_SkillObj.transform.GetChild(0).GetComponent<Renderer>();
                skin_cn.enabled = false;
            }
            reset = false;
        }
        else//離した
        {
            skillFlag = false;
            StartCoroutine(YureiSkillStart());
            if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false); // アイコングレー化
        }
    }

    private IEnumerator YureiSkillStart()
    {
        // アニメーション 回転
        YUREI_SkillObj.SetActive(false);
        YUREI_Skill_Col.SetActive(true);
        
        
        if (Object.HasStateAuthority)
        {
            process = START;
            skillCount++;
        }
        yield return new WaitForSeconds(0.8f);
        if (reset) { reset = false;  yield break; }
        // アニメーション 前進
        if (Object.HasStateAuthority)
        {
            Vector3 mePos = this.transform.position;
            Vector3 targetPos = YUREI_SkillPlace.transform.position;
            targetPos.y = mePos.y;
            vec3 = (targetPos - mePos);

            
            YureiRushTime = TickTimer.CreateFromSeconds(Runner, 0.5f);
            process = WIP;
        }
        player.SetMoveFlag(false, 0);
        
    }

    /*----------------------↑幽霊のスキル----↓キョンシーのスキル------------------------*/
    public void JiangshiSkill(bool press, int CharNum)
    {
        if (press)//押している最中
        {

        }
        else//離した
        {
            skillFlag = false;
            
            if (Object.HasStateAuthority)
            {
                process = START;
                skillCount++;
                NetworkObject networkBillObject;
                Vector3 pos = new Vector3(this.gameObject.transform.position.x, -0.5f, this.gameObject.transform.position.z) + transform.forward;
                networkBillObject = Runner.Spawn(billPrefab, pos, Quaternion.identity, PlayerRef.None);
                networkBillObject.GetComponent<Rigidbody>().AddForce(transform.forward * 700);
                networkBillObject.transform.rotation = transform.rotation;
                if (gameMode == (int)Scene.LOBBY)
                {
                    skillFlag_time = TickTimer.CreateFromSeconds(Runner, 0.2f);
                }
                else if (gameMode == (int)Scene.BATTLE)
                {
                    if(skillCount < 5)
                    {
                        skillFlag_time = TickTimer.CreateFromSeconds(Runner, 0.2f);
                    }
                }
            }
            StartCoroutine(UIdisp());
        }
    }
    IEnumerator UIdisp()
    {
        yield return new WaitForSeconds(0.1f);
        // UI操作
        if (gameMode == (int)Scene.LOBBY)
        {
            Debug.Log(skillCount);
            if (skillCount < 5)
            {
                float amount = (float)(5f - skillCount) / 5f;
                if (skillUI_d != null) skillUI_d.UISkill2(amount, 0, true);
            }
            else
            {
                if (skillUI_d != null) skillUI_d.UISkill2(0f, 0.4f, false);
                Task.Run(async () =>
                {
                    await Task.Delay(200);
                    skillCount = 0;
                });
            }
        }
        else if (gameMode == (int)Scene.BATTLE)
        {
            if (skillCount <= 5)
            {
                float amount = (float)(5f - skillCount) / 5f;
                if (skillUI_d != null) skillUI_d.UISkill2(amount, 0, true);
            }
        }
    }

    public void SpeedDown()
    {
        player.SetRunFlag(false, 3f);
        speedDownFlag = true;
    }

    /*----------------------↑キョンシーのスキル----↓九尾のスキル------------------------*/

    public void KyubiSkill(bool press, int CharNum)
    {
        if (press)//押している最中
        {

        }
        else//離した
        {
            
            
            if (Object.HasStateAuthority)
            {
                process = START;
                skillCount++;
                SoulDeleteTime = TickTimer.CreateFromSeconds(Runner, 5f);
                if (gameMode == (int)Scene.BATTLE)
                {
                    enemy.KyubiSkill();
                }
            }
            if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false); // アイコングレー化
        }
    }

    /*----------------------↑九尾のスキル----↓鬼のスキル------------------------*/

    public void OniSkill(bool press, int CharNum)
    {
        if (press)//押している途中
        {
            if (Object.HasStateAuthority)
            {
                process = START;
                skillCount++;
                speedSave = player.GetSpeed();
                player.SetSpeed(180);
                OniSkillTime = TickTimer.CreateFromSeconds(Runner, 8f);
            }
            
            if (skillUI_d != null) skillUI_d.UISkill2(1f, 0f, false); // アイコングレー化
        }
        else//離した
        {

        }
    }
}
