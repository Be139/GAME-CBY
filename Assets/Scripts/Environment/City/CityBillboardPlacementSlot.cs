using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CityBillboardPlacementSlot : MonoBehaviour
{
    [SerializeField] private CityBuildingPlacementSlot buildingSlot;
    [SerializeField] private Transform sourceBuildingRoot;
    [SerializeField] private string lotName;
    [SerializeField] private int buildingSlotIndex;
    [SerializeField] private string facadeSide;
    [SerializeField] private Vector2 billboardSize;
    [SerializeField] private float heightOnBuilding;
    [SerializeField] private Transform surfaceRoot;
    [SerializeField] private Transform contentRoot;

    public CityBuildingPlacementSlot BuildingSlot
    {
        get { return buildingSlot; }
    }

    public Transform SourceBuildingRoot
    {
        get { return sourceBuildingRoot; }
    }

    public string LotName
    {
        get { return lotName; }
    }

    public int BuildingSlotIndex
    {
        get { return buildingSlotIndex; }
    }

    public string FacadeSide
    {
        get { return facadeSide; }
    }

    public Vector2 BillboardSize
    {
        get { return billboardSize; }
    }

    public float HeightOnBuilding
    {
        get { return heightOnBuilding; }
    }

    public Transform SurfaceRoot
    {
        get { return surfaceRoot; }
    }

    public Transform ContentRoot
    {
        get { return contentRoot; }
    }

    public void Configure(CityBuildingPlacementSlot sourceSlot, string sideName, Vector2 size, float height, Transform newSurfaceRoot, Transform newContentRoot)
    {
        buildingSlot = sourceSlot;
        sourceBuildingRoot = sourceSlot != null ? sourceSlot.transform : null;
        lotName = sourceSlot != null ? sourceSlot.LotName : string.Empty;
        buildingSlotIndex = sourceSlot != null ? sourceSlot.SlotIndex : -1;
        facadeSide = sideName;
        billboardSize = size;
        heightOnBuilding = height;
        surfaceRoot = newSurfaceRoot;
        contentRoot = newContentRoot;
    }

    public void Configure(Transform sourceRoot, string sideName, Vector2 size, float height, Transform newSurfaceRoot, Transform newContentRoot)
    {
        buildingSlot = null;
        sourceBuildingRoot = sourceRoot;
        lotName = sourceRoot != null ? sourceRoot.name : string.Empty;
        buildingSlotIndex = -1;
        facadeSide = sideName;
        billboardSize = size;
        heightOnBuilding = height;
        surfaceRoot = newSurfaceRoot;
        contentRoot = newContentRoot;
    }

    public void SetContent(GameObject contentPrefab)
    {
        if (contentPrefab == null || contentRoot == null)
        {
            return;
        }

        ClearContent();

#if UNITY_EDITOR
        GameObject instance;
        if (!Application.isPlaying)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(contentPrefab, contentRoot);
            Undo.RegisterCreatedObjectUndo(instance, "Set billboard content");
        }
        else
        {
            instance = Instantiate(contentPrefab, contentRoot);
        }
#else
        GameObject instance = Instantiate(contentPrefab, contentRoot);
#endif

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    public void ClearContent()
    {
        if (contentRoot == null)
        {
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            DestroySlotObject(contentRoot.GetChild(i).gameObject);
        }
    }

    public void SetSurfaceMaterial(Material material)
    {
        if (surfaceRoot == null || material == null)
        {
            return;
        }

        Renderer[] renderers = surfaceRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }

    private static void DestroySlotObject(Object target)
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
