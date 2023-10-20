using System.Collections;
using UnityEngine;

namespace Jacob.Utilities
{
	[CreateAssetMenu(menuName = "Utilities/Icon Maker")]
	public class IconMakerAsset : ScriptableObject
	{
		public GameObject iconMakerGameobject;
		public SpriteMeshType spriteMeshType;
		public int renderLayer = 23;
		public RenderTexture renderTexture;

		public void RequestIcon(IIcon targetObject)
		{
			GameObject go = Instantiate(iconMakerGameobject);
			IconMakerActual iconMakerActual = go.GetComponentInChildren<IconMakerActual>();
			iconMakerActual.CreateIcon(targetObject, this);
		}
	}

	public static class IconMaker
	{
		static IconMakerAsset _iconMakerAsset;
		public static void RequestIcon(IIcon targetObject)
		{
			if (_iconMakerAsset == null)
			{
				_iconMakerAsset = Resources.Load("IconMakerAsset") as IconMakerAsset;
			}

			_iconMakerAsset.RequestIcon(targetObject);
		}
	}
}
