using UnityEngine;

public class OcclusionScript : MonoBehaviour
{
    private void OnBecameVisible()
    {
        if(Application.isPlaying)
        {
            if (GetComponent<GridSquareScript>() != null && !GetComponent<GridSquareScript>().enabled)
            {
                GetComponent<GridSquareScript>().enabled = true;
            }
            if (GetComponent<UnitScript>()!=null && !GetComponent<UnitScript>().enabled)
            {
                GetComponent<UnitScript>().enabled = true;
            }
        }
        
    }

    private void OnBecameInvisible()
    {
        if (GetComponent<GridSquareScript>() != null && GetComponent<GridSquareScript>().enabled)
        {
            GetComponent<GridSquareScript>().enabled = false;
        }
        if (GetComponent<UnitScript>() != null && GetComponent<UnitScript>().enabled)
        {
            GetComponent<UnitScript>().enabled = false;
        }
    }
}
