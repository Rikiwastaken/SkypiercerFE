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
    public FreezeFrameCapture LevelUpFreezeFrame;
    public List<CombatEnvirnoment> EnvirnomentList;
    private GameObject currentenv;


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
        if (AttackerHP.text != "" + AttackerInfo.currentHP)
        {
            AttackerHP.text = "" + AttackerInfo.currentHP;
        }
        if (DefenderHP.text != "" + DefenderInfo.currentHP)
        {
            DefenderHP.text = "" + DefenderInfo.currentHP;
        }
    }

    public void SetupScene(Character attacker, Character defender, List<int> attackerattacks, List<int> attackercriticals, List<int> defenderattacks, List<int> defendercriticals, int attackerhitrate, int attackercritrate, int attackerdamage, int defenderhitrate, int defendercritrate, int defenderdamage, int expgained, List<int> levelupbonuses) // list<int> for attacks: -1 are dodges, >=0 are damages, for criticals, 1=critical, 0=not
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
        //Filling out base classes

        AttackerInfo = new CharacterBattleInfo()
        {
            character = attacker,
            attacks = attackerattacks,
            criticals = attackercriticals,
            currentHP = attacker.currentHP,
        };

        AttackerInfo.getweapon();

        DefenderInfo = new CharacterBattleInfo()
        {
            character = defender,
            attacks = defenderattacks,
            criticals = defendercriticals,
            currentHP = defender.currentHP,
        };

        DefenderInfo.getweapon();

        // Fill out exp and level bonuses

        levelupbonus = levelupbonuses;
        exp = expgained;

        //Determine if attacker is healing defender or not

        ishealing = DetermineIfHealing();

        // Prefill Names and descriptions

        AttackerName.TMPComponent.text = AttackerInfo.character.name;
        AttackerDescription.TMPComponent.text = CreateDescriptionString(attackerattacks, attackerhitrate, attackercritrate, attackerdamage);
        DefenderName.TMPComponent.text = DefenderInfo.character.name;
        DefenderDescription.TMPComponent.text = CreateDescriptionString(defenderattacks, defenderhitrate, defendercritrate, defenderdamage);

        // Giving GameObjects their Character and load their models and get the animators

        AttackerGO.GetComponent<UnitScript>().UnitCharacteristics = attacker;
        AttackerGO.GetComponent<BattleCharacterScript>().ActivateModel(attacker.modelID);
        AttackerInfo.Animator = AttackerGO.GetComponent<UnitScript>().ModelList[AttackerInfo.character.modelID].wholeModel.GetComponentInChildren<Animator>();

        DefenderGO.GetComponent<UnitScript>().UnitCharacteristics = defender;
        DefenderGO.GetComponent<BattleCharacterScript>().ActivateModel(defender.modelID);
        DefenderInfo.Animator = DefenderGO.GetComponent<UnitScript>().ModelList[DefenderInfo.character.modelID].wholeModel.GetComponentInChildren<Animator>();

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

        yield return new WaitForSeconds(timetolookatdefender);

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
            Vector3 startPos = AttackerGO.transform.position;
            Vector3 defenderPos = DefenderGO.transform.position;

            // Direction from attacker to defender
            Vector3 dir = (defenderPos - startPos).normalized;

            // Target position stopping at melee distance
            Vector3 targetPos = defenderPos - dir * DistanceForMelee;

            float elapsed = 0f;

            while (elapsed < TimeToWalkToEnnemies)
            {
                PlayAnimation(AttackerGO, 1);
                elapsed += Time.deltaTime;
                float t = elapsed / TimeToWalkToEnnemies;

                AttackerGO.transform.position = Vector3.Lerp(startPos, targetPos, t);

                yield return null;
            }

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
            string texttoshow = "";
            int HPbeforeDamage = DefenderInfo.currentHP;
            bool iscritical = false;
            if (damage == -1)
            {
                texttoshow = "Miss";
            }
            else
            {
                if (ishealing)
                {
                    DefenderInfo.currentHP += AttackerInfo.attacks[attacksMade];
                }
                else
                {
                    DefenderInfo.currentHP -= AttackerInfo.attacks[attacksMade];
                }


                if (AttackerInfo.criticals != null && AttackerInfo.criticals.Count > attacksMade)
                {
                    iscritical = AttackerInfo.criticals[attacksMade] == 1;
                }
                texttoshow = AttackerInfo.attacks[attacksMade] + "";
            }
            StartCoroutine(ChangeLifeBar(DefenderName.LifeBar, HPbeforeDamage, DefenderInfo.currentHP, (int)DefenderInfo.character.AjustedStats.HP, timeforLifebar));
            GameObject textobject = Instantiate(TextObjectPrefab);
            textobject.GetComponent<CombatNumberPopup>().InitializeTMP(texttoshow, DefenderGO.transform.position, iscritical, ishealing);

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
                Vector3 startPos = DefenderGO.transform.position;
                Vector3 defenderPos = AttackerGO.transform.position;

                // Direction from attacker to defender
                Vector3 dir = (defenderPos - startPos).normalized;

                // Target position stopping at melee distance
                Vector3 targetPos = defenderPos - dir * DistanceForMelee;

                float elapsed = 0f;

                while (elapsed < TimeToWalkToEnnemies)
                {
                    PlayAnimation(DefenderGO, 1);
                    elapsed += Time.deltaTime;
                    float t = elapsed / TimeToWalkToEnnemies;

                    DefenderGO.transform.position = Vector3.Lerp(startPos, targetPos, t);

                    yield return null;
                }

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
                string texttoshow = "";
                bool iscritical = false;
                int HPbeforeDamage = AttackerInfo.currentHP;
                if (damage == -1)
                {
                    texttoshow = "Miss";
                }
                else
                {
                    if (ishealing)
                    {
                        AttackerInfo.currentHP += DefenderInfo.attacks[attacksMade];
                    }
                    else
                    {
                        AttackerInfo.currentHP -= DefenderInfo.attacks[attacksMade];
                    }


                    if (DefenderInfo.criticals != null && DefenderInfo.criticals.Count > attacksMade)
                    {
                        iscritical = DefenderInfo.criticals[attacksMade] == 1;
                    }
                    texttoshow = DefenderInfo.attacks[attacksMade] + "";
                }
                GameObject textobject = Instantiate(TextObjectPrefab);
                textobject.GetComponent<CombatNumberPopup>().InitializeTMP(texttoshow, DefenderGO.transform.position, iscritical, ishealing);

                StartCoroutine(ChangeLifeBar(AttackerName.LifeBar, HPbeforeDamage, AttackerInfo.currentHP, (int)AttackerInfo.character.AjustedStats.HP, timeforLifebar));
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
        }
        else if (DefenderInfo.character.affiliation.ToLower() == "playable" && DefenderInfo.currentHP > 0)
        {
            importantGO = DefenderGO;
            importantInfo = DefenderInfo;
        }

        if (importantGO != null)
        {
            // place la camera pour la fin du combat

            yield return StartCoroutine(ShowExpBar());

            yield return StartCoroutine(ChangeExpBar(importantInfo.character));

            if (ExpBarHolder.GetComponentInChildren<Image>().fillAmount >= 1 || importantInfo.character.experience + exp >= 100)
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

        Image ExpBar = ExpBarHolder.GetComponentInChildren<Image>();

        int baseexpbar = current.experience;

        int targetexp = baseexpbar + exp;

        float elapsedTime = 0f;

        while (elapsedTime < TimeforExpBar && ExpBar.fillAmount < 1)
        {
            elapsedTime += Time.deltaTime;

            float ratio = elapsedTime / TimeforExpBar;

            float FillAmount = (baseexpbar + (targetexp - baseexpbar) * ratio) / 100f;

            ExpBar.fillAmount = FillAmount;

            yield return null; // wait for next frame
        }

        // Ensure final position is exact
        ExpBar.fillAmount = (float)targetexp / 100f;
    }

    private void PlayAnimation(GameObject CharacterToAnimate, int animationtype, bool doubleattack = false, bool tripleattack = false, bool healing = false)
    {

        Animator animator = null;
        UnitScript CharacterUnitscript = null;
        CharacterUnitscript = CharacterToAnimate.GetComponent<UnitScript>();
        animator = CharacterUnitscript.ModelList[CharacterUnitscript.UnitCharacteristics.modelID].wholeModel.GetComponentInChildren<Animator>();

        switch (animationtype) // 0 : atk, 1 : walk, 2 : Take Damage, 3 : Dodge, 4 : Die
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
        }

    }

    private string CreateDescriptionString(List<int> attacks, int hitrate, int critrate, int basedamage)
    {
        if (attacks == null || attacks.Count == 0)
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

    private bool DetermineIfHealing()
    {

        if (AttackerInfo.weapon == null || AttackerInfo.weapon.type.ToLower() != "staff")
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
