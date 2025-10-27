using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    private string SceneToToad;
    public GameObject loadingCanvas;
    private int framestoloading;
    private bool fondu;
    public TextMeshProUGUI loadingtext;

    public Image LoadingImage;

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

    public void LoadScene(string sceneName)
    {
        SceneToToad = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}
