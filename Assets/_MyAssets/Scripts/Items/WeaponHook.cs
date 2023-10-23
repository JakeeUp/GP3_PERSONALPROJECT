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
	public AudioSource gunSoundSource;
	public AudioClip[] gunSound;

    private void Start()
    {
    }
    public void Init(WeaponItem weaponItem)
	{
		particles = GetComponentsInChildren<ParticleSystem>();
		baseItem = weaponItem;
		currentAmmo = baseItem.magazineAmmo;
	}

	public void Shoot()
	{
		Debug.Log("Shoot");
		int randomIndez = Random.Range(0,gunSound.Length);
		gunSoundSource.clip = gunSound[randomIndez];
		gunSoundSource.Play();	
		if (particles != null)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Play();
			}
		}

		currentAmmo--;


		GameReferences.UpdateLastKnownPositionOfCloseby(transform.position, 50);
		
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
