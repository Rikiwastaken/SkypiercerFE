using UnityEngine;
using static CalculationToolsScript;

public class StoryPointScript : MonoBehaviour
{

    public GameObject ChapterChangeVisuals;


    public int chapterID;

    public bool isSideStory;

    public int minchapterforSideStory;

    public Material MainStoryMat;
    public Material SideStoryMat;

    private void Start()
    {
        if (isSideStory)
        {
            ChangeMaterialInGOHierarchy(ChapterChangeVisuals, SideStoryMat);
            if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached < minchapterforSideStory)
            {

                ChapterChangeVisuals.SetActive(false);
                GetComponent<BoxCollider>().isTrigger = false;

            }
        }
        else
        {
            ChangeMaterialInGOHierarchy(ChapterChangeVisuals, MainStoryMat);
            if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached < chapterID)
            {

                ChapterChangeVisuals.SetActive(false);
                GetComponent<BoxCollider>().isTrigger = false;
            }
        }

    }


}
