using UnityEngine;

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
            ChangeMaterial(ChapterChangeVisuals, SideStoryMat);
            if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached < minchapterforSideStory)
            {

                ChapterChangeVisuals.SetActive(false);
                GetComponent<BoxCollider>().isTrigger = false;

            }
        }
        else
        {
            ChangeMaterial(ChapterChangeVisuals, MainStoryMat);
            if (DataScript.instance.GetComponent<SaveManager>().maxchapterreached < chapterID)
            {

                ChapterChangeVisuals.SetActive(false);
                GetComponent<BoxCollider>().isTrigger = false;
            }
        }

    }

    public void ChangeMaterial(GameObject GO, Material material)
    {
        Debug.Log("object name " + GO.name);
        if (GO.GetComponent<Renderer>())
        {
            GO.GetComponent<Renderer>().material = material;
        }
        foreach (Transform child in GO.transform)
        {
            ChangeMaterial(child.gameObject, material);
        }
    }


}
