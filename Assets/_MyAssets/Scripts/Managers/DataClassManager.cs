using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class DataClassManager
{
    Controller pc;

    public float MaxHealth = 100f;
    public float MoveSpeed = .4f;
    public float ProneSpeed = .4f;
    public float WallSpeed = .4f;
    public float RotateSpeed = .2f;
    public float FPSRotateSpeed = .2f;
    public float WallCheckDis = .2f;
    public float AimSpeed = 1;
    public bool IsWall;
    public bool IsAiming;
    public bool IsFreeLook;
    public bool IsProne;

    
}
   

