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

    private bool previousindialogue;

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

        if(TextBubbleScript.Instance.indialogue && BaseMenu.gameObject.activeSelf)
        {
            BaseMenu.gameObject.SetActive(false);
        }

        if(previousindialogue && ! TextBubbleScript.Instance.indialogue)
        {
            if(!BaseMenu.gameObject.activeSelf)
            {
                BaseMenu.gameObject.SetActive(true);
            }
        }

        if(EventSystem.current.currentSelectedGameObject==null || !EventSystem.current.currentSelectedGameObject.activeSelf || !EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
        }


        previousindialogue =TextBubbleScript.Instance.indialogue;
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
