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

    // Update is called once per frame
    void FixedUpdate()
    {
        if(currentTurn == 0)
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
                if(!character.alreadyplayed)
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
                return;
            }
        }
        if (currentlyplaying == "enemy")
        {
            bool alldone = true;
            foreach (Character character in enemyunit)
            {
                if (!character.alreadyplayed)
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
                return;
            }
        }
        if (currentlyplaying == "other")
        {
            bool alldone = true;
            foreach (Character character in otherunits)
            {
                if (!character.alreadyplayed)
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
                currentlyplaying = "playable";
                currentTurn++;
                return;
            }
        }
    }

    private void UpdateText()
    {
        if (currentlyplaying == "playable")
        {
            int numberremaining = 0;
            foreach( Character character in playableunit)
            {
                if(!character.alreadyplayed)
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
                if (!character.alreadyplayed)
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
                if (!character.alreadyplayed)
                {
                    numberremaining++;
                }
            }
            turntext.text = "Turn : Others \nRemaining : " + numberremaining;
        }
    }

    public void InitializeUnitLists(List<Character> allunits)
    {
        foreach (Character character in allunits)
        {
            if(character.affiliation=="playable")
            {
                playableunit.Add(character);
            }
            else if(character.affiliation=="enemy")
            {
                enemyunit.Add(character);
            }
            else
            {
                otherunits.Add(character);
            }
            
        }
    }
}
