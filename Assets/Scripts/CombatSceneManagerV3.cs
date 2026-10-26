using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnitScript;

public class CombatSceneManagerV3 : MonoBehaviour
{

    public static CombatSceneManagerV3 instance;

    [SerializeField] private GameObject EventSystem;

    [Header("Debug Variables")]
    [SerializeField] private int TestSceneID;
    [SerializeField] private int TestAttackerID;
    [SerializeField] private int TestDefenderID;

    private CombatSceneLoader _CombatSceneLoader;
    [Serializable]
    public class BattleSummary
    {
        public Character AttackerCopyBeforeCombat;
        public Character DefenderCopyBeforeCombat;
        public OffensiveSummary AttackerToDefender = null;
        public OffensiveSummary DefenderToAttacker = null;
        public int expearned;
        public List<int> LevelupBonuses;
        public BattleSummary(Character attackerCopyBeforeCombat, Character defenderCopyBeforeCombat, List<int> AttackerHits, List<int> AttackerDamage, List<int> AttackerToDefenderAttackerSkillTriggers, List<int> AttackerToDefenderDefenderSkillTriggers, List<int> Defenderhits, List<int> Defenderdamage, List<int> DefenderToAttackerAttackerSkillTriggers, List<int> DefenderToAttackerDefenderSkillTriggers, int exp, List<int> Levelup)
        {
            AttackerCopyBeforeCombat = attackerCopyBeforeCombat;
            DefenderCopyBeforeCombat = defenderCopyBeforeCombat;
            expearned = exp;
            LevelupBonuses = Levelup;
            AttackerToDefender = new OffensiveSummary(AttackerCopyBeforeCombat, DefenderCopyBeforeCombat, AttackerHits, AttackerDamage, AttackerToDefenderAttackerSkillTriggers, AttackerToDefenderDefenderSkillTriggers);
            DefenderToAttacker = new OffensiveSummary(DefenderCopyBeforeCombat, AttackerCopyBeforeCombat, Defenderhits, Defenderdamage, DefenderToAttackerDefenderSkillTriggers, DefenderToAttackerAttackerSkillTriggers);
        }

    }

    [Serializable]
    public class OffensiveSummary
    {
        [Header("CharacterA Attacked Character B")]
        public Character CharacterA;
        public Character CharacterB;
        public List<int> TypeOfHits; // -1 for Dodges, 1 for hits, 2 for crits
        public List<int> Damage;
        public List<int> CharacterASkillTriggers;
        public List<int> CharacterBSkillTriggers;

        public OffensiveSummary(Character characterA, Character characterB, List<int> typeOfHits, List<int> damage, List<int> characterASkillTriggers, List<int> characterBSkillTriggers)
        {
            CharacterA = characterA;
            CharacterB = characterB;
            TypeOfHits = typeOfHits;
            Damage = damage;
            CharacterASkillTriggers = characterASkillTriggers;
            CharacterBSkillTriggers = characterBSkillTriggers;
        }
    }



    [Serializable]
    public class CombatEnvirnoment
    {
        public GameObject Model;
        public Vector3 position;
        public Vector3 rotation;
        public List<int> ChapterInWhichToUse;
        public bool isSideStory;
    }


    [Serializable]
    public class CamData
    {
        public string Name;
        public CinemachineCamera Camera;
        public Vector3 DefaultPosition;
        public Vector3 DefaultLocalPosition;
        public Vector3 DefaultRotation;
        public Transform Holder;

        public CamData(Transform NewHolder)
        {
            Holder = NewHolder;
            Camera = Holder.GetComponentInChildren<CinemachineCamera>();
            Name = Camera.transform.name;
            DefaultPosition = NewHolder.localPosition;
            DefaultLocalPosition = Camera.transform.localPosition;
            DefaultRotation = NewHolder.localRotation.eulerAngles;
        }
    }


    [SerializeField] private List<CombatEnvirnoment> EnvirnomentList;
    [SerializeField] private BattleSummary _BattleSummary;
    [SerializeField] private FreezeFrameCapture LevelUpFreezeFrame;
    [Header("Unit Variables")]
    [SerializeField] private GameObject AttackerGO;
    private UnitScript AttackerUS;
    [SerializeField] private Vector3 AttackerStartpos;
    [SerializeField] private GameObject DefenderGO;
    private UnitScript DefenderUS;
    [SerializeField] private Vector3 DefenderStartpos;
    [SerializeField] private GameObject TextObjectPrefab;

    [Header("Cam Variables")]

    [SerializeField] private CamData AttackerRunCameraData;
    [SerializeField] private Transform AttackerRunCameraHolder;
    [SerializeField] private CamData FightCameraData;
    [SerializeField] private Transform FighterCamHolder;
    [SerializeField] private CamData AttackerVictoryCameraData;
    [SerializeField] private Transform AttackerVictoryCamHolder;
    [SerializeField] private CamData DefenderVictoryCameraData;
    [SerializeField] private Transform DefenderVictoryCamHolder;
    [SerializeField] private Vector3 DotweenRotation;
    [SerializeField] private float DotweenDuration;

    [Header("UI Variables")]
    [SerializeField] private TextMeshProUGUI AttackerName;
    [SerializeField] private TextMeshProUGUI DefenderName;
    [SerializeField] private TextMeshProUGUI AttackerStats;
    [SerializeField] private TextMeshProUGUI DefenderStats;
    [SerializeField] private TextMeshProUGUI AttackerHPText;
    [SerializeField] private TextMeshProUGUI DefenderHPText;
    [SerializeField] private Image AttackerLifebar;
    [SerializeField] private Image DefenderLifebar;
    [SerializeField] private Image ExpBarImage;
    [SerializeField] private TextMeshProUGUI ExpBarAmount;
    [SerializeField] private GameObject ExpBarHolder;
    [SerializeField] private List<TextMeshProUGUI> AttackerSkillNames;
    [SerializeField] private List<SkillIconScript> AttackerSkillIcons;
    [SerializeField] private Transform AttackerSkillHolder;
    [SerializeField] private List<TextMeshProUGUI> DefenderSkillNames;
    [SerializeField] private List<SkillIconScript> DefenderSkillIcons;
    [SerializeField] private Transform DefenderSkillHolder;

    [Header("Unwinding Variables")]
    private Tween currentCamTween;
    [SerializeField] private float TimeToWaitBeforeActionStarts;
    [SerializeField] private float DistanceForMelee;
    [SerializeField] private float TimeToWalkToEnnemies;
    [SerializeField] private float TimeBetweenAttackCombos;
    [SerializeField] private float LifeBarPercentageChangePerSecond = 1f;
    [SerializeField] private float ExpPointPerSec = 25f;
    [SerializeField] private bool distributingExp;
    [SerializeField] private int targetExp;
    private float currentlydistributedexp;
    private UnitScript CharacterEarningExp;


    private GameObject currentenv;




    private void Start()
    {
        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
        }

    }

    private void Update()
    {
        ChangeLifebarFilling();
        IncreaseExpBar();
        ChangeFightCameraPosition();
    }

    private void IncreaseExpBar()
    {
        if (distributingExp)
        {

            if (!ExpBarHolder.gameObject.activeSelf)
            {
                ExpBarHolder.gameObject.SetActive(true);
            }

            currentlydistributedexp += ExpPointPerSec * Time.deltaTime;
            if (currentlydistributedexp >= targetExp)
            {
                currentlydistributedexp = targetExp;
            }

            SetupExpBar(currentlydistributedexp);

            if (currentlydistributedexp >= targetExp)
            {
                distributingExp = false;
                if (targetExp >= 100)
                {
                    CharacterEarningExp.UnitCharacteristics.level++;
                    LevelUpFreezeFrame._CombatSceneManagerV3 = this;
                    LevelUpFreezeFrame.PlayFullAnimation(CharacterEarningExp.UnitCharacteristics, _BattleSummary.LevelupBonuses);
                }
                else
                {
                    StartCoroutine(CloseCombatScene(0.5f));
                }
            }

        }
    }

    private void ChangeFightCameraPosition()
    {
        if (AttackerUS == null || AttackerUS.ActiveModel == null || DefenderUS == null || DefenderUS.ActiveModel == null)
        {
            return;
        }
        Vector3 newpos = (AttackerUS.ActiveModel.transform.position + DefenderUS.ActiveModel.transform.position) / 2f;
        FighterCamHolder.position = new Vector3(newpos.x, FighterCamHolder.position.y, FighterCamHolder.position.z);
        Transform FightCamTransform = FightCameraData.Camera.transform;
        Vector3 NewLocalPos = FightCameraData.DefaultLocalPosition * (0.5f + 0.5f * Vector3.Distance(AttackerUS.ActiveModel.transform.position, DefenderUS.ActiveModel.transform.position) / 10f);
        FightCamTransform.localPosition = NewLocalPos;
    }

    private void SetupExpBar(float currentEXP)
    {
        ExpBarImage.fillAmount = (float)(currentEXP) / 100f;
        ExpBarAmount.text = (int)currentEXP + "";
    }

    private void ChangeLifebarFilling()
    {
        if (AttackerUS == null || DefenderUS == null)
        {
            return;
        }
        float attackerHPRatio = (float)AttackerUS.UnitCharacteristics.currentHP / (float)AttackerUS.UnitCharacteristics.AjustedStats.HP;

        if (Mathf.Abs(AttackerLifebar.fillAmount - attackerHPRatio) > 0.05f)
        {
            float fillChange = (float)LifeBarPercentageChangePerSecond * (float)AttackerUS.UnitCharacteristics.AjustedStats.HP * Time.deltaTime;
            if (AttackerLifebar.fillAmount < attackerHPRatio)
            {

                AttackerLifebar.fillAmount += fillChange;
            }
            else
            {
                AttackerLifebar.fillAmount -= fillChange;
            }
        }
        else
        {
            AttackerLifebar.fillAmount = attackerHPRatio;
        }

        AttackerHPText.text = (int)(AttackerLifebar.fillAmount * AttackerUS.UnitCharacteristics.AjustedStats.HP) + "";

        float defenderHPRatio = (float)DefenderUS.UnitCharacteristics.currentHP / (float)DefenderUS.UnitCharacteristics.AjustedStats.HP;
        if (Mathf.Abs(DefenderLifebar.fillAmount - defenderHPRatio) > 0.01f)
        {
            float fillChange = (float)LifeBarPercentageChangePerSecond * (float)DefenderUS.UnitCharacteristics.AjustedStats.HP * Time.deltaTime;
            if (DefenderLifebar.fillAmount < defenderHPRatio)
            {

                DefenderLifebar.fillAmount += fillChange;
            }
            else
            {
                DefenderLifebar.fillAmount -= fillChange;
            }
        }
        else
        {
            DefenderLifebar.fillAmount = defenderHPRatio;
        }

        DefenderHPText.text = (int)(DefenderLifebar.fillAmount * DefenderUS.UnitCharacteristics.AjustedStats.HP) + "";
    }

    public void LoadEnvironment(string ChapterToLoad, Scene SceneToLoadin)
    {
        StartCoroutine(LoadEnvironmentAsync(ChapterToLoad, SceneToLoadin));
    }

    private IEnumerator LoadEnvironmentAsync(string ChapterToLoad, Scene SceneToLoadin)
    {

        bool IsSideStory = false;
        int Chapter = -1;
        if (ChapterToLoad.Contains("Chapter"))
        {
            ChapterToLoad = ChapterToLoad.Replace("Chapter", "");
            Chapter = int.Parse(ChapterToLoad);
        }
        else if (ChapterToLoad.Contains("SideStory"))
        {
            ChapterToLoad = ChapterToLoad.Replace("SideStory", "");
            Chapter = int.Parse(ChapterToLoad);
            IsSideStory = true;
        }
        if (ChapterToLoad.Contains("Prologue"))
        {
            Chapter = 0;
        }

        if (ChapterToLoad.Contains("TestMap"))
        {
            Chapter = UnityEngine.Random.Range(0, EnvirnomentList.Count + 1);
        }


        foreach (CombatEnvirnoment env in EnvirnomentList)
        {

            if (env.ChapterInWhichToUse.Contains(Chapter) && env.isSideStory == IsSideStory)
            {
                if (currentenv == null)
                {
                    UnityEngine.Object newobject = Instantiate(env.Model, SceneToLoadin);
                    currentenv = newobject.GameObject();
                    currentenv.transform.position = env.position;
                    currentenv.transform.rotation = Quaternion.Euler(env.rotation);
                }
                else if (env != null)
                {
                    DestroyImmediate(currentenv);
                    UnityEngine.Object newobject = Instantiate(env.Model, SceneToLoadin);
                    currentenv = newobject.GameObject();
                    currentenv.transform.position = env.position;
                    currentenv.transform.rotation = Quaternion.Euler(env.rotation);
                }
                yield return null;
            }
        }
        yield return null;
    }

    public void SetupScene(CombatSceneLoader SceneLoaderVar, Character attackerCopyBeforeCombat, Character defenderCopyBeforeCombat, List<int> AttackerHits, List<int> AttackerDamage, List<int> AttackerToDefenderAttackerSkillTriggers, List<int> AttackerToDefenderDefenderSkillTriggers, List<int> Defenderhits, List<int> Defenderdamage, List<int> DefenderToAttackerAttackerSkillTriggers, List<int> DefenderToAttackerDefenderSkillTriggers, int AttackerBaseDmg, int AttackerBaseHitRate, int AttackerBaseCrit, int DefenderBaseDmg, int DefenderBaseHitRate, int DefenderBaseCrit, int expearned, List<int> LevelUpBonuses)
    {
        _CombatSceneLoader = SceneLoaderVar;
        ResetSkillIcons();
        EventSystem.SetActive(true);
        ExpBarHolder.gameObject.SetActive(false);
        distributingExp = false;
        InitializePositions();
        if (AttackerUS == null)
        {
            AttackerUS = AttackerGO.GetComponent<UnitScript>();
            DefenderUS = DefenderGO.GetComponent<UnitScript>();
        }
        _BattleSummary = new BattleSummary(attackerCopyBeforeCombat, defenderCopyBeforeCombat, AttackerHits, AttackerDamage, AttackerToDefenderAttackerSkillTriggers, AttackerToDefenderDefenderSkillTriggers, Defenderhits, Defenderdamage, DefenderToAttackerAttackerSkillTriggers, DefenderToAttackerDefenderSkillTriggers, expearned, LevelUpBonuses);
        AttackerUS.UnitCharacteristics = attackerCopyBeforeCombat;
        AttackerUS.InstantiateCharacterModel("Default");
        AttackerUS.UpdateWeaponModel(AttackerUS.GetComponentInChildren<Animator>(), 1f);

        DefenderUS.UnitCharacteristics = defenderCopyBeforeCombat;
        DefenderUS.InstantiateCharacterModel("Default");
        DefenderUS.UpdateWeaponModel(DefenderUS.GetComponentInChildren<Animator>(), 1f);

        bool ishealing = DetermineIfHealing();

        bool IsFirstInteractionRanged = DetermineIfRanged(attackerCopyBeforeCombat, ishealing);


        string attackerstatsString = "";
        string defenderstatsstring = "";
        if (ishealing)
        {
            attackerstatsString = "Heal:" + AttackerBaseDmg + " Hit:100 Crit:-";
            defenderstatsstring = "Dmg:- Hit:- Crit:-";
        }
        else
        {
            attackerstatsString = "Dmg:" + AttackerBaseDmg + " Hit:" + AttackerBaseHitRate + " Crit:" + AttackerBaseCrit;
            if (Defenderhits == null || Defenderhits.Count == 0)
            {
                defenderstatsstring = "Dmg:- Hit:- Crit:-";
            }
            else
            {
                defenderstatsstring = "Dmg:" + DefenderBaseDmg + " Hit:" + DefenderBaseHitRate + " Crit:" + DefenderBaseCrit;
            }

        }

        TMPSetup(attackerstatsString, defenderstatsstring);


        ChangeToCamera(AttackerRunCameraData);
        StartCoroutine(AttackerTurn(ishealing, IsFirstInteractionRanged));

    }


    private void StartCameraDotweenMovement(CamData CameraData)
    {
        if (CameraData == null || CameraData.Holder == null)
            return;

        Transform TransformToUse = CameraData.Holder;
        // Stop any previous animation.
        currentCamTween?.Kill();

        // Place the weapon beside the shoulder.
        TransformToUse.localPosition = CameraData.DefaultPosition;
        TransformToUse.localRotation = Quaternion.Euler(CameraData.DefaultRotation);

        Vector3 initialRotation = TransformToUse.localEulerAngles;

        float value = 2f * (float)UnityEngine.Random.Range(0, 2) - 1f;

        currentCamTween = TransformToUse.DOLocalRotate(initialRotation + (float)value * DotweenRotation, DotweenDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear);

    }

    private void ChangeToCamera(CamData CameraToprioritize)
    {
        AttackerRunCameraData.Camera.Priority = 0;
        FightCameraData.Camera.Priority = 0;
        AttackerVictoryCameraData.Camera.Priority = 0;
        DefenderVictoryCameraData.Camera.Priority = 0;
        CameraToprioritize.Camera.Priority = 1;
        StartCameraDotweenMovement(CameraToprioritize);
    }

    private bool DetermineIfRanged(Character concernedCharacter, bool ishealing)
    {
        if (ishealing)
        {
            return true;
        }

        if (concernedCharacter.telekinesisactivated)
        {
            return true;
        }

        equipment AttackerWeapon = concernedCharacter.equipments[0];

        if (AttackerWeapon != null && AttackerWeapon.type != "" && AttackerWeapon.type != null && AttackerWeapon.Range > 1)
        {
            return true;
        }

        return false;
    }

    private bool DetermineIfHealing() // If the weapon used is a staff an the two characters are friendly, then it's healing and not attacking
    {
        equipment AttackerWeapon = _BattleSummary.AttackerCopyBeforeCombat.equipments[0];
        if (AttackerWeapon == null || AttackerWeapon.type == "" || AttackerWeapon.type == null || AttackerWeapon.type.ToLower() != "staff")
        {
            return false;
        }

        string attackeraffiliation = _BattleSummary.AttackerCopyBeforeCombat.affiliation.ToLower();

        string defenderaffiliation = _BattleSummary.DefenderCopyBeforeCombat.affiliation.ToLower();

        if (attackeraffiliation == defenderaffiliation)
        {
            return true;
        }

        if (attackeraffiliation == "other" && !_BattleSummary.AttackerCopyBeforeCombat.attacksfriends && defenderaffiliation == "playable")
        {
            return true;
        }

        if (defenderaffiliation == "other" && !_BattleSummary.DefenderCopyBeforeCombat.attacksfriends && attackeraffiliation == "playable")
        {
            return true;
        }

        return false;
    }


    public IEnumerator AttackerTurn(bool IsHealing, bool IsRanged)
    {
        // We start by waitting for a bit
        yield return new WaitForSeconds(TimeToWaitBeforeActionStarts);

        bool AttackerMoved = false;

        if (!IsRanged)
        {
            // If it's melee we run towards the enemy

            AttackerMoved = true;
            yield return MoveCharacterTowardsOtherCharacter(AttackerUS, DefenderUS);
        }
        else
        {
            ChangeToCamera(FightCameraData);
        }

        //Then we attack

        ShowSkills(_BattleSummary.AttackerToDefender.CharacterASkillTriggers, true);
        ShowSkills(_BattleSummary.AttackerToDefender.CharacterBSkillTriggers, false);
        yield return AttackAction(AttackerUS, DefenderUS, _BattleSummary.AttackerToDefender, IsHealing);

        if (!IsHealing && DefenderUS.UnitCharacteristics.currentHP > 0)
        {
            StartCoroutine(DefenderTurn(AttackerMoved));
        }
        else
        {
            // Victory ScreenCoroutine
            StartCoroutine(Result());
        }

    }

    public IEnumerator DefenderTurn(bool AttackerMoved)
    {


        //First we check if the defender actually does something during their turn


        if (_BattleSummary.DefenderToAttacker.TypeOfHits != null && _BattleSummary.DefenderToAttacker.TypeOfHits.Count > 0)
        {
            // Then We start by waitting for a bit
            yield return new WaitForSeconds(TimeToWaitBeforeActionStarts);
            bool IsRanged = DetermineIfRanged(_BattleSummary.DefenderCopyBeforeCombat, false);

            if (!IsRanged && !AttackerMoved)
            {
                // If it's melee and the previous character didn't run, we run towards the enemy


                yield return MoveCharacterTowardsOtherCharacter(DefenderUS, AttackerUS);
            }

            //Then we attack
            ResetSkillIcons();
            ShowSkills(_BattleSummary.DefenderToAttacker.CharacterBSkillTriggers, true);
            ShowSkills(_BattleSummary.DefenderToAttacker.CharacterASkillTriggers, false);
            yield return AttackAction(DefenderUS, AttackerUS, _BattleSummary.DefenderToAttacker, false);
        }


        // Then we show the result screen
        StartCoroutine(Result());

    }

    private void TMPSetup(string AttackerstatString, string DefenderStatsString)
    {
        Character AttackerChar = _BattleSummary.AttackerCopyBeforeCombat;
        AttackerName.text = AttackerChar.name;
        AttackerHPText.text = AttackerChar.currentHP + "";
        AttackerLifebar.fillAmount = (float)AttackerChar.currentHP / (float)AttackerChar.AjustedStats.HP;
        AttackerStats.text = AttackerstatString;

        Character DefenderChar = _BattleSummary.DefenderCopyBeforeCombat;
        DefenderName.text = DefenderChar.name;
        DefenderHPText.text = DefenderChar.currentHP + "";
        DefenderLifebar.fillAmount = (float)DefenderChar.currentHP / (float)DefenderChar.AjustedStats.HP;
        DefenderStats.text = DefenderStatsString;

    }

    private void updateTMPHP()
    {
        AttackerHPText.text = AttackerUS.UnitCharacteristics.currentHP + "";
        DefenderHPText.text = DefenderUS.UnitCharacteristics.currentHP + "";
    }

    public IEnumerator Result()
    {

        // We start by waitting for a bit
        yield return new WaitForSeconds(TimeToWaitBeforeActionStarts);
        ResetSkillIcons();

        CharacterEarningExp = null;
        CamData ConcernedCamData = null;

        // We start by checking the Attacker, since he will get the EXP if he healed.
        if (AttackerUS.UnitCharacteristics.affiliation == "playable")
        {
            CharacterEarningExp = AttackerUS;
            ConcernedCamData = AttackerVictoryCameraData;
            DefenderGO.SetActive(false);
        }
        else if (DefenderUS.UnitCharacteristics.affiliation == "playable")
        {
            CharacterEarningExp = DefenderUS;
            ConcernedCamData = DefenderVictoryCameraData;
            AttackerGO.SetActive(true);
        }
        if (CharacterEarningExp != null)
        {
            ChangeToCamera(ConcernedCamData);
            SetupExpBar(CharacterEarningExp.UnitCharacteristics.experience);
            targetExp = (int)Mathf.Min(100, CharacterEarningExp.UnitCharacteristics.experience + _BattleSummary.expearned);
            currentlydistributedexp = CharacterEarningExp.UnitCharacteristics.experience;
            distributingExp = true;

            // Rest is done trough the update
        }
        else
        {
            StartCoroutine(CloseCombatScene(2f));
        }



    }

    public IEnumerator CloseCombatScene(float timetowait)
    {

        yield return new WaitForSeconds(timetowait);

        Debug.Log("ClosingScene");
        EventSystem.SetActive(false);
        currentCamTween?.Kill();
        _CombatSceneLoader.ActivateMainSceneFromCombatScene();

    }

    public IEnumerator AttackAction(UnitScript Attacker, UnitScript Defender, OffensiveSummary _OffensiveSummary, bool Healing)
    {

        int numberOfAttacks = _OffensiveSummary.TypeOfHits.Count;
        bool alreadydied = false;

        int AttacksPerformed = 0;

        while (AttacksPerformed < numberOfAttacks)
        {

            bool IsAttackDodged = _OffensiveSummary.TypeOfHits[AttacksPerformed] == -1;
            bool isCritical = _OffensiveSummary.TypeOfHits[AttacksPerformed] == 2;
            bool isDamageZero = true;
            if (_OffensiveSummary.Damage.Count > AttacksPerformed)
            {
                isDamageZero = _OffensiveSummary.Damage[AttacksPerformed] <= 0;
            }


            PlayAnimation(Attacker.gameObject, 0, isCritical, Healing);


            Animator anim = Attacker.ActiveModel.GetComponentInChildren<Animator>();
            yield return null;
            while (anim.IsInTransition(0))
            {
                yield return null;
            }


            // Wait until we're in an Attack-tagged state (with a safety timeout)
            float timeout = 3f;
            while (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            // Wait for 50% of the current attack state
            while (true)
            {

                AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
                if (!state.IsTag("Attack")) break;          // left the attack
                if (state.normalizedTime >= 0.5f) break;
                yield return null;
            }

            // Add logic for losing health and showing damage numbers
            if (Healing)
            {
                Defender.UnitCharacteristics.currentHP += _OffensiveSummary.Damage[AttacksPerformed];
                if (_OffensiveSummary.CharacterASkillTriggers.Contains(30)) // compassion
                {
                    int healedamount = Mathf.Min((int)Attacker.UnitCharacteristics.AjustedStats.HP - Attacker.UnitCharacteristics.currentHP, _OffensiveSummary.Damage[AttacksPerformed]);
                    Attacker.UnitCharacteristics.currentHP += healedamount;
                    GameObject compassiontextobject = Instantiate(TextObjectPrefab);
                    compassiontextobject.GetComponent<CombatNumberPopup>().InitializeTMP(healedamount + "", Attacker.transform.position, false, true);
                }
            }
            else if (_OffensiveSummary.Damage.Count > AttacksPerformed)
            {
                Defender.UnitCharacteristics.currentHP -= _OffensiveSummary.Damage[AttacksPerformed];
                if (_OffensiveSummary.CharacterASkillTriggers.Contains(14))// Invigorating
                {
                    List<int> Skillvalues = DataScript.instance.SkillList[14].SkillValues;
                    float multiplier = Attacker.GetSkillLevelMultiplier(14);
                    float HealingRatio = (Skillvalues[0] / 100f * multiplier);
                    int amounthealed = Mathf.Min((int)Attacker.UnitCharacteristics.AjustedStats.HP - Attacker.UnitCharacteristics.currentHP, (int)(_OffensiveSummary.Damage[AttacksPerformed] * HealingRatio));
                    Attacker.UnitCharacteristics.currentHP += amounthealed;
                    GameObject compassiontextobject = Instantiate(TextObjectPrefab);
                    compassiontextobject.GetComponent<CombatNumberPopup>().InitializeTMP(amounthealed + "", Attacker.transform.position, false, true);
                }
            }

            string textToShow = "";
            if (IsAttackDodged)
            {
                textToShow = "Miss";
            }
            else
            {
                textToShow = _OffensiveSummary.Damage[AttacksPerformed] + "";
            }

            GameObject textobject = Instantiate(TextObjectPrefab);
            textobject.GetComponent<CombatNumberPopup>().InitializeTMP(textToShow, Defender.transform.position, isCritical, Healing);


            if ((_OffensiveSummary.CharacterBSkillTriggers.Contains(44) || _OffensiveSummary.CharacterBSkillTriggers.Contains(97)) && Defender.UnitCharacteristics.currentHP <= 0)//Unyielding && Guardian Spirit
            {
                Defender.UnitCharacteristics.currentHP = 1;
            }

            if (Defender.UnitCharacteristics.currentHP < 0)
            {
                Defender.UnitCharacteristics.currentHP = 0;
            }
            if (Defender.UnitCharacteristics.currentHP > (int)Defender.UnitCharacteristics.AjustedStats.HP)
            {
                Defender.UnitCharacteristics.currentHP = (int)Defender.UnitCharacteristics.AjustedStats.HP;
            }

            if (Attacker.UnitCharacteristics.currentHP < 0)
            {
                Attacker.UnitCharacteristics.currentHP = 0;
            }
            if (Attacker.UnitCharacteristics.currentHP > (int)Attacker.UnitCharacteristics.AjustedStats.HP)
            {
                Attacker.UnitCharacteristics.currentHP = (int)Attacker.UnitCharacteristics.AjustedStats.HP;
            }

            updateTMPHP();

            if (!alreadydied)
            {
                if (Healing)
                {
                    PlayAnimation(Defender.gameObject, 6);
                }
                else if (Defender.UnitCharacteristics.currentHP <= 0)
                {
                    PlayAnimation(Defender.gameObject, 4);
                    alreadydied = true;
                }
                else
                {
                    if (IsAttackDodged)
                    {
                        PlayAnimation(Defender.gameObject, 3);
                    }
                    else if (!isDamageZero)
                    {
                        PlayAnimation(Defender.gameObject, 2);
                    }
                    else
                    {
                        // This case is the case where the blow is not dodged, but it does 0 damage. No animatio is played, the character stays unfaded
                    }
                }
            }
            // yield return new WaitForSeconds(animationLength * 0.1f);
            yield return new WaitForSeconds(TimeBetweenAttackCombos);

            AttacksPerformed++;
        }
    }

    private int GetSkillLevel(UnitScript.Character CharacterToUse, int SkillID)
    {
        if (CharacterToUse.UnitSkill == SkillID)
        {
            return CharacterToUse.UnitSkillLevel;
        }
        else if (CharacterToUse.SecondUnitSkill == SkillID)
        {
            return CharacterToUse.SecondSkillLevel;
        }
        else
        {
            return 1;
        }
    }

    public IEnumerator MoveCharacterTowardsOtherCharacter(UnitScript TheOneWhoMoves, UnitScript TheOther)
    {

        // If it's melee we run towards the enemy
        Vector3 startPos = TheOneWhoMoves.ActiveModel.transform.position;
        Vector3 TargetPos = TheOther.ActiveModel.transform.position;

        // Direction from attacker to defender
        Vector3 dir = (TargetPos - startPos).normalized;

        // Target position stopping at melee distance
        Vector3 targetPos = TargetPos - dir * DistanceForMelee;

        float elapsed = 0f;
        bool alreadyswitchedcam = false;

        while (elapsed < TimeToWalkToEnnemies && Vector3.Distance(TheOneWhoMoves.ActiveModel.transform.position, TargetPos) > DistanceForMelee)
        {

            PlayAnimation(TheOneWhoMoves.gameObject, 1);
            elapsed += Time.deltaTime;
            float t = elapsed / TimeToWalkToEnnemies;

            TheOneWhoMoves.transform.position = Vector3.Lerp(startPos, targetPos, t);

            if (t > 0.5f && !alreadyswitchedcam)
            {
                ChangeToCamera(FightCameraData);
                alreadyswitchedcam = true;
            }

            yield return null;
        }
        PlayAnimation(TheOneWhoMoves.gameObject, 5);


    }

    private void PlayAnimation(GameObject CharacterToAnimate, int animationtype, bool iscritical = false, bool healing = false)
    {

        Animator animator = null;
        UnitScript CharacterUnitscript = null;
        CharacterUnitscript = CharacterToAnimate.GetComponent<UnitScript>();
        animator = CharacterUnitscript.GetAnimator();
        animator.SetBool("victory", false);
        switch (animationtype) // 0 : atk, 1 : walk, 2 : Take Damage, 3 : Dodge, 4 : Die, 5 : Idle, 6 Victory
        {
            case 0:
                animator.SetBool("Critical", iscritical);
                CharacterUnitscript.PlayAttackAnimation(false, false, healing, animator);
                break;
            case 1:
                animator.SetBool("Walk", true);
                break;
            case 2:
                animator.SetTrigger("Damage");
                break;
            case 3:
                animator.SetTrigger("Dodge");
                break;
            case 4:
                animator.SetTrigger("Death");
                break;
            case 5:
                animator.SetBool("Walk", false);
                break;
            case 6:
                animator.SetBool("victory", true);
                break;
        }

    }

    [ContextMenu("InstantiateEnvironmentAndCharactersForTesting")]
    private void GenerateTestScene()
    {
        if (AttackerUS == null)
        {
            AttackerUS = AttackerGO.GetComponent<UnitScript>();
            DefenderUS = DefenderGO.GetComponent<UnitScript>();
        }
        InitializePositions();
        DataScript DS = null;
        string[] scriptGUIDs = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        foreach (string guid in scriptGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject GO = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (GO.GetComponent<DataScript>())
            {
                DS = GO.GetComponent<DataScript>();
            }

        }
        if (DS != null)
        {
            Debug.Log("DataScript found");
        }
        string Chaptername = "Chapter" + TestSceneID;
        StartCoroutine(LoadEnvironmentAsync(Chaptername, SceneManager.GetActiveScene()));
        AttackerUS.UnitCharacteristics = CalculationToolsScript.CreateCopy(DS.PlayableCharacterList[TestAttackerID]);
        AttackerUS.InstantiateCharacterModel("Default");
        DefenderUS.UnitCharacteristics = CalculationToolsScript.CreateCopy(DS.PlayableCharacterList[TestDefenderID]);
        DefenderUS.InstantiateCharacterModel("Default");
    }

    private void InitializePositions()
    {
        AttackerGO.transform.position = AttackerStartpos;
        DefenderGO.transform.position = DefenderStartpos;
        FighterCamHolder.position = (AttackerStartpos + DefenderStartpos) / 2f + new Vector3(0f, 1f, 0f);
    }

    /*
     
     phases:
    - Cam starts behind attacker

    - if attacker is not ranged, it runs to enemy. For that phase the camera is childed to the attacker (prévoir trois positions possibles en cas de déplacement ( haut, milieu, bas)

    - if character is ranged they attack directly

    - In Attack Phase Place the camera to look between the characters. For the transitions, a cut to this one is fine. The camera needs to move as the characters move if it's the case. Maybe use dotween to move cameras a bit.

    - Pour le déplacement de l'autre unité, reculer juste un peu la cam.

    - Pour les résulat, gros plan sur l'unité allié. sinon skip

     
     */


    private void ResetSkillIcons()
    {
        foreach (Transform Child in AttackerSkillHolder.transform)
        {
            Child.gameObject.SetActive(false);
        }
        foreach (Transform Child in DefenderSkillHolder.transform)
        {
            Child.gameObject.SetActive(false);
        }
    }

    private void ShowSkills(List<int> Skills, bool isattacker)
    {
        List<int> SkillswithoutDoubles = new List<int>();
        foreach (int Skill in Skills)
        {
            if (!SkillswithoutDoubles.Contains(Skill))
            {
                SkillswithoutDoubles.Add(Skill);
            }
        }
        Transform Holder = DefenderSkillHolder;
        List<SkillIconScript> IconList = DefenderSkillIcons;
        List<TextMeshProUGUI> NameList = DefenderSkillNames;
        if (isattacker)
        {
            Holder = AttackerSkillHolder;
            IconList = AttackerSkillIcons;
            NameList = AttackerSkillNames;
        }

        for (int i = 0; i < Math.Min(SkillswithoutDoubles.Count, IconList.Count); i++)
        {
            DataScript.Skill skill = DataScript.instance.SkillList[SkillswithoutDoubles[i]];
            Holder.GetChild(i).gameObject.SetActive(true);
            NameList[i].text = skill.name;
            IconList[i].InitializeIcon(skill.SkillIconInfo);
        }

    }

    [ContextMenu("Fill Cam Data")]
    private void FillCamData()
    {
        AttackerRunCameraData = new CamData(AttackerRunCameraHolder);
        AttackerVictoryCameraData = new CamData(AttackerVictoryCamHolder);
        DefenderVictoryCameraData = new CamData(DefenderVictoryCamHolder);
        FightCameraData = new CamData(FighterCamHolder);

    }

    [ContextMenu("Fill Skill Data")]
    private void FillSkillData()
    {
        if (AttackerSkillHolder != null)
        {
            AttackerSkillIcons = new List<SkillIconScript>();
            AttackerSkillNames = new List<TextMeshProUGUI>();
            for (int i = 0; i < AttackerSkillHolder.childCount; i++)
            {
                AttackerSkillIcons.Add(AttackerSkillHolder.GetChild(i).GetComponentInChildren<SkillIconScript>());
                AttackerSkillNames.Add(AttackerSkillHolder.GetChild(i).GetComponentInChildren<TextMeshProUGUI>());
            }
        }
        if (DefenderSkillHolder != null)
        {
            DefenderSkillIcons = new List<SkillIconScript>();
            DefenderSkillNames = new List<TextMeshProUGUI>();
            for (int i = 0; i < DefenderSkillHolder.childCount; i++)
            {
                DefenderSkillIcons.Add(DefenderSkillHolder.GetChild(i).GetComponentInChildren<SkillIconScript>());
                DefenderSkillNames.Add(DefenderSkillHolder.GetChild(i).GetComponentInChildren<TextMeshProUGUI>());
            }
        }

    }

}
