using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public int TemporarySkill;
        public List<int> EquipedSkills;
        public bool attacksfriends;

        public PlayableStats playableStats;
        public EnemyStats enemyStats;

        public List<GridSquareScript> currentTile;
        public int modelID;
        public List<WeaponMastery> Masteries;
        public float DialoguePitch;
        public int TauntTurns;
        public bool isintercepting;
        public StatusEffects statusEffects;
    }

    [Serializable]
    public class WeaponMastery
    {
        public string weapontype;
        public int Exp;
        public int Level;
        public string Modifier;
    }

    [Serializable]
    public class StatusEffects
    {
        public int BurnTurns;
        public int StunTurns;
        public int ParalyzedTurns;
        public int ConcussionTunrs;
        public int WeaknessTurns;
        public int RegenTurns;
        public int AccelerationTurns;
        public int PowerTurns;
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
        public int bossiD;
        public bool isother;
        public MonsterStats monsterStats;
        public int RemainingLifebars;
        public int modelID;
        public bool talkable;
        public int talkID;
        public bool talkedto;
        public int SpriteID;
        public int PlayableSpriteID;
        public int CopyID;
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
        public string Modifier;
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
        public bool isBond;
    }

    public Character UnitCharacteristics;



    public bool fixedgrowth;

    public equipment Fists;

    public float movespeed;

    private cameraScript cameraScript;

    private Animator animator;

    private Transform armature;

    private Vector3 initialpos;
    private Vector3 initialforward;

    private GameObject currentequipmentmodel;

    private AttackTurnScript AttackTurnScript;

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

    public BaseStats previousStats;


    public List<GridSquareScript> previouspos;

    public bool disableLifebar;


    [Header("Flying Weapon")]
    public GameObject FlyingWeapon;

    public Vector3 telekinesisWeaponPos;
    public Vector3 telekinesisWeaponRot;
    private bool flyingweaponmovingup;
    public float flyingmovingspeed;
    public float maxmovementrangevertical;

    private List<float> Randomnumbers;
    private int Randomnumbersindex;

    private MeshRenderer[] childrenmeshrender;

    private Transform CanvasTransform;

    private event Action<int> OnHealthChanged;
    private int previousHPForEvent;
    private int HPForEvent
    {
        get => previousHPForEvent;
        set
        {
            if (previousHPForEvent != value) // Only trigger if the value actually changes
            {
                previousHPForEvent = value;
                OnHealthChanged?.Invoke(previousHPForEvent); // Fire the event
            }
        }
    }

    private Color InitialCharacterColor;
    private event Action<bool> OnPlayedChanged;
    private bool previousalreadyplayedforevent;
    private bool PlayedEvent
    {
        get => previousalreadyplayedforevent;
        set
        {
            if (previousalreadyplayedforevent != value) // Only trigger if the value actually changes
            {
                previousalreadyplayedforevent = value;
                OnPlayedChanged?.Invoke(previousalreadyplayedforevent); // Fire the event
            }
        }
    }


    public Image Lifebar;
    public Image LBBackground;

    private int delayedUpdateCounter;

    public float delayedUpdateTime;

    public Image AffinityArrow;

    [Header("\nEquipment/Type/Copy/Telekinesis Sprites")]
    [Header("Equipment Sprites")]
    public Sprite BareHandSprite;
    public Sprite SwordSprite;
    public Sprite SpearSprite;
    public Sprite GreatSwordSprite;
    public Sprite BowSprite;
    public Sprite ScytheSprite;
    public Sprite ShieldSprite;
    public Sprite StaffSprite;
    public Image WeaponImage;

    [Header("Copy Sprites")]
    public Sprite SkillNotCopiedSprite;
    public Sprite SkillCopiedSprite;
    public Image CopiedSkillImage;

    [Header("Type Sprites")]
    public Sprite PluvialSprite;
    public Sprite MachineSprite;
    public Image UnitTypeImage;

    [Header("Type Sprites")]
    public Sprite TelekinesisSprite;
    public Image TelekinesisImage;

    [Header("Tools")]
    public bool trylvlup;
    public bool forcemoveTO;
    public Vector2 forcemovetonewpos;



    public GameObject ActiveModel;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CanvasTransform = transform.GetChild(0);
        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            this.enabled = false;
            CanvasTransform.gameObject.SetActive(false);
            GetComponent<BattleCharacterScript>().enabled = true;
            return;
        }



        MinimapScript = MinimapScript.instance;
        canvaselevation = CanvasTransform.localPosition.y;
        GridScript = GridScript.instance;
        AttackTurnScript = FindAnyObjectByType<AttackTurnScript>();

        if (UnitCharacteristics.EquipedSkills == null)
        {
            UnitCharacteristics.EquipedSkills = new List<int>(4);
        }
        DataScript.instance.GenerateEquipmentList(UnitCharacteristics);
        LevelSetup();
        UnitCharacteristics.alreadymoved = false;
        UnitCharacteristics.alreadyplayed = false;
        Fists = DataScript.instance.equipmentList[0];
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


        Randomnumbers = new List<float>();
        {
            for (int i = 0; i < 100; i++)
            {
                Randomnumbers.Add((float)UnityEngine.Random.Range(0.9f, 1.1f));
            }
        }


        if (animator.GetBool("Ismachine") != UnitCharacteristics.enemyStats.monsterStats.ismachine)
        {
            animator.SetBool("Ismachine", UnitCharacteristics.enemyStats.monsterStats.ismachine);
        }
        if (animator.GetBool("Ispluvial") != UnitCharacteristics.enemyStats.monsterStats.ispluvial)
        {
            animator.SetBool("Ispluvial", UnitCharacteristics.enemyStats.monsterStats.ispluvial);
        }
        ResetChildRenderers();
        OnHealthChanged += HealthChangedHandler;
        OnPlayedChanged += PlayedChangedHandler;


        if (UnitCharacteristics.affiliation == "enemy" || UnitCharacteristics.attacksfriends)
        {
            if (UnitCharacteristics.UnitSkill != 0)
            {
                CopiedSkillImage.sprite = SkillNotCopiedSprite;
            }
            else
            {
                CopiedSkillImage.color = Color.clear;
            }

            if (UnitCharacteristics.enemyStats.monsterStats.ispluvial)
            {
                UnitTypeImage.sprite = PluvialSprite;
            }
            else if (UnitCharacteristics.enemyStats.monsterStats.ismachine)
            {
                UnitTypeImage.sprite = MachineSprite;
            }
            else
            {
                UnitTypeImage.color = Color.clear;
            }
        }
        else
        {
            CopiedSkillImage.color = Color.clear;
            UnitTypeImage.color = Color.clear;
        }

    }


    public void ResetChildRenderers()
    {
        childrenmeshrender = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        ManageLifeBarRotation();

        if (cameraScript == null)
        {
            cameraScript = cameraScript.instance;
        }

        if (animator == null)
        {
            foreach (ModelInfo modelInfo in ModelList)
            {
                if (modelInfo.active)
                {
                    animator = modelInfo.wholeModel.GetComponentInChildren<Animator>();
                    rotationadjust = modelInfo.rotationadjust;
                    ActiveModel = modelInfo.wholeModel;
                }
            }


        }

        if (armature == null)
        {
            armature = animator.transform;
            initialpos = armature.localPosition;
            initialforward = armature.forward;
        }



        HPForEvent = UnitCharacteristics.currentHP;

        PlayedEvent = UnitCharacteristics.alreadyplayed && UnitCharacteristics.affiliation.ToLower() == TurnManger.instance.currentlyplaying.ToLower();


        //TemporaryColor();

        ManageDamagenumber();


        DelayedUpdate();


        if (forcemoveTO)
        {
            forcemoveTO = false;
            MoveTo(forcemovetonewpos);
        }
    }

    private void FixedUpdate()
    {

        if (cameraScript == null)
        {
            cameraScript = cameraScript.instance;
        }

        ManagePosition();
        ManageMovement();
        ManageFlyingWeaponPosition();
        if (animator != null && armature != null)
        {
            if (!isinattackanimation() && !cameraScript.incombat)
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
    }

    //Update that only happens once every couple of frames to improve performance
    private void DelayedUpdate()
    {
        if (delayedUpdateCounter <= 0)
        {
            delayedUpdateCounter = (int)(UnityEngine.Random.Range(0.9f, 1.1f) * delayedUpdateTime / Time.deltaTime);

            //dostuff



            if (copied && CopiedSkillImage.sprite != SkillCopiedSprite)
            {
                CopiedSkillImage.sprite = SkillCopiedSprite;
            }
            else if (!copied && CopiedSkillImage.sprite == SkillCopiedSprite)
            {
                CopiedSkillImage.sprite = SkillNotCopiedSprite;
            }

            if (animator.GetBool("UsingTelekinesis") != UnitCharacteristics.telekinesisactivated && GetFirstWeapon().type.ToLower() != "bow")
            {
                animator.SetBool("UsingTelekinesis", UnitCharacteristics.telekinesisactivated && GetFirstWeapon().type.ToLower() != "bow");
            }

            ShowAffinityArrow();
            UpdateRendererLayer();
            ManageSize();
            Hidedeactivated();

            if (trylvlup)
            {
                trylvlup = false;
                LevelUp();
            }
        }
        else
        {
            delayedUpdateCounter--;
        }
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

                if (cameraScript.incombat)
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



    public Character CreateCopy(Character CharacterToCopy = null)
    {
        if (CharacterToCopy == null)
        {
            CharacterToCopy = UnitCharacteristics;
        }

        Character copy = new Character
        {
            name = CharacterToCopy.name,
            ID = CharacterToCopy.ID,
            stats = new BaseStats
            {
                HP = CharacterToCopy.stats.HP,
                Strength = CharacterToCopy.stats.Strength,
                Psyche = CharacterToCopy.stats.Psyche,
                Defense = CharacterToCopy.stats.Defense,
                Resistance = CharacterToCopy.stats.Resistance,
                Speed = CharacterToCopy.stats.Speed,
                Dexterity = CharacterToCopy.stats.Dexterity
            },
            AjustedStats = new BaseStats
            {
                HP = CharacterToCopy.AjustedStats.HP,
                Strength = CharacterToCopy.AjustedStats.Strength,
                Psyche = CharacterToCopy.AjustedStats.Psyche,
                Defense = CharacterToCopy.AjustedStats.Defense,
                Resistance = CharacterToCopy.AjustedStats.Resistance,
                Speed = CharacterToCopy.AjustedStats.Speed,
                Dexterity = CharacterToCopy.AjustedStats.Dexterity
            },
            level = CharacterToCopy.level,
            experience = CharacterToCopy.experience,
            affiliation = CharacterToCopy.affiliation,
            growth = new StatGrowth
            {
                HPGrowth = CharacterToCopy.growth.HPGrowth,
                StrengthGrowth = CharacterToCopy.growth.StrengthGrowth,
                PsycheGrowth = CharacterToCopy.growth.PsycheGrowth,
                DefenseGrowth = CharacterToCopy.growth.DefenseGrowth,
                ResistanceGrowth = CharacterToCopy.growth.ResistanceGrowth,
                SpeedGrowth = CharacterToCopy.growth.SpeedGrowth,
                DexterityGrowth = CharacterToCopy.growth.DexterityGrowth
            },
            currentHP = CharacterToCopy.currentHP,
            movements = CharacterToCopy.movements,
            position = CharacterToCopy.position,
            alreadyplayed = CharacterToCopy.alreadyplayed,
            alreadymoved = CharacterToCopy.alreadymoved,
            telekinesisactivated = CharacterToCopy.telekinesisactivated,
            equipmentsIDs = new List<int>(CharacterToCopy.equipmentsIDs),
            equipments = CharacterToCopy.equipments,
            UnitSkill = CharacterToCopy.UnitSkill,
            EquipedSkills = new List<int>(CharacterToCopy.EquipedSkills),
            attacksfriends = CharacterToCopy.attacksfriends,
            playableStats = new PlayableStats
            {
                MaxSkillpoints = CharacterToCopy.playableStats.MaxSkillpoints,
                deployunit = CharacterToCopy.playableStats.deployunit,
                unlocked = CharacterToCopy.playableStats.unlocked,
                protagonist = CharacterToCopy.playableStats.protagonist,
                battalion = CharacterToCopy.playableStats.battalion,
                ID = CharacterToCopy.playableStats.ID
            },
            enemyStats = new EnemyStats
            {
                classID = CharacterToCopy.enemyStats.classID,
                desiredlevel = CharacterToCopy.enemyStats.desiredlevel,
                itemtodropID = CharacterToCopy.enemyStats.itemtodropID,
                usetelekinesis = CharacterToCopy.enemyStats.usetelekinesis,
                personality = CharacterToCopy.enemyStats.personality,
                startpos = CharacterToCopy.enemyStats.startpos,
                equipments = new List<int>(CharacterToCopy.enemyStats.equipments),
                Skills = new List<int>(CharacterToCopy.enemyStats.Skills),
                Name = CharacterToCopy.enemyStats.Name,
                bossiD = CharacterToCopy.enemyStats.bossiD,
                isother = CharacterToCopy.enemyStats.isother,
                monsterStats = new MonsterStats
                {
                    size = CharacterToCopy.enemyStats.monsterStats.size,
                    ispluvial = CharacterToCopy.enemyStats.monsterStats.ispluvial,
                    ismachine = CharacterToCopy.enemyStats.monsterStats.ismachine
                },
                RemainingLifebars = CharacterToCopy.enemyStats.RemainingLifebars,
                modelID = CharacterToCopy.enemyStats.modelID,
                talkable = CharacterToCopy.enemyStats.talkable,
                talkID = CharacterToCopy.enemyStats.talkID,
                talkedto = CharacterToCopy.enemyStats.talkedto,
                SpriteID = CharacterToCopy.enemyStats.SpriteID,
                CopyID = CharacterToCopy.enemyStats.CopyID,
                PlayableSpriteID = CharacterToCopy.enemyStats.PlayableSpriteID,
            },
            currentTile = new List<GridSquareScript>(CharacterToCopy.currentTile),
            modelID = CharacterToCopy.modelID,
            Masteries = CharacterToCopy.Masteries.Select(m => new WeaponMastery
            {
                weapontype = m.weapontype,
                Exp = m.Exp,
                Level = m.Level
            }).ToList(),
            DialoguePitch = CharacterToCopy.DialoguePitch,
            TauntTurns = CharacterToCopy.TauntTurns,
            isintercepting = CharacterToCopy.isintercepting,
            TemporarySkill = CharacterToCopy.TemporarySkill,
            statusEffects = new StatusEffects
            {
                BurnTurns = CharacterToCopy.statusEffects.BurnTurns,
                StunTurns = CharacterToCopy.statusEffects.StunTurns,
                ParalyzedTurns = CharacterToCopy.statusEffects.ParalyzedTurns,
                ConcussionTunrs = CharacterToCopy.statusEffects.ConcussionTunrs,
                WeaknessTurns = CharacterToCopy.statusEffects.WeaknessTurns,
                RegenTurns = CharacterToCopy.statusEffects.RegenTurns,
                AccelerationTurns = CharacterToCopy.statusEffects.AccelerationTurns,
                PowerTurns = CharacterToCopy.statusEffects.PowerTurns
            }
        };

        return copy;
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
                transform.position += new Vector3(direction.x, 0f, direction.y) * movespeed * Time.deltaTime;
                switch (direction.x)
                {
                    case > 0.1f:
                        transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                        break;
                    case < -0.1f:
                        transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                        break;
                }
                switch (direction.y)
                {
                    case > 0.1f:
                        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                        break;
                    case < -0.1f:
                        transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                        break;
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
                if (animator.GetBool("Walk") != true)
                {
                    animator.SetBool("Walk", true);
                }

                Vector2 direction = (destination - new Vector2(transform.position.x, transform.position.z)).normalized;
                transform.position += new Vector3(direction.x, 0f, direction.y) * movespeed * Time.deltaTime;
                //if (!cameraScript.incombat)
                //{
                //    transform.forward = new Vector3(direction.x, 0f, direction.y).normalized;
                //    animator.transform.forward = new Vector3(direction.x, 0f, direction.y).normalized;
                //    transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + rotationadjust);
                //}
            }
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), destination) <= 0.1f)
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


    public void ManageFlyingWeaponPosition()
    {
        if (FlyingWeapon != null)
        {
            if (Randomnumbers == null || Randomnumbers.Count == 0)
            {
                Randomnumbers = new List<float>();
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Randomnumbers.Add((float)UnityEngine.Random.Range(0.9f, 1.1f));
                    }
                }
            }
            if (Randomnumbersindex >= Randomnumbers.Count)
            {
                Randomnumbersindex = 0;
            }
            float RandomY = Randomnumbers[Randomnumbersindex];
            Randomnumbersindex++;


            if (flyingweaponmovingup)
            {
                FlyingWeapon.transform.localPosition += new Vector3(0f, flyingmovingspeed * RandomY * Time.deltaTime, 0f);

                if (FlyingWeapon.transform.localPosition.y >= telekinesisWeaponPos.y + maxmovementrangevertical)
                {

                    flyingweaponmovingup = false;
                }
            }
            else
            {
                FlyingWeapon.transform.localPosition -= new Vector3(0f, flyingmovingspeed * RandomY * Time.deltaTime, 0f);
                if (FlyingWeapon.transform.localPosition.y <= telekinesisWeaponPos.y - maxmovementrangevertical)
                {

                    flyingweaponmovingup = true;
                }
            }

        }
    }

    private void ManageLifeBarRotation()
    {
        if (CanvasTransform.rotation != Quaternion.Euler(90f, 0f, 0f))
        {
            CanvasTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
    public void ManageLifebars()
    {

        ManageLifeBarRotation();

        //if (cameraScript.incombat || disableLifebar)
        //{
        //    if (Lifebar.gameObject.activeSelf)
        //    {
        //        Lifebar.gameObject.SetActive(false);
        //        LifebarBehind.gameObject.SetActive(false);
        //    }
        //}
        //else
        //{

        //}
        if (disableLifebar)
        {
            if (Lifebar.gameObject.activeSelf)
            {
                Lifebar.gameObject.SetActive(false);
                LBBackground.gameObject.SetActive(false);
            }
        }
        else if (!Lifebar.gameObject.activeSelf)
        {
            Lifebar.gameObject.SetActive(true);
            LBBackground.gameObject.SetActive(true);
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

        if (UnitCharacteristics.AjustedStats.HP < 0)
        {
            UnitCharacteristics.AjustedStats.HP = 0;
        }
        if (UnitCharacteristics.AjustedStats.Strength < 0)
        {
            UnitCharacteristics.AjustedStats.Strength = 0;
        }
        if (UnitCharacteristics.AjustedStats.Psyche < 0)
        {
            UnitCharacteristics.AjustedStats.Psyche = 0;
        }
        if (UnitCharacteristics.AjustedStats.Defense < 0)
        {
            UnitCharacteristics.AjustedStats.Defense = 0;
        }
        if (UnitCharacteristics.AjustedStats.Resistance < 0)
        {
            UnitCharacteristics.AjustedStats.Resistance = 0;
        }
        if (UnitCharacteristics.AjustedStats.Speed < 0)
        {
            UnitCharacteristics.AjustedStats.Speed = 0;
        }
        if (UnitCharacteristics.AjustedStats.Dexterity < 0)
        {
            UnitCharacteristics.AjustedStats.Dexterity = 0;
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
            LevelupMasteryCheck(mastery, character);
        }

    }

    public void ShowAffinityArrow()
    {
        DataScript DS = DataScript.instance;
        if (GridScript.GetUnit(GridScript.selection) == gameObject)
        {

            List<GameObject> BondedUnitsList = new List<GameObject>();
            List<int> BondedUnitIDsList = new List<int>();

            foreach (Bonds bond in DS.BondsList)
            {
                if (bond.Characters.Contains(UnitCharacteristics.ID) && bond.BondLevel > 0)
                {
                    foreach (int ID in bond.Characters)
                    {
                        if (ID != UnitCharacteristics.ID && ManhattanDistance(UnitCharacteristics, DS.PlayableCharacterList[ID]) <= 2)
                        {
                            BondedUnitIDsList.Add(ID);
                        }
                    }
                }
            }

            foreach (GameObject unitGO in TurnManger.instance.playableunitGO)
            {
                Character character = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
                if (BondedUnitIDsList.Contains(character.ID))
                {
                    unitGO.GetComponent<UnitScript>().PointArrowToTarget(UnitCharacteristics);
                }
                else
                {
                    unitGO.GetComponent<UnitScript>().HideAffinityArrow();
                }
            }

        }
        else if (GridScript.GetUnit(GridScript.selection) == null)
        {
            HideAffinityArrow();
        }
    }

    public void PointArrowToTarget(Character target)
    {

        if (!AffinityArrow.gameObject.activeSelf)
        {
            AffinityArrow.gameObject.SetActive(true);
        }

        if (target == null || AffinityArrow == null) return;

        // fixed height above ground for the arrow
        const float arrowHeight = 1f;

        // Read grid coordinates (you said movement is X and Z in world)
        Vector3 myGrid = UnitCharacteristics.currentTile[0].GridCoordinates;
        Vector3 targetGrid = target.currentTile[0].GridCoordinates;

        // Heuristic: prefer Grid.z if it contains meaningful data; otherwise use Grid.y as Z.
        float myZ = (Mathf.Abs(myGrid.z) > 0.0001f || Mathf.Abs(targetGrid.z) > 0.0001f) ? myGrid.z : myGrid.y;
        float targetZ = (Mathf.Abs(targetGrid.z) > 0.0001f || Mathf.Abs(myGrid.z) > 0.0001f) ? targetGrid.z : targetGrid.y;

        Vector3 myPos = new Vector3(myGrid.x, arrowHeight, myZ);
        Vector3 targetPos = new Vector3(targetGrid.x, arrowHeight, targetZ);

        // Put arrow halfway (or change to any rule you want)
        Vector3 midpoint = (myPos + targetPos) * 0.5f;
        AffinityArrow.transform.position = midpoint; // Y is fixed by myPos/targetPos

        AffinityArrow.transform.localPosition = new Vector3(AffinityArrow.transform.localPosition.x, AffinityArrow.transform.localPosition.y, 1f);

        // Direction on XZ plane
        Vector3 dir = targetPos - myPos;
        dir.y = 0f;


        if (dir.sqrMagnitude <= 0.0001f)
        {
            // degenerate case: same tile � don't change rotation
            return;
        }

        // Compute yaw so the arrow faces the target on X-Z plane.
        // Angle from Z axis: Atan2(dir.x, dir.z) -> degrees
        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        // Make the arrow flat (so it's seen from above) and rotate only around Y.
        // Many arrows/quads are created facing +Z. This sets X to 90 so the arrow face is horizontal.
        // If your arrow already lies flat, use Quaternion.Euler(0, yaw, 0) instead.
        AffinityArrow.transform.rotation = Quaternion.Euler(90f, yaw, 0f);


    }



    public void HideAffinityArrow()
    {
        if (AffinityArrow.gameObject.activeSelf)
        {
            AffinityArrow.gameObject.SetActive(false);
        }
    }
    private void LevelupMasteryCheck(WeaponMastery mastery, Character character)
    {
        bool levelup = false;
        switch (mastery.Level)
        {
            case 0:
                if (mastery.Exp >= DataScript.instance.MasteryforLevel0)
                {
                    levelup = true;
                }
                break;
            case 1:
                if (mastery.Exp >= DataScript.instance.MasteryforLevel1)
                {
                    levelup = true;
                }
                break;
            case 2:
                if (mastery.Exp >= DataScript.instance.MasteryforLevel2)
                {
                    levelup = true;
                }
                break;
            case 3:
                if (mastery.Exp >= DataScript.instance.MasteryforLevel3)
                {
                    levelup = true;
                }
                break;
        }

        if (levelup)
        {
            mastery.Level++;
            mastery.Exp = 0;

        }
        GetNewWeaponFromMastery(mastery, character);
    }

    public void GetNewWeaponFromMastery(WeaponMastery mastery, Character character = null)
    {
        Character Chartouse = UnitCharacteristics;
        if (character != null)
        {
            Chartouse = character;
        }
        if (mastery.Level <= 0)
        {
            return;
        }
        int oldweaponID = 0;
        int newweaponID = 0;
        foreach (int ID in Chartouse.equipmentsIDs)
        {
            if (DataScript.instance.GetWeaponFromID(ID).type.ToLower() == mastery.weapontype.ToLower())
            {
                oldweaponID = ID;
                break;
            }
        }
        foreach (equipment weapon in DataScript.instance.equipmentList)
        {
            if (weapon.type.ToLower() == mastery.weapontype.ToLower() && weapon.Grade == mastery.Level)
            {
                newweaponID = weapon.ID;
                break;
            }
        }
        if (oldweaponID != newweaponID)
        {

            if (Chartouse.equipmentsIDs.Contains(oldweaponID))
            {
                int previouspos = -1;
                previouspos = Chartouse.equipmentsIDs.IndexOf(oldweaponID);
                Chartouse.equipmentsIDs[previouspos] = newweaponID;
            }
            else
            {
                Chartouse.equipmentsIDs.Add(newweaponID);
            }
        }
        foreach (equipment equip in Chartouse.equipments)
        {
            if (equip == null || equip.type == null)
            {
                continue;
            }
            foreach (int ID in Chartouse.equipmentsIDs)
            {
                if (equip.type.ToLower() == DataScript.instance.equipmentList[ID].type.ToLower() && equip.ID != ID)
                {
                    Chartouse.equipments[Chartouse.equipments.IndexOf(equip)] = DataScript.instance.equipmentList[ID];
                    return;
                }
            }
        }
        int IDtoAdd = -1;
        foreach (int ID in Chartouse.equipmentsIDs)
        {
            bool IDnotinequipments = true;
            foreach (equipment equip in Chartouse.equipments)
            {
                if (equip == null || equip.type == null)
                {
                    continue;
                }
                if (equip.type.ToLower() == DataScript.instance.equipmentList[ID].type.ToLower())
                {
                    IDnotinequipments = false;
                    break;
                }
            }
            if (IDnotinequipments)
            {
                IDtoAdd = ID;

            }
        }
        if (IDtoAdd > 0)
        {
            foreach (equipment equip in Chartouse.equipments)
            {
                if (equip == null || equip.type == null || equip.Name == "")
                {
                    Chartouse.equipments[Chartouse.equipments.IndexOf(equip)] = DataScript.instance.equipmentList[IDtoAdd];
                    break;
                }
            }

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
        if (UnitCharacteristics.currentTile != null)
        {
            previouspos = UnitCharacteristics.currentTile;
        }
        if (GridScript == null)
        {
            GridScript = GridScript.instance;
        }
        List<GridSquareScript> oldtiles = new List<GridSquareScript>();
        GridSquareScript destTile = GridScript.GetTile(destination);
        if ((GridScript.GetUnit(destTile) == null || GridScript.GetUnit(destTile) == gameObject) && !destTile.isobstacle)
        {
            if (UnitCharacteristics.currentTile != null && UnitCharacteristics.currentTile.Count > 0)
            {
                foreach (GridSquareScript tile in UnitCharacteristics.currentTile)
                {
                    if (tile != null)
                    {
                        oldtiles.Add(tile);
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
            foreach (GridSquareScript tile in oldtiles)
            {
                tile.UpdateInsideSprite(false);
            }
        }
        if (MinimapScript == null)
        {
            MinimapScript = MinimapScript.instance;
        }
        MinimapScript.UpdateMinimap();

    }

    public void UpdateTiles(GridSquareScript destination)
    {
        UnitCharacteristics.currentTile = new List<GridSquareScript> { destination };
        if (UnitCharacteristics.enemyStats.monsterStats.size > 1)
        {
            UnitCharacteristics.currentTile.Add(GridScript.GetTile(destination.GridCoordinates + new Vector2(-1, 0)));
            UnitCharacteristics.currentTile.Add(GridScript.GetTile(destination.GridCoordinates + new Vector2(0, 1)));
            UnitCharacteristics.currentTile.Add(GridScript.GetTile(destination.GridCoordinates + new Vector2(-1, 1)));
        }
    }

    public bool isinattackanimation(Animator otheranimator = null)
    {
        Animator animatortouse = otheranimator;
        if (otheranimator != null)
        {
            animatortouse = otheranimator;
        }
        else
        {
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
            animatortouse = animator;
        }


        if (animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_slow") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Telekinesis Idle") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("PluvialIdle") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_fast_3") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("HumanWalk"))
        {
            return false;
        }
        return true;
    }

    public bool AttackAnimationAlmostdone(Animator otheranimator = null, float percentage = 1)
    {
        Animator animatortouse = otheranimator;
        if (otheranimator != null)
        {
            animatortouse = otheranimator;
        }
        else
        {
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
            animatortouse = animator;
        }


        if (animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_slow") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Telekinesis Idle") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("PluvialIdle") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_fast_3") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("HumanWalk"))
        {
            return false;
        }
        else
        {
            AnimatorStateInfo stateInfo = animatortouse.GetCurrentAnimatorStateInfo(0);
            float progress = stateInfo.normalizedTime % 1;

            if (progress >= percentage)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool isinrunanimation(Animator otheranimator = null)
    {
        Animator animatortouse = otheranimator;
        if (otheranimator != null)
        {
            animatortouse = otheranimator;
        }
        else
        {
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
            animatortouse = animator;
        }
        if (animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Armature|spider_walk_fast_3") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("HumanWalk"))
        {
            return true;
        }
        return false;
    }

    public bool isinattackresponseanimation(Animator otheranimator = null)
    {
        Animator animatortouse = otheranimator;
        if (otheranimator != null)
        {
            animatortouse = otheranimator;
        }
        else
        {
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
            animatortouse = animator;
        }
        if (animatortouse.GetCurrentAnimatorStateInfo(0).IsName("TakeDamage") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
        {
            return true;
        }
        return false;
    }

    public void PlayAttackAnimation(bool doubleattack = false, bool tripleattack = false, bool healing = false, Animator otheranimator = null)
    {

        Animator animatortouse = otheranimator;
        if (otheranimator != null)
        {
            animatortouse = otheranimator;
        }
        else
        {
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
            animatortouse = animator;
        }
        if (tripleattack)
        {
            animatortouse.SetBool("Double", false);
            animatortouse.SetBool("Triple", true);
        }
        else if (doubleattack)
        {
            animatortouse.SetBool("Double", true);
            animatortouse.SetBool("Triple", false);
        }
        else
        {
            animatortouse.SetBool("Double", false);
            animatortouse.SetBool("Triple", false);
        }
        animatortouse.SetTrigger("Attack");

        equipment weapon = GetFirstWeapon();

        animatortouse.SetBool("Slash", false);
        animatortouse.SetBool("Stab", false);
        animatortouse.SetBool("Punch", false);
        animatortouse.SetBool("GreatSword", false);
        animatortouse.SetBool("Heal", false);
        animatortouse.SetBool("Bow", false);

        if (UnitCharacteristics.telekinesisactivated && weapon.type.ToLower() != "bow")
        {
            animatortouse.SetBool("Heal", true);
        }
        else
        {
            switch (weapon.type.ToLower())
            {
                case "sword":
                    animatortouse.SetBool("Slash", true);
                    break;
                case "spear":
                    animatortouse.SetBool("Stab", true);
                    break;
                case "greatsword":
                    animatortouse.SetBool("GreatSword", true);
                    break;
                case "scythe":
                    animatortouse.SetBool("GreatSword", true);
                    break;
                case "shield":
                    animatortouse.SetBool("Punch", true);
                    break;
                case "bow":
                    animatortouse.SetBool("Bow", true);
                    break;
                case "staff":
                    if (healing)
                    {
                        animatortouse.SetBool("Heal", true);
                    }
                    else
                    {
                        animatortouse.SetBool("Stab", true);
                    }
                    break;
                case "none":
                    animatortouse.SetBool("Punch", true);
                    break;
                default:
                    animatortouse.SetBool("Slash", true);
                    break;
            }
        }



        if (animatortouse.GetCurrentAnimatorStateInfo(0).IsName("Idle") || animatortouse.GetCurrentAnimatorStateInfo(0).IsName("run"))
        {
            return;
        }
        return;
    }

    public void AddNumber(int amount, bool ishealing, string effectname, bool isbond = false)
    {
        NumberToShow newnumber = new NumberToShow();
        newnumber.number = amount;
        newnumber.ishealing = ishealing;
        newnumber.EffectName = effectname;
        newnumber.framesremaining = (int)(timeforshowingnumbers / Time.deltaTime);
        newnumber.isBond = isbond;
        damagestoshow.Add(newnumber);
    }

    private void ManageDamagenumber()
    {
        if (damagestoshow.Count > 0 && !cameraScript.incombat)
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
            else if (!damagestoshow[0].isBond)
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

    public void TriggerStatusEffectsBegOfTurn()
    {
        if (UnitCharacteristics.statusEffects.BurnTurns > 0)
        {
            UnitCharacteristics.currentHP = (int)Mathf.Max(0f, UnitCharacteristics.currentHP - UnitCharacteristics.AjustedStats.HP * 0.1f);
            UnitCharacteristics.statusEffects.BurnTurns--;
        }
        if (UnitCharacteristics.statusEffects.StunTurns > 0)
        {
            UnitCharacteristics.alreadymoved = true;
            UnitCharacteristics.alreadyplayed = true;
            UnitCharacteristics.statusEffects.StunTurns--;
        }
        if (UnitCharacteristics.statusEffects.ParalyzedTurns > 0)
        {
            UnitCharacteristics.statusEffects.ParalyzedTurns--;
        }
        if (UnitCharacteristics.statusEffects.ConcussionTunrs > 0)
        {
            UnitCharacteristics.statusEffects.ConcussionTunrs--;
            UnitCharacteristics.telekinesisactivated = false;
            if (UnitCharacteristics.statusEffects.ConcussionTunrs == 0 && UnitCharacteristics.affiliation.ToLower() != "playable" && UnitCharacteristics.enemyStats != null && UnitCharacteristics.enemyStats.usetelekinesis)
            {
                UnitCharacteristics.telekinesisactivated = true;
            }
        }
        if (UnitCharacteristics.statusEffects.WeaknessTurns > 0)
        {
            UnitCharacteristics.statusEffects.WeaknessTurns--;
        }
        if (UnitCharacteristics.statusEffects.RegenTurns > 0)
        {
            UnitCharacteristics.currentHP = (int)Mathf.Min(UnitCharacteristics.AjustedStats.HP, UnitCharacteristics.currentHP + UnitCharacteristics.AjustedStats.HP * 0.1f);
            UnitCharacteristics.statusEffects.RegenTurns--;
        }
        if (UnitCharacteristics.statusEffects.AccelerationTurns > 0)
        {
            UnitCharacteristics.statusEffects.AccelerationTurns--;
        }
        if (UnitCharacteristics.statusEffects.PowerTurns > 0)
        {
            UnitCharacteristics.statusEffects.PowerTurns--;
        }

        // add logic for visuals

    }

    public void ResetForward()
    {
        initialforward = armature.forward;
    }
    public void UpdateWeaponModel(Animator otheranimator = null, float scalemultiplier = 0.5f)
    {

        Animator animatortouse = otheranimator;
        if (otheranimator != null)
        {
            animatortouse = otheranimator;
        }
        else
        {
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
            animatortouse = animator;
        }

        if (currentequipmentmodel != null)
        {
            Destroy(currentequipmentmodel);
            FlyingWeapon = null;
        }
        if (GetFirstWeapon().Grade != 0 && GetFirstWeapon().Currentuses != 0)
        {
            equipmentmodel equipmentmodel = GetFirstWeapon().equipmentmodel;
            if (equipmentmodel.Model != null)
            {
                currentequipmentmodel = Instantiate(equipmentmodel.Model);
                currentequipmentmodel.transform.localScale = Vector3.one * scalemultiplier;
                foreach (ModelInfo modelInfo in ModelList)
                {
                    if (modelInfo.active)
                    {
                        if (GetFirstWeapon().type.ToLower() == "bow")
                        {
                            currentequipmentmodel.transform.SetParent(modelInfo.Lefthandbone);
                        }
                        else if (UnitCharacteristics.telekinesisactivated)
                        {
                            FlyingWeapon = currentequipmentmodel;
                            currentequipmentmodel.transform.SetParent(animatortouse.transform);
                        }
                        else if (GetFirstWeapon().type.ToLower() != "machine")
                        {
                            currentequipmentmodel.transform.SetParent(modelInfo.handbone);
                        }
                    }
                }


                if (UnitCharacteristics.telekinesisactivated && GetFirstWeapon().type.ToLower() != "bow")
                {
                    currentequipmentmodel.transform.localPosition = telekinesisWeaponPos;
                    currentequipmentmodel.transform.localRotation = Quaternion.Euler(telekinesisWeaponRot);
                }
                else
                {
                    currentequipmentmodel.transform.localPosition = equipmentmodel.localposition;
                    currentequipmentmodel.transform.localRotation = Quaternion.Euler(equipmentmodel.localrotation);
                    if (UnitCharacteristics.modelID == 2)
                    {
                        currentequipmentmodel.transform.localPosition = Vector3.zero;
                        currentequipmentmodel.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 180));
                    }
                }

            }

        }

        UpdateWeaponIcon(GetFirstWeapon(), UnitCharacteristics.telekinesisactivated);

    }

    private void UpdateWeaponIcon(equipment weapon, bool telekinesis)
    {
        switch (weapon.type.ToLower())
        {
            case ("sword"):
                WeaponImage.sprite = SwordSprite;
                break;
            case ("spear"):
                WeaponImage.sprite = SpearSprite;
                break;
            case ("greatsword"):
                WeaponImage.sprite = GreatSwordSprite;
                break;
            case ("bow"):
                WeaponImage.sprite = BowSprite;
                break;
            case ("scythe"):
                WeaponImage.sprite = ScytheSprite;
                break;
            case ("shield"):
                WeaponImage.sprite = ShieldSprite;
                break;
            case ("staff"):
                WeaponImage.sprite = StaffSprite;
                break;
            default:
                WeaponImage.sprite = BareHandSprite;
                break;
        }

        if (telekinesis && TelekinesisImage.color != Color.white)
        {
            TelekinesisImage.color = Color.white;
        }
        else if (!telekinesis && TelekinesisImage.color == Color.white)
        {
            TelekinesisImage.color = Color.clear;
        }
    }

    void HealthChangedHandler(int newHealth)
    {
        ManageLifebars();
    }

    void PlayedChangedHandler(bool newPlayed)
    {
        if (ActiveModel != null && UnitCharacteristics.currentTile[0].activated)
        {
            if (newPlayed)
            {
                if (ActiveModel.GetComponentInChildren<Renderer>().material.GetColor("_BaseColor") != Color.grey)
                {
                    InitialCharacterColor = ActiveModel.GetComponentInChildren<Renderer>().material.GetColor("_BaseColor");
                    ActiveModel.GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", Color.grey);
                }
            }
            else
            {
                if (ActiveModel.GetComponentInChildren<Renderer>().material.GetColor("_BaseColor") != InitialCharacterColor)
                {
                    ActiveModel.GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", InitialCharacterColor); ;
                }
            }
        }
    }
    public void RetreatTrigger() // Effect of Retreat or Verso
    {
        AttackTurnScript.DeathCleanup();

        if (GetSkill(1)) //Retreat
        {
            AddNumber(0, true, "Retreat");
            UnitCharacteristics.alreadymoved = false;

            GridScript gridScript = GridScript.instance;
            gridScript.InitializeGOList();
            gridScript.selection = gridScript.GetTile(UnitCharacteristics.position);
            gridScript.ShowMovement();

            gridScript.lockedmovementtiles = gridScript.movementtiles;
            gridScript.lockselection = true;
            ActionManager.instance.frameswherenotlock = 0;
            ActionManager.instance.framestoskip = 10;
            ActionManager.instance.currentcharacter = gameObject;
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
                GridScript gridScript = GridScript.instance;
                gridScript.InitializeGOList();
                gridScript.selection = gridScript.GetTile(UnitCharacteristics.position);
                gridScript.ShowLimitedMovementOfUnit(gameObject, remainingMovements);

                gridScript.lockedmovementtiles = gridScript.movementtiles;
                gridScript.lockselection = true;
                ActionManager.instance.frameswherenotlock = 0;
                ActionManager.instance.framestoskip = 10;
                ActionManager.instance.currentcharacter = gameObject;
            }



        }
    }

    public bool GetSkill(int SkillID)
    {
        if (UnitCharacteristics.EquipedSkills.Contains(SkillID) || UnitCharacteristics.UnitSkill == SkillID || UnitCharacteristics.UnitSkill == SkillID)
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

            ClassInfo classtoapply = DataScript.instance.ClassList[UnitCharacteristics.enemyStats.classID];
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
            UnitCharacteristics.movements = classtoapply.movements;
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
            if (CanvasTransform.position != new Vector3(CanvasTransform.position.x, canvaselevation + UnitCharacteristics.enemyStats.monsterStats.size, CanvasTransform.position.z))
            {
                CanvasTransform.position = new Vector3(CanvasTransform.position.x, canvaselevation + UnitCharacteristics.enemyStats.monsterStats.size, CanvasTransform.position.z);
            }
        }
    }

    private void Hidedeactivated()
    {
        bool checkifonactivated = CheckIfOnActivated();
        if (CanvasTransform.gameObject.activeSelf != checkifonactivated)
        {
            animator.transform.gameObject.SetActive(CheckIfOnActivated());
            CanvasTransform.gameObject.SetActive(CheckIfOnActivated());
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
            foreach (MeshRenderer Renderer in childrenmeshrender)
            {
                //Renderer.renderingLayerMask = 0;
            }

        }
        else
        {
            foreach (MeshRenderer Renderer in childrenmeshrender)
            {
                if (Renderer != null)
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

        if (cameraScript.incombat)
        {
            newcolor.a = 0f;
            if (cameraScript.fighter1 == gameObject || cameraScript.fighter2 == gameObject)
            {
                newcolor.a = 1f;
            }
        }


        GetComponent<MeshRenderer>().material.color = newcolor;
    }


    public StatGrowth GetGrowthModifications()
    {
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
        //Crystal Heart, Guardian Spirit, Hero's Heir
        if (GetSkill(57) || GetSkill(72) || GetSkill(73))
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

        int bonussize = 20;
        if (GetFirstWeapon() != null && GetFirstWeapon().type != null)
        {
            switch (GetFirstWeapon().type.ToLower())
            {
                case "sword":
                    GrowthtoApply.SpeedGrowth += bonussize;
                    GrowthtoApply.DexterityGrowth += bonussize;
                    GrowthtoApply.HPGrowth -= bonussize;
                    break;
                case "spear":
                    GrowthtoApply.StrengthGrowth += bonussize;
                    GrowthtoApply.PsycheGrowth += bonussize;
                    GrowthtoApply.ResistanceGrowth -= bonussize;
                    break;
                case "greatsword":
                    GrowthtoApply.StrengthGrowth += bonussize;
                    GrowthtoApply.DefenseGrowth += bonussize;
                    GrowthtoApply.SpeedGrowth -= bonussize;
                    break;
                case "bow":
                    GrowthtoApply.StrengthGrowth += bonussize;
                    GrowthtoApply.DexterityGrowth += bonussize;
                    GrowthtoApply.DefenseGrowth -= bonussize;
                    break;
                case "scythe":
                    GrowthtoApply.PsycheGrowth += bonussize;
                    GrowthtoApply.DexterityGrowth += bonussize;
                    GrowthtoApply.SpeedGrowth -= bonussize;
                    break;
                case "shield":
                    GrowthtoApply.HPGrowth += bonussize;
                    GrowthtoApply.DefenseGrowth += bonussize;
                    GrowthtoApply.PsycheGrowth -= bonussize;
                    break;
                case "staff":
                    GrowthtoApply.PsycheGrowth += bonussize;
                    GrowthtoApply.ResistanceGrowth += bonussize;
                    GrowthtoApply.StrengthGrowth -= bonussize;
                    break;
            }
        }


        return GrowthtoApply;
    }

    public void CalculateSkillPoints(Character Character = null)
    {
        if (Character == null)
        {
            Character = UnitCharacteristics;
        }
        int newmaxskillpoints = 5;

        newmaxskillpoints += Character.level;

        Character.playableStats.MaxSkillpoints = newmaxskillpoints;

    }

    public List<int> LevelUp()
    {

        if (UnitCharacteristics.affiliation == "playable")
        {
            fixedgrowth = SaveManager.instance.Options.FixedGrowth;
        }

        BaseStats statsbeforelevelup = new BaseStats() { HP = UnitCharacteristics.stats.HP, Strength = UnitCharacteristics.stats.Strength, Psyche = UnitCharacteristics.stats.Psyche, Defense = UnitCharacteristics.stats.Defense, Resistance = UnitCharacteristics.stats.Resistance, Speed = UnitCharacteristics.stats.Speed, Dexterity = UnitCharacteristics.stats.Dexterity };
        previousStats = statsbeforelevelup;

        List<int> lvlupresult = new List<int>();
        StatGrowth GrowthtoApply = GetGrowthModifications();



        UnitCharacteristics.experience -= 100;
        if (UnitCharacteristics.experience < 0)
        {
            UnitCharacteristics.experience = 0;
        }
        UnitCharacteristics.level += 1;

        CalculateSkillPoints();

        if (fixedgrowth)
        {
            float oldHP = UnitCharacteristics.stats.HP;
            UnitCharacteristics.stats.HP += (GrowthtoApply.HPGrowth / 100f);

            if ((int)oldHP < (int)(UnitCharacteristics.stats.HP))
            {
                lvlupresult.Add(1);
            }
            else if ((int)oldHP > (int)(UnitCharacteristics.stats.HP))
            {
                lvlupresult.Add(-1);
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
            else if ((int)oldStrength > (int)(UnitCharacteristics.stats.Strength))
            {
                lvlupresult.Add(-1);
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
            else if ((int)oldPsyche > (int)(UnitCharacteristics.stats.Psyche))
            {
                lvlupresult.Add(-1);
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
            else if ((int)oldDefense > (int)(UnitCharacteristics.stats.Defense))
            {
                lvlupresult.Add(-1);
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
            else if ((int)oldResistance > (int)(UnitCharacteristics.stats.Resistance))
            {
                lvlupresult.Add(-1);
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
            else if ((int)oldSpeed > (int)(UnitCharacteristics.stats.Speed))
            {
                lvlupresult.Add(-1);
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
            else if ((int)oldDexterity > (int)(UnitCharacteristics.stats.Dexterity))
            {
                lvlupresult.Add(-1);
            }
            else
            {
                lvlupresult.Add(0);
            }

        }
        else
        {

            RandomScript.RandomLevelValues levelValues = GetComponent<RandomScript>().GetLevelUpRandomValues();


            float HPgain = GetLevelUpStatsChange(GrowthtoApply.HPGrowth, levelValues.HPRandomValue);
            UnitCharacteristics.stats.HP += HPgain;
            lvlupresult.Add((int)HPgain);

            float Strengthgain = GetLevelUpStatsChange(GrowthtoApply.StrengthGrowth, levelValues.StrengthRandomValue);
            UnitCharacteristics.stats.Strength += Strengthgain;
            lvlupresult.Add((int)Strengthgain);

            float Psychegain = GetLevelUpStatsChange(GrowthtoApply.PsycheGrowth, levelValues.PsycheRandomValue);
            UnitCharacteristics.stats.Psyche += Psychegain;
            lvlupresult.Add((int)Psychegain);

            float Defensegain = GetLevelUpStatsChange(GrowthtoApply.DefenseGrowth, levelValues.DefenseRandomValue);
            UnitCharacteristics.stats.Defense += Defensegain;
            lvlupresult.Add((int)Defensegain);

            float Resistancegain = GetLevelUpStatsChange(GrowthtoApply.ResistanceGrowth, levelValues.ResistanceRandomValue);
            UnitCharacteristics.stats.Resistance += Resistancegain;
            lvlupresult.Add((int)Resistancegain);

            float Speedgain = GetLevelUpStatsChange(GrowthtoApply.SpeedGrowth, levelValues.SpeedRandomValue);
            UnitCharacteristics.stats.Speed += Speedgain;
            lvlupresult.Add((int)Speedgain);


            float Dexteritygain = GetLevelUpStatsChange(GrowthtoApply.DexterityGrowth, levelValues.DexterityRandomValue);
            UnitCharacteristics.stats.Dexterity += Dexteritygain;
            lvlupresult.Add((int)Dexteritygain);
        }

        string levelupstring = UnitCharacteristics.name + "  levelup : ";
        foreach (int level in lvlupresult)
        {
            levelupstring += level + " ";
        }

        bool levelupwasnull = true;
        foreach (int statincrease in lvlupresult)
        {
            if (statincrease != 0)
            {
                levelupwasnull = false;
                break;
            }
        }

        if (levelupwasnull)
        {
            UnitCharacteristics.stats.HP = (int)UnitCharacteristics.stats.HP + 1;
            lvlupresult[0] = 1;
        }

        calculateStats();
        return lvlupresult;
    }

    public float GetLevelUpStatsChange(float growth, int randomvalue)
    {
        float gain = 0f;
        if (growth > 100)
        {
            gain = 1 + GetLevelUpStatsChange(growth - 100f, randomvalue);
        }
        else if (growth < 0)
        {
            if (randomvalue <= Mathf.Abs(growth))
            {
                gain--;
            }
        }
        else
        {
            if (randomvalue <= growth)
            {
                gain++;
            }
        }
        return gain;
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
    public (int, bool) GetRangeAndMele(equipment weapon = null)
    {
        equipment firstweapon = GetFirstWeapon();
        if (weapon != null)
        {
            firstweapon = weapon;
        }

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
        List<GameObject> allunitsGO = TurnManger.instance.playableunitGO;
        List<Character> allunits = TurnManger.instance.playableunit;
        foreach (GameObject characterGO in allunitsGO)
        {
            Character character = characterGO.GetComponent<UnitScript>().UnitCharacteristics;
            if (ManhattanDistance(UnitCharacteristics, character) == 1 && character.playableStats.battalion.ToLower() == "gale" && character.affiliation == UnitCharacteristics.affiliation)
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
            if (UnitCharacteristics.playableStats.battalion.ToLower() == "zack")
            {
                foreach (Character character in allunits)
                {
                    if (character.playableStats.battalion.ToLower() == "zack" && character != UnitCharacteristics)
                    {
                        statbonuses.Hit += 1;
                        statbonuses.Crit += 1;
                    }
                }
            }
            //Gale main effect
            if (UnitCharacteristics.playableStats.battalion.ToLower() == "gale")
            {
                foreach (Character character in allunits)
                {
                    if (character.playableStats.battalion.ToLower() == "gale" && character != UnitCharacteristics)
                    {
                        statbonuses.Defense += (int)character.AjustedStats.Defense / 20;
                        statbonuses.Strength += (int)character.AjustedStats.Strength / 20;
                    }
                }
            }
            //Kira main effect
            if (UnitCharacteristics.playableStats.battalion.ToLower() == "kira")
            {
                foreach (Character character in allunits)
                {
                    if (character.playableStats.battalion.ToLower() == "kira" && character != UnitCharacteristics)
                    {
                        statbonuses.Psyche += (int)character.AjustedStats.Psyche / 20;

                    }
                }
            }
        }
        else
        {
            //Zack Side Effect
            if (UnitCharacteristics.playableStats.battalion.ToLower() == "zack")
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

    public AllStatsSkillBonus GetBondCombatBonus()
    {
        AllStatsSkillBonus statbonuses = new AllStatsSkillBonus();

        TurnManger TM = TurnManger.instance;

        List<Bonds> pertinentbonds = new List<Bonds>();

        foreach (Bonds bond in DataScript.instance.BondsList)
        {
            if (bond.Characters.Contains(UnitCharacteristics.ID))
            {
                pertinentbonds.Add(bond);
            }
        }

        foreach (GameObject otherunitGO in TM.playableunitGO)
        {
            Character otherunit = otherunitGO.GetComponent<UnitScript>().UnitCharacteristics;
            if (otherunit.currentHP > 0 && otherunit != UnitCharacteristics && ManhattanDistance(UnitCharacteristics, otherunit) <= 2)
            {
                foreach (Bonds bond in pertinentbonds)
                {
                    if (bond.Characters.Contains(otherunit.ID))
                    {
                        statbonuses.Crit += 1 * bond.BondLevel;
                        statbonuses.Hit += 3 * bond.BondLevel;
                        statbonuses.Dodge += 2 * bond.BondLevel;
                        if (GetSkill(67)) // Friends are power
                        {
                            statbonuses.Crit += 1 * bond.BondLevel;
                            statbonuses.Hit += 3 * bond.BondLevel;
                            statbonuses.Dodge += 2 * bond.BondLevel;
                        }
                        if (otherunitGO.GetComponent<UnitScript>().GetSkill(67)) // Friends are power
                        {
                            statbonuses.Crit += 1 * bond.BondLevel;
                            statbonuses.Hit += 3 * bond.BondLevel;
                            statbonuses.Dodge += 2 * bond.BondLevel;
                        }
                    }
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
                activelist = TurnManger.instance.playableunit;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = TurnManger.instance.enemyunit;
            }
            else
            {
                activelist = TurnManger.instance.otherunits;
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
                if (enemy.GetComponent<UnitScript>().UnitCharacteristics.enemyStats != null && enemy.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD > 0)
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
                activelist = TurnManger.instance.playableunitGO;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = TurnManger.instance.enemyunitGO;
            }
            else
            {
                activelist = TurnManger.instance.otherunitsGO;
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
                activelist = TurnManger.instance.playableunit;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = TurnManger.instance.enemyunit;
            }
            else
            {
                activelist = TurnManger.instance.otherunits;
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
                statbonuses.FixedDamageBonus += ((int)UnitCharacteristics.AjustedStats.HP - UnitCharacteristics.currentHP);
            }

        }
        //KillingSpree
        if (GetSkill(29))
        {
            statbonuses.Strength += 1 * unitkilled;
            statbonuses.Psyche += 1 * unitkilled;
            statbonuses.Resistance += 1 * unitkilled;
            statbonuses.Defense += 1 * unitkilled;
            statbonuses.Speed += 1 * unitkilled;
            statbonuses.Dexterity += 1 * unitkilled;
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
                activelist = TurnManger.instance.playableunit;
            }
            else if (UnitCharacteristics.affiliation == "enemy")
            {
                activelist = TurnManger.instance.enemyunit;
            }
            else
            {
                activelist = TurnManger.instance.otherunits;
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
                if (tile.RemainingRainTurns > 0 || tile.type.ToLower() == "water" || tile.type.ToLower() == "medicinalwater")
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

        //Showdown (this unit)
        if (GetSkill(64))
        {
            statbonuses.Crit += 25;
        }

        //Showdown (enemy unit)
        if (enemy != null && enemy.GetComponent<UnitScript>().GetSkill(64))
        {
            statbonuses.Crit += 25;
        }

        //Fair Play (this unit)
        if (GetSkill(65))
        {
            statbonuses.Crit -= 25;
        }

        //Fair Play (enemy unit)
        if (enemy != null && enemy.GetComponent<UnitScript>().GetSkill(65))
        {
            statbonuses.Crit -= 25;
        }

        //Violent Misuse
        if (GetSkill(68))
        {
            statbonuses.FixedDamageBonus += GetFirstWeapon().BaseDamage / 2;
        }

        //Eye of Shining Justice
        if (GetSkill(74))
        {
            statbonuses.FixedDamageBonus += 100;
            statbonuses.FixedDamageReduction += 100;
        }

        //Caelum General
        if (GetSkill(77))
        {
            statbonuses.DamageReduction += 50;
        }

        //Overly Cautious
        if (GetSkill(78))
        {
            statbonuses.Crit -= 30;
            statbonuses.Dodge += 20;
        }

        //Brawl Master
        if (GetSkill(80))
        {
            statbonuses.Hit += (int)(UnitCharacteristics.AjustedStats.Dexterity + statbonuses.Dexterity) * 5;
            statbonuses.PhysDamage += (int)(UnitCharacteristics.AjustedStats.Strength + statbonuses.Strength) / 4;
        }



        // Battalion Bonuses
        AllStatsSkillBonus battalionskillbonus = GetBattalionCombatBonus();


        if (UnitCharacteristics.affiliation.ToLower() == "playable")
        {
            AllStatsSkillBonus bondbonus = GetBondCombatBonus();
            statbonuses.Hit += bondbonus.Hit;
            statbonuses.Crit += bondbonus.Crit;
            statbonuses.Dodge += bondbonus.Dodge;
        }


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

        // Status Effect Bonus/Malus

        if (UnitCharacteristics.statusEffects.WeaknessTurns > 0)
        {
            statbonuses.Strength -= 5;
            statbonuses.Psyche -= 5;
            statbonuses.Defense -= 5;
            statbonuses.Resistance -= 5;
            statbonuses.Dexterity -= 5;
            statbonuses.Speed -= 5;
        }

        if (UnitCharacteristics.statusEffects.PowerTurns > 0)
        {
            statbonuses.Strength += 5;
            statbonuses.Psyche += 5;
            statbonuses.Defense += 5;
            statbonuses.Resistance += 5;
            statbonuses.Dexterity += 5;
            statbonuses.Speed += 5;
        }

        return statbonuses;
    }

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }

    public List<Skill> GetCommands()
    {
        List<Skill> Commands = new List<Skill>();
        if (UnitCharacteristics.UnitSkill != 0)
        {
            if (DataScript.instance.SkillList[UnitCharacteristics.UnitSkill].IsCommand)
            {
                Commands.Add(DataScript.instance.SkillList[UnitCharacteristics.UnitSkill]);
            }
            foreach (int SkillID in UnitCharacteristics.EquipedSkills)
            {
                if (DataScript.instance.SkillList[SkillID].IsCommand && SkillID != 0)
                {
                    Commands.Add(DataScript.instance.SkillList[SkillID]);
                }
            }
        }
        return Commands;
    }

    public List<GameObject> GetSpectialInteraction()
    {
        List<GameObject> interactables = new List<GameObject>();
        GameObject newtile = GridScript.GetTileGO(UnitCharacteristics.currentTile[0].GridCoordinates + new Vector2(1, 0));
        if (newtile != null)
        {
            GameObject newunit = GridScript.GetUnit(newtile.GetComponent<GridSquareScript>());
            if (newunit != null)
            {
                if (newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkable && !newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkedto)
                {
                    interactables.Add(newunit);
                }
            }
            if (newtile.GetComponent<GridSquareScript>().Mechanism != null && !newtile.GetComponent<GridSquareScript>().Mechanism.isactivated && newtile.GetComponent<GridSquareScript>().Mechanism.type == 2)
            {
                interactables.Add(newtile);
            }
        }

        newtile = GridScript.GetTileGO(UnitCharacteristics.currentTile[0].GridCoordinates + new Vector2(-1, 0));
        if (newtile != null)
        {
            GameObject newunit = GridScript.GetUnit(newtile.GetComponent<GridSquareScript>());
            if (newunit != null)
            {
                if (newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkable && !newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkedto)
                {
                    interactables.Add(newunit);
                }
            }
            if (newtile.GetComponent<GridSquareScript>().Mechanism != null && !newtile.GetComponent<GridSquareScript>().Mechanism.isactivated && newtile.GetComponent<GridSquareScript>().Mechanism.type == 2)
            {
                interactables.Add(newtile);
            }
        }

        newtile = GridScript.GetTileGO(UnitCharacteristics.currentTile[0].GridCoordinates + new Vector2(0, 1));
        if (newtile != null)
        {
            GameObject newunit = GridScript.GetUnit(newtile.GetComponent<GridSquareScript>());
            if (newunit != null)
            {
                if (newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkable && !newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkedto)
                {
                    interactables.Add(newunit);
                }
            }
            if (newtile.GetComponent<GridSquareScript>().Mechanism != null && !newtile.GetComponent<GridSquareScript>().Mechanism.isactivated && newtile.GetComponent<GridSquareScript>().Mechanism.type == 2)
            {
                interactables.Add(newtile);
            }
        }

        newtile = GridScript.GetTileGO(UnitCharacteristics.currentTile[0].GridCoordinates + new Vector2(0, -1));
        if (newtile != null)
        {
            GameObject newunit = GridScript.GetUnit(newtile.GetComponent<GridSquareScript>());
            if (newunit != null)
            {
                if (newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkable && !newunit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.talkedto)
                {
                    interactables.Add(newunit);
                }
            }
            if (newtile.GetComponent<GridSquareScript>().Mechanism != null && !newtile.GetComponent<GridSquareScript>().Mechanism.isactivated && newtile.GetComponent<GridSquareScript>().Mechanism.type == 2)
            {
                interactables.Add(newtile);
            }
        }

        //check if there are any bosses
        foreach (GameObject unit in GridScript.allunitGOs)
        {
            if (unit.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.bossiD > 0 && UnitCharacteristics.playableStats.protagonist && UnitCharacteristics.currentTile != null && UnitCharacteristics.currentTile[0] != null && UnitCharacteristics.currentTile[0].isbossAttackTile)
            {
                interactables.Add(unit);
                break;
            }
        }


        return interactables;

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
