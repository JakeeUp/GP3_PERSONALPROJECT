using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectHeight : MonoBehaviour
{
    public Transform targetTransform;
    Transform mTransform;
    Transform parent;
    Controller controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller>();
        mTransform = transform;
        if (controller != null)
        {
            parent = controller.transform.parent;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (parent != null && mTransform != null)
        {
            Vector3 r = parent.InverseTransformPoint(targetTransform.position);
            r.x = mTransform.localPosition.x;
            r.z = mTransform.localPosition.z;
            mTransform.localPosition = r;
        }
    }
}
