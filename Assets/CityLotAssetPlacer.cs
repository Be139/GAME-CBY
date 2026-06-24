using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CityLotAssetPlacer : MonoBehaviour
{
    public enum PrefabFitMode
    {
        KeepPrefabScale,
        UniformFitFootprintKeepHeight,
        StretchFitFootprintKeepHeight,
        UniformFitAllAxes
    }

    public enum LotHeightMode
    {
        AutoByArea,
        LowRiseDistrict,
        MidRiseDistrict,
        HighRiseDistrict,
        LandmarkCore,
        Custom
    }

    [Serializable]
    public sealed class LotHeightOverride
    {
        public string LotName = "Group20";
        public LotHeightMode HeightMode = LotHeightMode.Custom;
        public float MinHeight = 70f;
        public float MaxHeight = 180f;
    }

    private enum BuildingAssetFamily
    {
        PrimaryCity,
        JapaneseCity,
        Other
    }

    [Header("Lot Source")]
    [SerializeField] private Transform lotRoot;
    [SerializeField] private string lotRootName = "dikuai";
    [SerializeField] private string[] targetLotNames =
    {
        "Group20",
        "Group51",
        "Group47",
        "Group29",
        "Group25",
        "Group106",
        "Group113",
        "Group39",
        "Group109"
    };

    [Header("Generated Output")]
    [SerializeField] private string generatedRootName = "CityAssets_AllTestLots_HeightControlled";
    [SerializeField] private int randomSeed = 20260615;
    [SerializeField] private bool clearBeforeGenerate = true;

    [Header("Planning Rules")]
    [SerializeField] private float edgeSetback = 1.5f;
    [SerializeField] private float internalRoadWidth = 9f;
    [SerializeField] private Vector2 blockSizeRange = new Vector2(38f, 64f);
    [SerializeField] private float buildingGap = 3f;
    [SerializeField] private float largeBuildingExtraGap = 8f;
    [SerializeField] private float hugeBuildingExtraGap = 12f;
    [SerializeField] private float largeFootprintThreshold = 45f;
    [SerializeField] private float hugeFootprintThreshold = 60f;
    [SerializeField] private float lotAreaPerBuilding = 3600f;
    [SerializeField] private int maxBuildingsPerLot = 90;
    [SerializeField] private int maxPlacementAttemptsPerLot = 1200;
    [SerializeField, Range(0.1f, 1f)] private float occupancyChance = 1f;
    [SerializeField] private Vector2 footprintWidthRange = new Vector2(16f, 40f);
    [SerializeField] private Vector2 buildingHeightRange = new Vector2(45f, 210f);

    [Header("Height Control")]
    [SerializeField] private bool useHeightPlanningBands = true;
    [SerializeField] private float globalMinimumBuildingHeight = 70f;
    [SerializeField] private float fallbackMinimumBuildingHeight = 60f;
    [SerializeField] private Vector2 transitionLotHeightRange = new Vector2(70f, 180f);
    [SerializeField] private Vector2 denseLotHeightRange = new Vector2(90f, 240f);
    [SerializeField] private Vector2 coreLotHeightRange = new Vector2(120f, 320f);
    [SerializeField] private float denseLotAreaThreshold = 30000f;
    [SerializeField] private float coreLotAreaThreshold = 70000f;
    [SerializeField] private LotHeightOverride[] lotHeightOverrides = new LotHeightOverride[0];

    [Header("Asset Pools")]
    [SerializeField] private GameObject[] buildingPrefabs;
    [SerializeField] private GameObject[] billboardPrefabs;
    [SerializeField] private float minBuildingPrefabHeight = 70f;
    [SerializeField] private float minBuildingPrefabFootprintEdge = 6f;
    [SerializeField] private bool useBuildingPrefabsWhenAvailable = true;
    [SerializeField] private bool createPlaceholdersWhenNoPrefab = true;
    [SerializeField] private PrefabFitMode prefabFitMode = PrefabFitMode.KeepPrefabScale;
    [SerializeField] private Vector2 prefabHeightScaleRange = new Vector2(1f, 1f);
    [SerializeField] private float minPrefabScale = 0.15f;
    [SerializeField] private float maxPrefabScale = 8f;

    [Header("Japanese City Rules")]
    [SerializeField, Range(0f, 0.5f)] private float maxJapaneseCityShare = 0.1f;
    [SerializeField] private bool allowJapaneseCityUniformUpscale = true;
    [SerializeField] private float japaneseCityUpscaleSourceMinHeight = 45f;
    [SerializeField] private float japaneseCityUpscaleMaxScale = 1.8f;
    [SerializeField] private float japaneseCityScaledMinFootprintEdge = 18f;
    [SerializeField] private float japaneseCityScaledMaxFootprintEdge = 62f;
    [SerializeField] private float japaneseCityUpscaledTargetMaxHeight = 140f;

    [Header("Visual Helpers")]
    [SerializeField] private bool createRoadPlaceholders = false;
    [SerializeField] private bool createBillboardAnchors = true;
    [SerializeField] private bool createVisibleBillboardPlaceholders = false;
    [SerializeField] private float roadSegmentLength = 18f;

    private readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    private struct Triangle2D
    {
        public Vector2 A;
        public Vector2 B;
        public Vector2 C;
    }

    private struct EdgeKey : IEquatable<EdgeKey>
    {
        public readonly long Ax;
        public readonly long Ay;
        public readonly long Bx;
        public readonly long By;

        public EdgeKey(Vector2 a, Vector2 b)
        {
            long ax = Quantize(a.x);
            long ay = Quantize(a.y);
            long bx = Quantize(b.x);
            long by = Quantize(b.y);

            bool swap = ax > bx || (ax == bx && ay > by);
            if (swap)
            {
                Ax = bx;
                Ay = by;
                Bx = ax;
                By = ay;
            }
            else
            {
                Ax = ax;
                Ay = ay;
                Bx = bx;
                By = by;
            }
        }

        public bool Equals(EdgeKey other)
        {
            return Ax == other.Ax && Ay == other.Ay && Bx == other.Bx && By == other.By;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeKey && Equals((EdgeKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Ax.GetHashCode();
                hash = hash * 31 + Ay.GetHashCode();
                hash = hash * 31 + Bx.GetHashCode();
                hash = hash * 31 + By.GetHashCode();
                return hash;
            }
        }

        private static long Quantize(float value)
        {
            return (long)Mathf.Round(value * 20f);
        }
    }

    private struct EdgeInfo
    {
        public Vector2 A;
        public Vector2 B;
        public int Count;
    }

    private struct AxisInterval
    {
        public float Min;
        public float Max;

        public AxisInterval(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Size
        {
            get { return Max - Min; }
        }

        public float Center
        {
            get { return (Min + Max) * 0.5f; }
        }
    }

    private sealed class AxisLayout
    {
        public readonly List<AxisInterval> Blocks = new List<AxisInterval>();
        public readonly List<AxisInterval> Roads = new List<AxisInterval>();
    }

    private sealed class LotData
    {
        public string Name;
        public Bounds Bounds;
        public float Area;
        public float GroundY;
        public Vector3 AxisU;
        public Vector3 AxisV;
        public float FacingAngle;
        public float MinU;
        public float MaxU;
        public float MinV;
        public float MaxV;
        public List<Triangle2D> Triangles = new List<Triangle2D>();
    }

    private struct PrefabFootprint
    {
        public GameObject Prefab;
        public Vector2 Size;
        public float Height;
        public float MaxEdge;
        public float MinEdge;
        public Vector2 OriginalSize;
        public float OriginalHeight;
        public float UniformScale;
        public BuildingAssetFamily Family;
        public bool IsUniformlyUpscaled;
    }

    private struct PlacedRect
    {
        public float MinU;
        public float MaxU;
        public float MinV;
        public float MaxV;

        public PlacedRect(float centerU, float centerV, float halfU, float halfV)
        {
            MinU = centerU - halfU;
            MaxU = centerU + halfU;
            MinV = centerV - halfV;
            MaxV = centerV + halfV;
        }
    }

    private struct LotHeightBand
    {
        public string Name;
        public float MinHeight;
        public float MaxHeight;
        public bool AllowFallback;

        public LotHeightBand(string name, float minHeight, float maxHeight, bool allowFallback)
        {
            Name = name;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            AllowFallback = allowFallback;
        }
    }

    private struct LotOptionStats
    {
        public int HeightRejected;
        public int JapaneseRejected;
        public int UpscaledJapaneseOptions;
        public int PrimaryOptions;
        public int JapaneseOptions;
        public bool UsedFallbackMinimum;
    }

    public Transform LotRoot
    {
        get { return lotRoot; }
        set { lotRoot = value; }
    }

    public string[] TargetLotNames
    {
        get { return targetLotNames; }
        set { targetLotNames = value; }
    }

    public GameObject[] BuildingPrefabs
    {
        get { return buildingPrefabs; }
        set { buildingPrefabs = value; }
    }

    public GameObject[] BillboardPrefabs
    {
        get { return billboardPrefabs; }
        set { billboardPrefabs = value; }
    }

    [ContextMenu("Find Lot Root")]
    public void FindLotRoot()
    {
        lotRoot = FindTransformByName(lotRootName);
    }

    [ContextMenu("Generate Asset Placement")]
    public void GeneratePlacement()
    {
        if (lotRoot == null)
        {
            FindLotRoot();
        }

        if (lotRoot == null)
        {
            Debug.LogWarning("CityLotAssetPlacer: lot root was not found.");
            return;
        }

        if (clearBeforeGenerate)
        {
            ClearPlacement();
        }

        GameObject generatedRoot = GameObject.Find(generatedRootName);
        if (generatedRoot == null)
        {
            generatedRoot = new GameObject(generatedRootName);
            generatedRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            generatedRoot.transform.localScale = Vector3.one;
            RegisterCreatedObject(generatedRoot, "Create generated city asset placement");
        }

        int totalSlots = 0;
        foreach (string lotName in targetLotNames)
        {
            Transform lotTransform = FindChildByExactName(lotRoot, lotName);
            if (lotTransform == null)
            {
                Debug.LogWarning("CityLotAssetPlacer: lot not found: " + lotName);
                continue;
            }

            LotData lot;
            if (!TryBuildLotData(lotTransform, lotName, out lot))
            {
                Debug.LogWarning("CityLotAssetPlacer: failed to read lot footprint: " + lotName);
                continue;
            }

            totalSlots += GenerateLot(lot, generatedRoot.transform);
        }

        Debug.Log("CityLotAssetPlacer: generated " + totalSlots + " building placement slots.");
        MarkSceneDirty();
    }

    [ContextMenu("Clear Asset Placement")]
    public void ClearPlacement()
    {
        GameObject existing = GameObject.Find(generatedRootName);
        if (existing == null)
        {
            return;
        }

        DestroyGeneratedObject(existing);
        MarkSceneDirty();
    }

    public void SetBuildingPrefabs(GameObject[] prefabs)
    {
        buildingPrefabs = prefabs;
    }

    public void SetBillboardPrefabs(GameObject[] prefabs)
    {
        billboardPrefabs = prefabs;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Fill Default Building Prefabs")]
    public void AutoFillDefaultBuildingPrefabs()
    {
        buildingPrefabs = FindDefaultBuildingPrefabs();
        EditorUtility.SetDirty(this);
        Debug.Log("CityLotAssetPlacer: auto-filled " + buildingPrefabs.Length + " candidate building prefabs.");
    }

    private static GameObject[] FindDefaultBuildingPrefabs()
    {
        List<GameObject> results = new List<GameObject>();
        AddPrefabsFromFolder(results, "Assets/hazelwoodloft/CITY_DATA_NEW/new_prefabs/prefabs_day_buildings_skyscrapers", new[] { "building_", "skyscraper_" }, new[] { "collider", "_col" });
        AddPrefabsFromFolder(results, "Assets/JapaneseCity/Prefabs/Buildings", new[] { "jctbld" }, new[] { "sign", "antenna", "collider", "_col" });
        results.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.name, b.name));
        return results.ToArray();
    }

    private static void AddPrefabsFromFolder(List<GameObject> results, string folder, string[] requiredPrefixes, string[] blockedNameParts)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (!HasAnyPrefix(name, requiredPrefixes) || HasAnyPart(name, blockedNameParts))
            {
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && !results.Contains(prefab))
            {
                results.Add(prefab);
            }
        }
    }

    private static bool HasAnyPrefix(string name, string[] prefixes)
    {
        for (int i = 0; i < prefixes.Length; i++)
        {
            if (name.StartsWith(prefixes[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAnyPart(string name, string[] blockedNameParts)
    {
        for (int i = 0; i < blockedNameParts.Length; i++)
        {
            if (name.Contains(blockedNameParts[i]))
            {
                return true;
            }
        }

        return false;
    }
#endif

    private int GenerateLot(LotData lot, Transform generatedRoot)
    {
        int seed = randomSeed + StableHash(lot.Name);
        System.Random random = new System.Random(seed);

        GameObject lotParent = new GameObject("AssetPlacement_" + lot.Name);
        lotParent.transform.SetParent(generatedRoot, false);
        RegisterCreatedObject(lotParent, "Create lot asset placement");

        float minU = lot.MinU + edgeSetback;
        float maxU = lot.MaxU - edgeSetback;
        float minV = lot.MinV + edgeSetback;
        float maxV = lot.MaxV - edgeSetback;

        if (useBuildingPrefabsWhenAvailable && prefabFitMode == PrefabFitMode.KeepPrefabScale)
        {
            List<PrefabFootprint> prefabOptions = BuildPrefabFootprints();
            if (prefabOptions.Count > 0)
            {
                return GenerateLotWithOriginalPrefabs(lot, lotParent.transform, random, prefabOptions, minU, maxU, minV, maxV);
            }
        }

        if (maxU - minU < blockSizeRange.x || maxV - minV < blockSizeRange.x)
        {
            return 0;
        }

        AxisLayout layoutU = BuildAxisLayout(minU, maxU, blockSizeRange, internalRoadWidth, random);
        AxisLayout layoutV = BuildAxisLayout(minV, maxV, blockSizeRange, internalRoadWidth, random);

        if (createRoadPlaceholders)
        {
            CreateRoadSegments(lot, lotParent.transform, layoutU.Roads, new AxisInterval(minV, maxV), true);
            CreateRoadSegments(lot, lotParent.transform, layoutV.Roads, new AxisInterval(minU, maxU), false);
        }

        int slotCount = 0;
        for (int u = 0; u < layoutU.Blocks.Count; u++)
        {
            for (int v = 0; v < layoutV.Blocks.Count; v++)
            {
                if (random.NextDouble() > occupancyChance)
                {
                    continue;
                }

                AxisInterval blockU = layoutU.Blocks[u];
                AxisInterval blockV = layoutV.Blocks[v];
                if (TryCreateBuildingSlot(lot, lotParent.transform, blockU, blockV, slotCount, random))
                {
                    slotCount++;
                }
            }
        }

        return slotCount;
    }

    private int GenerateLotWithOriginalPrefabs(LotData lot, Transform parent, System.Random random, List<PrefabFootprint> prefabOptions, float minU, float maxU, float minV, float maxV)
    {
        float usableU = maxU - minU;
        float usableV = maxV - minV;
        if (usableU <= 0f || usableV <= 0f)
        {
            Debug.LogWarning("CityLotAssetPlacer: " + lot.Name + " has no usable area after edge setback.");
            return 0;
        }

        int targetCount = DetermineTargetBuildingCount(lot, usableU, usableV);
        int maxAttempts = Mathf.Max(maxPlacementAttemptsPerLot, targetCount * 90);
        List<PlacedRect> placedRects = new List<PlacedRect>();
        LotHeightBand heightBand = DetermineLotHeightBand(lot, usableU, usableV);
        LotOptionStats optionStats;
        List<PrefabFootprint> lotOptions = BuildLotPrefabOptions(prefabOptions, heightBand, out optionStats);

        if (lotOptions.Count == 0 && heightBand.AllowFallback && heightBand.MinHeight > fallbackMinimumBuildingHeight)
        {
            heightBand = CreateHeightBand(heightBand.Name + " fallback", fallbackMinimumBuildingHeight, heightBand.MaxHeight, true);
            lotOptions = BuildLotPrefabOptions(prefabOptions, heightBand, out optionStats);
            optionStats.UsedFallbackMinimum = true;
        }

        if (lotOptions.Count == 0)
        {
            Debug.LogWarning("CityLotAssetPlacer: " + lot.Name + " has no building prefabs within height band " + FormatHeightBand(heightBand) + ".");
            return 0;
        }

        int boundaryRejects = 0;
        int overlapRejects = 0;
        int tooLargeRejects = 0;
        int japanesePlaced = 0;
        int upscaledJapanesePlaced = 0;
        int japaneseLimit = Mathf.Max(0, Mathf.FloorToInt(targetCount * maxJapaneseCityShare));

        for (int attempt = 0; attempt < maxAttempts && placedRects.Count < targetCount; attempt++)
        {
            PrefabFootprint prefabFootprint;
            if (!TryPickPrefabForLot(lotOptions, lot, usableU, usableV, placedRects.Count, targetCount, japanesePlaced, japaneseLimit, random, out prefabFootprint))
            {
                tooLargeRejects++;
                continue;
            }

            bool startRotated = random.NextDouble() > 0.5;
            bool accepted = false;

            for (int rotationTry = 0; rotationTry < 2 && !accepted; rotationTry++)
            {
                bool rotate90 = rotationTry == 0 ? startRotated : !startRotated;
                Vector2 footprint = GetFootprintForRotation(prefabFootprint, rotate90);
                float clearance = GetPrefabClearance(prefabFootprint);
                float halfU = footprint.x * 0.5f;
                float halfV = footprint.y * 0.5f;
                float clearHalf = clearance * 0.5f;

                if (footprint.x + clearance > usableU || footprint.y + clearance > usableV)
                {
                    tooLargeRejects++;
                    continue;
                }

                float centerMinU = minU + halfU + clearHalf;
                float centerMaxU = maxU - halfU - clearHalf;
                float centerMinV = minV + halfV + clearHalf;
                float centerMaxV = maxV - halfV - clearHalf;
                if (centerMinU >= centerMaxU || centerMinV >= centerMaxV)
                {
                    tooLargeRejects++;
                    continue;
                }

                float centerU = RandomRange(random, centerMinU, centerMaxU);
                float centerV = RandomRange(random, centerMinV, centerMaxV);
                if (!IsOrientedRectInsideLot(lot, centerU, centerV, halfU + clearHalf, halfV + clearHalf))
                {
                    boundaryRejects++;
                    continue;
                }

                PlacedRect testRect = new PlacedRect(centerU, centerV, halfU + clearHalf, halfV + clearHalf);
                if (IntersectsAny(testRect, placedRects))
                {
                    overlapRejects++;
                    continue;
                }

                CreateOriginalPrefabSlot(lot, parent, placedRects.Count, prefabFootprint, footprint, centerU, centerV, rotate90, random);
                placedRects.Add(testRect);
                if (prefabFootprint.Family == BuildingAssetFamily.JapaneseCity)
                {
                    japanesePlaced++;
                    if (prefabFootprint.IsUniformlyUpscaled)
                    {
                        upscaledJapanesePlaced++;
                    }
                }
                accepted = true;
            }
        }

        if (placedRects.Count == 0)
        {
            Debug.LogWarning("CityLotAssetPlacer: " + lot.Name + " placed 0 original-scale buildings. The imported prefabs may be too large for this lot.");
        }
        else
        {
            Debug.Log("CityLotAssetPlacer: " + lot.Name + " band " + FormatHeightBand(heightBand) + " placed " + placedRects.Count + " buildings. Target " + targetCount + ", primary options " + optionStats.PrimaryOptions + ", Japanese options " + optionStats.JapaneseOptions + ", Japanese placed " + japanesePlaced + ", upscaled Japanese " + upscaledJapanesePlaced + ", rejected height " + optionStats.HeightRejected + ", rejected Japanese " + optionStats.JapaneseRejected + ", boundary " + boundaryRejects + ", overlap " + overlapRejects + ", too large " + tooLargeRejects + ".");
        }

        return placedRects.Count;
    }

    private void CreateOriginalPrefabSlot(LotData lot, Transform parent, int slotIndex, PrefabFootprint prefabFootprint, Vector2 footprint, float centerU, float centerV, bool rotate90, System.Random random)
    {
        Vector3 slotWorldPosition = ToWorld(lot, centerU, centerV, lot.GroundY);
        Quaternion slotRotation = Quaternion.LookRotation(lot.AxisV, Vector3.up);

        GameObject slotObject = new GameObject("BuildingSlot_" + lot.Name + "_" + slotIndex.ToString("00"));
        slotObject.transform.SetParent(parent, true);
        slotObject.transform.SetPositionAndRotation(slotWorldPosition, slotRotation);
        RegisterCreatedObject(slotObject, "Create original-scale city building slot");

        GameObject content = new GameObject("Content");
        content.transform.SetParent(slotObject.transform, false);
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;
        RegisterCreatedObject(content, "Create city building content root");

        Transform billboardAnchor = null;
        if (createBillboardAnchors)
        {
            billboardAnchor = CreateBillboardAnchor(lot, slotObject.transform, footprint.x, footprint.y, prefabFootprint.Height, slotIndex, random);
        }

        CityBuildingPlacementSlot marker = slotObject.AddComponent<CityBuildingPlacementSlot>();
        marker.Configure(lot.Name, slotIndex, footprint, prefabFootprint.Height, lot.FacingAngle, content.transform, billboardAnchor);

        GameObject instance = InstantiatePrefabForSlot(prefabFootprint.Prefab, content.transform);
        if (rotate90)
        {
            instance.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }
        if (!Mathf.Approximately(prefabFootprint.UniformScale, 1f))
        {
            instance.transform.localScale = instance.transform.localScale * prefabFootprint.UniformScale;
        }

        AlignInstanceFootprintCenterAndBottom(instance, slotWorldPosition, lot.GroundY);
    }

    private List<PrefabFootprint> BuildPrefabFootprints()
    {
        List<PrefabFootprint> footprints = new List<PrefabFootprint>();
        if (buildingPrefabs == null)
        {
            return footprints;
        }

        for (int i = 0; i < buildingPrefabs.Length; i++)
        {
            GameObject prefab = buildingPrefabs[i];
            if (prefab == null)
            {
                continue;
            }

            PrefabFootprint footprint;
            if (TryBuildPrefabFootprint(prefab, out footprint))
            {
                footprints.Add(footprint);
            }
        }

        return footprints;
    }

    private List<PrefabFootprint> BuildLotPrefabOptions(List<PrefabFootprint> sourceOptions, LotHeightBand heightBand, out LotOptionStats stats)
    {
        stats = new LotOptionStats();
        List<PrefabFootprint> options = new List<PrefabFootprint>();

        for (int i = 0; i < sourceOptions.Count; i++)
        {
            PrefabFootprint option;
            if (TryPreparePrefabForHeightBand(sourceOptions[i], heightBand, out option, ref stats))
            {
                options.Add(option);
                if (option.Family == BuildingAssetFamily.JapaneseCity)
                {
                    stats.JapaneseOptions++;
                    if (option.IsUniformlyUpscaled)
                    {
                        stats.UpscaledJapaneseOptions++;
                    }
                }
                else if (option.Family == BuildingAssetFamily.PrimaryCity)
                {
                    stats.PrimaryOptions++;
                }
            }
        }

        return options;
    }

    private bool TryPreparePrefabForHeightBand(PrefabFootprint source, LotHeightBand heightBand, out PrefabFootprint option, ref LotOptionStats stats)
    {
        option = source;
        option.Size = source.OriginalSize;
        option.Height = source.OriginalHeight;
        option.MaxEdge = Mathf.Max(option.Size.x, option.Size.y);
        option.MinEdge = Mathf.Min(option.Size.x, option.Size.y);
        option.UniformScale = 1f;
        option.IsUniformlyUpscaled = false;

        if (float.IsPositiveInfinity(heightBand.MaxHeight))
        {
            return option.Height >= heightBand.MinHeight;
        }

        if (option.Height >= heightBand.MinHeight && option.Height <= heightBand.MaxHeight)
        {
            return true;
        }

        if (option.Family == BuildingAssetFamily.JapaneseCity)
        {
            if (TryUpscaleJapaneseCityPrefab(source, heightBand, out option))
            {
                return true;
            }

            stats.JapaneseRejected++;
            return false;
        }

        stats.HeightRejected++;
        return false;
    }

    private bool TryUpscaleJapaneseCityPrefab(PrefabFootprint source, LotHeightBand heightBand, out PrefabFootprint option)
    {
        option = source;
        if (!allowJapaneseCityUniformUpscale)
        {
            return false;
        }

        if (source.OriginalHeight < japaneseCityUpscaleSourceMinHeight || source.OriginalHeight >= heightBand.MinHeight)
        {
            return false;
        }

        float targetHeight = Mathf.Min(heightBand.MinHeight, japaneseCityUpscaledTargetMaxHeight);
        float scale = targetHeight / Mathf.Max(0.01f, source.OriginalHeight);
        if (scale <= 1f || scale > japaneseCityUpscaleMaxScale)
        {
            return false;
        }

        Vector2 scaledSize = source.OriginalSize * scale;
        float scaledMaxEdge = Mathf.Max(scaledSize.x, scaledSize.y);
        float scaledMinEdge = Mathf.Min(scaledSize.x, scaledSize.y);
        if (scaledMaxEdge < japaneseCityScaledMinFootprintEdge || scaledMaxEdge > japaneseCityScaledMaxFootprintEdge)
        {
            return false;
        }
        if (scaledMinEdge < minBuildingPrefabFootprintEdge)
        {
            return false;
        }

        option.Size = scaledSize;
        option.Height = source.OriginalHeight * scale;
        option.MaxEdge = scaledMaxEdge;
        option.MinEdge = scaledMinEdge;
        option.UniformScale = scale;
        option.IsUniformlyUpscaled = true;
        return option.Height >= heightBand.MinHeight && option.Height <= heightBand.MaxHeight;
    }

    private bool TryBuildPrefabFootprint(GameObject prefab, out PrefabFootprint footprint)
    {
        footprint = new PrefabFootprint();
        Bounds bounds;
        if (!TryGetRendererBounds(prefab.transform, out bounds))
        {
            return false;
        }

        Vector2 size = new Vector2(Mathf.Max(0.1f, bounds.size.x), Mathf.Max(0.1f, bounds.size.z));
        float height = Mathf.Max(0.1f, bounds.size.y);
        if (height < Mathf.Min(minBuildingPrefabHeight, japaneseCityUpscaleSourceMinHeight) || Mathf.Max(size.x, size.y) < minBuildingPrefabFootprintEdge)
        {
            return false;
        }

        footprint = new PrefabFootprint
        {
            Prefab = prefab,
            Size = size,
            Height = height,
            MaxEdge = Mathf.Max(size.x, size.y),
            MinEdge = Mathf.Min(size.x, size.y),
            OriginalSize = size,
            OriginalHeight = height,
            UniformScale = 1f,
            Family = DetectBuildingAssetFamily(prefab),
            IsUniformlyUpscaled = false
        };
        return true;
    }

    private static BuildingAssetFamily DetectBuildingAssetFamily(GameObject prefab)
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(prefab).Replace("\\", "/").ToLowerInvariant();
        if (path.Contains("/japanesecity/"))
        {
            return BuildingAssetFamily.JapaneseCity;
        }
        if (path.Contains("/hazelwoodloft/"))
        {
            return BuildingAssetFamily.PrimaryCity;
        }
#endif

        string name = prefab != null ? prefab.name.ToLowerInvariant() : string.Empty;
        if (name.StartsWith("jctbld"))
        {
            return BuildingAssetFamily.JapaneseCity;
        }
        if (name.StartsWith("building_") || name.StartsWith("skyscraper_"))
        {
            return BuildingAssetFamily.PrimaryCity;
        }

        return BuildingAssetFamily.Other;
    }

    private bool TryPickPrefabForLot(List<PrefabFootprint> options, LotData lot, float usableU, float usableV, int placedCount, int targetCount, int japanesePlaced, int japaneseLimit, System.Random random, out PrefabFootprint selected)
    {
        selected = new PrefabFootprint();
        float totalWeight = 0f;
        float[] weights = new float[options.Count];

        for (int i = 0; i < options.Count; i++)
        {
            PrefabFootprint option = options[i];
            if (option.Family == BuildingAssetFamily.JapaneseCity && japanesePlaced >= japaneseLimit)
            {
                weights[i] = 0f;
                continue;
            }

            if (!CanPrefabFitUsableArea(option, usableU, usableV))
            {
                weights[i] = 0f;
                continue;
            }

            float weight = GetPrefabWeightForLot(option, lot, usableU, usableV, placedCount, targetCount);
            weights[i] = Mathf.Max(0f, weight);
            totalWeight += weights[i];
        }

        if (totalWeight <= 0f)
        {
            return false;
        }

        float roll = RandomRange(random, 0f, totalWeight);
        float cursor = 0f;
        for (int i = 0; i < options.Count; i++)
        {
            cursor += weights[i];
            if (roll <= cursor)
            {
                selected = options[i];
                return true;
            }
        }

        selected = options[options.Count - 1];
        return true;
    }

    private bool CanPrefabFitUsableArea(PrefabFootprint option, float usableU, float usableV)
    {
        float clearance = GetPrefabClearance(option);
        bool normalFits = option.Size.x + clearance <= usableU && option.Size.y + clearance <= usableV;
        bool rotatedFits = option.Size.y + clearance <= usableU && option.Size.x + clearance <= usableV;
        return normalFits || rotatedFits;
    }

    private LotHeightBand DetermineLotHeightBand(LotData lot, float usableU, float usableV)
    {
        LotHeightOverride overrideRule;
        LotHeightBand overrideBand;
        if (TryGetLotHeightOverride(lot, usableU, usableV, out overrideRule, out overrideBand))
        {
            return overrideBand;
        }

        if (!useHeightPlanningBands)
        {
            float minimum = Mathf.Max(globalMinimumBuildingHeight, minBuildingPrefabHeight);
            return CreateHeightBand("global", minimum, float.PositiveInfinity, true);
        }

        return DetermineAutomaticLotHeightBand(lot, usableU, usableV);
    }

    private LotHeightBand DetermineAutomaticLotHeightBand(LotData lot, float usableU, float usableV)
    {
        float area = Mathf.Max(lot.Area, usableU * usableV * 0.45f);
        if (area >= coreLotAreaThreshold)
        {
            return CreateHeightBand("core", Mathf.Max(globalMinimumBuildingHeight, coreLotHeightRange.x), coreLotHeightRange.y, true);
        }

        if (area >= denseLotAreaThreshold)
        {
            return CreateHeightBand("dense", Mathf.Max(globalMinimumBuildingHeight, denseLotHeightRange.x), denseLotHeightRange.y, true);
        }

        return CreateHeightBand("transition", Mathf.Max(globalMinimumBuildingHeight, transitionLotHeightRange.x), transitionLotHeightRange.y, true);
    }

    private bool TryGetLotHeightOverride(LotData lot, float usableU, float usableV, out LotHeightOverride overrideRule, out LotHeightBand heightBand)
    {
        overrideRule = null;
        heightBand = default(LotHeightBand);
        if (lotHeightOverrides == null || lot == null || string.IsNullOrEmpty(lot.Name))
        {
            return false;
        }

        for (int i = 0; i < lotHeightOverrides.Length; i++)
        {
            LotHeightOverride candidate = lotHeightOverrides[i];
            if (candidate == null || string.IsNullOrEmpty(candidate.LotName))
            {
                continue;
            }

            if (!string.Equals(candidate.LotName.Trim(), lot.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            overrideRule = candidate;
            if (candidate.HeightMode == LotHeightMode.AutoByArea)
            {
                heightBand = DetermineAutomaticLotHeightBand(lot, usableU, usableV);
                return true;
            }

            heightBand = CreateHeightBandForOverride(candidate);
            return true;
        }

        return false;
    }

    private LotHeightBand CreateHeightBandForOverride(LotHeightOverride overrideRule)
    {
        switch (overrideRule.HeightMode)
        {
            case LotHeightMode.LowRiseDistrict:
                return CreateHeightBand("override low-rise", 60f, 120f, false);
            case LotHeightMode.MidRiseDistrict:
                return CreateHeightBand("override mid-rise", 80f, 180f, false);
            case LotHeightMode.HighRiseDistrict:
                return CreateHeightBand("override high-rise", 120f, 260f, false);
            case LotHeightMode.LandmarkCore:
                return CreateHeightBand("override landmark", 180f, 360f, false);
            case LotHeightMode.Custom:
                return CreateHeightBand("override custom", overrideRule.MinHeight, overrideRule.MaxHeight, false);
            default:
                return CreateHeightBand("override custom", overrideRule.MinHeight, overrideRule.MaxHeight, false);
        }
    }

    private static LotHeightBand CreateHeightBand(string name, float minHeight, float maxHeight, bool allowFallback)
    {
        float cleanMin = Mathf.Max(0f, minHeight);
        float cleanMax = maxHeight;
        if (!float.IsPositiveInfinity(cleanMax) && cleanMax < cleanMin)
        {
            float oldMin = cleanMin;
            cleanMin = Mathf.Max(0f, cleanMax);
            cleanMax = oldMin;
        }

        if (!float.IsPositiveInfinity(cleanMax))
        {
            cleanMax = Mathf.Max(cleanMin + 1f, cleanMax);
        }

        return new LotHeightBand(name, cleanMin, cleanMax, allowFallback);
    }

    private static string FormatHeightBand(LotHeightBand heightBand)
    {
        if (float.IsPositiveInfinity(heightBand.MaxHeight))
        {
            return heightBand.Name + " " + heightBand.MinHeight.ToString("0") + "m+";
        }

        return heightBand.Name + " " + heightBand.MinHeight.ToString("0") + "-" + heightBand.MaxHeight.ToString("0") + "m";
    }

    private float GetPrefabWeightForLot(PrefabFootprint option, LotData lot, float usableU, float usableV, int placedCount, int targetCount)
    {
        float lotArea = Mathf.Max(lot.Area, usableU * usableV * 0.5f);
        float weight = 1f;

        bool isSmall = option.MaxEdge <= 28f;
        bool isLarge = option.MaxEdge >= largeFootprintThreshold || option.Height >= 130f;
        bool isHuge = option.MaxEdge >= hugeFootprintThreshold || option.Height >= 210f;

        if (option.Family == BuildingAssetFamily.PrimaryCity)
        {
            weight *= 4f;
        }
        else if (option.Family == BuildingAssetFamily.JapaneseCity)
        {
            weight *= option.IsUniformlyUpscaled ? 0.2f : 0.35f;
        }

        if (lotArea < 30000f)
        {
            weight *= isSmall ? 0.75f : 1f;
            if (isLarge)
            {
                weight *= 0.8f;
            }
            if (isHuge)
            {
                weight *= 0.16f;
            }
        }
        else if (lotArea > 70000f)
        {
            if (isLarge)
            {
                weight *= 1.8f;
            }
            if (isHuge)
            {
                weight *= 1.25f;
            }
            if (isSmall)
            {
                weight *= 0.85f;
            }
        }
        else
        {
            if (isSmall)
            {
                weight *= 1.35f;
            }
            if (isHuge)
            {
                weight *= 0.55f;
            }
        }

        if (targetCount > 0 && placedCount > targetCount * 0.68f && isHuge)
        {
            weight *= 0.35f;
        }

        if (option.MaxEdge > Mathf.Min(usableU, usableV) * 0.72f)
        {
            weight *= 0.22f;
        }

        if (option.MinEdge <= 18f)
        {
            weight *= 0.55f;
        }

        return weight;
    }

    private int DetermineTargetBuildingCount(LotData lot, float usableU, float usableV)
    {
        float area = Mathf.Max(lot.Area, usableU * usableV * 0.45f);
        int count = Mathf.RoundToInt(area / Mathf.Max(500f, lotAreaPerBuilding));
        return Mathf.Clamp(count, 3, Mathf.Max(3, maxBuildingsPerLot));
    }

    private float GetPrefabClearance(PrefabFootprint option)
    {
        if (option.MaxEdge >= hugeFootprintThreshold || option.Height >= 210f)
        {
            return Mathf.Max(buildingGap, hugeBuildingExtraGap);
        }

        if (option.MaxEdge >= largeFootprintThreshold || option.Height >= 130f)
        {
            return Mathf.Max(buildingGap, largeBuildingExtraGap);
        }

        return Mathf.Max(0f, buildingGap);
    }

    private static Vector2 GetFootprintForRotation(PrefabFootprint option, bool rotate90)
    {
        return rotate90 ? new Vector2(option.Size.y, option.Size.x) : option.Size;
    }

    private static bool IntersectsAny(PlacedRect rect, List<PlacedRect> placedRects)
    {
        for (int i = 0; i < placedRects.Count; i++)
        {
            PlacedRect other = placedRects[i];
            bool separated = rect.MaxU <= other.MinU || rect.MinU >= other.MaxU || rect.MaxV <= other.MinV || rect.MinV >= other.MaxV;
            if (!separated)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryCreateBuildingSlot(LotData lot, Transform parent, AxisInterval blockU, AxisInterval blockV, int slotIndex, System.Random random)
    {
        float maxFootU = blockU.Size - buildingGap * 2f;
        float maxFootV = blockV.Size - buildingGap * 2f;
        if (maxFootU < footprintWidthRange.x || maxFootV < footprintWidthRange.x)
        {
            return false;
        }

        bool makeSlab = random.NextDouble() > 0.62;
        float footU = Mathf.Min(maxFootU, RandomRange(random, footprintWidthRange.x, footprintWidthRange.y));
        float footV = Mathf.Min(maxFootV, RandomRange(random, footprintWidthRange.x, footprintWidthRange.y));

        if (makeSlab)
        {
            if (maxFootU > maxFootV)
            {
                footU = Mathf.Min(maxFootU, footU * RandomRange(random, 1.25f, 1.65f));
                footV = Mathf.Min(maxFootV, footV * RandomRange(random, 0.72f, 0.92f));
            }
            else
            {
                footV = Mathf.Min(maxFootV, footV * RandomRange(random, 1.25f, 1.65f));
                footU = Mathf.Min(maxFootU, footU * RandomRange(random, 0.72f, 0.92f));
            }
        }

        float halfU = footU * 0.5f;
        float halfV = footV * 0.5f;
        float centerU = blockU.Center;
        float centerV = blockV.Center;
        bool foundPosition = false;

        for (int attempt = 0; attempt < 8; attempt++)
        {
            float jitterU = Mathf.Max(0f, (maxFootU - footU) * 0.38f);
            float jitterV = Mathf.Max(0f, (maxFootV - footV) * 0.38f);
            float testU = blockU.Center + RandomRange(random, -jitterU, jitterU);
            float testV = blockV.Center + RandomRange(random, -jitterV, jitterV);

            if (IsOrientedRectInsideLot(lot, testU, testV, halfU + buildingGap * 0.35f, halfV + buildingGap * 0.35f))
            {
                centerU = testU;
                centerV = testV;
                foundPosition = true;
                break;
            }
        }

        if (!foundPosition)
        {
            return false;
        }

        float lotAreaFactor = Mathf.Sqrt(Mathf.Max(1f, lot.Bounds.size.x * lot.Bounds.size.z));
        float heightBias = Mathf.InverseLerp(120f, 540f, lotAreaFactor);
        float minHeight = Mathf.Lerp(buildingHeightRange.x, buildingHeightRange.x * 1.4f, heightBias);
        float maxHeight = Mathf.Lerp(buildingHeightRange.y * 0.68f, buildingHeightRange.y, heightBias);
        float height = RandomRange(random, minHeight, maxHeight);

        Vector3 slotWorldPosition = ToWorld(lot, centerU, centerV, lot.GroundY);
        Quaternion slotRotation = Quaternion.LookRotation(lot.AxisV, Vector3.up);

        GameObject slotObject = new GameObject("BuildingSlot_" + lot.Name + "_" + slotIndex.ToString("00"));
        slotObject.transform.SetParent(parent, true);
        slotObject.transform.SetPositionAndRotation(slotWorldPosition, slotRotation);
        RegisterCreatedObject(slotObject, "Create city building slot");

        GameObject content = new GameObject("Content");
        content.transform.SetParent(slotObject.transform, false);
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;
        RegisterCreatedObject(content, "Create city building content root");

        Transform billboardAnchor = null;
        if (createBillboardAnchors)
        {
            billboardAnchor = CreateBillboardAnchor(lot, slotObject.transform, footU, footV, height, slotIndex, random);
        }

        CityBuildingPlacementSlot marker = slotObject.AddComponent<CityBuildingPlacementSlot>();
        marker.Configure(lot.Name, slotIndex, new Vector2(footU, footV), height, lot.FacingAngle, content.transform, billboardAnchor);

        GameObject selectedPrefab = PickPrefab(buildingPrefabs, random);
        if (useBuildingPrefabsWhenAvailable && selectedPrefab != null)
        {
            GameObject instance = InstantiatePrefabForSlot(selectedPrefab, content.transform);
            float heightScale = RandomRange(random, Mathf.Min(prefabHeightScaleRange.x, prefabHeightScaleRange.y), Mathf.Max(prefabHeightScaleRange.x, prefabHeightScaleRange.y));
            FitPrefabInstance(instance, footU, footV, heightScale, lot.GroundY);
        }
        else if (createPlaceholdersWhenNoPrefab)
        {
            CreatePlaceholderBuilding(content.transform, footU, footV, height, random);
        }

        return true;
    }

    private Transform CreateBillboardAnchor(LotData lot, Transform slotTransform, float footU, float footV, float height, int slotIndex, System.Random random)
    {
        GameObject anchor = new GameObject("BillboardAnchor_Front");
        anchor.transform.SetParent(slotTransform, false);
        anchor.transform.localPosition = new Vector3(0f, Mathf.Max(12f, height * 0.45f), footV * 0.5f + 0.35f);
        anchor.transform.localRotation = Quaternion.identity;
        RegisterCreatedObject(anchor, "Create billboard anchor");

        GameObject billboardPrefab = PickPrefab(billboardPrefabs, random);
        if (billboardPrefab != null)
        {
            GameObject instance = InstantiatePrefabForSlot(billboardPrefab, anchor.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
        }
        else if (createVisibleBillboardPlaceholders)
        {
            float width = Mathf.Clamp(footU * 0.46f, 8f, 28f);
            float billboardHeight = Mathf.Clamp(height * 0.16f, 5f, 18f);
            GameObject panel = CreateLocalBox("BillboardPlaceholder", anchor.transform, new Vector3(0f, 0f, 0f), new Vector3(width, billboardHeight, 0.45f), GetEmissionMaterial("asset_billboard_placeholder", new Color(0.05f, 0.75f, 1f), 2.2f));
            panel.transform.localRotation = Quaternion.identity;
        }

        return anchor.transform;
    }

    private void CreatePlaceholderBuilding(Transform parent, float footU, float footV, float height, System.Random random)
    {
        Color[] colors =
        {
            new Color(0.40f, 0.44f, 0.46f),
            new Color(0.16f, 0.24f, 0.30f),
            new Color(0.50f, 0.52f, 0.50f),
            new Color(0.22f, 0.27f, 0.33f)
        };

        Color bodyColor = colors[random.Next(colors.Length)];
        Material bodyMaterial = GetMaterial("asset_placeholder_" + random.Next(colors.Length), bodyColor);
        Material darkMaterial = GetMaterial("asset_placeholder_roof", new Color(0.06f, 0.07f, 0.08f));
        Material windowMaterial = GetEmissionMaterial("asset_placeholder_window", new Color(0.95f, 0.76f, 0.44f), 0.8f);

        float podiumHeight = Mathf.Clamp(height * 0.16f, 8f, 22f);
        CreateLocalBox("PodiumPlaceholder", parent, new Vector3(0f, podiumHeight * 0.5f, 0f), new Vector3(footU, podiumHeight, footV), bodyMaterial);

        float towerHeight = height - podiumHeight;
        float towerU = footU * RandomRange(random, 0.62f, 0.86f);
        float towerV = footV * RandomRange(random, 0.62f, 0.86f);
        CreateLocalBox("TowerPlaceholder", parent, new Vector3(0f, podiumHeight + towerHeight * 0.5f, 0f), new Vector3(towerU, towerHeight, towerV), bodyMaterial);

        int facadeBands = Mathf.Clamp(Mathf.RoundToInt(towerU / 12f), 2, 5);
        for (int i = 0; i < facadeBands; i++)
        {
            float t = facadeBands == 1 ? 0.5f : i / (float)(facadeBands - 1);
            float x = Mathf.Lerp(-towerU * 0.35f, towerU * 0.35f, t);
            CreateLocalBox("WindowBand_" + i, parent, new Vector3(x, podiumHeight + towerHeight * 0.53f, towerV * 0.5f + 0.08f), new Vector3(1.4f, towerHeight * 0.72f, 0.18f), windowMaterial);
        }

        CreateLocalBox("RoofDetail", parent, new Vector3(towerU * 0.18f, height + 3f, -towerV * 0.12f), new Vector3(towerU * 0.24f, 6f, towerV * 0.22f), darkMaterial);
    }

    private void CreateRoadSegments(LotData lot, Transform parent, List<AxisInterval> roadIntervals, AxisInterval crossRange, bool roadRunsAlongV)
    {
        Material roadMaterial = GetMaterial("asset_internal_road", new Color(0.075f, 0.08f, 0.085f));
        Quaternion rotation = Quaternion.LookRotation(lot.AxisV, Vector3.up);
        float step = Mathf.Max(4f, roadSegmentLength);

        for (int i = 0; i < roadIntervals.Count; i++)
        {
            AxisInterval road = roadIntervals[i];
            float cursor = crossRange.Min;
            while (cursor < crossRange.Max)
            {
                float segmentEnd = Mathf.Min(cursor + step, crossRange.Max);
                float segmentCenter = (cursor + segmentEnd) * 0.5f;
                float segmentSize = segmentEnd - cursor;

                float centerU = roadRunsAlongV ? road.Center : segmentCenter;
                float centerV = roadRunsAlongV ? segmentCenter : road.Center;
                float halfU = roadRunsAlongV ? road.Size * 0.5f : segmentSize * 0.5f;
                float halfV = roadRunsAlongV ? segmentSize * 0.5f : road.Size * 0.5f;

                if (IsOrientedRectInsideLot(lot, centerU, centerV, halfU * 0.92f, halfV * 0.92f))
                {
                    Vector3 center = ToWorld(lot, centerU, centerV, lot.GroundY + 0.035f);
                    Vector3 scale = roadRunsAlongV
                        ? new Vector3(road.Size, 0.07f, segmentSize)
                        : new Vector3(segmentSize, 0.07f, road.Size);
                    GameObject roadObject = CreateBox("InternalRoad", parent, center, scale, roadMaterial);
                    roadObject.transform.rotation = rotation;
                }

                cursor = segmentEnd;
            }
        }
    }

    private GameObject InstantiatePrefabForSlot(GameObject prefab, Transform parent)
    {
#if UNITY_EDITOR
        GameObject instance;
        if (!Application.isPlaying)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate city asset prefab");
        }
        else
        {
            instance = Instantiate(prefab, parent);
        }
#else
        GameObject instance = Instantiate(prefab, parent);
#endif
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        return instance;
    }

    private void FitPrefabInstance(GameObject instance, float targetU, float targetV, float heightScale, float groundY)
    {
        if (instance == null)
        {
            return;
        }

        if (prefabFitMode == PrefabFitMode.KeepPrefabScale)
        {
            AlignInstanceBottom(instance, groundY);
            return;
        }

        Bounds bounds;
        if (!TryGetRendererBounds(instance.transform, out bounds))
        {
            return;
        }

        float currentU = Mathf.Max(0.01f, bounds.size.x);
        float currentV = Mathf.Max(0.01f, bounds.size.z);
        Vector3 scale = instance.transform.localScale;
        float safeHeightScale = Mathf.Max(0.01f, heightScale);

        if (prefabFitMode == PrefabFitMode.StretchFitFootprintKeepHeight)
        {
            scale.x *= Mathf.Clamp(targetU / currentU, minPrefabScale, maxPrefabScale);
            scale.z *= Mathf.Clamp(targetV / currentV, minPrefabScale, maxPrefabScale);
            scale.y *= safeHeightScale;
        }
        else if (prefabFitMode == PrefabFitMode.UniformFitAllAxes)
        {
            float factor = Mathf.Min(targetU / currentU, targetV / currentV);
            factor = Mathf.Clamp(factor, minPrefabScale, maxPrefabScale);
            scale *= factor;
            scale.y *= safeHeightScale;
        }
        else
        {
            float factor = Mathf.Min(targetU / currentU, targetV / currentV);
            factor = Mathf.Clamp(factor, minPrefabScale, maxPrefabScale);
            scale.x *= factor;
            scale.z *= factor;
            scale.y *= safeHeightScale;
        }

        instance.transform.localScale = scale;
        AlignInstanceBottom(instance, groundY);
    }

    private void AlignInstanceBottom(GameObject instance, float groundY)
    {
        Bounds bounds;
        if (!TryGetRendererBounds(instance.transform, out bounds))
        {
            return;
        }

        Vector3 offset = new Vector3(0f, groundY - bounds.min.y, 0f);
        instance.transform.position += offset;
    }

    private void AlignInstanceFootprintCenterAndBottom(GameObject instance, Vector3 footprintCenter, float groundY)
    {
        Bounds bounds;
        if (!TryGetRendererBounds(instance.transform, out bounds))
        {
            return;
        }

        Vector3 offset = new Vector3(footprintCenter.x - bounds.center.x, groundY - bounds.min.y, footprintCenter.z - bounds.center.z);
        instance.transform.position += offset;
    }

    private static AxisLayout BuildAxisLayout(float min, float max, Vector2 blockRange, float roadWidth, System.Random random)
    {
        AxisLayout layout = new AxisLayout();
        float cursor = min;
        float minBlock = Mathf.Max(16f, blockRange.x);
        float maxBlock = Mathf.Max(minBlock, blockRange.y);

        while (cursor < max)
        {
            float remaining = max - cursor;
            if (remaining < minBlock)
            {
                break;
            }

            if (remaining <= maxBlock * 1.35f)
            {
                layout.Blocks.Add(new AxisInterval(cursor, max));
                break;
            }

            float blockSize = Mathf.Min(remaining, RandomRange(random, minBlock, maxBlock));
            if (remaining - blockSize - roadWidth < minBlock)
            {
                layout.Blocks.Add(new AxisInterval(cursor, max));
                break;
            }

            layout.Blocks.Add(new AxisInterval(cursor, cursor + blockSize));
            cursor += blockSize;

            layout.Roads.Add(new AxisInterval(cursor, Mathf.Min(cursor + roadWidth, max)));
            cursor += roadWidth;
        }

        return layout;
    }

    private bool TryBuildLotData(Transform lotTransform, string lotName, out LotData lot)
    {
        lot = null;

        Bounds bounds;
        if (!TryGetRendererBounds(lotTransform, out bounds))
        {
            return false;
        }

        List<Triangle2D> triangles = CollectProjectedTopTriangles(lotTransform, bounds, true);
        if (triangles.Count == 0)
        {
            triangles = CollectProjectedTopTriangles(lotTransform, bounds, false);
        }
        if (triangles.Count == 0)
        {
            triangles = BuildBoundsFallbackTriangles(bounds);
        }

        float angle = EstimateDominantEdgeAngle(triangles);
        Vector3 axisU = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        Vector3 axisV = new Vector3(-axisU.z, 0f, axisU.x).normalized;

        float minU = float.PositiveInfinity;
        float maxU = float.NegativeInfinity;
        float minV = float.PositiveInfinity;
        float maxV = float.NegativeInfinity;

        for (int i = 0; i < triangles.Count; i++)
        {
            ExpandLocalExtents(triangles[i].A, axisU, axisV, ref minU, ref maxU, ref minV, ref maxV);
            ExpandLocalExtents(triangles[i].B, axisU, axisV, ref minU, ref maxU, ref minV, ref maxV);
            ExpandLocalExtents(triangles[i].C, axisU, axisV, ref minU, ref maxU, ref minV, ref maxV);
        }

        float area = 0f;
        for (int i = 0; i < triangles.Count; i++)
        {
            area += TriangleArea(triangles[i]);
        }

        lot = new LotData
        {
            Name = lotName,
            Bounds = bounds,
            Area = area,
            GroundY = bounds.max.y,
            AxisU = axisU,
            AxisV = axisV,
            FacingAngle = angle,
            MinU = minU,
            MaxU = maxU,
            MinV = minV,
            MaxV = maxV,
            Triangles = triangles
        };

        return true;
    }

    private static List<Triangle2D> CollectProjectedTopTriangles(Transform lotTransform, Bounds lotBounds, bool topOnly)
    {
        List<Triangle2D> triangles = new List<Triangle2D>();
        MeshFilter[] meshFilters = lotTransform.GetComponentsInChildren<MeshFilter>(true);

        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                continue;
            }

            Vector3[] vertices = mesh.vertices;
            int[] meshTriangles = mesh.triangles;
            Matrix4x4 matrix = meshFilter.transform.localToWorldMatrix;

            for (int i = 0; i + 2 < meshTriangles.Length; i += 3)
            {
                Vector3 a = matrix.MultiplyPoint3x4(vertices[meshTriangles[i]]);
                Vector3 b = matrix.MultiplyPoint3x4(vertices[meshTriangles[i + 1]]);
                Vector3 c = matrix.MultiplyPoint3x4(vertices[meshTriangles[i + 2]]);
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;

                if (Mathf.Abs(normal.y) < 0.45f)
                {
                    continue;
                }

                float averageY = (a.y + b.y + c.y) / 3f;
                if (topOnly && averageY < lotBounds.center.y)
                {
                    continue;
                }

                Triangle2D triangle = new Triangle2D
                {
                    A = new Vector2(a.x, a.z),
                    B = new Vector2(b.x, b.z),
                    C = new Vector2(c.x, c.z)
                };

                if (TriangleArea(triangle) > 0.02f)
                {
                    triangles.Add(triangle);
                }
            }
        }

        return triangles;
    }

    private static List<Triangle2D> BuildBoundsFallbackTriangles(Bounds bounds)
    {
        Vector2 a = new Vector2(bounds.min.x, bounds.min.z);
        Vector2 b = new Vector2(bounds.max.x, bounds.min.z);
        Vector2 c = new Vector2(bounds.max.x, bounds.max.z);
        Vector2 d = new Vector2(bounds.min.x, bounds.max.z);

        return new List<Triangle2D>
        {
            new Triangle2D { A = a, B = b, C = c },
            new Triangle2D { A = a, B = c, C = d }
        };
    }

    private static float EstimateDominantEdgeAngle(List<Triangle2D> triangles)
    {
        Dictionary<EdgeKey, EdgeInfo> edges = new Dictionary<EdgeKey, EdgeInfo>();
        for (int i = 0; i < triangles.Count; i++)
        {
            AddEdge(edges, triangles[i].A, triangles[i].B);
            AddEdge(edges, triangles[i].B, triangles[i].C);
            AddEdge(edges, triangles[i].C, triangles[i].A);
        }

        float bestLength = 0f;
        float bestAngle = 0f;
        foreach (KeyValuePair<EdgeKey, EdgeInfo> pair in edges)
        {
            EdgeInfo edge = pair.Value;
            if (edge.Count != 1)
            {
                continue;
            }

            Vector2 delta = edge.B - edge.A;
            float length = delta.magnitude;
            if (length < 3f || length <= bestLength)
            {
                continue;
            }

            bestLength = length;
            bestAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        }

        if (bestLength <= 0f)
        {
            foreach (Triangle2D triangle in triangles)
            {
                Vector2 delta = triangle.B - triangle.A;
                float length = delta.magnitude;
                if (length > bestLength)
                {
                    bestLength = length;
                    bestAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                }
            }
        }

        return NormalizeAngle(bestAngle);
    }

    private static void AddEdge(Dictionary<EdgeKey, EdgeInfo> edges, Vector2 a, Vector2 b)
    {
        EdgeKey key = new EdgeKey(a, b);
        EdgeInfo info;
        if (edges.TryGetValue(key, out info))
        {
            info.Count++;
            edges[key] = info;
            return;
        }

        edges.Add(key, new EdgeInfo { A = a, B = b, Count = 1 });
    }

    private static void ExpandLocalExtents(Vector2 point, Vector3 axisU, Vector3 axisV, ref float minU, ref float maxU, ref float minV, ref float maxV)
    {
        Vector3 world = new Vector3(point.x, 0f, point.y);
        float u = Vector3.Dot(world, axisU);
        float v = Vector3.Dot(world, axisV);
        minU = Mathf.Min(minU, u);
        maxU = Mathf.Max(maxU, u);
        minV = Mathf.Min(minV, v);
        maxV = Mathf.Max(maxV, v);
    }

    private bool IsOrientedRectInsideLot(LotData lot, float centerU, float centerV, float halfU, float halfV)
    {
        Vector2[] samples =
        {
            new Vector2(0f, 0f),
            new Vector2(-halfU, -halfV),
            new Vector2(halfU, -halfV),
            new Vector2(halfU, halfV),
            new Vector2(-halfU, halfV),
            new Vector2(0f, -halfV),
            new Vector2(halfU, 0f),
            new Vector2(0f, halfV),
            new Vector2(-halfU, 0f)
        };

        for (int i = 0; i < samples.Length; i++)
        {
            Vector3 world = ToWorld(lot, centerU + samples[i].x, centerV + samples[i].y, lot.GroundY);
            if (!IsPointInsideLot(lot, new Vector2(world.x, world.z)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsPointInsideLot(LotData lot, Vector2 point)
    {
        for (int i = 0; i < lot.Triangles.Count; i++)
        {
            if (PointInTriangle(point, lot.Triangles[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool PointInTriangle(Vector2 point, Triangle2D triangle)
    {
        float d1 = Sign(point, triangle.A, triangle.B);
        float d2 = Sign(point, triangle.B, triangle.C);
        float d3 = Sign(point, triangle.C, triangle.A);

        bool hasNegative = d1 < -0.001f || d2 < -0.001f || d3 < -0.001f;
        bool hasPositive = d1 > 0.001f || d2 > 0.001f || d3 > 0.001f;
        return !(hasNegative && hasPositive);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static float TriangleArea(Triangle2D triangle)
    {
        return Mathf.Abs((triangle.B.x - triangle.A.x) * (triangle.C.y - triangle.A.y) - (triangle.C.x - triangle.A.x) * (triangle.B.y - triangle.A.y)) * 0.5f;
    }

    private static Vector3 ToWorld(LotData lot, float u, float v, float y)
    {
        Vector3 xz = lot.AxisU * u + lot.AxisV * v;
        return new Vector3(xz.x, y, xz.z);
    }

    private GameObject CreateBox(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, true);
        box.transform.position = position;
        box.transform.localScale = scale;

        Collider collider = box.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyGeneratedObject(collider);
        }

        Renderer renderer = box.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        RegisterCreatedObject(box, "Create city asset placement helper");
        return box;
    }

    private GameObject CreateLocalBox(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPosition;
        box.transform.localRotation = Quaternion.identity;
        box.transform.localScale = scale;

        Collider collider = box.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyGeneratedObject(collider);
        }

        Renderer renderer = box.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        RegisterCreatedObject(box, "Create city asset placement helper");
        return box;
    }

    private GameObject PickPrefab(GameObject[] prefabs, System.Random random)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] != null)
            {
                validPrefabs.Add(prefabs[i]);
            }
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        return validPrefabs[random.Next(validPrefabs.Count)];
    }

    private Material GetMaterial(string key, Color color)
    {
        Material material;
        if (materialCache.TryGetValue(key, out material) && material != null)
        {
            return material;
        }

        material = new Material(FindLitShader());
        material.name = "Generated_" + key;
        SetMaterialColor(material, color);
        materialCache[key] = material;
        return material;
    }

    private Material GetEmissionMaterial(string key, Color color, float intensity)
    {
        Material material = GetMaterial(key, color);
        material.EnableKeyword("_EMISSION");
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", color * intensity);
        }
        return material;
    }

    private static Shader FindLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        return shader;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bounds = new Bounds(root.position, Vector3.zero);
        if (renderers.Length == 0)
        {
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return true;
    }

    private static Transform FindTransformByName(string objectName)
    {
        Transform[] allTransforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
        foreach (Transform transform in allTransforms)
        {
            if (transform.name == objectName)
            {
                return transform;
            }
        }
        return null;
    }

    private static Transform FindChildByExactName(Transform root, string childName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == childName)
            {
                return child;
            }
        }
        return null;
    }

    private static float RandomRange(System.Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }

    private static int StableHash(string text)
    {
        unchecked
        {
            int hash = 23;
            for (int i = 0; i < text.Length; i++)
            {
                hash = hash * 31 + text[i];
            }
            return hash;
        }
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle < 0f)
        {
            angle += 180f;
        }
        while (angle >= 180f)
        {
            angle -= 180f;
        }
        return angle;
    }

    private static void DestroyGeneratedObject(UnityEngine.Object target)
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.DestroyObjectImmediate(target);
            return;
        }
#endif

        Destroy(target);
    }

    private static void RegisterCreatedObject(UnityEngine.Object target, string undoName)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && target != null)
        {
            Undo.RegisterCreatedObjectUndo(target, undoName);
        }
#endif
    }

    private static void MarkSceneDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
#endif
    }
}
