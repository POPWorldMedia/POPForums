namespace PopForums.Mvc.Areas.Forums.TagHelpers;

[HtmlTargetElement("div", Attributes = ValidationForAttributeName + "," + ValidationErrorClassName)]
public class ValidationClassTagHelper : TagHelper
{
	private const string ValidationForAttributeName = "pf-validation-for";
	private const string ValidationErrorClassName = "pf-validationerror-class";

	[HtmlAttributeName(ValidationForAttributeName)]
	public ModelExpression For { get; set; }

	[HtmlAttributeName(ValidationErrorClassName)]
	public string ValidationErrorClass { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; }

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		ModelStateEntry entry;
		ViewContext.ViewData.ModelState.TryGetValue(For.Name, out entry);
		if (entry == null || !entry.Errors.Any())
			return;
		var tagBuilder = new TagBuilder("div");
		tagBuilder.AddCssClass(ValidationErrorClass);
		output.MergeAttributes(tagBuilder);
	}
}