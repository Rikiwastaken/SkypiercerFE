using UnityEngine;
using UnityEngine.EventSystems;
using static UnitScript;

public class PreBattleMenuScript : MonoBehaviour
{

    public GameObject ReplaceUnitButton;
    private GridScript GridScript;
    private TurnManger TurnManager;
    public bool ChangingUnitPlace;
    private InputManager InputManager;
    public GameObject selectedunit;
    private DataScript DataScript;
    private MapInitializer MapInitializer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = FindAnyObjectByType<GridScript>();
        TurnManager = FindAnyObjectByType<TurnManger>();
        InputManager = FindAnyObjectByType<InputManager>();
        DataScript = FindAnyObjectByType<DataScript>();
        MapInitializer = FindAnyObjectByType<MapInitializer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (TurnManager.currentlyplaying != "")
        {
            gameObject.SetActive(false);
            return;
        }

        if (InputManager.canceljustpressed && ChangingUnitPlace)
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
            if (!buttonselected || currentselected == null)
            {
                EventSystem.current.SetSelectedGameObject(transform.GetChild(2).gameObject);
            }
        }

        if (ChangingUnitPlace)
        {
            if (GridScript.GetUnit(GridScript.selection) != null)
            {
                GameObject gridselection = GridScript.GetUnit(GridScript.selection);
                if (gridselection.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && InputManager.activatejustpressed)
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
                if (selectedunit != null && InputManager.activatejustpressed && MapInitializer.playablepos.Contains(GridScript.selection.GridCoordinates))
                {
                    selectedunit.GetComponent<UnitScript>().MoveTo(GridScript.selection.GridCoordinates);
                }
            }
        }

    }

    private void SwitchUnits(GameObject unit1, GameObject unit2)
    {
        Vector2 unit1pos = new Vector2(unit1.GetComponent<UnitScript>().UnitCharacteristics.position.x, unit1.GetComponent<UnitScript>().UnitCharacteristics.position.y);
        Vector2 unit2pos = new Vector2(unit2.GetComponent<UnitScript>().UnitCharacteristics.position.x, unit2.GetComponent<UnitScript>().UnitCharacteristics.position.y);
        GridSquareScript temp = GridScript.GetFirstClosestTile(unit1pos);
        unit1.GetComponent<UnitScript>().MoveTo(temp.GridCoordinates);
        unit2.GetComponent<UnitScript>().MoveTo(unit1pos);
        unit1.GetComponent<UnitScript>().MoveTo(unit2pos);
    }

    public void ActivatePlacement()
    {
        ChangingUnitPlace = true;
    }

    public void BeginBattle()
    {
        TurnManager.waittingforstart = false;
        gameObject.SetActive(false);
        GridScript.InitializeGOList();
    }

}
