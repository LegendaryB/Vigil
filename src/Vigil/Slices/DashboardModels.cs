using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Sessions;

namespace Vigil.Slices;

public record LayoutModel(string Title, bool ShowNav = true);

public record LoginPageModel(bool ShowError);

public record SessionsIndexModel(IReadOnlyList<Session> Sessions, ColumnFilterModel StatusFilter);

public record ClientKeysIndexModel(IReadOnlyList<ClientKey> ClientKeys, string? Error, ColumnFilterModel GroupFilter);

public record EventActionsIndexModel(
    IReadOnlyList<EventAction> EventActions,
    string? Error,
    IReadOnlyList<string> KnownGroups,
    ColumnFilterModel TypeFilter,
    ColumnFilterModel EventFilter,
    ColumnFilterModel GroupFilter);

public record EventActionDialogModel(string DialogId, string Heading, EventAction? Existing, string? Error, IReadOnlyList<string> KnownGroups);

public record ColumnFilterOption(string Value, string Text, bool Checked);

public record ColumnFilterModel(
    string PopoverId,
    string ListId,
    string QueryParamName,
    string TableEndpoint,
    string TableBodyTarget,
    IReadOnlyList<ColumnFilterOption> Options);
