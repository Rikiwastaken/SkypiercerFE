using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TipsMenuScript : MonoBehaviour
{

    public static TipsMenuScript instance;

    public List<Button> Buttons;

    public List<int> ButtonIDs;

    private int framesuppressed;
    private int framesdownpressed;

    public GameObject neutralmenu;

    public TextMeshProUGUI descriptionText;

    [Serializable]
    public class Tip
    {
        public string name;
        public string description;
        public int chapterWhereUnlocks;
    }

    [Serializable]
    public class TipClassForLoading
    {
        public List<Tip> tipList;
    }

    public List<Tip> Tips;

    public List<Tip> TipsToUse;

    private GameObject previousselection;

    private Color BaseColor;

    private InputAction _MovementAction;
    private InputAction _MoveCamAction;
    private InputAction _CancelAction;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _MovementAction = InputSystem.actions.FindAction("Movement");
        _MoveCamAction = InputSystem.actions.FindAction("MoveCam");
        _CancelAction = InputSystem.actions.FindAction("Cancel");
    }

    private void OnEnable()
    {
        BaseColor = Buttons[0].GetComponent<Image>().color;
        SelectTipsToUse();
        ButtonInitialization();

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movementValue = _MovementAction.ReadValue<Vector2>();
        Vector2 CamMoveValue = _MoveCamAction.ReadValue<Vector2>();
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected != null)
        {
            if (currentSelected.transform.parent != transform)
            {

                EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);
            }

            if (currentSelected == transform.GetChild(0).gameObject && ((_MovementAction.WasPressedThisFrame() && movementValue.y > 0) || (_MoveCamAction.WasPressedThisFrame() && CamMoveValue.y > 0)) && previousselection == currentSelected)
            {
                ChangeButtonID(-1);
            }

            if (currentSelected == transform.GetChild(Buttons.Count - 1).gameObject && ((_MovementAction.WasPressedThisFrame() && movementValue.y < 0) || (_MoveCamAction.WasPressedThisFrame() && CamMoveValue.y < 0)) && previousselection == currentSelected)
            {
                ChangeButtonID(1);
            }

            if (Buttons.IndexOf(currentSelected.GetComponent<Button>()) < ButtonIDs.Count && Buttons.IndexOf(currentSelected.GetComponent<Button>()) < TipsToUse.Count && Buttons.IndexOf(currentSelected.GetComponent<Button>()) != -1 && ButtonIDs[Buttons.IndexOf(currentSelected.GetComponent<Button>())] != -1)
            {
                if (!descriptionText.transform.parent.gameObject.activeSelf)
                {
                    descriptionText.transform.parent.gameObject.SetActive(true);
                }
                descriptionText.text = TipsToUse[ButtonIDs[Buttons.IndexOf(currentSelected.GetComponent<Button>())]].description;
            }
            else
            {
                if (descriptionText.transform.parent.gameObject.activeSelf)
                {
                    descriptionText.transform.parent.gameObject.SetActive(false);
                }
            }

            if (currentSelected == transform.GetChild(0).gameObject && (movementValue.y > 0 || CamMoveValue.y > 0))
            {
                framesuppressed++;
                if (framesuppressed > 0.15f / Time.fixedDeltaTime)
                {
                    framesuppressed = 0;
                    ChangeButtonID(-1);
                }

            }
            else
            {
                framesuppressed = 0;
            }

            if (currentSelected == transform.GetChild(Buttons.Count - 1).gameObject && (movementValue.y < 0 || CamMoveValue.y < 0))
            {
                framesdownpressed++;
                if (framesdownpressed > 0.15f / Time.fixedDeltaTime)
                {
                    framesdownpressed = 0;
                    ChangeButtonID(1);
                }

            }
            else
            {
                framesdownpressed = 0;
            }

        }


        if (_CancelAction.WasPressedThisFrame())
        {
            gameObject.SetActive(false);
            neutralmenu.SetActive(true);

            EventSystem.current.SetSelectedGameObject(neutralmenu.transform.GetChild(0).gameObject);
        }

        previousselection = currentSelected;
    }

    private void ChangeButtonID(int direction)
    {
        if (direction == 1)
        {
            if (ButtonIDs[Buttons.Count - 1] < TipsToUse.Count - 1)
            {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    ButtonIDs[i]++;
                }
            }
            UpdateButtonVisuals();
        }
        else if (direction == -1)
        {
            if (ButtonIDs[0] > 0)
            {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    ButtonIDs[i]--;
                }
            }
            UpdateButtonVisuals();
        }
    }


    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < ButtonIDs.Count; i++)
        {
            if (ButtonIDs[i] == -1 || TipsToUse.Count <= ButtonIDs[i])
            {
                Buttons[i].GetComponent<Image>().color = Color.grey;
                Buttons[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
            else
            {
                Buttons[i].GetComponent<Image>().color = BaseColor;
                Buttons[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = TipsToUse[ButtonIDs[i]].name;

            }
        }
    }

    private void SelectTipsToUse()
    {
        int currentchapter = 0;
        string SceneName = DataScript.instance.GetComponent<CombatSceneLoader>().MainSceneName.ToLower();
        if (SceneName.Contains("chapter"))
        {
            SceneName = SceneName.Replace("chapter", "");
            currentchapter = int.Parse(SceneName);
        }
        else if (!SceneName.Contains("prologue"))
        {
            currentchapter = 999;
        }
        TipsToUse = new List<Tip>();
        foreach (Tip tip in Tips)
        {
            if (tip.chapterWhereUnlocks <= currentchapter)
            {
                TipsToUse.Add(tip);
            }
        }
    }

    private void ButtonInitialization()
    {
        ButtonIDs = new List<int>();
        for (int i = 0; i < Buttons.Count; i++)
        {
            ButtonIDs.Add(-1);
        }
        int indexmod = 0;
        for (int i = Buttons.Count - 1; i >= 0; i--)
        {
            int newindex = (int)Math.Min(Tips.Count, Buttons.Count) - 1 - indexmod;
            if (newindex >= 0)
            {
                ButtonIDs[i] = newindex;
            }

            indexmod++;
        }


        UpdateButtonVisuals();
    }

#if UNITY_EDITOR
    [ContextMenu("Load Tips From JSON")]
    private void LoadTips()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Bond JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        string json = File.ReadAllText(path);

        TipClassForLoading wrapper = JsonUtility.FromJson<TipClassForLoading>(json);
        if (wrapper == null || wrapper.tipList == null)
        {
            Debug.LogError("JSON file format invalid. Needs { \"tipList\": [ ... ] }");
            return;
        }

        Tips = wrapper.tipList;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Loaded " + wrapper.tipList.Count + " tips into the Tips!");
    }


    [ContextMenu("Save Tips To JSON")]
    public void SaveTips()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Tip JSON File", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        TipClassForLoading wrapper = new TipClassForLoading() { tipList = Tips };

        string json = JsonUtility.ToJson(wrapper, true);

        try
        {
            AssetDatabase.StartAssetEditing();
            File.WriteAllText(path, json);
            Debug.Log($"Tips Saved : {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error when saving tips : {e.Message}");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }


#endif
}

