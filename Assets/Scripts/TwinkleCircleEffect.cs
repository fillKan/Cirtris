using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinkleCircleEffect : MonoBehaviour
{
    [SerializeField] private ArcController _ArcController;
    [SerializeField] private ArcBuilder _ArcBuilder;

    [Header("Animation Property")]
    [SerializeField] private float _DurationTime;
    [SerializeField] private Gradient _Gradient;
    [SerializeField] private AnimationCurve _AnimationCurve;

    [Header("Init Property")]
    [SerializeField] private float _StartRatio;
    [SerializeField] private float _EndRatio;
    [SerializeField] private float _StartBetRadius;
    [SerializeField] private float _EndBetRadius;

    private void OnEnable()
    {
        StartCoroutine(AnimationRoutine());
    }
    private IEnumerator AnimationRoutine()
    {
        float Lerp(float a, float b, float t) => a * (1 - t) + b * t;

        var ratio = 0f;
        for (float i = 0f; ratio < 1f; i += Time.deltaTime)
        {
            ratio = Mathf.Min(1f, i / _DurationTime);

            _ArcBuilder.color = _Gradient.Evaluate(ratio);
            float value = _AnimationCurve.Evaluate(ratio);

            _ArcController.Ratio = Lerp(_StartRatio, _EndRatio, value);
            _ArcController.BetweenMaskRadius = Lerp(_StartBetRadius, _EndBetRadius, value);

            yield return null;
        }
        gameObject.SetActive(false);
    }
}
