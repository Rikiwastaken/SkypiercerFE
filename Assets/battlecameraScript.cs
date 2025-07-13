using UnityEngine;

public class battlecameraScript : MonoBehaviour
{

    private Vector2 Destination;

    private GridScript GridScript;

    public float camspeed;

    private InputManager InputManager;

    void FixedUpdate()
    {
        if(GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

        if(GridScript.actionsMenu.activeSelf)
        {
            return;
        }

        Destination = new Vector2(GridScript.selection.transform.position.x, GridScript.selection.transform.position.z);

        if(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), Destination)>0.1f)
        {
            float movex = (Destination.x - transform.position.x) * camspeed * Time.fixedDeltaTime;
            float movez = (Destination.y - transform.position.z) * camspeed * Time.fixedDeltaTime;
            transform.position += new Vector3(movex, 0f, movez);
        }

        if(InputManager == null)
        {
            InputManager = FindAnyObjectByType<InputManager>();
        }

        if(InputManager.cammovementValue.x != 0f)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, InputManager.cammovementValue.x* -30f * Time.fixedDeltaTime, 0f));
        }

        if (InputManager.cammovementValue.y != 0f)
        {
            transform.position += new Vector3(0f, InputManager.cammovementValue.y * -3f * Time.fixedDeltaTime, 0f);
        }
        if(transform.position.y<2f)
        {
            transform.position = new Vector3(transform.position.x,2f,transform.position.z);
        }
        if (transform.position.y > 7f)
        {
            transform.position = new Vector3(transform.position.x, 7f, transform.position.z);
        }

    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
        transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90,0,0));
        transform.position = new Vector3(transform.position.x, 4.5f, transform.position.z);
    }
}
