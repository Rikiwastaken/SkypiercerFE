using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TutorialWindowScript : MonoBehaviour
{

    private float timecounter;

    public Button validatebutton;

    public GameObject TextGO;

    public float delay;

    private bool abletocontinue;

    public InputManager InputManager;

    public void InitializeWindow(Vector2 Dimensions, string text)
    {
        gameObject.SetActive(true);
        var theBarRectTransform = GetComponent<Transform>() as RectTransform;
        theBarRectTransform.sizeDelta = new Vector2(Dimensions.x, Dimensions.y);
        validatebutton.transform.localPosition = new Vector2(0f, (-Dimensions.y / 2) - 20);
        theBarRectTransform = TextGO.GetComponent<Transform>() as RectTransform;
        theBarRectTransform.sizeDelta = new Vector2(Dimensions.x - 10, Dimensions.y - 10);
        TextGO.transform.localPosition = Vector2.zero;
        timecounter = 0;
        TextGO.GetComponent<TextMeshProUGUI>().text = text;
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if(InputManager==null)
        {
            InputManager = FindAnyObjectByType<InputManager>();
        }
        if(timecounter < delay / Time.deltaTime)
        {
            timecounter++;
            validatebutton.GetComponentInChildren<TextMeshProUGUI>().text = " ( "+(int)(delay - (timecounter * Time.deltaTime))+ " ) ";
            abletocontinue = false;
        }
        else
        {
            timecounter++;
            validatebutton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
            abletocontinue = true;
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected != validatebutton.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(validatebutton.gameObject);
            }
        }

        if(InputManager.activatejustpressed)
        {
            ContinueButton();
        }

    }

    public void ContinueButton()
    {
        if(abletocontinue)
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }

}
