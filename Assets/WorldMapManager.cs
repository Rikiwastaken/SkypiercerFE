using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        foreach (Transform child in StoryPointTrsfrm)
        {
            if (DataScript.instance.GetComponent<SaveManager>().currentchapter == child.GetComponent<StoryPointScript>().chapterID)
            {
                Character.transform.position = child.position;
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

    }
}
