using UnityEngine;

public class WorldMapWaterScript : MonoBehaviour
{

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<worldmapController>() != null)
        {
            collision.transform.GetComponent<worldmapController>().isshipping = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.transform.GetComponent<worldmapController>() != null)
        {
            collision.transform.GetComponent<worldmapController>().isshipping = false;
        }
    }
}
