using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScoreAnim
{
    GameOver, GameOverReverse, ScoreUp, Release
}
public class ScoreSystem : Singleton<ScoreSystem>
{
    [SerializeField] private Animator _ThisAnimator;
    [SerializeField] private TMPro.TextMeshProUGUI _ScoreText;
    
    private TMPro.TextMeshProUGUI _HighScoreText;

    public bool AnimatorEnable
    {
        get => _ThisAnimator.enabled;
        set => _ThisAnimator.enabled = value;
    }

    public int HighScore
    { get; private set; }
    public int CurrentScore
    { get; private set; }
    [HideInInspector]
    public int IncreasingScore;

    private int _AnimHash_GameOver;
    private int _AnimHash_GameOverRe;
    private int _AnimHash_ScoreUp;

    private void Start()
    {
        var find = GameObject.FindGameObjectWithTag("HighScore");
        if (find)
        {
            find.TryGetComponent(out _HighScoreText);
            HighScore = int.Parse(_HighScoreText.text);
        }
        _AnimHash_GameOver   = _ThisAnimator.GetParameter(0).nameHash;
        _AnimHash_GameOverRe = _ThisAnimator.GetParameter(1).nameHash;
        _AnimHash_ScoreUp    = _ThisAnimator.GetParameter(2).nameHash;

        _ScoreText.text = (CurrentScore = 0).ToString();
    }
    public void PlayAnimation(ScoreAnim scoreAnim)
    {
        switch (scoreAnim)
        {
            case ScoreAnim.GameOver:
                {
                    _ThisAnimator.SetBool(_AnimHash_GameOver, true);
                    _ThisAnimator.SetBool(_AnimHash_GameOverRe, false);
                }
                break;
            case ScoreAnim.GameOverReverse:
                {
                    _ThisAnimator.SetBool(_AnimHash_GameOver, false);
                    _ThisAnimator.SetBool(_AnimHash_GameOverRe, true);
                }
                break;
            case ScoreAnim.ScoreUp:
                {
                    _ThisAnimator.SetBool(_AnimHash_ScoreUp, true);
                }
                break;
            case ScoreAnim.Release:
                {
                    _ThisAnimator.SetBool(_AnimHash_GameOver, false);
                    _ThisAnimator.SetBool(_AnimHash_GameOverRe, false);
                    _ThisAnimator.SetBool(_AnimHash_ScoreUp, false);
                }
                break;
        }
    }
    public void CurrentScoreReset()
    {
        IncreasingScore = -CurrentScore;
    }
    public void ScoreUp(int score)
    {
        _ThisAnimator.SetBool(_AnimHash_ScoreUp, true);
        IncreasingScore = score;
    }
    public void RenewHighScore()
    {
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            _HighScoreText.text = HighScore.ToString();

            BaseSystem.Instance.SaveHighScore(HighScore);
        }
    }
    // ========== Score Up Animation Event ========== //
    private void IncreaseScore()
    {
        _ScoreText.text = (CurrentScore += IncreasingScore).ToString();
        IncreasingScore = 0;

        RenewHighScore();
    }
    private void ScoreAnimationOver()
    {
        _ThisAnimator.SetBool(_AnimHash_ScoreUp, false);
    }
    // ========== Score Up Animation Event ========== //
}
