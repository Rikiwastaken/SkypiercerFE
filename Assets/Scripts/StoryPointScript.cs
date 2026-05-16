using UnityEngine;

public class StoryPointScript : MonoBehaviour
{

    public GameObject ChapterChangeVisuals;


    public int chapterID;

    public bool isSideStory;

    public int minchapterforSideStory;

    private void Start()
    {
        if (isSideStory)
        {
            if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached < minchapterforSideStory)
            {
                ChapterChangeVisuals.SetActive(false);
                GetComponent<BoxCollider>().isTrigger = false;
            }
        }
        else
        {
            if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached < chapterID)
            {
                ChapterChangeVisuals.SetActive(false);
                GetComponent<BoxCollider>().isTrigger = false;
            }
        }

    }


}
