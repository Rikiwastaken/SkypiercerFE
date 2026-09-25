
using System.Collections.Generic;
using UnityEngine;
using static UnitScript;

public class WeaponModelPositionFixer : MonoBehaviour
{
    public int ZackModelID = 3;
    public List<int> IDsToExclude;
    public Vector3 WeaponPosition;
    public Vector3 WeaponRotation;

#if UNITY_EDITOR

    [ContextMenu("Get Weapon Position")]
    void GetWeaponPosition()
    {
        UnitScript US = GetComponent<UnitScript>();

        GameObject ZackModel = US.ModelList[ZackModelID].wholeModel;

        WeaponPosition = ZackModel.GetComponent<Unit3DModelInfoScript>().weaponpositionajust;
        WeaponRotation = ZackModel.GetComponent<Unit3DModelInfoScript>().weaponrotationajust;
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }

    }

    [ContextMenu("Set Weapon Position")]
    void CalculateIDs()
    {
        UnitScript US = GetComponent<UnitScript>();
        if (Application.isPlaying)
        {
            Debug.Log("Application playing, changing only local variables");

            Unit3DModelInfoScript _Unit3DModelInfoScript = US.ActiveModel.GetComponent<Unit3DModelInfoScript>();
            _Unit3DModelInfoScript.weaponpositionajust = WeaponPosition;
            _Unit3DModelInfoScript.weaponrotationajust = WeaponRotation;
        }
        else
        {
            foreach (UnitScript.ModelInfo ModelInfo in US.ModelList)
            {
                if (IDsToExclude.Contains(ModelInfo.ID))
                {
                    continue;
                }
                Unit3DModelInfoScript _Unit3DModelInfoScript = ModelInfo.wholeModel.GetComponent<Unit3DModelInfoScript>();
                _Unit3DModelInfoScript.weaponpositionajust = WeaponPosition;
                _Unit3DModelInfoScript.weaponrotationajust = WeaponRotation;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }



    }



#endif
}
