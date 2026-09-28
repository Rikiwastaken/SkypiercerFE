using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{

    private SaveManager saveManager;
    private SceneLoader sceneLoader;

    public Transform OptionsMenu;
    public Button OptionsMenuButton;
    public Transform BaseMenu;
    public Transform ContinueMenu;
    public Transform ManuallyLoadChapterMenu;
    public Button ContinueMenuButton;

    public Transform Waterwheel;



    public float waterwheelrotationpersecond;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (DataScript.instance == null)
        {
            SceneManager.LoadScene("FirstScene");
            return;
        }

        saveManager = FindAnyObjectByType<SaveManager>();
        saveManager.LoadSaves();
        BaseMenu.GetChild(0).GetComponent<Button>().Select();
        sceneLoader = saveManager.GetComponent<SceneLoader>();
    }

    private void FixedUpdate()
    {
        Waterwheel.Rotate(waterwheelrotationpersecond * Time.deltaTime, 0f, 0f);

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (BaseMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != BaseMenu)
            {
                EventSystem.current.SetSelectedGameObject(BaseMenu.GetChild(0).gameObject);
            }
        }
        else if (ContinueMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(ContinueMenu.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != ContinueMenu)
            {
                EventSystem.current.SetSelectedGameObject(ContinueMenu.GetChild(0).gameObject);
            }
        }
        else if (ManuallyLoadChapterMenu.gameObject.activeSelf)
        {
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(ManuallyLoadChapterMenu.GetChild(0).gameObject);
            }
            else if (currentSelected.transform.parent != ManuallyLoadChapterMenu && currentSelected.transform.parent.parent != ManuallyLoadChapterMenu)
            {
                EventSystem.current.SetSelectedGameObject(ManuallyLoadChapterMenu.GetChild(0).gameObject);
            }
        }
    }

    public void InitializeSaveButtons()
    {
        List<Button> buttons = new List<Button>();
        for (int i = 0; i < ContinueMenu.childCount - 1; i++)
        {
            buttons.Add(ContinueMenu.GetChild(i).GetComponent<Button>());
        }
        saveManager.InitializeSaveButtons(buttons);
    }

    public void LoadTestMap()
    {
        saveManager.ApplySave(-1);
        DataScript.instance.SetupCharactersForChapter(99);
        sceneLoader.LoadScene("TestMap");
    }

    public void SetDeathType(bool permadeath)
    {
        DataScript.instance.Permadeath = permadeath;
    }

    public void SetGrowthType(bool fixedgrowth)
    {
        DataScript.instance.FixedGrowth = fixedgrowth;
    }

    public void OnCancel()
    {
        if (OptionsMenu.gameObject.activeSelf)
        {
            OptionsMenuButton.onClick.Invoke();
        }
        else if (ContinueMenu.gameObject.activeSelf)
        {
            ContinueMenuButton.onClick.Invoke();
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }

    private string GetChapterScene(int chapter)
    {
        if (chapter == 0)
        {
            return "Prologue";
        }
        else
        {
            return "Chapter" + chapter;
        }
    }

    public void LoadSave(int slot)
    {

        if (saveManager.SaveClasses[slot] != null)
        {
            saveManager.activeSlot = slot;
            saveManager.ApplySave(slot);
            if (saveManager.currentchapter == 1)
            {
                sceneLoader.LoadScene("Chapter1");
            }
            else
            {
                sceneLoader.LoadScene("Camp");
            }
        }
        else
        {
            saveManager.ApplySave(-1);
            ApplyRandomCharacterBoosts();
            saveManager.activeSlot = slot;
            sceneLoader.LoadScene("Prologue");
        }
        DataScript.instance.CalculateMaxSP();
    }

    public void ApplyRandomCharacterBoosts()
    {
        foreach (UnitScript.Character Character in DataScript.instance.PlayableCharacterList)
        {

            Character.stats.HP += Mathf.Min(0.99f, Character.growth.HPGrowth / 100f * Random.Range(0.5f, 1.5f));
            Character.stats.Strength += Mathf.Min(0.99f, Character.growth.StrengthGrowth / 100f * Random.Range(0.5f, 1.5f));
            Character.stats.Psyche += Mathf.Min(0.99f, Character.growth.PsycheGrowth / 100f * Random.Range(0.5f, 1.5f));
            Character.stats.Defense += Mathf.Min(0.99f, Character.growth.DefenseGrowth / 100f * Random.Range(0.5f, 1.5f));
            Character.stats.Resistance += Mathf.Min(0.99f, Character.growth.ResistanceGrowth / 100f * Random.Range(0.5f, 1.5f));
            Character.stats.Speed += Mathf.Min(0.99f, Character.growth.SpeedGrowth / 100f * Random.Range(0.5f, 1.5f));
            Character.stats.Dexterity += Mathf.Min(0.99f, Character.growth.DexterityGrowth / 100f * Random.Range(0.5f, 1.5f));
            Character.stats.Luck += Mathf.Min(0.99f, Character.growth.LuckGrowth / 100f * Random.Range(0.5f, 1.5f));
        }
    }

    public void LoadPrologue()
    {
        saveManager.ApplySave(-1);
        sceneLoader.LoadScene("CutsceneScene", 1);
    }

    public void ResetSave()
    {
        DataScript.instance.RestoreBaseCharacterValues();
    }

}
