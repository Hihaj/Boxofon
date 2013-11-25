namespace Boxofon.Web.Modules
{
    public class TestModule : WebsiteBaseModule
    {
        public TestModule()
            : base("/test")
        {
            Get["/"] = _ =>
            {
                return View["Test"];
            };
        }
    }
}