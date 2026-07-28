using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CityBillboardContentDistributor : MonoBehaviour
{
    [Header("Billboard Scope")]
    [SerializeField] private Transform billboardRoot;
    [SerializeField] private string billboardRootName = "CityBillboards_BUILDING";
    [SerializeField] private bool includeInactiveBillboards = true;
    [SerializeField] private Material mediaSurfaceMaterial;
    [SerializeField, Min(0.02f)] private float repairedSurfaceThickness = 0.28f;

    [Header("Media Pools")]
    [SerializeField] private Texture[] imageContents;
    [SerializeField] private VideoClip[] animationContents;

    [Header("Content Ratio (Image : Animation)")]
    [SerializeField, Min(0f)] private float imageWeight = 7f;
    [SerializeField, Min(0f)] private float animationWeight = 3f;
    [SerializeField] private bool useExactWholeSetRatio = true;

    [Header("Randomization")]
    [SerializeField] private int randomSeed = 20260716;
    [SerializeField] private bool advanceSeedAfterRedistribute = true;

    [Header("Last Result")]
    [SerializeField] private int lastBillboardCount;
    [SerializeField] private int lastImageCount;
    [SerializeField] private int lastAnimationCount;

    public Transform BillboardRoot
    {
        get { return billboardRoot; }
        set { billboardRoot = value; }
    }

    public float ImageWeight
    {
        get { return imageWeight; }
    }

    public float AnimationWeight
    {
        get { return animationWeight; }
    }

    public float ImageShare
    {
        get { return GetNormalizedImageShare(); }
    }

    public Material MediaSurfaceMaterial
    {
        get { return mediaSurfaceMaterial; }
        set { mediaSurfaceMaterial = value; }
    }

    public void FindBillboardRoot()
    {
        GameObject found = GameObject.Find(billboardRootName);
        billboardRoot = found != null ? found.transform : null;

        if (billboardRoot == null)
        {
            Debug.LogWarning("CityBillboardContentDistributor: could not find billboard root '" + billboardRootName + "'.");
        }

        MarkDirty();
    }

    public int PrepareExistingBillboards()
    {
        List<CityBillboardPlacementSlot> slots = CollectBillboardSlots();
        int prepared = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            if (EnsureContentController(slots[i]) != null)
            {
                prepared++;
            }
        }

        MarkDirty();
        Debug.Log("CityBillboardContentDistributor: prepared " + prepared + " billboard screens for image/video content.");
        return prepared;
    }

    public int ApplySurfaceMaterialToAll()
    {
        Material targetMaterial = ResolveMediaSurfaceMaterial();
        if (targetMaterial == null)
        {
            Debug.LogWarning("CityBillboardContentDistributor: no media surface material is available.");
            return 0;
        }

        List<CityBillboardPlacementSlot> slots = CollectBillboardSlots();
        int updated = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            Renderer renderer = EnsureSurfaceRenderer(slots[i]);
            if (renderer == null)
            {
                continue;
            }

            RecordUndo(renderer, "Apply billboard HDR material");
            renderer.sharedMaterial = targetMaterial;

            CityBillboardContentController controller = EnsureContentController(slots[i]);
            if (controller != null)
            {
                controller.ApplyAssignedContent();
            }

            MarkObjectDirty(renderer);
            updated++;
        }

        MarkDirty();
        Debug.Log("CityBillboardContentDistributor: applied the HDR media material to " + updated + " billboard screens.");
        return updated;
    }

    public void RedistributeAll()
    {
        List<Texture> images = GetValidImages();
        List<VideoClip> animations = GetValidAnimations();
        List<CityBillboardPlacementSlot> slots = CollectBillboardSlots();
        List<CityBillboardContentController> controllers = new List<CityBillboardContentController>();

        for (int i = 0; i < slots.Count; i++)
        {
            CityBillboardContentController controller = EnsureContentController(slots[i]);
            if (controller != null)
            {
                controllers.Add(controller);
            }
        }

        lastBillboardCount = controllers.Count;
        lastImageCount = 0;
        lastAnimationCount = 0;

        if (controllers.Count == 0)
        {
            Debug.LogWarning("CityBillboardContentDistributor: no billboard screens were found under the selected root.");
            MarkDirty();
            return;
        }

        if (images.Count == 0 && animations.Count == 0)
        {
            Debug.LogWarning("CityBillboardContentDistributor: add at least one image or VideoClip to the media pools before redistributing.");
            MarkDirty();
            return;
        }

        System.Random random = new System.Random(randomSeed);
        Shuffle(controllers, random);

        int imageTarget = CalculateImageTarget(controllers.Count, images.Count > 0, animations.Count > 0);
        for (int i = 0; i < controllers.Count; i++)
        {
            bool useImage;
            if (useExactWholeSetRatio)
            {
                useImage = i < imageTarget;
            }
            else
            {
                useImage = ChooseImageRandomly(random, images.Count > 0, animations.Count > 0);
            }

            CityBillboardContentController controller = controllers[i];
            RecordUndo(controller, "Redistribute billboard media");

            if (useImage)
            {
                controller.SetImage(images[random.Next(0, images.Count)]);
                lastImageCount++;
            }
            else
            {
                controller.SetVideo(animations[random.Next(0, animations.Count)]);
                lastAnimationCount++;
            }

            MarkObjectDirty(controller);
        }

        if (advanceSeedAfterRedistribute)
        {
            randomSeed++;
        }

        MarkDirty();
        Debug.Log("CityBillboardContentDistributor: redistributed " + lastBillboardCount + " billboards. Images: " + lastImageCount + ", looping animations: " + lastAnimationCount + ".");
    }

    public void SetWeights(float newImageWeight, float newAnimationWeight, bool redistributeImmediately)
    {
        imageWeight = Mathf.Max(0f, newImageWeight);
        animationWeight = Mathf.Max(0f, newAnimationWeight);

        if (imageWeight <= 0f && animationWeight <= 0f)
        {
            imageWeight = 1f;
        }

        MarkDirty();

        if (redistributeImmediately)
        {
            RedistributeAll();
        }
    }

    public void ClearAllContent()
    {
        List<CityBillboardPlacementSlot> slots = CollectBillboardSlots();
        int cleared = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            CityBillboardContentController controller = FindContentController(slots[i]);
            if (controller == null)
            {
                continue;
            }

            RecordUndo(controller, "Clear billboard media");
            controller.ClearContent();
            MarkObjectDirty(controller);
            cleared++;
        }

        lastBillboardCount = slots.Count;
        lastImageCount = 0;
        lastAnimationCount = 0;
        MarkDirty();
        Debug.Log("CityBillboardContentDistributor: cleared media assignments from " + cleared + " billboard screens.");
    }

    private List<CityBillboardPlacementSlot> CollectBillboardSlots()
    {
        if (billboardRoot == null)
        {
            FindBillboardRoot();
        }

        List<CityBillboardPlacementSlot> result = new List<CityBillboardPlacementSlot>();
        if (billboardRoot == null)
        {
            return result;
        }

        CityBillboardPlacementSlot[] slots = billboardRoot.GetComponentsInChildren<CityBillboardPlacementSlot>(includeInactiveBillboards);
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                result.Add(slots[i]);
            }
        }

        return result;
    }

    private CityBillboardContentController EnsureContentController(CityBillboardPlacementSlot slot)
    {
        if (slot == null || slot.SurfaceRoot == null)
        {
            return null;
        }

        Renderer renderer = EnsureSurfaceRenderer(slot);
        if (renderer == null)
        {
            return null;
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
        MarkObjectDirty(controller);
        return controller;
    }

    private Renderer EnsureSurfaceRenderer(CityBillboardPlacementSlot slot)
    {
        Renderer existing = slot.SurfaceRoot.GetComponentInChildren<Renderer>(true);
        if (existing != null)
        {
            if (mediaSurfaceMaterial == null && existing.sharedMaterial != null)
            {
                mediaSurfaceMaterial = existing.sharedMaterial;
            }

            if (mediaSurfaceMaterial != null && existing.sharedMaterial != mediaSurfaceMaterial)
            {
                RecordUndo(existing, "Assign billboard media material");
                existing.sharedMaterial = mediaSurfaceMaterial;
                MarkObjectDirty(existing);
            }

            return existing;
        }

        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = "BlankBillboardSurface_Media";
        surface.transform.SetParent(slot.SurfaceRoot, false);
        surface.transform.localPosition = Vector3.zero;
        surface.transform.localRotation = Quaternion.identity;
        surface.transform.localScale = new Vector3(
            Mathf.Max(0.1f, slot.BillboardSize.x),
            Mathf.Max(0.1f, slot.BillboardSize.y),
            Mathf.Max(0.02f, repairedSurfaceThickness));

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCreatedObjectUndo(surface, "Repair billboard media surface");
        }
#endif

        Collider collider = surface.GetComponent<Collider>();
        if (collider != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(collider);
            }
            else
            {
                Destroy(collider);
            }
#else
            Destroy(collider);
#endif
        }

        Renderer renderer = surface.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = ResolveMediaSurfaceMaterial();
        }

        return renderer;
    }

    private Material ResolveMediaSurfaceMaterial()
    {
        if (mediaSurfaceMaterial != null)
        {
            return mediaSurfaceMaterial;
        }

        if (billboardRoot != null)
        {
            Renderer[] renderers = billboardRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].sharedMaterial != null)
                {
                    mediaSurfaceMaterial = renderers[i].sharedMaterial;
                    return mediaSurfaceMaterial;
                }
            }
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        mediaSurfaceMaterial = new Material(shader);
        mediaSurfaceMaterial.name = "M_CityBillboard_Media_Fallback";

        if (mediaSurfaceMaterial.HasProperty("_BaseColor"))
        {
            mediaSurfaceMaterial.SetColor("_BaseColor", Color.white);
        }
        else
        {
            mediaSurfaceMaterial.color = Color.white;
        }

        return mediaSurfaceMaterial;
    }

    private static CityBillboardContentController FindContentController(CityBillboardPlacementSlot slot)
    {
        if (slot == null || slot.SurfaceRoot == null)
        {
            return null;
        }

        return slot.SurfaceRoot.GetComponentInChildren<CityBillboardContentController>(true);
    }

    private int CalculateImageTarget(int total, bool hasImages, bool hasAnimations)
    {
        if (!hasImages)
        {
            return 0;
        }

        if (!hasAnimations)
        {
            return total;
        }

        return Mathf.Clamp(Mathf.RoundToInt(total * GetNormalizedImageShare()), 0, total);
    }

    private bool ChooseImageRandomly(System.Random random, bool hasImages, bool hasAnimations)
    {
        if (!hasImages)
        {
            return false;
        }

        if (!hasAnimations)
        {
            return true;
        }

        return random.NextDouble() < GetNormalizedImageShare();
    }

    private float GetNormalizedImageShare()
    {
        float totalWeight = Mathf.Max(0f, imageWeight) + Mathf.Max(0f, animationWeight);
        return totalWeight > 0f ? Mathf.Max(0f, imageWeight) / totalWeight : 1f;
    }

    private List<Texture> GetValidImages()
    {
        List<Texture> result = new List<Texture>();
        if (imageContents == null)
        {
            return result;
        }

        for (int i = 0; i < imageContents.Length; i++)
        {
            if (imageContents[i] != null)
            {
                result.Add(imageContents[i]);
            }
        }

        return result;
    }

    private List<VideoClip> GetValidAnimations()
    {
        List<VideoClip> result = new List<VideoClip>();
        if (animationContents == null)
        {
            return result;
        }

        for (int i = 0; i < animationContents.Length; i++)
        {
            if (animationContents[i] != null)
            {
                result.Add(animationContents[i]);
            }
        }

        return result;
    }

    private static void Shuffle<T>(IList<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(0, i + 1);
            T temp = list[i];
            list[i] = list[swapIndex];
            list[swapIndex] = temp;
        }
    }

    private static void RecordUndo(Object target, string undoName)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && target != null)
        {
            Undo.RecordObject(target, undoName);
        }
#endif
    }

    private static void MarkObjectDirty(Object target)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && target != null)
        {
            EditorUtility.SetDirty(target);
        }
#endif
    }

    private void MarkDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
#endif
    }
}
