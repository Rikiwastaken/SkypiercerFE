using System.Collections.Generic;
using UnityEngine;
using static UnitScript;


public class CharacterCircleVisuals : MonoBehaviour
{

    public Sprite CircleSprite;
    public Sprite emptysprite;

    public GridScript GridScript;

    private List<GameObject> allunitGOsList;

    public List<GameObject> UnitCircles;

    public Transform UnitCirclesHolder;

    public float YElevation;

    private int delaycounter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        allunitGOsList = GridScript.allunitGOs;

        if (delaycounter > 0)
        {
            delaycounter--;
        }
        else
        {
            delaycounter = (int)(0.5f / Time.deltaTime);
            UpdateFilling();
        }


    }


    public void InitializeUnitCircles()
    {
        int safeguard = 0;
        int UnitCircleslength = UnitCircles.Count;
        while (allunitGOsList.Count != UnitCircleslength && safeguard < 100)
        {
            if (UnitCircleslength > allunitGOsList.Count)
            {
                GameObject lastobject = UnitCircles[UnitCircles.Count - 1];
                UnitCircles.Remove(lastobject);
                Destroy(lastobject);
                UnitCircleslength = UnitCircles.Count;
            }
            else
            {
                GameObject newcircle = new GameObject();
                newcircle.transform.parent = UnitCirclesHolder;
                newcircle.transform.localRotation = Quaternion.identity;
                newcircle.AddComponent<SpriteRenderer>();
                newcircle.GetComponent<SpriteRenderer>().sprite = CircleSprite;
                newcircle.layer = LayerMask.NameToLayer("Grid");



                UnitCircles.Add(newcircle);
                UnitCircleslength = UnitCircles.Count;
            }

            safeguard++;
        }
    }

    public void UpdateFilling()
    {

        InitializeUnitCircles();


        for (int i = 0; i < allunitGOsList.Count; i++)
        {
            GameObject unit = allunitGOsList[i];

            if (unit == null)
            {
                continue;
            }

            Character Char = unit.GetComponent<UnitScript>().UnitCharacteristics;

            Color newcolor = new Color();

            if (Char.affiliation == "playable")
            {
                if (Char.alreadyplayed)
                {
                    newcolor = Color.blue;
                }
                else
                {
                    newcolor = Color.cyan;

                }

            }
            else if (Char.affiliation == "enemy")
            {
                newcolor = Color.red;
            }
            else if (Char.affiliation == "other")
            {
                newcolor = Color.yellow;
            }
            newcolor.a = 0.5f;




            UnitCircles[i].GetComponent<SpriteRenderer>().color = newcolor;
            if (Char.enemyStats.monsterStats.size > 1)
            {
                UnitCircles[i].transform.localScale = Vector3.one * 2;
                UnitCircles[i].transform.position = new Vector3(Char.currentTile[0].GridCoordinates.x - 0.5f, YElevation + Char.currentTile[0].transform.position.y, Char.currentTile[0].GridCoordinates.y + 0.5f);



            }
            else
            {
                UnitCircles[i].transform.position = new Vector3(Char.currentTile[0].GridCoordinates.x, YElevation + Char.currentTile[0].transform.position.y, Char.currentTile[0].GridCoordinates.y);
                UnitCircles[i].transform.localScale = Vector3.one;
            }






        }


    }
}
