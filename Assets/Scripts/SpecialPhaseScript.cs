using System.Collections.Generic;
using UnityEngine;
using static MapEventManager;

public class SpecialPhaseScript : MonoBehaviour
{

    public CharacterCircleVisuals _CharacterCircleVisuals;
    public ForesightScript _ForesightScript;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TriggerSpecialPhase()
    {
        switch (GetComponent<MapInitializer>().ChapterID)
        {
            case 12:

                //First, create a new event which is the victory condition and an event which plays the outrop cutscene. All other events are removed.


                EventCondition OutroEvent = new EventCondition();
                OutroEvent.triggertype = 4;
                OutroEvent.triggerEffectType = 8;
                OutroEvent.CutsceneID = 6;
                OutroEvent.ID = 0;


                EventCondition victoryEvent = new EventCondition();
                victoryEvent.triggertype = 8;
                victoryEvent.triggerEffectType = 1;
                victoryEvent.ID = 1;
                victoryEvent.EventsToWatch = new List<int>() { 0 };

                GetComponent<MapEventManager>().EventsToMonitor = new List<EventCondition>() { OutroEvent, victoryEvent };


                // Then, we remove all characters that are not zack and raghnall and we activate zack's examode.

                GridScript _GridScript = GetComponent<GridScript>();

                GameObject Zack = null;

                foreach (GameObject unit in _GridScript.allunitGOs)
                {
                    string name = unit.GetComponent<UnitScript>().UnitCharacteristics.name.ToLower();
                    if (name == "zack")
                    {
                        unit.GetComponent<UnitScript>().UnitCharacteristics.ExamodeClass.ExamodePoints = 100;
                        unit.GetComponent<UnitScript>().ActivateExamode(true);
                        unit.GetComponent<UnitScript>().UnitCharacteristics.ExamodeClass.remaingExamodeTurns = 999;
                        Zack = unit;
                        continue;
                    }
                    else if (name == "raghnall")
                    {
                        continue;
                    }
                    unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile.UpdateInsideSprite(false);
                    unit.SetActive(false);

                }

                // we make sure raghnall is alive and well and activated
                Transform unitholder = GameObject.Find("Characters").transform;

                UnitScript.Character raghnallCharacter = null;

                for (int i = 0; i < unitholder.childCount; i++)
                {
                    GameObject unit = unitholder.GetChild(i).gameObject;

                    UnitScript.Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;


                    if (unitchar != null && unitchar.name.ToLower() == "raghnall")
                    {
                        unitchar.currentHP = (int)unitchar.AjustedStats.HP;
                        unitchar.enemyStats.RemainingLifebars = 1;
                        unitchar.alreadyplayed = true;
                        unitchar.alreadymoved = true;
                        unit.SetActive(true);
                        raghnallCharacter = unitchar;
                    }
                }

                _GridScript.InitializeGOList();

                // then we set the victory event's unit to ragnhall;

                OutroEvent.UnitList = new List<UnitScript.Character> { raghnallCharacter };


                // then we place zack close to Raghnall

                Zack.GetComponent<UnitScript>().MoveTo(raghnallCharacter.currentTile.GridCoordinates + new Vector2(0, -2), false, true);

                _CharacterCircleVisuals.UpdateFilling();

                // Then we remove previous Foresight Events

                _ForesightScript.actions.Clear();

                break;
        }
    }

}
