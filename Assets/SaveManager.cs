using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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

    public void LoadCurrentSave()
    {
        List<SaveClass> loadedSaves = GetAllSaves();
        foreach(SaveClass save in loadedSaves)
        {
            if(save.slot==activeSlot && save.versionID==versionID)
            {
                GetComponent<DataScript>().PlayableCharacterList = save.PlayableCharacterList;
                GetComponent<DataScript>().PlayerInventory = save.PlayerInventory;
                return;
            }
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
            PlayerInventory = GetComponent<DataScript>().PlayerInventory
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
