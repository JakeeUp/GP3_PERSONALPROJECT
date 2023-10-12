using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectHeight : MonoBehaviour
{
    public Transform targetTransform;
    Transform mTransform;
    Transform parent;

    // Start is called before the first frame update
    void Start()
    {
        mTransform = this.transform;
        parent = mTransform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 r = parent.InverseTransformPoint(targetTransform.position);
        r.x = mTransform.localPosition.x;
        r.z = mTransform.localPosition.z;
        mTransform.localPosition = r;
    }
}
