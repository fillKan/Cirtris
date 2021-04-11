using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ArcController : MonoBehaviour
{
    [SerializeField] private ArcBuilder _ArcBuilder;

    [Header("___Raidus Properties___")]
    [SerializeField, Range(0f, 1f)] private float _RadiusRatio = 0f;

    [SerializeField] private float _MaxRadius; public float MaxRadius { get => _MaxRadius; set => _MaxRadius = value; }
    [SerializeField] private float _MinRadius; public float MinRadius { get => _MinRadius; set => _MinRadius = value; }

    [SerializeField] private float _BetweenMaskRadius;

    [Header("___Other Properties___")]
    [SerializeField, Range(0f, 360f)] private float _ArcDegree;
    [SerializeField, Range(0f, 360f)] private float _Rotation;

    private float _ArcRadius;
    private float _MaskRadius;

    public float ArcRadius
    { get => _ArcRadius; }
    public float MaskRadius
    { get => _MaskRadius; }
    public float ArcDegree
    { 
        get => _ArcDegree;
        set => _ArcDegree = value;
    }
    public float Rotation
    { get => _Rotation; }
    public float BetweenMaskRadius
    {
        get => _BetweenMaskRadius;
        set => _BetweenMaskRadius = value;
    }
    public float Ratio
    { 
        get => _RadiusRatio;
        set => _RadiusRatio = value;
    }
    private void Reset()
    {
        if (TryGetComponent(out _ArcBuilder))
        {
            _ArcDegree = _ArcBuilder.GetDegree();
        }
    }
    private void Update()
    {
        float between = _BetweenMaskRadius;

        _ArcRadius = Mathf.Lerp(_MaxRadius, _MinRadius, _RadiusRatio);
        _MaskRadius = Mathf.Max(0f, _ArcRadius - _BetweenMaskRadius);

        _ArcBuilder.Rotate(_Rotation);

        _ArcBuilder.SetRadius(_ArcRadius, _MaskRadius);
        _ArcBuilder.SetDegree(_ArcDegree);
    }
}
