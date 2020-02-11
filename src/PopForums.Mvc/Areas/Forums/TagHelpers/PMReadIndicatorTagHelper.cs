using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PopForums.Models;

namespace PopForums.Mvc.Areas.Forums.TagHelpers
{
	[HtmlTargetElement("pf-pmReadIndicator", Attributes = "privateMessage")]
	public class PMReadIndicatorTagHelper : TagHelper
	{
		[HtmlAttributeName("class")]
		public string Class { get; set; }
		[HtmlAttributeName("privateMessage")]
		public PrivateMessage PrivateMessage { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (PrivateMessage.LastPostTime > PrivateMessage.LastViewDate)
			{
				output.Attributes.Add("title", Resources.NewPosts);
				output.PostElement.AppendHtml("<span class=\"icon-file-text2 text-warning\"></span>");
			}
			else
			{
				output.Attributes.Add("title", Resources.NoNewPosts);
				output.PostElement.AppendHtml("<span class=\"icon-file-text2 text-muted\"></span>");
			}

			output.TagName = "div";
			if (!String.IsNullOrWhiteSpace(Class))
				output.Attributes.Add("class", $"topicIndicator {Class}");
			else
				output.Attributes.Add("class", "topicIndicator");
		}
	}
}
