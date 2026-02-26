using UnityEngine;
using static UnitScript;

public class BattleCharacterScript : MonoBehaviour
{

    public void InitializeModels()
    {
        foreach (ModelInfo modelinfo in GetComponent<UnitScript>().ModelList)
        {
            modelinfo.wholeModel.SetActive(false);
            modelinfo.active = false;
        }
    }

    public void ActivateModel(int ID)
    {
        foreach (ModelInfo modelinfo in GetComponent<UnitScript>().ModelList)
        {
            if (modelinfo.ID == ID)
            {
                if (!modelinfo.wholeModel.activeSelf)
                {
                    modelinfo.wholeModel.SetActive(true);
                }
                modelinfo.active = true;

            }
            else
            {
                if (modelinfo.wholeModel.activeSelf)
                {
                    modelinfo.wholeModel.SetActive(false);
                }
            }


        }
    }

}
