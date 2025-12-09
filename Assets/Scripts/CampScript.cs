using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
        EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);

        foreach(StartDialogue startdialogue in StartDialogueList)
        {
            if(startdialogue.Chapter== SaveManager.instance.currentchapter)
            {
                TextBubbleScript.Instance.InitializeDialogue(startdialogue.Dialogue);
                break;
            }
        }
        
    }

    private void Update()
    {
        if(EventSystem.current.currentSelectedGameObject==null || !EventSystem.current.currentSelectedGameObject.activeSelf || !EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
        }
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
