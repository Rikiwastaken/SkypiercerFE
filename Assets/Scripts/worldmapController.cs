using UnityEngine;
using UnityEngine.InputSystem;

public class worldmapController : MonoBehaviour
{
    public static worldmapController instance;

    private Camera cam;

    public Transform CamHolder;

    public float cammovepersec;

    private CharacterController CC;

    public float speed;

    public Transform playermodel;

    private Vector3 forwardtarget = new Vector3(0, 0, 1);

    private Vector3 targetcamrotation;

    public bool isshipping;
    public float waterwheelrotationpersecond;
    public Transform Waterwheel;

    public GameObject ShipModel;
    public GameObject HumanModel;

    public float gravValue;

    private InputAction _MoveAction;
    private InputAction _MoveCamAction;
    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        CC = GetComponent<CharacterController>();
        playermodel.GetComponent<Animator>().SetBool("WorldMap", true);
        _MoveAction = InputSystem.actions.FindAction("Movement");
        _MoveCamAction = InputSystem.actions.FindAction("MoveCam");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 finalmovement = Vector3.zero;
        if (isshipping)
        {
            if (!ShipModel.activeSelf)
            {
                HumanModel.SetActive(false);
                ShipModel.SetActive(true);
                playermodel = ShipModel.transform;
            }

            Waterwheel.Rotate(waterwheelrotationpersecond * Time.deltaTime * CC.velocity.magnitude / speed, 0f, 0f);

        }
        else
        {
            if (ShipModel.activeSelf)
            {
                HumanModel.SetActive(true);
                ShipModel.SetActive(false);
                playermodel = HumanModel.transform;
            }
        }

        if (WorldMapManager.Instance.FastTravelMenu.activeSelf)
        {


            CC.Move(new Vector3(0f, -gravValue * Time.deltaTime, 0f));
            return;
        }
        Vector2 moveValue = _MoveAction.ReadValue<Vector2>();
        if (moveValue.magnitude != 0)
        {
            Vector3 movement = new Vector3(moveValue.x * speed * Time.deltaTime, 0.0f, moveValue.y * speed * Time.deltaTime);

            movement = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * movement;

            Vector3 newforward = movement.normalized;

            newforward.x = 0f;
            newforward.z = 0f;
            if (movement.normalized.magnitude != 0)
            {
                forwardtarget = movement.normalized;
            }
            finalmovement += movement;
        }
        else
        {
            //finalmovement += Vector3.Lerp(CC.velocity, Vector3.zero, 0.5f);
        }

        if (Mathf.Abs(playermodel.forward.x - forwardtarget.x) > 1.5f)
        {
            playermodel.forward = new Vector3(-playermodel.forward.x, playermodel.forward.y, playermodel.forward.z);
        }
        if (Mathf.Abs(playermodel.forward.z - forwardtarget.z) > 1.5f)
        {
            playermodel.forward = new Vector3(playermodel.forward.x, playermodel.forward.y, -playermodel.forward.z);
        }

        playermodel.forward = Vector3.Lerp(playermodel.forward, forwardtarget, 0.1f);

        Vector2 CamMoveValue = _MoveCamAction.ReadValue<Vector2>();

        if (CamMoveValue.x != 0)
        {
            CamHolder.localRotation = Quaternion.Euler(Vector3.Lerp(CamHolder.localRotation.eulerAngles, CamHolder.localRotation.eulerAngles + new Vector3(0f, CamMoveValue.x * cammovepersec * Time.deltaTime, 0f), 0.3f));
        }

        if (playermodel.GetComponent<Animator>())
        {
            if (Mathf.Abs(CC.velocity.x) > 0.1f || Mathf.Abs(CC.velocity.z) > 0.1f)
            {
                playermodel.GetComponent<Animator>().SetBool("Walk", true);
            }
            else
            {
                playermodel.GetComponent<Animator>().SetBool("Walk", false);
            }
        }

        // gravity

        finalmovement += new Vector3(0f, -gravValue * Time.deltaTime, 0f);

        CC.Move(finalmovement);

    }

    public void MoveTo(Vector3 destination)
    {
        if (CC == null)
        {
            CC = GetComponent<CharacterController>();

        }
        CC.enabled = false;
        transform.position = destination;
        CC.enabled = true;
    }

}
