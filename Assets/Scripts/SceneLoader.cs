using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{

    public static SceneLoader instance;

    private string SceneToLoad;
    public GameObject loadingCanvas;
    private int framestoloading;
    private bool fondu;
    public TextMeshProUGUI loadingtext;

    public Image LoadingImage;

    public int Cutscenetoplay;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
        {
            if (SceneToLoad != "")
            {
                framestoloading = 5;
                SceneManager.LoadScene(SceneToLoad);
                int chapternumber = 0;
                if (SceneToLoad.ToLower().Contains("chapter"))
                {
                    chapternumber = int.Parse(SceneToLoad.ToLower().Replace("chapter", ""));
                }
                DataScript.instance.CharacterUnlockingSafeguard(chapternumber);
                if (SceneToLoad != "CutsceneScene")
                {
                    GetComponent<CombatSceneLoader>().combatLoaded = false;
                    GetComponent<CombatSceneLoader>().MainSceneName = SceneToLoad;
                    GetComponent<CombatSceneLoader>().LoadCombatScene();
                    GetComponent<CombatSceneLoader>().LoadCutsceneScene();
                }


                SceneToLoad = "";
                LoadingImage.gameObject.SetActive(true);
                Color newcolor = LoadingImage.color;
                newcolor.a = 1;
                LoadingImage.color = newcolor;
                loadingtext.gameObject.SetActive(true);

            }
        }
        else
        {
            if (framestoloading > 5)
            {
                framestoloading--;
            }
            else
            {
                fondu = true;
            }
        }

        if (fondu)
        {
            LoadingImage.gameObject.SetActive(false);
            Color newcolor = LoadingImage.color;
            newcolor.a -= Time.fixedDeltaTime;
            LoadingImage.color = newcolor;
            if (LoadingImage.color.a <= 0)
            {
                LoadingImage.gameObject.SetActive(false);
                loadingtext.gameObject.SetActive(false);
                fondu = false;
            }
        }
    }
    public void LoadScene(string sceneName, int cutscene = -1)
    {

        int chapternumber = SaveManager.instance.maxchapterreached;
        DataScript.instance.CharacterUnlockingSafeguard(chapternumber);
        SceneToLoad = sceneName;
        Cutscenetoplay = cutscene;
        SceneManager.LoadScene("LoadingScene");
    }
}