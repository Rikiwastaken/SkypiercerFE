using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WorldMapManager : MonoBehaviour
{

    public static WorldMapManager Instance;

    public int selectedchapter = -1;


    public List<string> chapternames;
    public List<string> SideStoryNames;

    public GameObject ChapterUI;
    public TextMeshProUGUI chaptertext;
    public TextMeshProUGUI chapterNametext;
    public TextMeshProUGUI chapterlvl;

    private Vector3 initialPosition;

    public float displacementpersecond;

    public float maxx;

    public GameObject Character;

    public Transform StoryPointTrsfrm;

    public GameObject FastTravelMenu;
    public List<int> FastTravelMenuIDList;

    private int faststravelmenudelay;
    public bool selectedsidestory;

    private InputAction _ActivateAction;
    private InputAction _MoveAction;
    private InputAction _CancelAction;
    private InputAction _TelekinesisAction;
    private InputAction _ShowDetailsAction;
    private InputAction _CamAction;

    public Transform StoryPoints;

    public Transform SideStoryPoints;

    public float mindistforstorypoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = ChapterUI.transform.localPosition;
        _ActivateAction = InputSystem.actions.FindAction("Validate");
        _CancelAction = InputSystem.actions.FindAction("Cancel");
        _MoveAction = InputSystem.actions.FindAction("Movement");
        _TelekinesisAction = InputSystem.actions.FindAction("TelekinesisToggle");
        _ShowDetailsAction = InputSystem.actions.FindAction("ShowDetails");
        _CamAction = InputSystem.actions.FindAction("MoveCam");
        int maxchapter = DataScript.instance.GetComponent<SaveManager>().maxchapterreached;

        if (maxchapter == 0)
        {
            Character.GetComponent<worldmapController>().MoveTo(StoryPointTrsfrm.GetChild(0).position);
        }
        else
        {
            foreach (Transform child in StoryPointTrsfrm)
            {
                if (maxchapter - 1 == child.GetComponent<StoryPointScript>().chapterID)
                {
                    Character.GetComponent<worldmapController>().MoveTo(child.position);
                }
            }
        }


    }


    // Update is called once per frame
    void Update()
    {
        DetectNearestStoryPoint();
        if (selectedchapter >= 0)
        {

            if (!ChapterUI.activeSelf)
            {
                ChapterUI.SetActive(true);
            }

            string chaptertxt = "";
            if (selectedsidestory)
            {
                chapterNametext.text = "Sidestory " + (selectedchapter + 1) + " : ";
                int averagelvl = 0;

                foreach (DataScript.SkillPerMap mapinfo in DataScript.instance.skillsPerMap)
                {
                    if (mapinfo.SideStoryID == selectedchapter)
                    {
                        averagelvl = mapinfo.averagelevel;
                    }
                }
                chapterlvl.text = "Lvl: " + averagelvl;
                chaptertext.text = SideStoryNames[selectedchapter];
            }
            else
            {
                if (selectedchapter == 0)
                {
                    chaptertxt = "Prologue : ";
                }
                else
                {
                    chaptertxt = "Chapter " + selectedchapter + " : ";
                }

                chapterNametext.text = chaptertxt;
                int averagelvl = 0;

                foreach (DataScript.SkillPerMap mapinfo in DataScript.instance.skillsPerMap)
                {
                    if (mapinfo.mapID == selectedchapter)
                    {
                        averagelvl = mapinfo.averagelevel;
                    }
                }
                chapterlvl.text = "Lvl: " + averagelvl;
                chaptertext.text = chapternames[selectedchapter];
            }




            if (ChapterUI.transform.localPosition.x < maxx)
            {
                ChapterUI.transform.localPosition += new Vector3(displacementpersecond * Time.deltaTime, 0f, 0f);
            }
            else
            {
                ChapterUI.transform.localPosition = new Vector3(maxx, ChapterUI.transform.localPosition.y, ChapterUI.transform.localPosition.z);
            }

            if (_ActivateAction.WasPressedThisFrame())
            {

                string scenename = "";
                if (selectedsidestory)
                {
                    scenename = "SideStory" + (selectedchapter + 1);
                }
                else
                {
                    if (selectedchapter == 0)
                    {
                        scenename = "Prologue";
                    }
                    else
                    {
                        scenename = "Chapter" + selectedchapter;
                    }
                }

                if (!selectedsidestory && selectedchapter == 12)
                {
                    SceneLoader.instance.LoadScene("CutsceneScene", 12);
                }
                else
                {
                    SceneLoader.instance.LoadScene(scenename);
                }
            }

        }
        else
        {

            if (ChapterUI.transform.localPosition.x > initialPosition.x)
            {
                ChapterUI.transform.localPosition -= new Vector3(displacementpersecond * Time.deltaTime * 3f, 0f, 0f);
            }
            else
            {
                ChapterUI.transform.localPosition = initialPosition;
                if (ChapterUI.activeSelf)
                {
                    ChapterUI.SetActive(false);
                }
            }
        }

        if (_TelekinesisAction.WasPressedThisFrame())
        {
            SceneLoader.instance.LoadScene("Camp");
        }

        if (faststravelmenudelay > 0)
        {
            faststravelmenudelay--;
        }

        if (FastTravelMenu.activeSelf)
        {

            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(FastTravelMenu.transform.GetChild(0).gameObject);
            }

            if (faststravelmenudelay <= 0)
            {
                Vector2 MoveValue = _MoveAction.ReadValue<Vector2>();
                Vector2 CamValue = _CamAction.ReadValue<Vector2>();
                if ((MoveValue.y > 0f || CamValue.y > 0f) && EventSystem.current.currentSelectedGameObject == FastTravelMenu.transform.GetChild(0).gameObject)
                {
                    IncreaseList();
                    faststravelmenudelay = (int)(0.1f / Time.deltaTime);
                }
                if ((MoveValue.y < 0f || CamValue.y < 0f) && EventSystem.current.currentSelectedGameObject == FastTravelMenu.transform.GetChild(FastTravelMenu.transform.childCount - 1).gameObject)
                {

                    DecreaseList();
                    faststravelmenudelay = (int)(0.1f / Time.deltaTime);
                }
            }

            if (_CancelAction.IsPressed())
            {
                FastTravelMenu.SetActive(false);
            }

        }
        else
        {
            if (_ShowDetailsAction.WasPressedThisFrame())
            {
                InitializeFastTravelList();
            }
        }


    }


    private void DetectNearestStoryPoint()
    {
        Vector3 Characterpos = Character.transform.position;
        Transform closest = null;
        float mindist = 99999;

        foreach (Transform child in StoryPoints)
        {
            if (Vector3.Distance(child.position, Characterpos) < mindist)
            {
                closest = child;
                mindist = Vector3.Distance(child.position, Characterpos);
            }
        }

        foreach (Transform child in SideStoryPoints)
        {
            if (Vector3.Distance(child.position, Characterpos) < mindist)
            {
                closest = child;
                mindist = Vector3.Distance(child.position, Characterpos);
            }
        }

        if (closest != null && mindist <= mindistforstorypoints)
        {
            selectedsidestory = closest.GetComponent<StoryPointScript>().isSideStory;
            selectedchapter = closest.GetComponent<StoryPointScript>().chapterID;
        }
        else
        {
            selectedchapter = -1;
        }

    }

    public void InitializeFastTravelList()
    {

        FastTravelMenu.SetActive(true);

        FastTravelMenuIDList = new List<int>();

        for (int i = 0; i < FastTravelMenu.transform.childCount; i++)
        {
            if (i <= DataScript.instance.GetComponent<SaveManager>().maxchapterreached)
            {
                FastTravelMenuIDList.Add(i);
                FastTravelMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = chapternames[i];
            }
            else
            {
                FastTravelMenuIDList.Add(-1);
                FastTravelMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = "";
            }

        }

        FastTravelMenu.transform.GetChild(0).GetComponent<Button>().Select();

    }

    private void IncreaseList()
    {
        if (FastTravelMenuIDList[0] > 0)
        {
            for (int i = 0; i < FastTravelMenuIDList.Count; i++)
            {
                if (FastTravelMenuIDList[i] > 0)
                {
                    FastTravelMenuIDList[i]--;
                    FastTravelMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = chapternames[FastTravelMenuIDList[i]];
                }
            }
        }
    }

    private void DecreaseList()
    {

        if (FastTravelMenuIDList[FastTravelMenuIDList.Count - 1] < DataScript.instance.GetComponent<SaveManager>().maxchapterreached && FastTravelMenuIDList[FastTravelMenuIDList.Count - 1] >= 0)
        {
            for (int i = 0; i < FastTravelMenuIDList.Count; i++)
            {
                if (FastTravelMenuIDList[i] >= 0)
                {
                    FastTravelMenuIDList[i]++;
                    FastTravelMenu.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = chapternames[FastTravelMenuIDList[i]];
                }
            }
        }
    }

    public void ConfirmFastTravel(int ID)
    {
        if (FastTravelMenuIDList[ID] >= 0)
        {
            FastTravelMenu.gameObject.SetActive(false);
            worldmapController.instance.MoveTo(StoryPointTrsfrm.GetChild(FastTravelMenuIDList[ID]).position);
        }

    }

}
