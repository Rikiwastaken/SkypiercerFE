
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnitScript;

public class MapInitializer : MonoBehaviour
{

    public static MapInitializer instance;

    public int numberofplayables;

    public List<Vector2> playablepos;
    public List<int> ForcedCharacters;

    public bool IsSideStory;
    public int ChapterID;

    public GameObject BaseCharacter;

    public GameObject Characters;

    public List<EnemyStats> EnemyList;

    private GridScript GridScript;

    private int previousid;

    private List<GameObject> playableUnitsDeployed;

    public BossScript currentboss;
    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EmptyNotUnlockedPlayables();

        InitializePlayers(true);
        InitializeNonPlayers();
        InitializeCopyAndTalkID();
    }

    private void EmptyNotUnlockedPlayables()
    {
        int numberofdeployed = 0;
        for (int i = 0; i < DataScript.instance.PlayableCharacterList.Count; i++)
        {
            Character currentchar = DataScript.instance.PlayableCharacterList[i];
            if (!currentchar.playableStats.unlocked)
            {
                currentchar.playableStats.deployunit = false;
            }
            if (ForcedCharacters.Contains(currentchar.ID))
            {
                currentchar.playableStats.deployunit = true;
            }
            if (currentchar.playableStats.deployunit)
            {
                numberofdeployed++;
            }
        }

        foreach (Character chara in DataScript.instance.PlayableCharacterList)
        {
            if (chara.playableStats.unlocked && !chara.playableStats.deployunit)
            {
                if (numberofdeployed < playablepos.Count)
                {
                    chara.playableStats.deployunit = true;
                    numberofdeployed++;
                }
            }
        }

    }

    public void InitializePlayers(bool firstinit = false)
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

        if (firstinit)
        {
            playableUnitsDeployed = new List<GameObject>();
            foreach (Vector2 position in playablepos)
            {
                playableUnitsDeployed.Add(null);
            }
        }

        numberofplayables = playablepos.Count;

        bool intestmap = SceneManager.GetActiveScene().name == "TestMap";

        foreach (Character playable in DataScript.instance.PlayableCharacterList)
        {
            if (ForcedCharacters.Contains(playable.ID))
            {
                playable.playableStats.deployunit = true;
                playable.playableStats.unlocked = true;
                AddUnit(playable);
            }

        }

        foreach (Character playable in DataScript.instance.PlayableCharacterList)
        {

            if (playable.playableStats.deployunit && (intestmap || (playable.playableStats.unlocked && firstinit)) && !ForcedCharacters.Contains(playable.ID))
            {
                AddUnit(playable);
            }
            playable.TauntTurns = 0;
            playable.isintercepting = false;
            playable.TemporarySkill = 0;
            playable.statusEffects = new StatusEffects() { BurnTurns = 0, ConcussionTunrs = 0, WeaknessTurns = 0, RegenTurns = 0, AccelerationTurns = 0, PowerTurns = 0, ParalyzedTurns = 0, StunTurns = 0 };
            playable.currentHP = (int)playable.AjustedStats.HP;
        }


        GridScript.InitializeGOList();
    }

    private int GetFirstFreePlayablePos()
    {
        for (int i = 0; i < playableUnitsDeployed.Count; i++)
        {
            if (playableUnitsDeployed[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    private void PlaceUnit(GameObject unit)
    {
        int firstfreeindex = GetFirstFreePlayablePos();
        if (firstfreeindex != -1)
        {
            unit.GetComponent<UnitScript>().MoveTo(playablepos[firstfreeindex], false, true);
            playableUnitsDeployed[firstfreeindex] = unit;
        }
    }

    public void DeleteUnit(Character unit)
    {
        if (unit == null)
        {
            return;
        }
        GameObject UnitGO = null;
        foreach (GameObject unitdeployed in playableUnitsDeployed)
        {
            if (unitdeployed != null && unitdeployed.GetComponent<UnitScript>().UnitCharacteristics.ID == unit.ID)
            {
                UnitGO = unitdeployed;
                break;
            }
        }
        if (UnitGO != null && playableUnitsDeployed.Contains(UnitGO))
        {
            playableUnitsDeployed[playableUnitsDeployed.IndexOf(UnitGO)] = null;
            Destroy(UnitGO);
        }
    }

    public void AddUnit(Character playable)
    {
        if (GetFirstFreePlayablePos() != -1)
        {

            GameObject newcharacter = Instantiate(BaseCharacter);
            newcharacter.GetComponent<UnitScript>().UnitCharacteristics = playable;
            newcharacter.GetComponent<UnitScript>().InstantiateCharacterModel();
            newcharacter.GetComponent<UnitScript>().calculateStats();
            newcharacter.transform.parent = Characters.transform;
            PlaceUnit(newcharacter);
            newcharacter.name = playable.name;
        }

    }

    public void ExchangePlaces(GameObject unit1, GameObject unit2)
    {
        Vector2 unit1pos = new Vector2(unit1.GetComponent<UnitScript>().UnitCharacteristics.position.x, unit1.GetComponent<UnitScript>().UnitCharacteristics.position.y);
        Vector2 unit2pos = new Vector2(unit2.GetComponent<UnitScript>().UnitCharacteristics.position.x, unit2.GetComponent<UnitScript>().UnitCharacteristics.position.y);
        GridSquareScript temp = GridScript.GetFirstClosestTile(unit1pos);
        unit1.GetComponent<UnitScript>().MoveTo(temp.GridCoordinates);
        unit2.GetComponent<UnitScript>().MoveTo(unit1pos);
        unit1.GetComponent<UnitScript>().MoveTo(unit2pos);

        int unit1Index = playableUnitsDeployed.IndexOf(unit1);
        playableUnitsDeployed[playableUnitsDeployed.IndexOf(unit2)] = unit1;
        playableUnitsDeployed[unit1Index] = unit2;

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

    private void InitializeCopyAndTalkID()
    {
        int talkindex = 0;
        int copyindex = 0;

        if (DataScript.instance.ChapterFlagsList.Count <= ChapterID)
        {
            SaveManager.ChapterFlags newcurrentflags = new SaveManager.ChapterFlags();
            newcurrentflags.talkflags = new List<bool>();
            newcurrentflags.copyflags = new List<bool>();
            DataScript.instance.ChapterFlagsList.Add(newcurrentflags);
        }

        SaveManager.ChapterFlags currentflags = DataScript.instance.ChapterFlagsList[ChapterID];

        for (int i = 0; i < Characters.transform.childCount; i++)
        {
            GameObject CharGO = Characters.transform.GetChild(i).gameObject;
            Character Char = CharGO.GetComponent<UnitScript>().UnitCharacteristics;
            if (Char.affiliation != "playable")
            {

                if (Char.UnitSkill != 0)
                {
                    Char.enemyStats.CopyID = copyindex;
                    copyindex++;
                }

                if (currentflags.copyflags != null && currentflags.copyflags.Count > copyindex && currentflags.copyflags[copyindex])
                {
                    CharGO.GetComponent<UnitScript>().copied = true;
                }


                if (Char.enemyStats.talkable)
                {
                    Char.enemyStats.talkID = talkindex;
                    talkindex++;
                }
            }
        }
    }

    /// <summary>
    /// Creating a single nonplayable character based on the given EnemyStats. If index is given, it will be added to the name of the character, otherwise "spawned" will be added to the name.
    /// </summary>
    /// <param name="enemyStats"></param>
    /// <param name="index"></param>
    public void InitializeNonPlayable(EnemyStats enemyStats, int index = -1, Character chartouse = null)
    {
        GameObject newcharacter = Instantiate(BaseCharacter);
        if (chartouse != null)
        {
            newcharacter.GetComponent<UnitScript>().UnitCharacteristics = newcharacter.GetComponent<UnitScript>().CreateCopy(chartouse);
        }
        Character Character = newcharacter.GetComponent<UnitScript>().UnitCharacteristics;

        Character.enemyStats = enemyStats;
        // in the cae of pluvials, randomly take a model.
        if (enemyStats.monsterStats.ispluvial)
        {
            Character.modelID = Random.Range(3, newcharacter.GetComponent<UnitScript>().ModelList.Count);
        }
        else
        {
            Character.modelID = enemyStats.modelID;
        }

        if (enemyStats.bossiD > 0)
        {
            currentboss = newcharacter.GetComponent<BossScript>();
        }

        newcharacter.GetComponent<UnitScript>().InstantiateCharacterModel();
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
        if (GridScript.instance.checkifvalidpos(enemyStats.startpos, newcharacter, 0))
        {
            newcharacter.transform.position = new Vector3(enemyStats.startpos.x, 0, enemyStats.startpos.y);
            newcharacter.GetComponent<UnitScript>().previouspos = GridScript.GetTile(enemyStats.startpos);
            newcharacter.GetComponent<UnitScript>().MoveTo(enemyStats.startpos);
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {

                bool posfound = false;

                List<Vector2> newposlist = new List<Vector2>() { new Vector2(0, i), new Vector2(0, -i), new Vector2(i, 0), new Vector2(-i, 0), new Vector2(-i, i), new Vector2(i, i), new Vector2(-i, -i), new Vector2(i, -i) };

                foreach (Vector2 pos in newposlist)
                {
                    if (GridScript.instance.checkifvalidpos(pos, newcharacter, 0))
                    {
                        newcharacter.transform.position = new Vector3(pos.x, 0, pos.y);
                        newcharacter.GetComponent<UnitScript>().previouspos = GridScript.GetTile(pos);
                        newcharacter.GetComponent<UnitScript>().MoveTo(pos);
                        posfound = true;
                        break;
                    }
                }

                if (posfound)
                {
                    break;
                }
            }
        }

        if (index != -1)
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
        }
        else
        {
            Character.affiliation = "enemy";
        }

        if (Character.name.ToLower().Contains("kira"))
        {
            foreach (equipment equ in Character.equipments)
            {
                if (equ.type.ToLower() == "sword")
                {
                    equ.Name = "Reshine";
                }
            }
        }
        Character.statusEffects = new StatusEffects() { BurnTurns = 0, ConcussionTunrs = 0, WeaknessTurns = 0, RegenTurns = 0, AccelerationTurns = 0, PowerTurns = 0, ParalyzedTurns = 0, StunTurns = 0 };
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
}
