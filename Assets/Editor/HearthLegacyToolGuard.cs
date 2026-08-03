#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Shared warning gate for authoring utilities that predate the canonical V2 UI workflow.
/// These tools remain available during migration, but must never run by accident.
/// </summary>
public static class HearthLegacyToolGuard
{
    public const string MenuRoot = "Tools/Hearth/Legacy Unsafe/";

    public static bool Confirm(string operation, string affectedArea)
    {
        return EditorUtility.DisplayDialog(
            "HEARTH Legacy / Unsafe",
            operation + " belongs to the legacy migration path.\n\n" +
            "It may overwrite authored Prefab values or scene bindings in: " +
            affectedArea + ".\n\n" +
            "Use Tools > Hearth > Production UI for normal adjustments. " +
            "Continue only when restoring an old project state and after reviewing the current diff.",
            "Continue (Unsafe)",
            "Cancel");
    }
}
#endif
