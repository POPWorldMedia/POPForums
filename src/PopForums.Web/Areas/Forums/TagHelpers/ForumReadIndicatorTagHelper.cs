using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PopForums.Models;

namespace PopForums.Web.Areas.Forums.TagHelpers
{
	[HtmlTargetElement("pf-forumReadIndicator", Attributes = "forum, categorizedForumContainer, imagePath")]
    public class ForumReadIndicatorTagHelper : TagHelper
	{
		[HtmlAttributeName("forum")]
		public Forum Forum { get; set; }
		[HtmlAttributeName("categorizedForumContainer")]
		public CategorizedForumContainer CategorizedForumContainer { get; set; }
		[HtmlAttributeName("imagePath")]
		public string ImagePath { get; set; }
		[HtmlAttributeName("class")]
		public string Class { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var alt = Resources.NoNewPosts;
			var image = "NoNewIndicator.png";
			if (CategorizedForumContainer.ReadStatusLookup.ContainsKey(Forum.ForumID))
			{
				var status = CategorizedForumContainer.ReadStatusLookup[Forum.ForumID];
				switch (status)
				{
					case ReadStatus.Closed | ReadStatus.NoNewPosts:
						alt = Resources.Archived;
						image = "ClosedIndicator.png";
						break;
					case ReadStatus.Closed | ReadStatus.NewPosts:
						alt = Resources.ArchivedNewPosts;
						image = "NewClosedIndicator.png";
						break;
					case ReadStatus.NewPosts:
						alt = Resources.NewPosts;
						image = "NewIndicator.png";
						break;
					default:
						break;
				}
			}

			output.TagName = "img";
			output.Attributes.Add("src", $"{ImagePath}{image}");
			output.Attributes.Add("alt", alt);
			if (!String.IsNullOrWhiteSpace(Class))
				output.Attributes.Add("class", Class);
		}
    }
}
