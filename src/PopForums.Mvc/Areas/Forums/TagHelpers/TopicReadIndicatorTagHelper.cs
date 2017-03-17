using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PopForums.Models;

namespace PopForums.Mvc.Areas.Forums.TagHelpers
{
	[HtmlTargetElement("pf-topicReadIndicator", Attributes = "topic, pagedTopicContainer, imagePath")]
	public class TopicReadIndicatorTagHelper : TagHelper
	{
		[HtmlAttributeName("topic")]
		public Topic Topic { get; set; }
		[HtmlAttributeName("pagedTopicContainer")]
		public PagedTopicContainer PagedTopicContainer { get; set; }
		[HtmlAttributeName("imagePath")]
		public string ImagePath { get; set; }
		[HtmlAttributeName("class")]
		public string Class { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var alt = Resources.NoNewPosts;
			var image = "NoNewIndicator.png";
			if (PagedTopicContainer.ReadStatusLookup.ContainsKey(Topic.TopicID))
			{
				var status = PagedTopicContainer.ReadStatusLookup[Topic.TopicID];
				switch (status)
				{
					case ReadStatus.Open | ReadStatus.NewPosts | ReadStatus.Pinned:
						image = "NewPinnedIndicator.png";
						alt = Resources.NewPostsPinned;
						break;
					case ReadStatus.Open | ReadStatus.NewPosts | ReadStatus.NotPinned:
						image = "NewIndicator.png";
						alt = Resources.NewPosts;
						break;
					case ReadStatus.Open | ReadStatus.NoNewPosts | ReadStatus.Pinned:
						image = "PinnedIndicator.png";
						alt = Resources.Pinned;
						break;
					case ReadStatus.Open | ReadStatus.NoNewPosts | ReadStatus.NotPinned:
						image = "NoNewIndicator.png";
						alt = Resources.NoNewPosts;
						break;
					case ReadStatus.Closed | ReadStatus.NewPosts | ReadStatus.Pinned:
						image = "NewClosedPinnedIndicator.png";
						alt = Resources.NewPostsClosedPinned;
						break;
					case ReadStatus.Closed | ReadStatus.NewPosts | ReadStatus.NotPinned:
						image = "NewClosedIndicator.png";
						alt = Resources.NewPostsClosed;
						break;
					case ReadStatus.Closed | ReadStatus.NoNewPosts | ReadStatus.Pinned:
						image = "ClosedPinnedIndicator.png";
						alt = Resources.ClosedPinned;
						break;
					case ReadStatus.Closed | ReadStatus.NoNewPosts | ReadStatus.NotPinned:
						image = "ClosedIndicator.png";
						alt = Resources.Closed;
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