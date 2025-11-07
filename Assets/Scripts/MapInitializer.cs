using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnitScript;

public class MapInitializer : MonoBehaviour
{

    public static MapInitializer instance;

    public int numberofplayables;

    public List<Vector2> playablepos;

    public int ChapterID;

    public GameObject BaseCharacter;

    public GameObject Characters;

    public List<EnemyStats> EnemyList;

    private GridScript GridScript;

    private int previousid;

    private void Awake()
    {
        if(instance != null)
        {
            instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EmptyPlayables();
        InitializePlayers();
        InitializeNonPlayers();
    }

    private void EmptyPlayables()
    {
        for (int i = 0; i < DataScript.instance.PlayableCharacterList.Count; i++)
        {
            DataScript.instance.PlayableCharacterList[i].playableStats.deployunit = false;
        }
        for (int i = 0; i < Mathf.Min(playablepos.Count, DataScript.instance.PlayableCharacterList.Count); i++)
        {
            if (DataScript.instance.PlayableCharacterList[i].playableStats.unlocked || SceneManager.GetActiveScene().name == "TestMap")
            {
                DataScript.instance.PlayableCharacterList[i].playableStats.deployunit = true;
            }

        }
    }

    public void InitializePlayers()
    {
        if (GridScript == null)
        {
            GridScript = GridScript.instance;
            GridScript.InstantiateGrid();
        }
        if (Characters == null)
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
        foreach (Character playable in DataScript.instance.PlayableCharacterList)
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
    private void InitializeNonPlayers()
    {

        int index = 0;
        foreach (EnemyStats enemyStats in EnemyList)
        {
            InitializeNonPlayable(enemyStats, index);
            index++;
        }
    }

    public void InitializeNonPlayable(EnemyStats enemyStats, int index = -1)
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
        Character.ID = findfirstfreeid();
        newcharacter.transform.parent = Characters.transform;
        if(GridScript.instance.checkifvalidpos(enemyStats.startpos, newcharacter,0))
        {
            newcharacter.transform.position = new Vector3(enemyStats.startpos.x, 0, enemyStats.startpos.y);
            newcharacter.GetComponent<UnitScript>().previousposition = enemyStats.startpos;
            newcharacter.GetComponent<UnitScript>().MoveTo(enemyStats.startpos);
        }
        else
        {
            for (int i = 0;i<3;i++)
            {

                bool posfound = false;

                List<Vector2> newposlist = new List<Vector2>() { new Vector2(0, i), new Vector2(0, -i), new Vector2(i, 0), new Vector2(-i, 0), new Vector2(-i, i), new Vector2(i, i), new Vector2(-i, -i), new Vector2(i, -i) };

                foreach(Vector2 pos in newposlist)
                {
                    if (GridScript.instance.checkifvalidpos(pos, newcharacter, 0))
                    {
                        newcharacter.transform.position = new Vector3(pos.x, 0, pos.y);
                        newcharacter.GetComponent<UnitScript>().previousposition = pos;
                        newcharacter.GetComponent<UnitScript>().MoveTo(pos);
                        posfound = true;
                        break;
                    }
                }

                if(posfound)
                {
                    break;
                }
            }
        }
        
        if(index!=-1)
        {
            newcharacter.name = enemyStats.Name + " " + index;
        }
        else
        {
            newcharacter.name = enemyStats.Name + " spawned";
        }

        if (enemyStats.isother)
        {
            Character.affiliation = "other";
            Character.affiliation = "other";
        }
        else
        {
            Character.affiliation = "enemy";
        }
        newcharacter.GetComponent<RandomScript>().InitializeRandomValues();
    }

    private int findfirstfreeid()
    {
        int maxid = 50;
        for (int i = 0; i < Characters.transform.childCount; i++)
        {
            Character character = Characters.transform.GetChild(i).GetComponent<UnitScript>().UnitCharacteristics;
            if (character.ID > maxid)
            {
                maxid = character.ID + 1;
            }
        }
        if (previousid >= maxid)
        {
            maxid = previousid + 1;
        }
        previousid = maxid;
        return maxid;
    }

    private void ManageModel(GameObject character)
    {
        foreach (ModelInfo modelinf in character.GetComponent<UnitScript>().ModelList)
        {
            if (modelinf.ID == character.GetComponent<UnitScript>().UnitCharacteristics.modelID)
            {
                modelinf.active = true;
                modelinf.wholeModel.SetActive(true);
            }
            else
            {
                modelinf.active = false;
                DestroyImmediate(modelinf.wholeModel);
            }
        }
        character.GetComponent<UnitScript>().ResetChildRenderers();
    }
}
