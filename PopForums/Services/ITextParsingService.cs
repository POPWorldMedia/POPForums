namespace PopForums.Services
{
	public interface ITextParsingService
	{
		/// <summary>
		/// Converts forum code from the browser to HTML for storage. This method wraps <see cref="TextParsingService.CleanForumCode"/> and <see cref="TextParsingService.ForumCodeToHtml"/>.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		string ForumCodeToHtml(string text);

		/// <summary>
		/// Converts client HTML from the browser to HTML for storage. This method wraps <see cref="TextParsingService.ClientHtmlToForumCode"/> and <see cref="TextParsingService.ForumCodeToHtml"/>.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		string ClientHtmlToHtml(string text);

		string HtmlToClientHtml(string text);
		string Censor(string text);

		/// <summary>
		/// Converts client HTML to forum code. Important: This method does NOT attempt to create valid HTML, as it assumes that the forum code 
		/// will be cleaned. This method should generally not be called directly except for testing.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		string ClientHtmlToForumCode(string text);

		/// <summary>
		/// Cleans forum code by making sure tags are properly closed, escapes HTML, removes images if settings require it, removes extra line breaks 
		/// and marks up URL's and e-mail addresses as links. This method should generally not be called directly except for testing.
		/// </summary>
		/// <param name="text">Text with forum code to clean.</param>
		/// <returns>Cleaned forum code text.</returns>
		string CleanForumCode(string text);

		/// <summary>
		/// Converts forum code to HTML for storage. Important: This method does NOT attempt to create valid HTML, as it assumes that the forum code is 
		/// already well-formed. This method should generally not be called directly except for testing.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		string CleanForumCodeToHtml(string text);

		string EscapeHtmlAndCensor(string text);
		string HtmlToForumCode(string text);
	}
}