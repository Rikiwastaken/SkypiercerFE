using UnityEngine;
using UnityEngine.SceneManagement;

public class HideoutScript : MonoBehaviour
{

    SaveManager saveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadNextChapter()
    {
        if(saveManager == null)
        {
            saveManager = SaveManager.instance;
        }

        SceneLoader.instance.LoadScene("Chapter" + saveManager.currentchapter);

    }

}
