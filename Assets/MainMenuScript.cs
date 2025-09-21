using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{

    private SaveManager saveManager;
    private SceneLoader sceneLoader;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();
        saveManager.LoadCurrentSave();
        transform.GetChild(0).GetComponent<Button>().Select();
        sceneLoader = saveManager.GetComponent<SceneLoader>();
    }

    private void FixedUpdate()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if(currentSelected!= transform.GetChild(0).gameObject && currentSelected != transform.GetChild(1).gameObject && currentSelected != transform.GetChild(2).gameObject)
        {
            EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
        }
    }

    public void LoadTestMap()
    {
        sceneLoader.LoadScene("TestMap");
    }

    public void LoadPrologue()
    {
        sceneLoader.LoadScene("Prologue");
    }

    public void ResetSave()
    {
        FindAnyObjectByType<DataScript>().RestoreBaseCharacterValues();
    }

}
