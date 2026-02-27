using UnityEngine;
using Fusion;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using ComonData;

public class SelectDirecter : DirecterComon
{
    const int SELECT = 0, READY = 1, STAGE = 2;

    [SerializeField] GameObject selectUI;
    [SerializeField] GameObject loadUI;
    [SerializeField] GameObject[] charBtns;
    [SerializeField] GameObject okOhuda;
    [SerializeField] GameObject okP1Ohuda;
    [SerializeField] GameObject okP2Ohuda;
    [SerializeField] GameObject readyUI;
    [SerializeField] GameObject stageUI;
    [SerializeField] GameObject stageBtn;
    [SerializeField] GameObject stage1_btn;
    [SerializeField] Image stage1_sprite;
    [SerializeField] Image stage2_sprite;
    [SerializeField] EventSystem uiSystem;
    [SerializeField] GameObject stage1;
    [SerializeField] GameObject stage2;
    [SerializeField] SoulCreater soulCreater;
    [SerializeField] SkyManager skyManager;
    [SerializeField] CharMenuManager charMenuM;

    [Networked] int player1 { get; set; }
    [Networked] int player2 { get; set; }
    [Networked, OnChangedRender(nameof(StageSelect))] int stage { get; set; }
    [Networked] int gameStart { get; set; }

    int playNum;
    int process;

    /* 初期化 スタート処理 ----------------------------------------------------------------------------- */
    public override SceneDataHost InitNetwork(SceneDataHost _data)
    {
        _data.processState = State.UPDATA;
        player1 = -1;
        player2 = -1;
        stage = -1;
        gameStart = 0;

        return _data;
    }
    public override SceneDataPrivate Init(SceneDataPrivate _data)
    {
        _data.processState = State.UPDATA;
        playNum = _data.playNum;
        process = SELECT; // キャラクター選択状態
        selectUI.SetActive(true);
        loadUI.SetActive(true);
        stageUI.SetActive(false);
        StartCoroutine(Load());
        // ボタンを選択できるようにする
        foreach (var btn in charBtns) btn.GetComponent<CharMenuInterface>().enabled = true;
        gameStart = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        return _data;
    }

    /* アップデート処理 --------------------------------------------------------------------------------- */
    public override SceneDataHost UpdataNetwork(SceneDataHost _data)
    {
        if (gameStart != 0) _data.processState = State.END;
        return _data;
    }
    public override SceneDataPrivate Updata(SceneDataPrivate _data)
    {
        okP1Ohuda.SetActive(player1 != -1);
        okP2Ohuda.SetActive(player2 != -1);
        readyUI.SetActive(player1 != -1 && player2 != -1);
        if (gameStart != 0) _data.processState = State.END;
        return _data;
    }

    /* 終了処理 後かたずけ ------------------------------------------------------------------------------ */
    public override SceneDataHost EndNetwork(SceneDataHost _data)
    {
        soulCreater.SoulCreate(stage);
        _data.processState = State.INIT;
        _data.sceneType = Scene.BATTLE;
        _data.charNum.Clear();
        _data.charNum.Add(player1);
        _data.charNum.Add(player2);
        return _data;
    }
    public override SceneDataPrivate End(SceneDataPrivate _data)
    {
        if (stage == 1) {
            stage1.SetActive(true);
            stage2.SetActive(false);
            skyManager.SetSkyKyoukai();
        }
        else if (stage == 2)
        {
            stage1.SetActive(false);
            stage2.SetActive(true);
            skyManager.SetSkyZinzya();
        }
        
        UISoundScript adio = FindAnyObjectByType<UISoundScript>();
        adio.BGMplay(stage);
        okOhuda.SetActive(false);
        selectUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _data.processState = State.INIT;
        _data.sceneType = Scene.BATTLE;
        _data.charNum.Clear();
        _data.charNum.Add(player1);
        _data.charNum.Add(player2);
        return _data;
    }

    /* クライアントが途中退出した場合(ホスト) ------------------------------------------------- */
    public override void LeftPlayerHost(SceneDataHost _data, out SceneDataHost data)
    {
        _data.processState = State.INIT;
        _data.sceneType = Scene.LOBBY;
        _data.charNum.Clear();
        // ホストのプレイヤー生成
        if (Runner.IsServer) {
            NetworkObject playerObj = Runner.Spawn(_data.prefab, _data.spawnPos[0].position, Quaternion.identity, _data.playerRefs[0]);
            _data._spawnedCharacters.Add(_data.playerRefs[0], playerObj);
            PlayerController dataP = playerObj.GetComponent<PlayerController>();
            dataP.Init(Scene.LOBBY, 0, 0, 1, 100, 100, true);
        }

        data = _data;
    }
    public override void LeftPlayerPrivate(SceneDataPrivate _data, out SceneDataPrivate data)
    {
        okOhuda.SetActive(false);
        selectUI.SetActive(false);
        // ボタンを選択できるようにする
        foreach (var btn in charBtns) btn.GetComponent<CharMenuInterface>().enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _data.processState = State.INIT;
        _data.sceneType = Scene.LOBBY;
        _data.charNum.Clear();
        data = _data;
    }

    /* ==================================================================================================== */
    public void GetPlayerNum(int num)
    {
        playNum = num;
    }
    IEnumerator Load()
    {
        yield return new WaitForSeconds(1f);
        loadUI.SetActive(false);
        uiSystem.SetSelectedGameObject(charBtns[0]);
        charMenuM.SetCharMenu(0);
    }
    public void GoNextScene()
    {
        if (stage <= 0 || !Runner.IsServer) return;
        gameStart = 1;
    }
    public void SetCharNum(int num)
    {
        process = READY;
        RpcSetCharNum(playNum, num);
        okOhuda.SetActive(true);
        uiSystem.SetSelectedGameObject(null);
        // ボタンを選択できないようにする
        foreach(var btn in charBtns) btn.GetComponent<CharMenuInterface>().enabled = false;
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSetCharNum(int player, int num)
    {
        if (player == 0) player1 = num;
        if (player == 1) player2 = num;
    }
    public void SetStageNum(int num)
    {
        if(Object.HasStateAuthority) stage = num;
    }
    public void StageSelect()
    {
        RectTransform btn_a = stage1_btn.GetComponent<RectTransform>();
        RectTransform btn_b = stage1_btn.transform.GetChild(0).GetComponent<RectTransform>();
        Vector3 pos_a = btn_a.anchoredPosition;
        Vector3 pos_b = btn_b.anchoredPosition;
        switch (stage)
        {
            case 0:
                stageUI.SetActive(true);
                pos_a.x = -550f;
                pos_b.x = -140f;
                break;
            case 1:
                pos_a.x = 0f;
                pos_b.x = 0f;
                stage1_sprite.color = Color.white;
                stage2_sprite.color = new Color32(180, 180, 180, 255);
                break;
            case 2:
                pos_a.x = -1100f;
                pos_b.x = 1100f;
                stage1_sprite.color = new Color32(180, 180, 180, 255);
                stage2_sprite.color = Color.white;
                break;
            default:
                stageUI.SetActive(false);
                break;
        }
        btn_a.anchoredPosition = pos_a;
        btn_b.anchoredPosition = pos_b;
    }
    /* ======================================== コントローラー ================================================== */
    public override void Submit()
    {
        if(process == READY)
        {
            if (Object.HasStateAuthority && player1 != -1 && player2 != -1)
            {
                process = STAGE;
                stage = 0;
                uiSystem.SetSelectedGameObject(stageBtn);
            }
        }
    }
    public override void Cancel()
    {
        if(process == READY)
        {
            process = SELECT;
            
            okOhuda.SetActive(false);
            uiSystem.SetSelectedGameObject(charBtns[(playNum == 0 ? player1 : player2)]);
            RpcSetCharNum(playNum, -1);
            // ボタンを選択できるようにする
            foreach (var btn in charBtns) btn.GetComponent<CharMenuInterface>().enabled = true;
        }
    }

    public override void BattleStart()
    {
        Submit();
    }
}
