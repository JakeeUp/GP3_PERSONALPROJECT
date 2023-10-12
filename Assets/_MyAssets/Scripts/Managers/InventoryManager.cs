using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class InventoryManager : MonoBehaviour
{
	public Weapon currentWeapon;
}

[System.Serializable]
public class Weapon
{
	public GameObject model;
	public bool canMoveWithWeapon;
	public float fireRate = 0.15f;
	public ParticleSystem muzzle;
	public float weaponSpread = .2f;
}
