using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PopForums.Models;

namespace PopForums.Mvc.Areas.Forums.TagHelpers
{
	[HtmlTargetElement("pf-forumReadIndicator", Attributes = "forum, categorizedForumContainer")]
    public class ForumReadIndicatorTagHelper : TagHelper
	{
		[HtmlAttributeName("forum")]
		public Forum Forum { get; set; }
		[HtmlAttributeName("categorizedForumContainer")]
		public CategorizedForumContainer CategorizedForumContainer { get; set; }
		[HtmlAttributeName("class")]
		public string Class { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var alt = Resources.NoNewPosts;
			if (CategorizedForumContainer.ReadStatusLookup.ContainsKey(Forum.ForumID))
			{
				var status = CategorizedForumContainer.ReadStatusLookup[Forum.ForumID];
				switch (status)
				{
					case ReadStatus.Closed | ReadStatus.NoNewPosts:
						alt = Resources.Archived;
						output.PostElement.AppendHtml("<span class=\"icon-file-text2 text-muted\"></span><span class=\"icon-lock firstBadge topicIndicatorBadge text-danger\"></span>");
						break;
					case ReadStatus.Closed | ReadStatus.NewPosts:
						alt = Resources.ArchivedNewPosts;
						output.PostElement.AppendHtml("<span class=\"icon-file-text2 text-warning\"></span><span class=\"icon-lock firstBadge topicIndicatorBadge text-danger\"></span>");
						break;
					case ReadStatus.NewPosts:
						alt = Resources.NewPosts;
						output.PostElement.AppendHtml("<span class=\"icon-file-text2 text-warning\"></span>");
						break;
					default:
						output.PostElement.AppendHtml("<span class=\"icon-file-text2 text-muted\"></span>");
						break;
				}
			}

			output.TagName = "div";
			output.Attributes.Add("title", alt);
			if (!String.IsNullOrWhiteSpace(Class))
				output.Attributes.Add("class", $"topicIndicator {Class}");
			else
				output.Attributes.Add("class", "topicIndicator");
		}
    }
}
