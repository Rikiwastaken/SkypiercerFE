using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{

    private SaveManager saveManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();
        saveManager.LoadCurrentSave();
        transform.GetChild(0).GetComponent<Button>().Select();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadTestMap()
    {
        SceneManager.LoadScene("TestMap");
    }

    public void LoadPrologue()
    {
        SceneManager.LoadScene("Prologue");
    }

    public void ResetSave()
    {
        FindAnyObjectByType<DataScript>().RestoreBaseCharacterValues();
    }

}
