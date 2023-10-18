using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class InventoryManager : MonoBehaviour
{
	public Transform rightHand;

	public WeaponItem currentWeapon
	{
		get
		{
			if (currentWeaponHook == null)
				return null;

			return currentWeaponHook.baseItem;
		}
	}
	public WeaponHook currentWeaponHook;

	public List<WeaponItem> allWeapons;
	Dictionary<WeaponItem, WeaponHook> weaponsDict = new Dictionary<WeaponItem, WeaponHook>();

	private void Start()
	{
		LoadWeapon(allWeapons[0]);
	}

	public void SwitchWeapon()
	{
		int index = 0;
		if (allWeapons.Contains(currentWeapon))
		{
			index = allWeapons.IndexOf(currentWeapon);
		}

		index++;
		if (index > allWeapons.Count - 1)
		{
			index = 0;
		}

		LoadWeapon(allWeapons[index]);
	}

	public void LoadWeapon(WeaponItem weaponItem)
	{
		if (currentWeaponHook != null)
		{
			currentWeaponHook.gameObject.SetActive(false);
		}

		if (weaponsDict.ContainsKey(weaponItem))
		{
			weaponsDict.TryGetValue(weaponItem, out currentWeaponHook);
		}
		else
		{
			GameObject go = Instantiate(weaponItem.prefab);
			go.transform.parent = rightHand;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			currentWeaponHook = go.GetComponentInChildren<WeaponHook>();
			currentWeaponHook.Init(weaponItem);
			weaponsDict.Add(weaponItem, currentWeaponHook);
		}
	}
}