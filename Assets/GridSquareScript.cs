using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnitScript;

public class GridSquareScript : MonoBehaviour
{

    private SpriteRenderer filledimage;

    public Vector2 GridCoordinates;

    public float rotationperframe;

    private GameObject SelectRound;

    private GameObject SelectRoundFilling;

    private GridScript GridScript;

    private MapInitializer MapInitializer;
    private TurnManger TurnManger;

    public bool isobstacle;

    public float elevationchange;

    public float walloffset;

    public int elevation;

    public GameObject Stairs;

    public GameObject Cube;
    private List<Vector3> initialpos = new List<Vector3>();

    /*type is for potential bonus : 
        Forest : +30% dodge
        Ruins : +10% Dodge, -10% Accuracy
        Fire : -1 movement, +10% Damage taken
        Water : -1 movement, -10% dodge
        HighGround : +20% Accuracy, +10% Dodge
        Fortification : +5% Dodge, +15% accuracy
        Fog : +20% Dodge, -20% accuracy
    */
    public string type;

    public bool isstairs;

    private battlecameraScript battlecameraScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        filledimage = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SelectRound = transform.GetChild(1).gameObject;
        SelectRoundFilling = transform.GetChild(2).gameObject;
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        GridCoordinates = new Vector2((int)transform.position.x, (int)transform.position.z);
        for (int i = 0; i < transform.childCount; i++)
        {
            initialpos.Add(transform.GetChild(i).localPosition);
        }
    }

    private void FixedUpdate()
    {
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }
        if (battlecameraScript == null)
        {
            battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        }
        if (TurnManger == null)
        {
            TurnManger = FindAnyObjectByType<TurnManger>();
        }

        if (MapInitializer == null)
        {
            MapInitializer = FindAnyObjectByType<MapInitializer>();
        }

        if (TurnManger.currentlyplaying == "")
        {
            if (MapInitializer.playablepos.Contains(GridCoordinates))
            {

                filledimage.color = new Color(0.45f, 0f, 0.42f, 0.5f);
            }
        }


        if (GridScript.selection == this && !battlecameraScript.incombat)
        {
            SelectRound.SetActive(true);
            SelectRound.transform.rotation = Quaternion.Euler(SelectRound.transform.rotation.eulerAngles + new Vector3(0f, rotationperframe, 0f));
        }
        else
        {
            SelectRound.SetActive(false);
        }

        UpdateFilling();
        manageElevation();
    }

    private void manageElevation()
    {

        if (battlecameraScript.incombat)
        {
            GridSquareScript squareFighter1 = battlecameraScript.fighter1.GetComponent<UnitScript>().UnitCharacteristics.currentTile;
            GridSquareScript squareFighter2 = battlecameraScript.fighter2.GetComponent<UnitScript>().UnitCharacteristics.currentTile;


            if(isstairs)
            {
                if (transform.position.y > Mathf.Min(squareFighter1.elevation, squareFighter2.elevation) - 1.1f)
                {
                    transform.position += new Vector3(0f, -elevationchange * ((float)elevation + 1f) * Time.fixedDeltaTime, 0f);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, (float)Mathf.Min(squareFighter1.elevation, squareFighter2.elevation)-1.1f, transform.position.z);
                }
            }
            else
            {
                if (transform.position.y > Mathf.Min(squareFighter1.elevation, squareFighter2.elevation))
                {
                    transform.position += new Vector3(0f, -elevationchange * ((float)elevation + 1f) * Time.fixedDeltaTime, 0f);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, (float)Mathf.Min(squareFighter1.elevation, squareFighter2.elevation), transform.position.z);
                }
            }

            

        }
        else
        {
            if (isobstacle)
            {
                if (transform.position.y < elevation + walloffset - 0.05f)
                {
                    transform.position += new Vector3(0f, elevationchange * (1f * (float)elevation + walloffset) * Time.fixedDeltaTime, 0f);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, elevation + walloffset, transform.position.z);
                }
            }
            else
            {
                if (transform.position.y < elevation - 0.05f)
                {
                    transform.position += new Vector3(0f, elevationchange * (1f * (float)elevation + 1f) * Time.fixedDeltaTime, 0f);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, elevation, transform.position.z);
                }
            }
        }

        if (isstairs)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject != Cube)
                {
                    transform.GetChild(i).localPosition = initialpos[i] + new Vector3(0f, 0f, 1f);
                }
            }
            Stairs.SetActive(true);
        }
        else
        {
            Stairs.SetActive(false);
        }


    }

    public void UpdateFilling()
    {
        SpriteRenderer SR = SelectRoundFilling.GetComponent<SpriteRenderer>();
        GameObject unit = GridScript.GetUnit(this);
        Color newcolor = new Color();
        if (unit == null)
        {
            newcolor.a = 0;
        }
        else
        {
            Character Char = unit.GetComponent<UnitScript>().UnitCharacteristics;
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
        }
        SR.color = newcolor;
    }

    public void fillwithblue()
    {
        Color newcolor = Color.blue;
        newcolor.a = 0.5f;
        filledimage.color = newcolor;
    }

    public void fillwithRed()
    {
        Color newcolor = Color.red;
        newcolor.a = 0.5f;
        filledimage.color = newcolor;
    }

    public void fillwithGreen()
    {
        Color newcolor = Color.green;
        newcolor.a = 0.5f;
        filledimage.color = newcolor;
    }

    public void fillwithPurple()
    {
        Color newcolor = Color.magenta;
        newcolor.a = 0.5f;
        filledimage.color = newcolor;
    }
    public void fillwithGrey()
    {
        Color newcolor = Color.gray;
        newcolor.a = 0.5f;
        filledimage.color = newcolor;
    }

    public void fillwithNothing()
    {
        if (TurnManger == null)
        {
            TurnManger = FindAnyObjectByType<TurnManger>();
        }
        if (MapInitializer == null)
        {
            MapInitializer = FindAnyObjectByType<MapInitializer>();
        }
        if (TurnManger.currentlyplaying != "" || !MapInitializer.playablepos.Contains(GridCoordinates))
        {
            filledimage.color = new Color(1f, 1f, 1f, 0f);
        }

    }
}
