//#define UNITY_EDITOR_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlatformUtils
{
    public static RuntimePlatform platform
    {
        get
        {
#if UNITY_EDITOR_WIN && !UNITY_EDITOR_ANDROID
            return RuntimePlatform.WindowsPlayer;
#elif UNITY_ANDROID || UNITY_EDITOR_ANDROID
            return RuntimePlatform.Android;
#elif UNITY_EDITOR_WIN
            return RuntimePlatform.WindowsPlayer;
#elif UNITY_IOS
            return RuntimePlatform.IPhonePlayer;
#elif UNITY_STANDALONE_OSX
            return RuntimePlatform.OSXPlayer;
#elif UNITY_STANDALONE_WIN
            return RuntimePlatform.WindowsPlayer;
#endif
        }
    }
}
