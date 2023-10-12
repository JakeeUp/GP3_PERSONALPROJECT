using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShootable
{
	float currentHealth { get; }
	void OnHit(float damage);
	string GetHitFx();
}