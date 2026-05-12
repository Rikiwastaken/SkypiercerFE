using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialWindowScript : MonoBehaviour
{

    public static TutorialWindowScript instance;

    private float timecounter;

    public Button validatebutton;

    public GameObject TextGO;

    public float delay;

    private bool abletocontinue;

    private InputAction _ActivateAction;
    private void Start()
    {
        _ActivateAction = InputSystem.actions.FindAction("Validate");
    }

    private void Awake()
    {
        instance = this;
    }

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
    }

    private void Update()
    {

        if (timecounter < delay / Time.fixedDeltaTime)
        {
            timecounter++;
            validatebutton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
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

        if (_ActivateAction.IsPressed())
        {
            ContinueButton();
        }

    }

    public void ContinueButton()
    {
        if (abletocontinue)
        {
            gameObject.SetActive(false);
        }
    }

}
