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
    // Update is called once per frame
    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
        {
            framestoloading = 5;
            SceneManager.LoadScene(SceneToToad);
            SceneToToad = "";
            loadingCanvas.SetActive(true);
            Color newcolor = loadingCanvas.transform.GetChild(0).GetComponent<Image>().color;
            newcolor.a = 1;
            loadingCanvas.transform.GetChild(0).GetComponent<Image>().color = newcolor;
            loadingtext.gameObject.SetActive(true);
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
            loadingtext.gameObject.SetActive(false);
            Color newcolor = loadingCanvas.transform.GetChild(0).GetComponent<Image>().color;
            newcolor.a -= Time.fixedDeltaTime;
            loadingCanvas.transform.GetChild(0).GetComponent<Image>().color = newcolor;
            if(loadingCanvas.transform.GetChild(0).GetComponent<Image>().color.a <= 0)
            {
                loadingCanvas.SetActive(false);
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
