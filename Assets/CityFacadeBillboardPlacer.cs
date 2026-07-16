using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CityFacadeBillboardPlacer : MonoBehaviour
{
    private const int CurrentSettingsVersion = 2;

    public enum BillboardParentMode
    {
        UnderBuildingSlot,
        UnderSharedRoot
    }

    public enum BillboardSizeMode
    {
        Mixed,
        Small,
        Medium,
        Large,
        FacadeBand
    }

    private enum FacadeSide
    {
        Front,
        Back,
        Left,
        Right
    }

    [Header("Source Buildings")]
    [SerializeField] private bool useGeneratedCitySlots = true;
    [SerializeField] private Transform[] generatedCityRoots;
    [SerializeField] private string[] generatedCityRootNameHints =
    {
        "CityAssets_Batch_",
        "CityAssets_AllTestLots_HeightControlled"
    };
    [SerializeField] private bool useManualBuildingRoots = true;
    [SerializeField] private Transform[] manualBuildingRoots;
    [SerializeField] private string[] manualBuildingRootNameHints =
    {
        "BUILDING"
    };
    [SerializeField] private bool includeInactiveBuildings = true;

    [Header("Generated Output")]
    [SerializeField] private BillboardParentMode parentMode = BillboardParentMode.UnderSharedRoot;
    [SerializeField] private string sharedBillboardRootName = "CityBillboards_Generated";
    [SerializeField] private bool clearBeforeGenerate = false;
    [SerializeField] private int randomSeed = 20260617;

    [Header("Density")]
    [SerializeField, Range(0f, 1f)] private float buildingUseChance = 0.38f;
    [SerializeField] private int minBillboardsPerBuilding = 0;
    [SerializeField] private int maxBillboardsPerBuilding = 2;
    [SerializeField] private float minBuildingHeight = 60f;
    [SerializeField] private float minFacadeWidth = 10f;

    [Header("Size Rules")]
    [SerializeField] private BillboardSizeMode sizeMode = BillboardSizeMode.Mixed;
    [SerializeField, Range(0f, 1f)] private float verticalSignChance = 0.22f;
    [SerializeField, Range(0f, 1f)] private float facadeBandChance = 0.18f;
    [SerializeField] private Vector2 absoluteWidthRange = new Vector2(5f, 42f);
    [SerializeField] private Vector2 absoluteHeightRange = new Vector2(3f, 28f);
    [SerializeField, Range(0.05f, 0.98f)] private float maxFacadeWidthCoverage = 0.92f;
    [SerializeField, Range(0.05f, 0.65f)] private float maxBuildingHeightCoverage = 0.42f;

    [Header("Placement Rules")]
    [SerializeField] private Vector2 verticalPositionRange = new Vector2(0.28f, 0.78f);
    [SerializeField] private float facadeOffset = 0.42f;
    [SerializeField] private float minimumTopClearance = 2f;
    [SerializeField] private float minimumBottomClearance = 4f;
    [SerializeField] private bool avoidDuplicateFacadePerBuilding = true;

    [Header("Visual")]
    [SerializeField] private GameObject[] billboardPrefabs;
    [SerializeField] private bool useBillboardPrefabsWhenAvailable = false;
    [SerializeField] private bool createPlaceholderSurface = true;
    [SerializeField] private float placeholderThickness = 0.28f;
    [SerializeField] private Color placeholderBaseColor = new Color(0.02f, 0.09f, 0.13f, 1f);
    [SerializeField] private Color placeholderEmissionColor = new Color(0.05f, 0.85f, 1f, 1f);
    [SerializeField] private float placeholderEmissionIntensity = 2.4f;
    [SerializeField] private bool addCollider = false;

    [Header("Safety")]
    [SerializeField] private bool skipInactiveBuildingSlots = true;
    [SerializeField] private bool requireActiveRenderer = true;
    [SerializeField] private bool cleanupOrphanBillboardsBeforeGenerate = true;
    [SerializeField] private bool skipBuildingsWithExistingBillboards = true;
    [SerializeField, HideInInspector] private int settingsVersion;

    private Material placeholderMaterial;

    private struct FacadeCandidate
    {
        public FacadeSide Side;
        public Vector3 Normal;
        public Vector3 Center;
        public float Width;
        public float Height;
        public float MinHorizontal;
        public float MaxHorizontal;
        public float MinY;
        public float MaxY;
    }

    private struct BillboardPlan
    {
        public FacadeCandidate Facade;
        public Vector3 LocalPosition;
        public Vector2 Size;
    }

    public Transform[] GeneratedCityRoots
    {
        get { return generatedCityRoots; }
        set { generatedCityRoots = value; }
    }

    public Transform[] ManualBuildingRoots
    {
        get { return manualBuildingRoots; }
        set { manualBuildingRoots = value; }
    }

    private void OnValidate()
    {
        if (settingsVersion >= CurrentSettingsVersion)
        {
            return;
        }

        parentMode = BillboardParentMode.UnderSharedRoot;
        clearBeforeGenerate = false;
        skipBuildingsWithExistingBillboards = true;
        cleanupOrphanBillboardsBeforeGenerate = true;
        settingsVersion = CurrentSettingsVersion;
    }

    public void FindGeneratedCityRoots()
    {
        List<Transform> roots = new List<Transform>();
        Transform[] allObjects = FindObjectsOfType<Transform>(includeInactiveBuildings);

        for (int i = 0; i < allObjects.Length; i++)
        {
            Transform candidate = allObjects[i];
            if (candidate == null || candidate.parent != null)
            {
                continue;
            }

            if (MatchesGeneratedRootHint(candidate.name))
            {
                roots.Add(candidate);
            }
        }

        generatedCityRoots = roots.ToArray();
        Debug.Log("CityFacadeBillboardPlacer: found " + generatedCityRoots.Length + " generated city roots.");
    }

    public void FindManualBuildingRoots()
    {
        List<Transform> roots = new List<Transform>();
        Transform[] allObjects = FindObjectsOfType<Transform>(includeInactiveBuildings);

        for (int i = 0; i < allObjects.Length; i++)
        {
            Transform candidate = allObjects[i];
            if (candidate == null)
            {
                continue;
            }

            if (MatchesManualRootHint(candidate.name))
            {
                roots.Add(candidate);
            }
        }

        manualBuildingRoots = roots.ToArray();
        Debug.Log("CityFacadeBillboardPlacer: found " + manualBuildingRoots.Length + " manual building roots.");
    }

    public void GenerateBillboards()
    {
        if (clearBeforeGenerate)
        {
            ClearBillboards();
        }
        else if (cleanupOrphanBillboardsBeforeGenerate)
        {
            ClearOrphanBillboards();
        }

        List<CityBuildingPlacementSlot> buildingSlots = useGeneratedCitySlots ? CollectBuildingSlots() : new List<CityBuildingPlacementSlot>();
        List<Transform> manualBuildings = useManualBuildingRoots ? CollectManualBuildings() : new List<Transform>();
        if (buildingSlots.Count == 0 && manualBuildings.Count == 0)
        {
            Debug.LogWarning("CityFacadeBillboardPlacer: no building source found. Set Generated City Roots, or set Manual Building Roots such as BUILDING.");
            return;
        }

        System.Random random = new System.Random(randomSeed);
        Transform sharedRoot = null;
        if (parentMode == BillboardParentMode.UnderSharedRoot)
        {
            sharedRoot = GetOrCreateSharedRoot();
        }

        int createdCount = 0;
        int skippedTooSmall = 0;
        int skippedMissingBuilding = 0;
        int skippedExistingBillboard = 0;
        HashSet<CityBuildingPlacementSlot> slotsWithExistingBillboards = skipBuildingsWithExistingBillboards ? BuildExistingBillboardSlotSet() : null;
        HashSet<Transform> manualRootsWithExistingBillboards = skipBuildingsWithExistingBillboards ? BuildExistingManualBillboardRootSet() : null;

        for (int i = 0; i < buildingSlots.Count; i++)
        {
            CityBuildingPlacementSlot slot = buildingSlots[i];
            if (slot == null || random.NextDouble() > buildingUseChance)
            {
                continue;
            }

            if (skipBuildingsWithExistingBillboards && slotsWithExistingBillboards != null && slotsWithExistingBillboards.Contains(slot))
            {
                skippedExistingBillboard++;
                continue;
            }

            Bounds localBounds;
            if (!TryGetSlotLocalRendererBounds(slot, out localBounds))
            {
                skippedMissingBuilding++;
                continue;
            }

            if (localBounds.size.y < minBuildingHeight)
            {
                skippedTooSmall++;
                continue;
            }

            List<FacadeCandidate> facades = BuildFacadeCandidates(localBounds);
            if (facades.Count == 0)
            {
                skippedTooSmall++;
                continue;
            }

            int count = RandomRangeInt(random, Mathf.Min(minBillboardsPerBuilding, maxBillboardsPerBuilding), Mathf.Max(minBillboardsPerBuilding, maxBillboardsPerBuilding));
            HashSet<FacadeSide> usedFacades = new HashSet<FacadeSide>();

            for (int billboardIndex = 0; billboardIndex < count; billboardIndex++)
            {
                BillboardPlan plan;
                if (!TryCreatePlan(facades, localBounds, usedFacades, random, out plan))
                {
                    continue;
                }

                Transform parent = parentMode == BillboardParentMode.UnderSharedRoot ? sharedRoot : GetOrCreateSlotBillboardRoot(slot);
                CreateBillboard(slot, parent, plan, billboardIndex, random);
                usedFacades.Add(plan.Facade.Side);
                createdCount++;
            }
        }

        for (int i = 0; i < manualBuildings.Count; i++)
        {
            Transform sourceRoot = manualBuildings[i];
            if (sourceRoot == null || random.NextDouble() > buildingUseChance)
            {
                continue;
            }

            if (skipBuildingsWithExistingBillboards && manualRootsWithExistingBillboards != null && manualRootsWithExistingBillboards.Contains(sourceRoot))
            {
                skippedExistingBillboard++;
                continue;
            }

            Bounds localBounds;
            if (!TryGetTransformLocalRendererBounds(sourceRoot, out localBounds))
            {
                skippedMissingBuilding++;
                continue;
            }

            if (localBounds.size.y < minBuildingHeight)
            {
                skippedTooSmall++;
                continue;
            }

            List<FacadeCandidate> facades = BuildFacadeCandidates(localBounds);
            if (facades.Count == 0)
            {
                skippedTooSmall++;
                continue;
            }

            int count = RandomRangeInt(random, Mathf.Min(minBillboardsPerBuilding, maxBillboardsPerBuilding), Mathf.Max(minBillboardsPerBuilding, maxBillboardsPerBuilding));
            HashSet<FacadeSide> usedFacades = new HashSet<FacadeSide>();

            for (int billboardIndex = 0; billboardIndex < count; billboardIndex++)
            {
                BillboardPlan plan;
                if (!TryCreatePlan(facades, localBounds, usedFacades, random, out plan))
                {
                    continue;
                }

                Transform parent = parentMode == BillboardParentMode.UnderSharedRoot ? sharedRoot : GetOrCreateManualBillboardRoot(sourceRoot);
                CreateBillboard(sourceRoot, parent, plan, billboardIndex, random);
                usedFacades.Add(plan.Facade.Side);
                createdCount++;
            }
        }

        Debug.Log("CityFacadeBillboardPlacer: generated " + createdCount + " billboard placeholders. Skipped existing billboard buildings: " + skippedExistingBillboard + ". Skipped missing or inactive buildings: " + skippedMissingBuilding + ". Skipped too-small buildings: " + skippedTooSmall + ".");
    }

    public void ClearBillboards()
    {
        CityBillboardPlacementSlot[] billboards = FindObjectsOfType<CityBillboardPlacementSlot>(includeInactiveBuildings);
        int cleared = 0;

        for (int i = billboards.Length - 1; i >= 0; i--)
        {
            CityBillboardPlacementSlot billboard = billboards[i];
            if (billboard == null)
            {
                continue;
            }

            if (ShouldClearBillboard(billboard))
            {
                DestroyToolObject(billboard.gameObject);
                cleared++;
            }
        }

        if (parentMode == BillboardParentMode.UnderSharedRoot)
        {
            GameObject sharedRoot = GameObject.Find(sharedBillboardRootName);
            if (sharedRoot != null && sharedRoot.transform.childCount == 0)
            {
                DestroyToolObject(sharedRoot);
            }
        }

        Debug.Log("CityFacadeBillboardPlacer: cleared " + cleared + " generated billboards.");
    }

    public void ClearOrphanBillboards()
    {
        CityBillboardPlacementSlot[] billboards = FindObjectsOfType<CityBillboardPlacementSlot>(includeInactiveBuildings);
        int cleared = 0;

        for (int i = billboards.Length - 1; i >= 0; i--)
        {
            CityBillboardPlacementSlot billboard = billboards[i];
            if (billboard == null || !ShouldClearBillboard(billboard))
            {
                continue;
            }

            Bounds ignoredBounds;
            bool sourceMissing = billboard.BuildingSlot == null || !TryGetSlotLocalRendererBounds(billboard.BuildingSlot, out ignoredBounds);
            if (billboard.BuildingSlot == null && billboard.SourceBuildingRoot != null)
            {
                sourceMissing = !TryGetTransformLocalRendererBounds(billboard.SourceBuildingRoot, out ignoredBounds);
            }

            if (sourceMissing)
            {
                DestroyToolObject(billboard.gameObject);
                cleared++;
            }
        }

        Debug.Log("CityFacadeBillboardPlacer: cleared " + cleared + " orphan billboards.");
    }

    public void SetBillboardPrefabs(GameObject[] prefabs)
    {
        billboardPrefabs = prefabs;
    }

    private List<CityBuildingPlacementSlot> CollectBuildingSlots()
    {
        List<CityBuildingPlacementSlot> result = new List<CityBuildingPlacementSlot>();
        CityBuildingPlacementSlot[] allSlots = FindObjectsOfType<CityBuildingPlacementSlot>(includeInactiveBuildings);

        for (int i = 0; i < allSlots.Length; i++)
        {
            CityBuildingPlacementSlot slot = allSlots[i];
            if (slot == null)
            {
                continue;
            }

            if (generatedCityRoots == null || generatedCityRoots.Length == 0 || IsChildOfGeneratedRoot(slot.transform))
            {
                result.Add(slot);
            }
        }

        return result;
    }

    private List<Transform> CollectManualBuildings()
    {
        List<Transform> result = new List<Transform>();
        if (manualBuildingRoots == null)
        {
            return result;
        }

        for (int i = 0; i < manualBuildingRoots.Length; i++)
        {
            Transform root = manualBuildingRoots[i];
            if (root == null)
            {
                continue;
            }

            if (root.childCount == 0)
            {
                AddManualBuildingIfRenderable(result, root);
                continue;
            }

            for (int childIndex = 0; childIndex < root.childCount; childIndex++)
            {
                AddManualBuildingIfRenderable(result, root.GetChild(childIndex));
            }
        }

        return result;
    }

    private void AddManualBuildingIfRenderable(List<Transform> result, Transform candidate)
    {
        if (candidate == null)
        {
            return;
        }

        if (skipInactiveBuildingSlots && !candidate.gameObject.activeInHierarchy)
        {
            return;
        }

        Renderer[] renderers = candidate.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (requireActiveRenderer && (!renderer.enabled || !renderer.gameObject.activeInHierarchy))
            {
                continue;
            }

            result.Add(candidate);
            return;
        }
    }

    private bool MatchesGeneratedRootHint(string objectName)
    {
        if (string.IsNullOrEmpty(objectName) || generatedCityRootNameHints == null)
        {
            return false;
        }

        for (int i = 0; i < generatedCityRootNameHints.Length; i++)
        {
            string hint = generatedCityRootNameHints[i];
            if (!string.IsNullOrEmpty(hint) && objectName.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool MatchesManualRootHint(string objectName)
    {
        if (string.IsNullOrEmpty(objectName) || manualBuildingRootNameHints == null)
        {
            return false;
        }

        for (int i = 0; i < manualBuildingRootNameHints.Length; i++)
        {
            string hint = manualBuildingRootNameHints[i];
            if (!string.IsNullOrEmpty(hint) && objectName.Equals(hint, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsChildOfGeneratedRoot(Transform target)
    {
        if (target == null || generatedCityRoots == null)
        {
            return false;
        }

        for (int i = 0; i < generatedCityRoots.Length; i++)
        {
            Transform root = generatedCityRoots[i];
            if (root != null && (target == root || target.IsChildOf(root)))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsChildOfManualRoot(Transform target)
    {
        if (target == null || manualBuildingRoots == null)
        {
            return false;
        }

        for (int i = 0; i < manualBuildingRoots.Length; i++)
        {
            Transform root = manualBuildingRoots[i];
            if (root != null && (target == root || target.IsChildOf(root)))
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldClearBillboard(CityBillboardPlacementSlot billboard)
    {
        if (billboard == null)
        {
            return false;
        }

        if (parentMode == BillboardParentMode.UnderSharedRoot)
        {
            GameObject sharedRootObject = GameObject.Find(sharedBillboardRootName);
            Transform sharedRoot = sharedRootObject != null ? sharedRootObject.transform : null;
            return sharedRoot != null && billboard.transform.IsChildOf(sharedRoot);
        }

        CityBuildingPlacementSlot sourceSlot = billboard.BuildingSlot;
        if (sourceSlot == null)
        {
            Transform sourceRoot = billboard.SourceBuildingRoot;
            if (sourceRoot != null)
            {
                return manualBuildingRoots == null || manualBuildingRoots.Length == 0 || IsChildOfManualRoot(sourceRoot);
            }

            return generatedCityRoots == null || generatedCityRoots.Length == 0 || IsChildOfGeneratedRoot(billboard.transform);
        }

        return generatedCityRoots == null || generatedCityRoots.Length == 0 || IsChildOfGeneratedRoot(sourceSlot.transform);
    }

    private HashSet<CityBuildingPlacementSlot> BuildExistingBillboardSlotSet()
    {
        HashSet<CityBuildingPlacementSlot> slots = new HashSet<CityBuildingPlacementSlot>();
        CityBillboardPlacementSlot[] existingBillboards = FindObjectsOfType<CityBillboardPlacementSlot>(includeInactiveBuildings);
        for (int i = 0; i < existingBillboards.Length; i++)
        {
            CityBillboardPlacementSlot billboard = existingBillboards[i];
            if (billboard == null || billboard.BuildingSlot == null)
            {
                continue;
            }

            if (generatedCityRoots == null || generatedCityRoots.Length == 0 || IsChildOfGeneratedRoot(billboard.BuildingSlot.transform))
            {
                slots.Add(billboard.BuildingSlot);
            }
        }

        return slots;
    }

    private HashSet<Transform> BuildExistingManualBillboardRootSet()
    {
        HashSet<Transform> roots = new HashSet<Transform>();
        CityBillboardPlacementSlot[] existingBillboards = FindObjectsOfType<CityBillboardPlacementSlot>(includeInactiveBuildings);
        for (int i = 0; i < existingBillboards.Length; i++)
        {
            CityBillboardPlacementSlot billboard = existingBillboards[i];
            if (billboard == null || billboard.SourceBuildingRoot == null || billboard.BuildingSlot != null)
            {
                continue;
            }

            if (manualBuildingRoots == null || manualBuildingRoots.Length == 0 || IsChildOfManualRoot(billboard.SourceBuildingRoot))
            {
                roots.Add(billboard.SourceBuildingRoot);
            }
        }

        return roots;
    }

    private bool TryGetSlotLocalRendererBounds(CityBuildingPlacementSlot slot, out Bounds localBounds)
    {
        localBounds = new Bounds(Vector3.zero, Vector3.zero);
        if (slot == null || slot.ContentRoot == null)
        {
            return false;
        }

        if (skipInactiveBuildingSlots && (!slot.gameObject.activeInHierarchy || !slot.ContentRoot.gameObject.activeInHierarchy))
        {
            return false;
        }

        Renderer[] renderers = slot.ContentRoot.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (requireActiveRenderer && (!renderer.enabled || !renderer.gameObject.activeInHierarchy))
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            for (int cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
            {
                Vector3 localPoint = slot.transform.InverseTransformPoint(corners[cornerIndex]);
                if (!initialized)
                {
                    localBounds = new Bounds(localPoint, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    localBounds.Encapsulate(localPoint);
                }
            }
        }

        return initialized;
    }

    private bool TryGetTransformLocalRendererBounds(Transform sourceRoot, out Bounds localBounds)
    {
        localBounds = new Bounds(Vector3.zero, Vector3.zero);
        if (sourceRoot == null)
        {
            return false;
        }

        if (skipInactiveBuildingSlots && !sourceRoot.gameObject.activeInHierarchy)
        {
            return false;
        }

        Renderer[] renderers = sourceRoot.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (requireActiveRenderer && (!renderer.enabled || !renderer.gameObject.activeInHierarchy))
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;

            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            for (int cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
            {
                Vector3 localPoint = sourceRoot.InverseTransformPoint(corners[cornerIndex]);
                if (!initialized)
                {
                    localBounds = new Bounds(localPoint, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    localBounds.Encapsulate(localPoint);
                }
            }
        }

        return initialized;
    }

    private List<FacadeCandidate> BuildFacadeCandidates(Bounds localBounds)
    {
        List<FacadeCandidate> facades = new List<FacadeCandidate>();
        AddFacade(facades, localBounds, FacadeSide.Front, Vector3.forward, localBounds.max.z, localBounds.min.x, localBounds.max.x, localBounds.size.x);
        AddFacade(facades, localBounds, FacadeSide.Back, Vector3.back, localBounds.min.z, localBounds.min.x, localBounds.max.x, localBounds.size.x);
        AddFacade(facades, localBounds, FacadeSide.Left, Vector3.left, localBounds.min.x, localBounds.min.z, localBounds.max.z, localBounds.size.z);
        AddFacade(facades, localBounds, FacadeSide.Right, Vector3.right, localBounds.max.x, localBounds.min.z, localBounds.max.z, localBounds.size.z);
        return facades;
    }

    private void AddFacade(List<FacadeCandidate> facades, Bounds localBounds, FacadeSide side, Vector3 normal, float planeValue, float minHorizontal, float maxHorizontal, float width)
    {
        if (width < minFacadeWidth || localBounds.size.y < minBuildingHeight)
        {
            return;
        }

        Vector3 center = localBounds.center;
        if (side == FacadeSide.Front || side == FacadeSide.Back)
        {
            center.z = planeValue;
        }
        else
        {
            center.x = planeValue;
        }

        FacadeCandidate facade = new FacadeCandidate();
        facade.Side = side;
        facade.Normal = normal;
        facade.Center = center;
        facade.Width = width;
        facade.Height = localBounds.size.y;
        facade.MinHorizontal = minHorizontal;
        facade.MaxHorizontal = maxHorizontal;
        facade.MinY = localBounds.min.y;
        facade.MaxY = localBounds.max.y;
        facades.Add(facade);
    }

    private bool TryCreatePlan(List<FacadeCandidate> facades, Bounds localBounds, HashSet<FacadeSide> usedFacades, System.Random random, out BillboardPlan plan)
    {
        plan = new BillboardPlan();

        for (int attempt = 0; attempt < 16; attempt++)
        {
            FacadeCandidate facade = facades[random.Next(0, facades.Count)];
            if (avoidDuplicateFacadePerBuilding && usedFacades.Contains(facade.Side) && usedFacades.Count < facades.Count)
            {
                continue;
            }

            Vector2 size = PickBillboardSize(facade, localBounds.size.y, random);
            if (size.x <= 0f || size.y <= 0f)
            {
                continue;
            }

            float halfWidth = size.x * 0.5f;
            float horizontalMin = facade.MinHorizontal + halfWidth;
            float horizontalMax = facade.MaxHorizontal - halfWidth;
            if (horizontalMax < horizontalMin)
            {
                continue;
            }

            float halfHeight = size.y * 0.5f;
            float yMin = facade.MinY + minimumBottomClearance + halfHeight;
            float yMax = facade.MaxY - minimumTopClearance - halfHeight;
            if (yMax < yMin)
            {
                continue;
            }

            float heightRatio = RandomRange(random, Mathf.Min(verticalPositionRange.x, verticalPositionRange.y), Mathf.Max(verticalPositionRange.x, verticalPositionRange.y));
            float idealY = Mathf.Lerp(facade.MinY, facade.MaxY, heightRatio);
            float y = Mathf.Clamp(idealY, yMin, yMax);
            float horizontal = RandomRange(random, horizontalMin, horizontalMax);
            Vector3 localPosition = facade.Center + facade.Normal * facadeOffset;
            localPosition.y = y;

            if (facade.Side == FacadeSide.Front || facade.Side == FacadeSide.Back)
            {
                localPosition.x = horizontal;
            }
            else
            {
                localPosition.z = horizontal;
            }

            plan.Facade = facade;
            plan.LocalPosition = localPosition;
            plan.Size = size;
            return true;
        }

        return false;
    }

    private Vector2 PickBillboardSize(FacadeCandidate facade, float buildingHeight, System.Random random)
    {
        BillboardSizeMode resolvedMode = sizeMode;
        if (resolvedMode == BillboardSizeMode.Mixed)
        {
            double value = random.NextDouble();
            if (value < facadeBandChance)
            {
                resolvedMode = BillboardSizeMode.FacadeBand;
            }
            else if (value < 0.44)
            {
                resolvedMode = BillboardSizeMode.Large;
            }
            else if (value < 0.78)
            {
                resolvedMode = BillboardSizeMode.Medium;
            }
            else
            {
                resolvedMode = BillboardSizeMode.Small;
            }
        }

        bool vertical = random.NextDouble() < verticalSignChance && resolvedMode != BillboardSizeMode.FacadeBand;
        Vector2 widthRatioRange;
        Vector2 heightRatioRange;

        switch (resolvedMode)
        {
            case BillboardSizeMode.Small:
                widthRatioRange = new Vector2(0.16f, 0.30f);
                heightRatioRange = new Vector2(0.05f, 0.10f);
                break;
            case BillboardSizeMode.Medium:
                widthRatioRange = new Vector2(0.28f, 0.52f);
                heightRatioRange = new Vector2(0.08f, 0.15f);
                break;
            case BillboardSizeMode.Large:
                widthRatioRange = new Vector2(0.46f, 0.72f);
                heightRatioRange = new Vector2(0.12f, 0.22f);
                break;
            default:
                widthRatioRange = new Vector2(0.68f, 0.95f);
                heightRatioRange = new Vector2(0.10f, 0.20f);
                break;
        }

        if (vertical)
        {
            widthRatioRange = new Vector2(0.10f, 0.24f);
            heightRatioRange = new Vector2(0.20f, 0.38f);
        }

        float maxWidth = Mathf.Min(absoluteWidthRange.y, facade.Width * maxFacadeWidthCoverage);
        float maxHeight = Mathf.Min(absoluteHeightRange.y, buildingHeight * maxBuildingHeightCoverage);
        float width = Mathf.Clamp(facade.Width * RandomRange(random, widthRatioRange.x, widthRatioRange.y), absoluteWidthRange.x, maxWidth);
        float height = Mathf.Clamp(buildingHeight * RandomRange(random, heightRatioRange.x, heightRatioRange.y), absoluteHeightRange.x, maxHeight);

        if (width > facade.Width * maxFacadeWidthCoverage || height > buildingHeight * maxBuildingHeightCoverage)
        {
            return Vector2.zero;
        }

        return new Vector2(width, height);
    }

    private Transform GetOrCreateSlotBillboardRoot(CityBuildingPlacementSlot slot)
    {
        Transform existing = slot.transform.Find("Billboards");
        if (existing != null)
        {
            return existing;
        }

        GameObject root = new GameObject("Billboards");
        root.transform.SetParent(slot.transform, false);
        RegisterCreatedObject(root, "Create billboard root");
        return root.transform;
    }

    private Transform GetOrCreateManualBillboardRoot(Transform sourceRoot)
    {
        Transform existing = sourceRoot.Find("Billboards");
        if (existing != null)
        {
            return existing;
        }

        GameObject root = new GameObject("Billboards");
        root.transform.SetParent(sourceRoot, false);
        RegisterCreatedObject(root, "Create manual billboard root");
        return root.transform;
    }

    private Transform GetOrCreateSharedRoot()
    {
        GameObject existing = GameObject.Find(sharedBillboardRootName);
        if (existing != null)
        {
            return existing.transform;
        }

        GameObject root = new GameObject(sharedBillboardRootName);
        root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        RegisterCreatedObject(root, "Create shared billboard root");
        return root.transform;
    }

    private void CreateBillboard(CityBuildingPlacementSlot slot, Transform parent, BillboardPlan plan, int billboardIndex, System.Random random)
    {
        GameObject billboard = new GameObject("Billboard_" + slot.LotName + "_" + slot.SlotIndex.ToString("00") + "_" + billboardIndex.ToString("00"));
        billboard.transform.SetParent(parent, true);
        billboard.transform.SetPositionAndRotation(slot.transform.TransformPoint(plan.LocalPosition), slot.transform.rotation * Quaternion.LookRotation(plan.Facade.Normal, Vector3.up));
        RegisterCreatedObject(billboard, "Create city billboard");

        GameObject surfaceRoot = new GameObject("SurfaceRoot");
        surfaceRoot.transform.SetParent(billboard.transform, false);
        surfaceRoot.transform.localPosition = Vector3.zero;
        surfaceRoot.transform.localRotation = Quaternion.identity;
        RegisterCreatedObject(surfaceRoot, "Create billboard surface root");

        if (createPlaceholderSurface)
        {
            CreatePlaceholderSurface(surfaceRoot.transform, plan.Size);
        }

        if (useBillboardPrefabsWhenAvailable)
        {
            GameObject prefab = PickPrefab(random);
            if (prefab != null)
            {
                GameObject instance = InstantiatePrefab(prefab, surfaceRoot.transform);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
            }
        }

        GameObject contentRoot = new GameObject("ContentRoot");
        contentRoot.transform.SetParent(billboard.transform, false);
        contentRoot.transform.localPosition = new Vector3(0f, 0f, placeholderThickness * 0.6f + 0.03f);
        contentRoot.transform.localRotation = Quaternion.identity;
        RegisterCreatedObject(contentRoot, "Create billboard content root");

        CityBillboardPlacementSlot marker = billboard.AddComponent<CityBillboardPlacementSlot>();
        marker.Configure(slot, plan.Facade.Side.ToString(), plan.Size, plan.LocalPosition.y, surfaceRoot.transform, contentRoot.transform);
        EnsureContentController(surfaceRoot.transform);
    }

    private void CreateBillboard(Transform sourceRoot, Transform parent, BillboardPlan plan, int billboardIndex, System.Random random)
    {
        GameObject billboard = new GameObject("Billboard_" + SanitizeName(sourceRoot.name) + "_" + billboardIndex.ToString("00"));
        billboard.transform.SetParent(parent, true);
        billboard.transform.SetPositionAndRotation(sourceRoot.TransformPoint(plan.LocalPosition), sourceRoot.rotation * Quaternion.LookRotation(plan.Facade.Normal, Vector3.up));
        RegisterCreatedObject(billboard, "Create manual building billboard");

        GameObject surfaceRoot = new GameObject("SurfaceRoot");
        surfaceRoot.transform.SetParent(billboard.transform, false);
        surfaceRoot.transform.localPosition = Vector3.zero;
        surfaceRoot.transform.localRotation = Quaternion.identity;
        RegisterCreatedObject(surfaceRoot, "Create billboard surface root");

        if (createPlaceholderSurface)
        {
            CreatePlaceholderSurface(surfaceRoot.transform, plan.Size);
        }

        if (useBillboardPrefabsWhenAvailable)
        {
            GameObject prefab = PickPrefab(random);
            if (prefab != null)
            {
                GameObject instance = InstantiatePrefab(prefab, surfaceRoot.transform);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
            }
        }

        GameObject contentRoot = new GameObject("ContentRoot");
        contentRoot.transform.SetParent(billboard.transform, false);
        contentRoot.transform.localPosition = new Vector3(0f, 0f, placeholderThickness * 0.6f + 0.03f);
        contentRoot.transform.localRotation = Quaternion.identity;
        RegisterCreatedObject(contentRoot, "Create billboard content root");

        CityBillboardPlacementSlot marker = billboard.AddComponent<CityBillboardPlacementSlot>();
        marker.Configure(sourceRoot, plan.Facade.Side.ToString(), plan.Size, plan.LocalPosition.y, surfaceRoot.transform, contentRoot.transform);
        EnsureContentController(surfaceRoot.transform);
    }

    private static string SanitizeName(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return "ManualBuilding";
        }

        char[] chars = source.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    private void CreatePlaceholderSurface(Transform parent, Vector2 size)
    {
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = "BlankBillboardSurface";
        surface.transform.SetParent(parent, false);
        surface.transform.localPosition = Vector3.zero;
        surface.transform.localRotation = Quaternion.identity;
        surface.transform.localScale = new Vector3(size.x, size.y, Mathf.Max(0.02f, placeholderThickness));
        RegisterCreatedObject(surface, "Create blank billboard surface");

        if (!addCollider)
        {
            Collider collider = surface.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyToolObject(collider);
            }
        }

        Renderer renderer = surface.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = GetPlaceholderMaterial();
        }
    }

    private static void EnsureContentController(Transform surfaceRoot)
    {
        if (surfaceRoot == null)
        {
            return;
        }

        Renderer renderer = surfaceRoot.GetComponentInChildren<Renderer>(true);
        if (renderer == null)
        {
            return;
        }

        CityBillboardContentController controller = renderer.GetComponent<CityBillboardContentController>();
        if (controller == null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                controller = Undo.AddComponent<CityBillboardContentController>(renderer.gameObject);
            }
            else
            {
                controller = renderer.gameObject.AddComponent<CityBillboardContentController>();
            }
#else
            controller = renderer.gameObject.AddComponent<CityBillboardContentController>();
#endif
        }

        controller.Configure(renderer);
    }

    private Material GetPlaceholderMaterial()
    {
        if (placeholderMaterial != null)
        {
            return placeholderMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        placeholderMaterial = new Material(shader);
        placeholderMaterial.name = "M_CityBillboard_Blank_Preview";
        placeholderMaterial.color = placeholderBaseColor;

        if (placeholderMaterial.HasProperty("_BaseColor"))
        {
            placeholderMaterial.SetColor("_BaseColor", placeholderBaseColor);
        }

        if (placeholderMaterial.HasProperty("_EmissionColor"))
        {
            placeholderMaterial.SetColor("_EmissionColor", placeholderEmissionColor * placeholderEmissionIntensity);
            placeholderMaterial.EnableKeyword("_EMISSION");
        }

        return placeholderMaterial;
    }

    private GameObject PickPrefab(System.Random random)
    {
        if (billboardPrefabs == null || billboardPrefabs.Length == 0)
        {
            return null;
        }

        List<GameObject> validPrefabs = new List<GameObject>();
        for (int i = 0; i < billboardPrefabs.Length; i++)
        {
            if (billboardPrefabs[i] != null)
            {
                validPrefabs.Add(billboardPrefabs[i]);
            }
        }

        if (validPrefabs.Count == 0)
        {
            return null;
        }

        return validPrefabs[random.Next(0, validPrefabs.Count)];
    }

    private GameObject InstantiatePrefab(GameObject prefab, Transform parent)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Instantiate billboard prefab");
            return instance;
        }
#endif

        return Instantiate(prefab, parent);
    }

    private static int RandomRangeInt(System.Random random, int minInclusive, int maxInclusive)
    {
        if (maxInclusive < minInclusive)
        {
            int temp = minInclusive;
            minInclusive = maxInclusive;
            maxInclusive = temp;
        }

        return random.Next(minInclusive, maxInclusive + 1);
    }

    private static float RandomRange(System.Random random, float min, float max)
    {
        if (max < min)
        {
            float temp = min;
            min = max;
            max = temp;
        }

        return Mathf.Lerp(min, max, (float)random.NextDouble());
    }

    private static void RegisterCreatedObject(UnityEngine.Object target, string undoName)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCreatedObjectUndo(target, undoName);
        }
#endif
    }

    private static void DestroyToolObject(UnityEngine.Object target)
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
}
