using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;

public class CombatSceneManagerV2 : MonoBehaviour
{

    [Serializable]
    public class CharacterBattleInfo
    {
        public Character character;
        public equipment weapon;
        public List<int> attacks;

        public void getweapon()
        {
            if (character != null && character.equipments != null && character.equipments.Count > 0)
            {
                weapon = character.equipments[0];
            }
        }

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
            TMPComponent.rectTransform.anchoredPosition = BasePos;
        }

    }

    [Header("Scene Variables")]
    public InfoText AttackerName = new InfoText() { name = "AttackerName" };
    public InfoText AttackerDescription = new InfoText() { name = "AttackerDescription" };
    public InfoText DefenderName = new InfoText() { name = "DefenderName" };
    public InfoText DefenderDescription = new InfoText() { name = "DefenderDescription" };
    public GameObject AttackerGO;
    public Vector3 AttackerStartpos;
    public GameObject DefenderGO;
    public Vector3 DefenderStartpos;
    public float timetoshownames;
    public float DistanceForMelee;
    public float TimeToWalkToEnnemies;
    public float TimeBetweenAttacks;

    [Header("Scene Unwinding Variables")]
    //PreBattle

    public float timelookingatattacker;
    public float timetolookatdefender;



    [Header("Battle Variables")]
    public CharacterBattleInfo AttackerInfo;
    public CharacterBattleInfo DefenderInfo;
    public bool ishealing;
    public bool isinBeforeBattleStarts;
    public bool isinAttackerTurn;
    public bool isinDefenderTurn;
    public bool isinEndOfCombat;

    public void SetupScene(Character attacker, Character defender, List<int> attackerattacks, List<int> defenderattacks, int attackerhitrate, int attackercritrate, int attackerdamage, int defenderhitrate, int defendercritrate, int defenderdamage) // list<int> for attacks: -1 are dodges, >=0 are damages
    {

        //Filling out base classes

        AttackerInfo = new CharacterBattleInfo()
        {
            character = attacker,
            attacks = attackerattacks,
        };

        AttackerInfo.getweapon();

        DefenderInfo = new CharacterBattleInfo()
        {
            character = defender,
            attacks = defenderattacks,
        };

        DefenderInfo.getweapon();

        //Determine if attacker is healing defender or not

        ishealing = DetermineIfHealing();

        // Prefill Names and descriptions

        AttackerName.TMPComponent.text = AttackerInfo.character.name;
        AttackerDescription.TMPComponent.text = CreateDescriptionString(attackerattacks, attackerhitrate, attackercritrate, attackerdamage);
        DefenderName.TMPComponent.text = DefenderInfo.character.name;
        DefenderDescription.TMPComponent.text = CreateDescriptionString(defenderattacks, defenderhitrate, defendercritrate, defenderdamage);

        // Giving GameObjects their Character.

        AttackerGO.GetComponent<UnitScript>().UnitCharacteristics = attacker;
        DefenderGO.GetComponent<UnitScript>().UnitCharacteristics = defender;

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

        //Start the battle
        StartCoroutine(BeforeBattleStarts());
    }


    public IEnumerator BeforeBattleStarts()
    {
        isinBeforeBattleStarts = true;
        // Place Camera in front of the player

        StartCoroutine(ShowText(AttackerName, timetoshownames));

        yield return new WaitForSeconds(timelookingatattacker);

        // Move Camera to back of character to show enemy

        StartCoroutine(ShowText(DefenderName, timetoshownames));

        yield return new WaitForSeconds(timetolookatdefender);
        isinBeforeBattleStarts = false;
        StartCoroutine(AttackerTurn());
    }


    public IEnumerator AttackerTurn()
    {
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
        while (attacksMade < AttackerInfo.attacks.Count)
        {
            PlayAnimation(AttackerGO, 2);

            yield return null;

            yield return new WaitForSeconds(AttackerGO.GetComponent<UnitScript>().ModelList[AttackerInfo.character.modelID].wholeModel.GetComponentInChildren<Animator>().GetCurrentAnimatorClipInfo(0).Length);

            //reaction

            //Show text for damage

            if (ishealing)
            {
                //specialreactionforhealing
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




            yield return new WaitForSeconds(TimeBetweenAttacks);
            attacksMade++;
        }




        //ReactionToDamage && visual popup of damage

        isinAttackerTurn = false;
        StartCoroutine(DefenderTurn());
    }

    public IEnumerator DefenderTurn()
    {
        isinDefenderTurn = true;

        bool isranged = DetermineIfRanged(DefenderInfo);

        isinDefenderTurn = false;
        StartCoroutine(EndOfCombat());
        yield return true;
    }

    public IEnumerator EndOfCombat()
    {
        isinEndOfCombat = true;


        isinEndOfCombat = false;
        yield return true;
    }

    public IEnumerator ShowText(InfoText textClass, float durationofmanoeuver)
    {
        RectTransform rectTransform = textClass.TMPComponent.GetComponent<RectTransform>();

        Vector2 startPosition = rectTransform.anchoredPosition;

        float elapsedTime = 0f;

        while (elapsedTime < durationofmanoeuver)
        {
            elapsedTime += Time.deltaTime;

            float ratio = elapsedTime / durationofmanoeuver;

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, textClass.TargetPos, ratio);

            yield return null; // wait for next frame
        }

        // Ensure final position is exact
        rectTransform.anchoredPosition = textClass.TargetPos;
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

}
