using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnitScript;

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
        public Character attackerBeforeCombat;
        public equipment attackerWeapon;
        public Animator attackerAnimator;
        public Character defender;
        public Character defenderBeforeCombat;
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
        public int expGained;
        public List<int> levelupbonuses;
    }

    [Serializable]
    public class CombatEnvirnoment
    {
        public GameObject Model;
        public Vector3 position;
        public Vector3 rotation;
        public List<int> ChapterInWhichToUse;
    }

    public CombatData ActiveCombatData;

    public GameObject Arrow;

    private GameObject NewArrow;


    [Header("Scene Unraveling")]
    public float timebeforestarting;
    private int timebeforestartcounter;

    public float timebetweenattacks;
    private int timebetweenattackscounter;

    public float timebeforeending;
    private int timebeforeendcounter;

    public bool Attackermoveintoposition;

    public bool attackerLaunchAttack;

    public bool waitForAttackerProjectile;

    public bool DefenderResponse;

    public bool Defendermoveintoposition;

    public bool DefenderLaunchAttack;

    public bool waitForDefenderProjectile;

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

    [Header("Combat Text & Lifebars")]


    public Transform TextCanvas;

    public Image AttackerHPRemaining;
    public Image AttackerHPLost;
    public Image DefenderHPRemaining;
    public Image DefenderHPLost;
    public TextMeshProUGUI AttackerNameTMP;
    public TextMeshProUGUI DefenderNameTMP;
    public TextMeshProUGUI AttackerHPTMP;
    public TextMeshProUGUI DefenderHPTMP;

    public float LowerHPBarTime;
    private int LowerAttackerHPBarTimeCounter;
    private bool ChangeAttackerLifebar;
    private int LowerDefenderHPBarTimeCounter;
    private bool ChangeDefenderLifebar;

    public List<CombatEnvirnoment> EnvirnomentList;

    GameObject currentenv;

    private bool CombatStarted;

    private void Start()
    {
        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
        }
    }

    private void FixedUpdate()
    {


        TextCanvas.LookAt(cam.transform.position);

        if (ActiveCombatData != null)
        {

            if (!waitForAttackerProjectile)
            {

                ManageWeaponPositionResting(attackerSceneGO);
            }

            if (!waitForDefenderProjectile)
            {

                ManageWeaponPositionResting(defenderSceneGO);
            }


            if (timebeforestartcounter > 0)
            {
                timebeforestartcounter--;
            }
            else
            {
                ManageAnimationFlow();
                ManageLifeBars();
            }
        }
    }

    private void ManageLifeBars()
    {
        if (ChangeAttackerLifebar && LowerAttackerHPBarTimeCounter < LowerHPBarTime / Time.fixedDeltaTime)
        {

            LowerAttackerHPBarTimeCounter++;
            float IntialrationofHPAttackerLost = (float)(ActiveCombatData.attackerBeforeCombat.currentHP - ActiveCombatData.defenderdamage) / (float)ActiveCombatData.attackerBeforeCombat.AjustedStats.HP;

            float ratiotToAdd = ((LowerHPBarTime / Time.fixedDeltaTime - (float)LowerAttackerHPBarTimeCounter) / (LowerHPBarTime / Time.fixedDeltaTime)) * ((float)ActiveCombatData.defenderdamage / (float)ActiveCombatData.attackerBeforeCombat.AjustedStats.HP);

            AttackerHPTMP.text = "" + (int)Mathf.Max(((IntialrationofHPAttackerLost + ratiotToAdd) * ActiveCombatData.attackerBeforeCombat.AjustedStats.HP), 0);

            AttackerHPLost.fillAmount = IntialrationofHPAttackerLost + ratiotToAdd;
            AttackerHPRemaining.fillAmount = IntialrationofHPAttackerLost;
        }
        if (ChangeDefenderLifebar && LowerDefenderHPBarTimeCounter < LowerHPBarTime / Time.fixedDeltaTime)
        {

            LowerDefenderHPBarTimeCounter++;
            float IntialrationofHPDefenderLost = (float)(ActiveCombatData.defenderBeforeCombat.currentHP - ActiveCombatData.attackerdamage) / (float)ActiveCombatData.defenderBeforeCombat.AjustedStats.HP;

            float ratiotToAdd = ((LowerHPBarTime / Time.fixedDeltaTime - (float)LowerDefenderHPBarTimeCounter) / (LowerHPBarTime / Time.fixedDeltaTime)) * ((float)ActiveCombatData.attackerdamage / (float)ActiveCombatData.defenderBeforeCombat.AjustedStats.HP);

            DefenderHPTMP.text = "" + (int)Mathf.Max(((IntialrationofHPDefenderLost + ratiotToAdd) * ActiveCombatData.defenderBeforeCombat.AjustedStats.HP), 0);

            DefenderHPLost.fillAmount = IntialrationofHPDefenderLost + ratiotToAdd;
            DefenderHPRemaining.fillAmount = IntialrationofHPDefenderLost;
        }
    }
    private void ManageWeaponPositionResting(GameObject go)
    {
        if (go.GetComponent<UnitScript>().FlyingWeapon != null)
        {
            if (Vector3.Distance(go.GetComponent<UnitScript>().FlyingWeapon.transform.localPosition, go.GetComponent<UnitScript>().telekinesisWeaponPos) > go.GetComponent<UnitScript>().maxmovementrangevertical * 2f)
            {
                go.GetComponent<UnitScript>().FlyingWeapon.transform.localPosition = Vector3.Lerp(go.GetComponent<UnitScript>().FlyingWeapon.transform.localPosition, go.GetComponent<UnitScript>().telekinesisWeaponPos, Time.fixedDeltaTime * 5f);
                go.GetComponent<UnitScript>().FlyingWeapon.transform.localRotation = Quaternion.Lerp(go.GetComponent<UnitScript>().FlyingWeapon.transform.localRotation, Quaternion.Euler(go.GetComponent<UnitScript>().telekinesisWeaponRot), Time.fixedDeltaTime * 5f);
            }
            else
            {
                go.GetComponent<UnitScript>().ManageFlyingWeaponPosition();
            }
        }
    }

    private void ManageAnimationFlow()
    {
        attackbuffer--;
        if (Attackermoveintoposition) // moving attacker to range (if healing or telekinesis or bow, no need to move
        {
            if (ActiveCombatData.attacker.telekinesisactivated || ActiveCombatData.attackerWeapon.type.ToLower() == "staff" || ActiveCombatData.attackerWeapon.type.ToLower() == "bow")
            {
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
            else
            {
                ActiveCombatData.attackerAnimator.SetBool("Walk", true);
                ActiveCombatData.attackerAnimator.transform.position += (AttackerDestination - ActiveCombatData.attackerAnimator.transform.position).normalized * movespeed * Time.fixedDeltaTime;
                if (Vector3.Distance(AttackerDestination, ActiveCombatData.attackerAnimator.transform.position) < 0.1f)
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
        else if (attackerLaunchAttack) // Attacker starts their attack or heal
        {


            MiddleTransform.position = (ActiveCombatData.attackerAnimator.transform.position + ActiveCombatData.defenderAnimator.transform.position) / 2f;


            
            if (ActiveCombatData.attackerWeapon.type.ToLower() == "bow")
            {

                if ((attackerSceneGO.GetComponent<UnitScript>().AttackAnimationAlmostdone(ActiveCombatData.attackerAnimator, 0.8f) && attackbuffer <= 0) || attackbuffer < -2f / Time.deltaTime)
                {
                    NewArrow = Instantiate(Arrow);

                    NewArrow.transform.position = attackerSceneGO.transform.position + new Vector3(0f, 1.5f, 0f);
                    NewArrow.transform.rotation = Quaternion.Euler(new Vector3(180, 180, 90));
                    waitForAttackerProjectile = true;
                    attackerLaunchAttack = false;
                }
            }
            else if (ActiveCombatData.attacker.telekinesisactivated)
            {
                waitForAttackerProjectile = true;
                attackerLaunchAttack = false;

            }
            else
            {
                if ((attackerSceneGO.GetComponent<UnitScript>().AttackAnimationAlmostdone(ActiveCombatData.attackerAnimator, 0.8f) && attackbuffer <= 0) || attackbuffer< -2f/Time.deltaTime)
                {
                    attackerLaunchAttack = false;
                    if (ActiveCombatData.defenderdodged)
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



        }
        else if (waitForAttackerProjectile) // If attacker uses a projectile, we wait while the projectile moves
        {

            if (ActiveCombatData.attackerWeapon.type.ToLower() == "bow")
            {
                ManageProjectileMovement(true, NewArrow);
            }
            else
            {
                ManageProjectileMovement(true);
            }



        }
        else if (DefenderResponse) // Defender does the response animation to the attack
        {
            if (!firsttextcreated)
            {
                CreateText(true);
                if(!ActiveCombatData.defenderdodged)
                {
                    ChangeDefenderLifebar = true;
                }
                
                firsttextcreated = true;
            }

            if (deathcharactercounter > 0)
            {
                deathcharactercounter--;

                if (deathcharactercounter <= 0)
                {
                    DefenderResponse = false;
                    onetoreceiveexp = DetermineWhoGainedExp();
                    AwaitExp = onetoreceiveexp != null;
                    return;
                }
                else
                {
                    return;
                }

            }
            cam.transform.parent = MiddleTransform;

            if (!defenderSceneGO.GetComponent<UnitScript>().isinattackresponseanimation(ActiveCombatData.defenderAnimator))
            {
                DefenderResponse = false;
                if (ActiveCombatData.defenderattacks)
                {

                    Defendermoveintoposition = true;
                }
                else
                {
                    onetoreceiveexp = DetermineWhoGainedExp();
                    AwaitExp = onetoreceiveexp != null;
                }
            }

        }
        else if (Defendermoveintoposition) // moving defender to range(if healing or telekinesis or bow, no need to move)
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
        else if (DefenderLaunchAttack) // Defender attacks
        {
            MiddleTransform.rotation = Quaternion.Lerp(MiddleTransform.rotation, Quaternion.Euler(new Vector3(0, 180, 0)), Time.fixedDeltaTime * CamRotSpeed);


            
            if (ActiveCombatData.defenderWeapon.type.ToLower() == "bow")
            {
                if ((attackerSceneGO.GetComponent<UnitScript>().AttackAnimationAlmostdone(ActiveCombatData.attackerAnimator, 0.8f) && attackbuffer <= 0) || attackbuffer<- 2f / Time.deltaTime)
                {
                    NewArrow = Instantiate(Arrow);

                    NewArrow.transform.position = defenderSceneGO.transform.position + new Vector3(0f, 1.5f, 0f);
                    NewArrow.transform.rotation = Quaternion.Euler(new Vector3(180, 0, 90));
                    waitForDefenderProjectile = true;
                    DefenderLaunchAttack = false;
                }
            }
            else if (ActiveCombatData.defender.telekinesisactivated)
            {
                waitForDefenderProjectile = true;
                DefenderLaunchAttack = false;


            }
            else
            {
                if ((defenderSceneGO.GetComponent<UnitScript>().AttackAnimationAlmostdone(ActiveCombatData.defenderAnimator, 0.8f) && attackbuffer <= 0) || attackbuffer < -2f / Time.deltaTime)
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



        }
        else if (waitForDefenderProjectile) // Wait for defender projectile if any
        {

            if (ActiveCombatData.defenderWeapon.type.ToLower() == "bow")
            {
                ManageProjectileMovement(false, NewArrow);
            }
            else
            {
                ManageProjectileMovement(false);
            }



        }
        else if (AttackerResponse) // Attacker animation answer for damage
        {
            if (!secondtextcreated)
            {
                CreateText(false);
                if(!ActiveCombatData.attackerdodged)
                {
                    ChangeAttackerLifebar = true;
                }
                secondtextcreated = true;
            }

            if (!attackerSceneGO.GetComponent<UnitScript>().isinattackresponseanimation(ActiveCombatData.attackerAnimator))
            {
                AttackerResponse = false;
                onetoreceiveexp = DetermineWhoGainedExp();
                AwaitExp = onetoreceiveexp != null;
            }
            if (deathcharactercounter > 0)
            {
                deathcharactercounter--;
                if (deathcharactercounter <= 0)
                {
                    AttackerResponse = false;
                    onetoreceiveexp = DetermineWhoGainedExp();
                    AwaitExp = onetoreceiveexp != null;
                }
            }
        }
        else if (AwaitExp) // Distribute exp
        {
            if (!waittingforexp)
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

            if (expdistributed)
            {
                AwaitExp = false;
            }

        }
        else // Return to Map Scene
        {
            if (timebeforeendcounter > 0)
            {
                timebeforeendcounter--;
            }
            else
            {
                ReturnToMain();
            }
        }
    }

    private void ManageProjectileMovement(bool Attackerturn, GameObject Modeltomove = null)
    {
        if (Attackerturn)
        {

            if (Modeltomove == null)
            {
                Modeltomove = attackerSceneGO.GetComponent<UnitScript>().FlyingWeapon;
                Modeltomove.transform.position = Vector3.Lerp(Modeltomove.transform.position, ActiveCombatData.defenderAnimator.transform.position + new Vector3(0f, 1f, 0f), Time.fixedDeltaTime * 3f);
            }
            else
            {
                Modeltomove.transform.position += (ActiveCombatData.defenderAnimator.transform.position + new Vector3(0f, 1f, 0f) - Modeltomove.transform.position) * Time.fixedDeltaTime * 2f;
            }



            if (Mathf.Abs(Modeltomove.transform.position.x - ActiveCombatData.defenderAnimator.transform.position.x) < 0.1f)
            {
                waitForAttackerProjectile = false;
                if (ActiveCombatData.defenderdodged)
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
                if (Modeltomove == NewArrow)
                {
                    Destroy(NewArrow);
                }
                DefenderResponse = true;
            }
        }
        else
        {
            if (Modeltomove == null)
            {
                Modeltomove = defenderSceneGO.GetComponent<UnitScript>().FlyingWeapon;
                Modeltomove.transform.position = Vector3.Lerp(Modeltomove.transform.position, ActiveCombatData.attackerAnimator.transform.position + new Vector3(0f, 1f, 0f), Time.fixedDeltaTime * 3f);
            }
            else
            {
                Modeltomove.transform.position += (ActiveCombatData.attackerAnimator.transform.position + new Vector3(0f, 1f, 0f) - Modeltomove.transform.position) * Time.fixedDeltaTime * 2f;
            }

            if (Mathf.Abs(Modeltomove.transform.position.x - ActiveCombatData.attackerAnimator.transform.position.x) < 0.1f)
            {
                waitForDefenderProjectile = false;
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
                if (Modeltomove == NewArrow)
                {
                    Destroy(NewArrow);
                }
                AttackerResponse = true;
            }
        }
    }



    private Character DetermineWhoGainedExp()
    {
        Character onetoreceiveexp = null;
        if (!ActiveCombatData.attackerdied && ActiveCombatData.attacker.affiliation == "playable")
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
        if (attackerturn)
        {
            if (ActiveCombatData.healing)
            {
                AnimText.InitializeText("1 hit\n" + ActiveCombatData.attackerdamage, Color.green);
            }
            else
            {
                string attacktext = "";

                Color colortouse = Color.white;

                if (ActiveCombatData.defenderdodged)
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
                    if (ActiveCombatData.attackercrits == 1)
                    {
                        attacktext += " Critical !\n";
                        colortouse = Color.yellow;
                    }
                    else if (ActiveCombatData.attackercrits > 1)
                    {
                        attacktext += " Critical x" + ActiveCombatData.attackercrits + " !\n";
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
        Debug.Log("Setting up combat scene with the following data : Attacker - " + attacker.name + ", Defender - " + defender.name + ", Attacker Weapon - " + attackerweapon.Name + ", Defender Weapon - " + defenderweapon.Name + ", Double Attacker - " + (doubleattacker != null ? doubleattacker.name : "None") + ", Triple Hit - " + triplehit.ToString() + ", Healing - " + healing.ToString() + ", Attacker Dodged - " + attackerdodged.ToString() + ", Defender Attacks - " + defenderattacks.ToString() + ", Defender Dodged - " + defenderdodged.ToString() + ", Attacker Died - " + attackerdied.ToString() + ", Defender Died - " + defenderdied.ToString() + ", Exp Gained - " + expgained.ToString() + ", Level Up Bonuses Count - " + (levelupbonuses != null ? levelupbonuses.Count.ToString() : "None") + ", Attacker Before Combat HP - " + attackerbeforecombat.currentHP.ToString() + ", Defender Before Combat HP - " + defenderbeforecombat.currentHP.ToString() + ", Attacker Damage - " + attackerdamage.ToString() + ", Defender Damage - " + defenderdamage.ToString() + ", Attacker Crits - " + attackercrits.ToString() + ", Defender Crits - " + defendercrits.ToString());

        if (!CombatStarted)
        {
            CombatStarted = true;
            attackerSceneGO.GetComponent<UnitScript>().UnitCharacteristics = attacker;
            attackerSceneGO.GetComponent<BattleCharacterScript>().ActivateModel(attacker.modelID);

            defenderSceneGO.GetComponent<UnitScript>().UnitCharacteristics = defender;
            defenderSceneGO.GetComponent<BattleCharacterScript>().ActivateModel(defender.modelID);



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
            newdata.attackerBeforeCombat = attackerbeforecombat;
            newdata.defenderBeforeCombat = defenderbeforecombat;
            newdata.expGained = expgained;
            newdata.levelupbonuses = levelupbonuses;

            if(newdata.doubleAttacker == attacker)
            {
                if(newdata.triplehit)
                {

                    if(newdata.attackercrits ==3)
                    {
                        newdata.attackerdamage *= 9;
                    }
                    else if(newdata.attackercrits == 2)
                    {
                        newdata.attackerdamage *= 7;
                    }
                    else if (newdata.attackercrits == 1)
                    {
                        newdata.attackerdamage *= 5;
                    }
                    else
                    {
                        newdata.attackerdamage *= 3;
                    }
                }
                else
                {
                    if (newdata.attackercrits == 2)
                    {
                        newdata.attackerdamage *= 6;
                    }
                    else if (newdata.attackercrits == 1)
                    {
                        newdata.attackerdamage *= 4;
                    }
                    else
                    {
                        newdata.attackerdamage *= 2;
                    }
                }
            }
            else
            {
                if(newdata.attackercrits>0)
                {
                    newdata.attackerdamage *= 3;
                }
            }

            if (newdata.doubleAttacker == defender)
            {
                if (newdata.triplehit)
                {

                    if (newdata.defendercrits == 3)
                    {
                        newdata.defenderdamage *= 9;
                    }
                    else if (newdata.defendercrits == 2)
                    {
                        newdata.defenderdamage *= 7;
                    }
                    else if (newdata.defendercrits == 1)
                    {
                        newdata.defenderdamage *= 5;
                    }
                    else
                    {
                        newdata.defenderdamage *= 3;
                    }
                }
                else
                {
                    if (newdata.defendercrits == 2)
                    {
                        newdata.defenderdamage *= 6;
                    }
                    else if (newdata.defendercrits == 1)
                    {
                        newdata.defenderdamage *= 4;
                    }
                    else
                    {
                        newdata.defenderdamage *= 2;
                    }
                }
            }
            else
            {
                if (newdata.defendercrits > 0)
                {
                    newdata.defenderdamage *= 3;
                }
            }

            newdata.attackerdodged = attackerdodged;
            newdata.defenderdodged = defenderdodged;

            attackerSceneGO.GetComponent<UnitScript>().UpdateWeaponModel(newdata.attackerAnimator, 1f);
            defenderSceneGO.GetComponent<UnitScript>().UpdateWeaponModel(newdata.defenderAnimator, 1f);

            newdata.attackerAnimator.SetBool("Ismachine", attacker.enemyStats.monsterStats.ismachine);
            newdata.defenderAnimator.SetBool("Ismachine", defender.enemyStats.monsterStats.ismachine);
            newdata.attackerAnimator.SetBool("Ispluvial", attacker.enemyStats.monsterStats.ispluvial);
            newdata.defenderAnimator.SetBool("Ispluvial", defender.enemyStats.monsterStats.ispluvial);
            newdata.defenderAnimator.SetBool("UsingTelekinesis", defender.telekinesisactivated);
            newdata.attackerAnimator.SetBool("UsingTelekinesis", attacker.telekinesisactivated);


            ResetScene(newdata.attackerAnimator.transform, newdata.defenderAnimator.transform);

            AttackerDestination = (newdata.defenderAnimator.transform.position - newdata.attackerAnimator.transform.position) * 0.9f + newdata.attackerAnimator.transform.position;

            DefenderDestination = (newdata.attackerAnimator.transform.position - newdata.defenderAnimator.transform.position) * 0.9f + newdata.defenderAnimator.transform.position;

            ActiveCombatData = newdata;
            Attackermoveintoposition = true;

            cam.transform.position = cameraStartPos;
            cam.transform.rotation = Quaternion.Euler(cameraStartRotation);
            cam.transform.parent = newdata.attackerAnimator.transform;

            MiddleTransform.position = new Vector3((ActiveCombatData.attackerAnimator.transform.position.x + ActiveCombatData.defenderAnimator.transform.position.x) / 2f, 0f, 0f);


            float attackerHPRatio = (float)ActiveCombatData.attackerBeforeCombat.currentHP / (float)ActiveCombatData.attackerBeforeCombat.AjustedStats.HP;
            float defenderHPRatio = (float)ActiveCombatData.defenderBeforeCombat.currentHP / (float)ActiveCombatData.defenderBeforeCombat.AjustedStats.HP;

            AttackerHPTMP.text = "" + (int)(ActiveCombatData.attackerBeforeCombat.currentHP);
            DefenderHPTMP.text = "" + (int)(ActiveCombatData.defenderBeforeCombat.currentHP);

            Debug.Log("attacker ratio : " + attackerHPRatio);
            Debug.Log("defender ratio : " + defenderHPRatio);

            AttackerHPRemaining.fillAmount = attackerHPRatio;
            AttackerHPLost.fillAmount = attackerHPRatio;

            DefenderHPLost.fillAmount = defenderHPRatio;
            DefenderHPRemaining.fillAmount = defenderHPRatio;

            AttackerNameTMP.text = attacker.name;
            DefenderNameTMP.text = defender.name;

            ExpBarScript.gameObject.SetActive(false);
            ExpBarScript.LevelUpText.transform.parent.gameObject.SetActive(false);
        }


    }


    private void ReturnToMain()
    {
        waittingforexp = false;
        expdistributed = false;
        CombatStarted = false;
        ExpBarScript.gameObject.SetActive(false);
        ExpBarScript.LevelUpText.transform.parent.gameObject.SetActive(false);
        if (FindAnyObjectByType<CombatSceneLoader>() != null)
        {
            FindAnyObjectByType<CombatSceneLoader>().ActivateMainScene();
        }

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
        secondtextcreated = false;
        LowerAttackerHPBarTimeCounter = 0;
        LowerDefenderHPBarTimeCounter = 0;
        ChangeAttackerLifebar = false;
        ChangeDefenderLifebar = false;
    }


    private void PlayAnimation(Character charactertoAnimate, int animationtype, bool doubleattack = false, bool tripleattack = false, bool healing = false)
    {

        Animator animator = null;
        UnitScript CharacterUnitscript = null;
        if (charactertoAnimate == ActiveCombatData.attacker)
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

    public void LoadEnvironment(string ChapterToLoad, Scene SceneToLoadin)
    {
        StartCoroutine(LoadEnvironmentAsync(ChapterToLoad, SceneToLoadin));
    }

    private IEnumerator LoadEnvironmentAsync(string ChapterToLoad, Scene SceneToLoadin)
    {
        Debug.Log(ChapterToLoad);
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
            Chapter = UnityEngine.Random.Range(0, 1);
        }

        Debug.Log(Chapter);

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
}
