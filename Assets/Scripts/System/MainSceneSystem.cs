using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneSystem : MonoBehaviour
{
    [Header("ArchClosing Property")]
    [SerializeField] private ArcBuilder   [] _ArchBuilders;
    [SerializeField] private ArcController[] _ArchControllers;
    [SerializeField] private Animator     [] _Animators;

    [Space(), SerializeField] private float _CloseTime;
    [SerializeField] private AnimationCurve _CloseAnimationCurve;

    private float[] _ArchDegs;

    [Header("── ArchClosing to HideUI ──")]
    [SerializeField] private float _BetweenDelay;

    [Header("HideUI Property")]
    [SerializeField] private RectTransform _UpHideHolder;
    [SerializeField] private RectTransform _DownHideHolder;

    [Space()]
    [SerializeField] private float _HideTime;
    [SerializeField] private AnimationCurve _HideAnimationCurve;

    [Header("ShowLicense Property")]
    private bool _IsEnableLicense = false;

    [SerializeField] private RectTransform _DownMovmentHolder;
    [SerializeField] private RectTransform _LeftMovmentHolder;
    [SerializeField] private RectTransform _RightMovmentHolder;
    [SerializeField] private RectTransform _LicenseObject;

    private readonly Vector2 _DownGoalPosition = new Vector2(0, -495f);

    private readonly Vector2  _LeftStartPosition = new Vector2(-800f,    0f);
    private readonly Vector2  _DownStartPosition = new Vector2(   0f, -900f);
    private readonly Vector2 _RightStartPosition = new Vector2( 800f,    0f);

    [Space()]
    [SerializeField] private float _MoveTime;
    [SerializeField] private AnimationCurve _MoveAnimationCurve;

    private AsyncOperation _LoadedScene;
    private Coroutine _AnimateRoutine;

    private void Start()
    {
        _AnimateRoutine = new Coroutine(this);

        _LoadedScene = SceneManager.LoadSceneAsync((int)SceneIndex.InGame, LoadSceneMode.Single);
        _LoadedScene.allowSceneActivation = false;

        // =============_ArchDegs 초기화 ============= //
        _ArchDegs = new float[_ArchControllers.Length];

        for (int i = 0; i < _ArchControllers.Length; i++)
        {
            // !ArchController의 수와 ArchBuilder의 수는 같다
            _ArchDegs[i] = _ArchBuilders[i].GetDegree();

            // ====== 씬이 시작될 때의 연출을 위한 코드 ====== //
            _ArchBuilders[i].SetDegree(0f);
            _ArchControllers[i].enabled = false;
            // ====== 씬이 시작될 때의 연출을 위한 코드 ====== //
        }
        // =============_ArchDegs 초기화 ============= //

        _AnimateRoutine.Start(RewindScreenRoutine());
    }
    private void Update()
    {
        if (EventSystem.current.TouchScreen())
        {
            if (_AnimateRoutine.IsOver)
            {
                if (_IsEnableLicense)
                {
                    _AnimateRoutine.Start(ReposLicenseRoutine(RewindScreenRoutine()));
                }
                else
                {
                    _AnimateRoutine.Start(CleanUpScreenRoutine(InGameLoad()));
                }
            }
        }
    }
    public void ShowLicense()
    {
        if (_AnimateRoutine.IsOver)
        {
            if (_IsEnableLicense)
            {
                _AnimateRoutine.Start(ReposLicenseRoutine(RewindScreenRoutine()));
            }
            else
            {
                _AnimateRoutine.Start(CleanUpScreenRoutine(ScaleLicenseRoutine(null)));
            }
        }
    }
    private IEnumerator InGameLoad()
    {
        _LoadedScene.allowSceneActivation = true;
        yield return null;
    }


    private IEnumerator ReposLicenseRoutine(IEnumerator reposOverAction)
    {
        Vector2  downStart = (_IsEnableLicense ? _DownGoalPosition :  _DownStartPosition);
        Vector2  leftStart = (_IsEnableLicense ?      Vector2.zero :  _LeftStartPosition);
        Vector2 rightStart = (_IsEnableLicense ?      Vector2.zero : _RightStartPosition);

        Vector2  downGoal = (_IsEnableLicense ?  _DownStartPosition : _DownGoalPosition);
        Vector2  leftGoal = (_IsEnableLicense ?  _LeftStartPosition :      Vector2.zero);
        Vector2 rightGoal = (_IsEnableLicense ? _RightStartPosition :      Vector2.zero);

        float ratio = 0f;
        for (float i = 0f; ratio < 1f; i += Time.deltaTime)
        {
            ratio = Mathf.Min(i / _MoveTime, 1f);

            float lerpValue = _MoveAnimationCurve.Evaluate(ratio);

             _LeftMovmentHolder.localPosition = Vector2.Lerp( leftStart,  leftGoal, lerpValue);
             _DownMovmentHolder.localPosition = Vector2.Lerp( downStart,  downGoal, lerpValue);
            _RightMovmentHolder.localPosition = Vector2.Lerp(rightStart, rightGoal, lerpValue);

            yield return null;
        }
        _IsEnableLicense = !_IsEnableLicense;

        if (reposOverAction == null)
        {
            _AnimateRoutine.Finish();
        }
        else
        {
            yield return reposOverAction;
        }
        _LicenseObject.gameObject.SetActive(false);

         _LeftMovmentHolder.localPosition = Vector2.zero;
        _RightMovmentHolder.localPosition = Vector2.zero;

        _DownMovmentHolder.localPosition = _DownGoalPosition;
        _DownMovmentHolder.gameObject.SetActive(false);
    }
    private IEnumerator ScaleLicenseRoutine(IEnumerator scaleOverAction)
    {
        _LicenseObject.gameObject.SetActive(true);
        _DownMovmentHolder.gameObject.SetActive(true);

        float ratio = 0f;

        for (float i = 0f; ratio < 1f; i += Time.deltaTime)
        {
            ratio = Mathf.Min(i / _MoveTime, 1f);
            float lerpValue = _MoveAnimationCurve.Evaluate(ratio);

            _LicenseObject.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, lerpValue);
            _DownMovmentHolder.localPosition = Vector2.Lerp(_DownStartPosition, _DownGoalPosition, lerpValue);

            yield return null;
        }
        _IsEnableLicense = !_IsEnableLicense;

        if (scaleOverAction == null)
        {
            _AnimateRoutine.Finish();
        }
        else
        {
            yield return scaleOverAction;
        }
    }


    private IEnumerator CleanUpScreenRoutine(IEnumerator cleanUpAction)
    {
        // ========== ArchClosing ========== //
        {
            for (int i = 0; i < _ArchControllers.Length; i++)
            {
                _ArchControllers[i].enabled = false;

                // !ArchController의 수보다 Animator의 수가 더 적다
                if (i < _Animators.Length)
                {
                    _Animators[i].enabled = false;
                }
            }
            float ratio = 0f;

            for (float i = 0f; ratio < 1f; i += Time.deltaTime)
            {
                ratio = Mathf.Min(i / _CloseTime, 1f);
                float curveValue = _CloseAnimationCurve.Evaluate(ratio);

                for (int j = 0; j < _ArchBuilders.Length; j++)
                {
                    _ArchBuilders[j].SetDegree(Mathf.Lerp(_ArchDegs[j], 0f, curveValue));
                }
                yield return null;
            }
        }
        // ========== ArchClosing ========== //

        for (float i = 0f; i < _BetweenDelay; i += Time.deltaTime) yield return null;

        // ============= HideUI ============ //
        {
            float ratio;
            Vector2 goalTransform = new Vector2(0f, Screen.height / 2f);
            
            for (float i = 0f; (ratio = i / _HideTime) < 1f; i += Time.deltaTime)
            {
                ratio = Mathf.Min(ratio, 1f);
                float curveValue = _HideAnimationCurve.Evaluate(ratio);

                _UpHideHolder.localPosition
                    = Vector2.Lerp(Vector2.zero, goalTransform, curveValue);

                _DownHideHolder.localPosition
                    = Vector2.Lerp(Vector2.zero, goalTransform * -1f, curveValue);

                yield return null;
            }
        }
        // ============= HideUI ============ //

        if (cleanUpAction == null)
        {
            _AnimateRoutine.Finish();
        }
        else
        {
            yield return cleanUpAction;
        }
    }

    private IEnumerator RewindScreenRoutine()
    {
        // ============= ShowUI ============ //
        {
            float ratio;
            Vector2 curTransform = new Vector2(0f, Screen.height / 2f);

            for (float i = 0f; (ratio = i / _HideTime) < 1f; i += Time.deltaTime)
            {
                ratio = Mathf.Min(ratio, 1f);
                float curveValue = _HideAnimationCurve.Evaluate(ratio);

                _UpHideHolder.localPosition
                    = Vector2.Lerp(curTransform, Vector2.zero, curveValue);

                _DownHideHolder.localPosition
                    = Vector2.Lerp(curTransform * -1f, Vector2.zero, curveValue);

                yield return null;
            }
        }
        // ============= ShowUI ============ //

        for (float i = 0f; i < _BetweenDelay; i += Time.deltaTime) yield return null;

        // =========== ArchOpen ============ //
        {
            for (int i = 0; i < _ArchControllers.Length; i++)
            {
                _ArchControllers[i].enabled = true;

                // !ArchController의 수보다 Animator의 수가 더 적다
                if (i < _Animators.Length)
                {
                    _Animators[i].enabled = true;
                }
            }
            float ratio = 0f;

            for (float i = 0f; ratio < 1f; i += Time.deltaTime)
            {
                ratio = Mathf.Min(i / _CloseTime, 1f);
                float curveValue = _CloseAnimationCurve.Evaluate(ratio);

                for (int j = 0; j < _ArchBuilders.Length; j++)
                {
                    _ArchBuilders[j].SetDegree(Mathf.Lerp(0f, _ArchDegs[j], curveValue));
                }
                yield return null;
            }
        }
        // =========== ArchOpen ============ //
        _AnimateRoutine.Finish();
    }
}
