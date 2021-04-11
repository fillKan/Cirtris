using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class InGameSceneSystem : Singleton<InGameSceneSystem>
{
    public bool IsGameOver { get; private set; }

    [Header("# GameOver Property")]
    [SerializeField] private Animator _BotButtonAnimator;
    [SerializeField] private float _BotButtonAnimTime;
    private int _BotButton_ControlHash;

    [Header("## Deadline Property")]
    [SerializeField] private Animator _DeadlineAnimator;
    [SerializeField] private ArcController _DeadlineControl;
    
    [Space()]
    [SerializeField] private float _DeadlineTime;
    [SerializeField] private AnimationCurve _DeadlineCurve;

    [Header("# BackToHome Property")]
    [SerializeField] private float _BackHomeTime;
    [SerializeField] private AnimationCurve _BackHomeCurve;

    private Coroutine _DeadlineDisable;
    private Coroutine _SelectOnRoutine;
    private Coroutine _BotBtnReleaseRoutine;

    private void Start()
    {
        IsGameOver = false;

        _DeadlineDisable = new Coroutine(this);
        _SelectOnRoutine = new Coroutine(this);

        _BotBtnReleaseRoutine = new Coroutine(this);
    }
    public void GameOver()
    {
        MainCamera.Instance.CameraShake(1.4f, 0.1f);
        ScoreSystem.Instance.PlayAnimation(ScoreAnim.GameOver);

        _BotBtnReleaseRoutine.Stop();

        _BotButtonAnimator.gameObject.SetActive(true);
        _BotButton_ControlHash = _BotButtonAnimator.GetParameter(0).nameHash;
        _BotButtonAnimator.SetBool(_BotButton_ControlHash, true);

        if (_DeadlineDisable.IsOver)
        {
            _DeadlineDisable.Start(DeadlineSetActive(false));
        }
        IsGameOver = true;
    }
    public void SelectOnBackToHome()
    {
        if (_SelectOnRoutine.IsOver)
        {
            if (_DeadlineDisable.IsOver)
            {
                _DeadlineDisable.Start(DeadlineSetActive(false));
            }
            _SelectOnRoutine.Start(BackToHome());
            ArchBlockManager.Instance.AllBlockRelease = true;
        }
    }
    public void SelectOnRetry()
    {
        if (IsGameOver)
        {
            ScoreSystem.Instance.PlayAnimation(ScoreAnim.GameOverReverse);
            ScoreSystem.Instance.CurrentScoreReset();

            _BotButtonAnimator.SetBool(_BotButton_ControlHash, false);
            _BotBtnReleaseRoutine.Start(BotButtonReleaseRoutine(_BotButtonAnimTime));

            _DeadlineDisable.Start(DeadlineSetActive(true));

            IsGameOver = false;
            ArchBlockShooter.Instance.Awaken();
        }
    }
    private IEnumerator DeadlineSetActive(bool isActive)
    {
        if (_DeadlineAnimator.enabled != isActive)
        {
            _DeadlineAnimator.enabled = isActive;

            float from = isActive ? 0f : 0.2f;
            float to = isActive ? 0.2f : 0f;

            float ratio = 0f;
            for (float i = 0f; ratio < 1f; i += Time.deltaTime)
            {
                ratio = Mathf.Min(1f, i / _DeadlineTime);
                float curveValue = _DeadlineCurve.Evaluate(ratio);

                _DeadlineControl.BetweenMaskRadius = Mathf.Lerp(from, to, curveValue);

                yield return null;
            }
            _DeadlineAnimator.gameObject.SetActive(isActive);
            _DeadlineAnimator.gameObject.transform.rotation = Quaternion.identity;

            _DeadlineDisable.Finish();
        }
    }
    private IEnumerator BackToHome()
    {
        var   upTransform = ScoreSystem.Instance.transform;
        var downTransform = _BotButtonAnimator.transform;

        Vector2   up_goal = Vector2.up   * 800f;
        Vector2 down_goal = Vector2.down * 800f;

        Vector2 up_start   =   upTransform.localPosition;
        Vector2 down_start = downTransform.localPosition;

        ScoreSystem.Instance.AnimatorEnable = false;
        _BotButtonAnimator.enabled = false;

        float ratio = 0f;
        for (float i = 0f; ratio < 1f; i += Time.deltaTime)
        {
            ratio = Mathf.Min(1f, i / _BackHomeTime);
            float curveValue = _BackHomeCurve.Evaluate(ratio);

            upTransform.localPosition 
                = Vector2.Lerp(up_start, up_goal, curveValue);
            
            downTransform.localPosition 
                = Vector2.Lerp(down_start, down_goal, curveValue);

            yield return null;
        }
        SceneManager.LoadScene((int)SceneIndex.Main);
    }
    private IEnumerator BotButtonReleaseRoutine(float time)
    {
        for (float w = 0f; w < time; w += Time.deltaTime)
            yield return null;

        _BotBtnReleaseRoutine.Finish();
        _BotButtonAnimator.gameObject.SetActive(false);
    }
}
