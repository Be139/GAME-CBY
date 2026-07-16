using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HearthRuntimeInterfaceBinder
{
    private const string MenuRoot = "Tools/Hearth/Systems/";
    private const string HudPrefabPath = "Assets/Prefabs/UI/HearthHud/HearthHudRoot.prefab";

    [MenuItem(MenuRoot + "Apply HUD Audio And English Prompt Fixes")]
    public static void ApplyCurrentSceneSetup()
    {
        if (!EditorSceneManager.GetActiveScene().IsValid())
        {
            Debug.LogError("[HearthRuntimeInterfaceBinder] No valid scene is open.");
            return;
        }

        HearthAudioSettingsController audioSettings = EnsureAudioSettingsController();
        BindSettingsViews(audioSettings);
        ApplyHudPrefabAudioSettings();
        BindFootstepProfile(
            "Player/Person Controller/First Person Audio",
            HearthFootstepRole.Human,
            0.82f,
            1.05f);
        BindFootstepProfile(
            "Player/Robot Controller/Robot First Person Audio",
            HearthFootstepRole.Companion,
            1f,
            1.3f);
        BindDialogueSources();
        BindNamedAmbientSources();
        BindHudAndTerminalSources();
        ApplyEnglishInteractionCopy();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log("[HearthRuntimeInterfaceBinder] HUD settings, audio channels, footstep profiles and English single-press prompts are applied.");
        ValidateCurrentSceneSetup();
    }

    [MenuItem(MenuRoot + "Validate HUD Audio And English Prompts")]
    public static void ValidateCurrentSceneSetup()
    {
        int warnings = 0;

        HearthAudioSettingsController[] settings = UnityEngine.Object.FindObjectsOfType<HearthAudioSettingsController>(true);
        if (settings.Length != 1)
        {
            Debug.LogWarning("[HearthRuntimeInterfaceBinder] Expected one audio settings controller, found " + settings.Length + ".");
            warnings++;
        }

        warnings += ValidateFootstepProfile("Player/Person Controller/First Person Audio", HearthFootstepRole.Human);
        warnings += ValidateFootstepProfile("Player/Robot Controller/Robot First Person Audio", HearthFootstepRole.Companion);
        warnings += ValidateNamedAmbientSources();

        MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            IInteractable interactable = behaviours[i] as IInteractable;
            if (interactable == null)
            {
                continue;
            }

            string description;
            try
            {
                description = interactable.GetDescription();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[HearthRuntimeInterfaceBinder] Could not validate interaction text on " + GetPath(behaviours[i].transform) + ": " + exception.Message, behaviours[i]);
                warnings++;
                continue;
            }

            if (ContainsNonAscii(description))
            {
                Debug.LogWarning("[HearthRuntimeInterfaceBinder] Non-English runtime interaction text remains on " + GetPath(behaviours[i].transform) + ": " + description, behaviours[i]);
                warnings++;
            }
        }

        if (warnings == 0)
        {
            Debug.Log("[HearthRuntimeInterfaceBinder] Validation passed: audio interfaces are bound and runtime interaction prompts are English-only.");
        }
        else
        {
            Debug.LogWarning("[HearthRuntimeInterfaceBinder] Validation finished with " + warnings + " warning(s). Review the Console entries above.");
        }
    }

    private static HearthAudioSettingsController EnsureAudioSettingsController()
    {
        HearthAudioSettingsController existing = UnityEngine.Object.FindObjectOfType<HearthAudioSettingsController>(true);
        if (existing != null)
        {
            return existing;
        }

        HearthSettingsView settingsView = UnityEngine.Object.FindObjectOfType<HearthSettingsView>(true);
        GameObject owner = settingsView != null ? settingsView.gameObject : GameObject.Find("HearthHudRoot");
        if (owner == null)
        {
            owner = new GameObject("HEARTH_AUDIO_SETTINGS");
        }

        return Undo.AddComponent<HearthAudioSettingsController>(owner);
    }

    private static void BindSettingsViews(HearthAudioSettingsController audioSettings)
    {
        HearthSettingsView[] views = UnityEngine.Object.FindObjectsOfType<HearthSettingsView>(true);
        for (int i = 0; i < views.Length; i++)
        {
            Undo.RecordObject(views[i], "Bind HEARTH audio settings");
            SetObject(views[i], "audioSettings", audioSettings);
            views[i].RefreshFromAudioSettings();
            EditorUtility.SetDirty(views[i]);
        }
    }

    private static void ApplyHudPrefabAudioSettings()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[HearthRuntimeInterfaceBinder] HUD prefab not found: " + HudPrefabPath);
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(HudPrefabPath);
        try
        {
            HearthAudioSettingsController audioSettings = contents.GetComponent<HearthAudioSettingsController>();
            if (audioSettings == null)
            {
                audioSettings = contents.AddComponent<HearthAudioSettingsController>();
            }

            HearthSettingsView settingsView = contents.GetComponent<HearthSettingsView>();
            if (settingsView != null)
            {
                SetObject(settingsView, "audioSettings", audioSettings);
                settingsView.RefreshFromAudioSettings();
                EditorUtility.SetDirty(settingsView);
            }

            PrefabUtility.SaveAsPrefabAsset(contents, HudPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static void BindFootstepProfile(string path, HearthFootstepRole role, float walkSpeed, float runSpeed)
    {
        Transform target = FindTransform(path);
        if (target == null)
        {
            Debug.LogWarning("[HearthRuntimeInterfaceBinder] Footstep owner not found: " + path);
            return;
        }

        FirstPersonAudio firstPersonAudio = target.GetComponent<FirstPersonAudio>();
        if (firstPersonAudio == null)
        {
            Debug.LogWarning("[HearthRuntimeInterfaceBinder] FirstPersonAudio is missing on " + path, target);
            return;
        }

        HearthFootstepAudioProfile profile = target.GetComponent<HearthFootstepAudioProfile>();
        if (profile == null)
        {
            profile = Undo.AddComponent<HearthFootstepAudioProfile>(target.gameObject);
        }

        AudioClip walkClip = firstPersonAudio.stepAudio != null ? firstPersonAudio.stepAudio.clip : null;
        AudioClip runClip = firstPersonAudio.runningAudio != null ? firstPersonAudio.runningAudio.clip : walkClip;
        Undo.RecordObject(profile, "Configure HEARTH footstep profile");
        profile.Configure(role, firstPersonAudio, walkClip, runClip, walkSpeed, runSpeed);
        EditorUtility.SetDirty(profile);

        BindChannel(firstPersonAudio.stepAudio, HearthAudioChannel.SFX);
        BindChannel(firstPersonAudio.runningAudio, HearthAudioChannel.SFX);
    }

    private static void BindDialogueSources()
    {
        MinLoopSubtitlePlayer[] players = UnityEngine.Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        for (int i = 0; i < players.Length; i++)
        {
            SerializedObject serialized = new SerializedObject(players[i]);
            SerializedProperty sourceProperty = serialized.FindProperty("audioSource");
            AudioSource source = sourceProperty != null ? sourceProperty.objectReferenceValue as AudioSource : null;
            if (source == null)
            {
                source = players[i].GetComponent<AudioSource>();
            }

            if (source == null)
            {
                source = Undo.AddComponent<AudioSource>(players[i].gameObject);
                source.playOnAwake = false;
                source.spatialBlend = 0f;
            }

            HearthAudioChannelSource channelSource = BindChannel(source, HearthAudioChannel.Dialogue);
            SetObject(players[i], "audioSource", source);
            SetObject(players[i], "dialogueChannelSource", channelSource);
            EditorUtility.SetDirty(players[i]);
        }
    }

    private static void BindHudAndTerminalSources()
    {
        HearthFirstPersonHudController[] hudControllers = UnityEngine.Object.FindObjectsOfType<HearthFirstPersonHudController>(true);
        for (int i = 0; i < hudControllers.Length; i++)
        {
            BindChannel(hudControllers[i].GetComponent<AudioSource>(), HearthAudioChannel.SFX);
        }

        HearthTvTerminalController[] terminals = UnityEngine.Object.FindObjectsOfType<HearthTvTerminalController>(true);
        for (int i = 0; i < terminals.Length; i++)
        {
            BindChannel(terminals[i].GetComponent<AudioSource>(), HearthAudioChannel.SFX);
        }

        HearthCompanionHudController[] companionHuds = UnityEngine.Object.FindObjectsOfType<HearthCompanionHudController>(true);
        for (int i = 0; i < companionHuds.Length; i++)
        {
            BindChannel(companionHuds[i].GetComponent<AudioSource>(), HearthAudioChannel.SFX);
        }
    }

    private static void BindNamedAmbientSources()
    {
        AudioSource[] sources = UnityEngine.Object.FindObjectsOfType<AudioSource>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            string name = sources[i].name.ToUpperInvariant();
            if (name.Contains("AMBIENT") || name.Contains("AMBIENCE"))
            {
                BindChannel(sources[i], HearthAudioChannel.Ambient);
            }
        }
    }

    private static HearthAudioChannelSource BindChannel(AudioSource source, HearthAudioChannel channel)
    {
        if (source == null)
        {
            return null;
        }

        HearthAudioChannelSource channelSource = source.GetComponent<HearthAudioChannelSource>();
        float baseVolume = channelSource != null ? channelSource.BaseVolume : source.volume;
        if (channelSource == null)
        {
            channelSource = Undo.AddComponent<HearthAudioChannelSource>(source.gameObject);
        }

        Undo.RecordObject(channelSource, "Bind HEARTH audio channel");
        channelSource.Configure(source, channel, baseVolume);
        EditorUtility.SetDirty(channelSource);
        return channelSource;
    }

    private static void ApplyEnglishInteractionCopy()
    {
        PlayerInteraction[] players = UnityEngine.Object.FindObjectsOfType<PlayerInteraction>(true);
        for (int i = 0; i < players.Length; i++)
        {
            SetString(players[i], "fallbackDescription", "E  INTERACT");
            SetBool(players[i], "englishPromptsOnly", true);
            SetBool(players[i], "normalizeSinglePressPrompts", true);
            SetString(players[i], "interactionKeyLabel", "E");
        }

        HearthTvTerminalInteractable[] terminals = UnityEngine.Object.FindObjectsOfType<HearthTvTerminalInteractable>(true);
        for (int i = 0; i < terminals.Length; i++)
        {
            SetString(terminals[i], "openDescription", "E  ACCESS TERMINAL");
            SetString(terminals[i], "closeDescription", "E  LEAVE TERMINAL");
        }

        SmartDoorController[] doors = UnityEngine.Object.FindObjectsOfType<SmartDoorController>(true);
        for (int i = 0; i < doors.Length; i++)
        {
            SetString(doors[i], "closedDescription", "E  OPEN DOOR");
            SetString(doors[i], "openDescription", "E  CLOSE DOOR");
            SetString(doors[i], "lockedDescription", "DOOR LOCKED");
        }

        ComfortActionInteractable[] comfortActions = UnityEngine.Object.FindObjectsOfType<ComfortActionInteractable>(true);
        for (int i = 0; i < comfortActions.Length; i++)
        {
            SetString(comfortActions[i], "interactionDescription", "E  PERFORM COMFORT ACTION");
        }

        ResidentTerminalFlow[] residentTerminals = UnityEngine.Object.FindObjectsOfType<ResidentTerminalFlow>(true);
        for (int i = 0; i < residentTerminals.Length; i++)
        {
            SetString(residentTerminals[i], "fallbackDescription", "E  SCAN ID");
        }

        InteractionFeedbackController[] feedback = UnityEngine.Object.FindObjectsOfType<InteractionFeedbackController>(true);
        for (int i = 0; i < feedback.Length; i++)
        {
            SetString(feedback[i], "interactionDescription", GuessFeedbackPrompt(feedback[i].name));
        }

        MinLoopWorldGuideMarker[] markers = UnityEngine.Object.FindObjectsOfType<MinLoopWorldGuideMarker>(true);
        for (int i = 0; i < markers.Length; i++)
        {
            SetString(markers[i], "markerLabel", "OBJECTIVE");
        }
    }

    private static string GuessFeedbackPrompt(string objectName)
    {
        string value = objectName != null ? objectName.ToUpperInvariant() : string.Empty;
        if (value.Contains("PHOTO") || value.Contains("FRAME") || value.Contains("TV (4)"))
        {
            return "E  VIEW PHOTO";
        }

        if (value.Contains("ROBOT") || value.Contains("COMPANION"))
        {
            return "E  INSPECT UNIT";
        }

        if (value.Contains("TERMINAL") || value.Contains("TV"))
        {
            return "E  ACCESS TERMINAL";
        }

        return "E  INTERACT";
    }

    private static int ValidateFootstepProfile(string path, HearthFootstepRole role)
    {
        Transform target = FindTransform(path);
        if (target == null)
        {
            Debug.LogWarning("[HearthRuntimeInterfaceBinder] Missing formal footstep object: " + path);
            return 1;
        }

        HearthFootstepAudioProfile profile = target.GetComponent<HearthFootstepAudioProfile>();
        if (profile == null || profile.Role != role)
        {
            Debug.LogWarning("[HearthRuntimeInterfaceBinder] Missing or incorrect footstep profile on " + path, target);
            return 1;
        }

        return 0;
    }

    private static int ValidateNamedAmbientSources()
    {
        int warnings = 0;
        AudioSource[] sources = UnityEngine.Object.FindObjectsOfType<AudioSource>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            string name = sources[i].name.ToUpperInvariant();
            if (!name.Contains("AMBIENT") && !name.Contains("AMBIENCE"))
            {
                continue;
            }

            HearthAudioChannelSource channel = sources[i].GetComponent<HearthAudioChannelSource>();
            if (channel == null || channel.Channel != HearthAudioChannel.Ambient)
            {
                Debug.LogWarning("[HearthRuntimeInterfaceBinder] Ambient AudioSource is not on the Ambient channel: " + GetPath(sources[i].transform), sources[i]);
                warnings++;
            }
        }

        return warnings;
    }

    private static Transform FindTransform(string path)
    {
        string[] parts = path.Split('/');
        GameObject root = GameObject.Find(parts[0]);
        if (root == null)
        {
            return null;
        }

        Transform current = root.transform;
        for (int i = 1; i < parts.Length && current != null; i++)
        {
            current = current.Find(parts[i]);
        }

        return current;
    }

    private static void SetString(UnityEngine.Object target, string propertyName, string value)
    {
        SetSerializedValue(target, propertyName, property => property.stringValue = value);
    }

    private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
    {
        SetSerializedValue(target, propertyName, property => property.boolValue = value);
    }

    private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        SetSerializedValue(target, propertyName, property => property.objectReferenceValue = value);
    }

    private static void SetSerializedValue(UnityEngine.Object target, string propertyName, Action<SerializedProperty> assign)
    {
        if (target == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        Undo.RecordObject(target, "Configure HEARTH runtime interface");
        serialized.Update();
        assign(property);
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static bool ContainsNonAscii(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] > 127)
            {
                return true;
            }
        }

        return false;
    }

    private static string GetPath(Transform target)
    {
        if (target == null)
        {
            return "<missing>";
        }

        string path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}
