using UnityEngine;

public class Unit3DModelInfoScript : MonoBehaviour
{
    public Transform handbone;
    public Transform Lefthandbone;
    public Vector3 rotationadjust;
    public Vector3 positionadjust;
    public Vector3 scaleadjust = Vector3.one;

    public Vector3 weaponrotationajust;
    public Vector3 weaponpositionajust;
    public Vector3 weaponscameajust = Vector3.one;
    public Vector3 leftweaponrotationajust;
    public Vector3 leftweaponpositionajust;

    [SerializeField] private Texture2D BaseTexture;
    [SerializeField] private Texture2D MetallicTexture;
    [SerializeField] private Texture2D RoughnessTexture;
    [SerializeField] private Texture2D EmissionTexture;
    [SerializeField] private Texture2D DefaultBaseTexture;
    [SerializeField] private Texture2D DefaultMetalTexture;
    [SerializeField] private Texture2D DefaultRougnhess;
    [SerializeField] private Texture2D DefaultEmission;
    [SerializeField] private Material BaseMaterial;

    public Material GenerateMaterial()
    {
        Material newmat = new Material(BaseMaterial);
        if (BaseTexture != null)
        {
            newmat.SetTexture("_BaseMap", BaseTexture);
        }
        else
        {
            newmat.SetTexture("_BaseMap", DefaultRougnhess);
        }
        if (MetallicTexture != null)
        {
            newmat.SetTexture("_MetallicMap", MetallicTexture);
        }
        else
        {
            newmat.SetTexture("_MetallicMap", DefaultMetalTexture);
        }
        if (RoughnessTexture != null)
        {
            newmat.SetTexture("_RougnessMap", RoughnessTexture);
        }
        else
        {
            newmat.SetTexture("_RougnessMap", DefaultRougnhess);
        }
        if (EmissionTexture != null)
        {
            newmat.SetTexture("_EmissionMap", EmissionTexture);
        }
        else
        {
            newmat.SetTexture("_EmissionMap", DefaultEmission);
        }
        return newmat;
    }

}
