using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnitScript;

public class UnitDeploymentButton : MonoBehaviour
{

    private UnitDeploymentScript UnitDeploymentScript;

    public Character Character;
    public int CharacterID;
    private MapInitializer MapInitializer;

    private DataScript DataScript;

    private InputManager InputManager;

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
        if(Character.name !="")
        {
            GetComponentInChildren<TextMeshProUGUI>().text = Character.name + " Lvl " + Character.level;
            if(Character.deployunit)
            {
                GetComponent<Image>().color = Color.green;
            }
            else
            {
                GetComponent<Image>().color = Color.red;
            }
            if (InputManager.Telekinesisjustpressed && EventSystem.current.currentSelectedGameObject==gameObject && !Character.protagonist)
            {
                if(Character.battalion=="Zack")
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
        }
        else
        {
            GetComponentInChildren<TextMeshProUGUI>().text = "No Unit";
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
