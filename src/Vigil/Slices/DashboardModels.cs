using Vigil.Domain.ClientKeys;
using Vigil.Domain.Events.EventActions;
using Vigil.Domain.Sessions;

namespace Vigil.Slices;

public record LayoutModel(string Title, bool ShowNav = true);

public record LoginPageModel(bool ShowError);

public record SessionsIndexModel(IReadOnlyList<Session> Sessions, bool ShowClosed);

public record ClientKeysIndexModel(IReadOnlyList<ClientKey> ClientKeys, string? Error);

public record EventActionsIndexModel(IReadOnlyList<EventAction> EventActions, string? Error, IReadOnlyList<string> KnownGroups);

public record EventActionDialogModel(string DialogId, string Heading, EventAction? Existing, string? Error, IReadOnlyList<string> KnownGroups);
