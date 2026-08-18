using UnityEngine;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class CityBillboardContentController : MonoBehaviour
{
    public enum BillboardContentKind
    {
        None,
        Image,
        Video
    }

    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Assigned Content")]
    [SerializeField] private BillboardContentKind contentKind;
    [SerializeField] private Texture imageTexture;
    [SerializeField] private VideoClip videoClip;

    [Header("HDR Brightness")]
    [SerializeField, Min(0f)] private float imageBrightness = 2.2f;
    [SerializeField, Min(0f)] private float videoBrightness = 2.6f;

    [Header("Video Playback")]
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loopVideo = true;
    [SerializeField] private bool muteVideo = true;
    [SerializeField] private bool pauseWhenInvisible = true;

    private VideoPlayer videoPlayer;
    private bool resumeWhenVisible;

    public BillboardContentKind ContentKind
    {
        get { return contentKind; }
    }

    public Texture ImageTexture
    {
        get { return imageTexture; }
    }

    public VideoClip VideoClip
    {
        get { return videoClip; }
    }

    public Renderer TargetRenderer
    {
        get { return targetRenderer; }
    }

    public float ImageBrightness
    {
        get { return imageBrightness; }
    }

    public float VideoBrightness
    {
        get { return videoBrightness; }
    }

    private void Awake()
    {
        ResolveRenderer();
        if (Application.isPlaying)
        {
            ApplyAssignedContent();
        }
        else
        {
            ApplyEditorPreview();
        }
    }

    private void OnEnable()
    {
        ResolveRenderer();
        if (!Application.isPlaying)
        {
            ApplyEditorPreview();
            return;
        }

        if (contentKind == BillboardContentKind.Video && playOnAwake)
        {
            Play();
        }
    }

    private void OnValidate()
    {
        imageBrightness = Mathf.Max(0f, imageBrightness);
        videoBrightness = Mathf.Max(0f, videoBrightness);

        if (!Application.isPlaying)
        {
            ResolveRenderer();
            ApplyEditorPreview();
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }

    private void OnBecameInvisible()
    {
        if (!Application.isPlaying || !pauseWhenInvisible || videoPlayer == null || !videoPlayer.isPlaying)
        {
            return;
        }

        videoPlayer.Pause();
        resumeWhenVisible = true;
    }

    private void OnBecameVisible()
    {
        if (!Application.isPlaying || !resumeWhenVisible || contentKind != BillboardContentKind.Video)
        {
            return;
        }

        resumeWhenVisible = false;
        Play();
    }

    public void Configure(Renderer renderer)
    {
        targetRenderer = renderer != null ? renderer : GetComponent<Renderer>();
        ApplyAssignedContent();
    }

    public void SetImage(Texture texture)
    {
        imageTexture = texture;
        videoClip = null;
        contentKind = texture != null ? BillboardContentKind.Image : BillboardContentKind.None;
        StopVideoPlayer();

        if (texture == null)
        {
            ClearRendererContent();
            return;
        }

        ApplyImageTexture(texture);
    }

    public void SetVideo(VideoClip clip)
    {
        videoClip = clip;
        imageTexture = null;
        contentKind = clip != null ? BillboardContentKind.Video : BillboardContentKind.None;

        if (clip == null)
        {
            StopVideoPlayer();
            ClearRendererContent();
            return;
        }

        ConfigureVideoPlayer();
        if (Application.isPlaying && playOnAwake)
        {
            Play();
        }
    }

    public void ClearContent()
    {
        contentKind = BillboardContentKind.None;
        imageTexture = null;
        videoClip = null;
        StopVideoPlayer();
        ClearRendererContent();
    }

    public void SetBrightness(float newImageBrightness, float newVideoBrightness)
    {
        imageBrightness = Mathf.Max(0f, newImageBrightness);
        videoBrightness = Mathf.Max(0f, newVideoBrightness);
        ApplyAssignedContent();
    }

    public void Play()
    {
        if (contentKind != BillboardContentKind.Video || videoClip == null)
        {
            return;
        }

        ConfigureVideoPlayer();
        videoPlayer.Play();
    }

    public void Pause()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }

    public void Stop()
    {
        StopVideoPlayer();
    }

    public void ApplyAssignedContent()
    {
        ResolveRenderer();

        if (contentKind == BillboardContentKind.Image)
        {
            StopVideoPlayer();
            ApplyImageTexture(imageTexture);
            return;
        }

        if (contentKind == BillboardContentKind.Video)
        {
            ConfigureVideoPlayer();
            return;
        }

        ClearRendererContent();
    }

    private void ApplyEditorPreview()
    {
        if (contentKind == BillboardContentKind.Image)
        {
            ApplyImageTexture(imageTexture);
        }
        else if (contentKind == BillboardContentKind.None)
        {
            ClearRendererContent();
        }
    }

    private void ResolveRenderer()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
    }

    private void ApplyImageTexture(Texture texture)
    {
        ResolveRenderer();
        if (targetRenderer == null)
        {
            return;
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        SetTextureIfSupported(block, "_BaseMap", texture);
        SetTextureIfSupported(block, "_MainTex", texture);
        SetTextureIfSupported(block, "_EmissionMap", texture);
        Color hdrColor = CreateHdrColor(imageBrightness);
        SetColorIfSupported(block, "_BaseColor", hdrColor);
        SetColorIfSupported(block, "_Color", hdrColor);
        SetColorIfSupported(block, "_EmissionColor", hdrColor);
        targetRenderer.SetPropertyBlock(block);
    }

    private void ConfigureVideoPlayer()
    {
        ResolveRenderer();
        if (targetRenderer == null || videoClip == null)
        {
            return;
        }

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                videoPlayer = Undo.AddComponent<VideoPlayer>(gameObject);
            }
            else
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
#else
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
#endif
        }

        ClearRendererContent();
        PrepareRendererColor(videoBrightness);

        videoPlayer.enabled = true;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = targetRenderer;
        videoPlayer.targetMaterialProperty = ResolveVideoTextureProperty();
        videoPlayer.playOnAwake = playOnAwake;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = muteVideo ? VideoAudioOutputMode.None : VideoAudioOutputMode.Direct;
    }

    private void StopVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.enabled = false;
        resumeWhenVisible = false;
    }

    private void ClearRendererContent()
    {
        ResolveRenderer();
        if (targetRenderer == null)
        {
            return;
        }

        targetRenderer.SetPropertyBlock(null);
    }

    private void PrepareRendererColor(float brightness)
    {
        if (targetRenderer == null)
        {
            return;
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        Color hdrColor = CreateHdrColor(brightness);
        SetColorIfSupported(block, "_BaseColor", hdrColor);
        SetColorIfSupported(block, "_Color", hdrColor);
        SetColorIfSupported(block, "_EmissionColor", hdrColor);
        targetRenderer.SetPropertyBlock(block);
    }

    private static Color CreateHdrColor(float brightness)
    {
        float safeBrightness = Mathf.Max(0f, brightness);
        return new Color(safeBrightness, safeBrightness, safeBrightness, 1f);
    }

    private string ResolveVideoTextureProperty()
    {
        if (RendererSupportsProperty("_BaseMap"))
        {
            return "_BaseMap";
        }

        return "_MainTex";
    }

    private void SetTextureIfSupported(MaterialPropertyBlock block, string propertyName, Texture texture)
    {
        if (RendererSupportsProperty(propertyName))
        {
            block.SetTexture(propertyName, texture);
        }
    }

    private void SetColorIfSupported(MaterialPropertyBlock block, string propertyName, Color color)
    {
        if (RendererSupportsProperty(propertyName))
        {
            block.SetColor(propertyName, color);
        }
    }

    private bool RendererSupportsProperty(string propertyName)
    {
        if (targetRenderer == null || targetRenderer.sharedMaterials == null)
        {
            return false;
        }

        Material[] materials = targetRenderer.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null && materials[i].HasProperty(propertyName))
            {
                return true;
            }
        }

        return false;
    }
}
