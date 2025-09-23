using UnityEngine;
using UnityEngine.EventSystems;
using static SaveManager;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{

    private SaveManager saveManager;
    private SceneLoader sceneLoader;
    public Transform BaseMenu;
    public Transform ContinueMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();
        saveManager.LoadSaves();
        BaseMenu.GetChild(0).GetComponent<Button>().Select();
        sceneLoader = saveManager.GetComponent<SceneLoader>();
    }

    private void FixedUpdate()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (BaseMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
            }
            else if(currentSelected.transform.parent!= BaseMenu.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
            }
        }
        else if (ContinueMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(ContinueMenu.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != ContinueMenu.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(ContinueMenu.GetChild(0).gameObject);
            }
        }

    }

    public void InitializeSaveButtons()
    {
        for(int i = 0; i < BaseMenu.childCount-1; i++)
        {
            if (saveManager.SaveClasses[i] != null)
            {
                SaveClass save = saveManager.SaveClasses[i];
                int numberofseconds = (int)save.secondselapsed;
                int numberofminutes = save.secondselapsed / 60;
            }
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
