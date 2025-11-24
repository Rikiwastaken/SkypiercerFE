using TMPro;
using UnityEngine;

public class AnimationCombatTextScript : MonoBehaviour
{

    public TextMeshProUGUI ForegoundTMP;
    public TextMeshProUGUI BackGroundTMP;

    public float timebeforevanish;
    private int timebeforevanishcounter;

    public float timetotargetscale;
    private int timetotargetscalecounter;

    public float movespeed;

    public float targetscale;

    private Vector3 InitialPosition;

    private void Start()
    {
        InitialPosition = BackGroundTMP.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(BackGroundTMP.transform.localScale.x < targetscale)
        {
            timetotargetscalecounter++;
            BackGroundTMP.transform.localScale = Vector3.one * (float)timetotargetscalecounter / (float)(timetotargetscale*Time.fixedDeltaTime);
        }
        else
        {
            BackGroundTMP.transform.localScale = Vector3.one * targetscale;
        }

        BackGroundTMP.transform.position += new Vector3(0f, movespeed * Time.fixedDeltaTime, 0f);

        if (timebeforevanishcounter > 0)
        {
            timebeforevanishcounter--;
        }
        else
        {
            gameObject.SetActive(false);
        }
        

    }

    public void InitializeText(string text, Color color)
    {
        gameObject.SetActive(true);
        timetotargetscalecounter = 0;
        timebeforevanishcounter = (int)(timebeforevanish / Time.fixedDeltaTime);
        ForegoundTMP.text = text;
        ForegoundTMP.color = color;
        BackGroundTMP.text = text;
        BackGroundTMP.transform.position = InitialPosition;
        BackGroundTMP.transform.localScale = Vector3.one * 0.00001f;
    }

}
