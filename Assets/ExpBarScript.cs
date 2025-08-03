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

    private EnemyTurnScript EnemyTurnScript;

    public int filledupbardelaycounter;
    public float filledupbardelay;

    private void Start()
    {
        Expbar.fillAmount = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(EnemyTurnScript == null)
        {
            EnemyTurnScript = FindAnyObjectByType<EnemyTurnScript>();
        }

        if(EnemyTurnScript.waittingforexp)
        {
            if(filledupbardelaycounter==0)
            {
                currentnumber += Speedpersecond * Time.fixedDeltaTime;
                if(currentnumber >99)
                {
                    Expbar.fillAmount = (currentnumber-100f) / 100f;

                }
                else
                {
                    Expbar.fillAmount = currentnumber / 100f;

                }

                if (currentnumber >= targetnumber)
                {
                    filledupbardelaycounter = (int)(filledupbardelay / Time.fixedDeltaTime);
                }
            }
            else
            {
                filledupbardelaycounter--;
                if( filledupbardelaycounter <= 0 )
                {
                    EnemyTurnScript.expdistributed = true;
                    setupdone = false;
                }
            }
            
        }

        

    }

    public void SetupBar(Character Character, int exptogain)
    {
        setupdone = true;
        currentnumber = Character.experience- exptogain;
        targetnumber = Character.experience;
        if (currentnumber<0)
        {
            currentnumber = Character.experience - exptogain + 100f;
            targetnumber = Character.experience + 100;
        }
        
        Expbar.fillAmount = currentnumber / 100f;
    }
}
