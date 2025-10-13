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
        if (GridScript == null)
        {
            GridScript = FindAnyObjectByType<GridScript>();
        }

        if (character.equipments[slotID].Name.ToLower() != "fist" && character.equipments[slotID].Name.ToLower() != "fists" && character.equipments[slotID].Name != "" && character.equipments[slotID].Name != null)
        {
            if (character.equipments[slotID].type != "item")
            {
                equipment selectedequipment = character.equipments[slotID];
                if (slotID != 0)
                {
                    for (int i = slotID; i > 0; i--)
                    {
                        character.equipments[i] = character.equipments[i - 1];
                    }
                    character.equipments[0] = selectedequipment;
                    GameObject UnitGO = GridScript.GetUnit(character.currentTile[0]);
                    UnitGO.GetComponent<UnitScript>().UpdateWeaponModel();
                    (int range, bool frapperenmelee, string type) = UnitGO.GetComponent<UnitScript>().GetRangeMeleeAndType();
                    GridScript.ShowAttackAfterMovement(range, frapperenmelee, character.currentTile, type.ToLower() == "staff", character.enemyStats.monsterStats.size);
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
