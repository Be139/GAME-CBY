using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CityBuildingPlacementSlot : MonoBehaviour
{
    [SerializeField] private string lotName;
    [SerializeField] private int slotIndex;
    [SerializeField] private Vector2 footprintSize;
    [SerializeField] private float targetHeight;
    [SerializeField] private float facingAngle;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Transform billboardAnchor;

    public string LotName
    {
        get { return lotName; }
    }

    public int SlotIndex
    {
        get { return slotIndex; }
    }

    public Vector2 FootprintSize
    {
        get { return footprintSize; }
    }

    public float TargetHeight
    {
        get { return targetHeight; }
    }

    public float FacingAngle
    {
        get { return facingAngle; }
    }

    public Transform ContentRoot
    {
        get { return contentRoot; }
    }

    public Transform BillboardAnchor
    {
        get { return billboardAnchor; }
    }

    public void Configure(string newLotName, int newSlotIndex, Vector2 newFootprintSize, float newTargetHeight, float newFacingAngle, Transform newContentRoot, Transform newBillboardAnchor)
    {
        lotName = newLotName;
        slotIndex = newSlotIndex;
        footprintSize = newFootprintSize;
        targetHeight = newTargetHeight;
        facingAngle = newFacingAngle;
        contentRoot = newContentRoot;
        billboardAnchor = newBillboardAnchor;
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

    public GameObject ReplaceWithPrefab(GameObject prefab)
    {
        if (prefab == null || contentRoot == null)
        {
            return null;
        }

        ClearContent();

#if UNITY_EDITOR
        GameObject instance;
        if (!Application.isPlaying)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, contentRoot);
            Undo.RegisterCreatedObjectUndo(instance, "Replace city building slot content");
        }
        else
        {
            instance = Instantiate(prefab, contentRoot);
        }
#else
        GameObject instance = Instantiate(prefab, contentRoot);
#endif

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        return instance;
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
