using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{

    private SaveManager saveManager;
    private SceneLoader sceneLoader;

    public Transform OptionsMenu;
    public Button OptionsMenuButton;
    public Transform BaseMenu;
    public Transform ContinueMenu;
    public Transform ManuallyLoadChapterMenu;
    public Button ContinueMenuButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
            return;
        }

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
            else if (currentSelected.transform.parent != BaseMenu)
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
            else if (currentSelected.transform.parent != ContinueMenu)
            {
                EventSystem.current.SetSelectedGameObject(ContinueMenu.GetChild(0).gameObject);
            }
        }
        else if (ManuallyLoadChapterMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(ManuallyLoadChapterMenu.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != ManuallyLoadChapterMenu)
            {
                EventSystem.current.SetSelectedGameObject(ManuallyLoadChapterMenu.GetChild(0).gameObject);
            }
        }
    }

    public void InitializeSaveButtons()
    {
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < ContinueMenu.childCount - 1; i++)
        {
            buttons.Add(ContinueMenu.GetChild(i).GetComponent<Button>());
        }
        saveManager.InitializeSaveButtons(buttons);
    }

    public void LoadTestMap()
    {
        saveManager.ApplySave(-1);
        sceneLoader.LoadScene("TestMap");
    }

    public void ManuallyLoadChapter(int Chapter)
    {
        DataScript.instance.SetupCharactersForChapter(Chapter);


        sceneLoader.LoadScene("Chapter" + Chapter);
    }

    public void OnCancel()
    {
        if (OptionsMenu.gameObject.activeSelf)
        {
            OptionsMenuButton.onClick.Invoke();
        }
        else if (ContinueMenu.gameObject.activeSelf)
        {
            ContinueMenuButton.onClick.Invoke();
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }

    private string GetChapterScene(int chapter)
    {
        if (chapter == 0)
        {
            return "Prologue";
        }
        else
        {
            return "Chapter" + chapter;
        }
    }

    public void LoadSave(int slot)
    {

        if (saveManager.SaveClasses[slot] != null)
        {
            saveManager.activeSlot = slot;
            saveManager.ApplySave(slot);
            if (saveManager.currentchapter == 1)
            {
                sceneLoader.LoadScene("Chapter1");
            }
            else
            {
                sceneLoader.LoadScene("Camp");
            }
        }
        else
        {
            saveManager.ApplySave(-1);
            saveManager.activeSlot = slot;
            sceneLoader.LoadScene("Prologue");
        }
    }

    public void LoadPrologue()
    {
        saveManager.ApplySave(-1);
        sceneLoader.LoadScene("Prologue");
    }

    public void ResetSave()
    {
        DataScript.instance.RestoreBaseCharacterValues();
    }

}
