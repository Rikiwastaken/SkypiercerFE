using System.Collections;
using UnityEngine;
using static UnitScript;

public class TutorialScript : MonoBehaviour
{

    public static TutorialScript instance;

    private bool initialized;

    public SpriteRenderer CrossSprite;
    private float baseYCrossSprite;

    private Character Lea;

    private MapEventManager mapEventManager;

    [Header("Enemy Variables")]
    public int FirstEnemyID;
    private Character FirstEnemyGO;
    public int SecondEnemyID;
    private Character SecondEnemyGO;

    [Header("First Attack")]
    public bool firstenemyattacked;
    public GridSquareScript ZackTargetTile;

    [Header("Second Attack")]
    public bool secondenemyattacked;
    public GridSquareScript ElwynTargetTile;
    public int SecondDialogueEventID;

    public int ThirdDialogueEventID;
    public GridSquareScript LeaTargetTile;

    [Header("Delay Before Start")]
    public float timebeforestart = 0.1f;
    private float timebeforestartcounter;


    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapEventManager = GetComponent<MapEventManager>();
        timebeforestartcounter = Time.time + timebeforestart;
        baseYCrossSprite = CrossSprite.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < timebeforestartcounter)
        {
            return;
        }
        if (!initialized)
        {
            initialization();
            PlaceTutorialCross(ZackTargetTile);
            StartCoroutine(PlayEvent(8, 0f));
        }


        if (!firstenemyattacked)
        {
            if (FirstEnemyGO.currentHP <= 0)
            {
                firstenemyattacked = true;
                LockAllUnitsButOne(2);
                PlaceTutorialCross(ElwynTargetTile);

            }



        }
        else if (!secondenemyattacked)
        {
            StartCoroutine(PlayDialogueAfterCombat(SecondDialogueEventID, 0.5f));

            if (SecondEnemyGO.currentHP <= 0)
            {
                secondenemyattacked = true;
                LockAllUnitsButOne(1);
                PlaceTutorialCross(LeaTargetTile);

            }
        }
        else
        {

            StartCoroutine(PlayDialogueAfterCombat(ThirdDialogueEventID, 0.5f));

            if (GridScript.instance.GetUnit(LeaTargetTile) != null && Lea.alreadyplayed)
            {


                this.enabled = false;
                TurnManger.instance.currentlyplaying = "enemy";
                TurnManger.instance.phaseTextScript.SetupText(TurnManger.instance.currentlyplaying);
                TurnManger.instance.BeginningOfTurnsTrigger(TurnManger.instance.playableunitGO);
                CrossSprite.gameObject.SetActive(false);
                StartCoroutine(PlayEvent(5, 1f));
            }


        }
    }

    private IEnumerator PlayDialogueAfterCombat(int ID, float delay = 0.5f)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        mapEventManager.ManuallyTriggerEvent(ID, -1);
    }

    private IEnumerator PlayEvent(int ID, float delay)
    {
        yield return new WaitForSeconds(delay);
        mapEventManager.ManuallyTriggerEvent(ID, -1);
    }

    private void PlaceTutorialCross(GridSquareScript tile)
    {
        CrossSprite.gameObject.SetActive(true);
        CrossSprite.transform.position = new Vector3(tile.GridCoordinates.x, baseYCrossSprite + tile.transform.position.y, tile.GridCoordinates.y);
    }

    private void initialization()
    {
        initialized = true;
        foreach (GameObject unitGO in GetComponent<GridScript>().allunitGOs)
        {
            if (unitGO.GetComponent<UnitScript>().UnitCharacteristics.ID == FirstEnemyID)
            {
                FirstEnemyGO = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
            }
            if (unitGO.GetComponent<UnitScript>().UnitCharacteristics.ID == SecondEnemyID)
            {
                SecondEnemyGO = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
            }
            if (unitGO.GetComponent<UnitScript>().UnitCharacteristics.name == "Lea")
            {
                Lea = unitGO.GetComponent<UnitScript>().UnitCharacteristics;
            }
        }
        LockAllUnitsButOne(0);
    }

    private void LockAllUnitsButOne(int ID)
    {
        foreach (GameObject unitGO in GetComponent<GridScript>().allunitGOs)
        {
            if (unitGO.GetComponent<UnitScript>().UnitCharacteristics.affiliation == "playable")
            {
                if (unitGO.GetComponent<UnitScript>().UnitCharacteristics.ID != ID)
                {
                    unitGO.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = true;
                    unitGO.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = true;

                }
                else
                {
                    unitGO.GetComponent<UnitScript>().UnitCharacteristics.alreadymoved = false;
                    unitGO.GetComponent<UnitScript>().UnitCharacteristics.alreadyplayed = false;
                }

            }
        }
    }
}
