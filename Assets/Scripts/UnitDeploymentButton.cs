using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    private SkillEditionScript SkillEditionScript;

    public GameObject lockimage;

    private Image ButtonBGImage;

    public Color ImageDefaultColor;

    public Color ZackColor;
    public Color KiraColor;
    public Color GaleColor;
    public Color UnSelectedColor;

    private InputAction _TelekinesisAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnitDeploymentScript = GetComponentInParent<UnitDeploymentScript>();
        MapInitializer = FindAnyObjectByType<MapInitializer>();
        ButtonBGImage = GetComponent<Button>().image;
        ImageDefaultColor = ButtonBGImage.color;
        _TelekinesisAction = InputSystem.actions.FindAction("TelekinesisToggle");
    }

    private void Update()
    {
        if (SkillEditionScript == null)
        {
            SkillEditionScript = FindAnyObjectByType<SkillEditionScript>();
        }


        GetComponentInChildren<TextMeshProUGUI>().text = "";
        if (Character != null)
        {

            if (Character.name != "")
            {
                if (lockimage != null && !lockimage.activeSelf && MapInitializer != null && MapInitializer.ForcedCharacters.Contains(Character.ID))
                {
                    lockimage.SetActive(true);
                }

                if (lockimage != null && lockimage.activeSelf && (MapInitializer == null || !MapInitializer.ForcedCharacters.Contains(Character.ID)))
                {
                    lockimage.SetActive(false);
                }

                if (Character.playableStats.battalion.ToLower() != "zack" && Character.playableStats.battalion.ToLower() != "kira" && Character.playableStats.battalion.ToLower() != "gale")
                {
                    Character.playableStats.battalion = "Zack";
                }
                GetComponentInChildren<TextMeshProUGUI>().text = Character.name + " Lvl " + Character.level;
                if (!isskillbutton)
                {
                    if (Character.playableStats.deployunit)
                    {
                        if (Character.playableStats.battalion.ToLower() == "kira")
                        {
                            ButtonBGImage.color = KiraColor;
                        }
                        else if (Character.playableStats.battalion.ToLower() == "gale")
                        {
                            ButtonBGImage.color = GaleColor;
                        }
                        else
                        {
                            ButtonBGImage.color = ZackColor;
                        }


                    }
                    else
                    {
                        ButtonBGImage.color = UnSelectedColor;
                    }
                }
                if (_TelekinesisAction.WasPressedThisFrame() && EventSystem.current.currentSelectedGameObject == gameObject && !Character.playableStats.protagonist)
                {
                    ChangeBattallion();

                }
                return;
            }
            else
            {
                if (lockimage != null && lockimage.activeSelf)
                {
                    lockimage.SetActive(false);
                }
                GetComponentInChildren<TextMeshProUGUI>().text = "";
                ButtonBGImage.color = ImageDefaultColor;
            }
        }
        if (Item != null)
        {
            if (Item.ID != 0)
            {
                Skill skill = DataScript.instance.SkillList[Item.ID];
                GetComponentInChildren<TextMeshProUGUI>().text = skill.name;
                if (SkillEditionScript != null)
                {
                    GetComponentInChildren<TextMeshProUGUI>().text += " : " + Item.Quantity;
                    if (SkillEditionScript.selectedcharacter.UnitSkill == Item.ID)
                    {
                        Color newcolor = Color.green * 0.5f;
                        newcolor.a = 1;
                        ButtonBGImage.color = newcolor;
                    }
                    else if (SkillEditionScript.selectedcharacter.EquipedSkills.Contains(Item.ID))
                    {
                        Color newcolor = Color.blue * 0.5f;
                        newcolor.a = 1;
                        ButtonBGImage.color = newcolor;
                    }
                    else
                    {
                        ButtonBGImage.color = ImageDefaultColor;
                    }
                }

            }
            else if (transform.parent.gameObject.name == "EditSkills")
            {
                GetComponentInChildren<TextMeshProUGUI>().text = "";
                ButtonBGImage.color = ImageDefaultColor;
            }
            return;
        }
    }

    private void ChangeBattallion()
    {

        bool KiraUnlocked = false;
        bool GaleUnlocked = false;

        foreach (Character character in DataScript.instance.PlayableCharacterList)
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

        if (Character.playableStats.battalion.ToLower() == "zack")
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
        else if (Character.playableStats.battalion.ToLower() == "kira")
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
        foreach (Character character in DataScript.instance.PlayableCharacterList)
        {
            if (character.playableStats.deployunit)
            {
                numberofcharacterdeployed++;
            }

        }

        if (Character.playableStats.deployunit && !MapInitializer.ForcedCharacters.Contains(Character.ID))
        {
            Character.playableStats.deployunit = false;
            MapInitializer.DeleteUnit(Character);
        }
        else if (!Character.playableStats.deployunit && numberofcharacterdeployed < MapInitializer.playablepos.Count)
        {
            Character.playableStats.deployunit = true;
            MapInitializer.AddUnit(Character);
        }
    }
}
