using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private string SceneToToad;
    public GameObject loadingCanvas;
    private int framestoloading;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
        {
            framestoloading = 5;
            SceneManager.LoadScene(SceneToToad);
            SceneToToad = "";
            loadingCanvas.SetActive(true);
        }
        else
        {
            if(framestoloading >5)
            {
                framestoloading--;
            }
            else
            {
                loadingCanvas.SetActive(false);
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneToToad = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}
