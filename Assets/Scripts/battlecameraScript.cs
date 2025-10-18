using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class battlecameraScript : MonoBehaviour
{

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

    private float targetangle;

    private float currentVelocity = 0f;
    public float rotationSmoothTime = 0.3f;
    public float rotationSpeed = 45f;

    private void Start()
    {
        initialrotation = transform.GetChild(0).rotation.eulerAngles;
        previouselevation = transform.position.y;
        GridScript = FindAnyObjectByType<GridScript>();
        TextBubbleScript = FindAnyObjectByType<TextBubbleScript>();
        Destination = GridScript.GetComponent<MapInitializer>().playablepos[0];
        transform.position = new Vector3(Destination.x, transform.position.y, Destination.y);
    }

    void FixedUpdate()
    {
        if(Destination!=null)
        {
            destTile = GridScript.GetTile(Destination);
        }
        if(destTile!=null)
        {
            if(previousdestTile!=null)
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
        
        if(Mathf.Abs(elevationtoadd)<=0.1f)
        {
            elevationtoadd = 0;
        }

        if(elevationtoadd>0)
        {
            transform.position += new Vector3(0f, verticalautomovespeed * Time.deltaTime, 0f);
            elevationtoadd -= verticalautomovespeed * Time.deltaTime;
        }
        else if(elevationtoadd < 0)
        {
            transform.position += new Vector3(0f, -verticalautomovespeed * Time.deltaTime, 0f);
            elevationtoadd += verticalautomovespeed * Time.deltaTime;
        }

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), Destination) > 0.01f)
        {
            float movex = (Destination.x - transform.position.x) * camspeed * Time.fixedDeltaTime;
            float movez = (Destination.y - transform.position.z) * camspeed * Time.fixedDeltaTime;
            transform.position += new Vector3(movex, 0f, movez);
        }

        if(incombat)
        {
            Vector2 CoordUnit = fighter1.GetComponent<UnitScript>().UnitCharacteristics.position;
            Vector2 CoordTarget = fighter2.GetComponent<UnitScript>().UnitCharacteristics.position;
            transform.GetChild(0).LookAt(pointtolookat);
            float targetelevation = Mathf.Min(0.5f, Mathf.Max(0.5f,1.5f- Vector2.Distance(CoordTarget, CoordUnit)));
            float movey = (targetelevation - transform.position.y) * camspeed * Time.fixedDeltaTime;
            if(transform.position.y> targetelevation)
            {
                transform.position += new Vector3(0f, movey, 0f);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, targetelevation, transform.position.z);
            }
            resettingy = true;
        }
        else if(resettingy)
        {
            float movey = (previouselevation - transform.position.y) * camspeed * Time.fixedDeltaTime;
            transform.position += new Vector3(0f, movey, 0f);
            fighter1 = null;
            fighter2 = null;
            transform.GetChild(0).rotation = Quaternion.Euler(initialrotation);
            if (Mathf.Abs(previouselevation - transform.position.y) < 0.1f)
            {
                resettingy = false;
            }
        }
        else
        {
            previouselevation = transform.position.y;
        }

        if (GridScript.actionsMenu.activeSelf || incombat || (PreBattleMenu.activeSelf && !PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace) || TextBubbleScript.indialogue ||GridScript.NeutralMenu.activeSelf || GridScript.ForesightMenu.activeSelf)
        {
            return;
        }
        Destination = new Vector2(GridScript.selection.transform.position.x, GridScript.selection.transform.position.z);

        if (InputManager == null)
        {
            InputManager = FindAnyObjectByType<InputManager>();
        }

        if (InputManager.movecamjustpressed)
        {
            if (InputManager.cammovementValue.x > 0)
                targetangle -= rotationSpeed;
            else if (InputManager.cammovementValue.x < 0)
                targetangle += rotationSpeed;

            // Keep target angle in [-180, 180)
            targetangle = Mathf.Repeat(targetangle + 180f, 360f) - 180f;

            Debug.Log($"Input: {InputManager.cammovementValue}, Target: {targetangle}");
        }

        // Smooth rotation toward target
        float currentY = transform.eulerAngles.y;
        float newY = Mathf.SmoothDampAngle(currentY, targetangle, ref currentVelocity, rotationSmoothTime);

        transform.rotation = Quaternion.Euler(0f, newY, 0f);

        if (InputManager.cammovementValue.y != 0f)
        {
            transform.position += new Vector3(0f, InputManager.cammovementValue.y * -3f * Time.fixedDeltaTime, 0f);
        }
        if(transform.position.y<5f)
        {
            transform.position = new Vector3(transform.position.x,5f,transform.position.z);
        }
        if (transform.position.y > 20f)
        {
            transform.position = new Vector3(transform.position.x, 20f, transform.position.z);
        }
        
        initialrotation = transform.GetChild(0).rotation.eulerAngles;
    }

    public void ResetRotation()
    {
        if(!incombat)
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
        incombat = true;
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
        CombatTextScript.SetupCombat(unit, target);
        return CamCoordinates;
    }


    public Vector2 DetermineDirection(Vector2 input)
    {
        if (input == Vector2.zero)
            return Vector2.zero;

        // Camera rotation in multiples of 45°
        float camY = Camera.main.transform.eulerAngles.y;
        int steps = Mathf.RoundToInt(camY / 45f); // number of 45° steps clockwise

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


}
