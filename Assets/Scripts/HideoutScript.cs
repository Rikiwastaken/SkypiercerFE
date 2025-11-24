using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class HideoutScript : MonoBehaviour
{

    SaveManager saveManager;

    public Transform BaseMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
        EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
    }

    public void LoadNextChapter()
    {
        if (saveManager == null)
        {
            saveManager = SaveManager.instance;
        }

        SceneLoader.instance.LoadScene("Chapter" + saveManager.currentchapter);

    }

}
