using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using ComonData;

public class BattleDirecter : DirecterComon
{
    const int DICE = 1, INGAME = 2, END = 3;
    const int ZITTAI = 1, RETAI = -1;

    [SerializeField,Header("サイクル時間")]
    int MAXTIME = 180;
    [SerializeField, Header("残り時間表示タイミング")]
    int CALLTIME = 5;
    [SerializeField, Header("目標魂獲得数")]
    int MAXSOULLINE = 20;
    [SerializeField, Header("デフォルト鬼スピード")]
    int SPEED_ONI = 20;
    [SerializeField, Header("魂いくつで　(小数点以下切り捨て)")]
    int p_SOUL = 2;
    [SerializeField, Header("スピードをいくつ上げるか")]
    int SPEED_p = 15;
    [Header("UI")]
    [SerializeField] GameObject battleUI;
    [SerializeField] GameObject diceUI;
    [SerializeField] TextMeshProUGUI timer_t;
    [SerializeField] TextMeshProUGUI soulP1_t;
    [SerializeField] TextMeshProUGUI soulP2_t;
    [SerializeField] Image charP1Icon;
    [SerializeField] Image charP2Icon;
    [Header("UI素材")]
    [SerializeField] Sprite[] charIcon;
    [SerializeField] Animator countTime_an;
    [SerializeField] Animator endAnim;

    [Networked] int next { get; set; }
    int winner;
    [Networked, OnChangedRender(nameof(Dice2Start))] int process { get; set; }
    [Networked] float timer { get; set; }
    [Networked] int wave { get; set; }
    [Networked, Capacity(2)] NetworkArray<int> turn { get; }
    [Networked] int soulP1 { get; set; }
    [Networked] int soulP2 { get; set; }
    [Networked, OnChangedRender(nameof(Call))] NetworkBool callFlag { get; set; }
    [Networked] TickTimer CountTime { get; set; }
    [Networked] TickTimer StartGame { get; set; }
    [Networked] TickTimer EndGame { get; set; }

    int playNum;
    SoulCreater soulCreater;
    NetworkObject[] networkObjects;
    PlayerController[] player = new PlayerController[2];
    UISoundScript SE;

    private void Awake()
    {
        SE = FindAnyObjectByType<UISoundScript>();
    }

    /* 初期化 スタート処理 ----------------------------------------------------------------------------- */
    public override SceneDataHost InitNetwork(SceneDataHost _data)
    {
        _data.processState = State.UPDATA;
        process = DICE;
        timer = MAXTIME;
        wave = 0;
        next = 0;
        soulCreater = FindAnyObjectByType<SoulCreater>();

        // 魂設定
        soulP1 = 5;
        soulP2 = 5;

        int oni = -1;                               // どちらが鬼か
        int[] Soul = new int[2] { soulP1, soulP2 }; // 魂所持数
        int[] Speed = new int[2];                   // スピード
        Transform[] spawnPos = _data.spawnPos.ToArray(); // 生成位置
        if(oni == -1) oni = Random.Range(0, 20) % 2;// 鬼がランダムなら

        //鬼と逃げを確定する
        if(oni == 0) // P1が鬼
        {
            turn.Set(0, ZITTAI);
            turn.Set(1, RETAI);
            int OniSpeed = 100 + SPEED_ONI;
            Speed = new int[2] { OniSpeed, 100 };
        }
        else if (oni == 1) // P2が鬼
        {
            turn.Set(0, RETAI);
            turn.Set(1, ZITTAI);
            int OniSpeed = 100 + SPEED_ONI;
            Speed = new int[2] { 100, OniSpeed };
            Transform tf = spawnPos[0]; // 生成位置の入れ替え
            spawnPos[0] = spawnPos[1];
            spawnPos[1] = tf;
        }

        //プレイヤーの生成
        if (Runner.IsServer)
        {
            for (int i = 0; i < _data.playerRefs.Count; i++)
            {
                NetworkObject playerObj = Runner.Spawn(_data.prefab, spawnPos[i].position, Quaternion.identity, _data.playerRefs[i]);
                _data._spawnedCharacters.Add(_data.playerRefs[i], playerObj);
                PlayerController data = playerObj.GetComponent<PlayerController>();
                RpcSetPlayerData(i, data);
                data.Init(Scene.BATTLE, i, _data.charNum[i], turn[i], Speed[i], Soul[i], false);
            }
        }
        player[0].SetEnemy(player[1]);
        player[1].SetEnemy(player[0]);

        return _data;
    }
    public override SceneDataPrivate Init(SceneDataPrivate _data)
    {
        _data.processState = State.UPDATA;
        playNum = _data.playNum;
        battleUI.SetActive(true);
        charP1Icon.sprite = charIcon[_data.charNum[0]];
        charP2Icon.sprite = charIcon[_data.charNum[1]];
        next = 0;

        return _data;
    }

    /* アップデート処理 --------------------------------------------------------------------------------- */
    public override SceneDataHost UpdataNetwork(SceneDataHost _data)
    {
        // ダイスアニメーション
        if(process == DICE)
        {
            if (StartGame.Expired(Runner))
            {
                process = INGAME;
                StartGame = TickTimer.None;
                player[0].SetOniGageFlag(true);
                player[1].SetOniGageFlag(true);

                if (player[0].GetTurn() == ZITTAI)
                {
                    player[0].SetMoveFlag(false, 5f);
                    player[1].SetMoveFlag(true, 0f);
                }
                else if (player[1].GetTurn() == ZITTAI)
                {
                    player[0].SetMoveFlag(true, 0f);
                    player[1].SetMoveFlag(false, 5f); 
                }
            }
            else return _data;
        }
        // ENDアニメーション
        if (process == END)
        {
            if (EndGame.Expired(Runner))
            {
                next = 1;
                _data.processState = State.END;
                EndGame = TickTimer.None;
                return _data;
            }
            return _data;
        }

        // INGAME プロセス
        timer -= Runner.DeltaTime;
        soulP1 = player[0].GetSoul();
        soulP2 = player[1].GetSoul();

        //残り5秒
        if(timer <= CALLTIME && !callFlag) callFlag = true;
        // 制限時間
        if(timer <= 0)
        {
            process = END;
        }
        // どちらかの魂所持数が目標に達したら
        if(soulP1 >= MAXSOULLINE || soulP2 >= MAXSOULLINE)
        {
            process = END;
        }

        return _data;
    }
    public override SceneDataPrivate Updata(SceneDataPrivate _data)
    {
        if (next == 1) _data.processState = State.END;
        return _data;
    }

    /* 終了処理 後かたずけ ------------------------------------------------------------------------------ */
    public override SceneDataHost EndNetwork(SceneDataHost _data)
    {
        foreach (var obj in _data._spawnedCharacters.Values)
        {
            Runner.Despawn(obj);
        }
        _data._spawnedCharacters.Clear();
        soulCreater.SoulDestroy();

        _data.processState = State.INIT;
        _data.sceneType = Scene.RESULT;
        return _data;
    }
    public override SceneDataPrivate End(SceneDataPrivate _data)
    {
        winner = -1;
        if (soulP1 > soulP2) winner = 0;
        else winner = 1;

        Time.timeScale = 1f;
        diceUI.SetActive(true);
        endAnim.gameObject.SetActive(false);
        battleUI.SetActive(false);
        _data.processState = State.INIT;
        _data.sceneType = Scene.RESULT;
        _data.winner = (sbyte)winner;
        return _data;
    }

    /* 描画関係処理 ------------------------------------------------------------------------------------ */
    public override void RenderNetwork()
    {
        //タイマー表示
        int minut = (int)timer / 60;
        int second = (int)timer - minut*60;
        timer_t.text = minut + ":" + second.ToString("D2");

        // 魂の所持数
        soulP1_t.text = soulP1.ToString();
        soulP2_t.text = soulP2.ToString();
    }

    /* クライアントが途中退出した場合(ホスト) ------------------------------------------------- */
    public override void LeftPlayerHost(SceneDataHost _data, out SceneDataHost data)
    {
        foreach (var obj in _data._spawnedCharacters.Values)
        {
            Runner.Despawn(obj);
        }
        _data._spawnedCharacters.Clear();
        soulCreater.SoulDestroy();
        // ホストのプレイヤー生成
        if (Runner.IsServer) {
            NetworkObject playerObj = Runner.Spawn(_data.prefab, _data.spawnPos[0].position, Quaternion.identity, _data.playerRefs[0]);
            _data._spawnedCharacters.Add(_data.playerRefs[0], playerObj);
            PlayerController dataP = playerObj.GetComponent<PlayerController>();
            dataP.Init(Scene.LOBBY, 0, 0, ZITTAI, 100, 100, true);
        }

        _data.processState = State.INIT;
        _data.sceneType = Scene.LOBBY;
        data = _data;
    }
    public override void LeftPlayerPrivate(SceneDataPrivate _data, out SceneDataPrivate data)
    {
        Time.timeScale = 1f;
        diceUI.SetActive(true);
        endAnim.gameObject.SetActive(false);
        battleUI.SetActive(false);
        _data.processState = State.INIT;
        _data.sceneType = Scene.LOBBY;
        data = _data;
    }

    /* ==================================================================================================== */

    public void GetPlayerNum(int num)
    {
        playNum = num;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    void RpcSetPlayerData(int Num, PlayerController Data)
    {
        player[Num] = Data;
    }

    // ダイスからゲームスタート,決着から試合終了までの処理
    void Dice2Start()
    {
        if(process == DICE)
        {
            StartCoroutine(DiceAnim());
            if (Object.HasStateAuthority)
            {
                StartGame = TickTimer.CreateFromSeconds(Runner, 3.5f);
            }
        }
        else if(process == INGAME)
        {
            
            SE.ChangeSEPlay(); // ゲームスタート SE (スプラテューンのような)
            //画面を切り替える
            diceUI.SetActive(false);
            FindAnyObjectByType<ChangeTimer>().SetFlag();
        }
        else if(process == END)
        {
            Time.timeScale = 0.2f;
            StartCoroutine(EndAnim()); // ENDアニメーション
            if(Object.HasStateAuthority)
            {
                player[0].SetMoveFlag(false, 0);
                player[1].SetMoveFlag(false, 0);
                EndGame = TickTimer.CreateFromSeconds(Runner, 3.1f); // 3.1秒後に次のシーンへ行く
            }
        }
    }
    IEnumerator DiceAnim()
    {
        SE.DiceSEPlay(); // 抽選 SE
        diceUI.GetComponent<RandCountText>().SetName(playNum);
        yield return new WaitForSeconds(1f);
        //鬼逃げの確定
        diceUI.GetComponent<RandCountText>().StartGameText(turn[0], turn[1]);
    }

    //残り時間コール カウントダウン?
    void Call()
    {
        if (callFlag)
        {
            countTime_an.SetTrigger("Play");
        }
        else
        {
            countTime_an.Play("None");
        }
    }

    //鬼のスピード計算
    int GetOniSpeed(int Soul_b, int Soul_s)
    {
        int OniSpeed = 100 + SPEED_ONI + ((Soul_b - Soul_s) / p_SOUL * SPEED_p);
        OniSpeed = OniSpeed > 160 ? 160 : OniSpeed;
        return OniSpeed;
    }

    // 試合終了時のアニメーション
    IEnumerator EndAnim()
    {
        SE.FinishSEPlay(); // ゲーム終了 SE
        yield return new WaitForSeconds(0.1f);
        endAnim.gameObject.SetActive(true);
        endAnim.SetTrigger("End");
        
    }

    /* ======================================== コントローラー ================================================== */
}
