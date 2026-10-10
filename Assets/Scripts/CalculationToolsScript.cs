using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataScript;
using static UnitScript;

public class CalculationToolsScript : MonoBehaviour
{

    /// <summary>
    /// Calculated Manhattan distance from 2 Characters using the Tile they're on
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="otherunit"></param>
    public static int ManhattanDistance(Character unit, Character otherunit)
    {
        if (unit == null || otherunit == null)
        {
            return int.MaxValue;
        }
        return ManhattanDistance(unit.currentTile, otherunit.currentTile);
    }

    /// <summary>
    /// Calculated Manhattan distance from 2 Tile using the Grid Coordinates
    /// </summary>
    /// <param name="tile1"></param>
    /// <param name="tile2"></param>
    public static int ManhattanDistance(GridSquareScript tile1, GridSquareScript tile2)
    {
        if (tile1 == null || tile2 == null)
        {
            return int.MaxValue;
        }
        return ManhattanDistance(tile1.GridCoordinates, tile2.GridCoordinates);
    }

    /// <summary>
    /// Calculated Manhattan distance from 2 Vectors
    /// </summary>
    /// <param name="Vector1"></param>
    /// <param name="vector2"></param>
    public static int ManhattanDistance(Vector2 Vector1, Vector2 vector2)
    {
        return (int)(Mathf.Abs(Vector1.x - vector2.x) + Mathf.Abs(Vector1.y - vector2.y));
    }

    /// <summary>
    /// Change Material of Renderers in Object and all of its hierarchy. Also can change the Layer is a LayerMask Name is given
    /// </summary>
    /// <param name="GOtouse"></param>
    /// <param name="Mattoadd"></param>
    ///  <param name="LayerMaskName"></param>
    public static void ChangeMaterialInGOHierarchy(GameObject GOtouse, Material Mattoadd, string LayerMaskName = "")
    {
        // Sets material
        if (GOtouse.GetComponent<Renderer>() != null)
        {
            List<Material> materialstoset = new List<Material>();
            foreach (Material mat in GOtouse.GetComponent<Renderer>().materials)
            {
                materialstoset.Add(Mattoadd);
            }
            GOtouse.GetComponent<Renderer>().SetMaterials(materialstoset);
        }

        // Also sets layer
        if (LayerMaskName != "")
        {
            GOtouse.layer = LayerMask.NameToLayer(LayerMaskName);
        }


        // then for each child we do the same.

        foreach (Transform child in GOtouse.transform)
        {
            ChangeMaterialInGOHierarchy(child.gameObject, Mattoadd, LayerMaskName);
        }
    }
    /// <summary>
    /// Returns First Material found in hierarchy
    /// </summary>
    /// <param name="go"></param>
    public static Material GetMaterial(GameObject go)
    {
        if (go.GetComponent<Renderer>() != null)
        {
            return go.GetComponent<Renderer>().material;
        }
        foreach (Transform child in go.transform)
        {
            Material childmat = GetMaterial(child.gameObject);
            if (childmat != null)
            {
                return childmat;
            }
        }
        return null;
    }


    /// <summary>
    /// Change Layer of Object and all of its hierarchy.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layername"></param>
    public static void UpdateLayer(GameObject obj, string layername)
    {
        LayerMask layer = LayerMask.NameToLayer(layername);

        UpdateLayer(obj, layer);
    }

    /// <summary>
    /// Change Layer of Object and all of its hierarchy.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layer"></param>
    public static void UpdateLayer(GameObject obj, LayerMask layer)
    {

        if (obj == null)
        {
            return;
        }
        obj.layer = layer;
        if (obj.transform.childCount > 0)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                UpdateLayer(obj.transform.GetChild(i).gameObject, layer);
            }
        }
    }

    /// <summary>
    /// Creates a true copy of Character passed in argument
    /// </summary>
    /// <param name="CharacterToCopy"></param>
    public static Character CreateCopy(Character CharacterToCopy)
    {
        Character copy = new Character
        {
            name = CharacterToCopy.name,
            ID = CharacterToCopy.ID,
            stats = new BaseStats
            {
                HP = CharacterToCopy.stats.HP,
                Strength = CharacterToCopy.stats.Strength,
                Psyche = CharacterToCopy.stats.Psyche,
                Defense = CharacterToCopy.stats.Defense,
                Resistance = CharacterToCopy.stats.Resistance,
                Speed = CharacterToCopy.stats.Speed,
                Dexterity = CharacterToCopy.stats.Dexterity,
                Luck = CharacterToCopy.stats.Luck,
            },
            AjustedStats = new BaseStats
            {
                HP = CharacterToCopy.AjustedStats.HP,
                Strength = CharacterToCopy.AjustedStats.Strength,
                Psyche = CharacterToCopy.AjustedStats.Psyche,
                Defense = CharacterToCopy.AjustedStats.Defense,
                Resistance = CharacterToCopy.AjustedStats.Resistance,
                Speed = CharacterToCopy.AjustedStats.Speed,
                Dexterity = CharacterToCopy.AjustedStats.Dexterity,
                Luck = CharacterToCopy.AjustedStats.Luck,
            },
            level = CharacterToCopy.level,
            experience = CharacterToCopy.experience,
            affiliation = CharacterToCopy.affiliation,
            growth = new StatGrowth
            {
                HPGrowth = CharacterToCopy.growth.HPGrowth,
                StrengthGrowth = CharacterToCopy.growth.StrengthGrowth,
                PsycheGrowth = CharacterToCopy.growth.PsycheGrowth,
                DefenseGrowth = CharacterToCopy.growth.DefenseGrowth,
                ResistanceGrowth = CharacterToCopy.growth.ResistanceGrowth,
                SpeedGrowth = CharacterToCopy.growth.SpeedGrowth,
                DexterityGrowth = CharacterToCopy.growth.DexterityGrowth,
                LuckGrowth = CharacterToCopy.growth.LuckGrowth,
            },
            currentHP = CharacterToCopy.currentHP,
            movements = CharacterToCopy.movements,
            alreadyplayed = CharacterToCopy.alreadyplayed,
            alreadymoved = CharacterToCopy.alreadymoved,
            telekinesisactivated = CharacterToCopy.telekinesisactivated,
            equipmentsIDs = new List<int>(CharacterToCopy.equipmentsIDs),
            equipments = CharacterToCopy.equipments,
            UnitSkill = CharacterToCopy.UnitSkill,
            SecondUnitSkill = CharacterToCopy.SecondUnitSkill,
            EquipedSkills = new List<int>(CharacterToCopy.EquipedSkills),
            attacksfriends = CharacterToCopy.attacksfriends,
            PermaDied = CharacterToCopy.PermaDied,
            playableStats = new PlayableStats
            {
                MaxSkillpoints = CharacterToCopy.playableStats.MaxSkillpoints,
                deployunit = CharacterToCopy.playableStats.deployunit,
                unlocked = CharacterToCopy.playableStats.unlocked,
                protagonist = CharacterToCopy.playableStats.protagonist,
                battalion = CharacterToCopy.playableStats.battalion,
                ID = CharacterToCopy.playableStats.ID
            },
            enemyStats = new EnemyStats
            {
                classID = CharacterToCopy.enemyStats.classID,
                desiredlevel = CharacterToCopy.enemyStats.desiredlevel,
                itemtodropID = CharacterToCopy.enemyStats.itemtodropID,
                usetelekinesis = CharacterToCopy.enemyStats.usetelekinesis,
                personality = CharacterToCopy.enemyStats.personality,
                startpos = CharacterToCopy.enemyStats.startpos,
                equipments = new List<int>(CharacterToCopy.enemyStats.equipments),
                Skills = new List<int>(CharacterToCopy.enemyStats.Skills),
                Name = CharacterToCopy.enemyStats.Name,
                bossiD = CharacterToCopy.enemyStats.bossiD,
                isother = CharacterToCopy.enemyStats.isother,
                monsterStats = new MonsterStats
                {
                    ispluvial = CharacterToCopy.enemyStats.monsterStats.ispluvial,
                    ismachine = CharacterToCopy.enemyStats.monsterStats.ismachine
                },
                RemainingLifebars = CharacterToCopy.enemyStats.RemainingLifebars,
                modelID = CharacterToCopy.enemyStats.modelID,
                talkable = CharacterToCopy.enemyStats.talkable,
                talkID = CharacterToCopy.enemyStats.talkID,
                talkedto = CharacterToCopy.enemyStats.talkedto,
                SpriteID = CharacterToCopy.enemyStats.SpriteID,
                CopyID = CharacterToCopy.enemyStats.CopyID,
                PlayableSpriteID = CharacterToCopy.enemyStats.PlayableSpriteID,
            },
            currentTile = CharacterToCopy.currentTile,
            modelID = CharacterToCopy.modelID,
            Masteries = CharacterToCopy.Masteries.Select(m => new WeaponMastery
            {
                Modifier = m.Modifier,
                weapontype = m.weapontype,
                Exp = m.Exp,
                Level = m.Level,
                maxlevel = m.maxlevel
            }).ToList(),
            DialoguePitch = CharacterToCopy.DialoguePitch,
            TauntTurns = CharacterToCopy.TauntTurns,
            isintercepting = CharacterToCopy.isintercepting,
            TemporarySkill = CharacterToCopy.TemporarySkill,
            SecondSkillUnlocked = CharacterToCopy.SecondSkillUnlocked,
            statusEffects = new StatusEffects
            {
                BurnTurns = CharacterToCopy.statusEffects.BurnTurns,
                StunTurns = CharacterToCopy.statusEffects.StunTurns,
                ParalyzedTurns = CharacterToCopy.statusEffects.ParalyzedTurns,
                ConcussionTunrs = CharacterToCopy.statusEffects.ConcussionTunrs,
                WeaknessTurns = CharacterToCopy.statusEffects.WeaknessTurns,
                RegenTurns = CharacterToCopy.statusEffects.RegenTurns,
                AccelerationTurns = CharacterToCopy.statusEffects.AccelerationTurns,
                PowerTurns = CharacterToCopy.statusEffects.PowerTurns
            },
            motivateusedthisturn = CharacterToCopy.motivateusedthisturn,
            previouslyequipedweaponID = CharacterToCopy.previouslyequipedweaponID,
            previousTelekinesis = CharacterToCopy.previousTelekinesis,
            characterDialogues = new CharacterDialogues
            {
                CharacterID = CharacterToCopy.characterDialogues.CharacterID,
                DeathQuote = CharacterToCopy.characterDialogues.DeathQuote,
                DeathReaction_Zack = CharacterToCopy.characterDialogues.DeathReaction_Zack,
                DeathReaction_Kira = CharacterToCopy.characterDialogues.DeathReaction_Kira,
                DeathReaction_Gale = CharacterToCopy.characterDialogues.DeathReaction_Gale,
                GoodLvlUp = CharacterToCopy.characterDialogues.GoodLvlUp,
                MidLvlUp = CharacterToCopy.characterDialogues.MidLvlUp,
                LowLvlUp = CharacterToCopy.characterDialogues.LowLvlUp
            },
            ExamodeClass = new ExamodeClass
            {
                ExamodeTexture = CharacterToCopy.ExamodeClass.ExamodeTexture,
                remaingExamodeTurns = CharacterToCopy.ExamodeClass.remaingExamodeTurns,
                ExamodePoints = CharacterToCopy.ExamodeClass.ExamodePoints,
                Basemat = CharacterToCopy.ExamodeClass.Basemat,
                ExamodeMat = CharacterToCopy.ExamodeClass.ExamodeMat
            },
            totalenemieskilled = CharacterToCopy.totalenemieskilled,
        };

        return copy;
    }

    /// <summary>
    /// Creates a true copy of Invetory Item passed in argument
    /// </summary>
    /// <param name="itemtocopy"></param>
    public static InventoryItem CreateCopy(InventoryItem itemtocopy)
    {
        InventoryItem copy = new InventoryItem();
        copy.Quantity = itemtocopy.Quantity;
        copy.type = itemtocopy.type;
        copy.ID = itemtocopy.ID;
        return copy;
    }

    /// <summary>
    /// Creates a true copy of Bond List passed in argument
    /// </summary>
    /// <param name="BondsList"></param>
    public static List<Bonds> CreateCopy(List<Bonds> BondsList)
    {
        List<Bonds> bondscopy = new List<Bonds>();
        foreach (Bonds bond in BondsList)
        {
            Bonds bondcopy = new Bonds();
            bondcopy.ID = bond.ID;
            bondcopy.Characters = bond.Characters;
            bondcopy.BondDialogueSeen = bond.BondDialogueSeen;
            bondcopy.BondLevel = bond.BondLevel;
            bondcopy.BondPoints = bond.BondPoints;
            bondcopy.MaxLevel = bond.MaxLevel;
            bondscopy.Add(bondcopy);
        }
        return bondscopy;
    }

    /// <summary>
    /// Creates a true copy of equipment passed in argument. If character is also passed, it ajusts the name of the equipment depending the character
    /// </summary>
    /// <param name="equipmenttocopy"></param>
    /// <param name="character"></param>
    public static equipment CreateCopy(equipment equipmenttocopy, Character character = null)
    {
        equipment newequipment = new equipment();
        newequipment.Name = equipmenttocopy.Name;
        newequipment.BaseDamage = equipmenttocopy.BaseDamage;
        newequipment.BaseHit = equipmenttocopy.BaseHit;
        newequipment.BaseCrit = equipmenttocopy.BaseCrit;
        newequipment.Range = equipmenttocopy.Range;
        newequipment.type = equipmenttocopy.type;
        newequipment.Currentuses = equipmenttocopy.Currentuses;
        newequipment.Maxuses = equipmenttocopy.Maxuses;
        newequipment.ID = equipmenttocopy.ID;
        newequipment.Grade = equipmenttocopy.Grade;
        newequipment.equipmentmodel = equipmenttocopy.equipmentmodel;

        if (character != null)
        {
            if (character.name == "Zack")
            {
                switch (newequipment.type.ToLower())
                {
                    case "sword":
                        newequipment.Name = "Swino";
                        break;
                    case "spear":
                        newequipment.Name = "Spino";
                        break;
                    case "greatsword":
                        newequipment.Name = "Grino";
                        break;
                    case "bow":
                        newequipment.Name = "Bino";
                        break;
                    case "scythe":
                        newequipment.Name = "Scino";
                        break;
                    case "shield":
                        newequipment.Name = "Shino";
                        break;
                    case "staff":
                        newequipment.Name = "Stino";
                        break;
                    case "dagger":
                        newequipment.Name = "Dino";
                        break;
                }
                switch (newequipment.Grade)
                {
                    case 2:
                        newequipment.Name += "+";
                        break;
                    case 3:
                        newequipment.Name += "Ex";
                        break;
                    case 4:
                        newequipment.Name += "Ult";
                        break;
                    case 5:
                        newequipment.Name += "Final";
                        break;
                }
            }

            if (character.name == "Kira")
            {
                if (newequipment.type.ToLower() == "sword")
                {
                    newequipment.Name = "Reshine";
                    switch (newequipment.Grade)
                    {
                        case 3:
                            newequipment.Name += "II";
                            break;
                        case 4:
                            newequipment.Name += "III";
                            break;
                        case 5:
                            newequipment.Name += "IV";
                            break;
                    }
                }
            }

            if (character.name == "Gale")
            {
                if (newequipment.type.ToLower() == "greatsword")
                {
                    newequipment.Name = "Abyssal";
                    switch (newequipment.Grade)
                    {
                        case 3:
                            newequipment.Name += " 2";
                            break;
                        case 4:
                            newequipment.Name += " 3";
                            break;
                        case 5:
                            newequipment.Name += " 4";
                            break;
                    }
                }
            }
        }

        return newequipment;
    }

    /// <summary>
    /// Returns string corresponding to weapon type passed in argument
    /// </summary>
    /// <param name="weapontype"></param>

    public static string GetWeaponSpriteString(string weapontype)
    {
        switch (weapontype.ToLower())
        {
            case ("sword"):
                return "<sprite=0>";
            case ("spear"):
                return "<sprite=1>";

            case ("greatsword"):
                return "<sprite=2>";

            case ("bow"):
                return "<sprite=3>";

            case ("scythe"):
                return "<sprite=4>";

            case ("shield"):
                return "<sprite=6>";
            case ("staff"):
                return "<sprite=7>";
            case ("dagger"):
                return "<sprite=27>";
            default:
                return "<sprite=5>";
        }
    }
}
