using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Jacob.Utilities;



public class ItemSlot : MonoBehaviour
{
	public Image img;
	public Item targetItem;

	private void OnEnable()
	{
		if (UIManager.singleton == null || targetItem == null)
		{
			Debug.LogError("UIManager or targetItem is null");
			return;
		}

		bool isValid = UIManager.singleton.isInInventory(targetItem);

		if (targetItem != null)
		{
			if (!isValid)
			{
				this.gameObject.SetActive(false);
			}
		}
		else
		{
			this.gameObject.SetActive(false);
		}
	}

	public void LoadItem(Item targetItem)
	{

		this.targetItem = targetItem;

		if (targetItem.inventoryIcon == null)
		{
			IconMaker.RequestIcon(targetItem, LoadIcon);
		}
		else
		{
			LoadIcon();
		}
	}

	void LoadIcon()
	{
		img.sprite = targetItem.inventoryIcon;
	}
}

