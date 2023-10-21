using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager
{
	static Jacob.ManagerAssets.ResourcesManagerAsset _resourcesManager;
	public static Jacob.ManagerAssets.ResourcesManagerAsset singleton
	{
		get
		{
			if (_resourcesManager == null)
			{
				_resourcesManager = Resources.Load("ResourcesManager") as Jacob.ManagerAssets.ResourcesManagerAsset;
			}

			return _resourcesManager;
		}
	}
}
