using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DataScript;
using static UnitScript;

public class UnitDeploymentButton : MonoBehaviour
{

    private UnitDeploymentScript UnitDeploymentScript;


    public Character Character;
    public InventoryItem Item;
    public int CharacterID;

    public bool isskillbutton;

    private MapInitializer MapInitializer;

    private DataScript DataScript;

    private InputManager InputManager;

    private SkillEditionScript SkillEditionScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnitDeploymentScript = GetComponentInParent<UnitDeploymentScript>();
        DataScript = FindAnyObjectByType<DataScript>();
        MapInitializer = FindAnyObjectByType<MapInitializer>();
        InputManager = FindAnyObjectByType<InputManager>();
    }

    private void FixedUpdate()
    {
        if (SkillEditionScript == null)
        {
            SkillEditionScript = FindAnyObjectByType<SkillEditionScript>();
        }


        GetComponentInChildren<TextMeshProUGUI>().text = "None";
        if (Character != null)
        {
            if (Character.name != "")
            {
                if (Character.playableStats.battalion != "Zack" && Character.playableStats.battalion != "Kira" && Character.playableStats.battalion != "Gale")
                {
                    Character.playableStats.battalion = "Zack";
                }
                GetComponentInChildren<TextMeshProUGUI>().text = Character.name + " Lvl " + Character.level;
                if (!isskillbutton)
                {
                    if (Character.playableStats.deployunit)
                    {
                        GetComponent<Image>().color = Color.green;
                    }
                    else
                    {
                        GetComponent<Image>().color = Color.red;
                    }
                }
                if (InputManager.Telekinesisjustpressed && EventSystem.current.currentSelectedGameObject == gameObject && !Character.playableStats.protagonist)
                {
                    ChangeBattallion();

                }
                return;
            }
            else
            {
                GetComponentInChildren<TextMeshProUGUI>().text = "None";
            }
        }
        if (Item != null)
        {
            if (Item.ID != 0)
            {
                Skill skill = DataScript.SkillList[Item.ID];
                GetComponentInChildren<TextMeshProUGUI>().text = skill.name + " : " + Item.Quantity;
                if (SkillEditionScript.selectedcharacter.UnitSkill == Item.ID)
                {
                    GetComponent<Image>().color = Color.green;
                }
                else if (SkillEditionScript.selectedcharacter.EquipedSkills.Contains(Item.ID))
                {
                    GetComponent<Image>().color = Color.cyan;
                }
                else
                {
                    GetComponent<Image>().color = Color.white;
                }
            }
            else if (transform.parent.gameObject.name == "EditSkills")
            {
                GetComponentInChildren<TextMeshProUGUI>().text = "None";
            }
            return;
        }
    }

    private void ChangeBattallion()
    {

        bool KiraUnlocked = false;
        bool GaleUnlocked = false;

        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (character.name.ToLower() == "kira" && character.playableStats.unlocked)
            {
                KiraUnlocked = true;
            }
            if (character.name.ToLower() == "gale" && character.playableStats.unlocked)
            {
                GaleUnlocked = true;
            }
        }
        //exception for test map
        if (SceneManager.GetActiveScene().name == "TestMap")
        {
            KiraUnlocked = true;
            GaleUnlocked = true;
        }

        if (Character.playableStats.battalion == "Zack")
        {
            if (KiraUnlocked)
            {
                Character.playableStats.battalion = "Kira";
            }
            else if (GaleUnlocked)
            {
                Character.playableStats.battalion = "Gale";
            }
        }
        else if (Character.playableStats.battalion == "Kira")
        {
            if (GaleUnlocked)
            {
                Character.playableStats.battalion = "Gale";
            }
            else
            {
                Character.playableStats.battalion = "Zack";
            }
        }
        else
        {
            Character.playableStats.battalion = "Zack";
        }
    }

    // Update is called once per frame
    public void LockorUnlock()
    {
        int numberofcharacterdeployed = 0;
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (character.playableStats.deployunit)
            {
                numberofcharacterdeployed++;
            }

        }

        if (Character.playableStats.deployunit)
        {
            Character.playableStats.deployunit = false;
            MapInitializer.InitializePlayers();
        }
        else if (!Character.playableStats.deployunit && numberofcharacterdeployed < MapInitializer.playablepos.Count)
        {
            Character.playableStats.deployunit = true;
            MapInitializer.InitializePlayers();
        }
    }
}
