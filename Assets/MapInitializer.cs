using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static UnitScript;

public class MapInitializer : MonoBehaviour
{

    public int numberofplayables;

    public List<Vector2> playablepos;

    private DataScript DataScript;

    public GameObject BaseCharacter;

    public GameObject Characters;

    public List<EnemyStats> EnemyList;

    private GridScript GridScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InitializePlayers();
        InitialNonPlayers();
    }

    public void InitializePlayers()
    {
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
            GridScript.InstantiateGrid();
        }
        if (DataScript == null)
        {
            DataScript = FindAnyObjectByType<DataScript>();
        }



        foreach (Transform potentialplayable in Characters.transform)
        {
            if (potentialplayable.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
            {
                Destroy(potentialplayable.gameObject);
            }
        }

        int index = 0;
        foreach (Character playable in DataScript.PlayableCharacterList)
        {
            if (index < playablepos.Count && playable.deployunit)
            {
                GameObject newcharacter = Instantiate(BaseCharacter);
                newcharacter.GetComponent<UnitScript>().UnitCharacteristics = playable;
                newcharacter.transform.parent = Characters.transform;
                newcharacter.transform.position = new Vector3(playablepos[index].x, 0, playablepos[index].y);
                newcharacter.GetComponent<UnitScript>().MoveTo(playablepos[index]);
                newcharacter.name = playable.name;
                index++;
            }

        }
        GridScript.InitializeGOList();
    }


    // creating all gameobjects for nonplayer 
    private void InitialNonPlayers()
    {
        if (DataScript == null)
        {
            DataScript = FindAnyObjectByType<DataScript>();
        }
        int index = 0;
        foreach (EnemyStats enemyStats in EnemyList)
        {
            GameObject newcharacter = Instantiate(BaseCharacter);
            newcharacter.GetComponent<UnitScript>().enemyStats = enemyStats;
            Character Character = newcharacter.GetComponent<UnitScript>().UnitCharacteristics;
            Character.name = enemyStats.Name;
            if (enemyStats.Skills.Count > 0)
            {
                Character.UnitSkill = enemyStats.Skills[0];
                Character.EquipedSkills = new List<int>();
                for (int i = 1; i < Mathf.Min(enemyStats.Skills.Count, 5); i++)
                {
                    Character.EquipedSkills.Add(enemyStats.Skills[i]);
                }
            }
            Character.equipmentsIDs = enemyStats.equipments;
            newcharacter.transform.parent = Characters.transform;
            newcharacter.transform.position = new Vector3(enemyStats.startpos.x, 0, enemyStats.startpos.y);
            newcharacter.GetComponent<UnitScript>().MoveTo(enemyStats.startpos);
            newcharacter.name = enemyStats.Name + " " + index;
            if (enemyStats.isother)
            {
                Character.affiliation = "other";
            }
            else
            {
                Character.affiliation = "enemy";
            }
            index++;
        }
    }

}
