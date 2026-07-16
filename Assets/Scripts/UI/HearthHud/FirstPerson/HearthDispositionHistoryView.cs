using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthDispositionHistoryView : MonoBehaviour
{
    [Serializable]
    public class RowBinding
    {
        public int recordIndex;
        public GameObject rowRoot;
        public TMP_Text timestampText;
        public TMP_Text unitText;
        public TMP_Text actionText;
        public TMP_Text statusText;
        public TMP_Text trustDeltaText;
    }

    [Header("Records")]
    [SerializeField] private List<HearthDispositionRecord> records = new List<HearthDispositionRecord>();

    [Header("Dynamic Row Bindings")]
    [SerializeField] private RowBinding[] rowBindings;

    [Header("Optional Row Bindings")]
    [SerializeField] private TMP_Text[] timestampTexts;
    [SerializeField] private TMP_Text[] unitTexts;
    [SerializeField] private TMP_Text[] actionTexts;
    [SerializeField] private TMP_Text[] statusTexts;
    [SerializeField] private TMP_Text[] trustDeltaTexts;

    [Header("Optional Footer Bindings")]
    [SerializeField] private TMP_Text shiftTrustDeltaText;
    [SerializeField] private TMP_Text currentTrustText;
    [SerializeField] private TMP_Text[] shiftTrustDeltaTexts;
    [SerializeField] private TMP_Text[] currentTrustTexts;

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
        AddRecord(record, currentTrustScore + (record != null ? record.trustDelta : 0));
    }

    public void AddRecord(HearthDispositionRecord record, int currentTrustAfter)
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
        currentTrustScore = currentTrustAfter;
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
        RefreshDynamicRows();
        RefreshLegacyRows();
        RefreshFooters();
    }

    private void RefreshDynamicRows()
    {
        if (rowBindings == null)
        {
            return;
        }

        for (int i = 0; i < rowBindings.Length; i++)
        {
            RowBinding binding = rowBindings[i];
            if (binding == null)
            {
                continue;
            }

            int recordIndex = Mathf.Max(0, binding.recordIndex);
            HearthDispositionRecord record = recordIndex < records.Count ? records[recordIndex] : null;
            bool hasRecord = record != null;

            if (binding.rowRoot != null)
            {
                binding.rowRoot.SetActive(hasRecord);
            }

            SetText(binding.timestampText, hasRecord ? record.timestamp : string.Empty);
            SetText(binding.unitText, hasRecord ? record.unitId : string.Empty);
            SetText(binding.actionText, hasRecord ? record.actionLabel : string.Empty);
            SetText(binding.statusText, hasRecord ? record.statusLabel : string.Empty);
            SetText(binding.trustDeltaText, hasRecord ? FormatDelta(record.trustDelta) + " TRUST" : string.Empty);
        }
    }

    private void RefreshLegacyRows()
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
    }

    private void RefreshFooters()
    {
        int shiftTrustDelta = CalculateShiftTrustDelta();

        if (shiftTrustDeltaText != null)
        {
            shiftTrustDeltaText.text = FormatDelta(shiftTrustDelta);
        }

        if (currentTrustText != null)
        {
            currentTrustText.text = currentTrustScore.ToString();
        }

        SetAllTexts(shiftTrustDeltaTexts, FormatDelta(shiftTrustDelta));
        SetAllTexts(currentTrustTexts, currentTrustScore.ToString());
    }

    private int CalculateShiftTrustDelta()
    {
        int total = 0;
        if (records == null)
        {
            return total;
        }

        for (int i = 0; i < records.Count; i++)
        {
            if (records[i] != null)
            {
                total += records[i].trustDelta;
            }
        }

        return total;
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

    private static void SetAllTexts(TMP_Text[] values, string text)
    {
        if (values == null)
        {
            return;
        }

        for (int i = 0; i < values.Length; i++)
        {
            SetText(values[i], text);
        }
    }

    private static void SetText(TMP_Text value, string text)
    {
        if (value != null)
        {
            value.text = text;
        }
    }

    private static string FormatDelta(int delta)
    {
        return (delta >= 0 ? "+" : string.Empty) + delta.ToString();
    }
}
