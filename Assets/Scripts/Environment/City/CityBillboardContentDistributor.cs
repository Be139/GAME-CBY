using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CityBillboardContentDistributor : MonoBehaviour
{
    private enum AspectClass
    {
        Portrait,
        Square,
        Landscape
    }

    private struct BillboardTarget
    {
        public CityBillboardPlacementSlot Slot;
        public CityBillboardContentController Controller;

        public BillboardTarget(CityBillboardPlacementSlot slot, CityBillboardContentController controller)
        {
            Slot = slot;
            Controller = controller;
        }
    }

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
        List<BillboardTarget> targets = new List<BillboardTarget>();

        for (int i = 0; i < slots.Count; i++)
        {
            CityBillboardContentController controller = EnsureContentController(slots[i]);
            if (controller != null)
            {
                targets.Add(new BillboardTarget(slots[i], controller));
            }
        }

        lastBillboardCount = targets.Count;
        lastImageCount = 0;
        lastAnimationCount = 0;

        if (targets.Count == 0)
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
        Shuffle(targets, random);
        Dictionary<Texture, int> imageUseCounts = new Dictionary<Texture, int>();

        int imageTarget = CalculateImageTarget(targets.Count, images.Count > 0, animations.Count > 0);
        for (int i = 0; i < targets.Count; i++)
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

            BillboardTarget target = targets[i];
            CityBillboardContentController controller = target.Controller;
            RecordUndo(controller, "Redistribute billboard media");

            if (useImage)
            {
                Texture image = SelectBestImageForBillboard(target.Slot, images, imageUseCounts, random);
                controller.SetImage(image);
                imageUseCounts[image] = GetImageUseCount(imageUseCounts, image) + 1;
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

    public void SetImageContents(Texture[] textures, bool redistributeImmediately)
    {
        List<Texture> validTextures = new List<Texture>();
        HashSet<Texture> seenTextures = new HashSet<Texture>();

        if (textures != null)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                Texture texture = textures[i];
                if (texture != null && seenTextures.Add(texture))
                {
                    validTextures.Add(texture);
                }
            }
        }

        imageContents = validTextures.ToArray();
        MarkDirty();

        if (redistributeImmediately)
        {
            RedistributeAll();
        }
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

    private static Texture SelectBestImageForBillboard(
        CityBillboardPlacementSlot slot,
        List<Texture> images,
        Dictionary<Texture, int> imageUseCounts,
        System.Random random)
    {
        float billboardAspect = GetBillboardAspect(slot);
        AspectClass targetClass = GetAspectClass(billboardAspect);
        List<Texture> matchingClass = new List<Texture>();

        for (int i = 0; i < images.Count; i++)
        {
            Texture image = images[i];
            if (GetAspectClass(GetTextureAspect(image)) == targetClass)
            {
                matchingClass.Add(image);
            }
        }

        List<Texture> candidates = matchingClass.Count > 0 ? matchingClass : images;
        float bestError = float.MaxValue;
        List<Texture> closestImages = new List<Texture>();

        for (int i = 0; i < candidates.Count; i++)
        {
            Texture image = candidates[i];
            float imageAspect = GetTextureAspect(image);
            float error = Mathf.Abs(Mathf.Log(Mathf.Max(0.0001f, billboardAspect / imageAspect)));

            if (error < bestError - 0.0001f)
            {
                bestError = error;
                closestImages.Clear();
                closestImages.Add(image);
            }
            else if (Mathf.Abs(error - bestError) <= 0.0001f)
            {
                closestImages.Add(image);
            }
        }

        int lowestUseCount = int.MaxValue;
        List<Texture> leastUsedImages = new List<Texture>();
        for (int i = 0; i < closestImages.Count; i++)
        {
            Texture image = closestImages[i];
            int useCount = GetImageUseCount(imageUseCounts, image);

            if (useCount < lowestUseCount)
            {
                lowestUseCount = useCount;
                leastUsedImages.Clear();
                leastUsedImages.Add(image);
            }
            else if (useCount == lowestUseCount)
            {
                leastUsedImages.Add(image);
            }
        }

        return leastUsedImages[random.Next(0, leastUsedImages.Count)];
    }

    private static float GetBillboardAspect(CityBillboardPlacementSlot slot)
    {
        if (slot == null)
        {
            return 1f;
        }

        Vector2 size = slot.BillboardSize;
        return Mathf.Max(0.0001f, size.x) / Mathf.Max(0.0001f, size.y);
    }

    private static float GetTextureAspect(Texture texture)
    {
        if (texture == null)
        {
            return 1f;
        }

        return Mathf.Max(1, texture.width) / (float)Mathf.Max(1, texture.height);
    }

    private static AspectClass GetAspectClass(float aspect)
    {
        if (aspect < 0.85f)
        {
            return AspectClass.Portrait;
        }

        if (aspect > 1.2f)
        {
            return AspectClass.Landscape;
        }

        return AspectClass.Square;
    }

    private static int GetImageUseCount(Dictionary<Texture, int> imageUseCounts, Texture image)
    {
        int useCount;
        return imageUseCounts.TryGetValue(image, out useCount) ? useCount : 0;
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
