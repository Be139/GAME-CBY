using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HearthLocationDetectionSceneBinder
{
    private const string SystemRootName = "HEARTH_LOCATION_SYSTEM";
    private const string GeneratedColliderRootName = "GeneratedLocationColliders";

    [MenuItem("Tools/Hearth/HUD/Apply Location Detection To Open Scene")]
    public static void ApplyLocationDetectionToOpenScene()
    {
        GameObject systemRoot = GetOrCreate(SystemRootName, null);
        HearthLocationProbe probe = GetOrAdd<HearthLocationProbe>(systemRoot);

        BindLocationSurfaces();
        BindLobbySurface(systemRoot.transform);
        DisableCrouchComponents();
        BindProbe(probe);

        EditorUtility.SetDirty(systemRoot);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        Debug.Log("[HearthLocationDetectionSceneBinder] Applied HEARTH location detection to the open scene.");
    }

    private static void BindLocationSurfaces()
    {
        ConfigureSurface("17F/unity3/Group1/Group23/Group35/Mesh33", "17F-04", "17F-04", 20, true);
        ConfigureSurface("17F/unity3/Group1/Group23/Group36/Mesh34", "17F-04", "17F-04", 20, true);
        ConfigureSurface("17F/unity3/Group1/Group23/Group37/Mesh35", "17F-04", "17F-04", 20, true);
        ConfigureSurface("17F/unity3/Group1/Group23/Group38/Mesh36", "17F-04", "17F-04", 20, true);

        ConfigureSurface("17F/unity3/Group1/Group23/Group42/Mesh40", "17F-CORRIDOR", "17F CORRIDOR", 10, false);

        ConfigureSurface("17F/unity3/Group1/Group23/Group33/Mesh31", "17F-02", "17F-02", 20, false);
        ConfigureSurface("17F/unity3/Group1/Group23/Group39/Mesh37", "17F-02", "17F-02", 20, false);
        ConfigureSurface("17F/unity3/Group1/Group23/Group45/Mesh43", "17F-02", "17F-02", 20, false);

        ConfigureSurface("17F/unity3/Group1/Group23/Group29/Mesh27", "17F-01", "17F-01", 20, false);
        ConfigureSurface("17F/unity3/Group1/Group23/Group30/Mesh28", "17F-01", "17F-01", 20, false);
        ConfigureSurface("17F/unity3/Group1/Group23/Group41/Mesh39", "17F-01", "17F-01", 20, false);

        ConfigureSurface("17F/unity3/Group1/Group23/Group31/Mesh29", "17F-03", "17F-03", 20, false);
        ConfigureSurface("17F/unity3/Group1/Group23/Group32/Mesh30", "17F-03", "17F-03", 20, false);
        ConfigureSurface("17F/unity3/Group1/Group23/Group40/Mesh38", "17F-03", "17F-03", 20, false);
    }

    private static void ConfigureSurface(string path, string locationId, string displayLabel, int priority, bool canTriggerHomeWelcome)
    {
        GameObject target = GameObject.Find(path);
        if (target == null)
        {
            Debug.LogWarning("[HearthLocationDetectionSceneBinder] Missing location mesh: " + path);
            return;
        }

        HearthLocationSurface surface = GetOrAdd<HearthLocationSurface>(target);
        surface.Configure(locationId, displayLabel, priority, canTriggerHomeWelcome);
        EnsureCollider(target);
        EditorUtility.SetDirty(target);
        EditorUtility.SetDirty(surface);
    }

    private static void BindLobbySurface(Transform systemRoot)
    {
        GameObject source = GameObject.Find("DIKUAIunity/Group1/Group144/Line001/Mesh2491");
        if (source == null)
        {
            Debug.LogWarning("[HearthLocationDetectionSceneBinder] Missing lobby mesh: DIKUAIunity/Group1/Group144/Line001/Mesh2491");
            return;
        }

        Renderer renderer = source.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("[HearthLocationDetectionSceneBinder] Lobby mesh has no Renderer bounds.");
            return;
        }

        GameObject colliderRoot = GetOrCreate(GeneratedColliderRootName, systemRoot);
        GameObject lobby = GetOrCreate("Location_1F_LOBBY", colliderRoot.transform);
        lobby.transform.position = renderer.bounds.center;
        lobby.transform.rotation = Quaternion.identity;
        lobby.transform.localScale = Vector3.one;

        BoxCollider box = GetOrAdd<BoxCollider>(lobby);
        box.isTrigger = false;
        Vector3 size = renderer.bounds.size;
        box.size = new Vector3(Mathf.Max(0.1f, size.x), Mathf.Max(1f, size.y), Mathf.Max(0.1f, size.z));
        box.center = Vector3.zero;

        HearthLocationSurface surface = GetOrAdd<HearthLocationSurface>(lobby);
        surface.Configure("1F-LOBBY", "1F LOBBY", 20, false);

        EditorUtility.SetDirty(lobby);
        EditorUtility.SetDirty(box);
        EditorUtility.SetDirty(surface);
    }

    private static void DisableCrouchComponents()
    {
        Crouch[] crouches = Object.FindObjectsOfType<Crouch>(true);
        for (int i = 0; i < crouches.Length; i++)
        {
            if (crouches[i] == null)
            {
                continue;
            }

            Undo.RecordObject(crouches[i], "Disable HEARTH crouch");
            crouches[i].SetCrouchEnabled(false);
            crouches[i].enabled = false;
            EditorUtility.SetDirty(crouches[i]);
        }
    }

    private static void BindProbe(HearthLocationProbe probe)
    {
        SerializedObject serialized = new SerializedObject(probe);
        SetObject(
            serialized,
            "viewSwitchController",
            ViewSwitchController.FindPreferredController(probe.gameObject.scene));
        SetObject(serialized, "humanProbeRoot", FindControllerTransform("Person Controller", "Player_Mia_Controller", "Mia_Controller", "Mia"));
        SetObject(serialized, "companionProbeRoot", FindControllerTransform("Robot Controller", "Robot_Controller", "Companion_Controller", "Companion", "Robot"));
        SetObject(serialized, "hudView", Object.FindObjectOfType<HearthLocationHudView>(true));
        SetObject(serialized, "hudController", Object.FindObjectOfType<HearthFirstPersonHudController>(true));
        SetBool(serialized, "locationEnabled", true);
        SetFloat(serialized, "probeHeight", 2f);
        SetFloat(serialized, "probeDistance", 8f);
        SetFloat(serialized, "refreshInterval", 0.1f);
        SetBool(serialized, "showHomeWelcomeOnce", true);
        SetString(serialized, "homeWelcomeLocationId", "17F-04");
        SetFloat(serialized, "homeWelcomeAutoCloseSeconds", 2.5f);
        SetBool(serialized, "waitForPersistentHudBeforeWelcome", true);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(probe);
    }

    private static void EnsureCollider(GameObject target)
    {
        if (target.GetComponent<Collider>() != null)
        {
            return;
        }

        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return;
        }

        MeshCollider collider = target.AddComponent<MeshCollider>();
        collider.sharedMesh = meshFilter.sharedMesh;
        EditorUtility.SetDirty(collider);
    }

    private static Transform FindControllerTransform(params string[] nameHints)
    {
        FirstPersonMovement[] movements = Object.FindObjectsOfType<FirstPersonMovement>(true);
        for (int i = 0; i < movements.Length; i++)
        {
            if (movements[i] == null)
            {
                continue;
            }

            string path = GetPath(movements[i].transform);
            for (int hintIndex = 0; hintIndex < nameHints.Length; hintIndex++)
            {
                if (path.Contains(nameHints[hintIndex]))
                {
                    return movements[i].transform;
                }
            }
        }

        return null;
    }

    private static GameObject GetOrCreate(string name, Transform parent)
    {
        Transform existing = parent != null ? parent.Find(name) : null;
        GameObject found = existing != null ? existing.gameObject : parent == null ? GameObject.Find(name) : null;
        if (found != null)
        {
            if (parent != null && found.transform.parent != parent)
            {
                found.transform.SetParent(parent, true);
            }

            return found;
        }

        GameObject created = new GameObject(name);
        if (parent != null)
        {
            created.transform.SetParent(parent, false);
        }

        Undo.RegisterCreatedObjectUndo(created, "Create " + name);
        return created;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        component = Undo.AddComponent<T>(target);
        return component;
    }

    private static void SetObject(SerializedObject serialized, string propertyName, Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(SerializedObject serialized, string propertyName, bool value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetFloat(SerializedObject serialized, string propertyName, float value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetString(SerializedObject serialized, string propertyName, string value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static string GetPath(Transform transform)
    {
        System.Collections.Generic.Stack<string> stack = new System.Collections.Generic.Stack<string>();
        while (transform != null)
        {
            stack.Push(transform.name);
            transform = transform.parent;
        }

        return string.Join("/", stack.ToArray());
    }
}
