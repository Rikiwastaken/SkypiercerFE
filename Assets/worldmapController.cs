using UnityEngine;

public class worldmapController : MonoBehaviour
{

    private InputManager inputManager;

    private Camera cam;

    public Transform CamHolder;

    public float cammovepersec;

    private Rigidbody rb;

    public float speed;

    public Transform playermodel;

    private Vector3 forwardtarget = new Vector3(0, 0, 1);

    private Vector3 targetcamrotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputManager = InputManager.instance;
        cam = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        playermodel.GetComponent<Animator>().SetBool("WorldMap", true);
    }

    // Update is called once per frame
    void Update()
    {
        if (inputManager.movementValue.magnitude != 0)
        {
            Vector3 movement = new Vector3(inputManager.movementValue.x * speed, 0.0f, inputManager.movementValue.y * speed);

            movement = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * movement;



            movement.y = rb.linearVelocity.y;

            Vector3 newforward = movement.normalized;

            newforward.x = 0f;
            newforward.z = 0f;
            if (movement.normalized.magnitude != 0)
            {
                forwardtarget = movement.normalized;
            }
            rb.linearVelocity = movement;
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, 0.05f);
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


        if (inputManager.cammovementValue.x != 0)
        {
            CamHolder.localRotation = Quaternion.Euler(Vector3.Lerp(CamHolder.localRotation.eulerAngles, CamHolder.localRotation.eulerAngles + new Vector3(0f, inputManager.cammovementValue.x * cammovepersec * Time.deltaTime, 0f), 0.3f));
        }

        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f || Mathf.Abs(rb.linearVelocity.z) > 0.1f)
        {
            playermodel.GetComponent<Animator>().SetBool("Walk", true);
        }
        else
        {
            playermodel.GetComponent<Animator>().SetBool("Walk", false);
        }
    }
}
