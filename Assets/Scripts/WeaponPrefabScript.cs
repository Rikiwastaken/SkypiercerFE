using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPrefabScript : MonoBehaviour
{
    [Serializable]
    public struct weaponVisuals
    {
        public string weaponType;
        public List<Material> materials;
        public GameObject weaponGO;
        public Color WeaponEffectColor;
    }


    [Header("Weapon Gameobjects")]

    public List<weaponVisuals> WeaponVisualsList;

    [Header("Variables")]
    public float timetoappear;
    public float timetovanish;
    private UnitScript UnitScript;
    private weaponVisuals weapontypeequiped;

    private Coroutine AppearCoroutine;
    private weaponVisuals CurrentlyAppearing;
    private string previousweapon;

    [Header("Placement Variables")]
    public Vector3 positionoffset;
    public Vector3 rotationoffset;

    private void Awake()
    {
        for (int i = 0; i < WeaponVisualsList.Count; i++)
        {
            weaponVisuals Vis = WeaponVisualsList[i];
            GameObject CurrentGo = Vis.weaponGO;
            Vis.materials = new List<Material>();
            SetMaterialsToList(Vis.materials, CurrentGo);
            SetOutlineColor(Vis);
        }
        UnitScript = GetComponentInParent<UnitScript>();
    }


    // Update is called once per frame
    void Update()
    {
        if (UnitScript == null)
        {

            UnitScript = GetComponentInParent<UnitScript>();
            return;
        }

        if (UnitScript.GetFirstWeapon().type != previousweapon)
        {
            previousweapon = UnitScript.GetFirstWeapon().type;
            EquipWeaponVisuals(previousweapon);
        }


    }
    // this function is used to place the weapon on the unit, it takes the weapon type and the parent transform as parameters, it then finds the corresponding weapon gameobject and sets its parent and local position and rotation based on the offset variables
    public void PlaceWeapon(string type, Transform parent)
    {
        foreach (weaponVisuals weapon in WeaponVisualsList)
        {
            if (weapon.weaponType == type.ToLower())
            {
                GameObject weaponGO = weapon.weaponGO;
                weaponGO.transform.parent = parent;
                weaponGO.transform.SetLocalPositionAndRotation(positionoffset, Quaternion.Euler(rotationoffset));
                break;
            }
        }
    }
    // this function is used to get the weapon gameobject based on the type, it returns null if it fails
    public GameObject GetWeaponGO(string type)
    {
        foreach (weaponVisuals weaponVisuals in WeaponVisualsList)
        {
            if (weaponVisuals.weaponType == type)
            {
                return weaponVisuals.weaponGO;
            }
        }
        return null;
    }
    // this function is used to equip the weapon visuals, it checks if the new weapon type is different from the currently equiped one, if it is it will start the vanish coroutine for the currently equiped weapon and then start the appear coroutine for the new weapon
    public void EquipWeaponVisuals(string newweapontype)
    {
        if (newweapontype != weapontypeequiped.weaponType)
        {
            if (weapontypeequiped.weaponGO != null)
            {
                StartCoroutine(MakeWeaponVanish(weapontypeequiped));
            }



            foreach (weaponVisuals weaponVisuals in WeaponVisualsList)
            {
                if (weaponVisuals.weaponType == newweapontype)
                {
                    weapontypeequiped = weaponVisuals;
                    break;
                }
            }

            if (weapontypeequiped.weaponGO != null)
            {
                weapontypeequiped.weaponGO.SetActive(true);

                SetDissolve(weapontypeequiped, 0);


                if (AppearCoroutine != null)
                {
                    StopCoroutine(AppearCoroutine);
                }

                AppearCoroutine = StartCoroutine(MakeWeaponAppear(weapontypeequiped));
            }
        }
    }
    // this coroutine is used to make the weapon appear, it lerps the dissolve value from 1 to 0, if during the process the weapon to appear is not the same as the currently appearing weapon, it will stop lerping and just set the dissolve value to 0
    private IEnumerator MakeWeaponAppear(weaponVisuals Weapon)
    {
        CurrentlyAppearing = Weapon;
        float elapsedTime = 0f;
        while (elapsedTime < timetoappear)
        {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(0f, 1.1f, elapsedTime / timetoappear);

            SetDissolve(Weapon, lerpedDissolve);

            yield return null;
        }
        AppearCoroutine = null;
    }
    // this coroutine is used to make the weapon vanish, it lerps the dissolve value from 0 to 1 and then deactivates the gameobject, if during the process the weapon to vanish is not the same as the currently appearing weapon, it will stop lerping and just deactivate the gameobject
    private IEnumerator MakeWeaponVanish(weaponVisuals Weapon)
    {
        float elapsedTime = 0f;
        while (elapsedTime < timetovanish)
        {

            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(0.9f, 0f, elapsedTime / timetovanish);
            if (CurrentlyAppearing.weaponGO != Weapon.weaponGO)
            {
                SetDissolve(Weapon, lerpedDissolve);
            }


            yield return null;
        }
        if (CurrentlyAppearing.weaponGO != Weapon.weaponGO)
        {
            Weapon.weaponGO.SetActive(false);
        }
    }

    // sets the dissolve value for all materials in the list
    private void SetDissolve(weaponVisuals weapon, float dissolve)
    {
        if ((weapon.materials == null || weapon.materials.Count == 0) && weapon.weaponType != "")
        {
            SetMaterialsToList(weapon.materials, weapon.weaponGO);
        }
        foreach (Material mat in weapon.materials)
        {
            mat.SetFloat("_DissolvePercent", dissolve);
        }
    }

    private void SetOutlineColor(weaponVisuals weapon)
    {
        if ((weapon.materials == null || weapon.materials.Count == 0) && weapon.weaponType != "")
        {
            SetMaterialsToList(weapon.materials, weapon.weaponGO);
        }
        foreach (Material mat in weapon.materials)
        {
            mat.SetColor("_OutlineColor", weapon.WeaponEffectColor);
        }
    }

    // gets the dissolve value for the first material in the list, if it fails it returns 0  
    private float GetDissolve(weaponVisuals weapon, float dissolve)
    {

        try
        {
            if (weapon.materials == null || weapon.materials.Count == 0)
            {
                SetMaterialsToList(weapon.materials, weapon.weaponGO);
            }
            return weapon.materials[0].GetFloat("_DissolvePercent");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return 0f;
        }

    }
    //recursively adds material to the list
    private void SetMaterialsToList(List<Material> listotuse, GameObject GOtouse)
    {
        // if object has renderer, add material to list
        if (GOtouse.GetComponent<Renderer>() != null && !listotuse.Contains(GOtouse.GetComponent<Renderer>().material))
        {
            listotuse.Add(GOtouse.GetComponent<Renderer>().material);
        }

        // then for each child we do the same.

        foreach (Transform child in GOtouse.transform)
        {
            SetMaterialsToList(listotuse, child.gameObject);
        }
    }

}
