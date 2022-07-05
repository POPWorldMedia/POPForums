namespace PopForums.Mvc.Areas.Forums.TagHelpers;

[HtmlTargetElement("pf-topicReadIndicator", Attributes = "topic, pagedTopicContainer")]
public class TopicReadIndicatorTagHelper : TagHelper
{
	[HtmlAttributeName("topic")]
	public Topic Topic { get; set; }
	[HtmlAttributeName("pagedTopicContainer")]
	public PagedTopicContainer PagedTopicContainer { get; set; }
	[HtmlAttributeName("class")]
	public string Class { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		var alt = Resources.NoNewPosts;
		if (PagedTopicContainer.ReadStatusLookup.ContainsKey(Topic.TopicID))
		{
			var status = PagedTopicContainer.ReadStatusLookup[Topic.TopicID];
			switch (status)
			{
				case ReadStatus.Open | ReadStatus.NewPosts | ReadStatus.Pinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text-fill text-warning\"></span><span class=\"icon icon-pin-angle-fill soloLeftBadge topicIndicatorBadge text-success\"></span>");
					alt = Resources.NewPostsPinned;
					break;
				case ReadStatus.Open | ReadStatus.NewPosts | ReadStatus.NotPinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text-fill text-warning\"></span>");
					alt = Resources.NewPosts;
					break;
				case ReadStatus.Open | ReadStatus.NoNewPosts | ReadStatus.Pinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text text-muted\"></span><span class=\"icon icon-pin-angle-fill soloLeftBadge topicIndicatorBadge text-success\"></span>");
					alt = Resources.Pinned;
					break;
				case ReadStatus.Open | ReadStatus.NoNewPosts | ReadStatus.NotPinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text text-muted\"></span>");
					alt = Resources.NoNewPosts;
					break;
				case ReadStatus.Closed | ReadStatus.NewPosts | ReadStatus.Pinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text-fill text-warning\"></span><span class=\"icon icon-lock-fill firstBadge topicIndicatorBadge text-danger\"></span><span class=\"icon icon-pin-angle-fill secondBadge topicIndicatorBadge text-success\"></span>");
					alt = Resources.NewPostsClosedPinned;
					break;
				case ReadStatus.Closed | ReadStatus.NewPosts | ReadStatus.NotPinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text-fill text-warning\"></span><span class=\"icon icon-lock-fill firstBadge topicIndicatorBadge text-danger\"></span>");
					alt = Resources.NewPostsClosed;
					break;
				case ReadStatus.Closed | ReadStatus.NoNewPosts | ReadStatus.Pinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text text-muted\"></span><span class=\"icon icon-lock-fill firstBadge topicIndicatorBadge text-danger\"></span><span class=\"icon icon-pin-angle-fill secondBadge topicIndicatorBadge text-success\"></span>");
					alt = Resources.ClosedPinned;
					break;
				case ReadStatus.Closed | ReadStatus.NoNewPosts | ReadStatus.NotPinned:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text text-muted\"></span><span class=\"icon icon-lock-fill firstBadge topicIndicatorBadge text-danger\"></span>");
					alt = Resources.Closed;
					break;
				default:
					output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text text-muted\"></span>");
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