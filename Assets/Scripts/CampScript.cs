using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TextBubbleScript;

public class CampScript : MonoBehaviour
{

    SaveManager saveManager;

    public Transform BaseMenu;

    [Serializable]
    public class StartDialogue
    {
        public int Chapter;
        public List<TextBubbleInfo> Dialogue;
    }

    public List<StartDialogue> StartDialogueList;

    private bool previousindialogue;

    public Transform SaveButtonList;

    public Animator SittingCharacter;
    public Animator Sitting2Character;
    public Animator Sitting3Character;
    public Animator LeaningCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        saveManager = SaveManager.instance;

        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
        EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);

        foreach (StartDialogue startdialogue in StartDialogueList)
        {
            if (startdialogue.Chapter == SaveManager.instance.currentchapter)
            {
                TextBubbleScript.Instance.InitializeDialogue(startdialogue.Dialogue);
                break;
            }
        }

        List<Button> buttons = new List<Button>();
        for (int i = 0; i < SaveButtonList.childCount - 1; i++)
        {
            buttons.Add(SaveButtonList.GetChild(i).GetComponent<Button>());
        }
        saveManager.InitializeSaveButtons(buttons);
        SittingCharacter.SetBool("Sitting", true);
        Sitting2Character.SetBool("Sitting2", true);
        Sitting3Character.SetBool("Sitting3", true);
        LeaningCharacter.SetBool("Leaning", true);
    }

    private void Update()
    {

        if (TextBubbleScript.Instance.indialogue && BaseMenu.gameObject.activeSelf)
        {
            BaseMenu.gameObject.SetActive(false);
        }

        if (previousindialogue && !TextBubbleScript.Instance.indialogue)
        {
            if (!BaseMenu.gameObject.activeSelf)
            {
                BaseMenu.gameObject.SetActive(true);
            }
        }

        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeSelf || !EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
        }


        previousindialogue = TextBubbleScript.Instance.indialogue;
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
