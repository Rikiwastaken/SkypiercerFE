using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class cameraScriptV2 : MonoBehaviour
{

    public static cameraScriptV2 instance;

    [Header("Cinemachine Varibles")]

    // each Cinemachine Camera governs one of the 4 possible position
    public CinemachineCamera FarTopCamera;
    public CinemachineCamera MiddleTopCamera;
    public CinemachineCamera CloseTopCamera;
    public CinemachineCamera DownCamera;
    private List<CinemachineCamera> Cameras;

    public GameObject PreBattleMenu;

    public CinemachineBrain MainCamera;
    public CinemachineBrain HUDCamera;

    private CinemachineCamera currentcam;

    private InputAction _CameraAction;

    public Vector2 Destination;
    private GridSquareScript destTile;

    private TurnManger _TurnManger;
    private GridScript _GridScript;

    [Header("Cam Movement Varibles")]
    public float RotationPerInput;
    private float TargetAngle;
    private float currentTurnVelocity;
    public float rotationSmoothTime = 0.3f;
    public Vector3 BaseOffset;
    private float ElevationYadjust = 0f;
    public float movementSmoothTime;
    private Vector3 currentMoveVelocity;
    private Vector3 MovementTarget;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        Cameras = new List<CinemachineCamera>() { FarTopCamera, MiddleTopCamera, CloseTopCamera, DownCamera };
        _CameraAction = InputSystem.actions.FindAction("MoveCam");
        _TurnManger = TurnManger.instance;
        _GridScript = GridScript.instance;
        currentcam = GetCamera();
        Destination = _GridScript.GetComponent<MapInitializer>().playablepos[0];
    }

    private void Update()
    {

        if ((_TurnManger.currentlyplaying == "playable" || _TurnManger.currentlyplaying == "tutorial" || (PreBattleMenu.activeSelf && PreBattleMenu.GetComponent<PreBattleMenuScript>().ChangingUnitPlace)) && !ActionsMenu.instance.gameObject.activeSelf)
        {
            Destination = new Vector2(_GridScript.selection.transform.position.x, _GridScript.selection.transform.position.z);
        }

        ManageCamMovement();
        ApplySmoothMovement();
    }

    private void ManageCamMovement()
    {

        if (Destination != null)
        {
            destTile = _GridScript.GetTile(Destination);
        }

        if (destTile != null)
        {
            ElevationYadjust = destTile.elevation;
        }

        MovementTarget = BaseOffset + new Vector3(Destination.x, ElevationYadjust, Destination.y);

        if (_TurnManger.currentlyplaying == "playable" || _TurnManger.currentlyplaying == "tutorial")
        {

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected != null && currentSelected.activeInHierarchy)
            {
                return;
            }

            if (!_CameraAction.enabled)
            {
                _CameraAction.Enable();
            }

            if (_CameraAction.WasPerformedThisFrame())
            {
                // Rotation
                Vector2 cammovement = _CameraAction.ReadValue<Vector2>();
                if (cammovement.x > 0)
                {
                    TargetAngle -= RotationPerInput;
                }
                else if (cammovement.x < 0)
                {
                    TargetAngle += RotationPerInput;
                }

                // Keep target angle in [-180, 180)
                TargetAngle = Mathf.Repeat(TargetAngle + 180f, 360f) - 180f;


                //HeightChange
                if (cammovement.y < 0)
                {
                    if (Cameras.IndexOf(currentcam) < Cameras.Count - 1)
                    {
                        SetCamera(Cameras[Cameras.IndexOf(currentcam) + 1]);
                    }
                }
                else if (cammovement.y > 0)
                {
                    if (Cameras.IndexOf(currentcam) > 0)
                    {
                        SetCamera(Cameras[Cameras.IndexOf(currentcam) - 1]);
                    }
                }
            }

        }
        else if (_TurnManger.currentlyplaying == "other" || _TurnManger.currentlyplaying == "enemy")
        {
            if (currentcam != FarTopCamera)
            {
                SetCamera(FarTopCamera);
            }
        }
    }

    private void ApplySmoothMovement()
    {
        // Smooth rotation toward target
        float currentYRot = transform.eulerAngles.y;
        float newYRot = Mathf.SmoothDampAngle(currentYRot, TargetAngle, ref currentTurnVelocity, rotationSmoothTime);

        transform.rotation = Quaternion.Euler(0f, newYRot, 0f);

        // Smooth Movement Towards target

        Vector3 currentPosition = transform.position;

        Vector3 newposition = Vector3.SmoothDamp(currentPosition, MovementTarget, ref currentMoveVelocity, movementSmoothTime);

        transform.position = newposition;
    }

    private void SetCamera(CinemachineCamera newcamera)
    {

        foreach (CinemachineCamera cam in Cameras)
        {
            if (cam == newcamera)
            {
                if (!cam.enabled)
                {
                    cam.enabled = true;
                }

                currentcam = cam;
            }
            else
            {
                if (cam.enabled)
                {
                    cam.enabled = false;
                }

            }
        }
    }

    private CinemachineCamera GetCamera()
    {
        CinemachineCamera camtoreturn = null;
        foreach (CinemachineCamera cam in Cameras)
        {
            if (camtoreturn == null)
            {
                if (cam.enabled)
                {
                    camtoreturn = cam;
                }
            }
            else
            {
                if (cam.enabled)
                {
                    cam.enabled = false;
                }
            }

        }
        return camtoreturn;
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
        SetCamera(Cameras[1]);
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
}
