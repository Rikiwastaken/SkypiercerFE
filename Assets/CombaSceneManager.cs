using System;
using UnityEngine;
using static UnitScript;
using System.Collections.Generic;
using UnityEngineInternal;
using UnityEngine.SceneManagement;
using UnityEngine.ProBuilder.MeshOperations;

public class CombaSceneManager : MonoBehaviour
{

    public GameObject attackerSceneGO;
    public GameObject defenderSceneGO;
    public ExpBarScript ExpBarScript;
    public AnimationCombatTextScript AnimText;

    [Serializable]
    public class CombatData
    {
        public Character attacker;
        public equipment attackerWeapon;
        public Animator attackerAnimator;
        public Character defender;
        public equipment defenderWeapon;
        public Animator defenderAnimator;
        public Character doubleAttacker;
        public bool triplehit;
        public bool healing;
        public int attackerdamage;
        public int defenderdamage;
        public int attackercrits;
        public int defendercrits;
        public bool attackerdodged;
        public bool defenderdodged;
        public bool attackerdied;
        public bool defenderdied;
        public bool defenderattacks;
        public GameObject CharacterToLevelUp;
        public int expGained;
        public List<int> levelupbonuses;
    }

    public CombatData ActiveCombatData;


    [Header("Scene Unraveling")]
    public float timebeforestarting;
    private int timebeforestartcounter;

    public float timebetweenattacks;
    private int timebetweenattackscounter;

    public float timebeforeending;
    private int timebeforeendcounter;

    public bool Attackermoveintoposition;

    public bool attackerLaunchAttack;

    public bool DefenderResponse;

    public bool Defendermoveintoposition;

    public bool DefenderLaunchAttack;

    public bool AttackerResponse;

    public bool AwaitExp;

    public float movespeed;

    private Vector3 AttackerDestination;
    private Vector3 DefenderDestination;


    private int deathcharactercounter;

    public float deathtimeoutduration;

    private int attackbuffer;

    public float attackbufferduration;

    public bool waittingforexp;

    public bool expdistributed;

    private bool firsttextcreated;

    private bool secondtextcreated;

    [Header("Camera Positions")]

    public Camera cam;

    public Vector3 cameraStartPos;
    public Vector3 cameraStartRotation;

    public Vector3 cameraMidCombatPos;
    public Vector3 cameraMidCombatRot;

    public Vector3 cameraExpGainPos;
    public Vector3 cameraExpGainRot;

    public float CamMoveSpeed;
    public float CamRotSpeed;

    public Transform MiddleTransform;

    private Character onetoreceiveexp;

    [Header("Combat Text")]


    public Transform TextCanvas;


    private void Start()
    {
        if(FindAnyObjectByType<DataScript>()==null)
        {
            SceneManager.LoadScene("FirstScene");
        }
    }

    private void FixedUpdate()
    {

        TextCanvas.LookAt(cam.transform.position);

        if (ActiveCombatData != null)
        {
            if(timebeforestartcounter>0)
            {
                timebeforestartcounter--;
            }
            else
            {
                ManageAnimationFlow();
            }
        }
    }


    private void ManageAnimationFlow()
    {
        attackbuffer--;
        if (Attackermoveintoposition) // moving attacker to range (if healing or telekinesis or bow, no need to move
        {
            if(ActiveCombatData.attacker.telekinesisactivated || ActiveCombatData.attackerWeapon.type.ToLower()=="staff" || ActiveCombatData.attackerWeapon.type.ToLower() == "bow")
            {
                Attackermoveintoposition = false;
                attackerLaunchAttack = true;

                bool doubleattack = false;
                bool tripleattack = false;

                if (ActiveCombatData.doubleAttacker==ActiveCombatData.attacker)
                {
                    if(ActiveCombatData.triplehit)
                    {
                        tripleattack = true;
                    }
                    else
                    {
                        doubleattack = true;
                    }
                }

                PlayAnimation(ActiveCombatData.attacker, 0, doubleattack, tripleattack, ActiveCombatData.healing);
                attackbuffer = (int)(attackbufferduration / Time.fixedDeltaTime);
            }
            else
            {
                ActiveCombatData.attackerAnimator.SetBool("Walk",true);
                ActiveCombatData.attackerAnimator.transform.position += (AttackerDestination - ActiveCombatData.attackerAnimator.transform.position).normalized * movespeed*Time.fixedDeltaTime;
                if(Vector3.Distance(AttackerDestination, ActiveCombatData.attackerAnimator.transform.position)<0.1f)
                {
                    ActiveCombatData.attackerAnimator.transform.position = AttackerDestination;
                    ActiveCombatData.attackerAnimator.SetBool("Walk", false);
                    Attackermoveintoposition = false;
                    attackerLaunchAttack = true;

                    bool doubleattack = false;
                    bool tripleattack = false;

                    if (ActiveCombatData.doubleAttacker == ActiveCombatData.attacker)
                    {
                        if (ActiveCombatData.triplehit)
                        {
                            tripleattack = true;
                        }
                        else
                        {
                            doubleattack = true;
                        }
                    }

                    PlayAnimation(ActiveCombatData.attacker, 0, doubleattack, tripleattack, ActiveCombatData.healing);
                    attackbuffer = (int)(attackbufferduration / Time.fixedDeltaTime);
                }
            }
        }
        else if(attackerLaunchAttack)
        {


            MiddleTransform.position = (ActiveCombatData.attackerAnimator.transform.position + ActiveCombatData.defenderAnimator.transform.position) / 2f;

            if (attackerSceneGO.GetComponent<UnitScript>().AttackAnimationAlmostdone(ActiveCombatData.attackerAnimator,0.8f) && attackbuffer <= 0)
            {
                attackerLaunchAttack = false;
                if(ActiveCombatData.defenderdodged)
                {
                    ActiveCombatData.defenderAnimator.SetTrigger("Dodge");
                }
                else if (ActiveCombatData.defenderdied)
                {
                    ActiveCombatData.defenderAnimator.SetTrigger("Death");
                    deathcharactercounter = (int)(deathtimeoutduration / Time.fixedDeltaTime);
                }
                else
                {
                    ActiveCombatData.defenderAnimator.SetTrigger("Damage");
                }
                
                DefenderResponse = true;
            }
        }
        else if(DefenderResponse)
        {
            if(!firsttextcreated)
            {
                CreateText(true);
                firsttextcreated = true;
            }
            
            if (!defenderSceneGO.GetComponent<UnitScript>().isinattackresponseanimation(ActiveCombatData.defenderAnimator))
            {
                DefenderResponse = false;
                if (ActiveCombatData.defenderattacks)
                {
                    
                    Defendermoveintoposition = true;
                }
            }
            if(deathcharactercounter>0)
            {
                deathcharactercounter--;
                if(deathcharactercounter <= 0)
                {
                    DefenderResponse = false;
                    if (ActiveCombatData.defenderattacks)
                    {

                        Defendermoveintoposition = true;
                    }
                }
            }
            cam.transform.parent = MiddleTransform;
        }
        else if(Defendermoveintoposition)
        {
            if ((ActiveCombatData.defender.telekinesisactivated || ActiveCombatData.defenderWeapon.type.ToLower() == "staff" || ActiveCombatData.defenderWeapon.type.ToLower() == "bow") || !(ActiveCombatData.attacker.telekinesisactivated || ActiveCombatData.attackerWeapon.type.ToLower() == "staff" || ActiveCombatData.attackerWeapon.type.ToLower() == "bow"))
            {
                if (timebetweenattackscounter > 0)
                {
                    timebetweenattackscounter--;
                }
                else
                {
                    Defendermoveintoposition = false;
                    DefenderLaunchAttack = true;

                    bool doubleattack = false;
                    bool tripleattack = false;

                    if (ActiveCombatData.doubleAttacker == ActiveCombatData.defender)
                    {
                        if (ActiveCombatData.triplehit)
                        {
                            tripleattack = true;
                        }
                        else
                        {
                            doubleattack = true;
                        }
                    }

                    PlayAnimation(ActiveCombatData.defender, 0, doubleattack, tripleattack, ActiveCombatData.healing);
                    attackbuffer = (int)(attackbufferduration / Time.fixedDeltaTime);
                }
                

            }
            else
            {
                MiddleTransform.rotation = Quaternion.Lerp(MiddleTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), Time.fixedDeltaTime * CamRotSpeed);
                if (timebetweenattackscounter > 0)
                {
                    timebetweenattackscounter--;
                }
                else
                {
                    ActiveCombatData.defenderAnimator.SetBool("Walk", true);
                    ActiveCombatData.defenderAnimator.transform.position += (DefenderDestination - ActiveCombatData.defenderAnimator.transform.position).normalized * movespeed * Time.fixedDeltaTime;
                    MiddleTransform.position += (DefenderDestination - ActiveCombatData.defenderAnimator.transform.position).normalized * movespeed * Time.fixedDeltaTime;

                    if (Vector3.Distance(DefenderDestination, ActiveCombatData.defenderAnimator.transform.position) < 0.1f)
                    {
                        ActiveCombatData.defenderAnimator.transform.position = DefenderDestination;
                        ActiveCombatData.defenderAnimator.SetBool("Walk", false);
                        Defendermoveintoposition = false;
                        DefenderLaunchAttack = true;

                        bool doubleattack = false;
                        bool tripleattack = false;

                        if (ActiveCombatData.doubleAttacker == ActiveCombatData.defender)
                        {
                            if (ActiveCombatData.triplehit)
                            {
                                tripleattack = true;
                            }
                            else
                            {
                                doubleattack = true;
                            }
                        }

                        PlayAnimation(ActiveCombatData.defender, 0, doubleattack, tripleattack, ActiveCombatData.healing);
                        attackbuffer = (int)(attackbufferduration / Time.fixedDeltaTime);
                    }
                }
                
            }
            

            
        }
        else if (DefenderLaunchAttack)
        {
            MiddleTransform.rotation = Quaternion.Lerp(MiddleTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), Time.fixedDeltaTime * CamRotSpeed);
            if (defenderSceneGO.GetComponent<UnitScript>().AttackAnimationAlmostdone(ActiveCombatData.defenderAnimator, 0.8f) && attackbuffer<=0)
            {
                DefenderLaunchAttack = false;
                AttackerResponse = true;
                if (ActiveCombatData.attackerdodged)
                {
                    ActiveCombatData.attackerAnimator.SetTrigger("Dodge");
                }
                else if (ActiveCombatData.attackerdied)
                {
                    ActiveCombatData.attackerAnimator.SetTrigger("Death");
                    deathcharactercounter = (int)(deathtimeoutduration / Time.fixedDeltaTime);
                }
                else
                {
                    ActiveCombatData.attackerAnimator.SetTrigger("Damage");
                }
                
            }
        }
        else if(AttackerResponse)
        {
            if(!secondtextcreated)
            {
                CreateText(false);
                secondtextcreated = true;
            }
            
            if (!attackerSceneGO.GetComponent<UnitScript>().isinattackresponseanimation(ActiveCombatData.attackerAnimator))
            {
                AttackerResponse = false;
                onetoreceiveexp = DetermineWhoGainedExp();
                AwaitExp = onetoreceiveexp!=null;
            }
            if (deathcharactercounter > 0)
            {
                deathcharactercounter--;
                if (deathcharactercounter <= 0)
                {
                    AttackerResponse = false;
                }
            }
        }
        else if (AwaitExp)
        {
            if(!waittingforexp)
            {
                waittingforexp = true;
                ExpBarScript.SetupBar(onetoreceiveexp, ActiveCombatData.expGained, ActiveCombatData.levelupbonuses);
                ExpBarScript.gameObject.SetActive(true);
            }
            if (onetoreceiveexp == ActiveCombatData.attacker)
            {
                cam.transform.parent = ActiveCombatData.attackerAnimator.transform;
            }
            else
            {
                cam.transform.parent = ActiveCombatData.defenderAnimator.transform;
            }
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, cameraExpGainPos, Time.fixedDeltaTime * CamMoveSpeed);
            cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, Quaternion.Euler(cameraExpGainRot), Time.fixedDeltaTime * CamRotSpeed);

            if(expdistributed)
            {
                AwaitExp = false;
            }

        }
        else
        {
            if(timebeforeendcounter > 0)
            {
                timebeforeendcounter--;
            }
            else
            {
                ReturnToMain();
            }
        }
    }

    private Character DetermineWhoGainedExp()
    {
        Character onetoreceiveexp = null;
        if(!ActiveCombatData.attackerdied && ActiveCombatData.attacker.affiliation=="playable")
        {
            onetoreceiveexp = ActiveCombatData.attacker;
        }
        else if (!ActiveCombatData.defenderdied && ActiveCombatData.defender.affiliation == "playable")
        {
            onetoreceiveexp = ActiveCombatData.defender;
        }
        return onetoreceiveexp;
    }

    private void CreateText(bool attackerturn)
    {
        if(attackerturn)
        {
            if(ActiveCombatData.healing)
            {
                AnimText.InitializeText("1 hit\n" + ActiveCombatData.attackerdamage, Color.green);
            }
            else
            {
                string attacktext = "";

                Color colortouse = Color.white;

                if(ActiveCombatData.defenderdodged)
                {
                    attacktext = "Miss";
                }
                else
                {
                    if (ActiveCombatData.doubleAttacker == ActiveCombatData.attacker)
                    {
                        if (ActiveCombatData.triplehit)
                        {
                            attacktext = "3 hits\n";
                        }
                        else
                        {
                            attacktext = "2 hits\n";
                        }
                    }
                    else
                    {
                        attacktext = "1 hit\n";
                    }
                    if(ActiveCombatData.attackercrits>0)
                    {
                        attacktext += ActiveCombatData.attackercrits + " crits !\n";
                        colortouse = Color.yellow;
                    }
                    attacktext += ActiveCombatData.attackerdamage;
                }
                AnimText.InitializeText(attacktext, colortouse);
            }
        }
        else
        {
            string attacktext = "";

            Color colortouse = Color.white;

            if (ActiveCombatData.attackerdodged)
            {
                attacktext = "Miss";
            }
            else
            {
                if (ActiveCombatData.doubleAttacker == ActiveCombatData.defender)
                {
                    if (ActiveCombatData.triplehit)
                    {
                        attacktext = "3 hits\n";
                    }
                    else
                    {
                        attacktext = "2 hits\n";
                    }
                }
                else
                {
                    attacktext = "1 hit\n";
                }
                if (ActiveCombatData.defendercrits > 0)
                {
                    attacktext += ActiveCombatData.defendercrits + " crits !\n";
                    colortouse = Color.yellow;
                }
                attacktext += ActiveCombatData.defenderdamage;
            }
            AnimText.InitializeText(attacktext, colortouse);
        }
    }

    public void SetupScene(Character attacker, Character defender, equipment attackerweapon, equipment defenderweapon, Character doubleattacker, bool triplehit, bool healing, bool attackerdodged, bool defenderattacks, bool defenderdodged, bool attackerdied, bool defenderdied, int expgained, List<int> levelupbonuses, Character attackerbeforecombat, Character defenderbeforecombat, int attackerdamage, int defenderdamage, int attackercrits, int defendercrits)
    {
        attackerSceneGO.GetComponent<UnitScript>().UnitCharacteristics = attacker;
        attackerSceneGO.GetComponent<BattleCharacterScript>().ActivateModel(attacker.modelID);
        attackerSceneGO.GetComponent<UnitScript>().UpdateWeaponModel();
        defenderSceneGO.GetComponent<UnitScript>().UnitCharacteristics = defender;
        defenderSceneGO.GetComponent<BattleCharacterScript>().ActivateModel(defender.modelID);
        defenderSceneGO.GetComponent<UnitScript>().UpdateWeaponModel();


        CombatData newdata = new CombatData();
       
        newdata.attacker = attacker;
        newdata.attackerWeapon = attackerweapon;
        newdata.attackerAnimator = attackerSceneGO.GetComponent<UnitScript>().ModelList[attacker.modelID].wholeModel.GetComponentInChildren<Animator>();
        newdata.attackerdied = attackerdied;
        newdata.defender = defender;
        newdata.defenderWeapon = defenderweapon;
        newdata.defenderAnimator = defenderSceneGO.GetComponent<UnitScript>().ModelList[defender.modelID].wholeModel.GetComponentInChildren<Animator>();
        newdata.defenderdied = defenderdied;
        newdata.doubleAttacker = doubleattacker;
        newdata.triplehit = triplehit;
        newdata.healing = healing;
        newdata.defenderattacks = defenderattacks;
        newdata.attackerdamage = attackerdamage;
        newdata.defenderdamage = defenderdamage;
        newdata.attackercrits = attackercrits;
        newdata.defendercrits = defendercrits;



        newdata.attackerAnimator.SetBool("Ismachine", attacker.enemyStats.monsterStats.ismachine);
        newdata.defenderAnimator.SetBool("Ismachine", attacker.enemyStats.monsterStats.ismachine);
        newdata.attackerAnimator.SetBool("Ispluvial", attacker.enemyStats.monsterStats.ispluvial);
        newdata.defenderAnimator.SetBool("Ispluvial", attacker.enemyStats.monsterStats.ispluvial);


        ResetScene(newdata.attackerAnimator.transform, newdata.defenderAnimator.transform);

        AttackerDestination = (newdata.defenderAnimator.transform.position - newdata.attackerAnimator.transform.position) * 0.9f + newdata.attackerAnimator.transform.position;

        DefenderDestination = (newdata.attackerAnimator.transform.position - newdata.defenderAnimator.transform.position) * 0.9f + newdata.defenderAnimator.transform.position;

        ActiveCombatData = newdata;
        Attackermoveintoposition = true;

        cam.transform.position = cameraStartPos;
        cam.transform.rotation = Quaternion.Euler(cameraStartRotation);
        cam.transform.parent = newdata.attackerAnimator.transform;

        MiddleTransform.position = new Vector3((ActiveCombatData.attackerAnimator.transform.position.x + ActiveCombatData.defenderAnimator.transform.position.x) / 2f, 0f, 0f);

        ExpBarScript.gameObject.SetActive(false);
    }


    private void ReturnToMain()
    {
        waittingforexp = false;
        expdistributed = false;
        FindAnyObjectByType<CombatSceneLoader>().ActivateMainScene();
    }

    private void ResetScene(Transform AttackerTransform, Transform DefenderTransform)
    {
        AttackerTransform.localRotation = Quaternion.identity;
        AttackerTransform.transform.localPosition = Vector3.zero;

        DefenderTransform.localRotation = Quaternion.identity;
        DefenderTransform.localPosition = Vector3.zero;

        MiddleTransform.position = Vector3.zero;
        MiddleTransform.rotation = Quaternion.identity;

        timebeforestartcounter = (int)(timebeforestarting / Time.fixedDeltaTime);
        timebeforeendcounter = (int)(timebeforeending / Time.fixedDeltaTime);
        timebetweenattackscounter = (int)(timebetweenattacks / Time.fixedDeltaTime);

        waittingforexp = false;
        firsttextcreated = false;
        secondtextcreated= false;
    }


    private void PlayAnimation(Character charactertoAnimate, int animationtype, bool doubleattack = false, bool tripleattack = false, bool healing = false)
    {

        Animator animator = null;
        UnitScript CharacterUnitscript = null;
        if(charactertoAnimate == ActiveCombatData.attacker)
        {
            animator = ActiveCombatData.attackerAnimator;
            CharacterUnitscript = attackerSceneGO.GetComponent<UnitScript>();
        }
        else
        {
            animator = ActiveCombatData.defenderAnimator;
            CharacterUnitscript = defenderSceneGO.GetComponent<UnitScript>();
        }


        switch (animationtype) // 0 : atk, 1 : walk, 2 : Take Damage, 3 : Dodge, 4 : Die
        {
            case 0:
                CharacterUnitscript.PlayAttackAnimation(doubleattack,tripleattack,healing,animator);
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

}
