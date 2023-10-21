using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public InventoryUI leftUI;
	public InventoryUI rightUI;

	public GameObject inventorySlotPrefab;


	public static UIManager singleton;

	private void Awake()
	{
		singleton = this;
	}

	private void Start()
	{
		leftUI.Init(inventorySlotPrefab);
		rightUI.Init(inventorySlotPrefab);
	}

	public bool Tick(float vertical, float delta, bool isLeftActive, bool isRightActive)
	{
		if (isLeftActive)
		{
			leftUI.invGameObject.SetActive(true);
			leftUI.Tick(vertical, delta);
			return true;
		}
		else
		{
			if (leftUI.invGameObject.activeInHierarchy)
			{
				leftUI.invGameObject.SetActive(false);
			}
		}

		if (isRightActive)
		{
			rightUI.invGameObject.SetActive(true);
			rightUI.Tick(vertical, delta);
			return true;
		}
		else
		{
			if (rightUI.invGameObject.activeInHierarchy)
			{
				rightUI.invGameObject.SetActive(false);
			}
		}

		return false;
	}
}

[System.Serializable]
public class InventoryUI
{
	public RectTransform invGrid;
	public GameObject invGameObject;
	[System.NonSerialized]
	List<GameObject> createdItemsLeft = new List<GameObject>();
	GameObject currentObject;
	Vector2 targetYPosition;
	Vector2 startPosition;
	[System.NonSerialized]
	float t;
	[System.NonSerialized]
	float lastChange;

	public void Init(GameObject inventorySlotPrefab)
	{
		for (int i = 0; i < 5; i++)
		{
			GameObject go = GameObject.Instantiate(inventorySlotPrefab);
			go.transform.SetParent(invGrid);
			createdItemsLeft.Add(go);
			go.SetActive(true);
		}

		currentObject = createdItemsLeft[0];
		t = 1;
	}

	public void Tick(float vertical, float delta)
	{
		if (vertical == 1 || vertical == -1)
		{
			if (Time.realtimeSinceStartup - lastChange > 0.5f)
			{
				lastChange = Time.realtimeSinceStartup;

				bool isDown = (vertical < 0);
				int index = createdItemsLeft.IndexOf(currentObject);
				index = (isDown) ? index - 1 : index + 1;
				if (index < 0)
				{
					index = createdItemsLeft.Count - 1;
				}
				if (index > createdItemsLeft.Count - 1)
				{
					index = 0;
				}

				currentObject = createdItemsLeft[index];
				Vector2 position = invGrid.localPosition;
				startPosition = position;
				position.y = (index) * invGrid.GetComponent<GridLayoutGroup>().cellSize.y;
				targetYPosition = position;
				t = 0;
			}
		}

		t += delta * 2;
		if (t > 1)
		{
			t = 1;
		}

		Vector2 targetPosition = Vector2.Lerp(startPosition, targetYPosition, t);
		invGrid.localPosition = targetPosition;
	}
}

