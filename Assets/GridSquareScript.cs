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

    /*type is for potential bonus : 
        Forest : +30% dodge
        Ruins : +10% Dodge, -10% Accuracy
        Fire : -1 movement, +10% Damage taken
        Water : -1 movement, -10% dodge
        HighGround : +20% Accuracy, +10% Dodge
    */
    public string type;

    private battlecameraScript battlecameraScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        filledimage = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SelectRound = transform.GetChild(1).gameObject;
        SelectRoundFilling = transform.GetChild(2).gameObject;
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        GridCoordinates = new Vector2((int)transform.position.x, (int)transform.position.z);

    }

    private void FixedUpdate()
    {
        if(GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }
        if (battlecameraScript == null)
        {
            battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        }
        if(TurnManger == null)
        {
            TurnManger = FindAnyObjectByType<TurnManger>();
        }

        if(MapInitializer == null)
        {
            MapInitializer = FindAnyObjectByType<MapInitializer>();
        }

        if(TurnManger.currentlyplaying=="")
        {
            if(MapInitializer.playablepos.Contains(GridCoordinates))
            {

                filledimage.color =new Color(0.45f,0f,0.42f,0.5f) ;
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
    }

    public void UpdateFilling()
    {
        SpriteRenderer SR = SelectRoundFilling.GetComponent<SpriteRenderer>();
        GameObject unit = GridScript.GetUnit(this);
        Color newcolor = new Color();
        if(unit == null)
        {
            newcolor.a = 0;
        }
        else
        {
            Character Char = unit.GetComponent<UnitScript>().UnitCharacteristics;
            if(Char.affiliation == "playable")
            {
                newcolor = Color.blue;
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
        filledimage.color= newcolor;
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
