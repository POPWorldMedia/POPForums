namespace PopForums.Mvc.Areas.Forums.TagHelpers;

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
			output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text-fill text-warning\"></span>");
		}
		else
		{
			output.Attributes.Add("title", Resources.NoNewPosts);
			output.PostElement.AppendHtml("<span class=\"icon icon-file-earmark-text text-muted\"></span>");
		}

		output.TagName = "div";
		if (!String.IsNullOrWhiteSpace(Class))
			output.Attributes.Add("class", $"topicIndicator {Class}");
		else
			output.Attributes.Add("class", "topicIndicator");
	}
}