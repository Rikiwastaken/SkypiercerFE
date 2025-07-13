using TMPro;
using UnityEngine;
using static UnitScript;
using static UnityEngine.GraphicsBuffer;

public class ItemActionsMenuScript : MonoBehaviour
{
    public int slotID;

    public Character character;
    public void UseOrEquip()
    {
        if (character.equipments[slotID].Name!="" && character.equipments[slotID].Name != null)
        {
            if (character.equipments[slotID].type != "item")
            {
                equipment selectedequipment = character.equipments[slotID];
                if (slotID!=0)
                {
                    for (int i = slotID; i > 0; i--)
                    {
                        character.equipments[i] = character.equipments[i - 1];
                    }
                    character.equipments[0] = selectedequipment;
                    FindAnyObjectByType<ItemsScript>().InitializeButtons();
                }
            }
            else
            {
                //code items
            }
        }
    }

}
