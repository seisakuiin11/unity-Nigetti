using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using TMPro;

public class TitleDirecter : MonoBehaviour
{
    const int TITLE = 0, ROOM = 1, SETTING = 4;
    int process = 0;

    [SerializeField] GameObject titleUI;
    [SerializeField] GameObject room;
    [SerializeField] GameObject room_btn;
    [SerializeField] TMP_InputField inputID;
    [SerializeField] GameObject menu;
    [SerializeField] GameObject loadUI;
    [SerializeField] EventSystem uiSystem;

    bool waitBtn;
    string roomID;
    SettingManager menu_s;
    UISoundScript SE;

    void Awake()
    {
        waitBtn = true;
        menu_s = menu.GetComponent<SettingManager>();
        SE = FindAnyObjectByType<UISoundScript>();
        roomID = "1234";
        inputID.text = roomID;

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }

    /* 初期化 スタート処理 ----------------------------------------------------------------------------- */
    void Start()
    {
        process = TITLE;
        titleUI.SetActive(true);
        room.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /* アップデート処理 --------------------------------------------------------------------------------- */
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !waitBtn)
        {
            OnSubmit();
        }
    }

    /* ==================================================================================================== */

    public void SetRoomID()
    {
        roomID = inputID.text;
    }

    public void SetBtn(GameObject btn)
    {
        uiSystem.SetSelectedGameObject(btn);
    }

    //部屋作成,入室ボタンのクリック時 ロビーへ移る
    public void GameStart(int mode)
    {
        if (waitBtn) return;
        loadUI.SetActive(true);
        PlayerPrefs.SetInt("Mode", mode);
        PlayerPrefs.SetString("ROOMID", roomID);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    
    //ゲーム終了
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
        Application.Quit();//ゲームプレイ終了
#endif
    }

    /* =================================コントローラー=========================================== */
    public void OnSubmit()
    {
        if (waitBtn) return;

        if(process == TITLE)
        {
            waitBtn = true;
            process = ROOM;
            SE.TitleEnterSEPlay();
            room.SetActive(true);
            uiSystem.SetSelectedGameObject(room_btn);
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }
    public void OnCancel()
    {
        if (waitBtn) return;

        if (process == ROOM)
        {
            waitBtn = true;
            process = TITLE;
            room.SetActive(false);
        }
        else if(process == SETTING)
        {
            waitBtn = true;
            int close = menu_s.Cancel();
            if (close == 1) process = TITLE;
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }
    public void OnMenu()
    {
        if (waitBtn) return;
        

        if(process == SETTING)
        {
            waitBtn = true;
            int close = menu_s.Menu();
            if (close == 1) process = TITLE;
        }
        else if(process == TITLE)
        {
            waitBtn = true;
            process = SETTING;
            menu.SetActive(true);
            menu_s.Init(0);
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }
}
