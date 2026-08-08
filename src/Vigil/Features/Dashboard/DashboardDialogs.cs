using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Vigil.Features.Dashboard;

internal static class DashboardDialogs
{
    internal static string DetailsDialogId(Guid id) => $"details-dialog-{id}";

    internal static TagBuilder BuildHeader(string title)
    {
        var heading = new TagBuilder("h3");
        heading.InnerHtml.Append(title);

        var close = new TagBuilder("button")
        {
            Attributes =
            {
                ["type"] = "button",
                ["class"] = DashboardStyles.IconButton,
                ["title"] = "Close",
                ["aria-label"] = "Close",
                ["onclick"] = "this.closest('dialog').close()"
            }
        };
        close.InnerHtml.AppendHtml(new HtmlString(DashboardIcons.Close));

        var header = new TagBuilder("div")
        {
            Attributes = { ["class"] = DashboardStyles.DialogHeader }
        };
        header.InnerHtml.AppendHtml(heading);
        header.InnerHtml.AppendHtml(close);

        return header;
    }

    internal static TagBuilder BuildReadOnlyField(string label, string value)
    {
        var input = new TagBuilder("input")
        {
            TagRenderMode = TagRenderMode.SelfClosing,
            Attributes =
            {
                ["type"] = "text",
                ["value"] = value,
                ["readonly"] = "readonly"
            }
        };

        return FieldRow(label, input);
    }

    internal static TagBuilder BuildReadOnlyTextArea(string label, string value)
    {
        var textarea = new TagBuilder("textarea")
        {
            Attributes =
            {
                ["readonly"] = "readonly",
                ["rows"] = "4"
            }
        };

        if (!string.IsNullOrEmpty(value))
            textarea.InnerHtml.Append(value);

        return FieldRow(label, textarea);
    }

    internal static TagBuilder FieldRow(string labelText, TagBuilder field)
    {
        var label = new TagBuilder("label");
        label.InnerHtml.Append(labelText);
        label.InnerHtml.AppendHtml(field);

        var wrapper = new TagBuilder("div")
        {
            Attributes = { ["class"] = DashboardStyles.FormField }
        };
        wrapper.InnerHtml.AppendHtml(label);
        return wrapper;
    }

    internal static TagBuilder BuildError(string error)
    {
        var errorParagraph = new TagBuilder("p")
        {
            Attributes = { ["class"] = DashboardStyles.FormError }
        };
        var strong = new TagBuilder("strong");
        strong.InnerHtml.Append(error);
        errorParagraph.InnerHtml.AppendHtml(strong);
        return errorParagraph;
    }
}
