using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DataScript;

public class UnitScript : MonoBehaviour
{
    [Serializable]
    public class Character
    {

        public string name;
        public int ID;
        public BaseStats stats;
        public BaseStats AjustedStats;
        public int level;
        public int experience;
        public string affiliation; // playable, enemy, other
        public StatGrowth growth;
        public int currentHP;
        public int movements;
        public Vector2 position;
        public bool alreadyplayed;
        public bool alreadymoved;
        public bool telekinesisactivated;
        public List<int> equipmentsIDs;
        public List<equipment> equipments;
        public int UnitSkill;
        public List<int> EquipedSkills;
        public bool isboss;
        public bool attacksfriends;

        public PlayableStats playableStats;
        public EnemyStats enemyStats;

        public List<GridSquareScript> currentTile;
        public int modelID;
        public List<WeaponMastery> Masteries;
    }

    [Serializable]
    public class WeaponMastery
    {
        public string weapontype;
        public int Exp;
        public int Level;
    }

    [Serializable]
    public class ModelInfo
    {
        public int ID;
        public GameObject wholeModel;
        public Transform handbone;
        public Transform Lefthandbone;
        public Vector3 rotationadjust;
        public bool active;
    }

    [Serializable]
    public class MonsterStats
    {
        public int size;
        public bool ispluvial;
        public bool ismachine;
    }

    [Serializable]
    public class PlayableStats
    {
        public int MaxSkillpoints = 99;
        public bool deployunit;
        public bool unlocked;
        public bool protagonist;
        public string battalion;
        public int ID;
    }

    [Serializable]
    public class EnemyStats
    {
        public int classID;
        public int desiredlevel;
        public int itemtodropID;
        public bool usetelekinesis;
        public string personality;
        /* nothing : basic.
         * Deviant : High Random.
         * Coward : deviant if below 33% hp, survivor if below 10%.
         * Daredevil : never takes into account their own HP.
         * Survivor : Always avoid enemies and attacks.
         * Guard : Does not move 
         * Hunter : Can get to enemies no matter the distance
         */
        public Vector2 startpos;
        public List<int> equipments;
        public List<int> Skills;
        public String Name;
        public bool isboss;
        public bool isother;
        public MonsterStats monsterStats;
        public int RemainingLifebars;
        public int modelID;
    }

    [Serializable]
    public class equipment
    {
        public string Name;
        public int BaseDamage;
        public int BaseHit;
        public int BaseCrit;
        public int Range;
        public string type;
        public int Currentuses;
        public int Maxuses;
        public int ID;
        public int Grade;
        public equipmentmodel equipmentmodel;
    }

    [Serializable]
    public class equipmentmodel
    {
        public GameObject Model;
        public Vector3 localposition;
        public Vector3 localscale;
        public Vector3 localrotation;
    }

    [Serializable]
    public class BaseStats
    {
        public float HP;
        public float Strength;
        public float Psyche;
        public float Defense;
        public float Resistance;
        public float Speed;
        public float Dexterity;
    }


    [Serializable]
    public class AllStatsSkillBonus
    {
        public int Strength;
        public int Psyche;
        public int Defense;
        public int Resistance;
        public int Speed;
        public int Dexterity;
        public int Dodge;
        public int Hit;
        public int Crit;
        public int PhysDamage;
        public int TelekDamage;
        public int DamageReduction;
        public int FixedDamageBonus;
        public int FixedDamageReduction;
    }

    [Serializable]
    public class StatGrowth
    {
        public int HPGrowth;
        public int StrengthGrowth;
        public int PsycheGrowth;
        public int DefenseGrowth;
        public int ResistanceGrowth;
        public int SpeedGrowth;
        public int DexterityGrowth;
    }

    [Serializable]
    public class NumberToShow
    {
        public string EffectName;
        public int number;
        public bool ishealing;
        public int framesremaining;
    }

    public Character UnitCharacteristics;


    public bool trylvlup;
    public bool fixedgrowth;

    public equipment Fists;

    public float movespeed;

    private battlecameraScript battlecameraScript;

    private Animator animator;

    private Transform armature;

    private Vector3 initialpos;
    private Vector3 initialforward;

    private GameObject currentequipmentmodel;

    private AttackTurnScript AttackTurnScript;

    public Material AllyMat;
    public Material EnemyMat;
    public GameObject Head;

    public int unitkilled;

    public int tilesmoved;
    public int numberoftimeswaitted;
    public int SurvivorStacks;

    public TextMeshProUGUI DmgText;
    public TextMeshProUGUI DmgEffectNameText;
    public int waittedbonusturns;

    private List<NumberToShow> damagestoshow = new List<NumberToShow>();

    public float timeforshowingnumbers;

    public bool copied;

    private GridScript GridScript;

    private List<Vector2> pathtotake = new List<Vector2>();

    private float canvaselevation;
    private MinimapScript MinimapScript;

    public List<ModelInfo> ModelList;

    public Vector3 rotationadjust;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MinimapScript = FindAnyObjectByType<MinimapScript>();
        canvaselevation = transform.GetChild(0).localPosition.y;
        GridScript = FindAnyObjectByType<GridScript>();
        AttackTurnScript = FindAnyObjectByType<AttackTurnScript>();

        if (UnitCharacteristics.EquipedSkills == null)
        {
            UnitCharacteristics.EquipedSkills = new List<int>(4);
        }
        FindAnyObjectByType<DataScript>().GenerateEquipmentList(UnitCharacteristics);
        LevelSetup();
        UnitCharacteristics.alreadymoved = false;
        UnitCharacteristics.alreadyplayed = false;
        Fists = FindAnyObjectByType<DataScript>().equipmentList[0];
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        UnitCharacteristics.position = new Vector2((int)transform.position.x, (int)transform.position.z);
        if (UnitCharacteristics.equipments == null)
        {
            UnitCharacteristics.equipments = new List<equipment>();
            for (int i = 0; i < 8; i++)
            {
                UnitCharacteristics.equipments.Add(new equipment());
            }
        }
        else if (UnitCharacteristics.equipments.Count < 8)
        {
            for (int i = UnitCharacteristics.equipments.Count; i < 8; i++)
            {
                UnitCharacteristics.equipments.Add(new equipment());
            }
        }
        UnitCharacteristics.currentHP = (int)UnitCharacteristics.AjustedStats.HP;
        UpdateWeaponModel();
        //if (UnitCharacteristics.affiliation == "playable")
        //{
        //    Head.GetComponent<SkinnedMeshRenderer>().material = AllyMat;
        //}
        //else
        //{
        //    Head.GetComponent<SkinnedMeshRenderer>().material = EnemyMat;
        //}

        foreach (ModelInfo modelInfo in ModelList)
        {
            if (modelInfo.active)
            {
                modelInfo.wholeModel.SetActive(true); ;
            }
        }


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        ManagePosition();

        Debug.DrawLine(transform.GetChild(1).position, transform.GetChild(1).position + Vector3.Normalize(transform.GetChild(1).forward - transform.GetChild(1).position) * 2f, Color.red);

        if (battlecameraScript == null)
        {
            battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        }

        if (animator == null)
        {
            foreach (ModelInfo modelInfo in ModelList)
            {
                if (modelInfo.active)
                {
                    animator = modelInfo.wholeModel.GetComponentInChildren<Animator>();
                    rotationadjust = modelInfo.rotationadjust;
                }
            }


        }
        animator.SetBool("Ismachine", UnitCharacteristics.enemyStats.monsterStats.ismachine);
        if (armature == null)
        {
            armature = animator.transform;
            initialpos = armature.localPosition;
            initialforward = armature.forward;
        }
        if (animator != null)
        {
            if (!isinattackanimation() && !battlecameraScript.incombat)
            {
                if (Vector3.Distance(armature.localPosition, initialpos) > 0.1f)
                {
                    armature.localPosition += (initialpos - armature.localPosition).normalized * 0.2f * Time.deltaTime;
                }

                //armature.rotation = Quaternion.LookRotation(initialforward, Vector3.up);

            }
            if (!isinattackanimation() && !isinrunanimation() && Vector3.Distance(armature.localPosition, initialpos) > 0.15f)
            {
                armature.localPosition = initialpos;
            }
        }

        if (trylvlup)
        {
            trylvlup = false;
            LevelUp();
        }
        ManageLifebars();


        ManageMovement();


        bool shouldBeActive = battlecameraScript.fighter1 == gameObject || battlecameraScript.fighter2 == gameObject || !battlecameraScript.incombat;

        if (animator.gameObject.activeSelf != shouldBeActive)
        {
            animator.gameObject.SetActive(shouldBeActive);
        }

        //TemporaryColor();
        UpdateRendererLayer();
        ManageDamagenumber();
        Hidedeactivated();
        ManageSize();
    }

    //Manage Vertical Position
    private void ManagePosition()
    {
        if (UnitCharacteristics.currentTile.Count <= 0)
        {
            MoveTo(UnitCharacteristics.position);
        }
        else if (UnitCharacteristics.currentTile.Count > 0)
        {
            if (UnitCharacteristics.currentTile[0].isstairs)
            {

                if (battlecameraScript.incombat)
                {
                    transform.position = new Vector3(transform.position.x, UnitCharacteristics.currentTile[0].transform.position.y + 1f, transform.position.z);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, UnitCharacteristics.currentTile[0].transform.position.y + 0.5f, transform.position.z);
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x, UnitCharacteristics.currentTile[0].transform.position.y, transform.position.z);
            }

        }
    }

    //Manage horizontal movements
    private void ManageMovement()
    {
        if (pathtotake.Count > 0)
        {
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), pathtotake[0]) > 0.1f)
            {
                animator.SetBool("Walk", true);
                Vector2 direction = (pathtotake[0] - new Vector2(transform.position.x, transform.position.z)).normalized;
                transform.position += new Vector3(direction.x, 0f, direction.y) * movespeed * Time.fixedDeltaTime;
                if (!battlecameraScript.incombat)
                {
                    transform.forward = new Vector3(direction.x, 0f, direction.y).normalized;
                    transform.GetChild(1).forward = new Vector3(direction.x, 0f, direction.y).normalized;
                    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotationadjust);
                }
            }
            else
            {
                pathtotake.RemoveAt(0);
            }
        }
        else
        {
            Vector2 destination = new Vector2();
            if (UnitCharacteristics.enemyStats.monsterStats.size > 1)
            {
                destination = UnitCharacteristics.position + new Vector2(-0.5f, 0.5f);
            }
            else
            {
                destination = UnitCharacteristics.position;
            }
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), destination) > 0.1f)
            {
                animator.SetBool("Walk", true);
                Vector2 direction = (destination - new Vector2(transform.position.x, transform.position.z)).normalized;
                transform.position += new Vector3(direction.x, 0f, direction.y) * movespeed * Time.fixedDeltaTime;
                if (!battlecameraScript.incombat)
                {
                    transform.forward = new Vector3(direction.x, 0f, direction.y).normalized;
                    transform.GetChild(1).forward = new Vector3(direction.x, 0f, direction.y).normalized;
                    transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + rotationadjust);
                }


            }
            else
            {
                transform.position = new Vector3(destination.x, transform.position.y, destination.y);
                if (animator.gameObject.activeSelf)
                {
                    animator.SetBool("Walk", false);
                }

            }
        }

    }

    public void ResetPath()
    {
        pathtotake = new List<Vector2>();
    }
    private void ManageLifebars()
    {
        transform.GetChild(0).rotation = Quaternion.Euler(90f, 0f, 0f);
        Image LifebarBehind = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        Image Lifebar = transform.GetChild(0).GetChild(1).GetComponent<Image>();

        if (battlecameraScript.incombat)
        {
            if (Lifebar.gameObject.activeSelf)
            {
                Lifebar.gameObject.SetActive(false);
                LifebarBehind.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!Lifebar.gameObject.activeSelf)
            {
                Lifebar.gameObject.SetActive(true);
                LifebarBehind.gameObject.SetActive(true);
            }
            Lifebar.type = Image.Type.Filled;
            Lifebar.fillAmount = (float)UnitCharacteristics.currentHP / (float)UnitCharacteristics.AjustedStats.HP;
            if (UnitCharacteristics.enemyStats != null)
            {
                switch (UnitCharacteristics.enemyStats.RemainingLifebars)
                {
                    case 0:
                        Lifebar.color = Color.green;
                        break;
                    case 1:
                        Lifebar.color = Color.blue;
                        break;
                    case 2:
                        Lifebar.color = Color.magenta;
                        break;
                    case 3:
                        Lifebar.color = Color.yellow;
                        break;
                    case 4:
                        Lifebar.color = Color.gray;
                        break;
                }
            }
        }

    }

    public void calculateStats()
    {
        if (GetSkill(58)) //weakness
        {
            UnitCharacteristics.AjustedStats.HP = (int)UnitCharacteristics.stats.HP;
            UnitCharacteristics.AjustedStats.Strength = (int)UnitCharacteristics.stats.Strength / 2;
            UnitCharacteristics.AjustedStats.Psyche = (int)UnitCharacteristics.stats.Psyche / 2;
            UnitCharacteristics.AjustedStats.Defense = (int)UnitCharacteristics.stats.Defense / 2;
            UnitCharacteristics.AjustedStats.Resistance = (int)UnitCharacteristics.stats.Resistance / 2;
            UnitCharacteristics.AjustedStats.Speed = (int)UnitCharacteristics.stats.Speed / 2;
            UnitCharacteristics.AjustedStats.Dexterity = (int)UnitCharacteristics.stats.Dexterity / 2;
        }
        else
        {
            UnitCharacteristics.AjustedStats.HP = (int)UnitCharacteristics.stats.HP;
            UnitCharacteristics.AjustedStats.Strength = (int)UnitCharacteristics.stats.Strength;
            UnitCharacteristics.AjustedStats.Psyche = (int)UnitCharacteristics.stats.Psyche;
            UnitCharacteristics.AjustedStats.Defense = (int)UnitCharacteristics.stats.Defense;
            UnitCharacteristics.AjustedStats.Resistance = (int)UnitCharacteristics.stats.Resistance;
            UnitCharacteristics.AjustedStats.Speed = (int)UnitCharacteristics.stats.Speed;
            UnitCharacteristics.AjustedStats.Dexterity = (int)UnitCharacteristics.stats.Dexterity;
        }

    }

    private void GainMastery(Character character, string weapontounlock = "", bool secundary = false)
    {
        int maxlevel = 4;
        List<WeaponMastery> masteries = character.Masteries;
        if (weapontounlock == "")
        {
            weapontounlock = GetFirstWeapon().type.ToLower();
        }

        foreach (WeaponMastery mastery in masteries)
        {
            if (mastery.weapontype.ToLower() == weapontounlock)
            {
                if (mastery.Level > 0 && mastery.Level <= maxlevel || (mastery.Level == 0 && secundary))
                {
                    if (GetSkill(11))
                    {
                        mastery.Exp += 1;
                    }
                    mastery.Exp += 1;
                }
            }
            LevelupMasteryCheck(mastery);
        }
    }

    private void LevelupMasteryCheck(WeaponMastery mastery)
    {
        bool levelup = false;
        switch (mastery.Level)
        {
            case 0:
                if (mastery.Exp >= 30)
                {
                    levelup = true;
                }
                break;
            case 1:
                if (mastery.Exp >= 30)
                {
                    levelup = true;
                }
                break;
            case 2:
                if (mastery.Exp >= 50)
                {
                    levelup = true;
                }
                break;
            case 3:
                if (mastery.Exp >= 100)
                {
                    levelup = true;
                }
                break;
        }

        if (levelup)
        {
            mastery.Level++;
        }
    }

    public void GainCombatMastery()
    {

        foreach (Character ally in GridScript.allunits)
        {
            if (ally.affiliation == "playable" && ManhattanDistance(UnitCharacteristics, ally) <= 2 && ally != UnitCharacteristics)
            {
                GainMastery(ally, "", true);
            }
        }

        GainMastery(UnitCharacteristics, GetFirstWeapon().type.ToLower());


    }
    public void MoveTo(Vector2 destination, bool jump = false, bool instantaneousmovement = false)
    {
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }
        GridSquareScript destTile = GridScript.GetTile(destination);
        if ((GridScript.GetUnit(destTile) == null || GridScript.GetUnit(destTile) == gameObject) && !destTile.isobstacle)
        {
            if (UnitCharacteristics.currentTile != null && UnitCharacteristics.currentTile.Count > 0)
            {
                foreach (GridSquareScript tile in UnitCharacteristics.currentTile)
                {
                    if (tile != null)
                    {
                        tile.UpdateInsideSprite(false);
                    }

                }


                if (jump)
                {
                    pathtotake = new List<Vector2> { destination };
                }

                else if (UnitCharacteristics.currentTile[0] != null)
                {
                    pathtotake = GridScript.FindPath(UnitCharacteristics.currentTile[0].GridCoordinates, destination, UnitCharacteristics);
                }

            }
            else
            {
                transform.position = new Vector3(destination.x, transform.position.y, destination.y);
            }
            if (instantaneousmovement)
            {
                pathtotake = new List<Vector2>();
                transform.position = new Vector3(destination.x, transform.position.y, destination.y);
            }
            UnitCharacteristics.position = destination;
            UpdateTiles(destTile);
            foreach (GridSquareScript tile in UnitCharacteristics.currentTile)
            {
                tile.UpdateInsideSprite(true, UnitCharacteristics);
            }
        }
        if (MinimapScript == null)
        {
            MinimapScript = FindAnyObjectByType<MinimapScript>();
        }
        MinimapScript.UpdateMinimap();
    }

    private void UpdateTiles(GridSquareScript destination)
    {
        UnitCharacteristics.currentTile = new List<GridSquareScript> { destination };
        if (UnitCharacteristics.enemyStats.monsterStats.size > 1)
        {
            UnitCharacteristics.currentTile.Add(GridScript.GetTile(destination.GridCoordinates + new Vector2(-1, 0)));
            UnitCharacteristics.currentTile.Add(GridScript.GetTile(destination.GridCoordinates + new Vector2(0, 1)));
            UnitCharacteristics.currentTile.Add(GridScript.GetTile(destination.GridCoordinates + new Vector2(-1, 1)));
        }
    }

    public bool isinattackanimation()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_slow") || animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_fast_3") || animator.GetCurrentAnimatorStateInfo(0).IsName("HumanWalk"))
        {
            return false;
        }
        return true;
    }

    public bool isinrunanimation()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_fast_3") || animator.GetCurrentAnimatorStateInfo(0).IsName("HumanWalk"))
        {
            return false;
        }
        return true;
    }

    public void PlayAttackAnimation(bool doubleattack = false, bool tripleattack = false, bool healing = false)
    {
        if (tripleattack)
        {
            animator.SetBool("Double", false);
            animator.SetBool("Triple", true);
        }
        else if (doubleattack)
        {
            animator.SetBool("Double", true);
            animator.SetBool("Triple", false);
        }
        else
        {
            animator.SetBool("Double", false);
            animator.SetBool("Triple", false);
        }
        animator.SetTrigger("Attack");

        equipment weapon = GetFirstWeapon();

        animator.SetBool("Slash", false);
        animator.SetBool("Stab", false);
        animator.SetBool("Punch", false);
        animator.SetBool("GreatSword", false);
        animator.SetBool("Heal", false);
        animator.SetBool("Bow", false);

        switch (weapon.type.ToLower())
        {
            case "sword":
                animator.SetBool("Slash", true);
                break;
            case "spear":
                animator.SetBool("Stab", true);
                break;
            case "greatsword":
                animator.SetBool("GreatSword", true);
                break;
            case "scythe":
                animator.SetBool("GreatSword", true);
                break;
            case "shield":
                animator.SetBool("Punch", true);
                break;
            case "bow":
                animator.SetBool("Bow", true);
                break;
            case "staff":
                if (healing)
                {
                    animator.SetBool("Heal", true);
                }
                else
                {
                    animator.SetBool("Stab", true);
                }
                break;
            case "none":
                animator.SetBool("Punch", true);
                break;
            default:
                animator.SetBool("Slash", true);
                break;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("run"))
        {
            return;
        }
        return;
    }

    public void AddNumber(int amount, bool ishealing, string effectname)
    {
        NumberToShow newnumber = new NumberToShow();
        newnumber.number = amount;
        newnumber.ishealing = ishealing;
        newnumber.EffectName = effectname;
        newnumber.framesremaining = (int)(timeforshowingnumbers / Time.fixedDeltaTime);
        damagestoshow.Add(newnumber);
    }

    private void ManageDamagenumber()
    {
        if (damagestoshow.Count > 0 && !battlecameraScript.incombat)
        {
            DmgText.enabled = true;
            DmgEffectNameText.enabled = true;
            if (damagestoshow[0].number == 0)
            {
                DmgText.enabled = false;
            }
            else
            {
                DmgText.enabled = true;
                if (damagestoshow[0].ishealing)
                {
                    DmgText.text = "<color=green>" + damagestoshow[0].number + "</color>";
                }
                else
                {
                    DmgText.text = "<color=red>" + damagestoshow[0].number + "</color>";
                }
            }




            DmgEffectNameText.text = damagestoshow[0].EffectName;

            if (damagestoshow[0].framesremaining >= 0)
            {
                damagestoshow[0].framesremaining--;
            }
            if (damagestoshow[0].framesremaining <= 0)
            {
                damagestoshow.RemoveAt(0);
            }
            else
            {
                AttackTurnScript.delaybeforenxtunit = 3;
            }
        }
        else
        {
            DmgText.enabled = false;
            DmgEffectNameText.enabled = false;
        }
    }

    public void ResetForward()
    {
        initialforward = armature.forward;
    }
    public void UpdateWeaponModel()
    {
        if (currentequipmentmodel != null)
        {
            Destroy(currentequipmentmodel);
        }
        if (GetFirstWeapon().Grade != 0 && GetFirstWeapon().Currentuses != 0)
        {
            equipmentmodel equipmentmodel = GetFirstWeapon().equipmentmodel;
            if (equipmentmodel.Model != null)
            {
                currentequipmentmodel = Instantiate(equipmentmodel.Model);
                currentequipmentmodel.transform.localScale = Vector3.one * 0.5f;
                foreach (ModelInfo modelInfo in ModelList)
                {
                    if (modelInfo.active)
                    {
                        if (GetFirstWeapon().type.ToLower() == "bow")
                        {
                            currentequipmentmodel.transform.SetParent(modelInfo.Lefthandbone);
                        }
                        else if (GetFirstWeapon().type.ToLower() != "machine")
                        {
                            currentequipmentmodel.transform.SetParent(modelInfo.handbone);
                        }
                    }
                }



                currentequipmentmodel.transform.localPosition = Vector3.zero;
                currentequipmentmodel.transform.localRotation = Quaternion.identity;
            }

        }
    }

    public void RetreatTrigger() // Effect of Retreat or Verso
    {
        FindAnyObjectByType<AttackTurnScript>().DeathCleanup();

        if (GetSkill(1)) //Retreat
        {
            AddNumber(0, true, "Retreat");
            UnitCharacteristics.alreadymoved = false;

            GridScript gridScript = FindAnyObjectByType<GridScript>();
            gridScript.InitializeGOList();
            gridScript.selection = gridScript.GetTile(UnitCharacteristics.position);
            gridScript.ShowMovement();

            gridScript.lockedmovementtiles = gridScript.movementtiles;
            gridScript.lockselection = true;
            ActionManager actionManager = FindAnyObjectByType<ActionManager>();
            actionManager.frameswherenotlock = 0;
            actionManager.framestoskip = 10;
            actionManager.currentcharacter = gameObject;
        }
        if (GetSkill(31)) //Verso
        {
            AddNumber(0, true, "Verso");
            int remainingMovements = UnitCharacteristics.movements - tilesmoved;
            if (GetSkill(1))//checking if unit is using Retreat
            {
                remainingMovements -= 2;
            }
            if (GetSkill(5)) // checking if unit is using Fast Legs
            {
                remainingMovements += 1;
            }

            if (remainingMovements <= 0)
            {
                UnitCharacteristics.alreadymoved = true;
            }
            else
            {
                UnitCharacteristics.alreadymoved = false;
                GridScript gridScript = FindAnyObjectByType<GridScript>();
                gridScript.InitializeGOList();
                gridScript.selection = gridScript.GetTile(UnitCharacteristics.position);
                gridScript.ShowLimitedMovementOfUnit(gameObject, remainingMovements);

                gridScript.lockedmovementtiles = gridScript.movementtiles;
                gridScript.lockselection = true;
                ActionManager actionManager = FindAnyObjectByType<ActionManager>();
                actionManager.frameswherenotlock = 0;
                actionManager.framestoskip = 10;
                actionManager.currentcharacter = gameObject;
            }



        }
    }

    public bool GetSkill(int SkillID)
    {
        if (UnitCharacteristics.EquipedSkills.Contains(SkillID) || UnitCharacteristics.UnitSkill == SkillID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void LevelSetup()
    {
        if (UnitCharacteristics.affiliation != "playable")
        {

            UnitCharacteristics.telekinesisactivated = UnitCharacteristics.enemyStats.usetelekinesis;

            ClassInfo classtoapply = FindAnyObjectByType<DataScript>().ClassList[UnitCharacteristics.enemyStats.classID];
            UnitCharacteristics.stats.HP = classtoapply.BaseStats.HP;
            UnitCharacteristics.stats.Strength = classtoapply.BaseStats.Strength;
            UnitCharacteristics.stats.Psyche = classtoapply.BaseStats.Psyche;
            UnitCharacteristics.stats.Defense = classtoapply.BaseStats.Defense;
            UnitCharacteristics.stats.Resistance = classtoapply.BaseStats.Resistance;
            UnitCharacteristics.stats.Speed = classtoapply.BaseStats.Speed;
            UnitCharacteristics.stats.Dexterity = classtoapply.BaseStats.Dexterity;
            UnitCharacteristics.growth.HPGrowth = classtoapply.StatGrowth.HPGrowth;
            UnitCharacteristics.growth.StrengthGrowth = classtoapply.StatGrowth.StrengthGrowth;
            UnitCharacteristics.growth.PsycheGrowth = classtoapply.StatGrowth.PsycheGrowth;
            UnitCharacteristics.growth.DefenseGrowth = classtoapply.StatGrowth.DefenseGrowth;
            UnitCharacteristics.growth.ResistanceGrowth = classtoapply.StatGrowth.ResistanceGrowth;
            UnitCharacteristics.growth.SpeedGrowth = classtoapply.StatGrowth.SpeedGrowth;
            UnitCharacteristics.growth.DexterityGrowth = classtoapply.StatGrowth.DexterityGrowth;
            fixedgrowth = true;
            int numberoflevelups = UnitCharacteristics.enemyStats.desiredlevel - UnitCharacteristics.level;

            for (int i = 0; i < numberoflevelups; i++)
            {

                List<int> statsgained = LevelUp();
                string statsgainedstr = "";
                foreach (int level in statsgained)
                {
                    statsgainedstr += level.ToString() + " , ";
                }
            }
        }
        calculateStats();

    }

    private void ManageSize()
    {
        if (UnitCharacteristics.enemyStats.monsterStats.size > 0)
        {
            for (int i = 1; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.localScale.x != UnitCharacteristics.enemyStats.monsterStats.size)
                {
                    child.localScale = Vector3.one * UnitCharacteristics.enemyStats.monsterStats.size;
                }
            }
            transform.GetChild(0).position = new Vector3(transform.GetChild(0).position.x, canvaselevation + UnitCharacteristics.enemyStats.monsterStats.size, transform.GetChild(0).position.z);
        }



    }

    private void Hidedeactivated()
    {
        bool checkifonactivated = CheckIfOnActivated();
        if (transform.GetChild(0).gameObject.activeSelf != checkifonactivated)
        {
            transform.GetChild(1).gameObject.SetActive(CheckIfOnActivated());
            transform.GetChild(0).gameObject.SetActive(CheckIfOnActivated());
        }


    }

    public string GetName()
    {
        return UnitCharacteristics.name;
    }
    public bool CheckIfOnActivated()
    {
        bool isdeactivated = false;
        foreach (GridSquareScript tile in UnitCharacteristics.currentTile)
        {
            if (!tile.activated)
            {
                isdeactivated = true;
                break;
            }
        }
        if (isdeactivated)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void UpdateRendererLayer()
    {
        if (UnitCharacteristics.alreadyplayed)
        {
            foreach (MeshRenderer Renderer in GetComponentsInChildren<MeshRenderer>())
            {
                //Renderer.renderingLayerMask = 0;
            }

        }
        else
        {
            foreach (MeshRenderer Renderer in GetComponentsInChildren<MeshRenderer>())
            {
                switch (UnitCharacteristics.affiliation)
                {
                    case "playable":
                        Renderer.renderingLayerMask = 1;
                        break;
                    case "enemy":
                        Renderer.renderingLayerMask = 2;
                        break;
                    case "other":
                        Renderer.renderingLayerMask = 3;
                        break;

                }
            }
        }

    }
    private void TemporaryColor()
    {

        Color newcolor = Color.white;
        if (UnitCharacteristics.affiliation == "playable")
        {
            newcolor = Color.blue;
            if (UnitCharacteristics.alreadyplayed)
            {
                newcolor *= 0.5f;
                newcolor.a = 1f;
            }
        }
        else if (UnitCharacteristics.affiliation == "enemy")
        {
            newcolor = Color.red;
            if (UnitCharacteristics.alreadyplayed)
            {
                newcolor *= 0.5f;
                newcolor.a = 1f;
            }
        }
        else
        {
            newcolor = Color.yellow;
            if (UnitCharacteristics.alreadyplayed)
            {
                newcolor *= 0.5f;
                newcolor.a = 1f;
            }
        }

        if (battlecameraScript.incombat)
        {
            newcolor.a = 0f;
            if (battlecameraScript.fighter1 == gameObject || battlecameraScript.fighter2 == gameObject)
            {
                newcolor.a = 1f;
            }
        }


        GetComponent<MeshRenderer>().material.color = newcolor;
    }

    public List<int> LevelUp()
    {
        List<int> lvlupresult = new List<int>();
        StatGrowth GrowthtoApply = new StatGrowth();

        GrowthtoApply.HPGrowth = UnitCharacteristics.growth.HPGrowth;
        GrowthtoApply.PsycheGrowth = UnitCharacteristics.growth.PsycheGrowth;
        GrowthtoApply.StrengthGrowth = UnitCharacteristics.growth.StrengthGrowth;
        GrowthtoApply.DefenseGrowth = UnitCharacteristics.growth.DefenseGrowth;
        GrowthtoApply.ResistanceGrowth = UnitCharacteristics.growth.ResistanceGrowth;
        GrowthtoApply.SpeedGrowth = UnitCharacteristics.growth.SpeedGrowth;
        GrowthtoApply.DexterityGrowth = UnitCharacteristics.growth.DexterityGrowth;

        //Genius
        if (GetSkill(10))
        {
            int geniusgrowthboost = 25;
            GrowthtoApply.HPGrowth += geniusgrowthboost;
            GrowthtoApply.PsycheGrowth += geniusgrowthboost;
            GrowthtoApply.StrengthGrowth += geniusgrowthboost;
            GrowthtoApply.DefenseGrowth += geniusgrowthboost;
            GrowthtoApply.ResistanceGrowth += geniusgrowthboost;
            GrowthtoApply.SpeedGrowth += geniusgrowthboost;
            GrowthtoApply.DexterityGrowth += geniusgrowthboost;
        }

        //Crystal Heart
        if (GetSkill(57))
        {
            int cystalheartgrowthboost = 10;
            GrowthtoApply.HPGrowth += cystalheartgrowthboost;
            GrowthtoApply.PsycheGrowth += cystalheartgrowthboost;
            GrowthtoApply.StrengthGrowth += cystalheartgrowthboost;
            GrowthtoApply.DefenseGrowth += cystalheartgrowthboost;
            GrowthtoApply.ResistanceGrowth += cystalheartgrowthboost;
            GrowthtoApply.SpeedGrowth += cystalheartgrowthboost;
            GrowthtoApply.DexterityGrowth += cystalheartgrowthboost;
        }

        //JackOfAllTrades
        if (GetSkill(25))
        {
            int average = GrowthtoApply.HPGrowth + GrowthtoApply.PsycheGrowth + GrowthtoApply.StrengthGrowth + GrowthtoApply.DefenseGrowth + GrowthtoApply.ResistanceGrowth + GrowthtoApply.SpeedGrowth + GrowthtoApply.DexterityGrowth;
            average = average / 7;

            GrowthtoApply.HPGrowth = average;
            GrowthtoApply.PsycheGrowth = average;
            GrowthtoApply.StrengthGrowth = average;
            GrowthtoApply.DefenseGrowth = average;
            GrowthtoApply.ResistanceGrowth = average;
            GrowthtoApply.SpeedGrowth = average;
            GrowthtoApply.DexterityGrowth = average;
        }

        UnitCharacteristics.experience -= 100;
        UnitCharacteristics.level += 1;
        if (fixedgrowth)
        {
            float oldHP = UnitCharacteristics.stats.HP;
            UnitCharacteristics.stats.HP += (GrowthtoApply.HPGrowth / 100f);

            if ((int)oldHP < (int)(UnitCharacteristics.stats.HP))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldStrength = UnitCharacteristics.stats.Strength;

            UnitCharacteristics.stats.Strength += (GrowthtoApply.StrengthGrowth / 100f);

            if ((int)oldStrength < (int)(UnitCharacteristics.stats.Strength))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldPsyche = UnitCharacteristics.stats.Psyche;

            UnitCharacteristics.stats.Psyche += (GrowthtoApply.PsycheGrowth / 100f);

            if ((int)oldPsyche < (int)(UnitCharacteristics.stats.Psyche))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldDefense = UnitCharacteristics.stats.Defense;

            UnitCharacteristics.stats.Defense += (GrowthtoApply.DefenseGrowth / 100f);

            if ((int)oldDefense < (int)(UnitCharacteristics.stats.Defense))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldResistance = UnitCharacteristics.stats.Resistance;

            UnitCharacteristics.stats.Resistance += (GrowthtoApply.ResistanceGrowth / 100f);

            if ((int)oldResistance < (int)(UnitCharacteristics.stats.Resistance))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldSpeed = UnitCharacteristics.stats.Speed;

            UnitCharacteristics.stats.Speed += (GrowthtoApply.SpeedGrowth / 100f);

            if ((int)oldSpeed < (int)(UnitCharacteristics.stats.Speed))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldDexterity = UnitCharacteristics.stats.Dexterity;

            UnitCharacteristics.stats.Dexterity += (GrowthtoApply.DexterityGrowth / 100f);

            if ((int)oldDexterity < (int)(UnitCharacteristics.stats.Dexterity))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

        }
        else
        {

            RandomScript.RandomLevelValues levelValues = GetComponent<RandomScript>().GetLevelUpRandomValues();

            if (levelValues.HPRandomValue <= GrowthtoApply.HPGrowth)
            {
                UnitCharacteristics.stats.HP += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            if (levelValues.StrengthRandomValue <= GrowthtoApply.StrengthGrowth)
            {
                UnitCharacteristics.stats.Strength += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            if (levelValues.PsycheRandomValue <= GrowthtoApply.PsycheGrowth)
            {
                UnitCharacteristics.stats.Psyche += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            if (levelValues.DefenseRandomValue <= GrowthtoApply.DefenseGrowth)
            {
                UnitCharacteristics.stats.Defense += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }


            if (levelValues.ResistanceRandomValue <= GrowthtoApply.ResistanceGrowth)
            {
                UnitCharacteristics.stats.Resistance += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            if (levelValues.SpeedRandomValue <= GrowthtoApply.SpeedGrowth)
            {
                UnitCharacteristics.stats.Speed += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }


            if (levelValues.DexterityRandomValue <= GrowthtoApply.DexterityGrowth)
            {
                UnitCharacteristics.stats.Dexterity += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }
        }

        string levelupstring = UnitCharacteristics.name+"  levelup : ";
        foreach( int  level in lvlupresult )
        {
            levelupstring += level+" ";
        }

        Debug.Log(levelupstring);

        calculateStats();
        return lvlupresult;
    }

    public equipment GetFirstWeapon()
    {
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                return UnitCharacteristics.equipments[i];
            }
        }

        return Fists;
    }

    public List<equipment> GetAllWeapons()
    {
        List<equipment> weapons = new List<equipment>();
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                weapons.Add(UnitCharacteristics.equipments[i]);
            }
        }
        if (weapons.Count == 0)
        {
            weapons.Add(Fists);
        }
        return weapons;
    }

    public equipment GetNextWeapon()
    {
        List<equipment> listweapons = new List<equipment>();
        List<equipment> rest = new List<equipment>();
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                listweapons.Add(UnitCharacteristics.equipments[i]);
            }
            else
            {
                rest.Add(UnitCharacteristics.equipments[i]);
            }
        }
        if (listweapons.Count > 0)
        {
            UnitCharacteristics.equipments = new List<equipment>();
            for (int i = 1; i < listweapons.Count; i++)
            {
                UnitCharacteristics.equipments.Add(listweapons[i]);
            }
            UnitCharacteristics.equipments.Add(listweapons[0]);
            for (int i = 0; i < rest.Count; i++)
            {
                UnitCharacteristics.equipments.Add(rest[i]);
            }
            SynchroniseWeaponIDs();
            return UnitCharacteristics.equipments[0];
        }
        else
        {
            return null;
        }

    }

    private void SynchroniseWeaponIDs()
    {
        UnitCharacteristics.equipmentsIDs = new List<int>();
        foreach (equipment equipment in UnitCharacteristics.equipments)
        {
            UnitCharacteristics.equipmentsIDs.Add(equipment.ID);
        }
        UpdateWeaponModel();
    }

    public void EquipWeapon(equipment weapon)
    {
        if (UnitCharacteristics.equipments.Contains(weapon))
        {
            int index = UnitCharacteristics.equipments.IndexOf(weapon);
            int safegard = 0;
            while (index != 0 && safegard < 20)
            {
                GetNextWeapon();
                index = UnitCharacteristics.equipments.IndexOf(weapon);
            }
        }
        SynchroniseWeaponIDs();
    }

    public string GetWeatherType()
    {
        int numberofraintiles = 0;
        int numberofsuntiles = 0;
        foreach (GridSquareScript tile in UnitCharacteristics.currentTile)
        {
            if (tile.RemainingRainTurns > 0)
            {
                numberofraintiles++;
            }
            if (tile.RemainingSunTurns > 0)
            {
                numberofsuntiles++;
            }
        }
        if (numberofraintiles > numberofsuntiles)
        {
            return "rain";
        }
        else if (numberofraintiles < numberofsuntiles)
        {
            return "sun";
        }
        else
        {
            return "";
        }
    }

    public equipment GetPreviousWeapon()
    {
        List<equipment> listweapons = new List<equipment>();
        List<equipment> rest = new List<equipment>();
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].Currentuses > 0)
            {
                listweapons.Add(UnitCharacteristics.equipments[i]);
            }
            else
            {
                rest.Add(UnitCharacteristics.equipments[i]);
            }
        }
        if (listweapons.Count > 0)
        {
            UnitCharacteristics.equipments = new List<equipment>();
            UnitCharacteristics.equipments.Add(listweapons[listweapons.Count - 1]);
            for (int i = 0; i < listweapons.Count - 1; i++)
            {
                UnitCharacteristics.equipments.Add(listweapons[i]);
            }
            for (int i = 0; i < rest.Count; i++)
            {
                UnitCharacteristics.equipments.Add(rest[i]);
            }
            SynchroniseWeaponIDs();
            return UnitCharacteristics.equipments[0];
        }
        else
        {
            return null;
        }
    }

    public void RestoreUses(int number)
    {
        for (int i = 0; i < UnitCharacteristics.equipments.Count; i++)
        {
            if (UnitCharacteristics.equipments[i].type != "item" && UnitCharacteristics.equipments[i].type != null)
            {
                UnitCharacteristics.equipments[i].Currentuses += number;
                if (UnitCharacteristics.equipments[i].Currentuses > UnitCharacteristics.equipments[i].Maxuses)
                {
                    UnitCharacteristics.equipments[i].Currentuses = UnitCharacteristics.equipments[i].Maxuses;
                }
            }
        }
        UpdateWeaponModel();
    }
    public (int, bool) GetRangeAndMele()
    {
        equipment firstweapon = GetFirstWeapon();
        int range = firstweapon.Range;
        bool melee = true;
        if (firstweapon.type.ToLower() == "bow")
        {
            melee = false;
            if (UnitCharacteristics.telekinesisactivated)
            {
                range += 2;
                if (GetSkill(33)) //focus
                {
                    range += 1;
                }
            }
        }
        else
        {
            if (UnitCharacteristics.telekinesisactivated)
            {
                range += 1;
                if (GetSkill(33)) //focus
                {
                    range += 1;
                }
            }
        }

        return (range, melee);
    }


    public AllStatsSkillBonus GetBattalionCombatBonus()
    {
        AllStatsSkillBonus statbonuses = new AllStatsSkillBonus();

        //Gale Side effect
        List<GameObject> allunitsGO = FindAnyObjectByType<TurnManger>().playableunitGO;
        List<Character> allunits = FindAnyObjectByType<TurnManger>().playableunit;
        foreach (GameObject characterGO in allunitsGO)
        {
            Character character = characterGO.GetComponent<UnitScript>().UnitCharacteristics;
            if (ManhattanDistance(UnitCharacteristics, character) == 1 && character.playableStats.battalion == "Gale" && character.affiliation == UnitCharacteristics.affiliation)
            {
                statbonuses.FixedDamageReduction += (int)character.AjustedStats.Defense / 5;
                statbonuses.FixedDamageBonus += (int)character.AjustedStats.Strength / 5;
                //Loyal
                if (characterGO.GetComponent<UnitScript>().GetSkill(35))
                {
                    statbonuses.FixedDamageReduction += (int)character.AjustedStats.Defense / 5;
                    statbonuses.FixedDamageBonus += (int)character.AjustedStats.Strength / 5;
                }
            }
        }


        if (UnitCharacteristics.playableStats.protagonist)
        {
            //Zack main effect
            if (UnitCharacteristics.playableStats.battalion == "Zack")
            {
                foreach (Character character in allunits)
                {
                    if (character.playableStats.battalion == "Zack" && character != UnitCharacteristics)
                    {
                        statbonuses.Hit += 1;
                        statbonuses.Crit += 1;
                    }
                }
            }
            //Gale main effect
            if (UnitCharacteristics.playableStats.battalion == "Gale")
            {
                foreach (Character character in allunits)
                {
                    if (character.playableStats.battalion == "Gale" && character != UnitCharacteristics)
                    {
                        statbonuses.Defense += (int)character.AjustedStats.Defense / 20;
                        statbonuses.Strength += (int)character.AjustedStats.Strength / 20;
                    }
                }
            }
            //Kira main effect
            if (UnitCharacteristics.playableStats.battalion == "Kira")
            {
                foreach (Character character in allunits)
                {
                    if (character.playableStats.battalion == "Kira" && character != UnitCharacteristics)
                    {
                        statbonuses.Psyche += (int)character.AjustedStats.Psyche / 20;

                    }
                }
            }
        }
        else
        {
            //Zack Side Effect
            if (UnitCharacteristics.playableStats.battalion == "Zack")
            {
                statbonuses.Hit += 5;
                statbonuses.Crit += 5;
                //Loyal
                if (GetSkill(35))
                {
                    statbonuses.Hit += 5;
                    statbonuses.Crit += 5;
                }
            }
        }



        return statbonuses;
    }
    public AllStatsSkillBonus GetStatSkillBonus(GameObject enemy)
    {
        AllStatsSkillBonus statbonuses = new AllStatsSkillBonus();

        //Despair
        if (GetSkill(2))
        {
            if (UnitCharacteristics.currentHP <= (float)UnitCharacteristics.AjustedStats.HP * 0.33f)
            {
                statbonuses.Crit += 20;
                statbonuses.Dodge += 40;
            }
        }
        //Psychic
        if (GetSkill(3))
        {
            statbonuses.TelekDamage += 15;
            statbonuses.PhysDamage -= 20;
        }
        //Brute
        if (GetSkill(4))
        {
            statbonuses.TelekDamage += 20;
            statbonuses.PhysDamage -= 15;
        }
        //Inspired
        if (GetSkill(6))
        {
            List<Character> activelist = null;
            if (UnitCharacteristics.affiliation == "playable")
            {
                activelist = FindAnyObjectByType<TurnManger>().playableunit;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = FindAnyObjectByType<TurnManger>().enemyunit;
            }
            else
            {
                activelist = FindAnyObjectByType<TurnManger>().otherunits;
            }

            foreach (Character otherunitchar in activelist)
            {
                int movement = otherunitchar.movements;
                if (ManhattanDistance(UnitCharacteristics, otherunitchar) <= movement && otherunitchar.playableStats.battalion == UnitCharacteristics.playableStats.battalion && otherunitchar.playableStats.protagonist)
                {
                    statbonuses.FixedDamageBonus += 3;
                    statbonuses.FixedDamageReduction += 3;
                    break;
                }
            }
        }
        //FastAndDeadly
        if (GetSkill(13))
        {
            statbonuses.Crit += (int)UnitCharacteristics.AjustedStats.Speed / 2;
        }
        //Sniper
        if (GetSkill(16))
        {
            statbonuses.Hit += 9999;
        }
        //Nimble
        if (GetSkill(18))
        {
            statbonuses.Dodge += 20;
            statbonuses.DamageReduction -= 30;
        }
        //LuckySeven
        if (GetSkill(19))
        {
            if (Math.Abs(UnitCharacteristics.currentHP % 10) == 7)
            {
                statbonuses.Strength += 7;
                statbonuses.Psyche += 7;
                statbonuses.Resistance += 7;
                statbonuses.Defense += 7;
                statbonuses.Speed += 7;
                statbonuses.Dexterity += 7;
            }
        }
        //In Great Shape
        if (GetSkill(21))
        {
            if (UnitCharacteristics.currentHP >= UnitCharacteristics.AjustedStats.HP)
            {
                statbonuses.Hit += 10;
                statbonuses.Dodge += 10;
            }
        }
        // Competitive
        if (GetSkill(22))
        {
            if (enemy != null)
            {
                if (GetFirstWeapon().type.ToLower() == enemy.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower())
                {
                    statbonuses.FixedDamageBonus += 5;
                }
            }

        }
        //Giant Crusher
        if (GetSkill(23))
        {
            if (enemy != null)
            {
                if (enemy.GetComponent<UnitScript>().UnitCharacteristics.isboss)
                {
                    statbonuses.PhysDamage += 1;
                    statbonuses.TelekDamage += 10;
                    statbonuses.DamageReduction += 10;
                }
            }

        }
        //Last Stand
        if (GetSkill(24))
        {
            List<GameObject> activelist = null;
            if (UnitCharacteristics.affiliation == "playable")
            {
                activelist = FindAnyObjectByType<TurnManger>().playableunitGO;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = FindAnyObjectByType<TurnManger>().enemyunitGO;
            }
            else
            {
                activelist = FindAnyObjectByType<TurnManger>().otherunitsGO;
            }

            if (activelist.Count == 1)
            {
                statbonuses.Strength += (int)(UnitCharacteristics.AjustedStats.Strength * 0.25f);
                statbonuses.Psyche += (int)(UnitCharacteristics.AjustedStats.Psyche * 0.25f);
                statbonuses.Resistance += (int)(UnitCharacteristics.AjustedStats.Resistance * 0.25f);
                statbonuses.Defense += (int)(UnitCharacteristics.AjustedStats.Defense * 0.25f);
                statbonuses.Speed += (int)(UnitCharacteristics.AjustedStats.Speed * 0.25f);
                statbonuses.Dexterity += (int)(UnitCharacteristics.AjustedStats.Dexterity * 0.25f);
            }
        }
        // Solitary
        if (GetSkill(26))
        {
            List<Character> activelist = null;
            if (UnitCharacteristics.affiliation == "playable")
            {
                activelist = FindAnyObjectByType<TurnManger>().playableunit;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = FindAnyObjectByType<TurnManger>().enemyunit;
            }
            else
            {
                activelist = FindAnyObjectByType<TurnManger>().otherunits;
            }

            bool toofar = true;
            foreach (Character otherunitchar in activelist)
            {
                if (ManhattanDistance(UnitCharacteristics, otherunitchar) <= 3 && otherunitchar != UnitCharacteristics)
                {
                    toofar = false; break;
                }
            }
            if (toofar)
            {
                statbonuses.FixedDamageBonus += 5;
            }
        }

        //Revenge
        if (GetSkill(28))
        {
            if (enemy != null)
            {
                if (enemy.GetComponent<UnitScript>().UnitCharacteristics.isboss)
                {
                    statbonuses.FixedDamageBonus += ((int)UnitCharacteristics.AjustedStats.HP - UnitCharacteristics.currentHP);
                }
            }

        }
        //KillingSpree
        if (GetSkill(29))
        {
            if (enemy != null)
            {
                if (enemy.GetComponent<UnitScript>().UnitCharacteristics.isboss)
                {
                    statbonuses.Strength += 1 * unitkilled;
                    statbonuses.Psyche += 1 * unitkilled;
                    statbonuses.Resistance += 1 * unitkilled;
                    statbonuses.Defense += 1 * unitkilled;
                    statbonuses.Speed += 1 * unitkilled;
                    statbonuses.Dexterity += 1 * unitkilled;
                }
            }

        }

        //Survivor
        if (GetSkill(32))
        {
            statbonuses.Strength += (SurvivorStacks / 3);
            statbonuses.Psyche += (SurvivorStacks / 3);
            statbonuses.Resistance += (SurvivorStacks / 3);
            statbonuses.Defense += (SurvivorStacks / 3);
            statbonuses.Speed += (SurvivorStacks / 3);
            statbonuses.Dexterity += (SurvivorStacks / 3);
        }
        //Bravery
        if (GetSkill(36))
        {
            if (enemy != null)
            {
                int difference = enemy.GetComponent<UnitScript>().UnitCharacteristics.level - UnitCharacteristics.level;

                if (difference > 0)
                {
                    statbonuses.Strength += difference / 2;
                    statbonuses.Psyche += difference / 2;
                    statbonuses.Resistance += difference / 2;
                    statbonuses.Defense += difference / 2;
                    statbonuses.Speed += difference / 2;
                    statbonuses.Dexterity += difference / 2;
                }
            }



        }
        //Noble Fight
        if (GetSkill(37))
        {
            statbonuses.Hit += 30;
            statbonuses.Dodge -= 30;
        }
        //Thousand Needles
        if (GetSkill(39))
        {
            statbonuses.PhysDamage -= 50;
            statbonuses.TelekDamage -= 50;
        }

        // Together we ride
        if (GetSkill(41))
        {
            List<Character> activelist = null;
            if (UnitCharacteristics.affiliation == "playable")
            {
                activelist = FindAnyObjectByType<TurnManger>().playableunit;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = FindAnyObjectByType<TurnManger>().enemyunit;
            }
            else
            {
                activelist = FindAnyObjectByType<TurnManger>().otherunits;
            }

            int closepalls = 0;
            foreach (Character otherunitchar in activelist)
            {
                if (ManhattanDistance(UnitCharacteristics, otherunitchar) <= 2 && otherunitchar != UnitCharacteristics)
                {
                    closepalls++;
                }
            }
            statbonuses.Strength += closepalls / 2;
            statbonuses.Psyche += closepalls / 2;
            statbonuses.Resistance += closepalls / 2;
            statbonuses.Defense += closepalls / 2;
            statbonuses.Speed += 5 * closepalls / 2;
            statbonuses.Dexterity += closepalls / 2;

        }

        // Readiness
        if (GetSkill(42))
        {
            int dmgbonus = (int)Mathf.Min(numberoftimeswaitted * 5, 30);
            statbonuses.TelekDamage += dmgbonus;
            statbonuses.PhysDamage += dmgbonus;
        }

        // Recklessness
        if (GetSkill(43))
        {
            statbonuses.TelekDamage += 20;
            statbonuses.PhysDamage += 20;
            statbonuses.DamageReduction -= 20;
        }

        // Enduring
        if (GetSkill(55))
        {
            statbonuses.TelekDamage += ((int)UnitCharacteristics.AjustedStats.HP - UnitCharacteristics.currentHP) / 2;
            statbonuses.PhysDamage += ((int)UnitCharacteristics.AjustedStats.HP - UnitCharacteristics.currentHP) / 2;
        }

        //Amphibian
        if (GetSkill(61))
        {
            bool onwatertile = false;
            foreach (GridSquareScript tile in UnitCharacteristics.currentTile)
            {
                if (tile.RemainingRainTurns > 0 || tile.type.ToLower() == "water")
                {
                    onwatertile = true;
                    break;
                }
            }
            if (onwatertile)
            {
                statbonuses.Dodge += 15;
                statbonuses.Hit += 15;
            }
        }

        //Shut-In
        if (GetSkill(62))
        {
            bool onsunnytile = false;
            foreach (GridSquareScript tile in UnitCharacteristics.currentTile)
            {
                if (tile.RemainingSunTurns > 0)
                {
                    onsunnytile = true;
                    break;
                }
            }
            if (onsunnytile)
            {
                statbonuses.TelekDamage -= 6;
                statbonuses.PhysDamage -= 6;
            }
            else
            {
                statbonuses.TelekDamage += 3;
                statbonuses.PhysDamage += 3;
            }
        }

        //Master Hunter
        if (GetSkill(63))
        {
            if (GetFirstWeapon() == Fists)
            {
                statbonuses.Hit += 80;
                statbonuses.TelekDamage += 3;
                statbonuses.PhysDamage += 3;
            }
        }


        AllStatsSkillBonus battalionskillbonus = GetBattalionCombatBonus();

        statbonuses.FixedDamageBonus += battalionskillbonus.FixedDamageBonus;
        statbonuses.FixedDamageReduction += battalionskillbonus.FixedDamageReduction;
        statbonuses.PhysDamage += battalionskillbonus.PhysDamage;
        statbonuses.TelekDamage += battalionskillbonus.TelekDamage;
        statbonuses.Hit += battalionskillbonus.Hit;
        statbonuses.Crit += battalionskillbonus.Crit;
        statbonuses.Dodge += battalionskillbonus.Dodge;
        statbonuses.DamageReduction += battalionskillbonus.DamageReduction;
        statbonuses.Strength += battalionskillbonus.Strength;
        statbonuses.Psyche += battalionskillbonus.Psyche;
        statbonuses.Resistance += battalionskillbonus.Resistance;
        statbonuses.Defense += battalionskillbonus.Defense;
        statbonuses.Speed += battalionskillbonus.Speed;
        statbonuses.Dexterity += battalionskillbonus.Dexterity;

        return statbonuses;
    }

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }

    public List<Skill> GetCommands()
    {
        DataScript datascript = FindAnyObjectByType<DataScript>();
        List<Skill> Commands = new List<Skill>();
        if (UnitCharacteristics.UnitSkill != 0)
        {
            if (datascript.SkillList[UnitCharacteristics.UnitSkill].IsCommand)
            {
                Commands.Add(datascript.SkillList[UnitCharacteristics.UnitSkill]);
            }
            foreach (int SkillID in UnitCharacteristics.EquipedSkills)
            {
                if (datascript.SkillList[SkillID].IsCommand && SkillID != 0)
                {
                    Commands.Add(datascript.SkillList[SkillID]);
                }
            }
        }
        return Commands;
    }

    public (int, bool, string) GetRangeMeleeAndType()
    {
        equipment firstweapon = GetFirstWeapon();
        int range = firstweapon.Range;
        bool melee = true;
        string type = firstweapon.type;
        if (firstweapon.type.ToLower() == "bow")
        {
            melee = false;
            if (UnitCharacteristics.telekinesisactivated)
            {
                range += 2;

                if (GetSkill(33)) //focus
                {
                    range += 1;
                }
            }
        }
        else
        {
            if (UnitCharacteristics.telekinesisactivated)
            {
                range += 1;
                if (GetSkill(33)) //focus
                {
                    range += 1;
                }
            }
        }

        return (range, melee, type);
    }
}
