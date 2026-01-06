using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private CombaSceneManager CombatSceneManager;

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
    void Update()
    {

        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            if (CombatSceneManager == null)
            {
                CombatSceneManager = FindAnyObjectByType<CombaSceneManager>();
            }

            if (CombatSceneManager.waittingforexp)
            {
                if (filledupbardelaycounter == 0)
                {
                    currentnumber += Speedpersecond * Time.fixedDeltaTime;
                    if (currentnumber > 99)
                    {
                        Expbar.fillAmount = (currentnumber - 100f) / 100f;
                        SetupLevelUpText();

                    }
                    else
                    {
                        Expbar.fillAmount = currentnumber / 100f;

                    }

                    if (currentnumber >= targetnumber)
                    {
                        filledupbardelaycounter = (int)(filledupbardelay / Time.fixedDeltaTime);
                        if (currentnumber > 99)
                        {
                            filledupbardelaycounter += (int)(2f * filledupbardelay / Time.fixedDeltaTime);
                        }

                    }
                }
                else
                {
                    filledupbardelaycounter--;
                    if (filledupbardelaycounter <= 0)
                    {
                        CombatSceneManager.expdistributed = true;
                        setupdone = false;
                        LevelUpText.transform.parent.gameObject.SetActive(false);
                    }
                }

            }
        }
        else
        {
            if (AttackTurnScript == null)
            {
                AttackTurnScript = FindAnyObjectByType<AttackTurnScript>();
            }

            if (AttackTurnScript.waittingforexp)
            {
                if (filledupbardelaycounter == 0)
                {
                    currentnumber += Speedpersecond * Time.fixedDeltaTime;
                    if (currentnumber > 99)
                    {
                        Expbar.fillAmount = (currentnumber - 100f) / 100f;
                        SetupLevelUpText();

                    }
                    else
                    {
                        Expbar.fillAmount = currentnumber / 100f;

                    }

                    if (currentnumber >= targetnumber)
                    {
                        filledupbardelaycounter = (int)(filledupbardelay / Time.fixedDeltaTime);
                        if (currentnumber > 99)
                        {
                            filledupbardelaycounter += (int)(2f * filledupbardelay / Time.fixedDeltaTime);
                        }

                    }
                }
                else
                {
                    filledupbardelaycounter--;
                    if (filledupbardelaycounter <= 0)
                    {
                        AttackTurnScript.expdistributed = true;
                        setupdone = false;
                        LevelUpText.transform.parent.gameObject.SetActive(false);
                    }
                }

            }


        }



    }

    private void SetupLevelUpText()
    {
        if (levelupbonuses != null && levelupbonuses.Count > 0)
        {
            LevelUpText.transform.parent.gameObject.SetActive(true);
            string leveluptext = "Level Up !\n";
            leveluptext += newcharacter.name + "\n";
            leveluptext += "Level " + (newcharacter.level - 1) + " > <color=blue>" + newcharacter.level + "</color>\n";
            if (levelupbonuses[0] >= 1)
            {
                leveluptext += "Health " + (newcharacter.AjustedStats.HP - levelupbonuses[0]) + " > <color=blue>" + newcharacter.AjustedStats.HP + "</color>\n";
            }
            else
            {
                leveluptext += "Health " + (newcharacter.AjustedStats.HP) + "\n";
            }
            if (levelupbonuses[1] >= 1)
            {
                leveluptext += "Strength " + (newcharacter.AjustedStats.Strength - levelupbonuses[1]) + " > <color=blue>" + newcharacter.AjustedStats.Strength + "</color>\n";
            }
            else
            {
                leveluptext += "Strength " + (newcharacter.AjustedStats.Strength) + "\n";
            }
            if (levelupbonuses[2] >= 1)
            {
                leveluptext += "Psyche " + (newcharacter.AjustedStats.Psyche - levelupbonuses[2]) + " > <color=blue>" + newcharacter.AjustedStats.Psyche + "</color>\n";
            }
            else
            {
                leveluptext += "Psyche " + (newcharacter.AjustedStats.Psyche) + "\n";
            }
            if (levelupbonuses[3] >= 1)
            {
                leveluptext += "Defense " + (newcharacter.AjustedStats.Defense - levelupbonuses[3]) + " > <color=blue>" + newcharacter.AjustedStats.Defense + "</color>\n";
            }
            else
            {
                leveluptext += "Defense " + (newcharacter.AjustedStats.Defense) + "\n";
            }
            if (levelupbonuses[4] >= 1)
            {
                leveluptext += "Resistance " + (newcharacter.AjustedStats.Resistance - levelupbonuses[4]) + " > <color=blue>" + newcharacter.AjustedStats.Resistance + "</color>\n";
            }
            else
            {
                leveluptext += "Resistance " + (newcharacter.AjustedStats.Resistance) + "\n";
            }
            if (levelupbonuses[5] >= 1)
            {
                leveluptext += "Speed " + (newcharacter.AjustedStats.Speed - levelupbonuses[5]) + " > <color=blue>" + newcharacter.AjustedStats.Speed + "</color>\n";
            }
            else
            {
                leveluptext += "Speed " + (newcharacter.AjustedStats.Speed) + "\n";
            }
            if (levelupbonuses[6] >= 1)
            {
                leveluptext += "Dexterity " + (newcharacter.AjustedStats.Dexterity - levelupbonuses[6]) + " > <color=blue>" + newcharacter.AjustedStats.Dexterity + "</color>\n";
            }
            else
            {
                leveluptext += "Dexterity " + (newcharacter.AjustedStats.Dexterity) + "\n";
            }





            LevelUpText.text = leveluptext;
        }

    }

    public void SetupBar(Character character, int exptogain, List<int> levelupstats = null)
    {
        newcharacter = character;
        setupdone = true;
        currentnumber = character.experience - exptogain;
        targetnumber = character.experience;
        if (currentnumber < 0)
        {
            currentnumber = character.experience - exptogain + 100f;
            targetnumber = character.experience + 100;
            levelupbonuses = levelupstats;
        }
        Expbar.fillAmount = currentnumber / 100f;
    }
}
