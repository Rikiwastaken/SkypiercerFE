using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldMapManager : MonoBehaviour
{

    public static WorldMapManager Instance;

    public int selectedchapter = -1;


    public List<string> chapternames;

    public GameObject ChapterUI;
    public TextMeshProUGUI chaptertext;
    public TextMeshProUGUI chapterNametext;

    private Vector3 initialPosition;

    public float displacementpersecond;

    public float maxx;

    public GameObject Character;

    public Transform StoryPointTrsfrm;

    public GameObject FastTravelMenu;
    public List<int> FastTravelMenuIDList;

    private int faststravelmenudelay;

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

        int maxchapter = DataScript.instance.GetComponent<SaveManager>().maxchapterreached;

        if (maxchapter == 0)
        {
            Character.transform.position = StoryPointTrsfrm.GetChild(0).position;
        }
        else
        {
            foreach (Transform child in StoryPointTrsfrm)
            {
                if (maxchapter - 1 == child.GetComponent<StoryPointScript>().chapterID)
                {
                    Character.transform.position = child.position;
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (selectedchapter >= 0)
        {

            if (!ChapterUI.activeSelf)
            {
                ChapterUI.SetActive(true);
            }

            string chaptertxt = "";
            if (selectedchapter == 0)
            {
                chaptertxt = "Prologue : ";
            }
            else
            {
                chaptertxt = "Chapter " + selectedchapter + " : ";
            }
            chapterNametext.text = chaptertxt;
            chaptertext.text = chapternames[selectedchapter];

            if (ChapterUI.transform.localPosition.x < maxx)
            {
                ChapterUI.transform.localPosition += new Vector3(displacementpersecond * Time.deltaTime, 0f, 0f);
            }
            else
            {
                ChapterUI.transform.localPosition = new Vector3(maxx, ChapterUI.transform.localPosition.y, ChapterUI.transform.localPosition.z);
            }

            if (InputManager.instance.activatejustpressed)
            {

                string scenename = "";
                if (selectedchapter == 0)
                {
                    scenename = "Prologue";
                }
                else
                {
                    scenename = "Chapter" + selectedchapter;
                }

                SceneLoader.instance.LoadScene(scenename);
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

        if (InputManager.instance.Telekinesisjustpressed)
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
                if ((InputManager.instance.movementValue.y > 0f || InputManager.instance.cammovementValue.y > 0f) && EventSystem.current.currentSelectedGameObject == FastTravelMenu.transform.GetChild(0).gameObject)
                {
                    IncreaseList();
                    faststravelmenudelay = (int)(0.1f / Time.deltaTime);
                }
                if ((InputManager.instance.movementValue.y < 0f || InputManager.instance.cammovementValue.y < 0f) && EventSystem.current.currentSelectedGameObject == FastTravelMenu.transform.GetChild(FastTravelMenu.transform.childCount - 1).gameObject)
                {

                    DecreaseList();
                    faststravelmenudelay = (int)(0.1f / Time.deltaTime);
                }
            }

            if (InputManager.instance.cancelpressed)
            {
                FastTravelMenu.SetActive(false);
            }

        }
        else
        {
            if (InputManager.instance.ShowDetailspressed)
            {
                InitializeFastTravelList();
            }
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
            worldmapController.instance.transform.position = StoryPointTrsfrm.GetChild(FastTravelMenuIDList[ID]).position;
        }

    }

}
