using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameReferences
{
    public static LayerMask ignoreForShooting;
    public static LayerMask controllersLayer;
	static ObjectPooler _objectPooler;
    public static float damage;
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

    public static void RaycastShoot(Transform mTransform, WeaponHook weaponHook)
    {
        if (objectPooler == null)
        {
            Debug.LogError("Object pooler is not properly initialized in GameReferences.");
            return;
        }

        RaycastHit hit;
        Vector3 origin = Random.insideUnitCircle * weaponHook.baseItem.weaponSpread;
        origin = mTransform.TransformPoint(origin);
        origin.y += 1.3f;
        origin += mTransform.forward;
       
        Vector3 endPosition = origin + mTransform.forward * 100;

        if (Physics.Raycast(origin, mTransform.forward, out hit, 100, ignoreForShooting))
        {
            IShootable shootable = hit.transform.GetComponentInParent<IShootable>();
            if (shootable != null)
            {
                GameObject fx = GameReferences.objectPooler.GetObject(shootable.GetHitFx());
                if (fx != null) // Check if the particle effect is not null
                {
                    fx.transform.position = hit.point;
                    fx.transform.rotation = Quaternion.LookRotation(hit.normal);
                    fx.SetActive(true);

                    // Only apply damage if the particle effect is successfully triggered
                    shootable.OnHit(damage);
                    //shootable.OnHit(10f);// Replace 10f with the actual damage value
                }
            }
            else
            {
                GameObject fx = GameReferences.objectPooler.GetObject("default");
                if (fx != null) // Check if the particle effect is not null
                {
                    fx.transform.position = hit.point;
                    fx.transform.rotation = Quaternion.LookRotation(hit.normal);
                    fx.SetActive(true);
                }
            }
            endPosition = hit.point;
        }

        GameObject go = objectPooler.GetObject("bulletLine");
        LineRenderer line = go.GetComponent<LineRenderer>();
        line.SetPosition(0, weaponHook.bulletEmmiter.position);
        line.SetPosition(1, endPosition);
        go.SetActive(true);
    }

}