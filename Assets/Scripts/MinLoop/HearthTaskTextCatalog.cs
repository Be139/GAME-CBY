using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "HearthTaskTextCatalog",
    menuName = "Hearth/UI/Task Text Catalog")]
public sealed class HearthTaskTextCatalog : ScriptableObject
{
    [Serializable]
    public sealed class TaskEntry
    {
        public HearthCurrentTaskId taskId;
        [TextArea] public string text;
    }

    [Serializable]
    public sealed class CompanionSceneEntry
    {
        public string sceneId;
        [TextArea] public string text;
    }

    [SerializeField] private TaskEntry[] tasks = new TaskEntry[0];
    [SerializeField] private CompanionSceneEntry[] companionScenes =
        new CompanionSceneEntry[0];

    public TaskEntry[] Tasks { get { return tasks; } }
    public CompanionSceneEntry[] CompanionScenes { get { return companionScenes; } }

    public bool TryResolveTask(
        HearthCurrentTaskId taskId,
        string formattedResidentId,
        out string value)
    {
        if (tasks != null)
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                TaskEntry entry = tasks[i];
                if (entry != null && entry.taskId == taskId &&
                    !string.IsNullOrWhiteSpace(entry.text))
                {
                    value = entry.text.Replace(
                        "{RESIDENT}",
                        formattedResidentId ?? string.Empty);
                    return true;
                }
            }
        }

        value = string.Empty;
        return false;
    }

    public bool TryResolveCompanionScene(string sceneId, out string value)
    {
        string normalized = (sceneId ?? string.Empty).Trim();
        if (companionScenes != null)
        {
            for (int i = 0; i < companionScenes.Length; i++)
            {
                CompanionSceneEntry entry = companionScenes[i];
                if (entry != null &&
                    string.Equals(
                        entry.sceneId,
                        normalized,
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(entry.text))
                {
                    value = entry.text;
                    return true;
                }
            }
        }

        value = string.Empty;
        return false;
    }

#if UNITY_EDITOR
    public void Configure(
        TaskEntry[] newTasks,
        CompanionSceneEntry[] newCompanionScenes)
    {
        tasks = newTasks ?? new TaskEntry[0];
        companionScenes = newCompanionScenes ?? new CompanionSceneEntry[0];
    }
#endif
}
