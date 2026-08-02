using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SmartDoorController : MonoBehaviour, IInteractable, IInteractionAvailability
{
    public enum DoorMotionMode
    {
        Slide,
        Rotate,
        SlideAndRotate
    }

    [Header("Interaction")]
    [SerializeField] private string closedDescription = "E  OPEN DOOR";
    [SerializeField] private string openDescription = "E  CLOSE DOOR";
    [SerializeField] private string lockedDescription = "DOOR LOCKED";
    [SerializeField] private bool canToggle = true;
    [SerializeField] private bool locked;
    [SerializeField] private bool allowDirectPlayerInteraction = true;

    [Header("Motion")]
    [SerializeField] private Transform movingRoot;
    [SerializeField] private DoorMotionMode motionMode = DoorMotionMode.Slide;
    [SerializeField] private bool captureClosedStateOnAwake = true;
    [SerializeField] private Vector3 closedLocalPosition;
    [SerializeField] private Vector3 openLocalPositionOffset = new Vector3(1.6f, 0f, 0f);
    [SerializeField] private Vector3 closedLocalEulerAngles;
    [SerializeField] private Vector3 openLocalEulerOffset;
    [SerializeField] private float moveDuration = 0.55f;
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useUnscaledTime;
    [SerializeField] private bool startOpen;

    [Header("Auto Close")]
    [SerializeField] private bool autoClose;
    [SerializeField] private float autoCloseDelay = 2.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip lockedClip;
    [Tooltip("Start position inside Open Clip. Keeps source files untouched when one recording contains both open and close sounds.")]
    [Min(0f)] [SerializeField] private float openClipStartSeconds;
    [Tooltip("0 plays to the end of Open Clip.")]
    [Min(0f)] [SerializeField] private float openClipDurationSeconds;
    [Tooltip("Start position inside Close Clip. Keeps source files untouched when one recording contains both open and close sounds.")]
    [Min(0f)] [SerializeField] private float closeClipStartSeconds;
    [Tooltip("0 plays to the end of Close Clip.")]
    [Min(0f)] [SerializeField] private float closeClipDurationSeconds;

    [Header("Events")]
    [SerializeField] private UnityEvent opened = new UnityEvent();
    [SerializeField] private UnityEvent closed = new UnityEvent();
    [SerializeField] private UnityEvent lockRejected = new UnityEvent();

    private Coroutine moveRoutine;
    private Coroutine autoCloseRoutine;
    private Coroutine audioStopRoutine;
    private Vector3 openLocalPosition;
    private Quaternion closedLocalRotation;
    private Quaternion openLocalRotation;

    public bool IsOpen { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsInteractionAvailable
    {
        get { return allowDirectPlayerInteraction; }
    }

    public bool AllowDirectPlayerInteraction
    {
        get { return allowDirectPlayerInteraction; }
    }

    public bool IsLocked
    {
        get { return locked; }
    }

    private void Awake()
    {
        ResolveReferences();
        CacheDoorTargets();

        if (startOpen)
        {
            SnapOpen();
        }
        else
        {
            SnapClosed();
        }
    }

    private void OnValidate()
    {
        moveDuration = Mathf.Max(0f, moveDuration);
        autoCloseDelay = Mathf.Max(0f, autoCloseDelay);
        openClipStartSeconds = Mathf.Max(0f, openClipStartSeconds);
        openClipDurationSeconds = Mathf.Max(0f, openClipDurationSeconds);
        closeClipStartSeconds = Mathf.Max(0f, closeClipStartSeconds);
        closeClipDurationSeconds = Mathf.Max(0f, closeClipDurationSeconds);
    }

    public void Interact()
    {
        if (!allowDirectPlayerInteraction)
        {
            return;
        }

        if (locked)
        {
            PlayClip(lockedClip);
            lockRejected.Invoke();
            return;
        }

        if (!canToggle && IsOpen)
        {
            return;
        }

        Toggle();
    }

    public string GetDescription()
    {
        if (locked)
        {
            return lockedDescription;
        }

        return IsOpen ? openDescription : closedDescription;
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (locked)
        {
            PlayClip(lockedClip);
            lockRejected.Invoke();
            return;
        }

        StartMove(true);
    }

    public void Close()
    {
        StartMove(false);
    }

    public void SetLocked(bool value)
    {
        locked = value;
    }

    public void SetDirectPlayerInteractionAllowed(bool value)
    {
        allowDirectPlayerInteraction = value;
    }

    public void SnapOpen()
    {
        StopMotion();
        ApplyDoorPose(true);
        IsOpen = true;
        IsMoving = false;
    }

    public void SnapClosed()
    {
        StopMotion();
        ApplyDoorPose(false);
        IsOpen = false;
        IsMoving = false;
    }

    private void StartMove(bool open)
    {
        ResolveReferences();
        CacheDoorTargets();
        StopMotion();
        moveRoutine = StartCoroutine(MoveRoutine(open));
    }

    private IEnumerator MoveRoutine(bool open)
    {
        IsMoving = true;

        Vector3 startPosition = movingRoot.localPosition;
        Quaternion startRotation = movingRoot.localRotation;
        Vector3 targetPosition = open ? openLocalPosition : closedLocalPosition;
        Quaternion targetRotation = open ? openLocalRotation : closedLocalRotation;

        PlayDoorClip(open);

        if (moveDuration <= 0f)
        {
            ApplyDoorPose(open);
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += GetDeltaTime();
                float t = Mathf.Clamp01(elapsed / moveDuration);
                float easedT = motionCurve != null ? motionCurve.Evaluate(t) : t;

                ApplyInterpolatedPose(startPosition, targetPosition, startRotation, targetRotation, easedT);
                yield return null;
            }
        }

        // Keep the completed pose consistent with the selected motion mode.
        // A rotate-only door must not inherit its configured slide offset here.
        ApplyDoorPose(open);
        IsOpen = open;
        IsMoving = false;
        moveRoutine = null;

        if (open)
        {
            opened.Invoke();
            StartAutoCloseTimer();
        }
        else
        {
            closed.Invoke();
        }
    }

    private void ApplyInterpolatedPose(Vector3 startPosition, Vector3 targetPosition, Quaternion startRotation, Quaternion targetRotation, float t)
    {
        if (motionMode == DoorMotionMode.Slide || motionMode == DoorMotionMode.SlideAndRotate)
        {
            movingRoot.localPosition = Vector3.LerpUnclamped(startPosition, targetPosition, t);
        }

        if (motionMode == DoorMotionMode.Rotate || motionMode == DoorMotionMode.SlideAndRotate)
        {
            movingRoot.localRotation = Quaternion.SlerpUnclamped(startRotation, targetRotation, t);
        }
    }

    private void ApplyDoorPose(bool open)
    {
        ResolveReferences();
        CacheDoorTargets();

        if (motionMode == DoorMotionMode.Slide || motionMode == DoorMotionMode.SlideAndRotate)
        {
            movingRoot.localPosition = open ? openLocalPosition : closedLocalPosition;
        }

        if (motionMode == DoorMotionMode.Rotate || motionMode == DoorMotionMode.SlideAndRotate)
        {
            movingRoot.localRotation = open ? openLocalRotation : closedLocalRotation;
        }
    }

    private void StartAutoCloseTimer()
    {
        if (!autoClose)
        {
            return;
        }

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
        }

        autoCloseRoutine = StartCoroutine(AutoCloseRoutine());
    }

    private IEnumerator AutoCloseRoutine()
    {
        if (autoCloseDelay > 0f)
        {
            float elapsed = 0f;
            while (elapsed < autoCloseDelay)
            {
                elapsed += GetDeltaTime();
                yield return null;
            }
        }

        autoCloseRoutine = null;
        Close();
    }

    private void StopMotion()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }

        IsMoving = false;
    }

    private void ResolveReferences()
    {
        if (movingRoot == null)
        {
            movingRoot = transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void CacheDoorTargets()
    {
        if (movingRoot == null)
        {
            return;
        }

        if (captureClosedStateOnAwake)
        {
            closedLocalPosition = movingRoot.localPosition;
            closedLocalEulerAngles = movingRoot.localEulerAngles;
            captureClosedStateOnAwake = false;
        }

        openLocalPosition = closedLocalPosition + openLocalPositionOffset;
        closedLocalRotation = Quaternion.Euler(closedLocalEulerAngles);
        openLocalRotation = Quaternion.Euler(closedLocalEulerAngles + openLocalEulerOffset);
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    private void PlayDoorClip(bool open)
    {
        AudioClip clip = open ? openClip : closeClip;
        float startSeconds = open ? openClipStartSeconds : closeClipStartSeconds;
        float durationSeconds = open ? openClipDurationSeconds : closeClipDurationSeconds;
        if (audioSource == null || clip == null)
        {
            return;
        }

        if (audioStopRoutine != null)
        {
            StopCoroutine(audioStopRoutine);
            audioStopRoutine = null;
        }

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.time = Mathf.Clamp(startSeconds, 0f, Mathf.Max(0f, clip.length - 0.01f));
        audioSource.Play();

        float remaining = Mathf.Max(0f, clip.length - audioSource.time);
        float duration = durationSeconds > 0f
            ? Mathf.Min(durationSeconds, remaining)
            : remaining;
        if (duration > 0f && duration < remaining - 0.01f)
        {
            audioStopRoutine = StartCoroutine(StopDoorAudioAfter(duration));
        }
    }

    private IEnumerator StopDoorAudioAfter(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += GetDeltaTime();
            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        audioStopRoutine = null;
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}
