using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SaveManager;

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
            else if(currentSelected.transform.parent!= BaseMenu)
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

    }

    public void InitializeSaveButtons()
    {
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < ContinueMenu.childCount-1; i++)
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

    public void QuitApp()
    {
        Application.Quit();
    }

    private string GetChapterScene(int chapter)
    {
        switch (chapter)
        {
            case 0:
                return "Prologue";
        }
        return "Prologue";
    }

    public void LoadSave(int slot)
    {
        
        if (saveManager.SaveClasses[slot] != null)
        {
            saveManager.activeSlot = slot;
            saveManager.ApplySave(slot);
            sceneLoader.LoadScene(GetChapterScene(saveManager.SaveClasses[slot].chapter));
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
        FindAnyObjectByType<DataScript>().RestoreBaseCharacterValues();
    }

}
