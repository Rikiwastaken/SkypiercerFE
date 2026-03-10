using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnitScript;

public class ExpBarScript : MonoBehaviour
{
    public Image Expbar;
    public FreezeFrameCapture FreezeFrame;

    public int Speedpersecond;

    private int targetnumber;

    private float currentnumber;

    public bool setupdone;

    private AttackTurnScript AttackTurnScript;

    private CombaSceneManager CombatSceneManager;

    public int filledupbardelaycounter;
    public float filledupbardelay;

    private List<int> levelupbonuses;

    private Character newcharacter;

    private void Start()
    {
        Expbar.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (FreezeFrame != null && FreezeFrame.ShowingLevelUp)
        {
            return;
        }
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
                    }
                }

            }


        }



    }

    private void SetupLevelUpText()
    {
        if (levelupbonuses != null && levelupbonuses.Count > 0)
        {
            FreezeFrame.PlayFullAnimation(newcharacter, levelupbonuses);
            AttackTurnScript.expdistributed = true;
            gameObject.SetActive(false);
            setupdone = false;
            return;

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
