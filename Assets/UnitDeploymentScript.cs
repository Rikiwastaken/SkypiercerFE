using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnitScript;

public class UnitDeploymentScript : MonoBehaviour
{

    private DataScript DataScript;

    private MapInitializer MapInitializer;

    private int numberofunitstodeplay;

    private GridScript gridscript;

    private InputManager inputmanager;

    public List<GameObject> PreBattleMenuItems;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DataScript = FindAnyObjectByType<DataScript>();
        MapInitializer = FindAnyObjectByType<MapInitializer>();
        gridscript = FindAnyObjectByType<GridScript>();
        inputmanager = FindAnyObjectByType<InputManager>();
        numberofunitstodeplay = MapInitializer.playablepos.Count;
        InitializeButtons();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gridscript.movementbuffercounter = 3;

        if(inputmanager.canceljustpressed)
        {
            if(numberofSelectedUnits()>0)
            {
                foreach (GameObject go in PreBattleMenuItems)
                {
                    go.SetActive(true);
                }
                gameObject.SetActive(false);
                return;
            }
        }



        GameObject currentselected = EventSystem.current.currentSelectedGameObject;
        bool buttonselected = false;
        if(currentselected != null )
        {
            for( int i = 0; i < transform.childCount; i++ )
            {
                if(transform.GetChild(i).gameObject == currentselected)
                {
                    buttonselected = true; break;
                }
            }
        }
        if ( !buttonselected || currentselected==null)
        {
            EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
        }
    }

    private void InitializeButtons()
    {
        OrderUnits();
        for (int i = 0; i < Mathf.Min(DataScript.PlayableCharacterList.Count,transform.childCount); i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = DataScript.PlayableCharacterList[i];
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().CharacterID = i;
            if(i<numberofunitstodeplay)
            {
                transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character.deployunit = true;
            }
        }
        for(int i = DataScript.PlayableCharacterList.Count; i< Mathf.Min(DataScript.PlayableCharacterList.Count, transform.childCount);i++)
        {
            transform.GetChild(i).GetComponent<UnitDeploymentButton>().Character = null;
        }

    }

    private int numberofSelectedUnits()
    {
        int numberofunits = 0;
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (character.deployunit)
            {
                numberofunits++;
            }

        }
        return numberofunits;
    }

    private void OrderUnits()
    {
        List<Character> newcharacterlist = new List<Character>();
        foreach(Character character in DataScript.PlayableCharacterList)
        {
            if(character.deployunit)
            {
                newcharacterlist.Add(character);
                character.deployunit = false;
            }

        }
        foreach (Character character in DataScript.PlayableCharacterList)
        {
            if (!newcharacterlist.Contains(character))
            {
                newcharacterlist.Add(character);
            }
        }
        DataScript.PlayableCharacterList = newcharacterlist;
    }

}
