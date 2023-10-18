using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerEnableCamera : MonoBehaviour
{
	public GameObject targetCamera;

	private void Start()
	{
		targetCamera.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		Controller c = other.GetComponentInParent<Controller>();
		if (c != null)
		{
			targetCamera.SetActive(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Controller c = other.GetComponentInParent<Controller>();
		if (c != null)
		{
			targetCamera.SetActive(false);
		}
	}
}