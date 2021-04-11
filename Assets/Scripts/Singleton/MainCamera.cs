using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : Singleton<MainCamera>
{
    private readonly Vector3 OriginalPosition = new Vector3(0, 0, -10f);

    [SerializeField] private Camera _Camera;
    [SerializeField] private AnimationCurve _ShakeCurve;

    private float _RestShakeTime;
    private float _ShakeTime;
    private float _ShakeForcePerFrame;

    public void CameraShake(float time, float force)
    {
        float forcePerFrame = force / time;
        float ratio = 1f - Mathf.Min(_RestShakeTime / _ShakeTime, 1f);

        if (forcePerFrame > _ShakeForcePerFrame * _ShakeCurve.Evaluate(ratio))
        {
            _ShakeForcePerFrame = forcePerFrame;
            _RestShakeTime = _ShakeTime = time;
        }
        else
        {
            _ShakeForcePerFrame += forcePerFrame;
        }
    }
    private void Update()
    {
        if (_RestShakeTime > 0)
        {
            _RestShakeTime -= Time.deltaTime * Time.timeScale;

            float ratio = 1f - _RestShakeTime / _ShakeTime;
            float force = _ShakeForcePerFrame * _ShakeCurve.Evaluate(ratio);

            Vector3 position   = Random.onUnitSphere * force;
                    position.z = OriginalPosition.z;

            transform.position = position;

            if (_RestShakeTime <= 0f)
            {
                _RestShakeTime = _ShakeForcePerFrame = _ShakeTime = 0f;
                transform.position = OriginalPosition;
            }
        }
    }
}
