using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [Header("Sticks")]
    public Vector2 movementValue;
    public Vector2 cammovementValue;
    public bool movementjustpressed;
    private bool movementpressedonce;
    public bool movecamjustpressed;
    private bool movecampressedonce;
    public InputActionReference move;
    public InputActionReference movecam;
    private InputAction moveinput;
    private InputAction movecaminput;

    [Header("Validate Button")]
    public bool activatepressed;
    public bool activatejustpressed;
    public InputActionReference activate;
    private InputAction activateinput;
    private bool activatepressedonce;

    [Header("Cancel Button")]
    public bool cancelpressed;
    public bool canceljustpressed;
    public InputActionReference cancel;
    private InputAction cancelinput;
    private bool cancelpressedonce;

    [Header("NextWeapon Button")]
    public bool NextWeaponpressed;
    public bool NextWeaponjustpressed;
    public InputActionReference NextWeapon;
    private InputAction NextWeaponinput;
    private bool NextWeaponpressedonce;

    [Header("PreviousWeapon Button")]
    public bool PreviousWeaponpressed;
    public bool PreviousWeaponjustpressed;
    public InputActionReference PreviousWeapon;
    private InputAction PreviousWeaponinput;
    private bool PreviousWeaponpressedonce;

    [Header("NextTarget Button")]
    public bool NextTargetpressed;
    public bool NextTargetjustpressed;
    public InputActionReference NextTarget;
    private InputAction NextTargetinput;
    private bool NextTargetpressedonce;

    [Header("PreviousTarget Button")]
    public bool PreviousTargetpressed;
    public bool PreviousTargetjustpressed;
    public InputActionReference PreviousTarget;
    private InputAction PreviousTargetinput;
    private bool PreviousTargetpressedonce;

    [Header("Telekinesis Button")]
    public bool Telekinesispressed;
    public bool Telekinesisjustpressed;
    public InputActionReference Telekinesis;
    private InputAction Telekinesisinput;
    private bool Telekinesispressedonce;

    [Header("Select Button")]
    public bool Selectpressed;
    public bool Selectjustpressed;
    public InputActionReference Select;
    private InputAction Selectinput;
    private bool Selectpressedonce;

    [Header("Start Button")]
    public bool Startpressed;
    public bool Startjustpressed;
    public InputActionReference Startbtn;
    private InputAction Startinput;
    private bool Startpressedonce;

    [Header("Show Details")]
    public bool ShowDetailspressed;
    public bool ShowDetailsjustpressed;
    public InputActionReference ShowDetailsbtn;
    private InputAction ShowDetailsinput;
    private bool ShowDetailspressedonce;

    // ---------- EVENTS ----------
    public event Action OnMovementJustPressed;
    public event Action OnMoveCamJustPressed;
    public event Action OnActivateJustPressed;
    public event Action OnCancelJustPressed;
    public event Action OnNextWeaponJustPressed;
    public event Action OnPreviousWeaponJustPressed;
    public event Action OnNextTargetJustPressed;
    public event Action OnPreviousTargetJustPressed;
    public event Action OnTelekinesisJustPressed;
    public event Action OnSelectJustPressed;
    public event Action OnStartJustPressed;
    public event Action OnShowDetailsJustPressed;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // Setup of the inputs
        moveinput = move.ToInputAction();
        movecaminput = movecam.ToInputAction();
        moveinput.canceled += context => movementValue = Vector2.zero;
        movecaminput.canceled += context => cammovementValue = Vector2.zero;

        activateinput = activate.ToInputAction();
        cancelinput = cancel.ToInputAction();
        NextWeaponinput = NextWeapon.ToInputAction();
        PreviousWeaponinput = PreviousWeapon.ToInputAction();
        NextTargetinput = NextTarget.ToInputAction();
        PreviousTargetinput = PreviousTarget.ToInputAction();
        Telekinesisinput = Telekinesis.ToInputAction();
        Selectinput = Select.ToInputAction();
        Startinput = Startbtn.ToInputAction();
        ShowDetailsinput = ShowDetailsbtn.ToInputAction();
    }

    private void FixedUpdate()
    {
        // ---------- ACTIVATE ----------
        activatepressed = activateinput.IsPressed();
        if (activatepressed && !activatejustpressed && !activatepressedonce)
        {
            activatejustpressed = true;
            activatepressedonce = true;
            OnActivateJustPressed?.Invoke();
        }
        else
        {
            activatejustpressed = false;
        }

        if (!activatepressed)
            activatepressedonce = false;

        if (cameraScript.instance != null && cameraScript.instance.incombat)
        {
            activatepressed = false;
            activatejustpressed = false;
            activatepressedonce = false;
        }

        // ---------- CANCEL ----------
        cancelpressed = cancelinput.IsPressed();
        if (cancelpressed && !canceljustpressed && !cancelpressedonce)
        {
            canceljustpressed = true;
            cancelpressedonce = true;
            OnCancelJustPressed?.Invoke();
        }
        else
            canceljustpressed = false;

        if (!cancelpressed)
            cancelpressedonce = false;

        // ---------- NEXT WEAPON ----------
        NextWeaponpressed = NextWeaponinput.IsPressed();
        if (NextWeaponpressed && !NextWeaponjustpressed && !NextWeaponpressedonce)
        {
            NextWeaponjustpressed = true;
            NextWeaponpressedonce = true;
            OnNextWeaponJustPressed?.Invoke();
        }
        else
            NextWeaponjustpressed = false;

        if (!NextWeaponpressed)
            NextWeaponpressedonce = false;

        // ---------- PREVIOUS WEAPON ----------
        PreviousWeaponpressed = PreviousWeaponinput.IsPressed();
        if (PreviousWeaponpressed && !PreviousWeaponjustpressed && !PreviousWeaponpressedonce)
        {
            PreviousWeaponjustpressed = true;
            PreviousWeaponpressedonce = true;
            OnPreviousWeaponJustPressed?.Invoke();
        }
        else
            PreviousWeaponjustpressed = false;

        if (!PreviousWeaponpressed)
            PreviousWeaponpressedonce = false;

        // ---------- NEXT TARGET ----------
        NextTargetpressed = NextTargetinput.IsPressed();
        if (NextTargetpressed && !NextTargetjustpressed && !NextTargetpressedonce)
        {
            NextTargetjustpressed = true;
            NextTargetpressedonce = true;
            OnNextTargetJustPressed?.Invoke();
        }
        else
            NextTargetjustpressed = false;

        if (!NextTargetpressed)
            NextTargetpressedonce = false;

        // ---------- PREVIOUS TARGET ----------
        PreviousTargetpressed = PreviousTargetinput.IsPressed();
        if (PreviousTargetpressed && !PreviousTargetjustpressed && !PreviousTargetpressedonce)
        {
            PreviousTargetjustpressed = true;
            PreviousTargetpressedonce = true;
            OnPreviousTargetJustPressed?.Invoke();
        }
        else
            PreviousTargetjustpressed = false;

        if (!PreviousTargetpressed)
            PreviousTargetpressedonce = false;

        // ---------- TELEKINESIS ----------
        Telekinesispressed = Telekinesisinput.IsPressed();
        if (Telekinesispressed && !Telekinesisjustpressed && !Telekinesispressedonce)
        {
            Telekinesisjustpressed = true;
            Telekinesispressedonce = true;
            OnTelekinesisJustPressed?.Invoke();
        }
        else
            Telekinesisjustpressed = false;

        if (!Telekinesispressed)
            Telekinesispressedonce = false;

        // ---------- SELECT ----------
        Selectpressed = Selectinput.IsPressed();
        if (Selectpressed && !Selectjustpressed && !Selectpressedonce)
        {
            Selectjustpressed = true;
            Selectpressedonce = true;
            OnSelectJustPressed?.Invoke();
        }
        else
            Selectjustpressed = false;

        if (!Selectpressed)
            Selectpressedonce = false;

        // ---------- START ----------
        Startpressed = Startinput.IsPressed();
        if (Startpressed && !Startjustpressed && !Startpressedonce)
        {
            Startjustpressed = true;
            Startpressedonce = true;
            OnStartJustPressed?.Invoke();
        }
        else
            Startjustpressed = false;

        if (!Startpressed)
            Startpressedonce = false;

        // ---------- SHOW DETAILS ----------
        ShowDetailspressed = ShowDetailsinput.IsPressed();
        if (ShowDetailspressed && !ShowDetailsjustpressed && !ShowDetailspressedonce)
        {
            ShowDetailsjustpressed = true;
            ShowDetailspressedonce = true;
            OnShowDetailsJustPressed?.Invoke();
        }
        else
            ShowDetailsjustpressed = false;

        if (!ShowDetailspressed)
            ShowDetailspressedonce = false;

        // ---------- MOVEMENT ----------
        if (movementValue != Vector2.zero & !movementpressedonce)
        {
            movementpressedonce = true;
            movementjustpressed = true;
            OnMovementJustPressed?.Invoke();
        }
        else
            movementjustpressed = false;

        if (movementValue == Vector2.zero)
        {
            movementpressedonce = false;
            movementjustpressed = false;
        }

        // ---------- CAMERA MOVEMENT ----------
        if (cammovementValue != Vector2.zero & !movecampressedonce)
        {
            movecampressedonce = true;
            movecamjustpressed = true;
            OnMoveCamJustPressed?.Invoke();
        }
        else
            movecamjustpressed = false;

        if (cammovementValue == Vector2.zero)
        {
            movecampressedonce = false;
            movecamjustpressed = false;
        }
    }

    void OnMovement(InputValue value)
    {
        movementValue = value.Get<Vector2>();
        movementValue.x = Mathf.Abs(movementValue.x) >= 0.5f ? Mathf.Sign(movementValue.x) : 0f;
        movementValue.y = Mathf.Abs(movementValue.y) >= 0.5f ? Mathf.Sign(movementValue.y) : 0f;
    }

    void OnResetcam(InputValue value)
    {
        cameraScript cameraScript = FindAnyObjectByType<cameraScript>();
        cameraScript?.ResetRotation();
    }

    void OnMoveCam(InputValue value)
    {
        cammovementValue = value.Get<Vector2>();
        cammovementValue.x = Mathf.Abs(cammovementValue.x) >= 0.5f ? Mathf.Sign(cammovementValue.x) : 0f;
        cammovementValue.y = Mathf.Abs(cammovementValue.y) >= 0.5f ? Mathf.Sign(cammovementValue.y) : 0f;
    }
}
