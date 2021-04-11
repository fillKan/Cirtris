using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSystem : Singleton<BaseSystem>
{
    private const string EscTryMessage = "뒤로가기를 두 번 눌러서 종료합니다";

    private const float EscIntervalTime = 2.0f;
    private float _EscInputLastTime;

    private TMPro.TextMeshProUGUI _HighScoreText;
    private int _HighScore;

    private void Awake()
    {
        _HighScore = PlayerPrefs.GetInt("HighScore", 0);

        var find = GameObject.FindGameObjectWithTag("HighScore");
        if (find)
        {
            find.TryGetComponent(out _HighScoreText);
            _HighScoreText.text = _HighScore.ToString();
        }
        _EscInputLastTime = Time.time;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            float time = Time.time;
            if (time <= _EscInputLastTime + EscIntervalTime)
            {
                Application.Quit();
            }
            else
            {
                _EscInputLastTime = time;
                ShowToastMessage(EscTryMessage);
            }
        }
    }
    public void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt("HighScore", score);
    }
    public void ShowToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
