using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Jacob.Utilities
{
	public class IconMakerActual : MonoBehaviour
	{
		public Camera renderCamera;
		public Transform spawnPosition;

		public void CreateIcon(IIcon targetObject, IconMakerAsset asset)
		{
			StartCoroutine(CreateIconRoutine(targetObject, asset));
		}

		public void CreateIconsForList(List<IIcon> l, IconMakerAsset asset)
		{

		}

		IEnumerator CreateIconsForList_Actual(List<IIcon> l, IconMakerAsset asset)
		{
			int index = l.Count;

			while (l.Count > 0)
			{
				index--;

				yield return (CreateIconRoutine(l[index], asset, !(index > 0)));
				l.Remove(l[index]);
			}
		}

		IEnumerator CreateIconRoutine(IIcon targetObject, IconMakerAsset asset, bool clearReference = true)
		{
			GameObject go = Instantiate(targetObject.GetObjectForIcon(), spawnPosition) as GameObject;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;

			Transform[] transforms = go.GetComponentsInChildren<Transform>();
			foreach (Transform r in transforms)
			{
				r.gameObject.layer = asset.renderLayer;
			}

			renderCamera.targetTexture = asset.renderTexture;
			yield return new WaitForEndOfFrame();
			RenderTexture currentRT = RenderTexture.active;
			renderCamera.targetTexture.Release();
			RenderTexture.active = renderCamera.targetTexture;
			renderCamera.Render();

			Texture2D imgPng = new Texture2D(renderCamera.targetTexture.width, renderCamera.targetTexture.height, TextureFormat.ARGB32, true);
			imgPng.ReadPixels(new Rect(0, 0, renderCamera.targetTexture.width, renderCamera.targetTexture.height), 0, 0);
			imgPng.Apply();
			RenderTexture.active = currentRT;


			Sprite sprite = Sprite.Create(imgPng, new Rect(0, 0, imgPng.width, imgPng.height), targetObject.GetPivotPosition(), 100, 0, asset.spriteMeshType);
			targetObject.IconCreatedCallback(sprite);

			Destroy(go);
			if (clearReference)
				Destroy(this.gameObject);
		}
	}

}
