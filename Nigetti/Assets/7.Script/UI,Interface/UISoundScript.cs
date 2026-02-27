using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISoundScript : MonoBehaviour
{
    [SerializeField] AudioClip DefaultBGM, ChurchBGM, ShrineBGM, TitleEnterSE, EnterSE, FinishSE, WindowSE, SelectSE, BackSE, SceneSelectSE, ChangeSE, WinnerSE, LoserSE, CountSE, DiceSE, SoulSE;
    public AudioSource BGMPlayer;
    public AudioSource SEPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        GameObject soundPlayer = GameObject.FindGameObjectWithTag("SoundPlayer");
        if (soundPlayer != null)
        {
            TitleDirecter directer = GameObject.FindAnyObjectByType<TitleDirecter>();
            //directer.SoundPlayerChange(soundPlayer.GetComponent<UISoundScript>());
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        BGMplay(0);
        this.gameObject.tag = "SoundPlayer";
    }

    public void BGMplay(int BGMnum)
    {
        switch (BGMnum)
        {
            case 0:
                BGMPlayer.clip = DefaultBGM;
                BGMPlayer.Play();
                break;
            case 1:
                BGMPlayer.clip = ChurchBGM;
                BGMPlayer.Play();
                break;
            case 2:
                BGMPlayer.clip = ShrineBGM;
                BGMPlayer.Play();

                break;
        }
    }

    public void TitleEnterSEPlay()
    {
        SEPlayer.PlayOneShot(TitleEnterSE);
    }
    public void EnterSEPlay()
    {
       SEPlayer.PlayOneShot(EnterSE);
    }
    public void FinishSEPlay()
    {
        SEPlayer.PlayOneShot(FinishSE);
    }
    public void WindowSEPlay()
    {
        SEPlayer.PlayOneShot(WindowSE);
    }
    public void SelectSEPlay()
    {
        SEPlayer.PlayOneShot(SelectSE);
    }
    public void BackSEPlay()
    {
        SEPlayer.PlayOneShot(BackSE);
    }
    public void SceneSelectSEPlay()
    {
        SEPlayer.PlayOneShot(SceneSelectSE);
    }
    public void ChangeSEPlay()
    {
        SEPlayer.PlayOneShot(ChangeSE);
    }
    public void WinnerSEPlay()
    {
        SEPlayer.PlayOneShot(WinnerSE);
    }
    public void LoserSEPlay()
    {
        SEPlayer.PlayOneShot(LoserSE);
    }
    public void CountSEPlay()
    {
        SEPlayer.PlayOneShot(CountSE);
    }
    public void DiceSEPlay()
    {
        SEPlayer.PlayOneShot(DiceSE);
    }
    public void SoulSEPlay()
    {
        SEPlayer.PlayOneShot(SoulSE);
    }
}
