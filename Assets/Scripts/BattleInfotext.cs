
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnitScript;

public class BattleInfotext : MonoBehaviour
{

    private GridScript GridScript;

    private string stringtoshow;

    TextMeshProUGUI TMP;
    private GameObject selectedunit;

    private cameraScript battlecamera;
    private TurnManger turnManger;
    private AttackTurnScript attackTurnScript;
    public TextMeshProUGUI Skilltext;
    public List<Button> SkillButtonList;
    private List<int> SkillButtonIDList = new List<int>();
    public TextMeshProUGUI SkillDescription;
    public TextMeshProUGUI MasteryText;
    public List<Transform> MasteryExpBars;

    private InputManager inputManager;

    public GameObject ItemAction;

    public GameObject PreBattleMenu;
    public GameObject AttackMenu;

    public ActionsMenu ActionsMenu;

    public bool indescription;

    private TextBubbleScript textBubbleScript;

    public GameObject NeutralMenu;

    public GameObject ForeSightMenu;

    private EventSystem eventSystem;

    [Header("CharacterInfo")]

    public TextMeshProUGUI NameTMP;
    public TextMeshProUGUI HPTMP;
    public TextMeshProUGUI StatTMP;
    public TextMeshProUGUI LevelTMP;
    public Transform WeaponObject;
    public Transform WeaponIconsObject;
    public Image CharacterSprite;
    public Image ExpBarFilling;
    public Image BackgroundImage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TMP = GetComponent<TextMeshProUGUI>();
        inputManager = InputManager.instance;
        GridScript = GridScript.instance;
        textBubbleScript = FindAnyObjectByType<TextBubbleScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (textBubbleScript.indialogue || AttackMenu.activeSelf || NeutralMenu.activeSelf || ForeSightMenu.activeSelf)
        {
            if (transform.GetChild(0).gameObject.activeSelf)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
            if (MasteryText.transform.parent.gameObject.activeSelf)
            {
                MasteryText.transform.parent.gameObject.SetActive(false);
            }

        }
        else if ((!PreBattleMenu.activeSelf || PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace))
        {
            if (!transform.GetChild(0).gameObject.activeSelf)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        if ((AttackMenu.activeSelf || ItemAction.activeSelf || textBubbleScript.indialogue || NeutralMenu.activeSelf || ForeSightMenu.activeSelf))
        {
            if (transform.GetChild(0).gameObject.activeSelf)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }

            return;
        }
        else if (!PreBattleMenu.activeSelf)
        {
            if (!transform.parent.GetChild(0).gameObject.activeSelf)
            {
                transform.parent.GetChild(0).gameObject.SetActive(true);
            }

        }
        else
        {
            if (transform.parent.GetChild(0).gameObject.activeSelf)
            {
                transform.parent.GetChild(0).gameObject.SetActive(false);
            }
        }

        if (battlecamera == null)
        {
            battlecamera = FindAnyObjectByType<cameraScript>();
        }

        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }

        if (turnManger == null)
        {
            turnManger = FindAnyObjectByType<TurnManger>();
        }

        if (attackTurnScript == null)
        {
            attackTurnScript = FindAnyObjectByType<AttackTurnScript>();
        }

        if (GridScript.GetSelectedUnitGameObject() != null)
        {
            selectedunit = GridScript.GetSelectedUnitGameObject();
        }


        if ((GridScript.GetSelectedUnitGameObject() == null && GridScript.lockedmovementtiles.Count == 0) || battlecamera.incombat || (PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) || (!PreBattleMenu.activeSelf && GridScript.GetComponent<TurnManger>().currentlyplaying != "playable"))
        {

            if (!(PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) && !(GameOverScript.instance != null && GameOverScript.instance.gameObject.activeSelf))
            {
                eventSystem.SetSelectedGameObject(null);
            }

        }
        else
        {
            Character selectedunitCharacter = null;
            if (turnManger.currentlyplaying == "playable")
            {
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if (turnManger.currentlyplaying == "enemy")
            {
                if (attackTurnScript.CurrentEnemy != null)
                {
                    selectedunit = attackTurnScript.CurrentEnemy;
                    selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
                }
                else
                {
                    return;
                }

            }
            else if (attackTurnScript.CurrentOther != null)
            {
                selectedunit = attackTurnScript.CurrentOther;
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if (turnManger.currentlyplaying == "")
            {
                selectedunit = GridScript.GetUnit(GridScript.selection);
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;

            }
            if (selectedunit != null && selectedunitCharacter != null)
            {


                ManagedSkillVisuals(selectedunitCharacter);
                ManageSkillDescription();
                ManageMasteryVisuals(selectedunitCharacter);

                NameTMP.text = selectedunitCharacter.name;
                LevelTMP.text = "Lvl: " + selectedunitCharacter.level;
                HPTMP.text = "HP: " + selectedunitCharacter.currentHP + "/" + selectedunitCharacter.AjustedStats.HP;

                Sprite spriteToUse = null;
                if (selectedunitCharacter.affiliation.ToLower() == "playable")
                {
                    spriteToUse = DataScript.instance.DialogueSpriteList[selectedunitCharacter.ID];
                }
                else if (selectedunitCharacter.enemyStats.PlayableSpriteID > 0)
                {
                    spriteToUse = DataScript.instance.DialogueSpriteList[selectedunitCharacter.enemyStats.PlayableSpriteID];
                }
                else
                {
                    spriteToUse = DataScript.instance.EnemySprites[selectedunitCharacter.enemyStats.SpriteID];
                }
                CharacterSprite.sprite = spriteToUse;
                if (selectedunitCharacter.affiliation.ToLower() == "playable")
                {
                    if (!ExpBarFilling.transform.parent.gameObject.activeSelf)
                    {
                        ExpBarFilling.transform.parent.gameObject.SetActive(true);
                    }
                    ExpBarFilling.fillAmount = selectedunitCharacter.experience / 100f;
                }
                else
                {
                    if (ExpBarFilling.transform.parent.gameObject.activeSelf)
                    {
                        ExpBarFilling.transform.parent.gameObject.SetActive(false);
                    }
                }

                AllStatsSkillBonus statsmods = selectedunit.GetComponent<UnitScript>().GetStatSkillBonus(null);
                string statstring = "";
                statstring += "Str: " + (selectedunitCharacter.AjustedStats.Strength + statsmods.Strength);
                statstring += "\nPsy: " + (selectedunitCharacter.AjustedStats.Psyche + statsmods.Psyche);
                statstring += "\nDef: " + (selectedunitCharacter.AjustedStats.Defense + statsmods.Defense);
                statstring += "\nRes: " + (selectedunitCharacter.AjustedStats.Resistance + statsmods.Resistance);
                statstring += "\nDex: " + (selectedunitCharacter.AjustedStats.Dexterity + statsmods.Dexterity);
                statstring += "\nSpd: " + (selectedunitCharacter.AjustedStats.Speed + statsmods.Speed);

                int BaseDamage = ActionsMenu.CalculateDamage(selectedunit);

                statstring += "\nDmg: " + BaseDamage + "\nMvt: " + (selectedunitCharacter.movements - 1);

                StatTMP.text = statstring;

                int currentindex = 0;


                foreach (equipment weapon in selectedunitCharacter.equipments)
                {
                    string weaponstring = "";
                    string gradeletter = "E";
                    int grade = weapon.Grade;
                    switch (grade)
                    {
                        case 1:
                            gradeletter = "D";
                            break;
                        case 2:
                            gradeletter = "C";
                            break;
                        case 3:
                            gradeletter = "B";
                            break;
                        case 4:
                            gradeletter = "A";
                            break;
                        case 5:
                            gradeletter = "S";
                            break;
                    }

                    if (weapon.Name != "" && weapon.type != null)
                    {
                        switch (weapon.type.ToLower())
                        {
                            case ("sword"):
                                weaponstring += "<sprite=0>";
                                break;
                            case ("spear"):
                                weaponstring += "<sprite=1>";
                                break;
                            case ("greatsword"):
                                weaponstring += "<sprite=2>";
                                break;
                            case ("bow"):
                                weaponstring += "<sprite=3>";
                                break;
                            case ("scythe"):
                                weaponstring += "<sprite=4>";
                                break;
                            case ("shield"):
                                weaponstring += "<sprite=6>";
                                break;
                            case ("staff"):
                                weaponstring += "<sprite=7>";
                                break;
                            default:
                                weaponstring += "<sprite=5>";
                                break;
                        }
                        WeaponIconsObject.GetChild(currentindex).GetComponent<TextMeshProUGUI>().text = weaponstring;
                        weaponstring = " (" + gradeletter + ") " + weapon.Name;

                        WeaponObject.GetChild(currentindex).GetComponent<TextMeshProUGUI>().text = weaponstring;

                        currentindex++;
                    }
                }

                for (int i = currentindex; i < WeaponObject.childCount; i++)
                {
                    WeaponObject.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
                    WeaponIconsObject.GetChild(i).GetComponent<TextMeshProUGUI>().text = "";
                }

            }
            else
            {
                if (!Skilltext.transform.parent.gameObject.activeSelf)
                {

                    Skilltext.transform.parent.gameObject.SetActive(false);
                    SkillDescription.transform.parent.gameObject.SetActive(false);
                    MasteryText.transform.parent.gameObject.SetActive(false);
                }
            }

        }
    }

    private void ManageSkillDescription()
    {
        if (inputManager.ShowDetailsjustpressed && SkillButtonIDList.Count > 0)
        {
            SkillButtonList[0].Select();
        }
        if (inputManager.canceljustpressed && SkillButtonIDList.Count > 0)
        {
            eventSystem.SetSelectedGameObject(null);
        }
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < SkillButtonList.Count; i++)
        {
            if (SkillButtonList[i].gameObject == currentSelected)
            {
                SkillDescription.text = DataScript.instance.SkillList[SkillButtonIDList[i]].Descriptions;
                if (!SkillDescription.transform.parent.gameObject.activeSelf)
                {
                    SkillDescription.transform.parent.gameObject.SetActive(true);
                }

                GridScript.movementbuffercounter = 5;
                return;
            }
        }
        if (SkillDescription.transform.parent.gameObject.activeSelf)
        {
            SkillDescription.transform.parent.gameObject.SetActive(false);
        }
    }

    private void ManageMasteryVisuals(Character unit)
    {
        if (unit.affiliation == "playable")
        {
            if (MasteryText.transform.parent.gameObject.activeSelf == false)
            {
                MasteryText.transform.parent.gameObject.SetActive(true);
            }

        }
        else
        {
            if (MasteryText.transform.parent.gameObject.activeSelf)
            {
                MasteryText.transform.parent.gameObject.SetActive(false);
            }

            return;
        }

        MasteryText.text = "";

        List<WeaponMastery> masteries = unit.Masteries;
        int barID = 0;
        for (int i = 0; i < masteries.Count; i++)
        {
            if (MasteryExpBars == null || MasteryExpBars.Count <= i || MasteryExpBars[i] == null)
            {
                continue;
            }

            if (MasteryExpBars[i].gameObject.activeSelf == false)
            {
                MasteryExpBars[i].gameObject.SetActive(true);
            }
            DataScript ds = DataScript.instance;
            string masterylevel = "";
            switch (masteries[i].Level)
            {
                case (-1):
                    continue;
                case (0):
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel0;
                    masterylevel = "X";
                    break;
                case (1):
                    masterylevel = "D";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel1;
                    break;
                case (2):
                    masterylevel = "C";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel2;
                    break;
                case (3):
                    masterylevel = "B";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel3;
                    break;
                case (4):
                    masterylevel = "A";
                    MasteryExpBars[barID].GetChild(0).GetComponent<Image>().fillAmount = 1f;
                    break;
            }
            string masteryicontype = "<size=15>";
            switch (masteries[i].weapontype.ToLower())
            {
                case (""):
                    continue;
                case ("sword"):
                    masteryicontype += "<sprite=0>";
                    break;
                case ("spear"):
                    masteryicontype += "<sprite=1>";
                    break;
                case ("greatsword"):
                    masteryicontype += "<sprite=2>";
                    break;
                case ("bow"):
                    masteryicontype += "<sprite=3>";
                    break;
                case ("scythe"):
                    masteryicontype += "<sprite=4>";
                    break;
                case ("shield"):
                    masteryicontype += "<sprite=6>";
                    break;
                case ("staff"):
                    masteryicontype += "<sprite=7>";
                    break;
            }
            masteryicontype += "</size>";
            barID++;
            MasteryText.text += masteryicontype + " : " + masterylevel + "\n";
        }
        for (int i = barID; i < MasteryExpBars.Count; i++)
        {
            if (MasteryExpBars[i].gameObject.activeSelf)
            {
                MasteryExpBars[i].gameObject.SetActive(false);
            }

        }

    }
    private void ManagedSkillVisuals(Character unit)
    {
        SkillButtonIDList = new List<int>();
        if (unit.UnitSkill != 0)
        {
            if (!SkillButtonList[0].gameObject.activeSelf)
            {
                SkillButtonList[0].gameObject.SetActive(true);
            }

            DataScript.Skill unitskill = GetSkill(unit.UnitSkill);

            SkillButtonList[0].GetComponentInChildren<TextMeshProUGUI>().text = unitskill.name;
            SkillButtonIDList.Add(unitskill.ID);
            for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count, 4); i++)
            {
                if (!SkillButtonList[i + 1].gameObject.activeSelf)
                {
                    SkillButtonList[i + 1].gameObject.SetActive(true);
                }

                DataScript.Skill equipedskill = GetSkill(unit.EquipedSkills[i]);

                SkillButtonList[i + 1].GetComponentInChildren<TextMeshProUGUI>().text = equipedskill.name;
                SkillButtonIDList.Add(equipedskill.ID);
            }
            for (int i = Mathf.Min(unit.EquipedSkills.Count, 4); i < 4; i++)
            {
                if (SkillButtonList[i + 1].gameObject.activeSelf)
                {
                    SkillButtonList[i + 1].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count, 4); i++)
            {
                if (!SkillButtonList[i].gameObject.activeSelf)
                {
                    SkillButtonList[i].gameObject.SetActive(true);
                }

                DataScript.Skill equipedskill = GetSkill(unit.EquipedSkills[i]);

                SkillButtonList[i].GetComponentInChildren<TextMeshProUGUI>().text = equipedskill.name;
                SkillButtonIDList.Add(equipedskill.ID);
            }
            for (int i = Mathf.Min(unit.EquipedSkills.Count, 4); i < 4; i++)
            {
                if (SkillButtonList[i].gameObject.activeSelf)
                {
                    SkillButtonList[i].gameObject.SetActive(false);
                }

            }
        }

        if (unit.UnitSkill != 0 || unit.EquipedSkills.Count > 0)
        {
            if (!Skilltext.transform.parent.gameObject.activeSelf)
            {
                Skilltext.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            if (Skilltext.transform.parent.gameObject.activeSelf)
            {
                Skilltext.transform.parent.gameObject.SetActive(false);
            }

        }
    }

    private DataScript.Skill GetSkill(int skillID)
    {
        DataScript.Skill unitskill = null;

        foreach (DataScript.Skill skill in DataScript.instance.SkillList)
        {
            if (skill.ID == skillID)
            {
                unitskill = skill;
                break;
            }
        }
        return unitskill;
    }
}
