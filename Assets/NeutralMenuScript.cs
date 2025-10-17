using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static UnitScript;

public class NeutralMenuScript : MonoBehaviour
{

    public Transform RedoMenuTransfrom;
    public Transform OptionsMenuTransfrom;

    public TurnManger TurnManager;

    public TextMeshProUGUI ForeSightButtonText;

    private SaveManager SaveManager;

    private int chapter;

    public int chapterforUnlockingForesight;

    // Update is called once per frame

    private void Start()
    {
        if (SaveManager == null)
        {
            SaveManager = FindAnyObjectByType<SaveManager>();
        }

        if(SceneManager.GetActiveScene().name.Contains("Chapter"))
        {
            chapter = int.Parse(SceneManager.GetActiveScene().name.Replace("Chapter", ""));
        }
        else
        {
            chapter = 0;
        }

        if (chapter < chapterforUnlockingForesight)
        {
            ForeSightButtonText.text = "Locked";
        }
    }

    void FixedUpdate()
    {
        ManageSelection();
        
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    public void EndTurn()
    {
        foreach(Character character in TurnManager.playableunit)
        {
            character.alreadymoved = true;
            character.alreadyplayed = true;
        }
        CloseMenu();
    }

    public void OpenForesighMenu()
    {
        if (chapter < chapterforUnlockingForesight)
        {
            return;
        }
        else
        {
            RedoMenuTransfrom.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(RedoMenuTransfrom.GetChild(0).gameObject);
        }
    }

    private void ManageSelection()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (RedoMenuTransfrom.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(RedoMenuTransfrom.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != RedoMenuTransfrom)
            {
                EventSystem.current.SetSelectedGameObject(RedoMenuTransfrom.GetChild(0).gameObject);
            }
        }
        else if (OptionsMenuTransfrom.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(OptionsMenuTransfrom.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != OptionsMenuTransfrom && currentSelected.transform.parent.parent != OptionsMenuTransfrom)
            {
                EventSystem.current.SetSelectedGameObject(OptionsMenuTransfrom.GetChild(0).gameObject);
            }
        }
        else
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
            }
        }
    }

}
