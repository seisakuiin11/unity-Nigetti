using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResultDirecter : CommonDirecter
{
    [SerializeField] GameObject resultUI;
    [SerializeField] GameObject winnerUI;
    [SerializeField] GameObject loserUI;

    bool next;
    UISoundScript SE;

    // 初期化
    public override void Init()
    {
        SE = FindAnyObjectByType<UISoundScript>();
        resultUI.SetActive(false);
    }

    // プレイヤー入室時
    public override void OnJoin(GameManager.HostData data, PlayerRef playerRef)
    {

    }

    // プレイヤー退出時
    public override void OnLeft(GameManager.HostData data, PlayerRef playerRef)
    {

    }

    // シーン開始時に呼ばれる処理
    public override void StartMethod(GameManager.LocalData localData)
    {
        // 結果を表示
        resultUI.SetActive(true);
        Time.timeScale = 1.0f; // 再生速度を戻す
        bool win = Player == localData.Winner;
        if(localData.Winner == PlayerRef.None) win = true; // 勝者がいないなら、全員勝者
        winnerUI.SetActive(win);
        loserUI.SetActive(!win);

        // サウンド
        if (win) SE.WinnerSEPlay();
        else SE.LoserSEPlay();
        SE.BGMplay(0);
    }

    // Hostのアップデート処理
    public override void NetworkUpdateMethod(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {
        if (!next) return;

        // シーンの共有をやめる
        shareData.Unification = false;
        shareData.Scene = GameManager.SceneType.LOBBY;
    }

    // 各個人で行うプライベートアップデート処理
    public override void UpdateMethod(ref GameManager.LocalData localData)
    {
        if (!next) return;

        resultUI.SetActive(false);
        localData.Scene = GameManager.SceneType.LOBBY;
    }

    // シーン終了時に呼ばれる処理
    public override void EndMethod(GameManager.LocalData localData)
    {
        next = false;
    }

    // ローカル操作 ============================================================================
    // 決定
    public override void Submit(InputValue inputValue)
    {
        next = true;
        SE.EnterSEPlay();
    }
}
