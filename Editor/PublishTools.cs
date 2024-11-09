using Castrimaris.Core.Editor;
using Castrimaris.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Castrimaris.ScriptableObjects.InitializationParameters;


public class PublishTools : MonoBehaviour {

    [MenuItem("Tools/Publish/Publish APK and Server builds")]
    public static void Publish() {
        string path = EditorUtility.SaveFolderPanel("Choose Save Location", "", BuildTools.baseFileName);

        if (string.IsNullOrEmpty(path))
            return;

        //Get ScriptableObject for Initialization Parameters
        var initializationParametersGUID = AssetDatabase.FindAssets($"t:{nameof(InitializationParameters)}").First();
        var initializationParametersAssetPath = AssetDatabase.GUIDToAssetPath(initializationParametersGUID);
        var initializationParameters = AssetDatabase.LoadAssetAtPath(initializationParametersAssetPath, typeof(InitializationParameters)) as InitializationParameters;

        //Force chosen Network Mode
        initializationParameters.ForceNetworkMode(NetworkModes.SERVER);

        AssetDatabase.Refresh();

        VersioningEditor.SkipBuild = true;
        VersioningEditor.UpdateBuildNumber();
        BuildTools.MakeOculusBuild(path, BuildOptions.CleanBuildCache);
        BuildTools.MakeLinuxServerBuild(path, BuildOptions.CleanBuildCache);
        VersioningEditor.SkipBuild = false;
    }

}

