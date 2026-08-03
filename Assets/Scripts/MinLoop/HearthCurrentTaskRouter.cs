using UnityEngine;

public enum HearthCurrentTaskId
{
    None,
    ListenToFieldUnit,
    GoToAssignmentTerminal,
    ReviewAssignments,
    GoToElevator,
    RideToFloor17,
    GoToResidentTerminal,
    ReviewResidentProfile,
    WaitForCompanionLink,
    ReviewHouseholdEvent,
    ReturnToResidentTerminal,
    ReviewHouseholdAnalysis,
    SelectDisposition,
    ReturnHome,
    UseHomeTerminal,
    ReviewLilyMessage,
    InspectPhotoArchive,
    GoToLilyRoom,
    TalkToLily,
    MakeFinalResponse,
    ApproachHomeUnit,
    ConfirmShutdown
}

/// <summary>
/// Single source of truth for objective-only Current Task copy. Interaction
/// keys belong to the lower interaction prompt and never to this HUD region.
/// </summary>
public static class HearthCurrentTaskRouter
{
    private const string CatalogResourcePath =
        "HEARTH/HearthTaskTextCatalog";
    private static HearthTaskTextCatalog cachedCatalog;
    private static bool catalogLoadAttempted;

    public static void ApplyHuman(
        HearthFirstPersonHudController hud,
        HearthCurrentTaskId taskId,
        string residentId = null)
    {
        if (hud != null)
        {
            hud.SetCurrentTask(Resolve(taskId, residentId));
        }
    }

    public static void ApplyCompanion(
        HearthCompanionHudController hud,
        HearthCurrentTaskId taskId,
        string residentId = null)
    {
        if (hud != null)
        {
            hud.SetCurrentTask(Resolve(taskId, residentId));
        }
    }

    public static string Resolve(
        HearthCurrentTaskId taskId,
        string residentId = null)
    {
        string resident = FormatResidentId(residentId);
        HearthTaskTextCatalog catalog = GetCatalog();
        string catalogText;
        if (catalog != null &&
            catalog.TryResolveTask(taskId, resident, out catalogText))
        {
            return NormalizeTaskText(catalogText);
        }

        switch (taskId)
        {
            case HearthCurrentTaskId.ListenToFieldUnit:
                return "LISTEN TO FIELD UNIT";
            case HearthCurrentTaskId.GoToAssignmentTerminal:
                return "GO TO THE ASSIGNMENT TERMINAL";
            case HearthCurrentTaskId.ReviewAssignments:
                return "REVIEW TONIGHT'S ASSIGNMENTS";
            case HearthCurrentTaskId.GoToElevator:
                return "GO TO THE ELEVATOR";
            case HearthCurrentTaskId.RideToFloor17:
                return "RIDE TO FLOOR 17";
            case HearthCurrentTaskId.GoToResidentTerminal:
                return "GO TO TERMINAL " + resident;
            case HearthCurrentTaskId.ReviewResidentProfile:
                return "REVIEW HOUSEHOLD PROFILE";
            case HearthCurrentTaskId.WaitForCompanionLink:
                return "WAIT FOR COMPANION LINK";
            case HearthCurrentTaskId.ReviewHouseholdEvent:
                return "REVIEW RECORDED HOUSEHOLD EVENT";
            case HearthCurrentTaskId.ReturnToResidentTerminal:
                return "RETURN TO TERMINAL " + resident;
            case HearthCurrentTaskId.ReviewHouseholdAnalysis:
                return "REVIEW HOUSEHOLD ANALYSIS";
            case HearthCurrentTaskId.SelectDisposition:
                return "SELECT A DISPOSITION";
            case HearthCurrentTaskId.ReturnHome:
                return "RETURN HOME";
            case HearthCurrentTaskId.UseHomeTerminal:
                return "USE THE HOME TERMINAL";
            case HearthCurrentTaskId.ReviewLilyMessage:
                return "REVIEW LILY'S MESSAGE";
            case HearthCurrentTaskId.InspectPhotoArchive:
                return "INSPECT THE PHOTO ARCHIVE";
            case HearthCurrentTaskId.GoToLilyRoom:
                return "GO TO LILY'S ROOM";
            case HearthCurrentTaskId.TalkToLily:
                return "TALK TO LILY";
            case HearthCurrentTaskId.MakeFinalResponse:
                return "MAKE YOUR FINAL RESPONSE";
            case HearthCurrentTaskId.ApproachHomeUnit:
                return "APPROACH THE HOME UNIT";
            case HearthCurrentTaskId.ConfirmShutdown:
                return "CONFIRM COMPANION SHUTDOWN";
            default:
                return string.Empty;
        }
    }

    public static string ResolveMinLoopTask(
        MinLoopStage stage,
        string residentId)
    {
        switch (stage)
        {
            case MinLoopStage.AccessCard:
            case MinLoopStage.ResidentSummary:
                return Resolve(HearthCurrentTaskId.ReviewResidentProfile, residentId);
            case MinLoopStage.SwitchingToCompanion:
                return Resolve(HearthCurrentTaskId.WaitForCompanionLink);
            case MinLoopStage.CompanionReplay:
            case MinLoopStage.WaitingForComfort:
            case MinLoopStage.Comforting:
            case MinLoopStage.MorningReview:
            case MinLoopStage.EnteringResidentUnit:
            case MinLoopStage.ResidentUnitDialogue:
            case MinLoopStage.ResidentUnitInspection:
            case MinLoopStage.ResidentPostReplay:
                return Resolve(HearthCurrentTaskId.ReviewHouseholdEvent);
            case MinLoopStage.ReturningToTerminal:
                return Resolve(HearthCurrentTaskId.ReturnToResidentTerminal, residentId);
            case MinLoopStage.DispositionBriefing:
                return Resolve(HearthCurrentTaskId.ReviewHouseholdAnalysis);
            case MinLoopStage.DispositionChoice:
            case MinLoopStage.DispositionResult:
                return Resolve(HearthCurrentTaskId.SelectDisposition);
            case MinLoopStage.Complete:
                return ResolveHouseholdCompletionTask(residentId);
            default:
                return Resolve(HearthCurrentTaskId.GoToResidentTerminal, residentId);
        }
    }

    public static string ResolveHouseholdCompletionTask(string residentId)
    {
        switch (NormalizeResidentId(residentId))
        {
            case "17F01":
                return Resolve(HearthCurrentTaskId.GoToResidentTerminal, "17F02");
            case "17F02":
                return Resolve(HearthCurrentTaskId.GoToResidentTerminal, "17F03");
            case "17F03":
                return Resolve(HearthCurrentTaskId.ReturnHome);
            default:
                return Resolve(HearthCurrentTaskId.ReturnHome);
        }
    }

    public static string ResolveCompanionSceneTask(
        string sceneId,
        string fallback)
    {
        HearthTaskTextCatalog catalog = GetCatalog();
        string catalogText;
        if (catalog != null &&
            catalog.TryResolveCompanionScene(sceneId, out catalogText))
        {
            return NormalizeTaskText(catalogText);
        }

        switch ((sceneId ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "17F01_01":
                return "APPROACH THE BEDSIDE AND COMFORT NOAH";
            case "17F01_02":
                return "OBSERVE THE HALLWAY AND FOLLOW THE PARENTS";
            case "17F01_03":
                return "LISTEN TO THE PARENTS IN THE LIVING ROOM";
            case "17F02_01":
                return "LISTEN TO CLAIRE";
            case "17F02_02":
                return "OFFER REASSURANCE";
            case "17F02_03":
                return "FOLLOW CLAIRE TO THE DINING ROOM";
            case "17F02_04":
                return "OBSERVE THE HOUSEHOLD QUERY";
            case "17F02_05":
                return "INITIATE SOFT GUIDANCE";
            case "17F02_06":
                return "LISTEN TO THE RECORDED ARGUMENT";
            case "17F03_01":
                return "OBSERVE THE FAMILY CONFLICT";
            case "17F03_02":
                return "FACE THE DAUGHTER AND RELAY THE MESSAGE";
            case "17F03_03":
                return "FACE THE MOTHER AND RELAY THE MESSAGE";
            case "17F03_04":
                return "OBSERVE THE SERVICE SUBJECT";
            case "17F03_05":
                return "CONFIRM MAINTENANCE SHUTDOWN";
            default:
                return NormalizeTaskText(string.IsNullOrWhiteSpace(fallback)
                    ? Resolve(HearthCurrentTaskId.ReviewHouseholdEvent)
                    : fallback);
        }
    }

    public static string NormalizeTaskText(string value)
    {
        string task = string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();

        string[] retiredInputPhrases =
        {
            "HOLD 1 TO ",
            "HOLD E TO ",
            "PRESS 1 TO ",
            "PRESS E TO "
        };

        for (int i = 0; i < retiredInputPhrases.Length; i++)
        {
            task = task.Replace(retiredInputPhrases[i], string.Empty);
        }

        return task.Trim();
    }

    public static string FormatResidentId(string value)
    {
        string normalized = NormalizeResidentId(value);
        return normalized.Length == 5
            ? normalized.Substring(0, 3) + "-" + normalized.Substring(3)
            : "17F-01";
    }

    private static string NormalizeResidentId(string value)
    {
        string normalized = string.IsNullOrWhiteSpace(value)
            ? "17F01"
            : value.Trim().ToUpperInvariant()
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .Replace(" ", string.Empty);

        if (normalized.Contains("17F04") || normalized.Contains("ROOM4")) return "17F04";
        if (normalized.Contains("17F03") || normalized.Contains("ROOM3")) return "17F03";
        if (normalized.Contains("17F02") || normalized.Contains("ROOM2")) return "17F02";
        return "17F01";
    }

    private static HearthTaskTextCatalog GetCatalog()
    {
        if (!catalogLoadAttempted)
        {
            catalogLoadAttempted = true;
            cachedCatalog = Resources.Load<HearthTaskTextCatalog>(
                CatalogResourcePath);
        }

        return cachedCatalog;
    }
}
