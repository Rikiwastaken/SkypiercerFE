using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnitScript;

public class ExpBarScript : MonoBehaviour
{
    public Image Expbar;

    public int Speedpersecond;

    private int targetnumber;

    private float currentnumber;

    public bool setupdone;

    private AttackTurnScript AttackTurnScript;

    public int filledupbardelaycounter;
    public float filledupbardelay;

    public TextMeshProUGUI LevelUpText;

    private List<int> levelupbonuses;

    private Character newcharacter;

    private void Start()
    {
        Expbar.fillAmount = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(AttackTurnScript == null)
        {
            AttackTurnScript = FindAnyObjectByType<AttackTurnScript>();
        }

        if(AttackTurnScript.waittingforexp)
        {
            if(filledupbardelaycounter==0)
            {
                currentnumber += Speedpersecond * Time.fixedDeltaTime;
                if(currentnumber >99)
                {
                    Expbar.fillAmount = (currentnumber-100f) / 100f;
                    SetupLevelUpText();

                }
                else
                {
                    Expbar.fillAmount = currentnumber / 100f;

                }

                if (currentnumber >= targetnumber)
                {
                    filledupbardelaycounter = (int)(filledupbardelay / Time.fixedDeltaTime);
                    if (currentnumber>99)
                    {
                        filledupbardelaycounter += (int)(2f*filledupbardelay / Time.fixedDeltaTime);
                    }
                    
                }
            }
            else
            {
                filledupbardelaycounter--;
                if( filledupbardelaycounter <= 0 )
                {
                    AttackTurnScript.expdistributed = true;
                    setupdone = false;
                    LevelUpText.transform.parent.gameObject.SetActive(false);
                }
            }
            
        }

        

    }

    private void SetupLevelUpText()
    {
        if(levelupbonuses.Count > 0)
        {
            LevelUpText.transform.parent.gameObject.SetActive(true);
            string leveluptext = "Level Up !\n";
            leveluptext += newcharacter.name + "\n";
            leveluptext += "Level " + (newcharacter.level - 1) + " > <color=blue>" + newcharacter.level + "</color>\n";
            if (levelupbonuses[0] >= 1)
            {
                leveluptext += "Health " + (newcharacter.stats.HP - levelupbonuses[0]) + " > <color=blue>" + newcharacter.stats.HP + "</color>\n";
            }
            else
            {
                leveluptext += "Health " + (newcharacter.stats.HP) + "\n";
            }
            if (levelupbonuses[1] >= 1)
            {
                leveluptext += "Strength " + (newcharacter.stats.Strength - levelupbonuses[1]) + " > <color=blue>" + newcharacter.stats.Strength + "</color>\n";
            }
            else
            {
                leveluptext += "Strength " + (newcharacter.stats.Strength) + "\n";
            }
            if (levelupbonuses[2] >= 1)
            {
                leveluptext += "Psyche " + (newcharacter.stats.Psyche - levelupbonuses[2]) + " > <color=blue>" + newcharacter.stats.Psyche + "</color>\n";
            }
            else
            {
                leveluptext += "Psyche " + (newcharacter.stats.Psyche) + "\n";
            }
            if (levelupbonuses[3] >= 1)
            {
                leveluptext += "Defense " + (newcharacter.stats.Defense - levelupbonuses[3]) + " > <color=blue>" + newcharacter.stats.Defense + "</color>\n";
            }
            else
            {
                leveluptext += "Defense " + (newcharacter.stats.Defense) + "\n";
            }
            if (levelupbonuses[4] >= 1)
            {
                leveluptext += "Resistance " + (newcharacter.stats.Resistance - levelupbonuses[4]) + " > <color=blue>" + newcharacter.stats.Resistance + "</color>\n";
            }
            else
            {
                leveluptext += "Resistance " + (newcharacter.stats.Resistance) + "\n";
            }
            if (levelupbonuses[5] >= 1)
            {
                leveluptext += "Speed " + (newcharacter.stats.Speed - levelupbonuses[5]) + " > <color=blue>" + newcharacter.stats.Speed + "</color>\n";
            }
            else
            {
                leveluptext += "Speed " + (newcharacter.stats.Speed) + "\n";
            }
            if (levelupbonuses[6] >= 1)
            {
                leveluptext += "Dexterity " + (newcharacter.stats.Dexterity - levelupbonuses[6]) + " > <color=blue>" + newcharacter.stats.Dexterity + "</color>\n";
            }
            else
            {
                leveluptext += "Dexterity " + (newcharacter.stats.Dexterity) + "\n";
            }





            LevelUpText.text = leveluptext;
        }
        
    }

    public void SetupBar(Character character, int exptogain, List<int> levelupstats = null)
    {
        newcharacter = character;
        setupdone = true;
        currentnumber = character.experience- exptogain;
        targetnumber = character.experience;
        if (currentnumber<0)
        {
            currentnumber = character.experience - exptogain + 100f;
            targetnumber = character.experience + 100;
            levelupbonuses = levelupstats;
        }
        
        Expbar.fillAmount = currentnumber / 100f;
    }
}
