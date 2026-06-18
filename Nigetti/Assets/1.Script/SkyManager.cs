using UnityEngine;

public class SkyManager : MonoBehaviour
{
    [SerializeField] Material Sky_defult;
    [SerializeField] Material Sky_kyoukai;
    [SerializeField] Material Sky_zinzya;

    float speed = 0.5f;
    Material Skybox;

    // Start is called before the first frame update
    void Start()
    {
        Skybox = RenderSettings.skybox;
    }

    // Update is called once per frame
    void Update()
    {
        if (Skybox == null) return;

        if (!Skybox.HasProperty("_Rotation")) return;

        float rotation = Skybox.GetFloat("_Rotation");
        rotation += speed * Time.deltaTime;

        Skybox.SetFloat("_Rotation", rotation % 360f);
    }

    public void SetSkyDefult()
    {
        RenderSettings.skybox = Sky_defult;
        Skybox = RenderSettings.skybox;
    }
    public void SetSkyKyoukai()
    {
        RenderSettings.skybox = Sky_kyoukai;
        Skybox = RenderSettings.skybox;
    }
    public void SetSkyZinzya()
    {
        RenderSettings.skybox= Sky_zinzya;
        Skybox = RenderSettings.skybox;
    }
}
