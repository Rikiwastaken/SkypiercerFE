using TMPro;
using UnityEngine;

public class AnimationCombatTextScript : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI ForegoundTMP;
    [SerializeField] private TextMeshProUGUI BackGroundTMP;

    [SerializeField] private float timebeforevanish;
    private int timebeforevanishcounter;

    [SerializeField] private float timetotargetscale;
    private int timetotargetscalecounter;

    [SerializeField] private float movespeed;

    [SerializeField] private float targetscale;

    private Vector3 InitialPosition;

    private void Start()
    {
        InitialPosition = BackGroundTMP.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (BackGroundTMP.transform.localScale.x < targetscale)
        {
            timetotargetscalecounter++;
            BackGroundTMP.transform.localScale = Vector3.one * (float)timetotargetscalecounter / (float)(timetotargetscale * Time.fixedDeltaTime);
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
