using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using ComonData;

public class GameDirecter : NetworkBehaviour
{
    const sbyte ZITTAI = 1, RETAI = -1;

    [Header("ディレクタースクリプト")]
    [SerializeField] DirecterComon[] directers;
    [Header("生成するプレイヤー")]
    [SerializeField] NetworkPrefabRef _playerPrefab;
    [SerializeField] Transform[] spawnPos = new Transform[2];

    // ディレクター共通のデータ, 処理
    DirecterComon comon = new DirecterComon();
    DirecterComon.SceneDataHost hostData;
    DirecterComon.SceneDataPrivate privateData;

    //初期化、最初に行う処理  ----------------------------------------------------------------------------------
    public override void Spawned()
    {
        // ホストデータの初期化
        List<Transform> pos = new List<Transform> { spawnPos[0], spawnPos[1] };
        hostData = comon.InitHost(_playerPrefab, pos);
        // 個人データの初期化
        privateData = comon.InitPrivate(Runner.SessionInfo.PlayerCount - 1);
    }
    private void Awake()
    {

    }
    void Start()
    {
        
    }

    //アップデート処理　ネットワーク と ローカル  -----------------------------------------------------------------------
    public override void FixedUpdateNetwork()
    {
        directers[ (int)hostData.sceneType ].ProcessHost( hostData, out hostData );
    }
    void Update()
    {
        directers[(int)privateData.sceneType].ProcessPrivate(privateData, out privateData);
    }

    //見た目の処理　アニメーション　エフェクト　サウンド  ----------------------------------------------------------------
    public override void Render()
    {
        directers[ (int)privateData.sceneType ].RenderNetwork();
    }

    /* =================================================================================================================== */
    
    public void CreatePlayer(NetworkRunner runner, PlayerRef player, int num)
    {
        if (runner.IsServer)
        {
            int playerCount = num;
            // Create a unique position for the player
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPos[playerCount].position, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            hostData.playerRefs.Add(player);
            hostData._spawnedCharacters.Add(player, networkPlayerObject);
            PlayerController data = networkPlayerObject.GetComponent<PlayerController>();
            data.Init(Scene.LOBBY, playerCount, 0, ZITTAI, 100, 100, true);
        }
    }
    public void DestroyPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (player != hostData.playerRefs[0]) {
            directers[(int)hostData.sceneType].LeftPlayerHost(hostData, out hostData);
            directers[(int)privateData.sceneType].LeftPlayerPrivate(privateData, out privateData);
        }

        if (hostData._spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            hostData._spawnedCharacters.Remove(player);
        }
        hostData.playerRefs.Remove(player);
    }
    
    /* ============================================ コントローラー ========================================================== */
    public void OnSubmit(InputValue inputValue)
    {
        directers[(int)privateData.sceneType].Submit();
    }
    public void OnCancel(InputValue inputValue)
    {
        directers[(int)privateData.sceneType].Cancel();
    }
    public void OnMenu(InputValue inputValue)
    {
        directers[(int)privateData.sceneType].Menu();
    }
    public void OnCharMenu(InputValue inputValue)
    {
        directers[(int)privateData.sceneType].CharMenu();
    }
    public void OnStanby(InputValue inputValue)
    {
        directers[(int)privateData.sceneType].Stanby();
    }
    public void OnStart(InputValue inputValue)
    {
        directers[(int)privateData.sceneType].BattleStart();
    }
}
