using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static DataScript;
using static UnitScript;
public class ActionsMenu : MonoBehaviour
{

    public GameObject target;

    public Button ActionsCancelButton;
    public Button AttackButton;
    public Button AttackCancelButton;

    public GameObject ItemsScript;
    public GameObject CommandGO;
    public GameObject SpecialCommandGO;

    private InputManager inputManager;

    public TextMeshProUGUI unitAttackText;
    public TextMeshProUGUI targetAttackText;
    public Image unitSprite;
    public Image targetSprite;
    public TextMeshProUGUI UnitWeapon;
    public TextMeshProUGUI TargetWeapon;
    public TextMeshProUGUI UnitNameTMP;
    public TextMeshProUGUI TargetNameTMP;
    public GameObject UnitTelekinesis;
    public GameObject TargetTelekinesis;
    public TextMeshProUGUI UnitTileTMP;
    public TextMeshProUGUI TargetTileTMP;

    public Image UnitOrangeLifeBar;
    public Image UnitGreenLifebar;
    public Image TargetOrangeLifeBar;
    public Image TargetGreenLifebar;

    public Sprite EmptySprite;

    private GridScript GridScript;

    public List<GameObject> targetlist;



    private cameraScript cameraScript;

    public int activetargetid;

    public bool confirmattack;
    public int CommandUsedID;

    private Color BaseButtonColor;
    private Color BaseButtonPressedColor;

    public GameObject commandmenu;

    private AttackTurnScript attackTurnScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = GridScript.instance;
        cameraScript = FindAnyObjectByType<cameraScript>();
        attackTurnScript = FindAnyObjectByType<AttackTurnScript>();
    }
    private void OnEnable()
    {
        if (GridScript == null)
        {
            GridScript = GridScript.instance;
        }
        BaseButtonColor = transform.GetChild(0).GetComponent<Button>().colors.normalColor;
        BaseButtonPressedColor = transform.GetChild(0).GetComponent<Button>().colors.pressedColor;
        target = GridScript.GetSelectedUnitGameObject();

        if (target.GetComponent<UnitScript>().GetCommands().Count > 0)
        {
            var colors = transform.GetChild(2).GetComponent<Button>().colors;
            colors.normalColor = BaseButtonColor;
            colors.pressedColor = BaseButtonPressedColor;
            transform.GetChild(2).GetComponent<Button>().colors = colors;
        }
        else
        {

            var colors = transform.GetChild(2).GetComponent<Button>().colors;
            colors.normalColor = Color.gray;
            colors.pressedColor = Color.gray;
            transform.GetChild(2).GetComponent<Button>().colors = colors;
        }

        if (target.GetComponent<UnitScript>().GetSpectialInteraction().Count > 0)
        {
            var colors = transform.GetChild(2).GetComponent<Button>().colors;
            colors.normalColor = BaseButtonColor;
            colors.pressedColor = BaseButtonPressedColor;
            transform.GetChild(3).GetComponent<Button>().colors = colors;
            transform.GetChild(3).GetComponent<Image>().color = new Color(1f, 1f, 0f);
        }
        else
        {

            var colors = transform.GetChild(2).GetComponent<Button>().colors;
            colors.normalColor = Color.gray;
            colors.pressedColor = Color.gray;
            transform.GetChild(3).GetComponent<Button>().colors = colors;
            transform.GetChild(3).GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (GridScript == null)
        {
            GridScript = GridScript.instance;
        }
        SelectionSafeGuard();
        ActionManager.instance.preventfromlockingafteraction = true;


        inputManager = InputManager.instance;


        if (inputManager.canceljustpressed && !ItemsScript.activeSelf && !CommandGO.activeSelf && !AttackButton.transform.parent.gameObject.activeSelf)
        {
            ActionsCancelButton.onClick.Invoke();
        }
        else if (inputManager.canceljustpressed && AttackButton.transform.parent.gameObject.activeSelf)
        {
            AttackCancelButton.onClick.Invoke();
        }

        if (targetlist != null && targetlist.Count > 0)
        {
            if (inputManager.Telekinesisjustpressed)
            {
                ToggleTelekinesis(targetlist[activetargetid]);
            }
            if (inputManager.NextWeaponjustpressed)
            {
                if (!(targetlist[activetargetid].GetComponent<UnitScript>().UnitCharacteristics.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation && target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff"))
                {
                    NextWeapon(targetlist[activetargetid], target.GetComponent<UnitScript>().GetFirstWeapon());
                }


            }
            if (inputManager.PreviousWeaponjustpressed)
            {
                if (!(targetlist[activetargetid].GetComponent<UnitScript>().UnitCharacteristics.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation && target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff"))
                {
                    PreviousWeapon(targetlist[activetargetid], target.GetComponent<UnitScript>().GetFirstWeapon());
                }

            }

            if (inputManager.NextTargetjustpressed)
            {
                if (activetargetid < targetlist.Count - 1)
                {
                    activetargetid++;
                }
                else
                {
                    activetargetid = 0;
                }
                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
                {
                    initializeHealingWindows(target, targetlist[activetargetid]);
                }
                else
                {
                    initializeAttackWindows(target, targetlist[activetargetid]);
                }

            }
            if (inputManager.PreviousTargetjustpressed)
            {
                if (activetargetid > 0)
                {
                    activetargetid--;
                }
                else
                {
                    activetargetid = targetlist.Count - 1;
                }
                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
                {
                    initializeHealingWindows(target, targetlist[activetargetid]);
                }
                else
                {
                    initializeAttackWindows(target, targetlist[activetargetid]);
                }
            }
            if (CommandUsedID == 0)
            {
                if (activetargetid < targetlist.Count)
                {
                    cameraScript.Destination = targetlist[activetargetid].GetComponent<UnitScript>().UnitCharacteristics.position;
                }
                else
                {
                    cameraScript.Destination = target.GetComponent<UnitScript>().UnitCharacteristics.position;
                }
                CheckCorrectInfo(target, targetlist[activetargetid]);
            }
        }
    }

    private void CheckCorrectInfo(GameObject unit, GameObject enemy)
    {
        string unitweaponname = unit.GetComponent<UnitScript>().GetFirstWeapon().Name;
        string enemyweaponname = enemy.GetComponent<UnitScript>().GetFirstWeapon().Name;

        if (!unitAttackText.text.Contains(unitweaponname) || !targetAttackText.text.Contains(enemyweaponname))
        {
            if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
            {
                initializeHealingWindows(target, targetlist[activetargetid]);
            }
            else
            {
                initializeAttackWindows(target, targetlist[activetargetid]);
            }
        }
    }

    private void WeaponChange()
    {


        (int range, bool frapperenmelee, string type) = target.GetComponent<UnitScript>().GetRangeMeleeAndType();
        GridScript.ShowAttackAfterMovement(range, frapperenmelee, target.GetComponent<UnitScript>().UnitCharacteristics.currentTile, type.ToLower() == "staff", target.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.monsterStats.size, target.GetComponent<UnitScript>().UnitCharacteristics);
        GridScript.lockedattacktiles = GridScript.attacktiles;
        GridScript.lockedhealingtiles = GridScript.healingtiles;
        GridScript.Recolor();

    }

    private void NextWeapon(GameObject PreviousFoe, equipment initialweapon)
    {

        target.GetComponent<UnitScript>().GetNextWeapon();
        WeaponChange();
        bool enemytargettable = false;
        foreach (GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if ((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if (enemytargettable || target.GetComponent<UnitScript>().UnitCharacteristics.equipments[0] == initialweapon)
        {
            FindAttackers();
        }
        else
        {
            NextWeapon(PreviousFoe, initialweapon);
        }

    }

    private void PreviousWeapon(GameObject PreviousFoe, equipment initialweapon)
    {
        target.GetComponent<UnitScript>().GetPreviousWeapon();
        WeaponChange();
        bool enemytargettable = false;
        foreach (GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if ((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if (enemytargettable || target.GetComponent<UnitScript>().UnitCharacteristics.equipments[0] == initialweapon)
        {
            FindAttackers();
        }
        else
        {
            PreviousWeapon(PreviousFoe, initialweapon);
        }

    }

    private void ToggleTelekinesis(GameObject PreviousFoe)
    {
        target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
        WeaponChange();
        target.GetComponent<UnitScript>().UpdateWeaponModel();
        bool enemytargettable = false;
        foreach (GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if ((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if (enemytargettable)
        {
            FindAttackers();
        }
        else
        {
            NextWeapon(PreviousFoe, target.GetComponent<UnitScript>().UnitCharacteristics.equipments[0]);
        }
        enemytargettable = false;
        foreach (GridSquareScript tile in GridScript.lockedattacktiles)
        {
            if ((int)tile.GridCoordinates.x == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.x && (int)tile.GridCoordinates.y == PreviousFoe.GetComponent<UnitScript>().UnitCharacteristics.position.y)
            {
                enemytargettable = true;
                break;
            }
        }
        if (enemytargettable)
        {
            FindAttackers();
        }
        else
        {
            target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated = !target.GetComponent<UnitScript>().UnitCharacteristics.telekinesisactivated;
            WeaponChange();
            FindAttackers();
        }
    }

    public void ResetTargets()
    {
        targetlist = null;
    }

    private (List<equipment>, List<int>, bool) previouscharacterstate(Character character)
    {
        List<equipment> previousequipmentstate = new List<equipment>();
        List<int> previousequipmentIDstate = new List<int>();
        for (int i = 0; i < Mathf.Max(character.equipments.Count, character.equipmentsIDs.Count); i++)
        {
            if (i < character.equipments.Count)
            {
                previousequipmentstate.Add(character.equipments[i]);
            }
            if (i < character.equipmentsIDs.Count)
            {
                previousequipmentIDstate.Add(character.equipmentsIDs[i]);
            }
        }
        return (previousequipmentstate, previousequipmentIDstate, character.telekinesisactivated);
    }

    private void ResetCharacterEquipment(Character character, List<equipment> previousequipmentstate, List<int> previousequipmentIDstate, bool previoustelekinesis)
    {
        character.equipments = previousequipmentstate;
        character.equipmentsIDs = previousequipmentIDstate;
        character.telekinesisactivated = previoustelekinesis;
    }

    public void AttackCommand()
    {
        CommandUsedID = 0;
        // on essaie de trouver un combo arme/telekinesie pour pouvoir attaquer un ennemi
        if (target == null)
        {
            return;
        }
        Character targetcharacter = target.GetComponent<UnitScript>().UnitCharacteristics;
        FindAttackers();
        List<equipment> previousequipmentstate = new List<equipment>();
        List<int> previousequipmentIDstate = new List<int>();
        bool previoustelekinesis = false;
        (previousequipmentstate, previousequipmentIDstate, previoustelekinesis) = previouscharacterstate(targetcharacter);
        if (targetlist.Count == 0) //ici pas d'ennemi trouve donc on essaie d'autres armes
        {
            if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() != "staff")
            {
                List<equipment> weapons = target.GetComponent<UnitScript>().GetAllWeapons();
                foreach (equipment weapon in weapons)
                {
                    if (weapon.type.ToLower() == "staff")
                    {
                        continue;
                    }
                    int rangebonus = 0;
                    bool frapperenmelee = true;
                    if (targetcharacter.telekinesisactivated)
                    {
                        // if (weapon.type.ToLower() == "bow")
                        // {
                        //     rangebonus = 2;
                        // }
                        // else
                        // {
                        //     rangebonus = 1;
                        // }
                        rangebonus = 1;
                        if (target.GetComponent<UnitScript>().GetSkill(33))
                        {
                            rangebonus += 1;
                        }
                    }
                    if (weapon.type.ToLower() == "bow")
                    {
                        frapperenmelee = false;
                    }

                    GridScript.ShowAttackAfterMovement(weapon.Range + rangebonus, frapperenmelee, target.GetComponent<UnitScript>().UnitCharacteristics.currentTile, weapon.type.ToLower() == "staff", target.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.monsterStats.size, target.GetComponent<UnitScript>().UnitCharacteristics);
                    GridScript.lockedattacktiles = GridScript.attacktiles;
                    GridScript.lockedhealingtiles = GridScript.healingtiles;
                    GridScript.Recolor();
                    FindAttackers();
                    if (targetlist.Count > 0 && weapon != target.GetComponent<UnitScript>().Fists)
                    {
                        target.GetComponent<UnitScript>().EquipWeapon(weapon);
                        return;
                    }
                }
                if (targetlist.Count == 0) //ici toujours pas d'ennemi trouve donc on essaie d'autres armes en chengeant le reglage de telekinesie
                {
                    targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;
                    foreach (equipment weapon in weapons)
                    {
                        if (weapon.type.ToLower() == "staff")
                        {
                            continue;
                        }
                        int rangebonus = 0;
                        bool frapperenmelee = true;
                        if (targetcharacter.telekinesisactivated)
                        {
                            // if (weapon.type.ToLower() == "bow")
                            // {
                            //     rangebonus = 2;
                            // }
                            // else
                            // {
                            //     rangebonus = 1;
                            // }
                            rangebonus = 1;
                            if (target.GetComponent<UnitScript>().GetSkill(33))
                            {
                                rangebonus += 1;
                            }
                        }
                        if (weapon.type.ToLower() == "bow")
                        {
                            frapperenmelee = false;
                        }
                        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
                        GridScript.ShowAttackAfterMovement(weapon.Range + rangebonus, frapperenmelee, chartarget.currentTile, weapon.type.ToLower() == "staff", chartarget.enemyStats.monsterStats.size, target.GetComponent<UnitScript>().UnitCharacteristics);
                        GridScript.lockedattacktiles = GridScript.attacktiles;
                        GridScript.lockedhealingtiles = GridScript.healingtiles;
                        GridScript.Recolor();
                        FindAttackers();
                        if (targetlist.Count > 0)
                        {
                            if (weapon != target.GetComponent<UnitScript>().Fists)
                            {
                                target.GetComponent<UnitScript>().EquipWeapon(weapon);
                            }
                            return;
                        }
                    }
                    if (targetlist.Count == 0)  //Finalement pas d'ennemi donc on remet le reglage original de telekinesie
                    {
                        ResetCharacterEquipment(targetcharacter, previousequipmentstate, previousequipmentIDstate, previoustelekinesis);

                    }
                }
            }
            else
            {
                targetcharacter.telekinesisactivated = !targetcharacter.telekinesisactivated;
                FindAttackers();
                if (targetlist.Count == 0)  //Finalement pas d'ennemi donc on remet le reglage original de telekinesie
                {
                    ResetCharacterEquipment(targetcharacter, previousequipmentstate, previousequipmentIDstate, previoustelekinesis);

                }
            }

        }
        if (targetlist.Count == 0)  //Finalement pas d'ennemi donc on remet le reglage original de telekinesie
        {
            ResetCharacterEquipment(targetcharacter, previousequipmentstate, previousequipmentIDstate, previoustelekinesis);

        }
        equipment newweapon = target.GetComponent<UnitScript>().GetFirstWeapon();

        int newrangebonus = 0;
        bool newfrapperenmelee = true;
        if (targetcharacter.telekinesisactivated)
        {
            // if (newweapon.type.ToLower() == "bow")
            // {
            //     newrangebonus = 2;
            // }
            // else
            // {
            //     newrangebonus = 1;
            // }
            newrangebonus = 1;
            if (target.GetComponent<UnitScript>().GetSkill(33))
            {
                newrangebonus += 1;
            }
        }
        if (newweapon.type.ToLower() == "bow")
        {
            newfrapperenmelee = false;
        }
        Character newchartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        GridScript.ShowAttackAfterMovement(newweapon.Range + newrangebonus, newfrapperenmelee, newchartarget.currentTile, newweapon.type.ToLower() == "staff", newchartarget.enemyStats.monsterStats.size, newchartarget);
    }


    public void ActivateCommand(int CommandID)
    {
        FindAttackers(true, CommandID);
    }

    public void FinalizeAttack()
    {
        target.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;
        targetlist = new List<GameObject>();
        GameObject oldtarget = target;
        target = null;
        CommandUsedID = 0;
        GridScript.Recolor();
        confirmattack = false;
        ActionManager.instance.currentcharacter = null;
        FindAnyObjectByType<cameraScript>().incombat = false;
        ActionManager.instance.preventfromlockingafteraction = true;
        oldtarget.GetComponent<UnitScript>().RetreatTrigger(); // Canto/Retreat (move again after action)
    }
    public void ConfirmAttack()
    {
        if (targetlist.Count > 0)
        {
            confirmattack = true;
            unitAttackText.transform.parent.parent.gameObject.SetActive(false);
            gameObject.SetActive(false);
            GridScript.ResetAllSelections();
        }
    }



    private void FindAttackers(bool usecommand = false, int commandID = 0)
    {

        targetlist = new List<GameObject>();

        if (GridScript == null)
        {
            GridScript = GridScript.instance;
        }

        if (usecommand)
        {

            Skill command = DataScript.instance.SkillList[commandID]; // targetting : 0 enemies, 1 allies, 2 walls, 3 self
            CommandUsedID = command.ID;
            if (command.targettype == 0)
            {
                Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
                GridScript.ShowAttackAfterMovement(command.range, true, chartarget.currentTile, false, chartarget.enemyStats.monsterStats.size, chartarget);
                GridScript.lockedattacktiles = GridScript.attacktiles;

                foreach (GridSquareScript tile in GridScript.lockedattacktiles)
                {
                    GameObject potentialtarget = GridScript.GetUnit(tile);
                    if (potentialtarget != null && potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation != "playable")
                    {
                        if (commandID != 56 || potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.UnitSkill != 0) //copy
                        {
                            targetlist.Add(potentialtarget);
                        }
                    }
                }
                if (targetlist.Count > 0)
                {
                    activetargetid = 0;
                    initializeSkillWindow(target, targetlist[activetargetid], command);
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                    AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use";
                    AttackButton.Select();
                }

            }
            else if (command.targettype == 1)
            {
                Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
                GridScript.ShowAttackAfterMovement(command.range, true, chartarget.currentTile, true, chartarget.enemyStats.monsterStats.size, chartarget);
                GridScript.lockedhealingtiles = GridScript.healingtiles;
                foreach (GridSquareScript tile in GridScript.lockedhealingtiles)
                {
                    GameObject potentialtarget = GridScript.GetUnit(tile);
                    if (potentialtarget != null && potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" && potentialtarget != target)
                    {
                        targetlist.Add(potentialtarget);
                    }
                }
                if (targetlist.Count > 0)
                {
                    activetargetid = 0;
                    initializeSkillWindow(target, targetlist[activetargetid], command);
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                    AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use";
                    AttackButton.Select();
                }
            }
            else if (command.targettype == 2)
            {
                GridSquareScript positiontile = target.GetComponent<UnitScript>().UnitCharacteristics.currentTile[0];
                for (int i = -command.range; i <= command.range; i++)
                {
                    for (int j = -command.range; j <= command.range; j++)
                    {
                        if (Mathf.Abs(i) + Mathf.Abs(j) <= command.range)
                        {
                            Vector2 posoffset = new Vector2(i, j);
                            GridSquareScript newpositiontile = GridScript.GetTile(positiontile.GridCoordinates + posoffset);
                            if (newpositiontile != null)
                            {
                                if (newpositiontile.isobstacle && checkIfSmallWall(positiontile, newpositiontile))
                                {
                                    targetlist.Add(newpositiontile.gameObject);
                                }
                            }
                        }
                    }
                }
                if (targetlist.Count > 0)
                {
                    activetargetid = 0;
                    initializeSkillWindow(target, targetlist[activetargetid], command);
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                    AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use";
                    AttackButton.Select();
                }
            }
            else if (command.targettype == 3)
            {
                targetlist.Add(target.gameObject);
                activetargetid = 0;
                initializeSkillWindow(target, targetlist[activetargetid], command);
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use";
                AttackButton.Select();
            }

            if (targetlist.Count == 0)
            {
                commandmenu.SetActive(true);
            }
        }
        else
        {
            CommandUsedID = 0;
            if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
            {
                (int range, bool melee) = target.GetComponent<UnitScript>().GetRangeAndMele();
                Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
                GridScript.ShowAttackAfterMovement(range, melee, chartarget.currentTile, true, chartarget.enemyStats.monsterStats.size, chartarget);
                GridScript.lockedhealingtiles = GridScript.healingtiles;
                foreach (GridSquareScript tile in GridScript.lockedhealingtiles)
                {
                    GameObject potentialtarget = GridScript.GetUnit(tile);
                    if (potentialtarget != null && potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.currentHP < potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.AjustedStats.HP && (potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable" || (potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "other" && !potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends)) && potentialtarget != target)
                    {
                        targetlist.Add(potentialtarget);
                    }
                }
                if (targetlist.Count > 0)
                {
                    activetargetid = 0;
                    initializeHealingWindows(target, targetlist[activetargetid]);
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                    AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Heal";
                    AttackButton.Select();
                }
            }
            else
            {
                foreach (GridSquareScript tile in GridScript.lockedattacktiles)
                {
                    GameObject potentialtarget = GridScript.GetUnit(tile);
                    if (potentialtarget != null && (potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "enemy" || (potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "other" && potentialtarget.GetComponent<UnitScript>().UnitCharacteristics.attacksfriends)))
                    {
                        targetlist.Add(potentialtarget);
                    }
                }
                if (targetlist.Count > 0)
                {
                    activetargetid = 0;
                    initializeAttackWindows(target, targetlist[activetargetid]);
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                    AttackButton.GetComponentInChildren<TextMeshProUGUI>().text = "Attack";
                    AttackButton.Select();
                }
            }
        }

    }

    private bool checkIfSmallWall(GridSquareScript initialtile, GridSquareScript targettile)
    {

        Vector2 coorddiff = targettile.GridCoordinates - initialtile.GridCoordinates;


        int normalizedx = 0;
        int normalizedy = 0;

        if (coorddiff.x != 0)
        {
            normalizedx = (int)(coorddiff.x / Mathf.Abs(coorddiff.x));
        }
        if (coorddiff.y != 0)
        {
            normalizedy = (int)(coorddiff.y / Mathf.Abs(coorddiff.y));
        }
        Vector2 offset = new Vector2(normalizedx, normalizedy);

        if (GridScript.CheckIfPositionIsLegal(targettile.GridCoordinates + offset))
        {
            if (!GridScript.GetTile(targettile.GridCoordinates + offset).isobstacle && GridScript.GetUnit(GridScript.GetTile(targettile.GridCoordinates + offset)) == null)
            {
                return true;
            }
        }

        return false;
    }

    private void SelectionSafeGuard()
    {
        if (GameOverScript.instance != null && GameOverScript.instance.gameObject.activeSelf)
        {
            return;
        }
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        List<GameObject> listofChildren = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            listofChildren.Add(transform.GetChild(i).gameObject);
        }
        if (currentSelected != null)
        {
            if ((!currentSelected.activeSelf || !listofChildren.Contains(currentSelected)) && !ItemsScript.activeSelf && !CommandGO.activeSelf && !SpecialCommandGO.activeSelf && transform.GetChild(0).gameObject.activeSelf)
            {
                FindAnyObjectByType<EventSystem>().SetSelectedGameObject(transform.GetChild(0).gameObject);
            }
        }
        else if (!ItemsScript.activeSelf && !CommandGO.activeSelf && !SpecialCommandGO.activeSelf && transform.GetChild(0).gameObject.activeSelf)
        {
            FindAnyObjectByType<EventSystem>().SetSelectedGameObject(transform.GetChild(0).gameObject);
        }


    }

    public void initializeAttackWindows(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);



        (GameObject doubleattacker, bool tripleattack) = CalculatedoubleAttack(unit, target);



        string damageunittxt = "";
        int FinalUnitdmg = CalculateDamage(unit, target);
        int FinalTargetdmg = CalculateDamage(target, unit);

        if (CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38)) //Spite
        {
            if (doubleattacker == target)
            {
                if (tripleattack)
                {
                    FinalTargetdmg *= 3;
                }
                else
                {
                    FinalTargetdmg *= 2;
                }

            }
        }
        else
        {
            FinalTargetdmg = 0;
        }

        if (doubleattacker == unit)
        {
            if (tripleattack)
            {
                FinalUnitdmg *= 3;
            }
            else
            {
                FinalUnitdmg *= 2;
            }

        }


        if (doubleattacker == unit)
        {
            if (tripleattack)
            {
                damageunittxt += CalculateDamage(unit, target) + " x 3";
            }
            else
            {
                damageunittxt += CalculateDamage(unit, target) + " x 2";
            }

        }
        else
        {
            damageunittxt += CalculateDamage(unit, target);
        }

        string UnitText = "<align=left>" + (int)Mathf.Max(charunit.currentHP - FinalTargetdmg, 0f) + "<align=center>\n";

        UnitText += damageunittxt;


        UnitText += "Hit : " + CalculateHit(unit, target) + " %\n";
        UnitText += "Crit : " + CalculateCrit(unit, target) + " %\n";

        SetupCombatHUD(unit, true, true, (int)Mathf.Max(charunit.currentHP - FinalTargetdmg, 0f), false, damageunittxt, CalculateHit(unit, target) + "%", CalculateCrit(unit, target) + "%");

        if (CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38)) //Spite
        {
            string TargetDmgText = "";
            if (doubleattacker == target)
            {
                if (tripleattack)
                {
                    TargetDmgText += CalculateDamage(target, unit) + " x 3";
                }
                else
                {
                    TargetDmgText += CalculateDamage(target, unit) + " x 2";
                }

            }
            else
            {
                TargetDmgText += CalculateDamage(target, unit);
            }
            SetupCombatHUD(target, false, true, (int)Mathf.Max(chartarget.currentHP - FinalUnitdmg, 0f), false, TargetDmgText, CalculateHit(target, unit) + "%", CalculateCrit(target, unit) + "%");
        }
        else
        {
            SetupCombatHUD(target, false, true, (int)Mathf.Max(chartarget.currentHP - FinalUnitdmg, 0f));
        }


        if (doubleattacker == unit)
        {
            if (tripleattack)
            {
                TargetGreenLifebar.fillAmount = (float)(chartarget.currentHP - CalculateDamage(unit, target) * 3) / (float)chartarget.AjustedStats.HP;
                TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;
            }
            else
            {
                TargetGreenLifebar.fillAmount = (float)(chartarget.currentHP - CalculateDamage(unit, target) * 2) / (float)chartarget.AjustedStats.HP;
                TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;
            }

        }
        else
        {
            TargetGreenLifebar.fillAmount = (float)(chartarget.currentHP - CalculateDamage(unit, target)) / (float)chartarget.AjustedStats.HP;
            TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;
        }

        if (CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38)) //Spite
        {
            if (doubleattacker == target)
            {
                if (tripleattack)
                {
                    UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - CalculateDamage(target, unit) * 3) / (float)charunit.AjustedStats.HP;
                    UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
                }
                else
                {
                    UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - CalculateDamage(target, unit) * 2) / (float)charunit.AjustedStats.HP;
                    UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
                }
            }

            else
            {
                UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - CalculateDamage(target, unit)) / (float)charunit.AjustedStats.HP;
                UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
            }
        }
        else
        {
            UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - 0) / (float)charunit.AjustedStats.HP;
            UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
        }
    }

    public void initializeHealingWindows(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        int FinalHealingdmg = CalculateHealing(unit);


        SetupCombatHUD(unit, true, true, (int)Mathf.Max(charunit.currentHP, 0f), false, FinalHealingdmg + "", "100%", "-", true);

        SetupCombatHUD(target, false, true, (int)Mathf.Min(Mathf.Max(chartarget.currentHP + FinalHealingdmg, 0f), chartarget.AjustedStats.HP));

        TargetGreenLifebar.fillAmount = Mathf.Min((float)(chartarget.currentHP + CalculateHealing(unit)) / (float)chartarget.AjustedStats.HP, 1f);
        TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;

        UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - 0) / (float)charunit.AjustedStats.HP;
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
    }

    public void initializeSkillWindow(GameObject unit, GameObject target, Skill command)
    {
        if (command.ID == 47) //Transfuse
        {
            TransferCommandWindow(unit, target);
        }
        else if (command.ID == 48) //Motivate
        {
            BasicCommandWindow(unit, target);
        }
        else if (command.ID == 49) //Swap
        {
            BasicCommandWindow(unit, target);
        }
        else if (command.ID == 50) //Reinvigorate
        {
            ReinvigorateWindow(unit, target);
        }
        else if (command.ID == 51) // Jump
        {
            WallTargettingWindow(unit, target);
        }
        else if (command.ID == 52) // Fortify
        {
            WallTargettingWindow(unit, unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile[0].gameObject, "Fortification");
        }
        else if (command.ID == 53) // Smoke Bomb
        {
            WallTargettingWindow(unit, unit.GetComponent<UnitScript>().UnitCharacteristics.currentTile[0].gameObject, "Fog");
        }
        else if (command.ID == 54) // Chakra
        {
            ChakraCommandWindow(unit);
        }
        else if (command.ID == 56) // Copy
        {
            BasicCommandWindow(unit, target);
        }

        else if (command.ID == 59) // RainDance
        {
            BasicCommandWindow(unit, target);
        }

        else if (command.ID == 60) // SunDance
        {
            BasicCommandWindow(unit, target);
        }
        else if (command.ID == 70) //Blade Conversion
        {
            BladeConversionCommandWindow(unit);
        }
        else if (command.ID == 71) //Blade Sacrifice
        {
            BladeSacrificeCommandWindow(unit);
        }

    }


    private void BasicCommandWindow(GameObject unit, GameObject target)
    {




        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        SetupCombatHUD(unit, true, true, charunit.currentHP);
        SetupCombatHUD(target, false, true, chartarget.currentHP);

        TargetGreenLifebar.fillAmount = Mathf.Max((float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP, 1f);
        TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;

        UnitGreenLifebar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
    }

    private void ChakraCommandWindow(GameObject unit)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        int healthrestored = (int)((charunit.AjustedStats.HP - charunit.currentHP) * 0.25f);

        SetupCombatHUD(unit, true, true, (int)Mathf.Min(Mathf.Max(charunit.currentHP + healthrestored, 0f), charunit.AjustedStats.HP), false, healthrestored + "", "-", "-", true);
        SetupCombatHUD(target, false, true, 0, true);

        TargetGreenLifebar.fillAmount = 1f;
        TargetOrangeLifeBar.fillAmount = 1f;

        UnitGreenLifebar.fillAmount = Mathf.Min((float)(charunit.currentHP + healthrestored) / (float)charunit.AjustedStats.HP, 1f);
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
    }

    private void BladeConversionCommandWindow(GameObject unit)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        int healthrestored = (int)Mathf.Min((charunit.AjustedStats.HP - charunit.currentHP), charunit.AjustedStats.HP * 0.5f);

        SetupCombatHUD(unit, true, true, (int)Mathf.Min(Mathf.Max(charunit.currentHP + healthrestored, 0f), charunit.AjustedStats.HP), false, healthrestored + "", "-", "-", true);
        SetupCombatHUD(target, false, true, 0, true);

        string unitweapontxt = "";

        equipment unitweapon = unit.GetComponent<UnitScript>().GetFirstWeapon();

        switch (unitweapon.type.ToLower())
        {
            case ("sword"):
                unitweapontxt += "<sprite=0>";
                break;
            case ("spear"):
                unitweapontxt += "<sprite=1>";
                break;
            case ("greatsword"):
                unitweapontxt += "<sprite=2>";
                break;
            case ("bow"):
                unitweapontxt += "<sprite=3>";
                break;
            case ("scythe"):
                unitweapontxt += "<sprite=4>";
                break;
            case ("shield"):
                unitweapontxt += "<sprite=6>";
                break;
            case ("staff"):
                unitweapontxt += "<sprite=7>";
                break;
            default:
                unitweapontxt += "<sprite=5>";
                break;
        }



        unitweapontxt += " " + unitweapon.Name + " <color=red>0</color>/" + unitweapon.Maxuses;

        UnitWeapon.text = unitweapontxt;

        TargetGreenLifebar.fillAmount = 1f;
        TargetOrangeLifeBar.fillAmount = 1f;

        UnitGreenLifebar.fillAmount = Mathf.Min((float)(charunit.currentHP + healthrestored) / (float)charunit.AjustedStats.HP, 1f);
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
    }

    private void BladeSacrificeCommandWindow(GameObject unit)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        int healthlost = (int)Mathf.Min(charunit.currentHP - 1, charunit.AjustedStats.HP * 0.5f);


        SetupCombatHUD(unit, true, true, (int)Mathf.Min(Mathf.Max(charunit.currentHP - healthlost, 0f), charunit.AjustedStats.HP), false, healthlost + "");
        SetupCombatHUD(target, false, true, 0, true);



        string unitweapontxt = "";

        equipment unitweapon = unit.GetComponent<UnitScript>().GetFirstWeapon();

        switch (unitweapon.type.ToLower())
        {
            case ("sword"):
                unitweapontxt += "<sprite=0>";
                break;
            case ("spear"):
                unitweapontxt += "<sprite=1>";
                break;
            case ("greatsword"):
                unitweapontxt += "<sprite=2>";
                break;
            case ("bow"):
                unitweapontxt += "<sprite=3>";
                break;
            case ("scythe"):
                unitweapontxt += "<sprite=4>";
                break;
            case ("shield"):
                unitweapontxt += "<sprite=6>";
                break;
            case ("staff"):
                unitweapontxt += "<sprite=7>";
                break;
            default:
                unitweapontxt += "<sprite=5>";
                break;
        }



        unitweapontxt += " " + unitweapon.Name + " <color=green>" + unitweapon.Maxuses + "</color>/" + unitweapon.Maxuses;

        UnitWeapon.text = unitweapontxt;

        TargetGreenLifebar.fillAmount = 1f;
        TargetOrangeLifeBar.fillAmount = 1f;

        UnitGreenLifebar.fillAmount = Mathf.Min((float)(charunit.currentHP - healthlost) / (float)charunit.AjustedStats.HP, 1f);
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
    }

    private void ReinvigorateWindow(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);

        SetupCombatHUD(unit, true, true, charunit.currentHP);
        SetupCombatHUD(target, false, true, chartarget.currentHP);



        string targetweapontxt = "";

        equipment targetweapon = target.GetComponent<UnitScript>().GetFirstWeapon();

        switch (targetweapon.type.ToLower())
        {
            case ("sword"):
                targetweapontxt += "<sprite=0>";
                break;
            case ("spear"):
                targetweapontxt += "<sprite=1>";
                break;
            case ("greatsword"):
                targetweapontxt += "<sprite=2>";
                break;
            case ("bow"):
                targetweapontxt += "<sprite=3>";
                break;
            case ("scythe"):
                targetweapontxt += "<sprite=4>";
                break;
            case ("shield"):
                targetweapontxt += "<sprite=6>";
                break;
            case ("staff"):
                targetweapontxt += "<sprite=7>";
                break;
            default:
                targetweapontxt += "<sprite=5>";
                break;
        }


        if (targetweapon.Currentuses < targetweapon.Maxuses)
        {
            targetweapontxt += " " + targetweapon.Name + " <color=green>" + (targetweapon.Currentuses + 1) + "</color>/" + targetweapon.Maxuses;
        }
        else
        {
            targetweapontxt += " " + targetweapon.Name + " " + targetweapon.Currentuses + "/" + targetweapon.Maxuses;
        }


        TargetWeapon.text = targetweapontxt;

        TargetGreenLifebar.fillAmount = Mathf.Max((float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP, 1f);
        TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;

        UnitGreenLifebar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
    }



    private void WallTargettingWindow(GameObject unit, GameObject target, string change = "")
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        GridSquareScript Walltarget = target.GetComponent<GridSquareScript>();
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);


        SetupCombatHUD(unit, true, true, charunit.currentHP);
        SetupCombatHUD(target, false, true, 0, false);



        string TargetText = "\n";

        if (Walltarget.type != "")
        {
            TargetNameTMP.text = Walltarget.type;
            TargetText += Walltarget.type + "\n";
        }
        else if (Walltarget.isstairs)
        {
            TargetNameTMP.text = "Stairs";
            TargetText += "Stairs\n";
        }
        else if (Walltarget.isobstacle)
        {
            TargetNameTMP.text = "Wall";
            TargetText += "Wall\n";
        }
        else
        {
            TargetNameTMP.text = "Ground";
            TargetText += "Ground\n";
        }

        if (TargetTelekinesis.activeSelf)
        {
            TargetTelekinesis.SetActive(false);
        }

        TargetWeapon.text = " ";

        TargetTileTMP.text = "";




        if (change != "")
        {
            TargetText += "->" + change + "\n";
        }

        targetAttackText.text = TargetText;

        TargetGreenLifebar.fillAmount = 1f;
        TargetOrangeLifeBar.fillAmount = 1f;

        UnitGreenLifebar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
        UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
    }

    private void TransferCommandWindow(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);



        float unithealthpercent = (float)((float)charunit.currentHP / (float)charunit.AjustedStats.HP);
        float targethealthpercent = (float)((float)chartarget.currentHP / (float)chartarget.AjustedStats.HP);

        if (unithealthpercent > targethealthpercent)
        {

            int unithealthlost = charunit.currentHP - (int)(targethealthpercent * charunit.AjustedStats.HP);

            int targethealthgained = (int)(unithealthpercent * chartarget.AjustedStats.HP) - chartarget.currentHP;

            SetupCombatHUD(unit, true, true, (int)(unithealthpercent * chartarget.AjustedStats.HP));
            SetupCombatHUD(target, false, true, (int)(unithealthpercent * chartarget.AjustedStats.HP));

            TargetGreenLifebar.fillAmount = Mathf.Min((float)(chartarget.currentHP + targethealthgained) / (float)chartarget.AjustedStats.HP, 1f);
            TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;

            UnitGreenLifebar.fillAmount = (float)(charunit.currentHP - unithealthlost) / (float)charunit.AjustedStats.HP;
            UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;
        }
        else
        {
            int unithealthgained = (int)(targethealthpercent * charunit.AjustedStats.HP) - charunit.currentHP;

            int targethealthlost = chartarget.currentHP - (int)(unithealthpercent * chartarget.AjustedStats.HP);

            SetupCombatHUD(unit, true, true, (int)(unithealthpercent * chartarget.AjustedStats.HP));
            SetupCombatHUD(target, false, true, (int)(unithealthpercent * chartarget.AjustedStats.HP));


            TargetGreenLifebar.fillAmount = Mathf.Min((float)(chartarget.currentHP - targethealthlost) / (float)chartarget.AjustedStats.HP, 1f);
            TargetOrangeLifeBar.fillAmount = (float)(chartarget.currentHP) / (float)chartarget.AjustedStats.HP;

            UnitGreenLifebar.fillAmount = (float)(charunit.currentHP + unithealthgained) / (float)charunit.AjustedStats.HP;
            UnitOrangeLifeBar.fillAmount = (float)(charunit.currentHP) / (float)charunit.AjustedStats.HP;

        }


    }

    private void SetupCombatHUD(GameObject GOToUse, bool isUnit, bool isnormalUnit, int hptoshow, bool isnull = false, string dmgorhealing = "-", string hit = "-", string crit = "-", bool ishealing = false)
    {
        unitAttackText.transform.parent.parent.gameObject.SetActive(true);


        if (isnull)
        {
            GameObject TelekinesisGOToUse = null;
            TextMeshProUGUI weapontext = null;
            TextMeshProUGUI nametext = null;
            TextMeshProUGUI tiletext = null;
            TextMeshProUGUI MainText = null;
            if (isUnit)
            {
                unitSprite.sprite = EmptySprite;
                TelekinesisGOToUse = UnitTelekinesis;
                weapontext = UnitWeapon;
                nametext = UnitNameTMP;
                tiletext = UnitTileTMP;
                MainText = unitAttackText;
            }
            else
            {
                targetSprite.sprite = EmptySprite;
                TelekinesisGOToUse = TargetTelekinesis;
                weapontext = TargetWeapon;
                nametext = TargetNameTMP;
                tiletext = TargetTileTMP;
                MainText = targetAttackText;
            }
            weapontext.text = "";
            nametext.text = "";
            tiletext.text = "";
            MainText.text = "";


            return;
        }

        Character character = GOToUse.GetComponent<UnitScript>().UnitCharacteristics;




        if (isnormalUnit)
        {

            Sprite spriteToUse = null;
            if (character.affiliation.ToLower() == "playable")
            {
                spriteToUse = DataScript.instance.DialogueSpriteList[character.ID];
            }
            else
            {
                spriteToUse = DataScript.instance.EnemySprites[character.enemyStats.SpriteID];
            }

            GameObject TelekinesisGOToUse = null;
            TextMeshProUGUI weapontext = null;
            TextMeshProUGUI nametext = null;
            TextMeshProUGUI tiletext = null;
            TextMeshProUGUI MainText = null;

            if (isUnit)
            {
                unitSprite.sprite = spriteToUse;
                TelekinesisGOToUse = UnitTelekinesis;
                weapontext = UnitWeapon;
                nametext = UnitNameTMP;
                tiletext = UnitTileTMP;
                MainText = unitAttackText;
            }
            else
            {
                targetSprite.sprite = spriteToUse;
                TelekinesisGOToUse = TargetTelekinesis;
                weapontext = TargetWeapon;
                nametext = TargetNameTMP;
                tiletext = TargetTileTMP;
                MainText = targetAttackText;
            }


            string weapontxt = "";

            equipment weapon = GOToUse.GetComponent<UnitScript>().GetFirstWeapon();

            switch (weapon.type.ToLower())
            {
                case ("sword"):
                    weapontxt += "<sprite=0>";
                    break;
                case ("spear"):
                    weapontxt += "<sprite=1>";
                    break;
                case ("greatsword"):
                    weapontxt += "<sprite=2>";
                    break;
                case ("bow"):
                    weapontxt += "<sprite=3>";
                    break;
                case ("scythe"):
                    weapontxt += "<sprite=4>";
                    break;
                case ("shield"):
                    weapontxt += "<sprite=6>";
                    break;
                case ("staff"):
                    weapontxt += "<sprite=7>";
                    break;
                default:
                    weapontxt += "<sprite=5>";
                    break;
            }

            if (character.telekinesisactivated && !TelekinesisGOToUse.activeSelf)
            {
                TelekinesisGOToUse.SetActive(true);
            }

            if (!character.telekinesisactivated && TelekinesisGOToUse.activeSelf)
            {
                TelekinesisGOToUse.SetActive(false);
            }

            weapontext.text = weapontxt + " " + weapon.Name + " " + weapon.Currentuses + "/" + weapon.Maxuses; ;
            nametext.text = character.name;
            if (character.currentTile[0].type != "")
            {
                tiletext.text = character.currentTile[0].type + " ";
            }
            else
            {
                tiletext.text = "Ground ";
            }
            tiletext.text += character.currentTile[0].elevation;

            string MainTextstring = "<align=left>" + hptoshow + "<align=center>\n";
            if (ishealing)
            {
                MainTextstring += "Healing : " + dmgorhealing + "\n";
            }
            else
            {
                MainTextstring += "Dmg : " + dmgorhealing + "\n";
            }
            MainTextstring += "Hit : " + hit + "\n";
            MainTextstring += "Crit : " + crit + "\n";

            MainText.text = MainTextstring;


        }
        else
        {
            if (isUnit)
            {
                unitSprite.sprite = EmptySprite;
            }
            else
            {
                targetSprite.sprite = EmptySprite;
            }

        }
    }

    public bool CheckifInRange(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;


        if (chartarget.enemyStats.monsterStats.size > 0)
        {
            int DistanceSE = (int)(Mathf.Abs(chartarget.position.x - charunit.position.x) + Mathf.Abs(chartarget.position.y - charunit.position.y));
            int DistanceSW = (int)(Mathf.Abs(chartarget.position.x - 1 - charunit.position.x) + Mathf.Abs(chartarget.position.y - charunit.position.y));
            int DistanceNE = (int)(Mathf.Abs(chartarget.position.x - charunit.position.x) + Mathf.Abs(chartarget.position.y + 1 - charunit.position.y));
            int DistanceNW = (int)(Mathf.Abs(chartarget.position.x - 1 - charunit.position.x) + Mathf.Abs(chartarget.position.y + 1 - charunit.position.y));




            (int range, bool melee) = target.GetComponent<UnitScript>().GetRangeAndMele();
            if (DistanceSE <= 1 && DistanceSW <= 1 && DistanceNE <= 1 && DistanceNW <= 1)
            {
                if (!melee)
                {
                    return false;
                }
            }
            else if (DistanceSE > range && DistanceSW > range && DistanceNE > range && DistanceNW > range)
            {
                return false;
            }
            return true;
        }
        else
        {
            int Distance = (int)(Mathf.Abs(chartarget.position.x - charunit.position.x) + Mathf.Abs(chartarget.position.y - charunit.position.y));
            (int range, bool melee) = target.GetComponent<UnitScript>().GetRangeAndMele();
            if (Distance <= 1)
            {
                if (!melee)
                {
                    return false;
                }
            }
            else if (Distance > range)
            {
                return false;
            }
            return true;
        }


    }

    public (int, int, int, int, List<int>) ApplyDamage(GameObject unit, GameObject target, bool unitalreadyattacked)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        List<int> levelup = null;
        int exp = 1;
        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() != "staff")
        {
            (GameObject doubleattacker, bool tripleattack) = CalculatedoubleAttack(unit, target);

            bool inrange = CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38);//spite

            int unithitrate = CalculateHit(unit, target);
            int targethitrate = CalculateHit(target, unit);

            int unitdamage = CalculateDamage(unit, target);
            int targetdamage = CalculateDamage(target, unit);

            int unitcrit = CalculateCrit(unit, target);
            int targetcrit = CalculateCrit(target, unit);

            int numberofhits = 0;
            int numberofcritials = 0;

            int finaldamage = 0;

            if (!unitalreadyattacked)
            {
                int totaldamage = 0;
                if (doubleattacker == unit)
                {
                    if (tripleattack)
                    {
                        //calculating hit for first attack
                        if (unit.GetComponent<RandomScript>().GetHitValue() < unithitrate)
                        {

                            numberofhits++;
                            // calculating critical
                            if (unit.GetComponent<RandomScript>().GetCritValue() < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe" || charunit.enemyStats.monsterStats.size > 0)
                            {
                                DealScytheDamage(unit, target);
                            }
                        }

                        //calculating hit for second attack
                        if (unit.GetComponent<RandomScript>().GetHitValue() < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            if (unit.GetComponent<RandomScript>().GetCritValue() < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }
                        //calculating hit for third attack
                        if (unit.GetComponent<RandomScript>().GetHitValue() < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            if (unit.GetComponent<RandomScript>().GetCritValue() < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }
                    }
                    else
                    {
                        //calculating hit for first attack
                        if (unit.GetComponent<RandomScript>().GetHitValue() < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            if (unit.GetComponent<RandomScript>().GetCritValue() < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }

                        //calculating hit for second attack
                        if (unit.GetComponent<RandomScript>().GetHitValue() < unithitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            if (unit.GetComponent<RandomScript>().GetCritValue() < unitcrit)
                            {
                                numberofcritials++;
                                totaldamage += unitdamage * 3;
                            }
                            else
                            {
                                totaldamage += unitdamage;
                            }
                            if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(unit, target);
                            }
                        }
                    }

                }
                else
                {
                    //calculating hit for first attack
                    if (unit.GetComponent<RandomScript>().GetHitValue() < unithitrate)
                    {
                        numberofhits++;
                        // calculating critical
                        if (unit.GetComponent<RandomScript>().GetCritValue() < unitcrit)
                        {
                            numberofcritials++;
                            totaldamage += unitdamage * 3;
                        }
                        else
                        {
                            totaldamage += unitdamage;
                        }
                        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                        {
                            DealScytheDamage(unit, target);
                        }
                    }
                }

                AffectDamage(unit, target, totaldamage);

                OnDamageEffect(unit, totaldamage, false);
                finaldamage = unitdamage;
                if (chartarget.currentHP <= 0 || !(CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38))) //Spite
                {
                    if (charunit.currentHP > 0 && charunit.affiliation == "playable")
                    {
                        (exp, levelup) = AwardExp(unit, target);
                    }
                    else if (chartarget.currentHP > 0 && chartarget.affiliation == "playable")
                    {
                        (exp, levelup) = AwardExp(unit, target, false, true);
                    }
                }
            }
            else
            {
                //enemy attack

                int totaldamage = 0;

                if (chartarget.currentHP > 0)
                {
                    if (doubleattacker == target)
                    {
                        if (tripleattack)
                        {
                            //calculating hit for first attack
                            if (target.GetComponent<RandomScript>().GetHitValue() < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                if (target.GetComponent<RandomScript>().GetCritValue() < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                            //calculating hit for second attack
                            if (target.GetComponent<RandomScript>().GetHitValue() < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                if (target.GetComponent<RandomScript>().GetCritValue() < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                            //calculating hit for third attack
                            if (target.GetComponent<RandomScript>().GetHitValue() < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                if (target.GetComponent<RandomScript>().GetCritValue() < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                        }
                        else
                        {
                            //calculating hit for first attack
                            if (target.GetComponent<RandomScript>().GetHitValue() < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                if (target.GetComponent<RandomScript>().GetCritValue() < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                            //calculating hit for second attack
                            if (target.GetComponent<RandomScript>().GetHitValue() < targethitrate)
                            {
                                numberofhits++;
                                // calculating critical
                                if (target.GetComponent<RandomScript>().GetCritValue() < targetcrit)
                                {
                                    numberofcritials++;
                                    totaldamage += targetdamage * 3;
                                }
                                else
                                {
                                    totaldamage += targetdamage;
                                }
                                if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                                {
                                    DealScytheDamage(target, unit);
                                }
                            }
                        }

                    }
                    else
                    {
                        //calculating hit for first attack
                        if (target.GetComponent<RandomScript>().GetHitValue() < targethitrate)
                        {
                            numberofhits++;
                            // calculating critical
                            if (target.GetComponent<RandomScript>().GetCritValue() < targetcrit)
                            {
                                numberofcritials++;
                                totaldamage += targetdamage * 3;
                            }
                            else
                            {
                                totaldamage += targetdamage;
                            }
                            if (target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "scythe")
                            {
                                DealScytheDamage(target, unit);
                            }
                        }
                    }
                    AffectDamage(target, unit, totaldamage);
                    OnDamageEffect(target, targetdamage, false);
                    finaldamage = targetdamage;
                }
                if (charunit.currentHP > 0 && charunit.affiliation == "playable")
                {
                    (exp, levelup) = AwardExp(unit, target);
                }
                if (chartarget.currentHP > 0 && chartarget.affiliation == "playable")
                {
                    (exp, levelup) = AwardExp(target, unit);
                }
            }

            if (chartarget.currentHP <= 0)
            {
                unit.GetComponent<UnitScript>().unitkilled++;
            }
            if (charunit.currentHP <= 0)
            {
                target.GetComponent<UnitScript>().unitkilled++;
            }

            return (numberofhits, numberofcritials, finaldamage, exp, levelup);
        }
        //using a staff
        else
        {
            bool inrange = CheckifInRange(unit, target) || target.GetComponent<UnitScript>().GetSkill(38); //Spite
            int unitdamage = (int)Mathf.Min(CalculateHealing(unit), chartarget.AjustedStats.HP - chartarget.currentHP);
            int numberofhits = 1;
            int numberofcritials = 0;

            int finaldamage = 0;

            if (!unitalreadyattacked)
            {
                if (unitdamage + chartarget.currentHP > chartarget.AjustedStats.HP)
                {
                    unitdamage = (int)chartarget.AjustedStats.HP - chartarget.currentHP;
                }
                chartarget.currentHP += unitdamage;
                OnDamageEffect(unit, unitdamage, true);
                finaldamage = unitdamage;
            }
            else
            {
                //When healing the other does nothing
                numberofcritials = 0;
                numberofhits = 0;
                finaldamage = 0;
            }
            if (charunit.currentHP > 0 && chartarget.affiliation == "playable" && charunit.affiliation == "playable")
            {
                (exp, levelup) = AwardExp(unit, target, true);
            }
            return (numberofhits, numberofcritials, finaldamage, exp, levelup);
        }

    }

    private void AffectDamage(GameObject Attacker, GameObject target, int damage)
    {

        Character charTarget = target.GetComponent<UnitScript>().UnitCharacteristics;



        List<GameObject> activelist = null;
        if (charTarget.affiliation == "playable")
        {
            activelist = FindAnyObjectByType<TurnManger>().playableunitGO;
        }
        else if (charTarget.affiliation == "enemy")
        {
            activelist = FindAnyObjectByType<TurnManger>().enemyunitGO;
        }
        else
        {
            activelist = FindAnyObjectByType<TurnManger>().otherunitsGO;
        }

        //One for All
        Character allforonetransfertarget = null;
        GameObject allforonetransfertargetGO = null;
        foreach (GameObject othertarget in activelist)
        {
            Character charOthertarget = othertarget.GetComponent<UnitScript>().UnitCharacteristics;

            if (othertarget.GetComponent<UnitScript>().GetSkill(40) && ManhattanDistance(charTarget, charOthertarget) <= 3)
            {
                allforonetransfertarget = charOthertarget;
                allforonetransfertargetGO = othertarget;
                break;
            }
        }

        int basehp = charTarget.currentHP;

        if (allforonetransfertarget != null)
        {
            int transfertargethp = allforonetransfertarget.currentHP;
            target.GetComponent<UnitScript>().UnitCharacteristics.currentHP -= damage / 2;
            allforonetransfertarget.currentHP -= damage / 2;

            if (allforonetransfertarget.currentHP <= 0 && transfertargethp >= allforonetransfertarget.AjustedStats.HP && allforonetransfertargetGO.GetComponent<UnitScript>().GetSkill(44))  //unyielding
            {
                allforonetransfertarget.currentHP = 1;
                allforonetransfertargetGO.GetComponent<UnitScript>().AddNumber(0, true, "Unyielding");
            }

        }
        else
        {
            target.GetComponent<UnitScript>().UnitCharacteristics.currentHP -= damage;
        }

        if (charTarget.currentHP <= 0 && basehp >= charTarget.AjustedStats.HP && target.GetComponent<UnitScript>().GetSkill(44))  //unyielding
        {
            charTarget.currentHP = 1;
            target.GetComponent<UnitScript>().AddNumber(0, true, "Unyielding");
        }


    }
    private void OnDamageEffect(GameObject Attacker, int DamageDealt, bool healing)
    {


        if (healing)
        {
            if (Attacker.GetComponent<UnitScript>().GetSkill(30)) // Compassion
            {
                Attacker.GetComponent<UnitScript>().AddNumber(Mathf.Min(DamageDealt, (int)Attacker.GetComponent<UnitScript>().UnitCharacteristics.AjustedStats.HP - Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP), true, "Compassion");
                Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP += DamageDealt;
                if (Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP > Attacker.GetComponent<UnitScript>().UnitCharacteristics.AjustedStats.HP)
                {
                    Attacker.GetComponent<UnitScript>().UnitCharacteristics.currentHP = (int)Attacker.GetComponent<UnitScript>().UnitCharacteristics.AjustedStats.HP;
                }
            }
            if (Attacker.GetComponent<UnitScript>().GetSkill(34)) // Rebound
            {
                List<GameObject> list = new List<GameObject>();
                if (Attacker.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
                {
                    list = FindAnyObjectByType<TurnManger>().playableunitGO;
                }
                else if (Attacker.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "enemy")
                {
                    list = FindAnyObjectByType<TurnManger>().enemyunitGO;
                }
                else
                {
                    list = FindAnyObjectByType<TurnManger>().otherunitsGO;
                }

                foreach (GameObject unit in list)
                {
                    Character unitchar = unit.GetComponent<UnitScript>().UnitCharacteristics;

                    unit.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)unitchar.AjustedStats.HP - unitchar.currentHP, (int)(DamageDealt * 0.1f)), true, "Rebound");
                    unitchar.currentHP += (int)(DamageDealt * 0.1f);
                    if (unitchar.currentHP > unitchar.AjustedStats.HP)
                    {
                        unitchar.currentHP = (int)unitchar.AjustedStats.HP;
                    }
                }
            }
        }
        else
        {
            if (Attacker.GetComponent<UnitScript>().GetSkill(14)) // Invigorating
            {
                Character AttackerChar = Attacker.GetComponent<UnitScript>().UnitCharacteristics;
                Attacker.GetComponent<UnitScript>().AddNumber(Mathf.Min((int)AttackerChar.AjustedStats.HP - AttackerChar.currentHP, (DamageDealt / 10)), true, "Invigorating");
                AttackerChar.currentHP += (DamageDealt / 10);
                if (AttackerChar.currentHP > AttackerChar.AjustedStats.HP)
                {
                    AttackerChar.currentHP = (int)AttackerChar.AjustedStats.HP;
                }
            }

        }

        //Durability
        if (Attacker.GetComponent<UnitScript>().GetSkill(8)) //inexhaustible
        {
            return;
        }
        equipment weapon = Attacker.GetComponent<UnitScript>().GetFirstWeapon();
        if (weapon.Maxuses > 0)
        {
            weapon.Currentuses--;
            if (Attacker.GetComponent<UnitScript>().GetSkill(68))//Violent Misuse
            {
                weapon.Currentuses--;
            }
        }
        if (weapon.Currentuses <= 0)
        {
            Attacker.GetComponent<UnitScript>().UpdateWeaponModel();
        }

    }

    private void DealScytheDamage(GameObject attacker, GameObject target)
    {
        List<GameObject> targetlist = new List<GameObject>();
        Vector2 position = target.GetComponent<UnitScript>().UnitCharacteristics.position;

        if (position.x < GridScript.Grid.Count - 1)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)(position.x + 1), (int)position.y));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }


        if (position.x > 0)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)(position.x - 1), (int)position.y));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }

        if (position.y < GridScript.Grid[0].Count - 1)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)position.x, (int)(position.y + 1)));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }


        if (position.y > 0)
        {
            GameObject newunit = GridScript.GetUnit(GridScript.GetTile((int)position.x, (int)(position.y - 1)));
            if (newunit != null)
            {
                targetlist.Add(newunit);
            }
        }


        foreach (GameObject potentialtarget in targetlist)
        {

            Character Chartarget = potentialtarget.GetComponent<UnitScript>().UnitCharacteristics;

            int baseHP = Chartarget.currentHP;

            if (Chartarget.affiliation == target.GetComponent<UnitScript>().UnitCharacteristics.affiliation)
            {
                int damage = CalculateDamage(attacker, potentialtarget);

                //Cleaver
                int truedamage = damage / 4;
                if (attacker.GetComponent<UnitScript>().GetFirstWeapon().Modifier.ToLower() == "serrated")
                {
                    truedamage += damage / 4;
                }
                potentialtarget.GetComponent<UnitScript>().AddNumber(truedamage, false, "Scythe");
                if (attacker.GetComponent<UnitScript>().GetSkill(27))
                {
                    truedamage += damage / 4;
                    potentialtarget.GetComponent<UnitScript>().AddNumber(damage / 4, false, "Cleaver");
                }
                Chartarget.currentHP -= truedamage;
            }

            if (Chartarget.currentHP <= 0 && baseHP >= Chartarget.AjustedStats.HP && potentialtarget.GetComponent<UnitScript>().GetSkill(44))  //unyielding
            {
                Chartarget.currentHP = 1;
                potentialtarget.GetComponent<UnitScript>().AddNumber(0, true, "Unyielding");
            }

        }
    }
    public (int, List<int>) AwardExp(GameObject unit, GameObject target, bool usingstaff = false, bool noattack = false)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
        int baseexp = 15;
        if (chartarget.isboss)
        {
            baseexp = 50;
        }

        int adjustedexp = (int)(baseexp * (1f + (chartarget.level - charunit.level) / 5f));

        if (chartarget.currentHP <= 0)
        {
            adjustedexp *= 3;
        }

        if (usingstaff)
        {
            adjustedexp = 15;
        }
        if (noattack)
        {
            adjustedexp = 1;
        }

        if (unit.GetComponent<UnitScript>().GetSkill(57) || unit.GetComponent<UnitScript>().GetSkill(72) || unit.GetComponent<UnitScript>().GetSkill(73)) // Crystal Heart, Guardian Spirit, Hero's Heir
        {
            adjustedexp = (int)(adjustedexp * 1.1f);
        }

        if (adjustedexp < 0)
        {
            adjustedexp = 1;
        }
        if (adjustedexp > 100)
        {
            adjustedexp = 100;
        }
        charunit.experience += adjustedexp;
        List<int> levelup = new List<int>();
        if (charunit.experience > 100)
        {
            levelup = unit.GetComponent<UnitScript>().LevelUp();
        }

        unit.GetComponent<UnitScript>().GainCombatMastery();

        return (adjustedexp, levelup);
    }

    public int CalculateDamage(GameObject unit, GameObject target = null)
    {

        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = null;
        GridSquareScript targetTile = null;




        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = null;

        float baseweapondamage = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseDamage;
        float basestatdamage = charunit.AjustedStats.Strength + UnitSkillBonus.Strength;
        float basestatdef = 0;


        if (target != null && target.activeSelf)
        {

            chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
            targetTile = chartarget.currentTile[0];

            TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

            if (target.GetComponent<UnitScript>().GetSkill(69)) // Redirection
            {
                basestatdef = chartarget.AjustedStats.Resistance + TargetSkillBonus.Resistance;
            }
            else
            {
                basestatdef = chartarget.AjustedStats.Defense + TargetSkillBonus.Defense;
            }

        }


        if (charunit.telekinesisactivated)
        {
            baseweapondamage = baseweapondamage * 0.75f;
            basestatdamage = charunit.AjustedStats.Psyche + UnitSkillBonus.Psyche;
            if (target != null)
            {
                if (target.GetComponent<UnitScript>().GetSkill(69)) // Redirection
                {
                    basestatdef = chartarget.AjustedStats.Defense + TargetSkillBonus.Defense;
                }
                else
                {
                    basestatdef = chartarget.AjustedStats.Resistance + TargetSkillBonus.Resistance;
                }

            }

        }



        if (unit.GetComponent<UnitScript>().GetFirstWeapon().Name.ToLower() == "reshine")
        {
            basestatdamage = charunit.AjustedStats.Psyche + UnitSkillBonus.Psyche;
            if (target != null)
            {
                basestatdef = Mathf.Min(chartarget.AjustedStats.Defense + TargetSkillBonus.Defense, charunit.AjustedStats.Resistance + TargetSkillBonus.Resistance);
            }
        }

        float unitbasedamage = baseweapondamage + basestatdamage;

        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "shield" && unit.GetComponent<UnitScript>().GetFirstWeapon().Modifier != null && unit.GetComponent<UnitScript>().GetFirstWeapon().Modifier.ToLower() == "spiked")
        {
            unitbasedamage += (charunit.AjustedStats.Defense + UnitSkillBonus.Defense) / 2f;
        }

        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "greatsword")
        {
            basestatdef = basestatdef - unit.GetComponent<UnitScript>().GetFirstWeapon().Grade;
            if (unit.GetComponent<UnitScript>().GetFirstWeapon().Modifier.ToLower() == "heavyweight")
            {
                basestatdef -= unit.GetComponent<UnitScript>().GetFirstWeapon().Grade;
            }
        }
        if (target != null && target.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "shield")
        {
            basestatdef += target.GetComponent<UnitScript>().GetFirstWeapon().Grade * 2;
        }

        float finaldamagefloat = unitbasedamage - basestatdef;
        if (charunit.telekinesisactivated)
        {
            finaldamagefloat = finaldamagefloat * (1f + (float)UnitSkillBonus.TelekDamage / 100f);
        }
        else
        {
            finaldamagefloat = finaldamagefloat * (1f + (float)UnitSkillBonus.PhysDamage / 100f);
        }
        if (target != null && TargetSkillBonus != null)
        {
            finaldamagefloat = finaldamagefloat / (1f + (float)TargetSkillBonus.DamageReduction / 100f);
        }

        if (unit.GetComponent<UnitScript>().GetFirstWeapon().type.ToLower() == "staff")
        {
            finaldamagefloat = finaldamagefloat / 2f;
        }

        if (target != null && targetTile != null && targetTile.type == "Fire")
        {
            finaldamagefloat = finaldamagefloat * 1.1f;
        }

        finaldamagefloat = finaldamagefloat * CalculateRainDamageBonus(unit);

        if (finaldamagefloat < 0)
        {
            finaldamagefloat = 0;
        }

        int finaldamage = (int)finaldamagefloat + UnitSkillBonus.FixedDamageBonus;

        if (target != null && TargetSkillBonus != null)
        {
            finaldamage = finaldamage - TargetSkillBonus.FixedDamageReduction;
        }


        if (finaldamage < 0)
        {
            finaldamage = 0;
        }

        return finaldamage;

    }

    public int CalculateHealing(GameObject unit)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;

        int baseweapondamage = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseDamage;
        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        int basestatdamage = (int)charunit.AjustedStats.Psyche + UnitSkillBonus.Psyche;


        if (charunit.telekinesisactivated)
        {
            baseweapondamage = (int)(baseweapondamage * 0.25f);
        }

        int unitbasedamage = baseweapondamage + (int)(basestatdamage / 4f);
        return unitbasedamage;

    }

    private float CalculateRainDamageBonus(GameObject unit, GameObject target = null)
    {
        float damagebonus = 1f;
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;




        if (charunit.enemyStats != null)
        {
            if (charunit.enemyStats.monsterStats != null)
            {
                //If the attacker is pluvial and the weather is rain, increase damage by 10%
                if (charunit.enemyStats.monsterStats.ispluvial && unit.GetComponent<UnitScript>().GetWeatherType() == "rain")
                {
                    damagebonus += 0.15f;
                }

                //If the attacker is pluvial and the weather is sun, decrease damage by 10%
                if (charunit.enemyStats.monsterStats.ispluvial && unit.GetComponent<UnitScript>().GetWeatherType() == "sun")
                {
                    damagebonus -= 0.15f;
                }
            }
        }
        if (target != null)
        {
            Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;
            if (chartarget.enemyStats != null)
            {
                if (chartarget.enemyStats.monsterStats != null)
                {
                    //If the defender is pluvial and the weather is rain, decrease damage by 10%
                    if (chartarget.enemyStats.monsterStats.ispluvial && target.GetComponent<UnitScript>().GetWeatherType() == "rain")
                    {
                        damagebonus -= 0.15f;
                    }
                    //If the defender is pluvial and the weather is sun, increase damage by 10%
                    if (chartarget.enemyStats.monsterStats.ispluvial && target.GetComponent<UnitScript>().GetWeatherType() == "sun")
                    {
                        damagebonus += 0.15f;
                    }
                    //If the defender is a machine and the weather is rain, increase damage by 20%
                    if (chartarget.enemyStats.monsterStats.ismachine && target.GetComponent<UnitScript>().GetWeatherType() == "rain")
                    {
                        damagebonus += 0.2f;
                    }
                }
            }
        }

        return damagebonus;
    }

    public (GameObject, bool) CalculatedoubleAttack(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

        BaseStats unitweaponstatbonus;
        BaseStats targetweaponstatbonus;

        (unitweaponstatbonus, targetweaponstatbonus) = GetWeaponStatBonus(unit, target);

        int unitbasespeed = (int)charunit.AjustedStats.Speed + UnitSkillBonus.Speed + (int)unitweaponstatbonus.Speed;



        int targetbasespeed = (int)chartarget.AjustedStats.Speed + TargetSkillBonus.Speed + (int)targetweaponstatbonus.Speed;



        int SpeedDiff = unitbasespeed - targetbasespeed;


        if (SpeedDiff >= 15 || unit.GetComponent<UnitScript>().GetSkill(39)) // Thousand Needles
        {
            return (unit, true);
        }
        else if (SpeedDiff >= 5)
        {
            return (unit, false);
        }
        if (SpeedDiff <= -15)
        {
            return (target, true);
        }
        else if (SpeedDiff <= -5)
        {
            return (target, false);
        }

        return (null, false);

    }

    public (BaseStats, BaseStats) GetWeaponStatBonus(GameObject unit, GameObject target)
    {
        BaseStats unitstatbonus = new BaseStats();
        BaseStats targetstatbonus = new BaseStats();

        equipment unitfirstweapon = unit.GetComponent<UnitScript>().GetFirstWeapon();
        equipment targetfirstweapon = target.GetComponent<UnitScript>().GetFirstWeapon();

        if (unitfirstweapon.type.ToLower() == "sword")
        {
            int increaseamount = 2;
            if (unitfirstweapon.Modifier.ToLower() == "quick")
            {
                increaseamount = 3;
            }
            unitstatbonus.Speed += unitfirstweapon.Grade * increaseamount;
            unitstatbonus.Dexterity += unitfirstweapon.Grade * increaseamount;
        }
        if (targetfirstweapon.type.ToLower() == "sword")
        {
            int increaseamount = 2;
            if (targetfirstweapon.Modifier.ToLower() == "quick")
            {
                increaseamount = 3;
            }
            targetstatbonus.Speed += targetfirstweapon.Grade * increaseamount;
            targetstatbonus.Dexterity += targetfirstweapon.Grade * increaseamount;
        }

        if (unitfirstweapon.type.ToLower() == "greatsword")
        {
            if (unitfirstweapon.Modifier.ToLower() == "heavyweight")
            {
                unitstatbonus.Speed -= unitfirstweapon.Grade * 2;
            }
            else if (unitfirstweapon.Modifier.ToLower() != "lightweight")
            {
                unitstatbonus.Speed -= unitfirstweapon.Grade;
            }

        }
        if (targetfirstweapon.type.ToLower() == "greatsword")
        {
            if (targetfirstweapon.Modifier.ToLower() == "heavyweight")
            {
                targetstatbonus.Speed -= targetfirstweapon.Grade * 2;
            }
            else if (targetfirstweapon.Modifier.ToLower() != "lightweight")
            {
                targetstatbonus.Speed -= targetfirstweapon.Grade;
            }
        }

        if (targetfirstweapon.type.ToLower() == "spear")
        {
            if (targetfirstweapon.Modifier.ToLower() == "precise")
            {
                unitstatbonus.Dexterity -= targetfirstweapon.Grade * 3;
            }
            else
            {
                unitstatbonus.Dexterity -= targetfirstweapon.Grade * 2;
            }

        }
        if (unitfirstweapon.type.ToLower() == "spear")
        {
            if (targetfirstweapon.Modifier.ToLower() == "precise")
            {
                targetstatbonus.Dexterity -= unitfirstweapon.Grade * 3;
            }
            else
            {
                targetstatbonus.Dexterity -= unitfirstweapon.Grade * 2;
            }
        }


        return (unitstatbonus, targetstatbonus);
    }
    public int CalculateHit(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        GridSquareScript unitTile = charunit.currentTile[0];
        GridSquareScript targetTile = chartarget.currentTile[0];

        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

        int hitrateweapon = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseHit;

        int tilebonus = GetTileBonus(unitTile, targetTile);

        BaseStats unitweaponstatbonus;
        BaseStats targetweaponstatbonus;

        (unitweaponstatbonus, targetweaponstatbonus) = GetWeaponStatBonus(unit, target);

        int dexunit = (int)charunit.AjustedStats.Dexterity + UnitSkillBonus.Dexterity + (int)unitweaponstatbonus.Dexterity;


        int spdtarget = (int)chartarget.AjustedStats.Speed + TargetSkillBonus.Speed + (int)targetweaponstatbonus.Dexterity;


        int finalhitrate = (int)(hitrateweapon + (dexunit - spdtarget) * 0.2f) + tilebonus + UnitSkillBonus.Hit - TargetSkillBonus.Dodge;

        if (finalhitrate < 0)
        {
            finalhitrate = 0;
        }
        if (finalhitrate > 100)
        {
            finalhitrate = 100;
        }

        return finalhitrate;

    }

    private int GetTileBonus(GridSquareScript unitTile, GridSquareScript targetTile)
    {
        int tilebonus = 0;

        string unittype = unitTile.type;
        string targettype = unitTile.type;

        if (unittype.ToLower() == "forest")
        {
            tilebonus += 20;
        }
        else if (unittype.ToLower() == "ruins")
        {
            tilebonus += 10;
        }
        else if (unittype.ToLower() == "water")
        {
            tilebonus -= 20;
        }
        else if (unittype.ToLower() == "fortification")
        {
            tilebonus += 5;
        }
        else if (unittype.ToLower() == "fog")
        {
            tilebonus += 20;
        }

        if (targettype.ToLower() == "ruins")
        {
            tilebonus -= 10;
        }
        else if (targettype.ToLower() == "fortification")
        {
            tilebonus -= 15;
        }
        else if (targettype.ToLower() == "fog")
        {
            tilebonus -= 20;
        }

        if (!targetTile.isstairs && !unitTile.isstairs)
        {
            if (targetTile.elevation > unitTile.elevation)
            {
                tilebonus -= 40 * (targetTile.elevation - unitTile.elevation);
            }
            else if (targetTile.elevation < unitTile.elevation)
            {
                tilebonus += 40 * (unitTile.elevation - targetTile.elevation);
            }
        }



        return tilebonus;
    }

    public int CalculateCrit(GameObject unit, GameObject target)
    {
        Character charunit = unit.GetComponent<UnitScript>().UnitCharacteristics;
        Character chartarget = target.GetComponent<UnitScript>().UnitCharacteristics;

        AllStatsSkillBonus UnitSkillBonus = unit.GetComponent<UnitScript>().GetStatSkillBonus(target);
        AllStatsSkillBonus TargetSkillBonus = target.GetComponent<UnitScript>().GetStatSkillBonus(unit);

        int critweapon = unit.GetComponent<UnitScript>().GetFirstWeapon().BaseCrit;

        BaseStats unitweaponstatbonus;
        BaseStats targetweaponstatbonus;

        (unitweaponstatbonus, targetweaponstatbonus) = GetWeaponStatBonus(unit, target);

        int dexunit = (int)charunit.AjustedStats.Dexterity + UnitSkillBonus.Dexterity + (int)unitweaponstatbonus.Dexterity;

        int spdtarget = (int)chartarget.AjustedStats.Speed + TargetSkillBonus.Speed + (int)targetweaponstatbonus.Speed;


        int finalcritrate = (int)(critweapon + dexunit / 15f - spdtarget / 20f + UnitSkillBonus.Crit);

        if (finalcritrate < 0)
        {
            finalcritrate = 0;
        }
        if (finalcritrate > 100)
        {
            finalcritrate = 100;
        }

        return finalcritrate;
    }

    private int ManhattanDistance(Character unit, Character otherunit)
    {
        return (int)(Mathf.Abs(unit.position.x - otherunit.position.x) + Mathf.Abs(unit.position.y - otherunit.position.y));
    }


    public void CommandButtonFunction()
    {
        if (target != null)
        {
            List<Skill> Commands = target.GetComponent<UnitScript>().GetCommands();
            if (Commands.Count > 0)
            {
                CommandGO.SetActive(true);
                CommandGO.GetComponent<CommandScript>().target = target;
                CommandGO.GetComponent<CommandScript>().CommandList = Commands;
                CommandGO.GetComponent<CommandScript>().InitializeButtons();
                FindAnyObjectByType<EventSystem>().SetSelectedGameObject(CommandGO.transform.GetChild(0).gameObject);
            }
        }
    }

    public void SpecialButtonFunction()
    {
        if (target != null)
        {
            List<GameObject> SpecialInteractors = target.GetComponent<UnitScript>().GetSpectialInteraction();
            if (SpecialInteractors.Count > 0)
            {
                SpecialCommandGO.SetActive(true);
                SpecialCommandGO.GetComponent<SpecialCommandsScript>().target = target;
                SpecialCommandGO.GetComponent<SpecialCommandsScript>().SpecialInteractos = SpecialInteractors;
                SpecialCommandGO.GetComponent<SpecialCommandsScript>().InitializeButtons();
                FindAnyObjectByType<EventSystem>().SetSelectedGameObject(SpecialCommandGO.transform.GetChild(0).gameObject);
            }
        }
    }

}

