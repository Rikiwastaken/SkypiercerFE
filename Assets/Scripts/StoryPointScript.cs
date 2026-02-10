using UnityEngine;

public class StoryPointScript : MonoBehaviour
{

    public GameObject ChapterChangeVisuals;


    public int chapterID;


    private void Start()
    {
        if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached < chapterID)
        {
            ChapterChangeVisuals.SetActive(false);
            GetComponent<BoxCollider>().isTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {

        if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached >= chapterID)
        {
            if (collision.transform.CompareTag("Player"))
            {
                WorldMapManager.Instance.selectedchapter = chapterID;
            }
        }


    }

    private void OnTriggerExit(Collider collision)
    {
        if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached >= chapterID)
        {
            if (collision.transform.CompareTag("Player"))
            {
                WorldMapManager.Instance.selectedchapter = -1;
            }
        }
    }
}
