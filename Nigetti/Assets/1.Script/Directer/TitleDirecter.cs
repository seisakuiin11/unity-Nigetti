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
    }

    /* 初期化 スタート処理 ----------------------------------------------------------------------------- */
    void Start()
    {
        waitBtn = true;
        menu_s = menu.GetComponent<SettingManager>();
        roomID = "1234";
        inputID.text = roomID;

        process = TITLE;
        titleUI.SetActive(true);
        room.SetActive(false);
        menu_s.Init(ComonData.Scene.TITLE);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
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

    public void SetSEPlayer(UISoundScript uiSound) => SE = uiSound;

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
            bool open = menu_s.Cancel();
            if (!open) process = TITLE;
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
            bool open = menu_s.Menu();
            if (!open) {
                process = TITLE;

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else if(process == TITLE)
        {
            waitBtn = true;
            process = SETTING;
            menu_s.Menu();
        }

        Task.Run(async () =>
        {
            await Task.Delay(100);
            waitBtn = false;
        });
    }
}
