using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ManulChapterLoadScript : MonoBehaviour
{

    public Transform ManualLoadChapterButtons;
    public List<string> ChapterNames;
    public List<Sprite> ChapterImages;
    public Transform ManuallyLoadChapterMenu;
    private SceneLoader sceneLoader;
    private SaveManager saveManager;
    public Image ChapterImage;

    private Sprite SpriteToUse;


    private InputAction _MovementAction;
    private InputAction _CamAction;

    public int currentTopButtonID = 0;

    private GameObject previousSelected;
    public TextMeshProUGUI NameTMP;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveManager = FindAnyObjectByType<SaveManager>();
        sceneLoader = saveManager.GetComponent<SceneLoader>();
        _MovementAction = InputSystem.actions.FindAction("Movement");
        _MovementAction.Enable();
        _CamAction = InputSystem.actions.FindAction("MoveCam");
        _CamAction.Enable();
        ChangeButtonNames();
        NameTMP.text = ChapterNames[0];
        SpriteToUse = ChapterImages[0];
        ChapterImage.sprite = SpriteToUse;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected != null && currentSelected.transform.parent == ManualLoadChapterButtons)
        {
            if (_MovementAction.WasPerformedThisFrame())
            {
                Vector2 movementvalue = _MovementAction.ReadValue<Vector2>();
                ChangeCurrentID(currentSelected, movementvalue.y);
                string currentname = currentSelected.GetComponentInChildren<TextMeshProUGUI>().text;
                NameTMP.text = currentname;
                int ID = ChapterNames.IndexOf(currentname);
                SpriteToUse = ChapterImages[ID];
            }
            else if (_CamAction.WasPerformedThisFrame())
            {
                Vector2 movementvalue = _CamAction.ReadValue<Vector2>();
                ChangeCurrentID(currentSelected, movementvalue.y);
                string currentname = currentSelected.GetComponentInChildren<TextMeshProUGUI>().text;
                NameTMP.text = currentname;
                int ID = ChapterNames.IndexOf(currentname);
                SpriteToUse = ChapterImages[ID];
            }


            if (previousSelected != currentSelected)
            {
                string currentname = currentSelected.GetComponentInChildren<TextMeshProUGUI>().text;
                NameTMP.text = currentname;
                int ID = ChapterNames.IndexOf(currentname);
                SpriteToUse = ChapterImages[ID];
            }
            if (ChapterImage.sprite != SpriteToUse)
            {
                ChapterImage.sprite = SpriteToUse;
            }
        }

        previousSelected = currentSelected;
    }

    public void ManuallyLoadChapter(int Slot)
    {

        int Chapter = Slot + currentTopButtonID;

        DataScript.instance.SetupCharactersForChapter(Chapter);

        switch (Chapter)
        {
            case (12):
                sceneLoader.LoadScene("CutsceneScene", 2);
                break;
            case (13):
                sceneLoader.LoadScene("CutsceneScene", 7);
                break;
            case (14):
                sceneLoader.LoadScene("CutsceneScene", 10);
                break;
            default:
                sceneLoader.LoadScene("Chapter" + Chapter);
                break;
        }


    }


    private void ChangeCurrentID(GameObject currentselected, float YMovementValue)
    {

        if (previousSelected != currentselected)
        {
            return;
        }
        if (YMovementValue < 0 && currentselected == ManualLoadChapterButtons.GetChild(ManualLoadChapterButtons.childCount - 1).gameObject)
        {
            if (currentTopButtonID < ChapterNames.Count - ManualLoadChapterButtons.childCount)
            {
                currentTopButtonID++;
                ChangeButtonNames();
            }
        }
        else if (YMovementValue > 0 && currentselected == ManualLoadChapterButtons.GetChild(0).gameObject)
        {
            if (currentTopButtonID > 0)
            {
                currentTopButtonID--;
                ChangeButtonNames();
            }
        }
    }

    private void ChangeButtonNames()
    {

        for (int i = 0; i < ManualLoadChapterButtons.childCount; i++)
        {
            string chapter = ChapterNames[i + currentTopButtonID];
            Transform child = ManualLoadChapterButtons.GetChild(i);
            child.GetComponentInChildren<TextMeshProUGUI>().text = chapter;
        }
    }


#if UNITY_EDITOR


    [ContextMenu("Fillout Chapter Names")]
    private void FilloutChapterNames()
    {
        string[] sceneGUIDs = UnityEditor.AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Maps" });

        ChapterNames = new List<string>();
        foreach (string guid in sceneGUIDs)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            //Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
            string FullSceneName = path.Replace("Assets/Scenes/Maps/", "");
            FullSceneName = FullSceneName.Replace(".unity", "");
            string scenename = FullSceneName.ToLower();

            if (scenename != "prologue")
            {
                if (scenename.Contains("chapter"))
                {
                    ChapterNames.Add(FullSceneName);
                }
            }

        }
        UnityEditor.EditorUtility.SetDirty(this);
    }


#endif
}
