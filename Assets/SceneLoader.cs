using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private string SceneToToad;

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "LoadingScene")
        {
            SceneManager.LoadScene(SceneToToad);
            SceneToToad = "";
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneToToad = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}
