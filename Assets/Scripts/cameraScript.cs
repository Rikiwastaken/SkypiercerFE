using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class cameraScript : MonoBehaviour
{

    public static cameraScript instance;

    public Vector2 Destination;
    private GridSquareScript destTile;
    private GridSquareScript previousdestTile;

    private GridScript GridScript;

    public float camspeed;

    private InputManager InputManager;

    private Vector3 pointtolookat;

    public bool incombat;

    private Vector3 initialrotation;

    public float previouselevation;

    public GameObject fighter1;
    public GameObject fighter2;
    public CombatTextScript CombatTextScript;
    private bool resettingy;

    public GameObject PreBattleMenu;

    private float elevationtoadd;
    public float verticalautomovespeed;

    private TextBubbleScript TextBubbleScript;
    public TutorialWindowScript TutorialWindowScript;

    private float targetangle;

    private float currentVelocity = 0f;
    public float rotationSmoothTime = 0.3f;
    public float rotationSpeed = 45f;

    public float maxY = 20f;
    public float minY = 5f;

    public List<VolumeProfile> PostProcessingVolumeProfileList;

    public VolumeProfile DefaultVolumeProfile;
    private bool PostProcessingInitialized;

    private TurnManger turnmanager;

    public float previousY;

    private Vector2 prevdest;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        initialrotation = transform.GetChild(0).rotation.eulerAngles;
        previouselevation = transform.position.y;
        GridScript = GridScript.instance;
        TextBubbleScript = FindAnyObjectByType<TextBubbleScript>(FindObjectsInactive.Include);
        //Destination = GridScript.GetComponent<MapInitializer>().playablepos[0];
        //transform.position = new Vector3(Destination.x, transform.position.y, Destination.y);
        turnmanager = TurnManger.instance;
        previousY = transform.position.y;
    }

    void LateUpdate()
    {

        if (prevdest == Vector2.zero && Destination != Vector2.zero)
        {
            transform.position = new Vector3(Destination.x, transform.position.y, Destination.y);
            prevdest = Destination;
        }

        if (!PostProcessingInitialized)
        {
            ManagePostProcessing();
            PostProcessingInitialized = true;
        }
        if (Destination != Vector2.zero)
        {
            destTile = GridScript.GetTile(Destination);
        }
        if (destTile != null)
        {
            if (previousdestTile != null)
            {
                if (previousdestTile.elevation != destTile.elevation)
                {
                    elevationtoadd += destTile.elevation - previousdestTile.elevation;
                    previousdestTile = destTile;
                }
            }
            else
            {
                previousdestTile = destTile;
            }

        }

        if (Mathf.Abs(elevationtoadd) <= 0.1f)
        {
            elevationtoadd = 0;
        }

        if (elevationtoadd > 0)
        {
            transform.position += new Vector3(0f, verticalautomovespeed * Time.deltaTime, 0f);
            elevationtoadd -= verticalautomovespeed * Time.deltaTime;
        }
        else if (elevationtoadd < 0)
        {
            transform.position += new Vector3(0f, -verticalautomovespeed * Time.deltaTime, 0f);
            elevationtoadd += verticalautomovespeed * Time.deltaTime;
        }
        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), Destination) > 0.01f)
        {
            float movex = (Destination.x - transform.position.x) * camspeed * Time.deltaTime;
            float movez = (Destination.y - transform.position.z) * camspeed * Time.deltaTime;
            transform.position += new Vector3(movex, 0f, movez);
        }

        if (GridScript.actionsMenu.activeSelf || incombat || (PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) || TutorialWindowScript.gameObject.activeSelf || TextBubbleScript.indialogue || GridScript.NeutralMenu.activeSelf || GridScript.ForesightMenu.activeSelf)
        {
            return;
        }
        if (turnmanager.currentlyplaying == "playable" || turnmanager.currentlyplaying == "tutorial" || (PreBattleMenu.activeSelf && PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace))
        {
            Destination = new Vector2(GridScript.selection.transform.position.x, GridScript.selection.transform.position.z);
        }



        InputManager = InputManager.instance;

        // Zoom and rotation

        if (turnmanager.currentlyplaying == "playable" || turnmanager.currentlyplaying == "tutorial")
        {
            if (InputManager.movecamjustpressed)
            {
                if (InputManager.cammovementValue.x > 0)
                    targetangle -= rotationSpeed;
                else if (InputManager.cammovementValue.x < 0)
                    targetangle += rotationSpeed;

                // Keep target angle in [-180, 180)
                targetangle = Mathf.Repeat(targetangle + 180f, 360f) - 180f;

            }

            // Smooth rotation toward target
            float currentY = transform.eulerAngles.y;
            float newY = Mathf.SmoothDampAngle(currentY, targetangle, ref currentVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0f, newY, 0f);


            if (InputManager.cammovementValue.y != 0f)
            {
                previousY += InputManager.cammovementValue.y * -3f * Time.deltaTime;
                previousY = Mathf.Clamp(previousY, minY, maxY);
            }

            float speed = 6f;
            float newYPos = Mathf.MoveTowards(transform.position.y, previousY, speed * Time.deltaTime);

            transform.position = new Vector3(transform.position.x, newYPos, transform.position.z);

        }
        else if (turnmanager.currentlyplaying == "other" || turnmanager.currentlyplaying == "enemy")
        {
            if (transform.position.y <= maxY)
            {
                transform.position += new Vector3(0f, 3f * Time.deltaTime, 0f);
            }
        }



        initialrotation = transform.GetChild(0).rotation.eulerAngles;
    }

    public void ResetRotation()
    {
        if (!incombat)
        {
            transform.rotation = Quaternion.identity;
            transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            transform.position = new Vector3(transform.position.x, 13f, transform.position.z);
        }
    }

    public Vector2 GoToFightCamera(GameObject unit, GameObject target)
    {

        fighter1 = unit;
        fighter2 = target;
        CombatTextScript.SetupCombat(unit, target);
        incombat = true;
        /*
         
        Vector2 CoordUnit = unit.GetComponent<UnitScript>().UnitCharacteristics.position;
        Vector2 CoordTarget = target.GetComponent<UnitScript>().UnitCharacteristics.position;

        Vector2 Middle = new Vector2((CoordUnit.x + CoordTarget.x) / 2f, (CoordUnit.y + CoordTarget.y) / 2f);

        float n = Mathf.Sqrt((CoordTarget.y - CoordUnit.y) * (CoordTarget.y - CoordUnit.y) + (CoordTarget.x - CoordUnit.x) * (CoordTarget.x - CoordUnit.x));

        float length = Mathf.Max(Vector2.Distance(CoordTarget,CoordUnit)*2f,2.5f);

        Vector2 CamCoordinates = new Vector2();

        float side = -1f; //can also be -1
        
        CamCoordinates.x = Middle.x + side * length * ((float)CoordUnit.y- (float)CoordTarget.y)/n;
        CamCoordinates.y = Middle.y + side * length * ((float)CoordTarget.x - (float)CoordUnit.x) / n;

        float targetelevation = Mathf.Min(0.5f, Mathf.Max(0.5f, 1.5f - Vector2.Distance(CoordTarget, CoordUnit)));

        pointtolookat = new Vector3(Middle.x, targetelevation, Middle.y);
        
        
        return CamCoordinates;
        */
        return Destination;
    }


    public Vector2 DetermineDirection(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector2.zero;

        // Camera rotation in multiples of 45�
        float camY = Camera.main.transform.eulerAngles.y;
        int steps = Mathf.RoundToInt(camY / 45f); // number of 45� steps clockwise

        // Map discrete input (-1,0,1) to one of 8 directions
        int inputX = Mathf.RoundToInt(input.x);
        int inputY = Mathf.RoundToInt(input.y);

        // Encode input direction as a step index (0 = up, 1 = up-right, 2 = right, ..., 7 = up-left)
        int inputIndex = -1;
        if (inputX == 0 && inputY == 1) inputIndex = 0;
        else if (inputX == 1 && inputY == 1) inputIndex = 1;
        else if (inputX == 1 && inputY == 0) inputIndex = 2;
        else if (inputX == 1 && inputY == -1) inputIndex = 3;
        else if (inputX == 0 && inputY == -1) inputIndex = 4;
        else if (inputX == -1 && inputY == -1) inputIndex = 5;
        else if (inputX == -1 && inputY == 0) inputIndex = 6;
        else if (inputX == -1 && inputY == 1) inputIndex = 7;

        if (inputIndex == -1) return Vector2.zero; // invalid input

        // Rotate input by camera steps (modulo 8)
        int outputIndex = (inputIndex + steps) % 8;

        // Map index back to discrete Vector2
        Vector2[] directions = new Vector2[8] {
        new Vector2(0, 1),    // up
        new Vector2(1, 1),    // up-right
        new Vector2(1, 0),    // right
        new Vector2(1, -1),   // down-right
        new Vector2(0, -1),   // down
        new Vector2(-1, -1),  // down-left
        new Vector2(-1, 0),   // left
        new Vector2(-1, 1)    // up-left
    };

        return directions[outputIndex];
    }


    private void ManagePostProcessing()
    {
        int chapterID = MapInitializer.instance.ChapterID;
        if (chapterID < PostProcessingVolumeProfileList.Count && PostProcessingVolumeProfileList[chapterID] != null)
        {
            transform.GetChild(0).GetComponent<Volume>().profile = PostProcessingVolumeProfileList[chapterID];
        }
        else
        {
            transform.GetChild(0).GetComponent<Volume>().profile = DefaultVolumeProfile;
        }
    }

}
