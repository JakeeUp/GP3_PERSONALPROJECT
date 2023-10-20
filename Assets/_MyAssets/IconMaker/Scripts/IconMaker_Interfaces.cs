using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Jacob.Utilities
{
	public interface IIcon
	{
		GameObject GetObjectForIcon();
		Vector2 GetPivotPosition();
		void IconCreatedCallback(Sprite sprite);

	}
}
