using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnitScript;

public class RosterScript : MonoBehaviour
{

    private int characterwindowindex = 0;
    [SerializeField] private TextMeshProUGUI PageNumberText;
    [SerializeField] private Transform ButtonHolder;
    [SerializeField] private TextAsset CharacterSummaries;
    [SerializeField] private GameObject CampMenu;
    private List<string> CharacterSummariesList;
    private List<Character> unlockedplayables;
    private InputAction _CancelAction;
    private InputAction _NextWeaponAction;
    private InputAction _PreviousWeaponAction;
    private Character SelectedChar;

    [Header("CharacterDisplay")]
    [SerializeField] private TextMeshProUGUI CharacterStats;
    [SerializeField] private TextMeshProUGUI CharacterGrowth;
    [SerializeField] private TextMeshProUGUI CharacterBlades;
    [SerializeField] private TextMeshProUGUI CharacterSkills;
    [SerializeField] private TextMeshProUGUI CharacterDescription;
    [SerializeField] private Image CharacterSprite;

    [Header("Gears")]
    [SerializeField] private Sprite ZackGear;
    [SerializeField] private Sprite KiraGear;
    [SerializeField] private Sprite GaleGear;
    [SerializeField] private List<Image> Gears;
    [SerializeField] private float GearRotationPerSecond;


    public class CharacterDescriptionWrapper
    {
        public List<string> CharacterDescriptions;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _PreviousWeaponAction = InputSystem.actions.FindAction("PreviousWeapon");
        _NextWeaponAction = InputSystem.actions.FindAction("NextWeapon");
        _CancelAction = InputSystem.actions.FindAction("Cancel");
        InitializeButtons();
        LoadDescriptions();
    }

    // Update is called once per frame
    void Update()
    {

        if (_CancelAction.WasPressedThisFrame())
        {
            gameObject.SetActive(false);
            CampMenu.SetActive(true);
            CampMenu.transform.GetChild(0).GetComponent<Button>().Select();
            return;
        }


        GearRotation();

        int maxwindowstoshow = unlockedplayables.Count / 10;
        if (unlockedplayables.Count % 10 != 0)
        {
            maxwindowstoshow++;
        }

        PageNumberText.text = (characterwindowindex + 1) + "/" + maxwindowstoshow;

        if (_PreviousWeaponAction.WasPressedThisFrame())
        {
            if (characterwindowindex > 0)
            {
                characterwindowindex--;
                InitializeButtons();
            }

        }

        if (_NextWeaponAction.WasPressedThisFrame())
        {
            if (characterwindowindex * 10 < unlockedplayables.Count - 10)
            {
                characterwindowindex++;
                InitializeButtons();
            }
        }
        GameObject currentselected = EventSystem.current.currentSelectedGameObject;

        if (currentselected != null)
        {

            for (int i = 0; i < 10; i++)
            {
                if (ButtonHolder.GetChild(i).gameObject == currentselected)
                {
                    UnitDeploymentButton unitDeploymentButton = currentselected.GetComponent<UnitDeploymentButton>();
                    if (unitDeploymentButton != null && unitDeploymentButton.Character != null && unitDeploymentButton.Character != SelectedChar)
                    {
                        SelectedChar = unitDeploymentButton.Character;
                        UpdateDisplay();
                        break;
                    }
                }
            }
        }

    }


    private void UpdateDisplay()
    {
        //Sprite
        CharacterSprite.sprite = DataScript.instance.DialogueCharacterList[SelectedChar.ID].Portrait;

        //Gears
        Sprite GearToUse = ZackGear;
        switch (SelectedChar.playableStats.battalion.ToLower())
        {
            case "zack":
                GearToUse = ZackGear;
                break;
            case "kira":
                GearToUse = KiraGear;
                break;
            case "gale":
                GearToUse = GaleGear;
                break;
        }

        foreach (Image gear in Gears)
        {
            gear.sprite = GearToUse;
        }

        //Stats
        string newstatString = "<align=center>Stats</align>";
        newstatString += "\nHP: " + (int)SelectedChar.stats.HP + "%";
        newstatString += "\nStrength: " + (int)SelectedChar.stats.Strength + "%";
        newstatString += "\nPsyche: " + (int)SelectedChar.stats.Psyche + "%";
        newstatString += "\nDefense: " + (int)SelectedChar.stats.Defense + "%";
        newstatString += "\nResistance: " + (int)SelectedChar.stats.Resistance + "%";
        newstatString += "\nSpeed: " + (int)SelectedChar.stats.Speed + "%";
        newstatString += "\nDexterity: " + (int)SelectedChar.stats.Dexterity + "%";
        newstatString += "\nLuck: " + (int)SelectedChar.stats.Luck + "%";

        CharacterStats.text = newstatString;


        //Growth
        string newgrowthString = "<align=center>Growths</align>";
        newgrowthString += "\nHP: " + (int)SelectedChar.growth.HPGrowth;
        newgrowthString += "\nStrength: " + (int)SelectedChar.growth.StrengthGrowth;
        newgrowthString += "\nPsyche: " + (int)SelectedChar.growth.PsycheGrowth;
        newgrowthString += "\nDefense: " + (int)SelectedChar.growth.DefenseGrowth;
        newgrowthString += "\nResistance: " + (int)SelectedChar.growth.ResistanceGrowth;
        newgrowthString += "\nSpeed: " + (int)SelectedChar.growth.SpeedGrowth;
        newgrowthString += "\nDexterity: " + (int)SelectedChar.growth.DexterityGrowth;
        newgrowthString += "\nLuck: " + (int)SelectedChar.growth.LuckGrowth;

        CharacterGrowth.text = newgrowthString;

        //Blades
        string newBladeString = "<align=center><size=15>Blades</size></align>";
        foreach (WeaponMastery mastery in SelectedChar.Masteries)
        {
            if (mastery.Level != -1)
            {

                string weaponname = "";
                switch (mastery.weapontype.ToLower())
                {
                    case "sword":
                        weaponname = "Sword";
                        break;
                    case "greatsword":
                        weaponname = "Gr.Sword";
                        break;
                    case "bow":
                        weaponname = "Bow";
                        break;
                    case "spear":
                        weaponname = "Spear";
                        break;
                    case "scythe":
                        weaponname = "Scythe";
                        break;
                    case "shield":
                        weaponname = "Shield";
                        break;
                    case "staff":
                        weaponname = "Staff";
                        break;
                    case "dagger":
                        weaponname = "Dagger";
                        break;
                }

                newBladeString += "\n" + CalculationToolsScript.GetWeaponSpriteString(mastery.weapontype) + weaponname + ": Lvl" + mastery.Level;
                if (mastery.Level < mastery.maxlevel)
                {
                    newBladeString += " (" + mastery.Exp + "/";
                    int neededexp = 0;
                    switch (mastery.Level)
                    {
                        case 0:
                            neededexp = DataScript.instance.MasteryforLevel0;
                            break;
                        case 1:
                            neededexp = DataScript.instance.MasteryforLevel1;
                            break;
                        case 2:
                            neededexp = DataScript.instance.MasteryforLevel2;
                            break;
                        case 3:
                            neededexp = DataScript.instance.MasteryforLevel3;
                            break;
                    }
                    newBladeString += neededexp + ")";
                }
            }
        }

        CharacterBlades.text = newBladeString;


        //Skills
        string NewSkillText = "Character Skills:";
        NewSkillText += "\n" + DataScript.instance.GetSkillName(SelectedChar.UnitSkill, SelectedChar);
        if (SelectedChar.SecondSkillUnlocked)
        {
            NewSkillText += "\n" + DataScript.instance.GetSkillName(SelectedChar.SecondUnitSkill, SelectedChar);
        }
        else
        {
            NewSkillText += "\nLocked";
        }

        CharacterSkills.text = NewSkillText;

        //Description
        if (CharacterSummariesList.Count > SelectedChar.ID)
        {
            CharacterDescription.text = CharacterSummariesList[SelectedChar.ID];
        }
        else
        {
            CharacterDescription.text = "No Description yet for " + SelectedChar.name + " :(";
        }

    }

    private void GearRotation()
    {

        float speed = GearRotationPerSecond;

        foreach (Image gear in Gears)
        {
            Quaternion newrotation = Quaternion.Euler(gear.transform.localRotation.eulerAngles + new Vector3(0f, 0f, speed * Time.deltaTime));
            gear.transform.localRotation = newrotation;
            speed *= -1f;
        }

    }

    private void InitializeButtons()
    {

        unlockedplayables = new List<Character>();


        foreach (Character playablechar in DataScript.instance.PlayableCharacterList)
        {
            if (playablechar.playableStats.unlocked)
            {
                unlockedplayables.Add(playablechar);
            }
        }


        for (int i = 0; i < Mathf.Min(unlockedplayables.Count - 10 * (characterwindowindex), 10); i++)
        {
            ButtonHolder.GetChild(i).GetComponent<UnitDeploymentButton>().Character = unlockedplayables[i + 10 * (characterwindowindex)];
            ButtonHolder.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID = unlockedplayables[i + 10 * (characterwindowindex)].ID;



        }
        for (int i = Mathf.Min(unlockedplayables.Count - 10 * (characterwindowindex), 10); i < 10; i++)
        {
            ButtonHolder.GetChild(i).GetComponent<UnitDeploymentButton>().Character = null;
        }

        EventSystem.current.SetSelectedGameObject(ButtonHolder.GetChild(0).gameObject);
    }

    private void LoadDescriptions()
    {

        CharacterDescriptionWrapper wrapper = JsonUtility.FromJson<CharacterDescriptionWrapper>(CharacterSummaries.text);
        if (wrapper == null || wrapper.CharacterDescriptions == null)
        {
            Debug.LogError("JSON file format invalid. Needs { \"CharacterDescriptions\": [ ... ] }");
            return;
        }

        CharacterSummariesList = wrapper.CharacterDescriptions;
    }

}
