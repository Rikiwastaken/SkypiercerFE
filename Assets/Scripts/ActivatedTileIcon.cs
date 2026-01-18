using UnityEngine;

public class ActivatedTileIcon : MonoBehaviour
{

    public GameObject SelectedImage;
    public GameObject LockedSelectedImage;
    public int rotationperframe;

    public GridScript GridScript;
    public cameraScript cameraScript;

    private GridSquareScript SelectedTile;
    private GridSquareScript LockedSelectedTile;

    private float baseYSelected;
    private float baseYLocked;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = FindAnyObjectByType<GridScript>();
        cameraScript = FindAnyObjectByType<cameraScript>();
        baseYSelected = SelectedImage.transform.position.y;
        baseYLocked = LockedSelectedImage.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        SelectedTile = GridScript.selection;
        if (SelectedTile == null)
        {
            if (SelectedImage.activeSelf)
            {
                SelectedImage.SetActive(false);
            }
        }
        else
        {
            if (SelectedImage.transform.position.x != SelectedTile.GridCoordinates.x || SelectedImage.transform.position.z != SelectedTile.GridCoordinates.y)
            {
                SelectedImage.transform.position = new Vector3(SelectedTile.GridCoordinates.x, baseYSelected + SelectedTile.transform.position.y, SelectedTile.GridCoordinates.y);
            }


            if (!cameraScript.incombat)
            {
                if (!SelectedImage.activeSelf)
                {
                    SelectedImage.SetActive(true);
                }

                SelectedImage.transform.rotation = Quaternion.Euler(SelectedImage.transform.rotation.eulerAngles + new Vector3(0f, rotationperframe, 0f));
            }
            else
            {
                if (SelectedImage.activeSelf)
                {
                    SelectedImage.SetActive(false);
                }

            }
        }





        if (GridScript.lockselection && ActionManager.instance.currentcharacter != null)
        {
            LockedSelectedTile = ActionManager.instance.currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.currentTile[0];
        }

        if (PreBattleMenuScript.instance != null && PreBattleMenuScript.instance.selectedunit != null)
        {
            LockedSelectedTile = PreBattleMenuScript.instance.selectedunit.GetComponent<UnitScript>().UnitCharacteristics.currentTile[0];
        }

        if (LockedSelectedTile == null)
        {
            if (LockedSelectedImage.activeSelf)
            {
                LockedSelectedImage.SetActive(false);
            }
        }
        else
        {
            if (LockedSelectedImage.transform.position.x != LockedSelectedTile.GridCoordinates.x || LockedSelectedImage.transform.position.z != LockedSelectedTile.GridCoordinates.y)
            {
                LockedSelectedImage.transform.position = new Vector3(SelectedTile.GridCoordinates.x, baseYLocked + LockedSelectedImage.transform.position.y, SelectedTile.GridCoordinates.y);
            }


            if (!cameraScript.incombat)
            {
                if (!LockedSelectedImage.activeSelf)
                {
                    LockedSelectedImage.SetActive(true);
                }

                LockedSelectedImage.transform.rotation = Quaternion.Euler(LockedSelectedImage.transform.rotation.eulerAngles - new Vector3(0f, rotationperframe, 0f));
            }
            else
            {
                if (LockedSelectedImage.activeSelf)
                {
                    LockedSelectedImage.SetActive(false);
                }

            }
        }


    }
}
