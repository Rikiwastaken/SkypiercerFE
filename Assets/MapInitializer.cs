using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnitScript;

public class MapInitializer : MonoBehaviour
{

    public int numberofplayables;

    public List<Vector2> playablepos;

    private DataScript DataScript;

    public GameObject BaseCharacter;

    public GameObject Characters;

    public List<EnemyStats> EnemyList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InitializePlayers();
        InitialNonPlayers();
    }

    private void InitializePlayers()
    {
        if(DataScript == null)
        {
            DataScript = FindAnyObjectByType<DataScript>();
        }


        int index = 0;
        foreach(Character playable in DataScript.PlayableCharacterList)
        {
            GameObject newcharacter = Instantiate(BaseCharacter);
            newcharacter.GetComponent<UnitScript>().UnitCharacteristics = playable; 
            playable.position = playablepos[index];
            newcharacter.transform.parent = Characters.transform;
            newcharacter.transform.position = new Vector3(playablepos[index].x,0, playablepos[index].y);
            index++;
        }
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
            Character.position = enemyStats.startpos;
            Character.name = enemyStats.Name;
            if (enemyStats.Skills.Count>0)
            {
                Character.UnitSkill = enemyStats.Skills[0];
                Character.EquipedSkills = new List<int>();
                for (int i = 1; i< Mathf.Min(enemyStats.Skills.Count,5);i++)
                {
                    Character.EquipedSkills.Add(enemyStats.Skills[i]);
                }
            }
            Character.equipmentsIDs = enemyStats.equipments;
            newcharacter.transform.parent = Characters.transform;
            newcharacter.transform.position = new Vector3(enemyStats.startpos.x, 0, enemyStats.startpos.y);
            newcharacter.name = enemyStats.Name + " "+index;
            if(enemyStats.isother)
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
