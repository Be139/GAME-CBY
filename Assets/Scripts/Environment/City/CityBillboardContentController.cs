using UnityEngine;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    private void Awake()
    {
        ResolveRenderer();
        ApplyAssignedContent();
    }

    private void OnEnable()
    {
        if (Application.isPlaying && contentKind == BillboardContentKind.Video && playOnAwake)
        {
            Play();
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
        SetColorIfSupported(block, "_BaseColor", Color.white);
        SetColorIfSupported(block, "_Color", Color.white);
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
        PrepareRendererColor();

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

    private void PrepareRendererColor()
    {
        if (targetRenderer == null)
        {
            return;
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        SetColorIfSupported(block, "_BaseColor", Color.white);
        SetColorIfSupported(block, "_Color", Color.white);
        targetRenderer.SetPropertyBlock(block);
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
