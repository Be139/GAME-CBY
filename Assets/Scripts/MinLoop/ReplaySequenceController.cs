using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaySequenceController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private ComfortActionInteractable comfortAction;
    [SerializeField] private bool createFallbackComfortActionIfMissing = true;
    [SerializeField] private Vector3 fallbackComfortActionLocalPosition = new Vector3(0f, 0.65f, 1.2f);
    [SerializeField] private Vector3 fallbackComfortActionColliderSize = new Vector3(1.1f, 1.1f, 1.1f);

    [Header("Actors")]
    [SerializeField] private SimpleActorCueController childActor;
    [SerializeField] private SimpleActorCueController motherActor;
    [SerializeField] private SimpleActorCueController fatherActor;
    [SerializeField] private Transform doorLookTarget;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 0.75f;
    [SerializeField] private float waitBeforeDoorLookAfterWake = 0.35f;
    [SerializeField] private float waitAfterDoorLook = 0.8f;
    [SerializeField] private float waitAfterComfort = 0.75f;
    [SerializeField] private float childSecondDoorLookHold = 1.25f;

    [Header("Subtitle Lines")]
    [SerializeField] private bool seedDefaultLinesIfEmpty = true;
    [SerializeField] private List<MinLoopSubtitleLine> preComfortLines = new List<MinLoopSubtitleLine>();
    [SerializeField] private List<MinLoopSubtitleLine> comfortLines = new List<MinLoopSubtitleLine>();
    [SerializeField] private List<MinLoopSubtitleLine> morningLines = new List<MinLoopSubtitleLine>();

    private Coroutine replayRoutine;
    private bool comfortReceived;

    public bool IsRunning { get; private set; }
    public bool IsWaitingForComfort { get; private set; }

    public bool HasComfortAction
    {
        get { return comfortAction != null; }
    }

    public bool CanCreateFallbackComfortAction
    {
        get { return createFallbackComfortActionIfMissing; }
    }

    private void Reset()
    {
        SeedDefaultLines();
    }

    private void Awake()
    {
        ResolveReferences();
        if (seedDefaultLinesIfEmpty)
        {
            SeedDefaultLinesIfNeeded();
        }

        if (comfortAction != null)
        {
            comfortAction.SetSequenceController(this);
            comfortAction.SetAvailable(false);
        }
    }

    private void OnValidate()
    {
        initialDelay = Mathf.Max(0f, initialDelay);
        waitBeforeDoorLookAfterWake = Mathf.Max(0f, waitBeforeDoorLookAfterWake);
        waitAfterDoorLook = Mathf.Max(0f, waitAfterDoorLook);
        waitAfterComfort = Mathf.Max(0f, waitAfterComfort);
        childSecondDoorLookHold = Mathf.Max(0f, childSecondDoorLookHold);
        fallbackComfortActionColliderSize.x = Mathf.Max(0.1f, fallbackComfortActionColliderSize.x);
        fallbackComfortActionColliderSize.y = Mathf.Max(0.1f, fallbackComfortActionColliderSize.y);
        fallbackComfortActionColliderSize.z = Mathf.Max(0.1f, fallbackComfortActionColliderSize.z);
    }

    public void BeginReplay(MinLoopFlowController owner)
    {
        ResolveReferences();

        if (owner != null)
        {
            flowController = owner;
        }

        if (replayRoutine != null)
        {
            CancelReplay();
        }

        replayRoutine = StartCoroutine(ReplayRoutine());
    }

    public void PerformComfortAction()
    {
        if (!IsWaitingForComfort)
        {
            return;
        }

        comfortReceived = true;
        IsWaitingForComfort = false;

        if (comfortAction != null)
        {
            comfortAction.SetAvailable(false);
        }

        if (flowController != null)
        {
            flowController.NotifyComfortActionPerformed();
        }
    }

    public void CancelReplay()
    {
        if (replayRoutine != null)
        {
            StopCoroutine(replayRoutine);
            replayRoutine = null;
        }

        IsRunning = false;
        IsWaitingForComfort = false;
        comfortReceived = false;

        if (comfortAction != null)
        {
            comfortAction.SetAvailable(false);
        }

        if (subtitlePlayer != null)
        {
            subtitlePlayer.Hide();
        }
    }

    private IEnumerator ReplayRoutine()
    {
        IsRunning = true;
        IsWaitingForComfort = false;
        comfortReceived = false;

        if (comfortAction != null)
        {
            comfortAction.SetAvailable(false);
        }

        if (motherActor != null)
        {
            motherActor.SetVisible(false);
        }

        if (fatherActor != null)
        {
            fatherActor.SetVisible(false);
        }

        if (childActor != null)
        {
            childActor.SetVisible(true);
            childActor.PlaySleep();
        }

        yield return Wait(initialDelay);

        if (childActor != null)
        {
            childActor.PlayNightmareWake();
        }

        yield return Wait(waitBeforeDoorLookAfterWake);

        if (childActor != null)
        {
            childActor.LookAt(doorLookTarget);
        }

        yield return Wait(waitAfterDoorLook);
        yield return PlaySubtitleLines(preComfortLines);

        IsWaitingForComfort = true;
        if (comfortAction != null)
        {
            comfortAction.SetAvailable(true);
        }

        if (flowController != null)
        {
            flowController.NotifyReplayComfortReady();
        }

        while (!comfortReceived)
        {
            yield return null;
        }

        if (childActor != null)
        {
            childActor.PlayComforted();
        }

        yield return Wait(waitAfterComfort);
        yield return PlaySubtitleLines(comfortLines);

        if (childActor != null)
        {
            childActor.LookAt(doorLookTarget);
        }

        yield return Wait(childSecondDoorLookHold);

        if (flowController != null)
        {
            flowController.NotifyMorningReviewStarted();
        }

        if (motherActor != null)
        {
            motherActor.SetVisible(true);
            motherActor.PlayMorning();
        }

        if (fatherActor != null)
        {
            fatherActor.SetVisible(true);
            fatherActor.PlayMorning();
        }

        yield return PlaySubtitleLines(morningLines);

        IsRunning = false;
        replayRoutine = null;

        if (flowController != null)
        {
            flowController.NotifyReplayCompleted();
        }
    }

    private IEnumerator PlaySubtitleLines(IList<MinLoopSubtitleLine> lines)
    {
        if (subtitlePlayer != null)
        {
            yield return subtitlePlayer.PlayLines(lines);
        }
        else
        {
            yield return Wait(EstimateSubtitleDuration(lines));
        }
    }

    private float EstimateSubtitleDuration(IList<MinLoopSubtitleLine> lines)
    {
        if (lines == null)
        {
            return 0f;
        }

        float total = 0f;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] == null)
            {
                continue;
            }

            total += Mathf.Max(0f, lines[i].startDelay);
            total += Mathf.Max(0f, lines[i].holdSeconds);
        }

        return total;
    }

    private IEnumerator Wait(float seconds)
    {
        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);
        }
    }

    private void ResolveReferences()
    {
        if (flowController == null)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }

        if (subtitlePlayer == null)
        {
            subtitlePlayer = FindObjectOfType<MinLoopSubtitlePlayer>();
        }

        if (comfortAction == null)
        {
            comfortAction = GetComponentInChildren<ComfortActionInteractable>(true);
        }

        if (comfortAction == null && createFallbackComfortActionIfMissing)
        {
            comfortAction = CreateFallbackComfortAction();
        }

        if (comfortAction != null)
        {
            comfortAction.SetSequenceController(this);
        }
    }

    private ComfortActionInteractable CreateFallbackComfortAction()
    {
        GameObject comfortObject = new GameObject("Generated_ComfortAction_Bedside", typeof(BoxCollider));
        comfortObject.transform.SetParent(transform, false);
        comfortObject.transform.localPosition = fallbackComfortActionLocalPosition;
        comfortObject.transform.localRotation = Quaternion.identity;

        BoxCollider collider = comfortObject.GetComponent<BoxCollider>();
        collider.size = fallbackComfortActionColliderSize;

        ComfortActionInteractable generatedAction = comfortObject.AddComponent<ComfortActionInteractable>();
        generatedAction.SetSequenceController(this);
        generatedAction.SetAvailable(false);
        return generatedAction;
    }

    private void SeedDefaultLinesIfNeeded()
    {
        if (preComfortLines.Count == 0 && comfortLines.Count == 0 && morningLines.Count == 0)
        {
            SeedDefaultLines();
        }
    }

    private void SeedDefaultLines()
    {
        preComfortLines.Clear();
        comfortLines.Clear();
        morningLines.Clear();

        AddLine(preComfortLines, "系统", "02:47，检测到儿童睡眠中断。心率上升，噩梦可能性高。", 2.8f);
        AddLine(preComfortLines, "孩子", "妈妈。", 2.0f);
        AddLine(preComfortLines, "陪伴单元", "我在这里。你刚才做了一个很重的梦。", 2.8f);
        AddLine(preComfortLines, "陪伴单元", "这个点过去敲门，她明天会很累。先执行低刺激安抚。", 3.4f);

        AddLine(comfortLines, "陪伴单元", "看着这盏灯，跟着我的声音慢慢呼吸。", 2.8f);
        AddLine(comfortLines, "系统", "呼吸频率回落。哭泣停止。陪伴单元判定：安抚有效。", 3.0f);
        AddLine(comfortLines, "孩子", "嗯。", 1.6f);

        AddLine(morningLines, "母亲", "它昨晚处理得挺好。", 2.4f);
        AddLine(morningLines, "母亲", "但他后来有没有找我？", 2.6f);
        AddLine(morningLines, "父亲", "最起码，我们晚上再也不用为他担心了，不是吗？", 3.2f);
        AddLine(morningLines, "系统", "复盘结束。请回到终端提交处置意见。", 2.6f);
    }

    private void AddLine(List<MinLoopSubtitleLine> target, string speaker, string text, float holdSeconds)
    {
        MinLoopSubtitleLine line = new MinLoopSubtitleLine();
        line.speaker = speaker;
        line.text = text;
        line.holdSeconds = holdSeconds;
        target.Add(line);
    }
}
