﻿using System.Collections;
using UnityEngine;


public class WeaponHook : MonoBehaviour
{
	public int currentAmmo;
	public int allAmmo = 40;
	[HideInInspector]
	public WeaponItem baseItem;
	ParticleSystem[] particles;
	public Transform bulletEmmiter;

	public void Init(WeaponItem weaponItem)
	{
		particles = GetComponentsInChildren<ParticleSystem>();
		baseItem = weaponItem;
		currentAmmo = baseItem.magazineAmmo;
	}

	public void Shoot()
	{
		Debug.Log("!");
		if (particles != null)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Play();
			}
		}

		currentAmmo--;

		Collider[] colliders = Physics.OverlapSphere(transform.position, 100, GameReferences.controllersLayer);
		for (int i = 0; i < colliders.Length; i++)
		{
			AIController aIController = colliders[i].transform.GetComponentInParent<AIController>();
			if (aIController != null)
			{
				aIController.UpdateLastKnowPosition(transform.position);
			}
		}
	}

	public void Reload()
	{
		if (allAmmo <= baseItem.magazineAmmo)
		{
			currentAmmo = allAmmo;
			allAmmo = 0;
		}
		else
		{
			currentAmmo = baseItem.magazineAmmo;
			allAmmo -= baseItem.magazineAmmo;
		}
	}
}
