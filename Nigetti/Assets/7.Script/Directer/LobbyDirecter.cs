using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading.Tasks;
using Cinemachine;
using ComonData;

public class LobbyDirecter : DirecterComon
{
    const int NONE = 0, SETTING = 1, CHARMENU = 2, LOST = 3;

    [SerializeField] GameObject stage1;
    [SerializeField] GameObject stage2;
    [SerializeField] GameObject lobbyUI;
    [SerializeField] GameObject stanbyP1_s;
    [SerializeField] GameObject stanbyP2_s;
    [SerializeField] GameObject startUI;
    [SerializeField] GameObject menu;
    [SerializeField] GameObject charMenu;
    [SerializeField] GameObject lostUI;
    [SerializeField] EventSystem uiSystem;
    [SerializeField] SkyManager skyManager;
    [SerializeField] CinemachineFreeLook cameraLook;

    [Networked] NetworkBool stanbyP1 { get; set; }
    [Networked] NetworkBool stanbyP2 { get; set; }
    [Networked] int next { get; set; }

    bool online = false;
    bool waitBtn = false;
    bool startGame = false;
    int playNum;
    int openUI = 0;
    //int nextScene = -1;
    SettingManager menu_s;
    SoulCreater soulCreater;
    PlayerController[] player = new PlayerController[2];
    UISoundScript SE;

    void Awake()
    {
        menu_s = menu.GetComponent<SettingManager>();
        soulCreater = GetComponent<SoulCreater>();
        SE = FindAnyObjectByType<UISoundScript>();
    }
    public override void Spawned()
    {
        online = true;
    }

    /* 初期化 スタート処理 ----------------------------------------------------------------------------- */
    public override SceneDataHost InitNetwork(SceneDataHost _data)
    {
        _data.processState = State.UPDATA;
        startGame = false;
        next = 0;
        soulCreater.SoulCreate(1);
        
        hostData = _data;
        return _data;
    }
    public override SceneDataPrivate Init(SceneDataPrivate _data)
    {
        _data.processState = State.UPDATA;
        playNum = _data.playNum;
        stage1.SetActive(true);
        stage2.SetActive(false);
        skyManager.SetSkyDefult();
        lobbyUI.SetActive(true);
        stanbyP1_s.SetActive(false);
        stanbyP2_s.SetActive(false);
        // nextScene = SELECT;
        
        privateData = _data;
        return _data;
    }

    /* アップデート処理 --------------------------------------------------------------------------------- */
    public override SceneDataHost UpdataNetwork(SceneDataHost _data)
    {
        if(startGame)
        {
            startGame = false;
            if (Object.HasStateAuthority && stanbyP1 && stanbyP2)
            {
                next = 1; //終了 次のシーンへ
                _data.processState = State.END;
            }
        }
        return _data; //継続
    }
    public override SceneDataPrivate Updata(SceneDataPrivate _data)
    {
        if (!online) return _data;
        if (next == 1) _data.processState = State.END;
        return _data; //継続
    }

    /* 終了処理 後かたずけ ------------------------------------------------------------------------------ */
    public override SceneDataHost EndNetwork(SceneDataHost _data)
    {
        stanbyP1 = false;
        stanbyP2 = false;
        // プレイヤーの削除
        Runner.Despawn( _data._spawnedCharacters[_data.playerRefs[0]] );
        Runner.Despawn( _data._spawnedCharacters[_data.playerRefs[1]] );
        _data._spawnedCharacters.Clear();

        soulCreater.SoulDestroy(); // 魂の削除
        _data.processState= State.INIT;
        _data.sceneType = Scene.SELECT; // 次のシーンへ

        return _data;
    }
    public override SceneDataPrivate End(SceneDataPrivate _data)
    {
        lobbyUI.SetActive(false);
        menu_s.gameObject.SetActive(false);
        openUI = NONE;
        SE.SceneSelectSEPlay();
        _data.processState = State.INIT;
        _data.sceneType = Scene.SELECT; // 次のシーンへ

        return _data;
    }

    /* 描画系処理 ------------------------------------------------------------------------------------ */
    public override void RenderNetwork()
    {
        stanbyP1_s.SetActive(stanbyP1);
        stanbyP2_s.SetActive(stanbyP2);
        if (stanbyP1 && stanbyP2) startUI.SetActive(true);
        else startUI.SetActive(false);
    }

    /* クライアントが抜けた場合の処理(ホスト) ------------------------------------------------------ */
    public override void LeftPlayerHost(SceneDataHost _data, out SceneDataHost data)
    {
        data = _data;
        stanbyP2 = false;
    }

    /* ==================================================================================================== */
    public void GetPlayerNum(int num)
    {
        playNum = num;
        online = true;
    }

    public void GetPlayer(int num, PlayerController Player)
    {
        Debug.Log(num);
        player[num] = Player;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcStanby(int num)
    {
        if (num == 0) stanbyP1 = !stanbyP1;
        else stanbyP2 = !stanbyP2;
    }
    //プレイヤーの行動制御
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RpcSetPlayerMove(int PlayNum, bool Flag, float time)
    {
        player[PlayNum].SetMoveFlag(Flag, time);
    }
    //プレイヤーのキャラクターチェンジ ボタンにセットする
    public void SetCharNum(int num)
    {
        if (waitBtn) return;
        waitBtn = true;

        openUI = NONE;
        RpcSetCharNum(playNum, num);
        charMenu.SetActive(false);
        RpcSetPlayerMove(playNum , true, 0);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cameraLook.enabled = true;

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RpcSetCharNum(int PlayNum, int num)
    {
        player[PlayNum].SetCharNum(num);
    }

    //部屋作成,入室に失敗したとき
    public void Lost()
    {
        openUI = LOST;
        lostUI.SetActive(true);
    }

    //ゲーム終了
    public void EndGame()
    {
        //nextScene = TITLE;
        next = 1;
        Runner.Shutdown();
    }
    /* ======================================== コントローラー ================================================== */
    public override void Submit()
    {
        if(waitBtn) return;

        if(openUI == LOST)
        {
            waitBtn = true;
            openUI = NONE;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
            lostUI.SetActive(false);
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    public override void Cancel()
    {
        if (waitBtn) return;

        if (openUI == CHARMENU)
        {
            waitBtn = true;
            openUI = NONE;
            SE.BackSEPlay();
            charMenu.SetActive(false);
            RpcSetPlayerMove(playNum, true, 0);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cameraLook.enabled = true;
        }
        else if (openUI == SETTING)
        {
            waitBtn = true;
            int close = menu_s.Cancel();
            if (close == 1) {
                openUI = NONE;
                RpcSetPlayerMove(playNum, true, 0);

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                cameraLook.enabled = true;
            }
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    public override void Stanby()
    {
        if (!online) return;
        RpcStanby(playNum);
    }

    public override void BattleStart()
    {
        startGame = true;
    }
    public override void Menu()
    {
        if (waitBtn) return;

        if (openUI == SETTING)
        {
            waitBtn = true;
            int close = menu_s.Menu();
            if (close == 1)
            {
                openUI = NONE;
                RpcSetPlayerMove(playNum, true, 0);

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                cameraLook.enabled = true;
            }            
        }
        else if (openUI == NONE)
        {
            waitBtn = true;
            openUI = SETTING;
            menu.SetActive(true);
            menu_s.Init(1);
            RpcSetPlayerMove(playNum, false, 0);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            cameraLook.enabled = false;
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    public override void CharMenu()
    {
        if (waitBtn) return;

        if (openUI == NONE)
        {
            waitBtn= true;
            openUI = CHARMENU;
            SE.WindowSEPlay();
            charMenu.SetActive(true);
            charMenu.GetComponent<CharMenuManager>().OpenCharMenu(player[playNum].GetCharNum());
            RpcSetPlayerMove(playNum, false, 0);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            cameraLook.enabled = false;
        }
        else if(openUI == CHARMENU)
        {
            waitBtn = true;
            openUI = NONE;
            SE.BackSEPlay();
            charMenu.SetActive(false);
            RpcSetPlayerMove(playNum, true, 0);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cameraLook.enabled = true;
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }
}
