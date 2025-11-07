using NUnit.Framework;
using System;
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

    private cameraScript cameraScript;

    public bool activated;

    public int RemainingRainTurns;

    public int RemainingSunTurns;

    public bool justbecamerain;

    public string VisualType;

    private ParticleSystem rainparticle;

    [Serializable]
    public class MechanismClass
    {
        public int type; // 1 : door, 2 : lever;
        public bool isactivated;
        public List<GridSquareScript> Triggers;
        public List<GridSquareScript> PairedTiles;
    }

    public MechanismClass Mechanism;

    [Serializable]
    public class MaterialsClass
    {
        public string name;
        public Material groundmaterial;
        public Material wallmaterial;
    }

    public List<MaterialsClass> MaterialsList;

    public Sprite gridsquareinsideWithoutUnit;
    public Sprite gridsquareinsideWithUnit;
    public Sprite gridsquareinsideWithUnitBigEnemies;

    private TextBubbleScript textBubbleScript;

    public Transform PathPiecePrevious;
    public Transform PathPieceNext;
    public Transform PathPieceEnd;

    public Sprite gridsquareFilling;
    public Sprite gridsquareFillingBigEnemies;

    private bool previouslyincombat;

    public GameObject LeverGO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        filledimage = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SelectRound = transform.GetChild(1).gameObject;
        SelectRoundFilling = transform.GetChild(2).gameObject;
        rainparticle = GetComponentInChildren<ParticleSystem>();
        InitializePosition();
        textBubbleScript = FindAnyObjectByType<TextBubbleScript>();
        if (rainparticle != null && rainparticle.gameObject.activeSelf)
        {
            rainparticle.gameObject.SetActive(false);
        }
        manageVisuals();
    }

    public void InitializePosition()
    {
        transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        GridCoordinates = new Vector2((int)transform.position.x, (int)transform.position.z);
        for (int i = 0; i < transform.childCount; i++)
        {
            initialpos.Add(transform.GetChild(i).localPosition);
        }
        ManageActivation();
    }

    private void Update()
    {

        if (Mechanism != null)
        {
            if (Mechanism.type == 1)
            {
                if (!Mechanism.isactivated)
                {
                    isobstacle = true;
                    bool alltriggersactive = true;
                    foreach (GridSquareScript triggertile in Mechanism.Triggers)
                    {
                        if (triggertile.Mechanism != null)
                        {
                            if (triggertile.Mechanism.type == 2 && !triggertile.Mechanism.isactivated)
                            {
                                alltriggersactive = false;
                                break;
                            }
                        }

                    }
                    if (alltriggersactive)
                    {
                        Mechanism.isactivated = true;
                    }
                }

                if (Mechanism.isactivated)
                {
                    isobstacle = false;
                }
            }
        }

        if (GridScript == null)
        {
            GridScript = GridScript.instance;
        }
        if (cameraScript == null)
        {
            cameraScript = FindAnyObjectByType<cameraScript>();
        }
        if (TurnManger == null)
        {
            TurnManger = FindAnyObjectByType<TurnManger>();
        }

        if (MapInitializer == null)
        {
            MapInitializer = FindAnyObjectByType<MapInitializer>();
        }

        if (cameraScript.incombat)
        {
            //SelectRoundFilling.GetComponent<SpriteRenderer>().color = new Color(0f,0f,0f,0f);
        }
        else
        {
            ManageActivation();
            if (!activated)
            {
                return;
            }

            ManagePath();

            if (RemainingRainTurns < 3)
            {
                justbecamerain = false;
            }

            if (RemainingRainTurns > 0)
            {
                if(!rainparticle.gameObject.activeSelf)
                {
                    rainparticle.gameObject.SetActive(true);
                }
                
            }
            else
            {
                if(rainparticle.gameObject.activeSelf)
                {
                    rainparticle.gameObject.SetActive(false);
                }
                
            }




            if (TurnManger.currentlyplaying == "")
            {
                if (MapInitializer.playablepos.Contains(GridCoordinates) && filledimage.color!= new Color(0.45f, 0f, 0.42f, 0.5f))
                {

                    filledimage.color = new Color(0.45f, 0f, 0.42f, 0.5f);
                }
            }


            if (GridScript.selection == this && !cameraScript.incombat)
            {
                if(!SelectRound.activeSelf)
                {
                    SelectRound.SetActive(true);
                }
                
                SelectRound.transform.rotation = Quaternion.Euler(SelectRound.transform.rotation.eulerAngles + new Vector3(0f, rotationperframe, 0f));
            }
            else
            {
                if (SelectRound.activeSelf)
                {
                    SelectRound.SetActive(false);
                }
                
            }
            
            if(InputManager.instance.movementjustpressed)
            {
                UpdateFilling();
            }
            


        }

        manageElevation();
        previouslyincombat = cameraScript.incombat;
    }

    public void ReinitializeMechanismIfPairednotactive()
    {
        if(Mechanism!=null && Mechanism.type==2 && Mechanism.PairedTiles!=null && Mechanism.PairedTiles.Count>0)
        {
            bool pairedallactive = true;
            foreach(GridSquareScript pairedtile in Mechanism.PairedTiles)
            {
                if (pairedtile.Mechanism != null && !pairedtile.Mechanism.isactivated)
                {
                    pairedallactive = false;
                    break;
                }
            }
            if(!pairedallactive)
            {
                Mechanism.isactivated = false;
            }
        }
    }

    public void ManageLeverOrientation()
    {
        if(LeverGO.activeSelf && Mechanism != null)
        {
            Vector3 previousrot = LeverGO.transform.GetChild(0).localRotation.eulerAngles;
            if(previousrot.y<100)
            {
                previousrot = new Vector3(previousrot.x, 135, previousrot.z);
            }
            else
            {
                previousrot = new Vector3(previousrot.x, 45, previousrot.z);
            }
            LeverGO.transform.GetChild(0).localRotation = Quaternion.Euler(previousrot);
        }
    }

    private void ManagePath()
    {
        List<GridSquareScript> path = ActionManager.instance.currentpath;
        if (path != null)
        {
            if (path.Contains(this))
            {
                int index = path.IndexOf(this);
                if (index > 0)
                {
                    if(!PathPiecePrevious.gameObject.activeSelf)
                    {
                        PathPiecePrevious.gameObject.SetActive(true);
                    }
                    
                    Vector3 direction = path[index - 1].transform.position - transform.position;
                    float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                    PathPiecePrevious.rotation = Quaternion.Euler(90f, -angle + 90f, 0f);
                    if (index == path.Count - 1)
                    {
                        if (!PathPieceEnd.gameObject.activeSelf)
                        {
                            PathPieceEnd.gameObject.SetActive(true);
                        }
                        
                        PathPieceEnd.rotation = Quaternion.Euler(90f, -angle + 90f, 0f);
                    }
                    else
                    {
                        if (!PathPieceEnd.gameObject.activeSelf)
                        {
                            PathPieceEnd.gameObject.SetActive(true);
                        }
                        
                    }
                }
                else
                {
                    if (PathPiecePrevious.gameObject.activeSelf)
                    {
                        PathPiecePrevious.gameObject.SetActive(false);
                    }
                    if (PathPieceEnd.gameObject.activeSelf)
                    {
                        PathPieceEnd.gameObject.SetActive(false);
                    }
                }
                if (index < path.Count - 1)
                {
                    if (!PathPieceNext.gameObject.activeSelf)
                    {
                        PathPieceNext.gameObject.SetActive(true);
                    }
                    Vector3 direction = path[index + 1].transform.position - transform.position;
                    float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                    PathPieceNext.rotation = Quaternion.Euler(90f, -angle + 90f, 0f);

                }
                else
                {
                    if (PathPieceNext.gameObject.activeSelf)
                    {
                        PathPieceNext.gameObject.SetActive(false);
                    }

                }
            }
            else
            {
                if (PathPiecePrevious.gameObject.activeSelf)
                {
                    PathPiecePrevious.gameObject.SetActive(false);
                }
                if (PathPieceEnd.gameObject.activeSelf)
                {
                    PathPieceEnd.gameObject.SetActive(false);
                }
                if (PathPieceNext.gameObject.activeSelf)
                {
                    PathPieceNext.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (PathPiecePrevious.gameObject.activeSelf)
            {
                PathPiecePrevious.gameObject.SetActive(false);
            }
            if (PathPieceEnd.gameObject.activeSelf)
            {
                PathPieceEnd.gameObject.SetActive(false);
            }
            if (PathPieceNext.gameObject.activeSelf)
            {
                PathPieceNext.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateInsideSprite(bool unitenter, Character unitchar = null)
    {
        if (unitchar != null)
        {
            if (unitchar.enemyStats.monsterStats.size > 1)
            {
                filledimage.transform.GetComponent<SpriteRenderer>().sprite = gridsquareinsideWithUnitBigEnemies;
                int index = unitchar.currentTile.IndexOf(this);
                Debug.Log(index);
                switch (index)
                {
                    case 0:

                        filledimage.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                        break;
                    case 1:
                        filledimage.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                        break;
                    case 2:
                        filledimage.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                        break;
                    case 3:
                        filledimage.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                        break;
                }
            }
        }
        else if(unitenter)
        {
            filledimage.transform.GetComponent<SpriteRenderer>().sprite = gridsquareinsideWithUnit;
        }
        else
        {
            filledimage.transform.GetComponent<SpriteRenderer>().sprite = gridsquareinsideWithoutUnit;
        }

        
    }

    private void manageVisuals()
    {
        foreach (MaterialsClass mat in MaterialsList)
        {
            if (mat.name.ToLower() == VisualType.ToLower())
            {
                if (isobstacle && mat.wallmaterial != null)
                {
                    Cube.GetComponent<Renderer>().material = mat.wallmaterial;
                }
                else
                {
                    Cube.GetComponent<Renderer>().material = mat.groundmaterial;
                }
            }
        }
    }
    private void manageElevation()
    {

        if(GridScript.instance.MapModel!=null)
        {
            if(Cube.GetComponent<MeshRenderer>().enabled)
            {
                Cube.GetComponent<MeshRenderer>().enabled = false;
                Stairs.GetComponent<MeshRenderer>().enabled = false;
            }
            if(Mechanism!=null && Mechanism.type==2 && LeverGO.GetComponent<MeshRenderer>().enabled != false)
            {
                LeverGO.GetComponent<MeshRenderer>().enabled = false;
            }
            
        }
        else
        {
            if (!Cube.GetComponent<MeshRenderer>().enabled)
            {
                Cube.GetComponent<MeshRenderer>().enabled = true;
                Stairs.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        //made on purpose so that it's never active
        if (cameraScript.incombat && !cameraScript.incombat)
        {
            float targetelevation = 0;
            if (isstairs)
            {
                if (Mathf.Abs(transform.position.y - targetelevation) <= 1.1)
                {
                    transform.position = new Vector3(transform.position.x, targetelevation - 1.1f, transform.position.z);
                }
                else if (transform.position.y > targetelevation - 1.1f)
                {
                    transform.position += new Vector3(0f, -elevationchange * ((float)elevation + 1f) * Time.fixedDeltaTime, 0f);
                }
                else if (transform.position.y < targetelevation - 1.1f)
                {
                    transform.position += new Vector3(0f, elevationchange * ((float)elevation + 1f) * Time.fixedDeltaTime, 0f);
                }
            }
            else
            {
                if (Mathf.Abs(transform.position.y - targetelevation) <= 0.05f)
                {
                    transform.position = new Vector3(transform.position.x, targetelevation, transform.position.z);
                }
                else if (transform.position.y < targetelevation)
                {
                    transform.position += new Vector3(0f, elevationchange * ((float)elevation + 1f) * Time.fixedDeltaTime, 0f);
                }
                else if (transform.position.y > targetelevation)
                {
                    transform.position += new Vector3(0f, -elevationchange * ((float)elevation + 1f) * Time.fixedDeltaTime, 0f);
                }
            }



        }
        else
        {

            //if(previouslyincombat)
            //{
            //    if (isstairs)
            //    {
            //        transform.position = new Vector3(transform.position.x, 0 - 1.1f, transform.position.z);
            //    }
            //    else
            //    {
            //        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            //    }
            //}

            if (isobstacle)
            {
                if (Mathf.Abs(transform.position.y - (elevation + walloffset)) <= 0.05f || GridScript.instance.MapModel!=null)
                {
                    transform.position = new Vector3(transform.position.x, elevation + walloffset, transform.position.z);
                }
                else if (transform.position.y < elevation + walloffset - 0.05f)
                {
                    transform.position += new Vector3(0f, elevationchange * (1f * (float)Mathf.Abs(elevation) + walloffset) * Time.fixedDeltaTime, 0f);
                }
                else if (transform.position.y > elevation + walloffset + 0.05f)
                {
                    transform.position += new Vector3(0f, -elevationchange * (1f * (float)Mathf.Abs(elevation) + walloffset) * Time.fixedDeltaTime, 0f);
                }
            }
            else
            {
                if (Mathf.Abs(transform.position.y - elevation) <= 0.05f || GridScript.instance.MapModel != null)
                {
                    transform.position = new Vector3(transform.position.x, elevation, transform.position.z);
                }
                else if (transform.position.y < elevation - 0.05f)
                {
                    transform.position += new Vector3(0f, elevationchange * (1f * (float)Mathf.Abs(elevation) + 1f) * Time.fixedDeltaTime, 0f);
                }
                else if (transform.position.y > elevation + 0.05f)
                {
                    transform.position += new Vector3(0f, -elevationchange * (1f * (float)Mathf.Abs(elevation) + 1f) * Time.fixedDeltaTime, 0f);
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
            if(!Stairs.activeSelf)
            {
                Stairs.SetActive(true);
            }
            
        }
        else
        {
            if (Stairs.activeSelf)
            {
                Stairs.SetActive(false);
            }
            
        }


    }

    private void ManageActivation()
    {
        if (activated && !GetComponent<SpriteRenderer>().enabled)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                if(!transform.GetChild(i).gameObject.activeSelf)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
                
            }
        }
        if (!activated && GetComponent<SpriteRenderer>().enabled)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                
            }
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

            if (Char.enemyStats.monsterStats.size > 1)
            {
                SR.sprite = gridsquareFillingBigEnemies;
                int index = Char.currentTile.IndexOf(this);
                switch (index)
                {
                    case 0:
                        SelectRoundFilling.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                        break;
                    case 1:
                        SelectRoundFilling.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                        break;
                    case 2:
                        SelectRoundFilling.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                        break;
                    case 3:
                        SelectRoundFilling.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                        break;
                }
            }
            else
            {
                SR.sprite = gridsquareFilling;
            }


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

        GameObject GO = GridScript.instance.GetUnit(this);

        if(GO != null)
        {
            UpdateInsideSprite(true,GO.GetComponent<UnitScript>().UnitCharacteristics);
        }

        Color newcolor = Color.red;
        newcolor.a = 0.5f;
        filledimage.color = newcolor;
    }

    public void CorrectColor()
    {
        Color newcolor = Color.blue;
        newcolor.a = 0.5f;
        if(filledimage.color == newcolor)
        {
            GameObject GO = GridScript.instance.GetUnit(this);
            if (GO != null && GO.GetComponent<UnitScript>().UnitCharacteristics.enemyStats.monsterStats.size>0)
            {
                foreach(GridSquareScript tile in GO.GetComponent<UnitScript>().UnitCharacteristics.currentTile)
                {
                    tile.filledimage.color = newcolor ;
                }
            }
        }
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
        if (filledimage != null)
        {
            Color newcolor = Color.gray;
            newcolor.a = 0f;
            filledimage.color = newcolor;
        }

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
        if (TurnManger.currentlyplaying != "" || !MapInitializer.playablepos.Contains(GridCoordinates) && filledimage != null)
        {
            filledimage.color = new Color(1f, 1f, 1f, 0f);
        }

    }
}
