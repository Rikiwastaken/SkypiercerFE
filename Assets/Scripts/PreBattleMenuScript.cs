using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static SaveManager;

public class PreBattleMenuScript : MonoBehaviour
{

    public static PreBattleMenuScript instance;

    [SerializeField] private GameObject ReplaceUnitButton;
    private GridScript GridScript;
    private TurnManger TurnManager;
    public bool ChangingUnitPlace;
    public GameObject selectedunit;
    private MapInitializer MapInitializer;

    private InputAction _CancelAction;
    private InputAction _ActivateAction;


    private SaveClass SaveBeforeBattle;

    [SerializeField] private GameObject BattallionChangeMenu;
    [SerializeField] private Button BattallionChangeButton;
    private bool BattallionAvailable;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeVariables();
        _CancelAction = InputSystem.actions.FindAction("Cancel");
        _ActivateAction = InputSystem.actions.FindAction("Validate");
        CheckIfBattallionMenuIsAvailable();
    }

    // Update is called once per frame
    void Update()
    {
        if (TurnManager.currentlyplaying != "")
        {
            gameObject.SetActive(false);
            return;
        }


        if (BattallionChangeMenu.activeSelf)
        {
            return;
        }

        if (_CancelAction.WasPressedThisFrame() && ChangingUnitPlace)
        {
            ChangingUnitPlace = false;
            for (int i = 2; i < transform.childCount; i++)
            {
                transform.GetChild(2).gameObject.SetActive(true);
                transform.GetChild(3).gameObject.SetActive(true);
                transform.GetChild(4).gameObject.SetActive(true);
                transform.GetChild(5).gameObject.SetActive(true);
                transform.GetChild(6).gameObject.SetActive(true);
            }

        }

        if (ReplaceUnitButton.activeSelf)
        {
            GridScript.movementbuffercounter = 3;
            GameObject currentselected = EventSystem.current.currentSelectedGameObject;
            bool buttonselected = false;
            if (currentselected != null)
            {
                for (int i = 2; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).gameObject == currentselected)
                    {

                        buttonselected = true;
                        break;

                    }
                }
            }
            if (!(GameOverScript.instance != null && GameOverScript.instance.gameObject.activeSelf) && (!buttonselected || currentselected == null))
            {

                EventSystem.current.SetSelectedGameObject(transform.GetChild(2).gameObject);
            }
        }

        if (ChangingUnitPlace)
        {
            if (GridScript.GetUnit(GridScript.selection) != null)
            {
                GameObject gridselection = GridScript.GetUnit(GridScript.selection);
                if (gridselection.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && _ActivateAction.WasPressedThisFrame())
                {
                    if (selectedunit == null)
                    {
                        selectedunit = gridselection;
                    }
                    else
                    {
                        SwitchUnits(selectedunit, gridselection);
                        selectedunit = null;
                    }
                }
            }
            else
            {
                if (selectedunit != null && _ActivateAction.WasPressedThisFrame() && MapInitializer.playablepos.Contains(GridScript.selection.GridCoordinates))
                {
                    selectedunit.GetComponent<UnitScript>().MoveTo(GridScript.selection.GridCoordinates);
                }
            }
        }

    }

    private void SwitchUnits(GameObject unit1, GameObject unit2)
    {
        MapInitializer.ExchangePlaces(unit1, unit2);
    }

    public void ActivatePlacement()
    {
        ChangingUnitPlace = true;
    }

    private void CheckIfBattallionMenuIsAvailable()
    {
        BattallionAvailable = false;
        if (DataScript.instance.PlayableCharacterList[9].playableStats.unlocked)
        {
            BattallionAvailable = true;
        }

        if (!BattallionAvailable)
        {
            BattallionChangeButton.GetComponent<Image>().color = Color.gray;
        }
    }

    public void OpenBattallionMenu()
    {
        if (BattallionAvailable)
        {
            BattallionChangeMenu.SetActive(true);
            BattallionChangeMenu.GetComponent<BattallionChoiceScript>().Initialization();
        }
    }

    public void BeginBattle()
    {
        InitializeVariables();
        TurnManager.waittingforstart = false;
        gameObject.SetActive(false);
        GridScript.InitializeGOList();
        List<int> newpreviouslydeployed = new List<int>();
        foreach (GameObject playableGO in GridScript.allunitGOs)
        {
            UnitScript.Character playable = playableGO.GetComponent<UnitScript>().UnitCharacteristics;
            if (playable.affiliation.ToLower() == "playable")
            {
                newpreviouslydeployed.Add(playable.ID);
            }

        }
        SaveManager.instance.PreviouslyDeployed = newpreviouslydeployed;
        SaveBeforeBattle = SaveManager.instance.GenerateSaveClassFromGameState(false, false);
    }

    public void ReturnToCamp(bool ApplySave)
    {
        if (ApplySave)
        {
            SaveManager.instance.ApplySaveFromSaveClass(SaveBeforeBattle);
        }

        SceneLoader.instance.LoadScene("Camp");
    }

    private void InitializeVariables()
    {
        GridScript = GridScript.instance;
        TurnManager = FindAnyObjectByType<TurnManger>();
        MapInitializer = FindAnyObjectByType<MapInitializer>();
    }

}
