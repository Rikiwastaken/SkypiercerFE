using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static DataScript;
using static UnitScript;

public class SaveManager : MonoBehaviour
{

    public static SaveManager instance;

    public string versionID;

    public int activeSlot;

    [Serializable]
    public class SaveClass
    {
        public string versionID;
        public int slot;
        public int chapter;
        public List<Character> PlayableCharacterList;
        public Inventory PlayerInventory;
        public float secondselapsed;
    }

    [Serializable]
    public class OptionsClass
    {
        public string versionID;
        public float musicvolume;
        public float SEVolume;
        public bool Fullscreen;
        public bool BattleAnimations;
        public bool FixedGrowth;
    }

    public int numberofslots;

    public List<SaveClass> SaveClasses;

    public float secondselapsed;

    public SaveClass DefaultSave;

    public OptionsClass Options;

    private void Awake()
    {
        if(instance== null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        LoadOptions();
        DefaultSave.slot = -1;
        DefaultSave.versionID = versionID;
        DefaultSave.chapter = 0;
        DefaultSave.PlayableCharacterList = DataScript.instance.PlayableCharacterList;
        DefaultSave.PlayerInventory = DataScript.instance.PlayerInventory;
        DefaultSave.secondselapsed = 0;
    }

    private void FixedUpdate()
    {
        string activescenename = SceneManager.GetActiveScene().name;
        if (activescenename != "MainMenu" && activescenename != "FirstScene" && activescenename != "LoadingScene")
        {
            secondselapsed += Time.fixedDeltaTime;
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private List<SaveClass> GetAllSaves()
    {
        List<SaveClass> loadedSaves = new List<SaveClass>();
        string path = Application.persistentDataPath;
        Debug.Log("Chargement des sauvegardes depuis : " + path);

        if (!Directory.Exists(path))
        {
            Debug.LogWarning("Dossier introuvable : " + path);
            return null;
        }

        string[] files = Directory.GetFiles(path, "*.json");

        foreach (string file in files)
        {
            try
            {
                if (file.Contains("options"))
                {
                    continue;
                }
                string json = File.ReadAllText(file);
                SaveClass save = JsonUtility.FromJson<SaveClass>(json);
                if (save != null)
                {
                    loadedSaves.Add(save);
                    Debug.Log($"File Loaded : {Path.GetFileName(file)}");
                }
                else
                {
                    Debug.LogWarning($"Impossible to parse : {Path.GetFileName(file)}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error reading {file} : {e.Message}");
            }
        }

        Debug.Log($"Loading successful. {loadedSaves.Count} save(s) found.");
        return loadedSaves;
    }

    private void LoadOptions()
    {
        string path = Application.persistentDataPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string fileName = "options.json";
        string fullPath = Path.Combine(path, fileName);

        try
        {
            string json = File.ReadAllText(fullPath);
            if (json != null)
            {
                Options = JsonUtility.FromJson<OptionsClass>(json);
                Screen.fullScreen = Options.Fullscreen;
            }
        }
        catch
        {
            Debug.Log("creating new options data");
            Options = new OptionsClass();
            Options.musicvolume = 1.000001f;
            Options.SEVolume = 1.000001f;
            Options.Fullscreen = Screen.fullScreen;
            Options.BattleAnimations = true;
            Options.FixedGrowth = false;
        }
    }

    public void SaveOptions()
    {
        if (Options == null)
        {
            Options = new OptionsClass();
        }
        string path = Application.persistentDataPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string fileName = "options.json";
        string fullPath = Path.Combine(path, fileName);
        string json = JsonUtility.ToJson(Options, true);

        try
        {
            File.WriteAllText(fullPath, json);
            Debug.Log($"Options Saved : {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error when saving options : {e.Message}");
        }

        if(MusicManager.instance != null)
        {
            MusicManager.instance.ChangeVolume();

        }

    }

    public void InitializeSaveButtons(List<Button> buttons)
    {
        LoadSaves();
        for (int i = 0; i < buttons.Count; i++)
        {
            if (SaveClasses[i] != null)
            {
                SaveClass save = SaveClasses[i];
                int numberofseconds = (int)save.secondselapsed;
                int numberofminutes = (int)save.secondselapsed / 60;
                int numberofhours = numberofminutes / 60;
                numberofminutes = numberofminutes % 60;
                string text = "Slot : " + (i + 1) + "  Time played : " + numberofhours + "h" + numberofminutes + "min\nChapter : " + save.chapter;
                buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
            }
            else
            {
                string text = "Slot : " + (i + 1) + "  Empty";
                buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
            }
        }
    }
    public void LoadSaves()
    {
        List<SaveClass> loadedSaves = GetAllSaves();
        SaveClasses = new List<SaveClass>();
        for (int i = 0; i < numberofslots; i++)
        {
            SaveClasses.Add(null);
        }
        foreach (SaveClass save in loadedSaves)
        {
            if (save.slot >= 0 && save.slot < numberofslots && save.versionID == versionID)
            {
                SaveClasses[save.slot] = save;
            }
        }
    }

    public void ApplySave(int slot)
    {
        DataScript DS = DataScript.instance;

        if (slot >= 0 && slot < SaveClasses.Count)
        {
            DS.PlayableCharacterList = SaveClasses[slot].PlayableCharacterList;
            DS.PlayerInventory = SaveClasses[slot].PlayerInventory;
            secondselapsed = SaveClasses[slot].secondselapsed;
        }
        else if (slot == -1)
        {
            DS.PlayableCharacterList = DefaultSave.PlayableCharacterList;
            DS.PlayerInventory = DefaultSave.PlayerInventory;
            secondselapsed = 0;
        }

    }

    public void SaveCurrentSlot()
    {

        int currentchapter = 0;
        string scenename = SceneManager.GetActiveScene().name;

        for(int i = 0; i < SceneManager.sceneCount;i++)
        {
            if(SceneManager.GetSceneAt(i).name== "Prologue" || SceneManager.GetSceneAt(i).name.Contains("Chapter"))
            {
                scenename = SceneManager.GetSceneAt(i).name;
                break;
            }
        }

        if (scenename.Contains("Chapter"))
        {
            scenename =scenename.Replace("Chapter", "");
            currentchapter = int.Parse(scenename) + 1;
            DataScript.instance.UpdatePlayableUnits();
        }
        if(scenename.Contains("Prologue"))
        {
            currentchapter = 1;
            DataScript.instance.UpdatePlayableUnits();
        }

        SaveClass save = new SaveClass
        {
            versionID = versionID,
            slot = activeSlot,
            chapter = currentchapter,
            PlayableCharacterList = DataScript.instance.PlayableCharacterList,
            PlayerInventory = DataScript.instance.PlayerInventory,
            secondselapsed = secondselapsed

        };

        string json = JsonUtility.ToJson(save, true);

        string path = Application.persistentDataPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fileName = $"{activeSlot}_{versionID}.json";
        string fullPath = Path.Combine(path, fileName);


        try
        {
            File.WriteAllText(fullPath, json);
            Debug.Log($"Sauvegarde effectu�e : {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la sauvegarde : {e.Message}");
        }
    }
}
