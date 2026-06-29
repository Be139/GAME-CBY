using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthDispositionHistoryView : MonoBehaviour
{
    [Header("Records")]
    [SerializeField] private List<HearthDispositionRecord> records = new List<HearthDispositionRecord>();

    [Header("Optional Row Bindings")]
    [SerializeField] private TMP_Text[] timestampTexts;
    [SerializeField] private TMP_Text[] unitTexts;
    [SerializeField] private TMP_Text[] actionTexts;
    [SerializeField] private TMP_Text[] statusTexts;
    [SerializeField] private TMP_Text[] trustDeltaTexts;

    [Header("Optional Footer Bindings")]
    [SerializeField] private TMP_Text shiftTrustDeltaText;
    [SerializeField] private TMP_Text currentTrustText;

    [Header("Footer Values")]
    [SerializeField] private int currentTrustScore;

    public int RecordCount
    {
        get { return records != null ? records.Count : 0; }
    }

    public IReadOnlyList<HearthDispositionRecord> Records
    {
        get { return records; }
    }

    private void Awake()
    {
        Refresh();
    }

    public void SetRecords(IEnumerable<HearthDispositionRecord> newRecords)
    {
        records.Clear();
        if (newRecords != null)
        {
            records.AddRange(newRecords);
        }

        Refresh();
    }

    public void AddRecord(HearthDispositionRecord record)
    {
        if (record == null)
        {
            return;
        }

        if (records.Count >= 3)
        {
            records.RemoveAt(0);
        }

        records.Add(record);
        currentTrustScore += record.trustDelta;
        Refresh();
    }

    public void ClearRecords()
    {
        records.Clear();
        currentTrustScore = 0;
        Refresh();
    }

    public void SetCurrentTrustScore(int value)
    {
        currentTrustScore = value;
        Refresh();
    }

    public HearthFirstPersonHudPageId GetPageForCurrentRecordCount()
    {
        int count = Mathf.Clamp(RecordCount, 0, 3);
        return (HearthFirstPersonHudPageId)((int)HearthFirstPersonHudPageId.Slide18HistoryEmpty + count);
    }

    public void Refresh()
    {
        int rowCount = Mathf.Max(
            MaxLength(timestampTexts),
            Mathf.Max(MaxLength(unitTexts), Mathf.Max(MaxLength(actionTexts), Mathf.Max(MaxLength(statusTexts), MaxLength(trustDeltaTexts)))));

        for (int i = 0; i < rowCount; i++)
        {
            HearthDispositionRecord record = i < records.Count ? records[i] : null;
            SetOptionalText(timestampTexts, i, record != null ? record.timestamp : string.Empty);
            SetOptionalText(unitTexts, i, record != null ? record.unitId : string.Empty);
            SetOptionalText(actionTexts, i, record != null ? record.actionLabel : string.Empty);
            SetOptionalText(statusTexts, i, record != null ? record.statusLabel : string.Empty);
            SetOptionalText(trustDeltaTexts, i, record != null ? FormatDelta(record.trustDelta) + " TRUST" : string.Empty);
        }

        if (shiftTrustDeltaText != null)
        {
            shiftTrustDeltaText.text = FormatDelta(currentTrustScore);
        }

        if (currentTrustText != null)
        {
            currentTrustText.text = currentTrustScore.ToString();
        }
    }

    private static int MaxLength(TMP_Text[] values)
    {
        return values != null ? values.Length : 0;
    }

    private static void SetOptionalText(TMP_Text[] values, int index, string text)
    {
        if (values != null && index >= 0 && index < values.Length && values[index] != null)
        {
            values[index].text = text;
        }
    }

    private static string FormatDelta(int delta)
    {
        return (delta >= 0 ? "+" : string.Empty) + delta.ToString();
    }
}
