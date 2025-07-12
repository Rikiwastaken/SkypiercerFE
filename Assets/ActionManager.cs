using UnityEngine;
using static UnitScript;

public class ActionManager : MonoBehaviour
{

    private TurnManger TurnManager;

    private InputManager InputManager;

    private GridScript GridScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnManager = GetComponent<TurnManger>();
        InputManager = FindAnyObjectByType<InputManager>();
        GridScript = GetComponent<GridScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(TurnManager.currentlyplaying=="playable")
        {
            Character currentcharacter = GridScript.GetSelectedUnit();
            if(currentcharacter!=null)
            {
                if(currentcharacter.affiliation=="playable" && InputManager.activatejustpressed)
                {
                    GridScript.lockselection = true;
                    GridScript.LockcurrentSelection();
                    GridScript.Recolor();
                }
            }
        }
    }
}
