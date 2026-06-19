using UnityEngine;

public class PlayerAudioScript : MonoBehaviour
{
    [SerializeField, Header("âπëfçﬁ")] AudioClip Yurei;
    [SerializeField]AudioClip  Jiangshi, JiangshiHit, Kyubi,Oni, Attack, Hit, TP, Invisible, StanbySE, GetSoulSE;
    [SerializeField,Header("âπÇçƒê∂Ç∑ÇÈÇ‚Ç¬")] AudioSource audioPlayer;
    UISoundScript AudioPlayer;


    // Start is called before the first frame update
    void Awake()
    {
        AudioPlayer = FindObjectOfType<UISoundScript>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SkillSound(int Charanum)
    {
        audioPlayer.volume = AudioPlayer.SEPlayer.volume;
        switch (Charanum)
        {
            case 0:
                audioPlayer.PlayOneShot(Yurei);
                break;
            case 1:
                audioPlayer.PlayOneShot(Jiangshi);
                break;
            case 2:
                audioPlayer.PlayOneShot(Kyubi);
                break;
            case 3:
                audioPlayer.PlayOneShot(Oni);
                break;

        }
    }

    public void NigeSkillSound(int SkillNum)//0Ç™TPÅA1Ç™ìßñæ
    {
        audioPlayer.volume = AudioPlayer.SEPlayer.volume;
        switch (SkillNum)
        {
            case 0:
                audioPlayer.PlayOneShot(TP);
                break;
            case 1:
                audioPlayer.PlayOneShot(Invisible);
                break;
        }
    }

    public void AttackSound()
    {
        audioPlayer.volume = AudioPlayer.SEPlayer.volume;
        audioPlayer.PlayOneShot(Attack);
    }
    public void HitSound() 
    {
        audioPlayer.volume = AudioPlayer.SEPlayer.volume;
        audioPlayer.PlayOneShot(Hit);
    }
    public void JiangshiHitSound()
    {
        audioPlayer.volume = AudioPlayer.SEPlayer.volume;
        audioPlayer.PlayOneShot(JiangshiHit);
    }
    public void StanbySEPlay()
    {
        audioPlayer.volume = AudioPlayer.SEPlayer.volume;
        audioPlayer.PlayOneShot(StanbySE);
    }
    public void GetSoulSEPlay()
    {
        audioPlayer.volume = AudioPlayer.SEPlayer.volume;
        audioPlayer.PlayOneShot(GetSoulSE);
    }
}
