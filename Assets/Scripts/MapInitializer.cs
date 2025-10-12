using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using static UnitScript;

public class MapInitializer : MonoBehaviour
{

    public int numberofplayables;

    public List<Vector2> playablepos;

    public int ChapterID;

    private DataScript DataScript;

    public GameObject BaseCharacter;

    public GameObject Characters;

    public List<EnemyStats> EnemyList;

    private GridScript GridScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EmptyPlayables();
        InitializePlayers();
        InitialNonPlayers();
    }

    private void EmptyPlayables()
    {
        if (DataScript == null)
        {
            DataScript = FindAnyObjectByType<DataScript>();
        }
        for (int i = 0; i < DataScript.PlayableCharacterList.Count; i++)
        {
            DataScript.PlayableCharacterList[i].playableStats.deployunit = false;
        }
        for (int i = 0; i < Mathf.Min(playablepos.Count, DataScript.PlayableCharacterList.Count); i++)
        {
            if(DataScript.PlayableCharacterList[i].playableStats.unlocked || SceneManager.GetActiveScene().name=="TestMap")
            {
                DataScript.PlayableCharacterList[i].playableStats.deployunit = true;
            }
            
        }
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
        if(Characters == null)
        {
            Characters = GameObject.Find("Characters");
        }

        numberofplayables = playablepos.Count;
        foreach (Transform potentialplayable in Characters.transform)
        {
            if (potentialplayable.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
            {
                string name = potentialplayable.name;
                Destroy(potentialplayable.gameObject);
            }
        }

        int index = 0;
        bool intestmap = SceneManager.GetActiveScene().name == "TestMap";
        foreach (Character playable in DataScript.PlayableCharacterList)
        {

            if (index < playablepos.Count && (playable.playableStats.deployunit || (intestmap || playable.playableStats.unlocked)))
            {
                GameObject newcharacter = Instantiate(BaseCharacter);
                newcharacter.GetComponent<UnitScript>().UnitCharacteristics = playable;
                ManageModel(newcharacter);
                newcharacter.GetComponent<UnitScript>().calculateStats();
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
            Character Character = newcharacter.GetComponent<UnitScript>().UnitCharacteristics;
            Character.enemyStats = enemyStats;
            Character.modelID = enemyStats.modelID;
            ManageModel(newcharacter);
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
    private void ManageModel(GameObject character)
    {
        foreach (ModelInfo modelinf in character.GetComponent<UnitScript>().ModelList)
        {
            if(modelinf.ID == character.GetComponent<UnitScript>().UnitCharacteristics.modelID)
            {
                modelinf.active = true;
                modelinf.wholeModel.SetActive(true);
            }
            else
            {
                Debug.Log("destroying");
                modelinf.active = false;
                DestroyImmediate(modelinf.wholeModel);
            }
        }
    }
}
