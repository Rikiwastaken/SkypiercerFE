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

        public GridSquareScript currentTile;

    }

    [Serializable]
    public class MonsterStats
    {
        public int size;
        public bool ispluvial;
    }

    [Serializable]
    public class PlayableStats
    {
        public int MaxSkillpoints = 99;
        public bool deployunit;
        public bool unlocked;
        public bool protagonist;
        public string battalion;
    }

    [Serializable]
    public class EnemyStats
    {
        public int classID;
        public int desiredlevel;
        public int itemtodropID;
        public bool usetelekinesis;
        public string personality; // nothing : basic. Deviant : High Random. Coward : deviant if below 33% hp. Daredevil : never takes into account their own HP
        public Vector2 startpos;
        public List<int> equipments;
        public List<int> Skills;
        public String Name;
        public bool isboss;
        public bool isother;
        public MonsterStats monsterStats;
        public int RemainingLifebars;
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

    public Transform handbone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GridScript = FindAnyObjectByType<GridScript>();
        AttackTurnScript = FindAnyObjectByType<AttackTurnScript>();

        if (UnitCharacteristics.EquipedSkills == null)
        {
            UnitCharacteristics.EquipedSkills = new List<int>(4);
        }
        FindAnyObjectByType<DataScript>().GenerateEquipmentList(UnitCharacteristics);
        LevelSetup();
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


    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (UnitCharacteristics.currentTile == null)
        {
            MoveTo(UnitCharacteristics.position);
        }
        else
        {
            if (UnitCharacteristics.currentTile.isstairs)
            {
                
                if(battlecameraScript.incombat)
                {
                    transform.position = new Vector3(transform.position.x, UnitCharacteristics.currentTile.transform.position.y + 1f, transform.position.z);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, UnitCharacteristics.currentTile.transform.position.y + 0.5f, transform.position.z);
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x, UnitCharacteristics.currentTile.transform.position.y, transform.position.z);
            }

        }

        Debug.DrawLine(transform.GetChild(1).position, transform.GetChild(1).position + Vector3.Normalize(transform.GetChild(1).forward - transform.GetChild(1).position) * 2f, Color.red);

        if (battlecameraScript == null)
        {
            battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

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
            if (!isinattackanimation() && !animator.GetCurrentAnimatorStateInfo(0).IsName("run") && Vector3.Distance(armature.localPosition, initialpos) > 0.15f)
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


        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), UnitCharacteristics.position) > 0.1f)
        {
            animator.SetBool("Walk", true);
            Vector2 direction = (UnitCharacteristics.position - new Vector2(transform.position.x, transform.position.z)).normalized;
            transform.position += new Vector3(direction.x, 0f, direction.y) * movespeed * Time.fixedDeltaTime;
            if (!battlecameraScript.incombat)
            {
                transform.forward =new Vector3(direction.x, 0f, direction.y).normalized;
                transform.GetChild(1).forward = new Vector3(direction.x, 0f, direction.y).normalized;
            }


        }
        else
        {
            transform.position = new Vector3(UnitCharacteristics.position.x, transform.position.y, UnitCharacteristics.position.y);
            if (animator.gameObject.activeSelf)
            {
                animator.SetBool("Walk", false);
            }

        }


        if (battlecameraScript.incombat)
        {
            if (battlecameraScript.fighter1 == gameObject || battlecameraScript.fighter2 == gameObject)
            {
                animator.gameObject.SetActive(true);
            }
            else
            {
                animator.gameObject.SetActive(false);
            }
        }
        else if (!animator.gameObject.activeSelf)
        {
            animator.gameObject.SetActive(true);
        }

        //TemporaryColor();
        UpdateRendererLayer();
        ManageDamagenumber();
        Hidedeactivated();
    }

    private void ManageLifebars()
    {
        transform.GetChild(0).rotation = Quaternion.Euler(90f, 0f, 0f);
        Image LifebarBehind = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        Image Lifebar = transform.GetChild(0).GetChild(1).GetComponent<Image>();

        if (battlecameraScript.incombat)
        {
            Lifebar.gameObject.SetActive(false);
            LifebarBehind.gameObject.SetActive(false);
        }
        else
        {
            Lifebar.gameObject.SetActive(true);
            LifebarBehind.gameObject.SetActive(true);
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
        if(GetSkill(58)) //weakness
        {
            UnitCharacteristics.AjustedStats.HP = (int)UnitCharacteristics.stats.HP / 2;
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
    public void MoveTo(Vector2 destination)
    {
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }
        GridSquareScript destTile = GridScript.GetTile(destination);
        if ((GridScript.GetUnit(destTile) == null || GridScript.GetUnit(destTile)==gameObject) && !destTile.isobstacle)
        {
            UnitCharacteristics.position = destination;
            UnitCharacteristics.currentTile = destTile;
        }

    }
    public bool isinattackanimation()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("run") )
        {
            return false;
        }
        return true;
    }

    public void PlayAttackAnimation(bool doubleattack =false, bool tripleattack = false, bool healing=false)
    {
        if(tripleattack)
        {
            animator.SetBool("Double", false);
            animator.SetBool("Triple", true);
        }
        else if(doubleattack)
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
            case "staff":
                if(healing)
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
        return ;
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
            currentequipmentmodel = Instantiate(equipmentmodel.Model);
            currentequipmentmodel.transform.localScale = Vector3.one * 0.5f;
            currentequipmentmodel.transform.SetParent(handbone);
            
            currentequipmentmodel.transform.localPosition = Vector3.zero;
            currentequipmentmodel.transform.localRotation = Quaternion.identity;
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

    private void Hidedeactivated()
    {
        if(UnitCharacteristics.currentTile.activated)
        {
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
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
            UnitCharacteristics.stats.HP += (int)(GrowthtoApply.HPGrowth / 100f);

            if ((int)oldHP < (int)(UnitCharacteristics.stats.HP))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldStrength = UnitCharacteristics.stats.Strength;

            UnitCharacteristics.stats.Strength += (int)(GrowthtoApply.StrengthGrowth / 100f);

            if ((int)oldStrength < (int)(UnitCharacteristics.stats.Strength))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldPsyche = UnitCharacteristics.stats.Psyche;

            UnitCharacteristics.stats.Psyche += (int)(GrowthtoApply.PsycheGrowth / 100f);

            if ((int)oldPsyche < (int)(UnitCharacteristics.stats.Psyche))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldDefense = UnitCharacteristics.stats.Defense;

            UnitCharacteristics.stats.Defense += (int)(GrowthtoApply.DefenseGrowth / 100f);

            if ((int)oldDefense < (int)(UnitCharacteristics.stats.Defense))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldResistance = UnitCharacteristics.stats.Resistance;

            UnitCharacteristics.stats.Resistance += (int)(GrowthtoApply.ResistanceGrowth / 100f);

            if ((int)oldResistance < (int)(UnitCharacteristics.stats.Resistance))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldSpeed = UnitCharacteristics.stats.Speed;

            UnitCharacteristics.stats.Speed += (int)(GrowthtoApply.SpeedGrowth / 100f);

            if ((int)oldSpeed < (int)(UnitCharacteristics.stats.Speed))
            {
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            float oldDexterity = UnitCharacteristics.stats.Dexterity;

            UnitCharacteristics.stats.Dexterity += (int)(GrowthtoApply.DexterityGrowth / 100f);

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
            int rd = UnityEngine.Random.Range(0, 100);
            if (rd <= GrowthtoApply.HPGrowth)
            {
                UnitCharacteristics.stats.HP += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= GrowthtoApply.StrengthGrowth)
            {
                UnitCharacteristics.stats.Strength += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= GrowthtoApply.PsycheGrowth)
            {
                UnitCharacteristics.stats.Psyche += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= GrowthtoApply.DefenseGrowth)
            {
                UnitCharacteristics.stats.Defense += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }


            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= GrowthtoApply.ResistanceGrowth)
            {
                UnitCharacteristics.stats.Resistance += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= GrowthtoApply.SpeedGrowth)
            {
                UnitCharacteristics.stats.Speed += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }

            rd = UnityEngine.Random.Range(0, 100);
            if (rd <= GrowthtoApply.DexterityGrowth)
            {
                UnitCharacteristics.stats.Dexterity += 1;
                lvlupresult.Add(1);
            }
            else
            {
                lvlupresult.Add(0);
            }
        }
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
