#pragma warning disable
using System;
using UnityEngine;
#pragma warning enable

namespace ARLocation
{
    public static class AndroidMagneticDeclination
    {
        public static float GetDeclination(Location location)
        {
#if PLATFORM_ANDROID
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var javaObject = new AndroidJavaObject("com.dmbfm.magneticdeclination.MagneticDeclination");

            return javaObject.Call<float>("GetMagneticDeclination", (float) location.Latitude, (float) location.Longitude, (float) location.Altitude, time);
#else
            return 0.0f;
#endif
        }
    }
}
