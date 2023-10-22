using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Cardboard Box")]
public class CardboardBox : PassiveItem
{
	public override void OnEquip(Controller controller)
	{
		Debug.Log("BOX ON");
		GameObject go = Instantiate(prefab) as GameObject;
		go.transform.parent = controller.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		controller.storedObject = go;
		controller.animator.gameObject.SetActive(false);
		controller.controllerState = Controller.ControllerState.cardboardBox;
		controller.boxAnimator = go.GetComponentInChildren<Animator>();
	}

	public override void OnUnEquip(Controller controller)
	{
		
		if (controller.storedObject != null)
		{
			Destroy(controller.storedObject);
		}

		controller.animator.gameObject.SetActive(true);
		controller.controllerState = Controller.ControllerState.normal;
		Debug.Log("BOX OFF");
	}
}