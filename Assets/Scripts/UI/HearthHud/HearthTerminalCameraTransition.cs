using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthTerminalCameraTransition : MonoBehaviour
{
    [Header("Transition")]
    [SerializeField] private bool smoothTransitionEnabled = true;
    [SerializeField] private float enterDuration = 0.5f;
    [SerializeField] private float exitDuration = 0.5f;
    [SerializeField] private bool smoothExit = true;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Runtime Camera")]
    [SerializeField] private Camera transitionCamera;
    [SerializeField] private bool createTransitionCameraIfMissing = true;
    [SerializeField] private bool copyCameraSettings = true;
    [SerializeField] private bool copyAudioListenerIfMissing;

    public bool IsTransitioning { get; private set; }

    public bool SmoothTransitionEnabled
    {
        get { return smoothTransitionEnabled; }
    }

    public bool SmoothExit
    {
        get { return smoothExit; }
    }

    public float EnterDuration
    {
        get { return enterDuration; }
    }

    public float ExitDuration
    {
        get { return exitDuration; }
    }

    private void OnValidate()
    {
        enterDuration = Mathf.Max(0f, enterDuration);
        exitDuration = Mathf.Max(0f, exitDuration);
    }

    private void OnDisable()
    {
        IsTransitioning = false;
        SetCameraAndAudioEnabled(transitionCamera, false, false);
    }

    public IEnumerator TransitionToTerminal(Camera playerCamera, Camera terminalCamera, Action<Camera> setWorldCamera)
    {
        if (!CanRunTransition(playerCamera, terminalCamera) || enterDuration <= 0f)
        {
            yield break;
        }

        Camera runtimeCamera = EnsureTransitionCamera(playerCamera);
        if (runtimeCamera == null)
        {
            yield break;
        }

        IsTransitioning = true;

        CopyFromCamera(playerCamera, runtimeCamera);
        runtimeCamera.transform.position = playerCamera.transform.position;
        runtimeCamera.transform.rotation = playerCamera.transform.rotation;
        runtimeCamera.fieldOfView = playerCamera.fieldOfView;

        SetCameraAndAudioEnabled(playerCamera, false, false);
        SetCameraAndAudioEnabled(terminalCamera, false, false);
        SetCameraAndAudioEnabled(runtimeCamera, true, true);
        InvokeWorldCamera(setWorldCamera, runtimeCamera);

        yield return MoveCameraRoutine(
            runtimeCamera,
            playerCamera.transform.position,
            playerCamera.transform.rotation,
            playerCamera.fieldOfView,
            terminalCamera.transform.position,
            terminalCamera.transform.rotation,
            terminalCamera.fieldOfView,
            enterDuration);

        SetCameraAndAudioEnabled(runtimeCamera, false, false);
        SetCameraAndAudioEnabled(terminalCamera, true, true);
        InvokeWorldCamera(setWorldCamera, terminalCamera);

        IsTransitioning = false;
    }

    public IEnumerator TransitionToPlayer(
        Camera playerCamera,
        Camera terminalCamera,
        Action<Camera> setWorldCamera,
        bool playerCameraWasEnabled,
        bool terminalCameraWasEnabled,
        bool playerAudioWasEnabled,
        bool terminalAudioWasEnabled)
    {
        if (!CanRunTransition(playerCamera, terminalCamera) || !smoothExit || exitDuration <= 0f)
        {
            yield break;
        }

        Camera runtimeCamera = EnsureTransitionCamera(terminalCamera);
        if (runtimeCamera == null)
        {
            yield break;
        }

        IsTransitioning = true;

        CopyFromCamera(terminalCamera, runtimeCamera);
        runtimeCamera.transform.position = terminalCamera.transform.position;
        runtimeCamera.transform.rotation = terminalCamera.transform.rotation;
        runtimeCamera.fieldOfView = terminalCamera.fieldOfView;

        SetCameraAndAudioEnabled(playerCamera, false, false);
        SetCameraAndAudioEnabled(terminalCamera, false, false);
        SetCameraAndAudioEnabled(runtimeCamera, true, true);
        InvokeWorldCamera(setWorldCamera, runtimeCamera);

        yield return MoveCameraRoutine(
            runtimeCamera,
            terminalCamera.transform.position,
            terminalCamera.transform.rotation,
            terminalCamera.fieldOfView,
            playerCamera.transform.position,
            playerCamera.transform.rotation,
            playerCamera.fieldOfView,
            exitDuration);

        SetCameraAndAudioEnabled(runtimeCamera, false, false);
        SetCameraAndAudioEnabled(playerCamera, playerCameraWasEnabled, playerAudioWasEnabled);
        SetCameraAndAudioEnabled(terminalCamera, terminalCameraWasEnabled, terminalAudioWasEnabled);
        InvokeWorldCamera(setWorldCamera, playerCamera);

        IsTransitioning = false;
    }

    public bool CanRunTransition(Camera playerCamera, Camera terminalCamera)
    {
        return smoothTransitionEnabled && playerCamera != null && terminalCamera != null;
    }

    public void CancelTransition()
    {
        IsTransitioning = false;
        SetCameraAndAudioEnabled(transitionCamera, false, false);
    }

    public bool CanRunEnterTransition(Camera playerCamera, Camera terminalCamera)
    {
        return CanRunTransition(playerCamera, terminalCamera) && enterDuration > 0f;
    }

    public bool CanRunExitTransition(Camera playerCamera, Camera terminalCamera)
    {
        return CanRunTransition(playerCamera, terminalCamera) && smoothExit && exitDuration > 0f;
    }

    private IEnumerator MoveCameraRoutine(
        Camera cameraToMove,
        Vector3 startPosition,
        Quaternion startRotation,
        float startFieldOfView,
        Vector3 targetPosition,
        Quaternion targetRotation,
        float targetFieldOfView,
        float duration)
    {
        float elapsed = 0f;
        float realtimeStarted = Time.realtimeSinceStartup;
        float lastRealtime = realtimeStarted;

        while (elapsed < duration)
        {
            float nowRealtime = Time.realtimeSinceStartup;
            float realtimeDelta = Mathf.Max(0f, nowRealtime - lastRealtime);
            lastRealtime = nowRealtime;

            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (delta <= 0f || float.IsNaN(delta) || float.IsInfinity(delta))
            {
                delta = realtimeDelta;
            }

            elapsed += Mathf.Max(0f, delta);
            float normalized = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            float eased = transitionCurve != null ? transitionCurve.Evaluate(normalized) : normalized;

            cameraToMove.transform.position = Vector3.Lerp(startPosition, targetPosition, eased);
            cameraToMove.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, eased);
            cameraToMove.fieldOfView = Mathf.Lerp(startFieldOfView, targetFieldOfView, eased);

            if (nowRealtime - realtimeStarted > duration + 1f)
            {
                break;
            }

            yield return null;
        }

        cameraToMove.transform.position = targetPosition;
        cameraToMove.transform.rotation = targetRotation;
        cameraToMove.fieldOfView = targetFieldOfView;
    }

    private Camera EnsureTransitionCamera(Camera sourceCamera)
    {
        if (transitionCamera != null)
        {
            return transitionCamera;
        }

        if (!createTransitionCameraIfMissing)
        {
            return null;
        }

        GameObject cameraObject = new GameObject("Terminal Transition Camera");
        cameraObject.transform.SetParent(transform, false);
        transitionCamera = cameraObject.AddComponent<Camera>();
        transitionCamera.enabled = false;

        if (copyAudioListenerIfMissing && sourceCamera != null && sourceCamera.GetComponent<AudioListener>() != null)
        {
            AudioListener listener = cameraObject.AddComponent<AudioListener>();
            listener.enabled = false;
        }

        return transitionCamera;
    }

    private void CopyFromCamera(Camera sourceCamera, Camera targetCamera)
    {
        if (!copyCameraSettings || sourceCamera == null || targetCamera == null)
        {
            return;
        }

        targetCamera.clearFlags = sourceCamera.clearFlags;
        targetCamera.backgroundColor = sourceCamera.backgroundColor;
        targetCamera.cullingMask = sourceCamera.cullingMask;
        targetCamera.orthographic = sourceCamera.orthographic;
        targetCamera.orthographicSize = sourceCamera.orthographicSize;
        targetCamera.nearClipPlane = sourceCamera.nearClipPlane;
        targetCamera.farClipPlane = sourceCamera.farClipPlane;
        targetCamera.depth = sourceCamera.depth + 1f;
        targetCamera.targetDisplay = sourceCamera.targetDisplay;
        targetCamera.allowHDR = sourceCamera.allowHDR;
        targetCamera.allowMSAA = sourceCamera.allowMSAA;
    }

    private static void InvokeWorldCamera(Action<Camera> setWorldCamera, Camera camera)
    {
        if (setWorldCamera != null)
        {
            setWorldCamera(camera);
        }
    }

    private static void SetCameraAndAudioEnabled(Camera camera, bool cameraEnabled, bool audioEnabled)
    {
        if (camera == null)
        {
            return;
        }

        camera.enabled = cameraEnabled;

        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = audioEnabled;
        }
    }
}
