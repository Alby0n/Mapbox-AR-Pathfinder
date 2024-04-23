#pragma warning disable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning enable

namespace ARLocation
{
    public class AndroidNativeCompass
    {
#if PLATFORM_ANDROID
        private readonly AndroidJavaObject _nativeHeading;

        public AndroidNativeCompass(float alpha = 0.1f)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _nativeHeading = new AndroidJavaObject("com.dmbfm.magneticdeclination.NativeHeading", activity, alpha);
        }

        public float GetMagneticHeading()
        {
            return _nativeHeading.Call<float>("GetCurrentHeading");
        }
#else
        public AndroidNativeCompass(float alpha = 0.1f) {}
#endif
    }
}
