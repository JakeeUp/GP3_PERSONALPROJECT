using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon Item")]
public class WeaponItem : Item
{
	public GameObject prefab;
	public int magazineAmmo = 40;
	public int damageValue = 20;
	public float fireRate = 0.1f;
	public float weaponSpread = .2f;
	public bool canMoveWithWeapon;
}