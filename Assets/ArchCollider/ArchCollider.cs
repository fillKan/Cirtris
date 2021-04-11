using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchCollider : MonoBehaviour
{
    public float RotationZ 
    {
        get
        {
            return transform.rotation.eulerAngles.z;
        }
    }


    [Space()]
    [Range(0f, 360f)] public float Degree;
    [Range(0f, 720f)] public float Rotation;

    [Space()]
    public float Original_InsideRadius;
    public float Original_OutsideRadius;

    [HideInInspector] public float Offset_InsideRadius;
    [HideInInspector] public float Offset_OutsideRadius;

    public float OutsideRadius
    { get => Original_OutsideRadius + Offset_InsideRadius; }
    public float InsideRadius
    { get => Original_InsideRadius + Offset_InsideRadius; }

    public bool IsInArch(Vector2 point)
    {
        float distance =
            Vector2.Distance(point, transform.position);

        if (distance > InsideRadius && distance < OutsideRadius)
        {
            Vector3 dir = transform.InverseTransformVector(point).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            float rotation = Rotation * 0.5f + RotationZ;
                  rotation = rotation > 180 ? rotation % 180 : rotation;

            return angle < Degree * +0.5f - rotation
                && angle > Degree * -0.5f - rotation;
        }
        return false;
    }
}