using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    const int BGM = 0, CAMERA = 1, BACK = 2, END = 3, SETTING = 4;
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
        //カメラ感度スライダー値設定
        float speed_c = PlayerPrefs.GetFloat("CAMERASPEED", 200f);
        cameraSlider.value = speed_c;
        if(cameraLook != null) cameraLook.m_XAxis.m_MaxSpeed = cameraSlider.value;
        cameraSpeed_t.text = (cameraSlider.value / 10).ToString("f1");

        oldSelect = BGM;
        SE = FindAnyObjectByType<UISoundScript>();
    }

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /* ============================================================================ */
    public void Init(int Scene)
    {
        SE.WindowSEPlay();
        uiSystem.SetSelectedGameObject(menu_btn[oldSelect]);
        uiType = SETTING;
        Navigation btn_nav = menu_btn[CAMERA].GetComponent<Button>().navigation;
        if (Scene == 0) btn_nav = SetBtnTitle(btn_nav);
        else btn_nav = SetBtnLobby(btn_nav);
        menu_btn[CAMERA].GetComponent<Button>().navigation = btn_nav;
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
    public int Cancel()
    {
        SE.BackSEPlay();
        switch (uiType)
        {
        case BGM:
        case CAMERA:
            uiSystem.SetSelectedGameObject(menu_btn[uiType]);
            uiType = SETTING;
            break;
        case BACK:
        case END:
            this.gameObject.SetActive(true);
            if(BackBtn != null) BackBtn.SetActive(false);
            if(EndBtn != null) EndBtn.SetActive(false);
            uiSystem.SetSelectedGameObject(menu_btn[uiType]);
            uiType = SETTING;
            break;
        case SETTING:
            this.gameObject.SetActive(false);
            return 1;
        default: break;
        }
        return 0;
    }
    public int Menu()
    {
        if(uiType == BACK ||  uiType == END)
        {
            Cancel();
            return 0;
        }
        uiType = BGM;
        SE.BackSEPlay();
        this.gameObject.SetActive(false);
        return 1;
    }
}
