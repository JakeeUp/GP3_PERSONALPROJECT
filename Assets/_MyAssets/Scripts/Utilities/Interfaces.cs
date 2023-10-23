using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShootable
{
	public float currentHealth { get; }
	void OnHit(float damage);
	string GetHitFx();
}

public interface IPointOfInterest
{
	bool OnDetect(AIController aIController);
	Transform GetTransform();
}