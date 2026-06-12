using System.Collections.Generic;
using UnityEngine;
using static MapEventManager;

public class SpecialPhaseScript : MonoBehaviour
{
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

                //First, create a new event which is the victory condition. All other events are removed.

                EventCondition victoryEvent = new EventCondition();
                victoryEvent.triggertype = 4;
                victoryEvent.triggerEffectType = 1;

                GetComponent<MapEventManager>().EventsToMonitor = new List<EventCondition>() { victoryEvent };


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

                victoryEvent.UnitList = new List<UnitScript.Character> { raghnallCharacter };


                // then we place zack close to Raghnall

                Zack.GetComponent<UnitScript>().MoveTo(raghnallCharacter.currentTile.GridCoordinates + new Vector2(0, -2), false, true);


                break;
        }
    }

}
