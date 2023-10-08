using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameReferences
{
	static ObjectPooler _objectPooler;
	public static ObjectPooler objectPooler
	{
		get
		{
			if (_objectPooler == null)
			{
				_objectPooler = Resources.Load("ObjectPooler") as ObjectPooler;
				_objectPooler.Init();
			}

			return _objectPooler;
		}
	}

}