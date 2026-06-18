using Fusion;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectDirecter : CommonDirecter
{
    const int SELECT = 0, READY = 1, STAGE = 2;

    [SerializeField] GameObject selectUI;
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
    [Networked] bool gameStart { get; set; }

    int playNum;
    int process;

    // 初期化
    public override void Init()
    {
        selectUI.SetActive(false);
        stageUI.SetActive(false);
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
        process = SELECT; // キャラクター選択状態

        playNum = localData.PlayerNum;

        selectUI.SetActive(true);
        loadUI.SetActive(true);
        stageUI.SetActive(false);
        Load();

        // ボタンを選択できるようにする
        foreach (var btn in charBtns) btn.GetComponent<CharMenuInterface>().enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!Object.HasStateAuthority) return;
        // ホストが行う

        player1 = -1;
        player2 = -1;
        stage = -1;
        gameStart = false;
    }

    // Hostのアップデート処理
    public override void NetworkUpdateMethod(ref GameManager.HostData data, ref GameManager.ShareData shareData)
    {
        if (!gameStart) return;

        shareData.Unification = true;

        data.CharaIDs = new List<int>() { player1, player2 };

        shareData.Scene = GameManager.SceneType.BATTLE;
        gameStart = false;
    }

    // 各個人で行うプライベートアップデート処理
    public override void UpdateMethod(ref GameManager.LocalData localData)
    {
        okP1Ohuda.SetActive(player1 != -1);
        okP2Ohuda.SetActive(player2 != -1);
        readyUI.SetActive(player1 != -1 && player2 != -1);
    }

    // シーン終了時に呼ばれる処理
    public override void EndMethod(GameManager.LocalData localData)
    {
        if (stage == 1)
        {
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

        // 魂 生成
        if(Object.HasStateAuthority)
            soulCreater.SoulCreate(stage);

        UISoundScript adio = FindAnyObjectByType<UISoundScript>();
        adio.BGMplay(stage);
        okOhuda.SetActive(false);
        selectUI.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // ==========================================================================================

    async void Load()
    {
        await Task.Delay(1000);

        loadUI.SetActive(false);
        uiSystem.SetSelectedGameObject(charBtns[0]);
        charMenuM.SetCharMenu(0);
    }

    public void GoNextScene()
    {
        if (stage <= 0 || !Runner.IsServer) return;
        gameStart = true;
    }

    public void SetCharNum(int num)
    {
        process = READY;
        RpcSetCharNum(playNum, num);
        okOhuda.SetActive(true);
        uiSystem.SetSelectedGameObject(null);
        // ボタンを選択できないようにする
        foreach (var btn in charBtns) btn.GetComponent<CharMenuInterface>().enabled = false;
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSetCharNum(int player, int num)
    {
        if (player == 0) player1 = num;
        if (player == 1) player2 = num;
    }

    public void SetStageNum(int num)
    {
        if (Object.HasStateAuthority) stage = num;
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

    // ローカル操作 ============================================================================
    // 決定
    public override void Submit(InputValue inputValue)
    {
        if (process != READY) return;

        if (Object.HasStateAuthority && player1 != -1 && player2 != -1)
        {
            process = STAGE;
            stage = 0;
            uiSystem.SetSelectedGameObject(stageBtn);
        }
    }

    // キャンセル
    public override void Cancel(InputValue inputValue)
    {
        if (process != READY) return;

        process = SELECT;

        okOhuda.SetActive(false);
        uiSystem.SetSelectedGameObject(charBtns[(playNum == 0 ? player1 : player2)]);
        RpcSetCharNum(playNum, -1);
        // ボタンを選択できるようにする
        foreach (var btn in charBtns) btn.GetComponent<CharMenuInterface>().enabled = true;
    }
}
