using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItem : MonoBehaviour
{
	public Item targetItem;

    private void Start()
    {

    }
   
    private void OnTriggerEnter(Collider other)
	{
		Controller c = other.GetComponentInParent<Controller>();
		
		if (c != null)
		{
            c.inventoryManager.PickUpItem(targetItem);
			gameObject.SetActive(false);
		}
	}
}