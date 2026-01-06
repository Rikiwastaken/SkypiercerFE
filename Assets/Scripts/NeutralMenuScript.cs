using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class NeutralMenuScript : MonoBehaviour
{

    public Transform RedoMenuTransfrom;
    public Transform OptionsMenuTransfrom;

    public TurnManger TurnManager;

    public TextMeshProUGUI ForeSightButtonText;

    private SaveManager SaveManager;

    private int chapter;

    public int chapterforUnlockingForesight;

    private InputManager InputManager;

    // Update is called once per frame

    private void Start()
    {
        if (SaveManager == null)
        {
            SaveManager = FindAnyObjectByType<SaveManager>();
        }

        InputManager = InputManager.instance;

        if (SceneManager.GetActiveScene().name.Contains("Chapter"))
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

    void Update()
    {

        ManageSelection();

        if (InputManager.cancelpressed || (TextBubbleScript.Instance != null && TextBubbleScript.Instance.indialogue))
        {
            if (OptionsMenuTransfrom.gameObject.activeSelf)
            {
                OptionsMenuTransfrom.gameObject.SetActive(false);
            }
            else
            {
                CloseMenu();
            }
        }

    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    public void EndTurn()
    {
        foreach (GameObject character in TurnManager.playableunitGO)
        {
            if (!character.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed && !character.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved)
                ActionManager.instance.Wait(character);
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
            gameObject.SetActive(false);
        }
    }

    private void ManageSelection()
    {
        if (GameOverScript.instance != null && GameOverScript.instance.gameObject.activeSelf)
        {
            return;
        }
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
