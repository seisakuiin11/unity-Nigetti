using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleDirecter : CommonDirecter
{
    const sbyte ZITTAI = 1, RETAI = -1;
    enum GameState : sbyte
    {
        NONE,
        DICE,
        STANBY,
        GAME,
        END
    }

    [SerializeField] int startSoul = 5;
    [SerializeField] int judgeSoul = 20;
    [SerializeField] float diceTime = 1f;
    [SerializeField] float stanbyTime = 2.5f;
    [SerializeField] float battleTime = 180f;
    [SerializeField] float cycleTime = 20f;
    [SerializeField] float endAnimTime = 3.1f;
    [SerializeField] float endTimeScale = 0.2f;
    [SerializeField, Header("バトルUI")] GameObject battleUI;
    [SerializeField, Header("鬼逃げ決定UI")] RandCountText diceUI;
    [SerializeField, Header("ゲームタイマー")] TextMeshProUGUI timer_t;
    [SerializeField, Header("サイクルタイマー")] ChangeTimer cycleTimerUI;
    [SerializeField, Header("魂所持数テキスト")] TextMeshProUGUI[] soulTexts;
    [SerializeField, Header("キャラアイコン")] Image[] icons;
    [SerializeField, Header("ゲームセット時の演出")] Animator endAnim;
    [SerializeField, Header("キャラアイコン素材")] Sprite[] charaIcons;

    [Networked] GameState state {  get; set; }
    [Networked] TickTimer GameTimer {  get; set; }
    [Networked] TickTimer CycleTimer { get; set; }
    [Networked] PlayerRef oni { get; set; }
    [Networked] PlayerRef nige { get; set; }

    [Networked, Capacity(GameManager.MaxPlayer)]
    NetworkDictionary<PlayerRef, PlayerController> players => default;

    UISoundScript SE;

    // 初期化
    public override void Init()
    {
        SE = FindAnyObjectByType<UISoundScript>();

        battleUI.SetActive(false);
    }

    // プレイヤー入室時
    public override void OnJoin(GameManager.HostData data, PlayerRef playerRef)
    {

    }

    // プレイヤー退出時
    public override void OnLeft(GameManager.HostData data, PlayerRef playerRef)
    {
        Runner.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }

    // シーン開始時に呼ばれる処理
    public override void StartMethod(GameManager.LocalData localData)
    {
        battleUI.SetActive(true);
        // 表記の変更
        diceUI.gameObject.SetActive(true);
        diceUI.SetName(localData.PlayerNum);
        SE.DiceSEPlay(); // 抽選SE

        if (!Object.HasStateAuthority) return;
        // Host処理

        state = GameState.DICE;
        GameTimer = TickTimer.CreateFromSeconds(Runner, diceTime);
    }

    // Hostのアップデート処理
    public override void NetworkUpdateMethod(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {
        switch (state)
        {
        case GameState.DICE:
            Decide(ref data, ref shareData);
            break;

        case GameState.STANBY:
            Stanby(ref data, ref shareData);
            break;

        case GameState.GAME:
            Game(ref data, ref shareData);
            break;

        case GameState.END:
            End(ref data, ref shareData);
            break;

        default: break;
        }
    }

    // 各個人で行うプライベートアップデート処理
    public override void UpdateMethod(ref GameManager.LocalData localData)
    {
        if(state != GameState.GAME) return;
        // ゲーム中UI

        // 残り時間 表示
        float? time = GameTimer.RemainingTime(Runner);
        if(time != null)
        {
            int minut = (int)time / 60;
            int second = (int)time - minut * 60;
            timer_t.text = minut + ":" + second.ToString("D2");
        }

        // 魂の所持数 表示
        int count = 0;
        foreach(var item in players) { soulTexts[count].text = item.Value.GetSoul().ToString(); count++; }
    }

    // シーン終了時に呼ばれる処理
    public override void EndMethod(GameManager.LocalData localData)
    {
        battleUI.SetActive(false);
    }

    // ===================================================================================================
    // 鬼と逃げを決める演出中
    void Decide(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {
        if(GameTimer.Expired(Runner))
        {
            state = GameState.STANBY;
            GameTimer = TickTimer.CreateFromSeconds(Runner, stanbyTime);
            // 鬼と逃げを決定
            AssignRoles(data.PlayerRefs);
        }
    }

    // 演出確定後の準備中
    void Stanby(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {
        if (GameTimer.Expired(Runner))
        {
            // ダイスUIの非表示
            RPC_DeleteDiceUI();

            // キャラ生成
            CreateCharacter(data);
            GameStart();
        }
    }

    // ゲーム中
    void Game(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {
        // ゲーム全体の経過時間
        CheckGameTimer();
        // サイクルの経過時間
        CheckCycleTimer();
        // 魂の所持数確認
        CheckSoulCount();
    }

    // ゲーム終了演出中
    void End(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {
        if (GameTimer.Expired(Runner))
        {
            Debug.Log(state);
            shareData.Winner = Judge();
            // 次のシーンへ
            shareData.Scene = GameManager.SceneType.RESULT;
        }
    }

    // 鬼と逃げを決める
    void AssignRoles(List<PlayerRef> playerRefs)
    {
        int num = Random.Range(0, 10) % 2; // 鬼プレイヤーの決定
        oni = playerRefs[num];
        nige = playerRefs[(num + 1) % 2];

        int p1 = num == 0 ? ZITTAI : RETAI; // 鬼がプレイヤー1なら
        int p2 = num == 1 ? ZITTAI : RETAI; // 鬼がプレイヤー2なら

        // 全体で演出の確定
        RPC_AssignRoles(p1, p2);
    }

    // 鬼と逃げの確定演出
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_AssignRoles(int p1, int p2)
    {
        diceUI.StartGameText(p1, p2);
    }
    // ダイス演出の非表示
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_DeleteDiceUI()
    {
        diceUI.gameObject.SetActive(false);
    }

    // ゲームスタート
    void GameStart()
    {
        state = GameState.GAME;

        // ゲーム全体のタイマーを設定
        GameTimer = TickTimer.CreateFromSeconds(Runner, battleTime);

        ResetCycle();
    }

    // ゲームセット
    void GameEnd()
    {
        state = GameState.END;
        GameTimer = TickTimer.CreateFromSeconds(Runner, endAnimTime);

        // プレイヤーを動けなくする
        foreach (var item in players) { 
            item.Value.SetStunTime();
            item.Value.RemoveOnHitAction(ResetCycle);
        }

        // 終了演出
        RPC_EndAnim();
    }

    // 終了演出
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_EndAnim()
    {
        Time.timeScale = endTimeScale;
        SE.FinishSEPlay();
        endAnim.gameObject.SetActive(true);
        endAnim.SetTrigger("End");
    }

    // キャラクターを生成する
    void CreateCharacter(GameManager.HostData data)
    {
        int count = 0;
        foreach(var pf in data.PlayerRefs)
        {
            // キャラの生成
            NetworkObject playerObj = data.Runner.Spawn(data.CharacterPrefab, data.CreatePositions[count].position, Quaternion.identity, pf);
            PlayerController player = playerObj.GetComponent<PlayerController>();
            player.Init(data.CharaIDs[count], startSoul, false);
            player.AddOnHitAction(ResetCycle);
            players.Add(pf, player);
            count++;
        }

        // 逃げに切り替える
        players[nige].ChangeTurn();
        // 鬼を動けないようにする
        players[oni].SetStunTime();
        players[oni].RPC_StunCountUI();

        // キャラアイコンの設定
        RPC_SetCharaIcon(data.CharaIDs.ToArray());
    }

    // キャラアイコンの設定
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetCharaIcon(int[] charaIDs)
    {
        for (int i = 0; i < charaIDs.Length; i++) { icons[i].sprite = charaIcons[charaIDs[i]]; }

        // サイクルタイマーUI 起動
        cycleTimerUI.SetActiveTimer();
    }

    // ゲーム全体のタイマー
    void CheckGameTimer()
    {
        // タイマー継続中
        if (!GameTimer.Expired(Runner)) return;

        // ゲーム終了
        GameEnd();
    }

    // サイクルのタイマー ➡ 鬼と逃げの切り替え
    void CheckCycleTimer()
    {
        // タイマー継続中
        if(!CycleTimer.Expired(Runner)) return;

        // 鬼と逃げを切り替える
        foreach (var item in players) item.Value.ChangeTurn();

        // サイクルリセット
        ResetCycle();
    }

    // サイクルをリセットする
    void ResetCycle()
    {
        CycleTimer = TickTimer.CreateFromSeconds(Runner,cycleTime);

        // プレイヤーのTurnを参照
        RPC_SetCycleTimerUI();
    }

    // サイクルタイマーUIの設定
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetCycleTimerUI()
    {
        int mode = players[Player].GetTurn();
        cycleTimerUI.ChangeUI(mode, cycleTime);
    }

    // 魂の所持数を参照➡上限に達したら、ゲーム終了
    void CheckSoulCount()
    {
        foreach(var item in players)
            if(item.Value.GetSoul() >= judgeSoul) { GameEnd(); break; }
    }

    // 勝者を決める
    PlayerRef Judge()
    {
        PlayerRef player = PlayerRef.None;

        int soul = 0;
        foreach(var item in players)
        {
            if (item.Value.GetSoul() < soul) continue;

            player = item.Key;
            // 同点なら
            if(soul == item.Value.GetSoul()) player = PlayerRef.None;
            soul = item.Value.GetSoul();
        }

        return player;
    }
}
