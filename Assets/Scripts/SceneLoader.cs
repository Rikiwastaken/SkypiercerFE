using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{

    public static SceneLoader instance;

    private string SceneToToad;
    public GameObject loadingCanvas;
    private int framestoloading;
    private bool fondu;
    public TextMeshProUGUI loadingtext;

    public Image LoadingImage;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
        {
            if(SceneToToad!="")
            {
                framestoloading = 5;
                Debug.Log("loading : " + SceneToToad);
                SceneManager.LoadScene(SceneToToad);
                GetComponent<CombatSceneLoader>().MainSceneName = SceneToToad;
                GetComponent<CombatSceneLoader>().LoadCombatScene();
                MusicManager.instance.InitializeMusics(SceneToToad);
                SceneToToad = "";
                LoadingImage.gameObject.SetActive(true);
                Color newcolor = LoadingImage.color;
                newcolor.a = 1;
                LoadingImage.color = newcolor;
                loadingtext.gameObject.SetActive(true);
                
            }
        }
        else
        {
            if(framestoloading >5)
            {
                framestoloading--;
            }
            else
            {
                fondu = true;
            }
        }

        if(fondu)
        {
            LoadingImage.gameObject.SetActive(false);
            Color newcolor = LoadingImage.color;
            newcolor.a -= Time.fixedDeltaTime;
            LoadingImage.color = newcolor;
            if(LoadingImage.color.a <= 0)
            {
                LoadingImage.gameObject.SetActive(false);
                loadingtext.gameObject.SetActive(false);
                fondu = false;
            }
        }
    }

    private void UnlockCharactersWhenLoading(int chapter)
    {
        switch(chapter)
        {
            case 4:
                //Unlocking Ruben for Chapter4;
                foreach(UnitScript.Character character in DataScript.instance.PlayableCharacterList)
                {
                    if(character.ID == 7)
                    {
                        character.playableStats.unlocked = true;
                    }
                }
                break;
        }
        

    }
    public void LoadScene(string sceneName)
    {

        if(sceneName.ToLower().Contains("chapter"))
        {
            int chapternumber = int.Parse(sceneName.ToLower().Replace("chapter", ""));

            UnlockCharactersWhenLoading(chapternumber);
        }
        SceneToToad = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}