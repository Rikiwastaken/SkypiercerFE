using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour
{
    public static GameOverScript instance;

    public bool victory;
    public bool InHideout;

    private SceneLoader sceneLoader;
    private SaveManager saveManager;

    public Transform GameOverMenu;
    public Transform VictoryMenu;
    public Transform SaveMenu;
    public Transform ConfirmMenu;

    private int chosenSlot;


    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnEnable()
    {
        sceneLoader = SceneLoader.instance;

        if (!InHideout)
        {

            VictoryMenu.gameObject.SetActive(victory);
            GameOverMenu.gameObject.SetActive(!victory);

            StartCoroutine(InitializeSelection());
        }
    }

    private void FixedUpdate()
    {
        GameObject buttonToSelect = EventSystem.current.currentSelectedGameObject;
        if (buttonToSelect==null)
        {
            if (GameOverMenu.gameObject.activeSelf)
                buttonToSelect = GameOverMenu.GetChild(1).gameObject;
            else if (VictoryMenu.gameObject.activeSelf)
                buttonToSelect = VictoryMenu.GetChild(1).gameObject;
            else if (SaveMenu.gameObject.activeSelf)
                buttonToSelect = SaveMenu.GetChild(0).gameObject;

            EventSystem.current.SetSelectedGameObject(buttonToSelect);
        }
    }

    private IEnumerator InitializeSelection()
    {
        yield return null; // wait one frame so UI activates

        GameObject buttonToSelect = null;

        if (GameOverMenu.gameObject.activeSelf)
            buttonToSelect = GameOverMenu.GetChild(1).gameObject;
        else if (VictoryMenu.gameObject.activeSelf)
            buttonToSelect = VictoryMenu.GetChild(1).gameObject;
        else if (SaveMenu.gameObject.activeSelf)
            buttonToSelect = SaveMenu.GetChild(0).gameObject;

        if (buttonToSelect == null)
            yield break;

        Button btn = buttonToSelect.GetComponent<Button>();
        if (btn == null || !btn.interactable)
            yield break;

        // wait one more frame to ensure EventSystem is ready
        yield return new WaitForEndOfFrame();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }

    public void InitializeSaveButtons()
    {
        if (saveManager == null)
            saveManager = FindAnyObjectByType<SaveManager>();

        List<Button> buttons = new List<Button>();
        for (int i = 0; i < SaveMenu.childCount - 1; i++)
            buttons.Add(SaveMenu.GetChild(i).GetComponent<Button>());

        saveManager.InitializeSaveButtons(buttons);
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
        ConfirmMenu.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            $"Overwrite Slot {slot + 1} ?";
    }

    public void Continue()
    {
        if (saveManager == null)
            saveManager = SaveManager.instance;

        int nextsceneindex = FindAnyObjectByType<MapInitializer>().ChapterID;
        saveManager.currentchapter = nextsceneindex + 1;

        sceneLoader.LoadScene("Hideout");
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
