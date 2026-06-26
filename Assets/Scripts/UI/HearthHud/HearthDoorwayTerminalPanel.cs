using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthDoorwayTerminalPanel : MonoBehaviour
{
    [Serializable]
    public class TabBinding
    {
        public HearthDoorwayTab tab;
        public Button button;
        public GameObject contentRoot;
        public TMP_Text labelText;
        public Image accentImage;
    }

    [Header("Tabs")]
    [SerializeField] private TabBinding[] tabs;
    [SerializeField] private HearthDoorwayTab defaultTab = HearthDoorwayTab.ResidentSummary;
    [SerializeField] private bool bindButtonsOnAwake = true;

    [Header("Colors")]
    [SerializeField] private Color activeLabelColor = new Color(0.88f, 1f, 0.94f, 1f);
    [SerializeField] private Color inactiveLabelColor = new Color(0.52f, 0.64f, 0.62f, 0.86f);
    [SerializeField] private Color activeAccentColor = new Color(0.12f, 0.94f, 0.57f, 0.92f);
    [SerializeField] private Color inactiveAccentColor = new Color(0.16f, 0.24f, 0.24f, 0.36f);

    public HearthDoorwayTab CurrentTab { get; private set; }

    private bool buttonsBound;

    private void Awake()
    {
        if (bindButtonsOnAwake)
        {
            BindButtons();
        }

        SelectTab(defaultTab);
    }

    public void ConfigureTabs(TabBinding[] newTabs, HearthDoorwayTab newDefaultTab)
    {
        tabs = newTabs;
        defaultTab = newDefaultTab;
        buttonsBound = false;
        BindButtons();
        SelectTab(defaultTab);
    }

    public void BindButtons()
    {
        if (buttonsBound || tabs == null)
        {
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            TabBinding binding = tabs[i];
            if (binding == null || binding.button == null)
            {
                continue;
            }

            HearthDoorwayTab capturedTab = binding.tab;
            binding.button.onClick.AddListener(delegate { SelectTab(capturedTab); });
        }

        buttonsBound = true;
    }

    public void SelectTab(HearthDoorwayTab tab)
    {
        CurrentTab = tab;

        if (tabs == null)
        {
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            TabBinding binding = tabs[i];
            if (binding == null)
            {
                continue;
            }

            bool active = binding.tab == tab;

            if (binding.contentRoot != null)
            {
                binding.contentRoot.SetActive(active);
            }

            if (binding.labelText != null)
            {
                binding.labelText.color = active ? activeLabelColor : inactiveLabelColor;
            }

            if (binding.accentImage != null)
            {
                binding.accentImage.color = active ? activeAccentColor : inactiveAccentColor;
            }
        }
    }

    public void SelectResidentSummary()
    {
        SelectTab(HearthDoorwayTab.ResidentSummary);
    }

    public void SelectAcquisition()
    {
        SelectTab(HearthDoorwayTab.Acquisition);
    }

    public void SelectFamilyLog()
    {
        SelectTab(HearthDoorwayTab.FamilyLog);
    }

    public void SelectTrustTrend()
    {
        SelectTab(HearthDoorwayTab.TrustTrend);
    }

    public void SelectInspectionHistory()
    {
        SelectTab(HearthDoorwayTab.InspectionHistory);
    }
}
