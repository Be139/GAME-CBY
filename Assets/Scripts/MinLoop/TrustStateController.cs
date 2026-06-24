using UnityEngine;
using UnityEngine.Events;

public class TrustStateController : MonoBehaviour
{
    [Header("Trust Value")]
    [SerializeField] private int startingTrust = 50;
    [SerializeField] private int minTrust = 0;
    [SerializeField] private int maxTrust = 100;

    [Header("Choice Delta")]
    [SerializeField] private int optionADelta = 8;
    [SerializeField] private int optionBDelta = -3;

    [Header("Events")]
    [SerializeField] private UnityEvent<int> trustChanged = new UnityEvent<int>();

    [Header("Runtime State")]
    [SerializeField] private int currentTrust;
    [SerializeField] private int lastDelta;

    public int CurrentTrust
    {
        get { return currentTrust; }
    }

    public int LastDelta
    {
        get { return lastDelta; }
    }

    public UnityEvent<int> TrustChanged
    {
        get { return trustChanged; }
    }

    private void Awake()
    {
        ResetTrust();
    }

    private void OnValidate()
    {
        if (maxTrust < minTrust)
        {
            maxTrust = minTrust;
        }

        startingTrust = Mathf.Clamp(startingTrust, minTrust, maxTrust);
    }

    public void ResetTrust()
    {
        currentTrust = Mathf.Clamp(startingTrust, minTrust, maxTrust);
        lastDelta = 0;
        trustChanged.Invoke(currentTrust);
    }

    public int ApplyChoice(MinLoopDispositionChoice choice)
    {
        int delta = choice == MinLoopDispositionChoice.SystemRecommendedA ? optionADelta : optionBDelta;
        SetTrustInternal(currentTrust + delta);
        return currentTrust;
    }

    public void SetTrust(int value)
    {
        SetTrustInternal(value);
    }

    private void SetTrustInternal(int value)
    {
        int clampedValue = Mathf.Clamp(value, minTrust, maxTrust);
        lastDelta = clampedValue - currentTrust;
        currentTrust = clampedValue;
        trustChanged.Invoke(currentTrust);
    }
}
