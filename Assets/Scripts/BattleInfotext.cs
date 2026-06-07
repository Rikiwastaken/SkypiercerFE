using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnitScript;

public class BattleInfotext : MonoBehaviour
{

    private GridScript GridScript;
    private GameObject selectedunit;

    private cameraScript battlecamera;
    private TurnManger turnManger;
    private AttackTurnScript attackTurnScript;
    public TextMeshProUGUI Skilltext;

    public List<TextMeshProUGUI> MasteryTexts;
    public List<Image> MasteryImages;
    public List<Transform> MasteryExpBars;
    public List<Sprite> WeaponClassImages;

    public GameObject ItemAction;

    public GameObject PreBattleMenu;
    public GameObject AttackMenu;

    public ActionsMenu ActionsMenu;

    public bool indescription;

    private TextBubbleScript textBubbleScript;

    public GameObject NeutralMenu;

    public GameObject ForeSightMenu;

    private EventSystem eventSystem;

    public int framesbeforeactivation;

    public GameObject previousEnemy;
    public GameObject previousOther;

    [Header("CharacterInfo")]

    public TextMeshProUGUI NameTMP;
    public TextMeshProUGUI HPTMP;
    public Image HPLifebar;
    public TextMeshProUGUI ExpAndLevelTMP;
    public TextMeshProUGUI StrAndPsyTMP;
    public TextMeshProUGUI DefAndResTMP;
    public TextMeshProUGUI SpdAndDexTMP;
    public TextMeshProUGUI DmgAndMovTMP;
    public Image EquipedWeaponIco;
    public TextMeshProUGUI equipedweaponText;
    public Image CharacterSprite;
    public Image ExpBarFilling;
    public Image ExamodeBar;
    public GameObject ExamodeGO;
    public Color ExamodeActivatedColor;
    public Color ExamodeDeactivatedColor;

    [Header("StatusAilment")]

    public List<TextMeshProUGUI> AilmentIconList;
    public List<TextMeshProUGUI> AilmentDurationLst;

    [Header("Skill-related")]

    public Color TemporarySkillColor;
    public List<Button> SkillButtonList;
    private List<int> SkillButtonIDList = new List<int>();
    public TextMeshProUGUI SkillDescription;
    private Color BaseSkillColor;

    private float timefordisappearsrpite;
    public float Timefordisappearsrpite = 1f;

    private InputAction _ShowDetailsAction;
    private InputAction _CancelAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _CancelAction = InputSystem.actions.FindAction("Cancel");
        _ShowDetailsAction = InputSystem.actions.FindAction("ShowDetails");
        GridScript = GridScript.instance;
        textBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);
        BaseSkillColor = SkillButtonList[0].image.color;
        attackTurnScript = FindAnyObjectByType<AttackTurnScript>(FindObjectsInactive.Include);
    }


    private void OnDisable()
    {
        SkillDescription.transform.parent.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (textBubbleScript.indialogue || AttackMenu.activeSelf || NeutralMenu.activeSelf || ForeSightMenu.activeSelf || (PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) || (TipsMenuScript.instance != null && TipsMenuScript.instance.gameObject.activeSelf) || (TutorialWindowScript.instance != null && TutorialWindowScript.instance.gameObject.activeSelf) || (attackTurnScript != null && attackTurnScript.AttackCoroutine != null))
        {
            framesbeforeactivation = 5;


        }
        if ((AttackMenu.activeSelf || textBubbleScript.indialogue || NeutralMenu.activeSelf || ForeSightMenu.activeSelf))
        {
            framesbeforeactivation = 5;


        }

        if (GridScript.GetSelectedUnitGameObject() != null)
        {
            selectedunit = GridScript.GetSelectedUnitGameObject();
            timefordisappearsrpite = Time.time + Timefordisappearsrpite;
        }

        if (Time.time > timefordisappearsrpite && ActionManager.instance.currentcharacter == null)
        {
            framesbeforeactivation = 1;
        }


        if (framesbeforeactivation <= 0)
        {
            if (!transform.GetChild(0).gameObject.activeSelf)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        else
        {
            framesbeforeactivation--;
            if (transform.GetChild(0).gameObject.activeSelf)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
            if (MasteryTexts[0].transform.parent.gameObject.activeSelf)
            {
                MasteryTexts[0].transform.parent.gameObject.SetActive(false);
            }
            if (Skilltext.transform.parent.gameObject.activeSelf)
            {
                Skilltext.transform.parent.gameObject.SetActive(false);
            }
            return;
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



        if ((GridScript.GetSelectedUnitGameObject() == null && GridScript.lockedmovementtiles.Count == 0) || battlecamera.incombat || (PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) || (!PreBattleMenu.activeSelf && GridScript.GetComponent<TurnManger>().currentlyplaying != "playable" && GridScript.GetComponent<TurnManger>().currentlyplaying != "tutorial"))
        {

            if (!(PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) && !(GameOverScript.instance != null && GameOverScript.instance.gameObject.activeSelf))
            {
                eventSystem.SetSelectedGameObject(null);
            }

        }
        else
        {
            Character selectedunitCharacter = null;
            if (turnManger.currentlyplaying == "playable" || turnManger.currentlyplaying == "tutorial")
            {
                selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
            }
            else if (turnManger.currentlyplaying == "enemy")
            {
                if (previousEnemy != null)
                {
                    selectedunit = previousEnemy;
                    selectedunitCharacter = selectedunit.GetComponent<UnitScript>().UnitCharacteristics;
                }
                else
                {
                    return;
                }

            }
            else if (previousOther != null)
            {
                selectedunit = previousOther;
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
                if (ActionsMenu.gameObject.activeSelf)
                {
                    if (Skilltext.transform.parent.gameObject.activeSelf)
                    {
                        Skilltext.transform.parent.gameObject.SetActive(false);
                    }
                    if (SkillDescription.transform.gameObject.activeSelf)
                    {
                        SkillDescription.transform.gameObject.SetActive(false);
                    }
                }
                else
                {
                    ManageSkillDescription();
                    ManageMasteryVisuals(selectedunitCharacter);


                }

                ManageStatusAilmentVisuals(selectedunit);
                ManageExamodeVisuals(selectedunitCharacter);
                NameTMP.text = selectedunitCharacter.name;
                ExpAndLevelTMP.text = "Lvl: " + selectedunitCharacter.level + "\nExp: " + selectedunitCharacter.experience;
                HPTMP.text = "HP: " + selectedunitCharacter.currentHP + "/" + selectedunitCharacter.AjustedStats.HP;
                HPLifebar.fillAmount = (float)selectedunitCharacter.currentHP / (float)selectedunitCharacter.AjustedStats.HP;
                if (selectedunitCharacter.enemyStats != null && selectedunitCharacter.enemyStats.RemainingLifebars > 0)
                {
                    HPTMP.text += "( +" + (selectedunitCharacter.enemyStats.RemainingLifebars * selectedunitCharacter.AjustedStats.HP) + ")";
                }

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
                    ExpBarFilling.fillAmount = selectedunitCharacter.experience / 100f * 0.75f;
                }
                else
                {
                    if (ExpBarFilling.transform.parent.gameObject.activeSelf)
                    {
                        ExpBarFilling.transform.parent.gameObject.SetActive(false);
                    }
                }

                AllStatsSkillBonus statsmods = selectedunit.GetComponent<UnitScript>().GetStatSkillBonus(null);

                string strcolorstring = getcolorstring(statsmods.Strength);
                string psycolorstring = getcolorstring(statsmods.Psyche);

                StrAndPsyTMP.text = "Str: " + strcolorstring + (selectedunitCharacter.AjustedStats.Strength + statsmods.Strength) + "</color>\n";

                StrAndPsyTMP.text += "Psy: " + psycolorstring + (selectedunitCharacter.AjustedStats.Psyche + statsmods.Psyche);

                string defcolorstring = getcolorstring(statsmods.Defense);
                string rescolorstring = getcolorstring(statsmods.Resistance);

                DefAndResTMP.text = "Def: " + defcolorstring + (selectedunitCharacter.AjustedStats.Defense + statsmods.Defense) + "</color>\n";

                DefAndResTMP.text += "Res: " + rescolorstring + (selectedunitCharacter.AjustedStats.Resistance + statsmods.Resistance);

                string dexcolorstring = getcolorstring(statsmods.Dexterity);
                string spdcolorstring = getcolorstring(statsmods.Speed);

                SpdAndDexTMP.text = "Dex: " + dexcolorstring + (selectedunitCharacter.AjustedStats.Dexterity + statsmods.Dexterity) + "</color>\n";

                SpdAndDexTMP.text += "Spd: " + spdcolorstring + (selectedunitCharacter.AjustedStats.Speed + statsmods.Speed);

                (int BaseDamage, int damagebonus) = ActionsMenu.CalculateDamage(selectedunit, true);

                string dmgcolorstring = getcolorstring(damagebonus);

                DmgAndMovTMP.text = "Dmg: " + dmgcolorstring + BaseDamage + "</color>\nMvt: " + (selectedunitCharacter.movements - 1);

                equipment EquipedWeapon = selectedunit.GetComponent<UnitScript>().GetFirstWeapon();
                EquipedWeaponIco.sprite = GetWeaponIcons(EquipedWeapon.type);
                equipedweaponText.text = EquipedWeapon.Currentuses + "/" + EquipedWeapon.Maxuses;


            }
            else
            {
                if (!Skilltext.transform.parent.gameObject.activeSelf)
                {

                    Deactivate();
                }
            }

        }
    }

    private void ManageSkillDescription()
    {

        if (_ShowDetailsAction.IsPressed() && SkillButtonIDList.Count > 0 && !SkillDescription.transform.parent.gameObject.activeSelf && !ActionsMenu.gameObject.activeSelf && !NeutralMenu.activeSelf)
        {
            SkillButtonList[0].Select();
        }
        if (_CancelAction.IsPressed())
        {
            eventSystem.SetSelectedGameObject(null);
            Deactivate();
        }
        if (!SkillDescription.transform.gameObject.activeSelf)
        {
            SkillDescription.transform.gameObject.SetActive(true);
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

    }

    private void Deactivate()
    {
        if (!Skilltext.transform.parent.gameObject.activeSelf)
        {

            Skilltext.transform.parent.gameObject.SetActive(false);

            MasteryTexts[0].transform.parent.gameObject.SetActive(false);
        }
        if (SkillDescription.transform.parent.gameObject.activeSelf)
        {
            SkillDescription.transform.parent.gameObject.SetActive(false);
        }
    }

    private void ManageExamodeVisuals(Character Charactertouse)
    {
        DataScript _Datascript = DataScript.instance;
        int maxchapterreached = _Datascript.GetComponent<SaveManager>().maxchapterreached;
        bool skip = false;
        if (!Charactertouse.playableStats.protagonist)
        {
            skip = true;
        }
        if (Charactertouse.name.ToLower() == "zack" && _Datascript.ExamodeUnlockChapter_Zack > maxchapterreached)
        {
            skip = true;
        }
        else if (Charactertouse.name.ToLower() == "kira" && _Datascript.ExamodeUnlockChapter_Kira > maxchapterreached)
        {
            skip = true;
        }
        else if (Charactertouse.name.ToLower() == "gale" && _Datascript.ExamodeUnlockChapter_Gale > maxchapterreached)
        {
            skip = true;
        }
        if (skip)
        {
            if (ExamodeGO.activeSelf)
            {
                ExamodeGO.SetActive(false);
            }
        }
        else
        {
            if (!ExamodeGO.activeSelf)
            {
                ExamodeGO.SetActive(true);
            }

            float ratioforbar;
            Color Colortouse;
            if (Charactertouse.ExamodeClass.remaingExamodeTurns > 0)
            {
                ratioforbar = (float)Charactertouse.ExamodeClass.remaingExamodeTurns / (float)_Datascript.ExamodeMaxTurns;
                Colortouse = ExamodeActivatedColor;
            }
            else
            {
                ratioforbar = (float)Charactertouse.ExamodeClass.ExamodePoints / (float)_Datascript.ExamodePointsForActivation;
                Colortouse = ExamodeDeactivatedColor;
            }
            ExamodeBar.fillAmount = ratioforbar;
            ExamodeBar.color = Colortouse;
        }
    }

    private void ManageMasteryVisuals(Character unit)
    {
        if (unit.affiliation == "playable")
        {
            if (MasteryTexts[0].transform.parent.gameObject.activeSelf == false)
            {
                MasteryTexts[0].transform.parent.gameObject.SetActive(true);
            }

        }
        else
        {
            if (MasteryTexts[0].transform.parent.gameObject.activeSelf)
            {
                MasteryTexts[0].transform.parent.gameObject.SetActive(false);
            }

            return;
        }

        List<WeaponMastery> masteries = unit.Masteries;
        int barID = 0;
        for (int i = 0; i < masteries.Count; i++)
        {
            if (MasteryExpBars == null || MasteryExpBars.Count <= i || MasteryExpBars[i] == null)
            {
                continue;
            }
            if (MasteryTexts == null || MasteryTexts.Count <= i || MasteryTexts[i] == null)
            {
                continue;
            }
            if (MasteryImages == null || MasteryImages.Count <= i || MasteryImages[i] == null)
            {
                continue;
            }

            if (!MasteryExpBars[i].parent.gameObject.activeSelf)
            {
                MasteryExpBars[i].parent.gameObject.SetActive(true);
            }
            DataScript ds = DataScript.instance;
            string masterylevel = "";

            switch (masteries[i].Level)
            {
                case (-1):
                    continue;
                case (0):
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel0;
                    masterylevel = "X";
                    break;
                case (1):
                    masterylevel = "D";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel1;
                    break;
                case (2):
                    masterylevel = "C";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel2;
                    break;
                case (3):
                    masterylevel = "B";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = (float)masteries[i].Exp / ds.MasteryforLevel3;
                    break;
                case (4):
                    masterylevel = "A";
                    MasteryExpBars[barID].GetComponent<Image>().fillAmount = 1f;
                    break;
            }

            MasteryImages[i].sprite = GetWeaponIcons(masteries[i].weapontype);
            MasteryTexts[i].text = masterylevel;


            barID++;

        }
        for (int i = barID; i < MasteryExpBars.Count; i++)
        {
            if (MasteryExpBars[i].parent.gameObject.activeSelf)
            {
                MasteryExpBars[i].parent.gameObject.SetActive(false);
            }


        }

    }

    private Sprite GetWeaponIcons(int weaponclass)
    {

        return WeaponClassImages[weaponclass];
    }

    private Sprite GetWeaponIcons(string weapontype)
    {

        switch (weapontype.ToLower())
        {
            case "":
                return WeaponClassImages[0];
            case "sword":
                return WeaponClassImages[1];
            case "spear":
                return WeaponClassImages[2];
            case "greatsword":
                return WeaponClassImages[3];
            case "bow":
                return WeaponClassImages[4];
            case "scythe":
                return WeaponClassImages[5];
            case "shield":
                return WeaponClassImages[6];
            case "staff":
                return WeaponClassImages[7];
        }
        return null;
    }

    private void ManagedSkillVisuals(Character unit)
    {
        SkillButtonIDList = new List<int>();

        int usedindex = 0;


        if (unit.UnitSkill != 0)
        {
            usedindex++;
            if (!SkillButtonList[0].gameObject.activeSelf)
            {
                SkillButtonList[0].gameObject.SetActive(true);
            }

            DataScript.Skill unitskill = GetSkill(unit.UnitSkill);
            SkillButtonList[0].image.color = BaseSkillColor;
            SkillButtonList[0].GetComponentInChildren<TextMeshProUGUI>().text = unitskill.name;
            SkillButtonIDList.Add(unitskill.ID);

        }

        if (unit.SecondUnitSkill != 0)
        {
            if (!SkillButtonList[usedindex].gameObject.activeSelf)
            {
                SkillButtonList[usedindex].gameObject.SetActive(true);
            }

            DataScript.Skill unitskill = GetSkill(unit.SecondUnitSkill);
            SkillButtonList[usedindex].image.color = BaseSkillColor;
            SkillButtonList[usedindex].GetComponentInChildren<TextMeshProUGUI>().text = unitskill.name;
            SkillButtonIDList.Add(unitskill.ID);
            usedindex++;
        }

        for (int i = 0; i < Mathf.Min(unit.EquipedSkills.Count, 5); i++)
        {
            SkillButtonList[i].image.color = BaseSkillColor;
            if (!SkillButtonList[i + usedindex].gameObject.activeSelf)
            {
                SkillButtonList[i + usedindex].gameObject.SetActive(true);
            }

            DataScript.Skill equipedskill = GetSkill(unit.EquipedSkills[i]);

            SkillButtonList[i + usedindex].GetComponentInChildren<TextMeshProUGUI>().text = equipedskill.name;
            SkillButtonIDList.Add(equipedskill.ID);
        }

        for (int i = Mathf.Min(unit.EquipedSkills.Count, 5); i < 5; i++)
        {
            if (i == Mathf.Min(unit.EquipedSkills.Count, 5) && unit.TemporarySkill != 0)
            {
                SkillButtonList[i + usedindex].image.color = TemporarySkillColor;
                if (!SkillButtonList[i + usedindex].gameObject.activeSelf)
                {
                    SkillButtonList[i + usedindex].gameObject.SetActive(true);
                }
                DataScript.Skill tempskill = GetSkill(unit.TemporarySkill);

                SkillButtonList[i + usedindex].GetComponentInChildren<TextMeshProUGUI>().text = tempskill.name;
                SkillButtonIDList.Add(tempskill.ID);
            }
            else
            {
                if (SkillButtonList[i + usedindex].gameObject.activeSelf)
                {
                    SkillButtonList[i + usedindex].gameObject.SetActive(false);
                }
            }

        }

        int tempskillused = 0;
        if (unit.TemporarySkill != 0)
        {
            tempskillused = 1;
        }

        for (int i = Mathf.Min(unit.EquipedSkills.Count, 5) + usedindex + tempskillused; i < SkillButtonList.Count; i++)
        {
            if (SkillButtonList[i].gameObject.activeSelf)
            {
                SkillButtonList[i].gameObject.SetActive(false);
            }

        }





        if (unit.UnitSkill != 0 || unit.SecondUnitSkill != 0 || unit.EquipedSkills.Count > 0)
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
                SkillDescription.transform.gameObject.SetActive(false);
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

    private void ManageStatusAilmentVisuals(GameObject Unit)
    {
        Character UnitChar = Unit.GetComponent<UnitScript>().UnitCharacteristics;

        int lastactiveID = 0;

        if (UnitChar.statusEffects.BurnTurns > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=19>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.BurnTurns + "";
            lastactiveID++;
        }
        if (UnitChar.statusEffects.StunTurns > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=20>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.StunTurns + "";
            lastactiveID++;
        }
        if (UnitChar.statusEffects.ParalyzedTurns > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=21>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.ParalyzedTurns + "";
            lastactiveID++;
        }
        if (UnitChar.statusEffects.ConcussionTunrs > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=22>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.ConcussionTunrs + "";
            lastactiveID++;
        }
        if (UnitChar.statusEffects.WeaknessTurns > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=23>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.WeaknessTurns + "";
            lastactiveID++;
        }
        if (UnitChar.statusEffects.RegenTurns > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=24>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.RegenTurns + "";
            lastactiveID++;
        }
        if (UnitChar.statusEffects.AccelerationTurns > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=25>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.AccelerationTurns + "";
            lastactiveID++;
        }
        if (UnitChar.statusEffects.PowerTurns > 0)
        {
            if (!AilmentIconList[lastactiveID].gameObject.activeSelf)
            {
                AilmentIconList[lastactiveID].transform.parent.gameObject.SetActive(true);
            }
            AilmentIconList[lastactiveID].text = "<sprite=26>";
            AilmentDurationLst[lastactiveID].text = UnitChar.statusEffects.PowerTurns + "";
            lastactiveID++;
        }
        for (int i = lastactiveID; i < AilmentIconList.Count; i++)
        {
            if (AilmentIconList[i].gameObject.activeSelf)
            {
                AilmentIconList[i].transform.parent.gameObject.SetActive(false);
            }
            AilmentDurationLst[i].text = "";
        }


    }

    private string getcolorstring(int valuemodifier)
    {
        if (valuemodifier > 0)
        {
            return "</color><color=green>";
        }
        else if (valuemodifier < 0)
        {
            return "</color><color=red>";
        }
        else
        {
            return "</color>";
        }
    }
}
