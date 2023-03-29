using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CastrimarisStudios.Android
{

    public class Utilities
    {

        /// <summary>
        /// Returns the current Unity Activity. Useful for Native plugins.
        /// </summary>
        /// <returns>And AndroidJavaObject representing the Unity Activity</returns>
        public static AndroidJavaObject GetUnityActivity()
        {
            if (!Application.isEditor)
            {
                AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject unityActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                return unityActivity;
            }
            return null;
        }

        /// <summary>
        /// Launchs another app installed on the device. Requires root permissions.
        /// </summary>
        /// <param name="PackageName">The package name (ex. it.castrimarisstudios.awesomegame)</param>
        public static void LaunchApp(string PackageName)
        {
            if (!isEditor)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject androidPackageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                AndroidJavaObject launchIntent = androidPackageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", PackageName);
                currentActivity.Call("startActivity", launchIntent);
                unityPlayer.Dispose();
                currentActivity.Dispose();
                androidPackageManager.Dispose();
                launchIntent.Dispose();
            }
        }
    }
}