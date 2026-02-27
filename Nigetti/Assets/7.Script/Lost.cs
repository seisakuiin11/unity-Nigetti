using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Lost : MonoBehaviour
{
    [SerializeField] GameObject lostUI;

    bool lost;
    GameObject ui;

    // Start is called before the first frame update
    void Start()
    {
        lost = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LostGame(ShutdownReason shutdownReason)
    {
        lost = true;
        switch (shutdownReason)
        {
            case ShutdownReason.ServerInRoom:
                ui = lostUI;
                break;
            case ShutdownReason.DisconnectedByPluginLogic:
                ui = lostUI;
                break;
            default: break;
        }
        if(ui != null) ui.SetActive(true);
    }

    /* ======================= コントローラー ======================== */

    public void OnSubmit(InputValue inputValue)
    {
        if(!lost) return;
        SceneManager.LoadScene("TitleScene");
        //ui.SetActive(false);
    }
}
