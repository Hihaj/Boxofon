using System.Text;
using System.Xml;
using System.Xml.Linq;
using Nancy;
using Twilio.TwiML;

namespace Boxofon.Web.Twilio
{
    public static class TwilioResponseExtensions
    {
        public static TwilioResponse SayInSwedish(this TwilioResponse response, string text)
        {
            return response.Say(text, new { voice = "alice", language = "sv-SE" });
        }

        public static Nancy.Response ToNancyResponse(this TwilioResponse response)
        {
            var data = response == null ?
                           new XDocument(new XElement("Response")) :
                           response.ToXDocument();
            return new Nancy.Response
            {
                ContentType = "application/xml; charset=utf-8",
                StatusCode = HttpStatusCode.OK,
                Contents = stream =>
                {
                    using (var writer = new XmlTextWriter(stream, new UTF8Encoding(false)))
                    {
                        data.Save(writer);
                    }
                }
            };
        }
    }
}