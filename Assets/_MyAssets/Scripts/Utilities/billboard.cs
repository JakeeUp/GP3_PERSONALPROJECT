using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class billboard : MonoBehaviour
{
    public static Transform cam;
    public Vector3 freeRotation = Vector3.one;
    Vector3 eangles = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if(billboard.cam == null)
        {
            cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        }
    }
    private void LateUpdate()
    {
        this.transform.LookAt(billboard.cam);
        transform.Rotate(0, 180, 0);
        eangles = transform.eulerAngles;
        eangles.x *= freeRotation.x;
        eangles.y *= freeRotation.y;
        eangles.z *= freeRotation.z;
        transform.eulerAngles = eangles;
    }
    
}
