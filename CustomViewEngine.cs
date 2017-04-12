using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace PopForums.Web
{
    public class CustomViewEngine : RazorViewEngine
    {
        public CustomViewEngine()
        {
            var viewLocations = new[] { 
                "~/Areas/PopForums/Views/{0}/{1}.aspx",
                "~/Areas/PopForums/Views/{0}/{1}.ascx",
                "~/Areas/PopForums/Views/Shared/{0}.aspx",
                "~/Areas/PopForums/Views/Shared/{0}.ascx",
                "~/Areas/PopForums/Views/{1}/{0}.cshtml",
                "~/Areas/PopForums/Views/{0}/{1}.vbhtml",
                "~/Areas/PopForums/Views/Shared/{0}.cshtml",
                "~/Areas/PopForums/Views/Shared/{0}.vbhtml"
            };
            this.PartialViewLocationFormats = viewLocations;
            this.ViewLocationFormats = viewLocations;
        }
    }
}
