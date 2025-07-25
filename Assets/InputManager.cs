using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Sticks")]
    public Vector2 movementValue;
    public Vector2 cammovementValue;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //setup of the inputs
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
    }

    private void FixedUpdate()
    {
        activatepressed = activateinput.IsPressed();
        if(activatepressed && ! activatejustpressed && !activatepressedonce)
        {
            activatejustpressed = true;
            activatepressedonce = true;
        }
        else
        {
            activatejustpressed = false;
        }

        if(!activatepressed)
        {
            activatepressedonce = false;
        }

        cancelpressed = cancelinput.IsPressed();
        if (cancelpressed && !canceljustpressed && !cancelpressedonce)
        {
            canceljustpressed = true;
            cancelpressedonce = true;
        }
        else
        {
            canceljustpressed = false;
        }

        if (!cancelpressed)
        {
            cancelpressedonce = false;
        }

        NextWeaponpressed = NextWeaponinput.IsPressed();
        if (NextWeaponpressed && !NextWeaponjustpressed && !NextWeaponpressedonce)
        {
            NextWeaponjustpressed = true;
            NextWeaponpressedonce = true;
        }
        else
        {
            NextWeaponjustpressed = false;
        }

        if (!NextWeaponpressed)
        {
            NextWeaponpressedonce = false;
        }

        PreviousWeaponpressed = PreviousWeaponinput.IsPressed();
        if (PreviousWeaponpressed && !PreviousWeaponjustpressed && !PreviousWeaponpressedonce)
        {
            PreviousWeaponjustpressed = true;
            PreviousWeaponpressedonce = true;
        }
        else
        {
            PreviousWeaponjustpressed = false;
        }

        if (!PreviousWeaponpressed)
        {
            PreviousWeaponpressedonce = false;
        }

        NextTargetpressed = NextTargetinput.IsPressed();
        if (NextTargetpressed && !NextTargetjustpressed && !NextTargetpressedonce)
        {
            NextTargetjustpressed = true;
            NextTargetpressedonce = true;
        }
        else
        {
            NextTargetjustpressed = false;
        }

        if (!NextTargetpressed)
        {
            NextTargetpressedonce = false;
        }

        PreviousTargetpressed = PreviousTargetinput.IsPressed();
        if (PreviousTargetpressed && !PreviousTargetjustpressed && !PreviousTargetpressedonce)
        {
            PreviousTargetjustpressed = true;
            PreviousTargetpressedonce = true;
        }
        else
        {
            PreviousTargetjustpressed = false;
        }

        if (!PreviousTargetpressed)
        {
            PreviousTargetpressedonce = false;
        }

        Telekinesispressed = Telekinesisinput.IsPressed();
        if (Telekinesispressed && !Telekinesisjustpressed && !Telekinesispressedonce)
        {
            Telekinesisjustpressed = true;
            Telekinesispressedonce = true;
        }
        else
        {
            Telekinesisjustpressed = false;
        }

        if (!Telekinesispressed)
        {
            Telekinesispressedonce = false;
        }

        Selectpressed = Selectinput.IsPressed();
        if (Selectpressed && !Selectjustpressed && !Selectpressedonce)
        {
            Selectjustpressed = true;
            Selectpressedonce = true;
        }
        else
        {
            Selectjustpressed = false;
        }

        if (!Selectpressed)
        {
            Selectpressedonce = false;
        }

    }

    void OnMovement(InputValue value)
    {
        movementValue = value.Get<Vector2>();
        if (movementValue.x >= 0.5f)
        {
            movementValue.x = 1f;
        }
        else if (movementValue.x <= -0.5f)
        {
            movementValue.x = -1f;
        }
        else
        {
            movementValue.x = 0f;
        }
        if (movementValue.y >= 0.5f)
        {
            movementValue.y = 1f;
        }
        else if (movementValue.y <= -0.5f)
        {
            movementValue.y = -1f;
        }
        else
        {
            movementValue.y = 0f;
        }

    }

    void OnResetcam(InputValue value)
    {
        battlecameraScript battlecameraScript = FindAnyObjectByType<battlecameraScript>();
        if (battlecameraScript != null)
        {
            battlecameraScript.ResetRotation();
        }
    }

    void OnMoveCam(InputValue value)
    {
        cammovementValue = value.Get<Vector2>();
        if (cammovementValue.x >= 0.5f)
        {
            cammovementValue.x = 1f;
        }
        else if (cammovementValue.x <= -0.5f)
        {
            cammovementValue.x = -1f;
        }
        else
        {
            cammovementValue.x = 0f;
        }
        if (cammovementValue.y >= 0.5f)
        {
            cammovementValue.y = 1f;
        }
        else if (cammovementValue.y <= -0.5f)
        {
            cammovementValue.y = -1f;
        }
        else
        {
            cammovementValue.y = 0f;
        }
    }
}
