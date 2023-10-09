using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLine : MonoBehaviour
{
	public LineRenderer lineRenderer;
	public float speed = .2f;

	private void OnEnable()
	{
		lineRenderer.widthMultiplier = 1;
	}

	private void Update()
	{
		if (lineRenderer.widthMultiplier <= 0)
		{
			gameObject.SetActive(false);
		}
		else
		{
			lineRenderer.widthMultiplier -= Time.deltaTime / speed;
		}
	}
}