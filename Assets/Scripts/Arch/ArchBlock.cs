using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcController)/*, typeof(ArchCollider), typeof(ArchContacter)*/)]
public class ArchBlock : MonoBehaviour
{
    public const float  BlockThicknessF = 0.2f;
    public const double BlockThicknessD = 0.2d;

    public const float BlockDestroyLifeTime = 3.0f;

    public const float BlockDestroyForce_Dir  = 2.5f;
    public const float BlockDestroyForce_Rand = 1.75f;
    public const float BlockDestroyForce_Up   = 6.0f;

    public const float DefaultBlockDegree = 45.0f;

    [SerializeField] private SpriteRenderer _Renderer;
    [SerializeField] private ArcController _Controller;
    [SerializeField] private Rigidbody2D _Rigidbody;

    public ArcController ArchController => _Controller;

    [Space()]
    public ArcBuilder ArchBuilder;
    
    // ========== Obsolete ========== //
    // public ArchCollider ArchCollider;
    // public ArchContacter ArchContacter;
    // ========== Obsolete ========== //

    public float  RotationZ
    {
        get => transform.rotation.eulerAngles.z;
        set
        {
            if (Mathf.Abs(value) > 360f)
            {
                value %= 360f;
            }
            transform.rotation = Quaternion.Euler(0, 0, _RotationZ = value);
        }
    }
    private float _RotationZ;

    public float Ratio
    {
        get => _Controller.Ratio;
        set
        {
            _Controller.Ratio = value;
            ArchUpdate();
        }
    }
    [Range( 0f, 1f)] public float InitRatio;
    [Range(-1f, 1f)] public float RatioOffset;
    public float DefaultRotationZ;

    [SerializeField, Header("Friend Property")]
    private ArchBlock[] _FriendBlocks;

    private void Reset()
    {
        TryGetComponent(out _Controller);
        // ========== Obsolete ========== //
        // TryGetComponent(out ArchCollider);
        // TryGetComponent(out ArchContacter);
        // ========== Obsolete ========== //
    }
    public void DeltaRotationZ(float delta)
    {
        if (Mathf.Abs(delta += _RotationZ) > 360f)
        {
            delta %= 360f;
        }
        transform.rotation = Quaternion.Euler(0, 0, _RotationZ = delta);
    }
    public void ArchUpdate()
    {
        // ========== Obsolete ========== ========== Obsolete ========== ========== Obsolete ========== //
        // float maxR = _Controller.MaxRadius * 0.5f;
        // float minR = _Controller.MinRadius - _Controller.BetweenMaskRadius;
        // ArchCollider.Original_OutsideRadius = Mathf.Lerp(maxR, minR, _Controller.Ratio);
        // ArchCollider.Original_InsideRadius = ArchCollider.Original_OutsideRadius - _Controller.BetweenMaskRadius * 0.5f;
        // 
        // ArchContacter.ContactPointUpdate();
        // ========== Obsolete ========== ========== Obsolete ========== ========== Obsolete ========== //
        _Renderer.sortingOrder = (int)(_Controller.Ratio * 10);
    }
    public bool CanMoveLower(float lowerLevel)
    {
        return _FriendBlocks.All(o => Mathf.Abs(o.Ratio - lowerLevel) <= BlockThicknessF);
    }
    // ========== Obsolete ========== //
    // public bool IsContactOnOtherArch(ArchBlock other)
    // {
    //     bool Contact(Vector2[] contactPoints, ArchCollider otherCollider)
    //     {
    //         for (int i = 0; i < contactPoints.Length; ++i)
    //         {
    //             if (otherCollider.IsInArch(contactPoints[i]))
    //             {
    //                 return true;
    //             }
    //         }
    //         return false;
    //     }
    //     return Contact(other.ArchContacter.ArchContactPoint, ArchCollider)
    //         || Contact(ArchContacter.ArchContactPoint, other.ArchCollider);
    // }
    // ========== Obsolete ========== //
    public void Release(float time = RotationAxis.BlockReleaseTime)
    {
        StartCoroutine(ReleaseRoutine(time));
    }
    public void Destroy()
    {
        _Rigidbody.simulated = true;

        var angle = (RotationZ - 90f) * Mathf.Deg2Rad;

        var force = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * BlockDestroyForce_Dir;
            force += Random.onUnitSphere * BlockDestroyForce_Rand;
            force += Vector3.up          * BlockDestroyForce_Up;

        _Rigidbody.AddRelativeForce(force, ForceMode2D.Impulse);
    }
    public void DestroyEnd()
    {
        _Controller.Ratio = 0.0f;
        _Renderer.sortingOrder = 0;

        transform.position = RotationAxis.WorldPosition;
        transform.rotation = Quaternion.identity;

        _Rigidbody.velocity = Vector2.zero;

        ArchBlockPool.Instance.Add(this);
    }
    private IEnumerator ReleaseRoutine(float time)
    {
        float minRadius = _Controller.MinRadius;
        float maskRadius = _Controller.BetweenMaskRadius;

        float Lerp(float a, float b, float t)
        {
            return a * (1 - t) + b * t;
        }
        var curve = ArchBlockManager.Instance.ArchBlockReleaseCurve;

        for (float i = 0f; i < time; i += Time.deltaTime * Time.timeScale)
        {
            float ratio = Mathf.Clamp(i / time, 0f, 1f);
            float value = curve.Evaluate(ratio);

            _Controller.MinRadius = Lerp(minRadius, 0f, value);
            _Controller.BetweenMaskRadius = Lerp(maskRadius, 0f, value);

            yield return null;
        }
        _Controller.MinRadius = minRadius;
        _Controller.BetweenMaskRadius = maskRadius;

        ArchBlockPool.Instance.Add(this);
    }
    private void OnEnable()
    {
        _Controller.Ratio = InitRatio;
        RotationZ = DefaultRotationZ;

        _Rigidbody.simulated = false;
    }
}
