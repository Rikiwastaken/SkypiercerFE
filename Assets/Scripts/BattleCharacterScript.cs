using UnityEngine;
using static UnitScript;

public class BattleCharacterScript : MonoBehaviour
{

    private void OnEnable()
    {
        foreach(ModelInfo modelinfo in GetComponent<UnitScript>().ModelList)
        {
            modelinfo.wholeModel.SetActive(false);
        }
    }

    public void ActivateModel(int ID)
    {
        foreach (ModelInfo modelinfo in GetComponent<UnitScript>().ModelList)
        {
            if(modelinfo.ID == ID)
            {
                if(!modelinfo.wholeModel.activeSelf)
                {
                    modelinfo.wholeModel.SetActive(true);
                }
                
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
