using UnityEngine;
using UnityEngine.EventSystems;

public enum SceneIndex
{
    Main, InGame
}
public static class InputExtension
{
    public static bool TouchScreen(this EventSystem eventSystem)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WebGLPlayer:
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        return !eventSystem.IsPointerOverGameObject();
                    }
                    return false;
                }

            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                {
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);

                        if (touch.phase == TouchPhase.Stationary)
                        {
                            return !eventSystem.IsPointerOverGameObject(touch.fingerId);
                        }
                    }
                    return false;
                }
            default:
                return false;
        }
    }
}