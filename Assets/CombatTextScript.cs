using TMPro;
using UnityEngine;
using static UnitScript;
using UnityEngine.UI;
public class CombatTextScript : MonoBehaviour
{

    private Character attacker;
    private Character defender;

    public TextMeshProUGUI attackertext;
    public TextMeshProUGUI defendertext;

    public Image AttackerLifebarLost;
    public Image DefenderLifebarLost;
    public Image AttackerLifebarRemaining;
    public Image DefenderLifebarRemaining;

    public TextMeshProUGUI combattext;

    private battlecameraScript battlecameraScript;

    public float fillspeed;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(battlecameraScript == null)
        {
            battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        }

        if(battlecameraScript.incombat && attacker!=null)
        {
            
            AttackerLifebarRemaining.fillAmount = (float)attacker.currentHP / (float)attacker.stats.HP;
            DefenderLifebarRemaining.fillAmount = (float)defender.currentHP / (float)defender.stats.HP;


            float newlost = AttackerLifebarLost.fillAmount;
            if (AttackerLifebarRemaining.fillAmount > newlost)
            {
                newlost = AttackerLifebarRemaining.fillAmount;
            }
            else
            {
                newlost -= fillspeed * Time.fixedDeltaTime;
            }
            AttackerLifebarLost.fillAmount = newlost;

           

            newlost = DefenderLifebarLost.fillAmount;
            if (DefenderLifebarRemaining.fillAmount > newlost)
            {
                newlost = DefenderLifebarRemaining.fillAmount;
            }
            else
            {
                
                newlost -= fillspeed * Time.fixedDeltaTime;
            }

            DefenderLifebarLost.fillAmount = newlost;
        }
        else
        {
            attacker = null;
            defender = null;
            gameObject.SetActive(false);
        }


    }

    public void SetupCombat(Character newattacker,Character newdefender)
    {
        gameObject.SetActive(true);
        attacker = newattacker;
        defender = newdefender;
        AttackerLifebarRemaining.fillAmount = (float)(attacker.currentHP / attacker.stats.HP);
        DefenderLifebarRemaining.fillAmount = (float)(attacker.currentHP / attacker.stats.HP);
        attackertext.text = attacker.name;
        defendertext.text = defender.name;
    }

    public void ResetInfo()
    {
        combattext.text = "";
    }

    public void UpdateInfo(int damage, int hits, int crits, Character newattacker, Character newdefender, bool healing = false)
    {
        if (healing)
        {
            combattext.text += "\n" + newdefender.name + " gained " + damage + " Health.";
        }
        else
        {
            int finaldamage = damage * hits + 2 * damage * crits;
            if (hits == 0)
            {
                combattext.text = newattacker.name + " missed " + newdefender.name + ".";
            }
            else if (hits == 1)
            {
                combattext.text = newattacker.name + " hit " + newdefender.name + ".";
            }
            else
            {
                combattext.text = newattacker.name + " hit " + newdefender.name + " " + hits + " times.";
            }
            if (crits == 1)
            {
                combattext.text += "\n Critical hit !";
            }
            else if (crits > 1)
            {
                combattext.text += "\n Critical hit " + crits + " times !";
            }
            combattext.text += "\n" + newdefender.name + " lost " + finaldamage + " Health.";
        }
            
    }
}
