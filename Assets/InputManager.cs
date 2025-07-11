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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //setup of the inputs
        moveinput = move.ToInputAction();
        movecaminput = movecam.ToInputAction();
        moveinput.canceled += context => movementValue = Vector2.zero;
        movecaminput.canceled += context => cammovementValue = Vector2.zero;

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
