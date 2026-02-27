using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using TMPro;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] GameObject loadeUI;
    [SerializeField] Lost lost;
    [SerializeField] TextMeshProUGUI roomID_text;
    [Header("生成するプレイヤー")]
    [SerializeField] NetworkPrefabRef _playerPrefab;
    [Networked] public static int winner { get; set; } = 0;
    GameDirecter directer;
    Vector3 inputDirection;
    bool skill, skillstanby, attack, attackstanby, stanby, runCheck;
    int _change;
    StartGameResult result;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("接続");
        //プレイヤー入室によるプレイヤー生成
        directer.CreatePlayer(runner, player, runner.SessionInfo.PlayerCount - 1);

        loadeUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        directer.DestroyPlayer(runner, player);
    }
    /* ------------  コントローラー入力  ----------------- */
    public void OnMove(InputValue value)
    {
        inputDirection = Vector3.zero;
        var axis = value.Get<Vector2>();
        inputDirection = new Vector3(axis.x,0,axis.y);
    }
    public void OnRunShift(InputValue value)
    {
        runCheck = true;
    }
    public void OnAttack_Press(InputValue value)//捕まえる動作
    {
        attackstanby = true;//攻撃用意
    }
    public void OnAttack_Release(InputValue value)//捕まえる動作
    {
       attack = true;//攻撃
    }
    public void OnReady(InputValue value)
    {
        stanby = true;
    }
    public void OnLbtn(InputValue value)
    {
        _change = 1;
    }
    public void OnRbtn(InputValue value)
    {
        _change = -1;
    }
    public void OnSkill_Press(InputValue value)
    {
        skillstanby = true;
    }
    public void OnSkill_Release(InputValue value)
    {

        skill = true;
    }
    /* ---------------------------------------------------------- */
    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {
        var data = new NetworkInputData();
        var cameraR = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        data.stanby = stanby;
        data._change = _change;
        data.direction = cameraR * inputDirection; //カメラを正面とした移動方向へ変更
        data.runCheck = runCheck;
        data.attackStanby = attackstanby;
        data.attackFlag = attack;
        data.skillStanby = skillstanby;
        data.skillFlag = skill;

        input.Set(data);
        runCheck = false;
        attack = false;
        attackstanby = false;
        stanby = false;
        skillstanby = false;
        skill = false;
        _change = 0;
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        Debug.Log("切断");
        Debug.Log(shutdownReason);
        //if(shutdownReason == ShutdownReason.DisconnectedByPluginLogic) { } ホストが退出,キックされる
        //if(shutdownReason == ShutdownReason.ServerInRoom) { } すでに部屋がある
        if(shutdownReason == ShutdownReason.Ok) SceneManager.LoadScene("TitleScene"); //意図したシャットダウン
        else
        {
            lost.LostGame(shutdownReason);
        }
        //UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
        
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) 
    {
        runner.Shutdown();
    }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    private NetworkRunner _runner;

    async void StartGame(GameMode mode, int mode_num)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        //gameObject.AddComponent<RunnerSimulatePhysics3D>();
        _runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        string roomID = PlayerPrefs.GetString("ROOMID");
        roomID_text.text = "部屋ID："+ roomID;
        // Start or join (depends on gamemode) a session with a specific name
        result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomID,
            Scene = scene,
            PlayerCount = 2,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        loadeUI.SetActive(false);
        if (result != null && result.Ok)
        {
            Debug.Log("成功");
            loadeUI.SetActive(false);
        }
        else
        {
            Debug.Log("失敗");
        }
    }

    //public void GameStart(int mode)
    //{
    //    if (_runner == null)
    //    {
    //        Debug.Log("ゲームスタート");
    //        loadeUI.SetActive(true);
    //        if (mode == 1) StartGame(GameMode.Host, mode);
    //        if (mode == 2) StartGame(GameMode.AutoHostOrClient, mode);
    //    }
    //}

    void Awake()
    {
        loadeUI.SetActive(true);
        directer = FindAnyObjectByType<GameDirecter>();
        if (_runner == null)
        {
            int mode = PlayerPrefs.GetInt("Mode");
            if (mode == 0) return;
            Debug.Log("ゲームスタート");
            if (mode == 1) StartGame(GameMode.Host, mode);
            if (mode == 2) StartGame(GameMode.AutoHostOrClient, mode);
            PlayerPrefs.SetInt("Mode", 0);
        }
    }
}