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

    public Image ExpBarImage;

    public TextMeshProUGUI combattext;

    private cameraScript cameraScript;

    public ActionsMenu ActionsMenu;

    public float fillspeed;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(cameraScript == null)
        {
            cameraScript = FindAnyObjectByType<cameraScript>();
        }

        

        if (cameraScript.incombat && attacker!=null)
        {
            
            AttackerLifebarRemaining.fillAmount = (float)attacker.currentHP / (float)attacker.AjustedStats.HP;
            DefenderLifebarRemaining.fillAmount = (float)defender.currentHP / (float)defender.AjustedStats.HP;


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

    public void SetupCombat(GameObject newattacker, GameObject newdefender)
    {
        gameObject.SetActive(true);
        attacker = newattacker.GetComponent<UnitScript>().UnitCharacteristics;
        defender = newdefender.GetComponent<UnitScript>().UnitCharacteristics;
        AttackerLifebarRemaining.fillAmount = (float)(attacker.currentHP / attacker.AjustedStats.HP);
        DefenderLifebarRemaining.fillAmount = (float)(defender.currentHP / defender.AjustedStats.HP);
        AttackerLifebarLost.fillAmount = (float)(attacker.currentHP / attacker.AjustedStats.HP);
        DefenderLifebarLost.fillAmount = (float)(defender.currentHP / defender.AjustedStats.HP);

        string attackerText = attacker.name+"\n";
        string defenderText = defender.name + "\n";
        if (newattacker.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower()=="staff")
        {
            int healing = ActionsMenu.CalculateHealing(newattacker);
            attackerText += "healing: " + healing+"  hit: 100%  crit: NA";
            defenderText += "dmg: NA  hit: NA  crit: NA";
        }
        else
        {
            (GameObject doubler, bool istriple) = ActionsMenu.CalculatedoubleAttack(newattacker, newdefender);


            int damagedealtattacker = ActionsMenu.CalculateDamage(newattacker, newdefender);
            int hitrateattacker = ActionsMenu.CalculateHit(newattacker, newdefender);
            int critrateattacker = ActionsMenu.CalculateCrit(newattacker, newdefender);

            attackerText += "dmg: " + damagedealtattacker;
            if(doubler==newattacker)
            {
                if(istriple)
                {
                    attackerText += "x3";
                }
                else
                {
                    attackerText += "x2";
                }
                    
            }
            attackerText += "  hit: " + hitrateattacker + "%  crit: " + critrateattacker+"%";


            int damagedealtDefender = ActionsMenu.CalculateDamage(newdefender, newattacker);
            int hitrateDefender = ActionsMenu.CalculateHit(newdefender, newattacker);
            int critrateDefender = ActionsMenu.CalculateCrit(newdefender, newattacker);

            defenderText += "dmg: " + damagedealtDefender;
            if (doubler == newdefender)
            {
                if (istriple)
                {
                    defenderText += "x3";
                }
                else
                {
                    defenderText += "x2";
                }

            }
            defenderText += "  hit: " + hitrateDefender + "%  crit: " + critrateDefender + "%";

        }
        


        attackertext.text = attackerText;
        defendertext.text = defenderText;
    }

    public void ResetInfo()
    {
        combattext.text = "";
    }

    public void UpdateInfo(int damage, int hits, int crits, Character newattacker, Character newdefender, bool healing = false)
    {
        if (healing)
        {
            combattext.text = "\n" + newdefender.name + " gained " + damage + " Health.";
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
