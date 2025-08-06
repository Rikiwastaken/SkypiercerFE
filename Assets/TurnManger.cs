using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnitScript;

public class TurnManger : MonoBehaviour
{
    public int currentTurn;

    public string currentlyplaying; // playable, enemy, other

    public List<Character> playableunit;

    public List<Character> enemyunit;

    public List<Character> otherunits;

    public TextMeshProUGUI turntext;

    public GameOverScript GameOverScript;

    private GridScript GridScript;

    private bool updatevisuals;

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if(updatevisuals)
        {
            UpdateVisuals();
            updatevisuals = false;
        }

        if(playableunit.Count ==0)
        {
            GameOverScript.gameObject.SetActive(true);
            return;
        }
        if (enemyunit.Count == 0)
        {
            GameOverScript.gameObject.SetActive(true);
            GameOverScript.victory = true;
            return;
        }
        if (currentTurn == 0)
        {
            currentTurn = 1;
        }
        if(currentlyplaying == "")
        {
            currentlyplaying = "playable";
        }
        ManageTurnRotation();
        UpdateText();
    }
    private void ManageTurnRotation()
    {
        if(currentlyplaying=="playable")
        {
            bool alldone = true;
            foreach(Character character in playableunit)
            {
                if(!character.alreadyplayed || !character.alreadymoved)
                {
                    alldone = false;
                }
            }
            if(alldone)
            {
                foreach (Character character in enemyunit)
                {
                    character.alreadyplayed = false;
                    character.alreadymoved = false;
                }
                currentlyplaying = "enemy";
                updatevisuals = true;
                return;
            }
        }
        if (currentlyplaying == "enemy")
        {
            bool alldone = true;
            foreach (Character character in enemyunit)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    alldone = false;
                }
            }
            if (alldone)
            {
                foreach (Character character in otherunits)
                {
                    character.alreadyplayed = false;
                    character.alreadymoved = false;
                }
                currentlyplaying = "other";
                updatevisuals = true;
                return;
            }
        }
        if (currentlyplaying == "other")
        {
            bool alldone = true;
            foreach (Character character in otherunits)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    alldone = false;
                }
            }
            if (alldone)
            {
                foreach (Character character in playableunit)
                {
                    character.alreadyplayed = false;
                    character.alreadymoved = false;
                }
                if(FindAnyObjectByType<ActionManager>())
                {
                    FindAnyObjectByType<ActionManager>().preventfromlockingafteraction = true;
                    FindAnyObjectByType<GridScript>().ResetAllSelections();
                }
                currentlyplaying = "playable";
                currentTurn++;
                updatevisuals = true;
                return;
            }
        }
    }

    private void UpdateVisuals()
    {
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

        GridScript.ResetAllSelections();

        if(GridScript.selection!=null)
        {
            GridScript.ShowMovement();
        }
    }
    private void UpdateText()
    {
        if (currentlyplaying == "playable")
        {
            int numberremaining = 0;
            foreach( Character character in playableunit)
            {
                if(!character.alreadyplayed || !character.alreadymoved)
                {
                    numberremaining++;
                }
            }
            turntext.text = "Turn : Allies \nRemaining : " + numberremaining;
        }
        if (currentlyplaying == "enemy")
        {
            int numberremaining = 0;
            foreach (Character character in enemyunit)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    numberremaining++;
                }
            }
            turntext.text = "Turn : Enemies \nRemaining : " + numberremaining;
        }
        if (currentlyplaying == "other")
        {
            int numberremaining = 0;
            foreach (Character character in otherunits)
            {
                if (!character.alreadyplayed || !character.alreadymoved)
                {
                    numberremaining++;
                }
            }
            turntext.text = "Turn : Others \nRemaining : " + numberremaining;
        }
    }

    public void InitializeUnitLists(List<GameObject> allunits)
    {
        playableunit = new List<Character>();
        enemyunit = new List<Character>();
        otherunits = new List<Character>();
        foreach (GameObject character in allunits)
        {
            if(character.GetComponent<UnitScript>().UnitCharacteristics.affiliation=="playable")
            {
                playableunit.Add(character.GetComponent<UnitScript>().UnitCharacteristics);
            }
            else if(character.GetComponent<UnitScript>().UnitCharacteristics.affiliation=="enemy")
            {
                enemyunit.Add(character.GetComponent<UnitScript>().UnitCharacteristics);
            }
            else
            {
                otherunits.Add(character.GetComponent<UnitScript>().UnitCharacteristics);
            }
            
        }
    }
}
