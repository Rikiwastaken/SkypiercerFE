using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScript : MonoBehaviour
{
    public bool victory;

    private SceneLoader sceneLoader;

    public Transform GameOverMenu;
    public Transform VictoryMenu;
    public Transform SaveMenu;
    public Transform ConfirmMenu;

    private SaveManager saveManager;

    private int chosenSlot;

    private void OnEnable()
    {
        Time.timeScale = 0f;
        if (victory)
        {
            VictoryMenu.gameObject.SetActive(true);
            GameOverMenu.gameObject.SetActive(false);
        }
        else
        {
            VictoryMenu.gameObject.SetActive(false);
            GameOverMenu.gameObject.SetActive(true);
        }
        
    }

    public void InitializeSaveButtons()
    {
        if(saveManager == null)
        {
            saveManager = FindAnyObjectByType<SaveManager>();
        }
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < SaveMenu.childCount - 1; i++)
        {
            buttons.Add(SaveMenu.GetChild(i).GetComponent<Button>());
        }
        saveManager.InitializeSaveButtons(buttons);
    }

    private void Update()
    {
        ManageSelection();
    }

    public void ConfirmSave()
    {
        saveManager.activeSlot = chosenSlot;
        saveManager.SaveCurrentSlot();
        InitializeSaveButtons();
    }

    public void ChooseSaveFile(int slot)
    {
        chosenSlot = slot;
        ConfirmMenu.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Overwrite Slot " + (slot + 1) + " ?";
    }

    private void ManageSelection()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (GameOverMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(GameOverMenu.GetChild(1).gameObject);
            }
            else if (currentSelected.transform.parent != GameOverMenu)
            {
                EventSystem.current.SetSelectedGameObject(GameOverMenu.GetChild(1).gameObject);
            }
        }
        else if (VictoryMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(VictoryMenu.GetChild(1).gameObject);
            }
            else if (currentSelected.transform.parent != VictoryMenu)
            {
                EventSystem.current.SetSelectedGameObject(VictoryMenu.GetChild(1).gameObject);
            }
        }
        else if (SaveMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(SaveMenu.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != SaveMenu)
            {
                EventSystem.current.SetSelectedGameObject(SaveMenu.GetChild(0).gameObject);
            }
        }
    }

    public void Continue()
    {
        int nextsceneindex = FindAnyObjectByType<MapInitializer>().ChapterID;
        string scenetoload = "Chapter" + nextsceneindex+1;

        sceneLoader.LoadScene(scenetoload);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        sceneLoader.LoadScene("MainMenu");
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        sceneLoader.LoadScene(SceneManager.GetActiveScene().name);
    }

}
