using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CityWindowNightController : MonoBehaviour
{
    [Serializable]
    public sealed class WindowMaterialSet
    {
        [SerializeField] private Material sourceMaterial;
        [SerializeField] private List<Material> nightVariants = new List<Material>();

        public Material SourceMaterial
        {
            get { return sourceMaterial; }
        }

        public List<Material> NightVariants
        {
            get { return nightVariants; }
        }

        public WindowMaterialSet(Material source, IList<Material> variants)
        {
            sourceMaterial = source;
            nightVariants = variants == null
                ? new List<Material>()
                : new List<Material>(variants);
        }

        public bool Contains(Material material)
        {
            if (material == null)
            {
                return false;
            }

            if (material == sourceMaterial)
            {
                return true;
            }

            for (int i = 0; i < nightVariants.Count; i++)
            {
                if (nightVariants[i] == material)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Header("Building Roots")]
    [SerializeField] private Transform selectedBuildingRoot;
    [SerializeField] private Transform heightGroupedBuildingRoot;

    [Header("Window Pattern")]
    [SerializeField, Range(0f, 1f)] private float litWindowRatio = 0.75f;
    [SerializeField, Range(0f, 1f)] private float warmWindowShare = 0.35f;
    [SerializeField, Min(0f)] private float emissionIntensity = 1.8f;
    [SerializeField, Range(8, 64)] private int patternGridSize = 32;
    [SerializeField, Range(1, 8)] private int patternVariantCount = 4;
    [SerializeField] private int randomSeed = 20260728;

    [Header("Window Colors")]
    [SerializeField] private Color deepWarmColor = new Color(1f, 0.416f, 0.165f, 1f);
    [SerializeField] private Color blueColor = new Color(0.247f, 0.447f, 1f, 1f);
    [SerializeField] private Color cyanColor = new Color(0.125f, 0.902f, 1f, 1f);
    [SerializeField] private Color magentaColor = new Color(0.945f, 0.231f, 0.784f, 1f);

    [Header("Generated Material Mapping")]
    [SerializeField] private List<WindowMaterialSet> materialSets =
        new List<WindowMaterialSet>();

    public float LitWindowRatio
    {
        get { return litWindowRatio; }
    }

    public float WarmWindowShare
    {
        get { return warmWindowShare; }
    }

    public float EmissionIntensity
    {
        get { return emissionIntensity; }
    }

    public int PatternGridSize
    {
        get { return patternGridSize; }
    }

    public int PatternVariantCount
    {
        get { return patternVariantCount; }
    }

    public int RandomSeed
    {
        get { return randomSeed; }
    }

    public Color DeepWarmColor
    {
        get { return deepWarmColor; }
    }

    public Color BlueColor
    {
        get { return blueColor; }
    }

    public Color CyanColor
    {
        get { return cyanColor; }
    }

    public Color MagentaColor
    {
        get { return magentaColor; }
    }

    public List<WindowMaterialSet> MaterialSets
    {
        get { return materialSets; }
    }

    public void Configure(
        Transform selectedRoot,
        Transform heightGroupedRoot,
        IList<WindowMaterialSet> generatedMaterialSets)
    {
        selectedBuildingRoot = selectedRoot;
        heightGroupedBuildingRoot = heightGroupedRoot;
        materialSets = generatedMaterialSets == null
            ? new List<WindowMaterialSet>()
            : new List<WindowMaterialSet>(generatedMaterialSets);
    }

    public int ApplyNightWindows()
    {
        int changedSlots = ApplyNightMaterials();
        MarkSceneDirty();
        Debug.Log(
            "City window night: applied night materials to " + changedSlots +
            " window material slots.");
        return changedSlots;
    }

    public int RestoreOriginalWindows()
    {
        ResolveBuildingRoots();
        List<GameObject> buildings = GetManagedBuildings();
        int changedSlots = 0;

        for (int buildingIndex = 0; buildingIndex < buildings.Count; buildingIndex++)
        {
            Renderer[] renderers = buildings[buildingIndex].GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;

                for (int slot = 0; slot < materials.Length; slot++)
                {
                    WindowMaterialSet set = FindSetForVariant(materials[slot]);
                    if (set == null || set.SourceMaterial == null ||
                        materials[slot] == set.SourceMaterial)
                    {
                        continue;
                    }

                    materials[slot] = set.SourceMaterial;
                    changed = true;
                    changedSlots++;
                }

                if (changed)
                {
                    AssignMaterials(renderer, materials, "Restore original city windows");
                }
            }
        }

        MarkSceneDirty();
        Debug.Log(
            "City window night: restored " + changedSlots +
            " window material slots to their original materials.");
        return changedSlots;
    }

    public int RedistributeNightWindows()
    {
        randomSeed = unchecked(randomSeed + 1);
        int changedSlots = ApplyNightMaterials();
        MarkSceneDirty();
        Debug.Log(
            "City window night: redistributed window patterns with seed " +
            randomSeed + ". Changed slots: " + changedSlots + ".");
        return changedSlots;
    }

    public bool ValidateSetup()
    {
        ResolveBuildingRoots();

        int totalBuildings;
        int eligibleBuildings;
        GetScanCounts(out totalBuildings, out eligibleBuildings);

        bool rootsValid = selectedBuildingRoot != null &&
                          heightGroupedBuildingRoot != null;
        bool mappingsValid = materialSets.Count > 0;

        for (int i = 0; i < materialSets.Count; i++)
        {
            WindowMaterialSet set = materialSets[i];
            if (set == null || set.SourceMaterial == null ||
                set.NightVariants == null || set.NightVariants.Count == 0)
            {
                mappingsValid = false;
                break;
            }
        }

        bool valid = rootsValid && mappingsValid && eligibleBuildings > 0;
        string message =
            "City window night validation: total buildings=" + totalBuildings +
            ", eligible buildings=" + eligibleBuildings +
            ", material sets=" + materialSets.Count + ".";

        if (valid)
        {
            Debug.Log(message);
        }
        else
        {
            Debug.LogError(message + " Setup is incomplete.");
        }

        if (valid && (totalBuildings != 480 || eligibleBuildings != 186))
        {
            Debug.LogWarning(
                "City window night: the current scene differs from the initial " +
                "480-building / 186-eligible audit. This is allowed after city edits.");
        }

        return valid;
    }

    public void GetScanCounts(out int totalBuildings, out int eligibleBuildings)
    {
        ResolveBuildingRoots();
        List<GameObject> buildings = GetManagedBuildings();
        totalBuildings = buildings.Count;
        eligibleBuildings = 0;

        for (int i = 0; i < buildings.Count; i++)
        {
            if (BuildingContainsManagedWindowMaterial(buildings[i]))
            {
                eligibleBuildings++;
            }
        }
    }

    private int ApplyNightMaterials()
    {
        ResolveBuildingRoots();
        List<GameObject> buildings = GetManagedBuildings();
        int changedSlots = 0;

        for (int buildingIndex = 0; buildingIndex < buildings.Count; buildingIndex++)
        {
            GameObject building = buildings[buildingIndex];
            Renderer[] renderers = building.GetComponentsInChildren<Renderer>(true);

            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;

                for (int slot = 0; slot < materials.Length; slot++)
                {
                    WindowMaterialSet set = FindSet(materials[slot]);
                    if (set == null || set.NightVariants.Count == 0)
                    {
                        continue;
                    }

                    int variantIndex = GetVariantIndex(building, renderer, slot, set);
                    Material targetMaterial = set.NightVariants[variantIndex];
                    if (targetMaterial == null || materials[slot] == targetMaterial)
                    {
                        continue;
                    }

                    materials[slot] = targetMaterial;
                    changed = true;
                    changedSlots++;
                }

                if (changed)
                {
                    AssignMaterials(renderer, materials, "Apply city window night");
                }
            }
        }

        return changedSlots;
    }

    private int GetVariantIndex(
        GameObject building,
        Renderer renderer,
        int slot,
        WindowMaterialSet set)
    {
        string key =
            GetHierarchyPath(building.transform) + "|" +
            GetRelativePath(building.transform, renderer.transform) + "|" +
            slot + "|" +
            set.SourceMaterial.name;
        uint hash = unchecked((uint)StableHash(key));
        uint seedOffset = unchecked((uint)randomSeed);
        return (int)((hash + seedOffset) % (uint)set.NightVariants.Count);
    }

    private WindowMaterialSet FindSet(Material material)
    {
        if (material == null)
        {
            return null;
        }

        for (int i = 0; i < materialSets.Count; i++)
        {
            WindowMaterialSet set = materialSets[i];
            if (set != null && set.Contains(material))
            {
                return set;
            }
        }

        return null;
    }

    private WindowMaterialSet FindSetForVariant(Material material)
    {
        if (material == null)
        {
            return null;
        }

        for (int setIndex = 0; setIndex < materialSets.Count; setIndex++)
        {
            WindowMaterialSet set = materialSets[setIndex];
            if (set == null || set.NightVariants == null)
            {
                continue;
            }

            for (int variantIndex = 0; variantIndex < set.NightVariants.Count; variantIndex++)
            {
                if (set.NightVariants[variantIndex] == material)
                {
                    return set;
                }
            }
        }

        return null;
    }

    private bool BuildingContainsManagedWindowMaterial(GameObject building)
    {
        Renderer[] renderers = building.GetComponentsInChildren<Renderer>(true);
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Material[] materials = renderers[rendererIndex].sharedMaterials;
            for (int slot = 0; slot < materials.Length; slot++)
            {
                if (FindSet(materials[slot]) != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<GameObject> GetManagedBuildings()
    {
        List<GameObject> buildings = new List<GameObject>();
        HashSet<int> seen = new HashSet<int>();

        AddDirectChildren(selectedBuildingRoot, buildings, seen);

        if (heightGroupedBuildingRoot != null)
        {
            foreach (Transform category in heightGroupedBuildingRoot)
            {
                AddDirectChildren(category, buildings, seen);
            }
        }

        return buildings;
    }

    private static void AddDirectChildren(
        Transform root,
        List<GameObject> buildings,
        HashSet<int> seen)
    {
        if (root == null)
        {
            return;
        }

        foreach (Transform child in root)
        {
            if (seen.Add(child.gameObject.GetInstanceID()))
            {
                buildings.Add(child.gameObject);
            }
        }
    }

    private void ResolveBuildingRoots()
    {
        if (selectedBuildingRoot == null)
        {
            selectedBuildingRoot = FindSceneTransform("BUILDING");
        }

        if (heightGroupedBuildingRoot == null)
        {
            heightGroupedBuildingRoot = FindSceneTransform("CityBuildings_ByHeight");
        }
    }

    private Transform FindSceneTransform(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate.name == objectName &&
                candidate.gameObject.scene == gameObject.scene)
            {
                return candidate;
            }
        }

        return null;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    private static string GetRelativePath(Transform root, Transform target)
    {
        if (root == target)
        {
            return root.name;
        }

        string path = target.name;
        Transform parent = target.parent;
        while (parent != null && parent != root)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            const int offsetBasis = (int)2166136261;
            const int prime = 16777619;
            int hash = offsetBasis;

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= prime;
            }

            return hash;
        }
    }

    private static void AssignMaterials(
        Renderer renderer,
        Material[] materials,
        string undoName)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.RecordObject(renderer, undoName);
        }
#endif
        renderer.sharedMaterials = materials;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(renderer);
        }
#endif
    }

    private void MarkSceneDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }

    private void OnValidate()
    {
        litWindowRatio = Mathf.Clamp01(litWindowRatio);
        warmWindowShare = Mathf.Clamp01(warmWindowShare);
        emissionIntensity = Mathf.Max(0f, emissionIntensity);
        patternGridSize = Mathf.Clamp(patternGridSize, 8, 64);
        patternVariantCount = Mathf.Clamp(patternVariantCount, 1, 8);
    }
}
