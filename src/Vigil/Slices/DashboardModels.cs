using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Sessions;

namespace Vigil.Slices;

public enum DashboardPage
{
    None,
    Sessions,
    ClientKeys,
    EventActions,
    DispatchLog
}

public record LayoutModel(string Title, bool ShowNav = true, DashboardPage ActivePage = DashboardPage.None);

public record LoginPageModel(bool ShowError);

public record SessionStats(int OpenCount, int ClosedTodayCount, int OverdueCount);

public record SessionsIndexModel(IReadOnlyList<Session> Sessions, ColumnFilterModel StatusFilter, SessionStats Stats);

public record DispatchLogIndexModel(
    IReadOnlyList<DispatchLogEntry> Entries,
    ColumnFilterModel EventFilter,
    ColumnFilterModel TypeFilter,
    ColumnFilterModel OutcomeFilter);

public record ClientKeysIndexModel(
    IReadOnlyList<ClientKey> ClientKeys,
    ColumnFilterModel GroupFilter);

public record EventActionsIndexModel(
    IReadOnlyList<EventAction> EventActions,
    ColumnFilterModel TypeFilter,
    ColumnFilterModel EventFilter,
    ColumnFilterModel GroupFilter);

public record ColumnFilterOption(string Value, string Text, bool Checked);

public record ColumnFilterModel(
    string PopoverId,
    string ListId,
    string QueryParamName,
    string TableEndpoint,
    string TableBodyTarget,
    IReadOnlyList<ColumnFilterOption> Options);
