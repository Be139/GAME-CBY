using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityStandardAssets.CrossPlatformInput.Inspector
{
    public class CrossPlatformInitialize
    {
        // Custom compiler defines:
        //
        // CROSS_PLATFORM_INPUT : denotes that cross platform input package exists, so that other packages can use their CrossPlatformInput functions.
        // EDITOR_MOBILE_INPUT : denotes that mobile input should be used in editor, if a mobile build target is selected. (i.e. using Unity Remote app).
        // MOBILE_INPUT : denotes that mobile input should be used right now!

        [MenuItem("Mobile Input/Enable")]
        private static void Enable()
        {
            SetEnabled(true);
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                case BuildTarget.iOS:
				case BuildTarget.WSAPlayer: 
                    EditorUtility.DisplayDialog("Mobile Input",
                                                "You have enabled Mobile Input. You'll need to use the Unity Remote app on a connected device to control your game in the Editor.",
                                                "OK");
                    break;

                default:
                    EditorUtility.DisplayDialog("Mobile Input",
                                                "You have enabled Mobile Input, but you have a non-mobile build target selected in your build settings. The mobile control rigs won't be active or visible on-screen until you switch the build target to a mobile platform.",
                                                "OK");
                    break;
            }
        }


        [MenuItem("Mobile Input/Enable", true)]
        private static bool EnableValidate()
        {
            foreach (var group in mobileBuildTargetGroups)
            {
                if (!GetDefinesList(group).Contains("MOBILE_INPUT")) return true;
            }
            return false;
        }


        [MenuItem("Mobile Input/Disable")]
        private static void Disable()
        {
            SetEnabled(false);
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                case BuildTarget.iOS:
                case BuildTarget.WSAPlayer:
                    EditorUtility.DisplayDialog("Mobile Input",
                                                "You have disabled Mobile Input. Mobile control rigs won't be visible, and the Cross Platform Input functions will always return standalone controls.",
                                                "OK");
                    break;
            }
        }


        [MenuItem("Mobile Input/Disable", true)]
        private static bool DisableValidate()
        {
            foreach (var group in mobileBuildTargetGroups)
            {
                if (GetDefinesList(group).Contains("MOBILE_INPUT")) return true;
            }
            return false;
        }


        private static BuildTargetGroup[] mobileBuildTargetGroups = new BuildTargetGroup[]
            {
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS,
				BuildTargetGroup.WSA 
            };


        private static void SetEnabled(bool enable)
        {
            const string defineName = "MOBILE_INPUT";
            foreach (var group in mobileBuildTargetGroups)
            {
                var defines = GetDefinesList(group);
                if (enable)
                {
                    if (defines.Contains(defineName))
                    {
                        continue;
                    }
                    defines.Add(defineName);
                }
                else
                {
                    if (!defines.Contains(defineName))
                    {
                        continue;
                    }
                    while (defines.Contains(defineName))
                    {
                        defines.Remove(defineName);
                    }
                }
                string definesString = string.Join(";", defines.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
            }
        }


        private static List<string> GetDefinesList(BuildTargetGroup group)
        {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }
    }
}
