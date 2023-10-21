using System.Collections;
using System.Collections.Generic;
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
		
		public void RequestIconForList(List<IIcon> l, IconMaker.OnIconComplete callback = null)
        {
			GameObject go = Instantiate(iconMakerGameobject);
			IconMakerActual iconMakerActual = go.GetComponentInChildren<IconMakerActual>();
			//iconMakerActual.CreateIconsForList(l, this, callback);
		}

		public void RequestIcon(IIcon targetObject, IconMaker.OnIconComplete callback = null)
		{
			GameObject go = Instantiate(iconMakerGameobject);
			IconMakerActual iconMakerActual = go.GetComponentInChildren<IconMakerActual>();
			iconMakerActual.CreateIcon(targetObject, this,callback);
		}
	}

	public static class IconMaker
	{
		static IconMakerAsset _iconMakerAsset;
		public delegate void OnIconComplete();

		public static void RequestIcon(List<IIcon> targetList , OnIconComplete iconCompleteCallback)
		{
			if (_iconMakerAsset == null)
			{
				_iconMakerAsset = Resources.Load("IconMakerAsset") as IconMakerAsset;
			}

			_iconMakerAsset.RequestIconForList(targetList, iconCompleteCallback);
		}


		public static void RequestIcon(IIcon targetObject, OnIconComplete iconCompleteCallback)
		{
			if (_iconMakerAsset == null)
			{
				_iconMakerAsset = Resources.Load("IconMakerAsset") as IconMakerAsset;
			}

			_iconMakerAsset.RequestIcon(targetObject, iconCompleteCallback);
		}

		public static void RequestIcon(IIcon targetObject)
        {
			if(_iconMakerAsset == null)
            {
				_iconMakerAsset = Resources.Load("IconMakerAsset") as IconMakerAsset;
            }

			_iconMakerAsset.RequestIcon(targetObject);
        }
	}
}
