using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PreBattleMenuScript : MonoBehaviour
{

    public static PreBattleMenuScript instance;

    public GameObject ReplaceUnitButton;
    private GridScript GridScript;
    private TurnManger TurnManager;
    public bool ChangingUnitPlace;
    public GameObject selectedunit;
    private MapInitializer MapInitializer;

    private InputAction _CancelAction;
    private InputAction _ActivateAction;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (TurnManager.currentlyplaying != "")
        {
            gameObject.SetActive(false);
            return;
        }

        if (_CancelAction.WasPressedThisFrame() && ChangingUnitPlace)
        {
            ChangingUnitPlace = false;
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(4).gameObject.SetActive(true);
            transform.GetChild(5).gameObject.SetActive(true);
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

    public void BeginBattle()
    {
        InitializeVariables();
        TurnManager.waittingforstart = false;
        gameObject.SetActive(false);
        GridScript.InitializeGOList();
    }

    private void InitializeVariables()
    {
        GridScript = GridScript.instance;
        TurnManager = FindAnyObjectByType<TurnManger>();
        MapInitializer = FindAnyObjectByType<MapInitializer>();
    }

}
