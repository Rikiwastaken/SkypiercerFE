using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class DataScript : MonoBehaviour
{

    public List<equipment> equipmentList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SetupEquipmentIDs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupEquipmentIDs()
    {
        for (int i = 0; i < equipmentList.Count; i++)
        {
            equipmentList[i].ID = i;
        }
    }

    public void GenerateEquipmentList(Character Character)
    {
        List<int> equipmentList = Character.equipmentsIDs;
        List<equipment> newequipmentlist = new List<equipment>();
        foreach(int equipmentID in equipmentList)
        {
            if(equipmentID >= 0 && equipmentID<equipmentList.Count)
            {
                newequipmentlist.Add(newequipmentlist[equipmentID]);
            }
        }
        Character.equipments = newequipmentlist;
    }
}
