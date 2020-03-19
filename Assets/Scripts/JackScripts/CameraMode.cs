
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class CameraMode : MonoBehaviour
{
    CameraSettings myCameraSettings;
    void Start()
    {
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted); //注册Vulfia启动之后的回调
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);  //程序暂停时的回调
    }

    private void OnVuforiaStarted()
    {
        CameraDevice.Instance.SetFocusMode(
          CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO); //设置摄像机的对焦模式
    }

    private void OnPaused(bool paused)
    {
        if (!paused) // resumed
        {
            // Set again autofocus mode when app is resumed
            CameraDevice.Instance.SetFocusMode(
        CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }
}