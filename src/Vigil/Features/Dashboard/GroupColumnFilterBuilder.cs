using Vigil.Slices;

namespace Vigil.Features.Dashboard;

internal static class GroupColumnFilterBuilder
{
    internal static ColumnFilterModel Build(
        IEnumerable<string?> allGroupValues,
        string popoverId,
        string listId,
        string queryParamName,
        string tableEndpoint,
        string tableBodyTarget,
        IReadOnlyCollection<string>? selected,
        string ungroupedValue)
    {
        var groupValues = allGroupValues.ToList();

        var distinctGroups = groupValues
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .Select(g => g!)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        var options = distinctGroups
            .Select(g => new ColumnFilterOption(g, g, IsChecked(selected, g)))
            .ToList();

        if (groupValues.Any(string.IsNullOrWhiteSpace))
            options.Add(new ColumnFilterOption(ungroupedValue, "Ungrouped", IsChecked(selected, ungroupedValue)));

        return new ColumnFilterModel(popoverId, listId, queryParamName, tableEndpoint, tableBodyTarget, options);
    }

    private static bool IsChecked(IReadOnlyCollection<string>? selected, string value) =>
        selected is null || selected.Count == 0 || selected.Contains(value);
}
