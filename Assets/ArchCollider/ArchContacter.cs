using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArchCollider))][ExecuteInEditMode]
public class ArchContacter : MonoBehaviour
{
    [SerializeField] private ArchCollider _ArchCollider;

    public  Vector2[]  ArchContactPoint
    {
        get { return _ArchContactPoint; }
    }
    private Vector2[] _ArchContactPoint = new Vector2[4];

    private void Reset()
    {
        TryGetComponent(out _ArchCollider);
    }
    private void OnEnable()
    {
        ContactPointUpdate();
    }
    public void ContactPointUpdate()
    {
        float rotationValue = (_ArchCollider.Rotation + _ArchCollider.RotationZ * 2) * Mathf.Deg2Rad;

        for (int i = 0; i < 3; i += 2)
        {
            var rot = _ArchCollider.Degree * 0.5f * (i > 0 ? -1 : 1) * Mathf.Deg2Rad + rotationValue * 0.5f;

            var direction = new Vector3(Mathf.Cos(rot), Mathf.Sin(rot));

            _ArchContactPoint[i + 0] = transform.position + direction * _ArchCollider.InsideRadius;
            _ArchContactPoint[i + 1] = transform.position + direction * _ArchCollider.OutsideRadius;
        }
    }
    private void OnDrawGizmos()
    {
        ContactPointUpdate();
        for (int i = 0; i < _ArchContactPoint.Length; i++)
        {
            Gizmos.DrawIcon(_ArchContactPoint[i], "ParticleSystemForceField Gizmo.png");
        }
    }
}
