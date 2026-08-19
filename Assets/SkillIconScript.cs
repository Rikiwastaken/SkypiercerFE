using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillIconScript : MonoBehaviour
{

    public Image OuterCircle;
    public Image InnerCircle;
    public Image MainLogo;

    public List<Color> OuterColors;
    public List<Color> InnerColors;
    public List<Sprite> Logos;

    [Serializable]
    public class ImageIconInfo
    {
        [Header("Couleurs:\n0: Offensif, 1: Défensif, 2: Soin, 3: Mobilité,\n4: Command, 5: Météo, 6: Type d'ennemi, 7: Armes, 8:Progression")]
        public int OuterCircleColorID;
        [Header("Couleurs:\n0: Physique, 1: Psychique, 2: Soin, 3: Mobilité,\n4: Esquive, 5: Météo, 6: Status, 7: Armes, 8:Progression, 9:Chance")]
        public int InnerCircleColorID;
        [Header("IDs:\n0: Swr, 1: Spr, 2: GS, 3: Bow, 4: Scy, 5:Shld, 6: Stf, 7: Dggr,\n 8: Pluit, 9: Soleil, 10: Nuage, 11: Zone,\n12: Pluvials, 13: Machines, 14: Barehands, 15: Telek, 16: Copy, 17: Heart,\n18: Acc, 19: Brun, 20: Concuss, 21: Para, 22: Power,\n23: Regen, 24: Stun, 25: Weakness")]
        public int MainLogoID;

    }

    public void InitializeIcon(ImageIconInfo info)
    {
        OuterCircle.color = OuterColors[info.OuterCircleColorID];
        InnerCircle.color = InnerColors[info.OuterCircleColorID];
        MainLogo.color = Color.white;
        MainLogo.sprite = Logos[info.MainLogoID];
    }

    public void DisableIcon()
    {
        Color emptycolor = Color.white;
        emptycolor.a = 0;
        OuterCircle.color = emptycolor;
        InnerCircle.color = emptycolor;
        MainLogo.color = emptycolor;
    }

#if UNITY_EDITOR

    [ContextMenu("Initialize Colors")]

    private void InitializeColor()
    {
        for (int i = 0; i < OuterColors.Count; i++)
        {
            OuterColors[i] = RemplaceColorByCorrectAlpha(OuterColors[i]);
        }
        for (int i = 0; i < InnerColors.Count; i++)
        {
            InnerColors[i] = RemplaceColorByCorrectAlpha(InnerColors[i]);
        }
    }
    private Color RemplaceColorByCorrectAlpha(Color initialcolor)
    {
        initialcolor.a = 1f;
        return initialcolor;
    }

#endif
}
