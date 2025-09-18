using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnitScript;
using static DataScript;

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
        if(SkillEditionScript==null)
        {
            SkillEditionScript = FindAnyObjectByType<SkillEditionScript>();
        }
        

        GetComponentInChildren<TextMeshProUGUI>().text = "None";
        if (Character!=null)
        {
            if (Character.name != "")
            {
                GetComponentInChildren<TextMeshProUGUI>().text = Character.name + " Lvl " + Character.level;
                if(!isskillbutton)
                {
                    if (Character.deployunit)
                    {
                        GetComponent<Image>().color = Color.green;
                    }
                    else
                    {
                        GetComponent<Image>().color = Color.red;
                    }
                }
                if (InputManager.Telekinesisjustpressed && EventSystem.current.currentSelectedGameObject == gameObject && !Character.protagonist)
                {
                    if (Character.battalion == "Zack")
                    {
                        Character.battalion = "Kira";
                    }
                    else if (Character.battalion == "Kira")
                    {
                        Character.battalion = "Gale";
                    }
                    else
                    {
                        Character.battalion = "Zack";
                    }
                }
                return;
            }
            else
            {
                GetComponentInChildren<TextMeshProUGUI>().text = "None";
            }
        }
        if(Item!=null)
        {
            if(Item.ID!=0)
            {
                Skill skill = DataScript.SkillList[Item.ID];
                GetComponentInChildren<TextMeshProUGUI>().text = skill.name + " : " + Item.Quantity;
                if(SkillEditionScript.selectedcharacter.UnitSkill == Item.ID)
                {
                    GetComponent<Image>().color = Color.green;
                }
                else if(SkillEditionScript.selectedcharacter.EquipedSkills.Contains(Item.ID))
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

    // Update is called once per frame
    public void LockorUnlock()
    {
        int numberofcharacterdeployed = 0;
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (character.deployunit)
            {
                numberofcharacterdeployed++;
            }

        }

        if (Character.deployunit)
        {
            Character.deployunit = false;
            MapInitializer.InitializePlayers();
        }
        else if (!Character.deployunit && numberofcharacterdeployed <MapInitializer.playablepos.Count)
        {
            Character.deployunit = true;
            MapInitializer.InitializePlayers();
        }
    }
}
