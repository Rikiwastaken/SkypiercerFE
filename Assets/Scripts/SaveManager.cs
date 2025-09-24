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

    public int numberofslots;

    public List<SaveClass> SaveClasses;

    public float secondselapsed;

    public SaveClass DefaultSave;

    private void Start()
    {
        DefaultSave.slot = -1;
        DefaultSave.versionID = versionID;
        DefaultSave.chapter = 0;
        DefaultSave.PlayableCharacterList = GetComponent<DataScript>().PlayableCharacterList;
        DefaultSave.PlayerInventory = GetComponent<DataScript>().PlayerInventory;
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
                string json = File.ReadAllText(file);
                SaveClass save = JsonUtility.FromJson<SaveClass>(json);
                if (save != null)
                {
                    loadedSaves.Add(save);
                    Debug.Log($"Fichier chargé : {Path.GetFileName(file)}");
                }
                else
                {
                    Debug.LogWarning($"Impossible de parser : {Path.GetFileName(file)}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur en lisant {file} : {e.Message}");
            }
        }

        Debug.Log($"Chargement terminé. {loadedSaves.Count} sauvegarde(s) trouvée(s).");
        return loadedSaves;
    }

    public void InitializeSaveButtons(List<Button> buttons)
    {
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
        for(int i = 0; i < numberofslots; i++)
        {
            SaveClasses.Add(null);
        }
        foreach (SaveClass save in loadedSaves)
        {
            if(save.slot>=0 && save.slot<numberofslots && save.versionID==versionID)
            {
                SaveClasses[save.slot] = save;
            }
        }
    }

    public void ApplySave(int slot)
    {
        if(slot>=0 && slot< SaveClasses.Count)
        {
            GetComponent<DataScript>().PlayableCharacterList = SaveClasses[slot].PlayableCharacterList;
            GetComponent<DataScript>().PlayerInventory = SaveClasses[slot].PlayerInventory;
            secondselapsed = SaveClasses[slot].secondselapsed;
        }
        else if(slot==-1)
        {
            GetComponent<DataScript>().PlayableCharacterList = DefaultSave.PlayableCharacterList;
            GetComponent<DataScript>().PlayerInventory = DefaultSave.PlayerInventory;
            secondselapsed = 0;
        }

    }

    public void SaveCurrentSlot()
    {
        SaveClass save = new SaveClass
        {
            versionID = versionID,
            slot = activeSlot,
            chapter = 0,
            PlayableCharacterList = GetComponent<DataScript>().PlayableCharacterList,
            PlayerInventory = GetComponent<DataScript>().PlayerInventory,
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
            Debug.Log($"Sauvegarde effectuée : {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la sauvegarde : {e.Message}");
        }
    }
}
