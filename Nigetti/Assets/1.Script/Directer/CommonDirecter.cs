using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class CommonDirecter : NetworkBehaviour
{
    [SerializeField] protected GameObject loadUI;
    protected PlayerRef Player { get; private set; }
    public void SetPlayerRef(PlayerRef _player) => Player = _player;

    /// <summary>
    /// 初期化
    /// </summary>
    public virtual void Init()
    {

    }

    /// <summary>
    /// プレイヤー入室時
    /// </summary>
    public virtual void OnJoin(GameManager.HostData data, PlayerRef playerRef)
    {

    }

    /// <summary>
    /// プレイヤー退出時
    /// </summary>
    public virtual void OnLeft(GameManager.HostData data, PlayerRef playerRef)
    {

    }

    /// <summary>
    /// シーン開始時に呼ばれる処理
    /// </summary>
    public virtual void StartMethod(GameManager.LocalData localData)
    {

    }

    /// <summary>
    /// Hostのアップデート処理
    /// </summary>
    public virtual void NetworkUpdateMethod(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {

    }

    /// <summary>
    /// 各個人で行うプライベートアップデート処理
    /// </summary>
    public virtual void UpdateMethod(ref GameManager.LocalData localData)
    {

    }

    /// <summary>
    /// シーン終了時に呼ばれる処理
    /// </summary>
    public virtual void EndMethod(GameManager.LocalData localData)
    {

    }

    // コントローラー ==================================================================
    // 決定
    public virtual void Submit(InputValue inputValue)
    {

    }

    // キャンセル
    public virtual void Cancel(InputValue inputValue)
    {
        
    }

    // 準備OK
    public virtual void Stanby(InputValue inputValue)
    {

    }

    // ゲーム開始
    public virtual void BattleStart(InputValue inputValue)
    {

    }

    // キャラ選択画面表示
    public virtual void CharMenu(InputValue inputValue)
    {

    }

    // 設定画面表示
    public virtual void Menu(InputValue inputValue)
    {

    }
}
