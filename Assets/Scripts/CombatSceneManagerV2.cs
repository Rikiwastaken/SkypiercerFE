using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnitScript;

public class CombatSceneManagerV2 : MonoBehaviour
{

    public static CombatSceneManagerV2 instance;

    [Serializable]
    public class CharacterBattleInfo
    {
        public Character character;
        public equipment weapon;
        public List<int> attacks;
        public List<int> criticals;
        public Animator Animator;
        public int currentHP;
        public bool unyieldingactivated;
        public bool oneforallactivated;
        public bool compassionactivated;
        public bool invigoratingactivated;

        public void getweapon()
        {
            if (character != null && character.equipments != null && character.equipments.Count > 0)
            {
                weapon = character.equipments[0];
            }
        }

    }

    [Serializable]
    public class CombatEnvirnoment
    {
        public GameObject Model;
        public Vector3 position;
        public Vector3 rotation;
        public List<int> ChapterInWhichToUse;
    }

    [Serializable]
    public class CamPosition
    {
        public Vector3 CamHolderPosition;
        public Vector3 CamHolderRotation;
        public Transform parent;
        public Vector3 CameraPosition;
        public Vector3 CameraRotation;
    }


    [Serializable]
    public class InfoText
    {
        public string name;
        public TextMeshProUGUI TMPComponent;
        public Image LifeBar;
        public Vector2 BasePos;
        public Vector2 TargetPos;

        public void Replace()
        {
            TMPComponent.transform.parent.GetComponent<RectTransform>().localPosition = BasePos;
        }

    }

    [Header("Scene Variables")]
    public InfoText AttackerName = new InfoText() { name = "AttackerName" };
    public InfoText AttackerDescription = new InfoText() { name = "AttackerDescription" };
    public InfoText DefenderName = new InfoText() { name = "DefenderName" };
    public InfoText DefenderDescription = new InfoText() { name = "DefenderDescription" };
    public TextMeshProUGUI AttackerHP;
    public TextMeshProUGUI DefenderHP;
    public GameObject AttackerGO;
    public Vector3 AttackerStartpos;
    public GameObject DefenderGO;
    public Vector3 DefenderStartpos;
    public GameObject TextObjectPrefab;
    public Vector3 ExpBarBasePos;
    public Vector3 ExptargetBasePos;
    public GameObject ExpBarHolder;
    public Image ExpBarImage;
    public TextMeshProUGUI ExpBarText;
    public FreezeFrameCapture LevelUpFreezeFrame;
    public List<CombatEnvirnoment> EnvirnomentList;
    private GameObject currentenv;

    [Header("Camera Positions")]
    public Transform Camera;
    public Transform CameraHolder;
    public CamPosition InitialCameraPosition;
    public CamPosition CamPositionToSeeEnemy;
    public CamPosition CamePositionAttackerRunning;
    public CamPosition CamPositionEnemyRunning;
    public CamPosition AttackerVictoryCam;
    public CamPosition EnemyVictoryCam;

    [Header("Scene Unwinding Variables")]
    //PreBattle

    public float timelookingatattacker;
    public float timetolookatdefender;
    public float timetoshownames;
    // attack
    public float DistanceForMelee;
    public float TimeToWalkToEnnemies;
    public float TimeBetweenAttacks;
    public float timeforLifebar;

    //results
    public float TimeforExpBar;
    public float TimeAfterExpBar;



    [Header("Battle Variables")]
    public CharacterBattleInfo AttackerInfo;
    public CharacterBattleInfo DefenderInfo;
    public List<int> levelupbonus;
    public int exp;
    public bool ishealing;
    public bool isinBeforeBattleStarts;
    public bool isinAttackerTurn;
    public bool isinDefenderTurn;
    public bool isinEndOfCombat;

    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
    }

    private void Update()
    {
        if (AttackerHP.text != "" + Mathf.Max(0, AttackerInfo.currentHP))
        {
            AttackerHP.text = "" + Mathf.Max(0, AttackerInfo.currentHP);
        }
        if (DefenderHP.text != "" + Mathf.Max(0, DefenderInfo.currentHP))
        {
            DefenderHP.text = "" + Mathf.Max(0, DefenderInfo.currentHP);
        }
    }

    public void SetupScene(Character attacker, Character defender, List<int> attackerattacks, List<int> attackercriticals, List<int> defenderattacks, List<int> defendercriticals, int attackerhitrate, int attackercritrate, int attackerdamage, int defenderhitrate, int defendercritrate, int defenderdamage, int expgained, List<int> levelupbonuses, bool doesdefenderattacks, bool attackeroneforall, bool attackerunyielding, bool attackercompassionused, bool attackerinvigorating, bool defenderoneforall, bool defenderunyielding, bool defendercompassionused, bool defenderinvigorating) // list<int> for attacks: -1 are dodges, >=0 are damages, for criticals, 1=critical, 0=not
    {
        Debug.Log("Setting up scene");
        Debug.Log(attacker.name + " vs " + defender.name);
        Debug.Log("Attacker attacks: " + string.Join(", ", attackerattacks));
        Debug.Log("Attacker criticals: " + string.Join(", ", attackercriticals));
        Debug.Log("Defender attacks: " + string.Join(", ", defenderattacks));
        Debug.Log("Defender criticals: " + string.Join(", ", defendercriticals));
        Debug.Log("Attacker hitrate: " + attackerhitrate);
        Debug.Log("Attacker critrate: " + attackercritrate);
        Debug.Log("Attacker damage: " + attackerdamage);
        Debug.Log("Defender hitrate: " + defenderhitrate);
        Debug.Log("Defender critrate: " + defendercritrate);
        Debug.Log("Defender damage: " + defenderdamage);
        Debug.Log("Exp gained: " + expgained);
        Debug.Log("Level up bonuses: " + string.Join(", ", levelupbonuses));
        Debug.Log("Does defender attack: " + doesdefenderattacks);
        Debug.Log("Attacker oneforall: " + attackeroneforall);
        Debug.Log("Attacker unyielding: " + attackerunyielding);
        Debug.Log("Attacker compassion used: " + attackercompassionused);
        Debug.Log("Attacker invigorating: " + attackerinvigorating);
        Debug.Log("Defender oneforall: " + defenderoneforall);
        Debug.Log("Defender unyielding: " + defenderunyielding);
        Debug.Log("Defender compassion used: " + defendercompassionused);
        Debug.Log("Defender invigorating: " + defenderinvigorating);

        //Filling out base classes

        AttackerInfo = new CharacterBattleInfo()
        {
            character = attacker,
            attacks = attackerattacks,
            criticals = attackercriticals,
            currentHP = attacker.currentHP,
            unyieldingactivated = attackerunyielding,
            oneforallactivated = attackeroneforall,
            compassionactivated = attackercompassionused,
            invigoratingactivated = attackerinvigorating
        };

        AttackerInfo.getweapon();

        DefenderInfo = new CharacterBattleInfo()
        {
            character = defender,
            attacks = defenderattacks,
            criticals = defendercriticals,
            currentHP = defender.currentHP,
            unyieldingactivated = defenderunyielding,
            oneforallactivated = defenderoneforall,
            compassionactivated = defendercompassionused,
            invigoratingactivated = defenderinvigorating
        };

        DefenderInfo.getweapon();

        // Fill out exp and level bonuses

        levelupbonus = levelupbonuses;
        exp = expgained;

        //Determine if attacker is healing defender or not

        ishealing = DetermineIfHealing();

        // Prefill Names and descriptions

        AttackerName.TMPComponent.text = AttackerInfo.character.name;
        AttackerDescription.TMPComponent.text = CreateDescriptionString(attackerattacks, attackerhitrate, attackercritrate, attackerdamage, true);
        DefenderName.TMPComponent.text = DefenderInfo.character.name;
        DefenderDescription.TMPComponent.text = CreateDescriptionString(defenderattacks, defenderhitrate, defendercritrate, defenderdamage, doesdefenderattacks);

        // Giving GameObjects their Character and load their models and get the animators

        AttackerGO.GetComponent<BattleCharacterScript>().InitializeModels();
        AttackerInfo.Animator = AttackerGO.GetComponent<UnitScript>().ModelList[attacker.modelID].wholeModel.GetComponentInChildren<Animator>();
        AttackerGO.GetComponent<UnitScript>().UnitCharacteristics = attacker;

        AttackerGO.GetComponent<BattleCharacterScript>().ActivateModel(attacker.modelID);


        DefenderGO.GetComponent<BattleCharacterScript>().InitializeModels();
        DefenderInfo.Animator = DefenderGO.GetComponent<UnitScript>().ModelList[defender.modelID].wholeModel.GetComponentInChildren<Animator>();
        DefenderGO.GetComponent<UnitScript>().UnitCharacteristics = defender;
        DefenderGO.GetComponent<BattleCharacterScript>().ActivateModel(defender.modelID);


        //Initiliaze the animations and weapons

        AttackerGO.GetComponent<UnitScript>().UpdateWeaponModel(AttackerInfo.Animator, 1f);
        DefenderGO.GetComponent<UnitScript>().UpdateWeaponModel(DefenderInfo.Animator, 1f);

        AttackerInfo.Animator.SetBool("Ismachine", attacker.enemyStats.monsterStats.ismachine);
        DefenderInfo.Animator.SetBool("Ismachine", defender.enemyStats.monsterStats.ismachine);
        AttackerInfo.Animator.SetBool("Ispluvial", attacker.enemyStats.monsterStats.ispluvial);
        DefenderInfo.Animator.SetBool("Ispluvial", defender.enemyStats.monsterStats.ispluvial);
        DefenderInfo.Animator.SetBool("UsingTelekinesis", defender.telekinesisactivated);
        AttackerInfo.Animator.SetBool("UsingTelekinesis", attacker.telekinesisactivated);

        //Initializing Lifebars
        AttackerName.LifeBar.fillAmount = ((float)attacker.currentHP / (float)attacker.AjustedStats.HP);
        DefenderName.LifeBar.fillAmount = ((float)defender.currentHP / (float)defender.AjustedStats.HP);

        // Replace GameObjects
        AttackerName.Replace();
        AttackerDescription.Replace();
        DefenderName.Replace();
        DefenderDescription.Replace();
        AttackerGO.transform.position = AttackerStartpos;
        DefenderGO.transform.position = DefenderStartpos;
        ExpBarHolder.GetComponent<RectTransform>().localPosition = ExpBarBasePos;

        // Set Camera Position
        CameraHolder.parent = InitialCameraPosition.parent;
        CameraHolder.transform.localPosition = InitialCameraPosition.CamHolderPosition;
        CameraHolder.transform.localRotation = Quaternion.Euler(InitialCameraPosition.CamHolderRotation);
        Camera.transform.localPosition = InitialCameraPosition.CameraPosition;
        Camera.transform.localRotation = Quaternion.Euler(InitialCameraPosition.CameraRotation);

        //Start the battle
        StartCoroutine(BeforeBattleStarts());
    }


    public IEnumerator BeforeBattleStarts()
    {
        isinBeforeBattleStarts = true;

        Debug.Log("BeforeBattleStarts");

        // Place Camera in front of the player

        StartCoroutine(ShowText(AttackerName, timetoshownames));

        yield return new WaitForSeconds(timelookingatattacker);

        // Move Camera to back of character to show enemy

        StartCoroutine(ShowText(DefenderName, timetoshownames));

        yield return StartCoroutine(MoveCamera(CamPositionToSeeEnemy, timetolookatdefender));

        StartCoroutine(ShowText(AttackerDescription, timetoshownames));
        StartCoroutine(ShowText(DefenderDescription, timetoshownames));

        isinBeforeBattleStarts = false;
        StartCoroutine(AttackerTurn());
    }


    public IEnumerator AttackerTurn()
    {

        Debug.Log("AttackerTurn");

        isinAttackerTurn = true;

        bool isranged = DetermineIfRanged(AttackerInfo);
        if (!isranged)
        {
            yield return MoveCamera(CamePositionAttackerRunning, 0.1f);
            Vector3 startPos = AttackerGO.transform.position;
            Vector3 defenderPos = DefenderGO.transform.position;

            // Direction from attacker to defender
            Vector3 dir = (defenderPos - startPos).normalized;

            // Target position stopping at melee distance
            Vector3 targetPos = defenderPos - dir * DistanceForMelee;

            float elapsed = 0f;

            while (elapsed < TimeToWalkToEnnemies && Vector3.Distance(startPos, defenderPos) > DistanceForMelee)
            {
                PlayAnimation(AttackerGO, 1);
                elapsed += Time.deltaTime;
                float t = elapsed / TimeToWalkToEnnemies;

                AttackerGO.transform.position = Vector3.Lerp(startPos, targetPos, t);

                yield return null;
            }
            PlayAnimation(AttackerGO, 5);
            // Ensure exact final position
            AttackerGO.transform.position = targetPos;
        }

        // Attack

        int attacksMade = 0;
        bool alreadydied = false;
        while (attacksMade < AttackerInfo.attacks.Count)
        {
            PlayAnimation(AttackerGO, 0);

            yield return null;

            var animator = AttackerGO.GetComponent<UnitScript>().ModelList[AttackerInfo.character.modelID].wholeModel.GetComponentInChildren<Animator>();

            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            float animationLength = clipInfo[0].clip.length;

            yield return new WaitForSeconds(animationLength);

            //reaction

            //Show text for damage

            int damage = AttackerInfo.attacks[attacksMade];
            bool iscritical = false;
            if (AttackerInfo.criticals != null && AttackerInfo.criticals.Count > attacksMade)
            {
                iscritical = AttackerInfo.criticals[attacksMade] == 1;
            }
            bool ismiss = damage == -1;
            TakeDamageOrHealing(AttackerInfo, DefenderInfo, ishealing, damage, iscritical, ismiss);


            if (!alreadydied)
            {
                if (ishealing)
                {
                    //specialreactionforhealing
                }
                else if (AttackerInfo.currentHP <= 0)
                {
                    PlayAnimation(DefenderGO, 4);
                    alreadydied = true;
                }
                else
                {
                    if (AttackerInfo.attacks[attacksMade] == -1)
                    {
                        PlayAnimation(DefenderGO, 3);
                    }
                    else
                    {
                        PlayAnimation(DefenderGO, 2);
                    }
                }
            }





            yield return new WaitForSeconds(TimeBetweenAttacks);
            attacksMade++;
        }



        isinAttackerTurn = false;
        StartCoroutine(DefenderTurn());
    }
    public IEnumerator DefenderTurn()
    {
        Debug.Log("DefenderTurn");
        isinDefenderTurn = true;

        if (DefenderInfo.attacks.Count > 0)
        {
            bool isranged = DetermineIfRanged(DefenderInfo);
            if (!isranged)
            {
                yield return MoveCamera(CamPositionEnemyRunning, 0.25f);
                Vector3 startPos = DefenderGO.transform.position;
                Vector3 defenderPos = AttackerGO.transform.position;

                // Direction from attacker to defender
                Vector3 dir = (defenderPos - startPos).normalized;

                // Target position stopping at melee distance
                Vector3 targetPos = defenderPos - dir * DistanceForMelee;

                float elapsed = 0f;

                while (elapsed < TimeToWalkToEnnemies && Vector3.Distance(startPos, defenderPos) > DistanceForMelee)
                {
                    PlayAnimation(DefenderGO, 1);
                    elapsed += Time.deltaTime;
                    float t = elapsed / TimeToWalkToEnnemies;

                    DefenderGO.transform.position = Vector3.Lerp(startPos, targetPos, t);

                    yield return null;
                }

                PlayAnimation(DefenderGO, 5);
                // Ensure exact final position
                DefenderGO.transform.position = targetPos;
            }

            // Attack

            bool alreadydied = false;

            int attacksMade = 0;
            while (attacksMade < DefenderInfo.attacks.Count)
            {
                PlayAnimation(DefenderGO, 0);

                yield return null;

                yield return new WaitForSeconds(DefenderGO.GetComponent<UnitScript>().ModelList[DefenderInfo.character.modelID].wholeModel.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0).Length);

                //reaction

                //Show text for damage

                int damage = DefenderInfo.attacks[attacksMade];
                bool iscritical = false;
                if (DefenderInfo.criticals != null && DefenderInfo.criticals.Count > attacksMade)
                {
                    iscritical = DefenderInfo.criticals[attacksMade] == 1;
                }
                bool ismiss = damage == -1;

                TakeDamageOrHealing(DefenderInfo, AttackerInfo, false, damage, iscritical, ismiss);

                if (!alreadydied)
                {
                    if (DefenderInfo.currentHP <= 0)
                    {
                        PlayAnimation(DefenderGO, 4);
                        alreadydied = true;
                    }
                    else if (DefenderInfo.attacks[attacksMade] == -1)
                    {
                        PlayAnimation(AttackerGO, 3);
                    }
                    else
                    {
                        PlayAnimation(AttackerGO, 2);
                    }
                }





                yield return new WaitForSeconds(TimeBetweenAttacks);
                attacksMade++;
            }
        }



        isinDefenderTurn = false;
        StartCoroutine(EndOfCombat());
    }

    public void TakeDamageOrHealing(CharacterBattleInfo attacker, CharacterBattleInfo defender, bool ishealing, int damage, bool iscritical, bool ismiss)
    {

        int HPbeforeDamage = defender.currentHP;

        string texttoshow = "";
        if (ishealing)
        {
            int healing = Mathf.Min(damage, (int)defender.character.AjustedStats.HP - defender.currentHP);
            texttoshow = healing + "";
            defender.currentHP += healing;
            if (attacker.compassionactivated)
            {
                int attackerHPBeforeDamage = attacker.currentHP;
                attacker.currentHP += healing;
                StartCoroutine(ChangeLifeBar(AttackerName.LifeBar, attackerHPBeforeDamage, attacker.currentHP, (int)attacker.character.AjustedStats.HP, timeforLifebar));
                GameObject compassiontextobject = Instantiate(TextObjectPrefab);
                compassiontextobject.GetComponent<CombatNumberPopup>().InitializeTMP("" + healing, AttackerGO.transform.position, false, true);
            }
        }
        else if (ismiss)
        {
            texttoshow = "Miss";
        }
        else
        {
            int truedamage = damage;

            if (defender.oneforallactivated)
            {
                truedamage = damage / 2;
            }
            if (defender.unyieldingactivated)
            {
                if (truedamage >= defender.currentHP)
                {
                    truedamage = defender.currentHP - 1;
                }
            }

            defender.currentHP -= truedamage;
            texttoshow = truedamage + "";
            if (attacker.invigoratingactivated)
            {
                int attackerHPBeforeDamage = attacker.currentHP;
                int healing = (int)Math.Min(attacker.character.AjustedStats.HP - attacker.currentHP, (int)(truedamage * 0.1f));
                attacker.currentHP += healing;
                Debug.Log(attacker.currentHP);
                StartCoroutine(ChangeLifeBar(AttackerName.LifeBar, attackerHPBeforeDamage, attacker.currentHP, (int)attacker.character.AjustedStats.HP, timeforLifebar));
                GameObject compassiontextobject = Instantiate(TextObjectPrefab);
                compassiontextobject.GetComponent<CombatNumberPopup>().InitializeTMP("" + healing, AttackerGO.transform.position, false, true);
            }

        }
        Image lifebartouse;
        GameObject GOToUse;
        if (defender == AttackerInfo)
        {
            lifebartouse = AttackerName.LifeBar;
            GOToUse = AttackerGO;
        }
        else
        {
            lifebartouse = DefenderName.LifeBar;
            GOToUse = DefenderGO;
        }


        StartCoroutine(ChangeLifeBar(lifebartouse, HPbeforeDamage, defender.currentHP, (int)defender.character.AjustedStats.HP, timeforLifebar));
        GameObject textobject = Instantiate(TextObjectPrefab);
        textobject.GetComponent<CombatNumberPopup>().InitializeTMP(texttoshow, GOToUse.transform.position, iscritical, ishealing);
    }

    public IEnumerator EndOfCombat()
    {
        Debug.Log("EndOfCombat");
        isinEndOfCombat = true;

        GameObject importantGO = null;
        CharacterBattleInfo importantInfo = null;
        if (AttackerInfo.character.affiliation.ToLower() == "playable" && AttackerInfo.currentHP > 0)
        {
            importantGO = AttackerGO;
            importantInfo = AttackerInfo;
            yield return MoveCamera(AttackerVictoryCam, 0.5f, true);
        }
        else if (DefenderInfo.character.affiliation.ToLower() == "playable" && DefenderInfo.currentHP > 0)
        {
            importantGO = DefenderGO;
            importantInfo = DefenderInfo;
            yield return MoveCamera(EnemyVictoryCam, 0.5f, true);
        }

        if (importantGO != null)
        {
            importantInfo.character.level++;
            ExpBarImage.fillAmount = (float)importantInfo.character.experience / 100f;
            ExpBarText.text = importantInfo.character.experience + "";

            // place la camera pour la fin du combat

            yield return StartCoroutine(ShowExpBar());

            yield return StartCoroutine(ChangeExpBar(importantInfo.character));

            yield return new WaitForSeconds(TimeAfterExpBar);

            if (importantInfo.character.experience + exp >= 100)
            {
                LevelUpFreezeFrame.PlayFullAnimation(importantInfo.character, levelupbonus);
            }
            else
            {
                CloseCombatScene();
            }

        }
        else
        {
            CloseCombatScene();
        }
    }

    public void CloseCombatScene()
    {
        Debug.Log("ClosingScene");
        isinEndOfCombat = false;
        if (FindAnyObjectByType<CombatSceneLoader>() != null)
        {
            FindAnyObjectByType<CombatSceneLoader>().ActivateMainScene();
        }
    }

    public IEnumerator ShowText(InfoText textClass, float durationofmanoeuver)
    {
        RectTransform rectTransform = textClass.TMPComponent.transform.parent.GetComponent<RectTransform>();

        Vector2 startPosition = rectTransform.localPosition;

        float elapsedTime = 0f;

        while (elapsedTime < durationofmanoeuver)
        {
            elapsedTime += Time.deltaTime;

            float ratio = elapsedTime / durationofmanoeuver;

            rectTransform.localPosition = Vector2.Lerp(startPosition, textClass.TargetPos, ratio);

            yield return null; // wait for next frame
        }

        // Ensure final position is exact
        rectTransform.localPosition = textClass.TargetPos;
    }

    public IEnumerator ShowExpBar()
    {
        RectTransform rectTransform = ExpBarHolder.GetComponent<RectTransform>();

        Vector2 startPosition = ExpBarBasePos;

        float elapsedTime = 0f;

        while (elapsedTime < timetoshownames)
        {
            elapsedTime += Time.deltaTime;

            float ratio = elapsedTime / timetoshownames;

            rectTransform.localPosition = Vector2.Lerp(startPosition, ExptargetBasePos, ratio);

            yield return null; // wait for next frame
        }

        // Ensure final position is exact
        rectTransform.localPosition = ExptargetBasePos;
    }

    public IEnumerator ChangeLifeBar(Image Lifebar, int baseHP, int targetHP, int maxHP, float durationofmanoeuver)
    {

        float elapsedTime = 0f;

        while (elapsedTime < durationofmanoeuver)
        {
            elapsedTime += Time.deltaTime;

            float ratio = elapsedTime / durationofmanoeuver;

            float FillAmount = (baseHP + (targetHP - baseHP) * ratio) / maxHP;

            Lifebar.fillAmount = FillAmount;

            yield return null; // wait for next frame
        }

        // Ensure final position is exact
        Lifebar.fillAmount = (float)targetHP / (float)maxHP;
    }

    public IEnumerator ChangeExpBar(Character current)
    {

        int baseexpbar = current.experience;

        int targetexp = Mathf.Min(100, baseexpbar + exp);

        float elapsedTime = 0f;

        while (elapsedTime < TimeforExpBar)
        {
            elapsedTime += Time.deltaTime;

            float ratio = elapsedTime / TimeforExpBar;

            float FillAmount = (baseexpbar + (targetexp - baseexpbar) * ratio) / 100f;

            ExpBarText.text = (int)Mathf.Min(100, FillAmount * 100) + "";

            ExpBarImage.fillAmount = FillAmount;

            if (FillAmount > 1)
            {
                break;
            }

            yield return null; // wait for next frame
        }

        // Ensure final position is exact
        ExpBarImage.fillAmount = (float)targetexp / 100f;
    }

    private void PlayAnimation(GameObject CharacterToAnimate, int animationtype, bool doubleattack = false, bool tripleattack = false, bool healing = false)
    {

        Animator animator = null;
        UnitScript CharacterUnitscript = null;
        CharacterUnitscript = CharacterToAnimate.GetComponent<UnitScript>();
        animator = CharacterUnitscript.ModelList[CharacterUnitscript.UnitCharacteristics.modelID].wholeModel.GetComponentInChildren<Animator>();

        switch (animationtype) // 0 : atk, 1 : walk, 2 : Take Damage, 3 : Dodge, 4 : Die, 5 : Idle
        {
            case 0:
                CharacterUnitscript.PlayAttackAnimation(doubleattack, tripleattack, healing, animator);
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
        }

    }

    private string CreateDescriptionString(List<int> attacks, int hitrate, int critrate, int basedamage, bool characterattacks)
    {
        if (!characterattacks)
        {
            return "Dmg -  Hit -  Crit -";
        }

        string description = "Dmg " + basedamage;
        if (attacks.Count > 1)
        {
            description += "x" + attacks.Count;
        }

        description += "  Hit " + hitrate + "%  Crit " + critrate + "%";

        return description;
    }

    private bool DetermineIfRanged(CharacterBattleInfo charactertouse)
    {

        if (charactertouse.character.telekinesisactivated)
        {
            return true;
        }
        if (charactertouse.weapon.Range > 1)
        {
            return true;
        }


        return false;
    }

    IEnumerator MoveCamera(CamPosition newposition, float timetomovecam, bool instantaneous = false)
    {
        CameraHolder.SetParent(newposition.parent, true);
        float elapsed = 0f;

        // Store starting transforms
        Vector3 startHolderPos = CameraHolder.localPosition;
        Quaternion startHolderRot = CameraHolder.localRotation;

        Vector3 startCamPos = Camera.localPosition;
        Quaternion startCamRot = Camera.localRotation;

        // Target transforms
        Vector3 targetHolderPos = newposition.CamHolderPosition;
        Quaternion targetHolderRot = Quaternion.Euler(newposition.CamHolderRotation);

        Vector3 targetCamPos = newposition.CameraPosition;
        Quaternion targetCamRot = Quaternion.Euler(newposition.CameraRotation);

        if (instantaneous)
        {
            // Snap exactly to final values (avoids floating precision issues)
            CameraHolder.localPosition = targetHolderPos;
            CameraHolder.localRotation = targetHolderRot;

            Camera.localPosition = targetCamPos;
            Camera.localRotation = targetCamRot;
        }
        else
        {
            while (elapsed < timetomovecam)
            {
                elapsed += Time.deltaTime;
                float time = elapsed / timetomovecam;

                // Smooth interpolation (ease in/out)
                time = Mathf.SmoothStep(0f, 1f, time);

                // Move holder
                CameraHolder.localPosition = Vector3.Lerp(startHolderPos, targetHolderPos, time);

                CameraHolder.localRotation = Quaternion.Slerp(startHolderRot, targetHolderRot, time);

                // Move camera
                Camera.localPosition = Vector3.Lerp(startCamPos, targetCamPos, time);

                Camera.localRotation = Quaternion.Slerp(startCamRot, targetCamRot, time);

                yield return null;
            }

            // Snap exactly to final values (avoids floating precision issues)
            CameraHolder.localPosition = targetHolderPos;
            CameraHolder.localRotation = targetHolderRot;

            Camera.localPosition = targetCamPos;
            Camera.localRotation = targetCamRot;
        }



    }

    private bool DetermineIfHealing()
    {

        if (AttackerInfo.weapon == null || AttackerInfo.weapon.type == "" || AttackerInfo.weapon.type == null || AttackerInfo.weapon.type.ToLower() != "staff")
        {
            return false;
        }

        string attackeraffiliation = AttackerInfo.character.affiliation.ToLower();

        string defenderaffiliation = DefenderInfo.character.affiliation.ToLower();

        if (attackeraffiliation == defenderaffiliation)
        {
            return true;
        }

        if (attackeraffiliation == "other" && !AttackerInfo.character.attacksfriends && defenderaffiliation == "playable")
        {
            return true;
        }

        if (defenderaffiliation == "other" && !DefenderInfo.character.attacksfriends && attackeraffiliation == "playable")
        {
            return true;
        }

        return false;
    }

    public void LoadEnvironment(string ChapterToLoad, Scene SceneToLoadin)
    {
        StartCoroutine(LoadEnvironmentAsync(ChapterToLoad, SceneToLoadin));
    }

    private IEnumerator LoadEnvironmentAsync(string ChapterToLoad, Scene SceneToLoadin)
    {
        int Chapter = -1;
        if (ChapterToLoad.Contains("Chapter"))
        {
            ChapterToLoad = ChapterToLoad.Replace("Chapter", "");
            Chapter = int.Parse(ChapterToLoad);
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
            if (env.ChapterInWhichToUse.Contains(Chapter))
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
                    Destroy(currentenv);
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

#if UNITY_EDITOR
    [ContextMenu("CopyEnvironments")]
    void CopyEnvironments()
    {
        List<CombatEnvirnoment> newcombatenvlist = new List<CombatEnvirnoment>();
        foreach (CombaSceneManager.CombatEnvirnoment env in GetComponent<CombaSceneManager>().EnvirnomentList)
        {
            CombatEnvirnoment combatEnvirnoment = new CombatEnvirnoment();
            combatEnvirnoment.ChapterInWhichToUse = env.ChapterInWhichToUse;
            combatEnvirnoment.Model = env.Model;
            combatEnvirnoment.position = env.position;
            combatEnvirnoment.rotation = env.rotation;
            newcombatenvlist.Add(combatEnvirnoment);
        }
        EnvirnomentList = newcombatenvlist;
    }



#endif

}
