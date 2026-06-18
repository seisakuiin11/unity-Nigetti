using Cinemachine;
using ComonData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    const sbyte BGM = 0, CAMERA = 1, BACK = 2, END = 3, SETTING = 4, CLOSE = 5;
    int uiType = 0;

    [SerializeField] GameObject[] menu_btn;
    [Header("BGM SE")]
    AudioSource BGMPlayer;
    AudioSource SEPlayer;
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;
    [Header("画面 カメラ")]
    [SerializeField] CinemachineFreeLook cameraLook;
    [SerializeField] TMP_Dropdown sceneMode;
    [SerializeField] Slider cameraSlider;
    [SerializeField] TMP_InputField cameraSpeed_t;
    [Header("タイトルへ戻る ゲームを終了する")]
    [SerializeField] GameObject BackBtn;
    [SerializeField] GameObject EndBtn;
    [SerializeField] EventSystem uiSystem;

    int oldSelect;
    UISoundScript SE;


    // Start is called before the first frame update
    void Awake()
    {
        BGMPlayer = GameObject.Find("BGM").GetComponent<AudioSource>();
        SEPlayer = GameObject.Find("SE").GetComponent<AudioSource>();

        //BGM,SEのスライダー値設定
        BGMSlider.value = BGMPlayer.volume * 20f;
        SESlider.value = SEPlayer.volume * 10f;

        SE = FindAnyObjectByType<UISoundScript>();
    }

    /* ============================================================================ */
    public void Init(Scene scene)
    {
        uiType = CLOSE;
        oldSelect = BGM;

        // タイトルとロビーで表示するボタンを切り替える
        Navigation btn_nav = menu_btn[CAMERA].GetComponent<Button>().navigation;
        if (scene == Scene.LOBBY) btn_nav = SetBtnLobby(btn_nav);
        else btn_nav = SetBtnTitle(btn_nav);
        menu_btn[CAMERA].GetComponent<Button>().navigation = btn_nav;

        //カメラ感度スライダー値設定
        float speed_c = PlayerPrefs.GetFloat("CAMERASPEED", 200f);
        cameraSlider.value = speed_c;
        if (cameraLook != null) cameraLook.m_XAxis.m_MaxSpeed = cameraSlider.value;
        cameraSpeed_t.text = (cameraSlider.value / 10).ToString("f1");
    }
    public Navigation SetBtnTitle(Navigation btn_nav)
    {
        menu_btn[BACK].SetActive(false);
        menu_btn[END].SetActive(true);
        btn_nav.selectOnDown = menu_btn[END].GetComponent<Button>();
        return btn_nav;
    }
    public Navigation SetBtnLobby(Navigation btn_nav)
    {
        menu_btn[BACK].SetActive(true);
        menu_btn[END].SetActive(false);
        btn_nav.selectOnDown = menu_btn[BACK].GetComponent<Button>();
        return btn_nav;
    }

    // 設定画面を開く
    void OpenMenu()
    {
        uiType = SETTING;

        this.gameObject.SetActive(true);
        SE.WindowSEPlay();
        uiSystem.SetSelectedGameObject(menu_btn[oldSelect]);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // 設定画面を閉じる
    void CloseMenu()
    {
        uiType = CLOSE;

        SE.BackSEPlay();
        this.gameObject.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetUIType(int type)
    {
        uiType = type;
        if (type == BGM || type == CAMERA) oldSelect = type;
    }

    //選択ボタン設定
    public void SetBtn(GameObject btn)
    {
        uiSystem.SetSelectedGameObject(btn);
    }

    //BGM SE ボリューム設定
    public void BGMVolume()
    {
        BGMPlayer.volume = BGMSlider.value * 0.05f;
    }
    public void SEVolume()
    {
        SEPlayer.volume = SESlider.value * 0.1f;
    }
    //ウィンドウ切り替え
    public void WindowMode()
    {
        switch(sceneMode.value)
        {
            case 0:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                break; 
            case 1:
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
                break;
            default: break;
        }
    }
    //カメラ感度設定
    public void SetCameraSpeed()
    {
        if(cameraLook != null) cameraLook.m_XAxis.m_MaxSpeed = cameraSlider.value;
        PlayerPrefs.SetFloat("CAMERASPEED", cameraSlider.value);
        cameraSpeed_t.text = (cameraSlider.value / 10).ToString("f1");
    }

    /* =============================コントローラー======================================= */
    /// <summary>
    /// キャンセルボタンを押したとき
    /// </summary>
    /// <returns>開いているか</returns>
    public bool Cancel()
    {
        switch (uiType)
        {
        case BGM:
        case CAMERA:
            SE.BackSEPlay();
            uiSystem.SetSelectedGameObject(menu_btn[uiType]);
            uiType = SETTING;
            break;

        case BACK:
        case END:
            SE.BackSEPlay();
            this.gameObject.SetActive(true);
            if(BackBtn != null) BackBtn.SetActive(false);
            if(EndBtn != null) EndBtn.SetActive(false);
            uiSystem.SetSelectedGameObject(menu_btn[uiType]);
            uiType = SETTING;
            break;

        case SETTING:
            CloseMenu();
            return false;

        default: break;
        }

        return true;
    }

    /// <summary>
    /// メニューボタンを押したとき
    /// </summary>
    /// <returns>開いているか</returns>
    public bool Menu()
    {
        // 別ウィンドウを開いていたら
        if(uiType == BACK ||  uiType == END)
        {
            Cancel();
            return true; // 開いている
        }

        // 閉じていたら開く　開いていたら閉じる
        if(uiType == CLOSE) OpenMenu();
        else CloseMenu();

        return uiType != CLOSE;
    }
}
