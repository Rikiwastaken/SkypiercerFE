using TMPro;
using UnityEngine;
using static UnitScript;

public class ItemActionsMenuScript : MonoBehaviour
{
    public int slotID;

    public Character character;
    private GridScript GridScript;
    public void UseOrEquip()
    {
        if(GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

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
                    GridSquareScript tile = GridScript.GetTile(character.position);
                    GameObject UnitGO = GridScript.GetUnit(tile);
                    (int range, bool frapperenmelee, string type) = UnitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    GridScript.ShowAttackAfterMovement(range, frapperenmelee, tile,type.ToLower()=="staff");
                    GridScript.lockedattacktiles = GridScript.attacktiles;
                    FindAnyObjectByType<ItemsScript>().InitializeButtons();
                    GridScript.Recolor();
                }
            }
            else
            {
                //code items
            }
        }
    }

}
