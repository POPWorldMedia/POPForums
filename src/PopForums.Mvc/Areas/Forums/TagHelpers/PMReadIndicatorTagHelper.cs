using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PopForums.Models;

namespace PopForums.Mvc.Areas.Forums.TagHelpers
{
	[HtmlTargetElement("pf-pmReadIndicator", Attributes = "privateMessage, imagePath")]
	public class PMReadIndicatorTagHelper : TagHelper
	{
		[HtmlAttributeName("class")]
		public string Class { get; set; }
		[HtmlAttributeName("privateMessage")]
		public PrivateMessage PrivateMessage { get; set; }
		[HtmlAttributeName("imagePath")]
		public string ImagePath { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var alt = Resources.NoNewPosts;
			var image = "NoNewIndicator.png";
			if (PrivateMessage.LastPostTime > PrivateMessage.LastViewDate)
			{
				alt = Resources.NewPosts;
				image = "NewIndicator.png";
			}

			output.TagName = "img";
			output.Attributes.Add("src", $"{ImagePath}{image}");
			output.Attributes.Add("alt", alt);
			if (!String.IsNullOrWhiteSpace(Class))
				output.Attributes.Add("class", Class);
		}
	}
}
