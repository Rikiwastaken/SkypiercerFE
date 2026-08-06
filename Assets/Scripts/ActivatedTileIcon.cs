using UnityEngine;

public class ActivatedTileIcon : MonoBehaviour
{

    public GameObject SelectedImage;
    public GameObject LockedSelectedImage;
    public int rotationperframe;

    public GridScript GridScript;
    public ActionsMenu _ActionsMenu;

    private GridSquareScript SelectedTile;
    private GridSquareScript LockedSelectedTile;

    private float baseYSelected;
    private float baseYLocked;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GridScript = FindAnyObjectByType<GridScript>();
        _ActionsMenu = ActionsMenu.instance;
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


            if (!_ActionsMenu.incombat)
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
            LockedSelectedTile = ActionManager.instance.currentcharacter.GetComponent<UnitScript>().UnitCharacteristics.currentTile;
        }
        else
        {
            LockedSelectedTile = null;
        }

        if (PreBattleMenuScript.instance != null && PreBattleMenuScript.instance.selectedunit != null)
        {
            LockedSelectedTile = PreBattleMenuScript.instance.selectedunit.GetComponent<UnitScript>().UnitCharacteristics.currentTile;
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
                LockedSelectedImage.transform.position = new Vector3(SelectedTile.GridCoordinates.x, baseYLocked + SelectedTile.transform.position.y, SelectedTile.GridCoordinates.y);
            }


            if (!_ActionsMenu.incombat)
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
