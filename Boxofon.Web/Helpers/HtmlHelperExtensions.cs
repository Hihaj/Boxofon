using System;
using System.Linq;
using System.Text;
using Boxofon.Web.Infrastructure;
using Nancy.ViewEngines.Razor;

namespace Boxofon.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlString AlertMessages<TModel>(this HtmlHelpers<TModel> htmlHelper)
        {
            const string message = @"<div class=""alert alert-{0}"">{1}</div>";
            var alertsDynamicValue = htmlHelper.RenderContext.Context.ViewBag.Alerts;
            var alerts = (AlertMessageStore)(alertsDynamicValue.HasValue ? alertsDynamicValue.Value : null);

            if (alerts == null || !alerts.Messages.Any())
            {
                return new NonEncodedHtmlString(String.Empty);
            }

            var builder = new StringBuilder();

            foreach (var messageDetail in alerts.Messages)
            {
                builder.AppendFormat(message, messageDetail.Key, messageDetail.Value);
            }

            return new NonEncodedHtmlString(builder.ToString());
        }
    }
}