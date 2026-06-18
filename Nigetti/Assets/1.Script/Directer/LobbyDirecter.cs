using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class LobbyDirecter : CommonDirecter
{
    const sbyte NONE = 0, SETTING = 1, CHARA = 2;

    [SerializeField, Header("ロビー用UI")] GameObject lobbyUI;
    [SerializeField, Header("準備完了アイコン")] GameObject[] readyIcons;
    [SerializeField, Header("ゲーム開始アイコン")] GameObject startIcon;
    [SerializeField, Header("キャラ切り替え画面")] CharMenuManager charaMenuUI;
    [SerializeField, Header("設定画面")] SettingManager menuUI;
    [SerializeField, Header("ステージ")] GameObject[] stages;
    [SerializeField] SkyManager skyManager;
    [SerializeField] SoulCreater soulCreater;

    [Networked, Capacity(GameManager.MaxPlayer)]
    NetworkDictionary<PlayerRef, NetworkBool> readyFlags => default;    // 準備完了管理配列

    [Networked, Capacity(GameManager.MaxPlayer)]
    NetworkDictionary<PlayerRef, PlayerController> players => default;

    GameManager.HostData hostData;
    bool stanby;
    bool gameStart;
    bool waitBtn;
    sbyte uiType;


    // 初期化
    public override void Init()
    {
        menuUI.Init(ComonData.Scene.LOBBY);

        StartMethod(new GameManager.LocalData());
    }

    // プレイヤー入室時 Hostのみが行う
    public override void OnJoin(GameManager.HostData data, PlayerRef playerRef)
    {
        // ホストが接続 魂の生成
        if (Player == playerRef) { hostData = data; soulCreater.SoulCreate(1); }

        // プレイヤーの追加
        AddPlayer(data, playerRef);
    }

    // プレイヤー退出時 Hostのみが行う
    public override void OnLeft(GameManager.HostData data, PlayerRef playerRef)
    {
        // 参加者の削除
        readyFlags.Remove(playerRef);

        // キャラの削除
        NetworkObject playerObj = players[playerRef].GetComponent<NetworkObject>();
        data.Runner.Despawn(playerObj);
        players.Remove(playerRef);
    }

    // シーン開始時に呼ばれる処理
    public override void StartMethod(GameManager.LocalData localData)
    {
        // 必要UIの表示
        lobbyUI.SetActive(true);
        skyManager.SetSkyDefult();

        // ステージの用意
        foreach(var stage  in stages) stage.SetActive(false);
        stages[0].SetActive(true);

        // 準備完了アイコン非表示
        foreach (var icon in readyIcons) icon.SetActive(false);
        startIcon.SetActive(false);

        // まだ接続できていない
        if (Player == PlayerRef.None) return;

        // 接続ができているなら、Hostにお願いする
        RPC_RequestCreatePlayer(Player);
    }

    // Hostのアップデート処理
    public override void NetworkUpdateMethod(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {

        if (!gameStart) return;
        // キャラ選択画面に移動する

        // シーンの共有を行う
        shareData.Unification = true;

        // キャラクターの削除
        foreach (var player  in players)
        {
            NetworkObject playerObj = player.Value.GetComponent<NetworkObject>();
            data.Runner.Despawn(playerObj);
        }
        players.Clear();
        readyFlags.Clear();

        // 魂の削除
        soulCreater.SoulDestroy();

        shareData.Scene = GameManager.SceneType.SELECT;
        gameStart = false;

        Debug.Log("スタート");
    }

    // 各個人で行うプライベートアップデート処理
    public override void UpdateMethod(ref GameManager.LocalData localData)
    {
        // 準備完了アイコンの表示
        DispReadyIcon();

        // スタートアイコンの表示
        DispStartIcon();
    }

    // シーン終了時に呼ばれる処理
    public override void EndMethod(GameManager.LocalData localData)
    {
        stanby = false;

        // 準備完了アイコン非表示
        foreach (var icon in readyIcons) icon.SetActive(false);
        startIcon.SetActive(false);

        CloseMenu();

        // UI非表示
        lobbyUI.SetActive(false);

        // 音の再生

        // Host限定処理
        if (!Runner.IsServer) return;

        // 全員の準備完了を解除
        foreach(var item in readyFlags) readyFlags.Set(item.Key, false);
    }

    // ==========================================================================================

    void AddPlayer(GameManager.HostData data, PlayerRef playerRef)
    {
        // 参加者の登録
        readyFlags.Add(playerRef, false);

        // キャラの生成
        NetworkObject playerObj = data.Runner.Spawn(data.CharacterPrefab, data.CreatePositions[data.PlayerRefs.Count - 1].position, Quaternion.identity, playerRef);
        PlayerController player = playerObj.GetComponent<PlayerController>();
        player.Init(0, 100, true);
        players.Add(playerRef, player);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestCreatePlayer(PlayerRef _player) => AddPlayer(hostData, _player);

    // 準備完了
    void SetStanbyFlag(bool flag)
    {
        RPC_StanbyOK(flag, Player);
    }
    // 操作者本人なら、Hostに依頼する
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_StanbyOK(bool flag, PlayerRef sender)
    {
        readyFlags.Set(sender, flag);
    }

    // ゲームスタート
    void GameStart()
    {
        // Host以外に権限はない
        if (!Runner.IsServer) return;
        // 全員の準備ができていない
        if (!IsAllReady()) return;

        // ゲーム開始
        gameStart = true;
    }

    // 全員が準備完了しているか
    bool IsAllReady()
    {
        bool flag = true;

        if (readyFlags.Count < 2) return false;

        foreach (var item in readyFlags)
            if(item.Value == false) { flag = false; break; }

        return flag;
    }

    // キャラクター切り替え
    public void ChengeCharaID(int num)
    {
        RPC_ChangeCharaID(num, Player);
        CloseMenu();
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_ChangeCharaID(int num, PlayerRef sender)
    {
        players[sender].ChangeCharacter(num);
    }

    // 準備完了アイコンの表示
    void DispReadyIcon()
    {
        int i = 0;
        foreach(var item in readyFlags) // 準備完了確認
        {
            readyIcons[i].SetActive(item.Value); // 準備完了 表示
            i++;
        }

        // 余計なものは非表示
        for(;i < readyIcons.Length; i++) readyIcons[i].SetActive(false);
    }

    // スタートアイコンの表示
    void DispStartIcon()
    {
        startIcon.SetActive(IsAllReady());
    }

    public void CloseMenu()
    {
        if(uiType == NONE) return;

        if (uiType == SETTING)
            menuUI.Menu();

        else if(uiType == CHARA) 
            charaMenuUI.CharaMenu(players[Player].GetCharNum());

        uiType = NONE;
        players[Player].SetMenuOpenFlag(false);
    }

    public void EndGame()
    {
        loadUI.SetActive(true);
        Runner.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }

    // ローカル操作 ============================================================================

    // キャンセル
    public override void Cancel(InputValue inputValue)
    {
        if (waitBtn || uiType == NONE) return;

        waitBtn = true;

        bool open = false;
        if (uiType == SETTING) open = menuUI.Cancel();
        else if (uiType == CHARA) open = charaMenuUI.CharaMenu(players[Player].GetCharNum());

        players[Player].SetMenuOpenFlag(open);

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    // 準備OK
    public override void Stanby(InputValue inputValue)
    {
        if (waitBtn) return;

        waitBtn = true;

        stanby = !stanby;
        SetStanbyFlag(stanby);

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    // ゲーム開始
    public override void BattleStart(InputValue inputValue)
    {
        if (waitBtn) return;

        waitBtn = true;

        GameStart();

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    // キャラ選択画面表示
    public override void CharMenu(InputValue inputValue)
    {
        if (waitBtn || uiType == SETTING) return;

        waitBtn = true;

        bool open = charaMenuUI.CharaMenu(players[Player].GetCharNum());

        uiType = open ? CHARA : NONE;
        players[Player].SetMenuOpenFlag(open);

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    // 設定画面表示
    public override void Menu(InputValue inputValue)
    {
        if (waitBtn || uiType == CHARA) return;

        waitBtn = true;

        bool open = menuUI.Menu();

        uiType = open ? SETTING : NONE;
        players[Player].SetMenuOpenFlag(open);

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }
}
