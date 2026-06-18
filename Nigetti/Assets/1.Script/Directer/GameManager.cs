using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ロビーからリザルトまでのゲームループを管理する
/// </summary>
public class GameManager : NetworkBehaviour
{
    /* 構造体 データ群 ===============================  */
    public const int MaxPlayer = 2;

    public enum SceneType
    {
        LOBBY,
        SELECT,
        BATTLE,
        RESULT
    }

    /// <summary>
    /// 全体で参照するデータ
    /// </summary>
    public struct ShareData : INetworkStruct
    {
        public SceneType Scene;
        public bool Unification;
        public PlayerRef Winner;
    }

    /// <summary>
    /// Hostが編集,使用するデータ
    /// </summary>
    public struct HostData
    {
        public NetworkRunner Runner;
        public List<PlayerRef> PlayerRefs;
        public NetworkPrefabRef CharacterPrefab;
        public List<Transform> CreatePositions;
        public List<int> CharaIDs;
    }

    /// <summary>
    /// 全プレイヤーが持つローカルデータ
    /// </summary>
    public struct LocalData
    {
        public bool Online;
        public SceneType Scene;
        public PlayerRef Player;
        public int PlayerNum;
        public PlayerRef Winner;
    }

    /* =============================================== */

    [SerializeField, Header("各シーンの管理者")] CommonDirecter[] sceneDirecters;
    [SerializeField, Header("キャラプレハブ")] NetworkPrefabRef characterPrefab;
    [SerializeField, Header("キャラの生成位置")] List<Transform> createPos;

    [Networked] ShareData shareData { get; set; }   // シーンの変更,勝者を共有

    LocalData localData;    // ローカルデータ (個人用)
    HostData hostData;      // ホスト専用データ
    SceneType sceneOld;     // シーン変更の検出用


    /// <summary>
    /// サーバーに接続
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="playerRef"></param>
    /// <param name="playerNum"></param>
    public void OnJoin(NetworkRunner runner, PlayerRef playerRef, int playerNum)
    {
        localData.Online = true;

        if (localData.Player == PlayerRef.None)
        {
            localData.Player = playerRef;
            localData.PlayerNum = playerNum;
            foreach(var directer in sceneDirecters) directer.SetPlayerRef(localData.Player);
        }

        if (!runner.IsServer) return;

        // Hostが入室
        if(playerNum == 0)
        {
            hostData = new HostData()
            {
                Runner = runner,
                PlayerRefs = new List<PlayerRef>(),
                CharacterPrefab = characterPrefab,
                CreatePositions = createPos,
                CharaIDs = new List<int>(),
            };

            shareData = new ShareData() { Scene = SceneType.LOBBY, Winner = PlayerRef.None };
        }

        // プレイヤーデータを登録
        hostData.PlayerRefs.Add(playerRef);
        hostData.CharaIDs.Add(0);

        // 各シーンの処理 (登録したデータを使って)
        sceneDirecters[(int)localData.Scene].OnJoin(hostData, playerRef);
    }

    /// <summary>
    /// サーバーから退出
    /// </summary>
    /// <param name="playerRef"></param>
    public void OnLeft(PlayerRef playerRef)
    {
        // プレイヤー番号を取得
        int num = -1;
        for(int i = 0; i < hostData.PlayerRefs.Count; i++)
            if (hostData.PlayerRefs[i] == playerRef) { num = i; break; }

        // 各シーンの処理 (データを削除する前に)
        sceneDirecters[(int)localData.Scene].OnLeft(hostData, playerRef);

        // 登録データを削除
        hostData.PlayerRefs.Remove(playerRef);
        hostData.CharaIDs.RemoveAt(num);
    }

    // プライベート初期化
    private void Start()
    {
        localData = new LocalData() { Online = false, Player = PlayerRef.None, Scene = SceneType.LOBBY };

        // 初期化
        foreach (var directer in sceneDirecters) directer.Init();

        sceneOld = SceneType.LOBBY;
    }

    // ネットワークアップデート処理
    public override void FixedUpdateNetwork()
    {
        var _share = shareData;
        sceneDirecters[(int)shareData.Scene].NetworkUpdateMethod(ref hostData, ref _share);
        shareData = _share;
    }

    // 各個人のプライベートアップデート処理
    private void Update()
    {
        if (!localData.Online) return;

        sceneDirecters[(int)localData.Scene].UpdateMethod(ref localData);

        // シーンの共有があれば
        if(shareData.Unification)
        {
            localData.Scene = shareData.Scene;
            localData.Winner = shareData.Winner;
        }

        // シーンが変わっていないなら
        if (localData.Scene == sceneOld) return;

        sceneDirecters[(int)sceneOld].EndMethod(localData);  // 過去のシーンを終了
        sceneOld = localData.Scene;
        sceneDirecters[(int)sceneOld].StartMethod(localData);// 現在シーンの起動
    }

    // コントローラー =================================================================================
    // 決定
    public void OnSubmit(InputValue inputValue)
    {
        sceneDirecters[(int)localData.Scene].Submit(inputValue);
    }

    // キャンセル
    public void OnCancel(InputValue inputValue)
    {
        sceneDirecters[(int)localData.Scene].Cancel(inputValue);
    }

    // 準備OK
    public void OnStanby(InputValue inputValue)
    {
        sceneDirecters[(int)localData.Scene].Stanby(inputValue);
    }

    // ゲーム開始
    public void OnStart(InputValue inputValue)
    {
        sceneDirecters[(int)localData.Scene].BattleStart(inputValue);
    }

    // キャラ選択画面表示
    public void OnCharMenu(InputValue inputValue)
    {
        sceneDirecters[(int)localData.Scene].CharMenu(inputValue);
    }

    // 設定画面表示
    public void OnMenu(InputValue inputValue)
    {
        sceneDirecters[(int)localData.Scene].Menu(inputValue);
    }
}
