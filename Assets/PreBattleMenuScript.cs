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
    private GameObject selectedunit;
    private DataScript DataScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = FindAnyObjectByType<GridScript>();
        TurnManager = FindAnyObjectByType<TurnManger>();
        InputManager = FindAnyObjectByType<InputManager>();
        DataScript = FindAnyObjectByType<DataScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(TurnManager.currentlyplaying!="")
        {
            gameObject.SetActive(false);
            return;
        }

        if(InputManager.canceljustpressed && ChangingUnitPlace)
        {
            ChangingUnitPlace = false;
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(true);
        }

        if(ReplaceUnitButton.activeSelf)
        {
            GridScript.movementbuffercounter = 3;
            GameObject currentselected = EventSystem.current.currentSelectedGameObject;
            bool buttonselected = false;
            if (currentselected != null)
            {
                for (int i = 1; i < transform.childCount; i++)
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
                EventSystem.current.SetSelectedGameObject(transform.GetChild(1).gameObject);
            }
        }

        if(ChangingUnitPlace)
        {
            if(GridScript.GetUnit(GridScript.selection)!=null)
            {
                GameObject gridselection = GridScript.GetUnit(GridScript.selection);
                if(gridselection.GetComponent<UnitScript>().UnitCharacteristics.affiliation=="playable" && InputManager.activatejustpressed)
                {
                    if(selectedunit==null)
                    {
                        selectedunit = gridselection;
                    }
                    else
                    {
                        SwitchUnits(selectedunit , gridselection);
                        selectedunit = null;
                    }
                }
            }
        }

    }
    
    private void SwitchUnits( GameObject unit1, GameObject unit2)
    {
        Vector2 unit1pos = unit1.GetComponent<UnitScript>().UnitCharacteristics.position;
        unit1.GetComponent<UnitScript>().UnitCharacteristics.position = unit2.GetComponent<UnitScript>().UnitCharacteristics.position;
        unit2.GetComponent<UnitScript>().UnitCharacteristics.position = unit1pos;
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
