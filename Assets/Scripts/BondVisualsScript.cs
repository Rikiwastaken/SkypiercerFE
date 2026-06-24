using System;
using System.Collections.Generic;
using UnityEngine;
using static DataScript;

public class BondVisualsScript : MonoBehaviour
{

    public static BondVisualsScript instance;

    private BezierCurveManager curveManager;
    private GridScript _GridScript;
    private DataScript _datascript;

    [Serializable]
    public class BondPerCharacter
    {
        public int unitID;
        public List<int> BondWithOtherUnits;
    }

    public List<BondPerCharacter> bondPerCharacterList;

    private GameObject previousSelectedUnit;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curveManager = GetComponent<BezierCurveManager>();
        _GridScript = GridScript.instance;
        _datascript = DataScript.instance;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject selectedUnit = _GridScript.GetUnit(_GridScript.selection);

        if (selectedUnit != null)
        {
            if (selectedUnit != previousSelectedUnit && selectedUnit.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
            {
                ShowAffinityLine(selectedUnit);
            }
        }
        else
        {
            curveManager.DisableLines();
        }

        previousSelectedUnit = selectedUnit;
    }

    private void ShowAffinityLine(GameObject UnitGO)
    {
        curveManager.DisableLines();
        int CharacterID = UnitGO.GetComponent<UnitScript>().UnitCharacteristics.ID;
        GridSquareScript CharacterTile = _datascript.PlayableCharacterList[CharacterID].currentTile;

        List<int> charactersbonded = new List<int>();
        foreach (BondPerCharacter bondInfo in bondPerCharacterList)
        {
            if (bondInfo.unitID == CharacterID)
            {
                charactersbonded = bondInfo.BondWithOtherUnits;
                break;
            }
        }

        int lineID = 0;
        foreach (int BondedcharacterID in charactersbonded)
        {
            GridSquareScript BondedCharacterTile = _datascript.PlayableCharacterList[BondedcharacterID].currentTile;
            if (ManhattanDistance(CharacterTile, BondedCharacterTile) <= 2)
            {
                curveManager.DrawLineBetween2Tiles(CharacterTile, BondedCharacterTile, lineID);
                lineID++;
            }
        }

    }

    private int ManhattanDistance(GridSquareScript tile1, GridSquareScript tile2)
    {
        if (tile1 == null || tile2 == null)
        {
            return 0;
        }
        return (int)(Mathf.Abs(tile1.GridCoordinates.x - tile2.GridCoordinates.x) + Mathf.Abs(tile1.GridCoordinates.y - tile2.GridCoordinates.y));
    }

    public void InitializeBondList()
    {
        if (_datascript == null)
        {
            _datascript = DataScript.instance;
        }
        GridScript GS = GridScript.instance;

        List<int> unitpresentinscene = new List<int>();

        List<BondPerCharacter> newbondlist = new List<BondPerCharacter>();

        foreach (GameObject unitGO in GS.allunitGOs)
        {
            UnitScript.Character unitchar = unitGO.GetComponent<UnitScript>().UnitCharacteristics;

            if (unitchar.affiliation == "playable")
            {
                unitpresentinscene.Add(unitchar.ID);
            }
        }

        foreach (int unitID in unitpresentinscene)
        {
            BondPerCharacter newbond = new BondPerCharacter() { unitID = unitID, BondWithOtherUnits = new List<int>() };
            foreach (Bonds bond in _datascript.BondsList)
            {
                if (bond.Characters.Contains(unitID) && bond.BondLevel > 0)
                {
                    foreach (int character in bond.Characters)
                    {
                        if (character != unitID)
                        {
                            newbond.BondWithOtherUnits.Add(character);
                        }
                    }
                    newbondlist.Add(newbond);
                }
            }
        }

        bondPerCharacterList = newbondlist;

    }
}
