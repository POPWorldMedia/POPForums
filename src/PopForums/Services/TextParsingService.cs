using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PopForums.Configuration;
using PopForums.Extensions;

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
		/// <summary>
		/// Removes all forum code markup from the text. Useful for scrubbing text to be saved in search repositories.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		string RemoveForumCode(string text);
	}

	public class TextParsingService : ITextParsingService
	{
		public TextParsingService(ISettingsManager settingsManager)
		{
			_settingsManager = settingsManager;
		}

		private readonly ISettingsManager _settingsManager;

		public static string[] AllowedCloseableTags = { "b", "i", "code", "pre", "ul", "ol", "li", "url", "quote", "img" };
		private static readonly Regex TagPattern = new Regex(@"\[[\w""\?=&/;\+%\*\:~,\!\.\-\$\|@#]+\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex TagID = new Regex(@"\[/?(\w+)\=*.*?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ProtocolPattern = new Regex(@"(?<![\]""\>=])(((news|(ht|f)tp(s?))\://)[\w\-\*]+(\.[\w\-/~\*]+)*/?)([\w\?=&/;\+%\*\:~,\.\-\$\|@#])*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex WwwPattern = new Regex(@"(?<!(\]|""|//))(?<=\s|^)(w{3}(\.[\w\-/~\*]+)*/?)([\?\w=&;\+%\*\:~,\-\$\|@#])*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex EmailPattern = new Regex(@"(?<=\s|\])(?<!(mailto:|""\]))([\w\.\-_']+)@(([\w\-]+\.)+[\w\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex YouTubePattern = new Regex(@"(?<![\]""\>=])(((http(s?))\://)[w*\.]*(youtu\.be|youtube\.com+))([\w\?=&/;\+%\*\:~,\.\-\$\|@#])*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Converts forum code from the browser to HTML for storage. This method wraps <see cref="CleanForumCode(string)"/> and <see cref="ForumCodeToHtml(string)"/>, and censors the text.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		public string ForumCodeToHtml(string text)
		{
			text = Censor(text);
			text = CleanForumCode(text);
			text = CleanForumCodeToHtml(text);
			if (text == "<p></p>")
				text = String.Empty;
			return text;
		}

		public string EscapeHtmlAndCensor(string text)
		{
			text = Censor(text);
			return EscapeHtmlTags(text);
		}

		/// <summary>
		/// Converts client HTML from the browser to HTML for storage. This method wraps <see cref="ClientHtmlToForumCode(string)"/> and <see cref="ForumCodeToHtml(string)"/>, and censors the text.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		public string ClientHtmlToHtml(string text)
		{
			text = Censor(text);
			text = ClientHtmlToForumCode(text);
			text = CleanForumCode(text);
			return CleanForumCodeToHtml(text);
		}

		public string HtmlToClientHtml(string text)
		{
			text = text.Replace("<blockquote>", "[quote]");
			text = text.Replace("</blockquote>", "[/quote]");
			text = Regex.Replace(text, @" *target=""[_\w]*""", String.Empty, RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(<iframe )(.)*?(src=""https?://www.youtube.com/embed/)(\S+)("")(.)*?( */iframe>)", "https://www.youtube.com/watch?v=$4", RegexOptions.IgnoreCase);
			return text;
		}

		public string HtmlToForumCode(string text)
		{
			text = HtmlToClientHtml(text);
			text = ClientHtmlToForumCode(text);
			return text;
		}

		public string Censor(string text)
		{
			if (String.IsNullOrEmpty(text))
				return String.Empty;
			// build the censored words list
			var words = _settingsManager.Current.CensorWords.Trim();
			if (String.IsNullOrWhiteSpace(words))
				return text;
			var cleanedCensorList = words.Replace("  ", " ").Replace("\r", " ");
			var list = cleanedCensorList.Split(new[] { ' ' });
			// convert any stand alone words (with * before of after them) to spaces
			for (var i = 0; i < list.Length; i++) { list[i] = list[i].Replace("*", " "); }
			// now you've got your list of naughty words, clean them out of the text
			var newWord = String.Empty;
			for (var i = 0; i < list.Length; i++)
			{
				for (var j = 1; j <= list[i].Length; j++) newWord += _settingsManager.Current.CensorCharacter;
				text = Regex.Replace(text, list[i], newWord, RegexOptions.IgnoreCase);
				newWord = String.Empty;
			}
			return text;
		}

		/// <summary>
		/// Converts client HTML to forum code. Important: This method does NOT attempt to create valid HTML, as it assumes that the forum code 
		/// will be cleaned. This method should generally not be called directly except for testing.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		public string ClientHtmlToForumCode(string text)
		{
			text = text.Trim();

			// replace line breaks, get block elements right
			text = text.Replace("\r\n", String.Empty);
			text = text.Replace("\n", String.Empty);
			text = Regex.Replace(text, @"((?<!(\A|<blockquote>|</blockquote>|</p>))(<blockquote>|<p>))", "</p>$1", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(</blockquote>|</p>)((?!(<p>|<blockquote>|</blockquote>))(.*</p>))", "$1<p>$2", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "^<p>", String.Empty, RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "</p>$", String.Empty, RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "<blockquote>", "\r\n[quote]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "</blockquote>", "[/quote]\r\n", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "<p>", "\r\n", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "</p>", "\r\n", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "<br ?/?>", "\r\n", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"\[quote\](?!(\r\n))", "[quote]\r\n", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(?<!(\r\n))\[/quote\]", "\r\n[/quote]", RegexOptions.IgnoreCase);

			// replace basic tags
			text = Regex.Replace(text, @"<em>", "[i]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</em>", "[/i]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<i>", "[i]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</i>", "[/i]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<strong>", "[b]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</strong>", "[/b]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<b>", "[b]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</b>", "[/b]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<code>", "[code]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</code>", "[/code]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<pre>", "[pre]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</pre>", "[/pre]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<li>", "[li]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</li>", "[/li]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<ol>", "[ol]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</ol>", "[/ol]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<ul>", "[ul]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</ul>", "[/ul]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</a>", "[/url]", RegexOptions.IgnoreCase);

			// replace img and a tags
			text = Regex.Replace(text, @"(<a href="")(\S+)""( *target=""?[_\w]*""?)*>", "[url=$2]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<img .*src=""(\S+)"".*/>", "[image=$1]", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(<iframe )(\S+ )*(src=""https?://www.youtube.com/embed/)(\S+)("")( *\S+)*( */iframe>)", "[youtube=https://www.youtube.com/watch?v=$4]", RegexOptions.IgnoreCase);

			// catch remaining HTML as invalid
			text = Regex.Replace(text, @"<.*?>", String.Empty, RegexOptions.IgnoreCase);

			// convert HTML escapes
			text = Regex.Replace(text, "&nbsp;", " ", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "&amp;", "&", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "&lt;", "<", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, "&gt;", ">", RegexOptions.IgnoreCase);

			return text.Trim();
		}

		/// <summary>
		/// Cleans forum code by making sure tags are properly closed, escapes HTML, removes images if settings require it, removes extra line breaks 
		/// and marks up URL's and e-mail addresses as links. This method should generally not be called directly except for testing.
		/// </summary>
		/// <param name="text">Text with forum code to clean.</param>
		/// <returns>Cleaned forum code text.</returns>
		public string CleanForumCode(string text)
		{
			// ditch white space
			text = text.Trim();

			// replace lonely \n (some browsers)
			text = Regex.Replace(text, @"((?<!\r)\n)", "\r\n", RegexOptions.Multiline | RegexOptions.IgnoreCase);

			// remove duplicate line breaks
			text = Regex.Replace(text, @"(\r\n){3,}", "\r\n\r\n", RegexOptions.Multiline);

			// handle images
			if (_settingsManager.Current.AllowImages)
				text = Regex.Replace(text, @"(\[image){1}(?!\=).+?\]", String.Empty, RegexOptions.IgnoreCase);
			else
				text = Regex.Replace(text, @"\[image=.+?\]", String.Empty, RegexOptions.IgnoreCase);

			// close all tags
			var stack = new Stack<string>();
			var allMatches = TagPattern.Match(text);
			var indexAdjustment = 0;
			while (allMatches.Success)
			{
				var tag = allMatches.ToString();
				if (!tag.StartsWith("[/"))
				{
					// opening tag
					var tagID = TagID.Replace(tag, "$1");
					if (AllowedCloseableTags.Contains(tagID))
						stack.Push(tagID);
				}
				else
				{
					// closing tag
					var tagID = TagID.Replace(tag, "$1");
					if (stack.Count == 0 || !stack.Contains(tagID))
					{
						// prepend with opener
						if (tagID == "url")
						{
							var tagIndex = allMatches.Index;
							text = text.Remove(tagIndex + indexAdjustment, 6);
							indexAdjustment -= 6;
						}
						else if (tagID == "youtube")
						{
							var tagIndex = allMatches.Index;
							text = text.Remove(tagIndex + indexAdjustment, 10);
							indexAdjustment -= 10;
						}
						else
						{
							var opener = String.Format("[{0}]", tagID);
							text = opener + text;
						}
					}
					else if (AllowedCloseableTags.Contains(tagID) && tagID == stack.Peek())
						stack.Pop();
					else
					{
						// close then reopen tag
						var miniStack = new Stack<string>();
						var tagIndex = allMatches.Index;
						while (tagID != stack.Peek())
						{
							miniStack.Push(stack.Peek());
							var closer = String.Format("[/{0}]", miniStack.Peek());
							text = text.Insert(tagIndex + indexAdjustment, closer);
							indexAdjustment += closer.Length;
							stack.Pop();
						}
						stack.Pop();
						while (miniStack.Count > 0)
						{
							var opener = String.Format("[{0}]", miniStack.Peek());
							text = text.Insert(tagIndex + indexAdjustment + tag.Length, opener);
							stack.Push(miniStack.Pop());
							indexAdjustment += opener.Length;
						}
					}
				}
				allMatches = allMatches.NextMatch();
			}
			while (stack.Count != 0)
			{
				// add closers
				var closer = String.Format("[/{0}]", stack.Peek());
				text += closer;
				stack.Pop();
			}

			// put URL's in url tags (plus youtube)
			if (_settingsManager.Current.AllowImages)
				text = YouTubePattern.Replace(text, match => String.Format("[youtube={0}]", match.Value));
			text = ProtocolPattern.Replace(text, match => String.Format("[url={0}]{1}[/url]", match.Value, match.Value.Trimmer(80)));
			text = WwwPattern.Replace(text, match => String.Format("[url=http://{0}]{1}[/url]", match.Value, match.Value.Trimmer(80)));
			text = EmailPattern.Replace(text, match => String.Format("[url=mailto:{0}]{0}[/url]", match.Value));

			// escape out rogue HTML tags
			text = EscapeHtmlTags(text);

			return text;
		}

		private static string EscapeHtmlTags(string text)
		{
			text = text.Replace("<", "&lt;");
			text = text.Replace(">", "&gt;");
			return text;
		}

		/// <summary>
		/// Converts forum code to HTML for storage. Important: This method does NOT attempt to create valid HTML, as it assumes that the forum code is 
		/// already well-formed. This method should generally not be called directly except for testing.
		/// </summary>
		/// <param name="text">Text to parse.</param>
		/// <returns>Parsed text.</returns>
		public string CleanForumCodeToHtml(string text)
		{
			text = text.Trim();

			// replace URL tags
			text = Regex.Replace(text, @"(\[url=""?)(\S+?)(""?\])", "<a href=\"$2\" target=\"_blank\">", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(<a href=\""mailto:)(\S+?)(\"" target=\""_blank\"">)", "<a href=\"mailto:$2\">", RegexOptions.IgnoreCase);
			text = text.Replace("[/url]", "</a>");
			text = Regex.Replace(text, @"<(?=a)\b[^>]*>", match => match.Value.Replace("javascript:", String.Empty), RegexOptions.IgnoreCase);

			// replace image tags
			if (_settingsManager.Current.AllowImages)
			{
				text = Regex.Replace(text, @"(\[img\])(\S+?)(\[/img\])", "<img src=\"$2\" />", RegexOptions.IgnoreCase);
				text = Regex.Replace(text, @"(\[image=""?)(\S+?)(""?\])", "<img src=\"$2\" />", RegexOptions.IgnoreCase);
				text = ParseYouTubeTags(text);
			}
			else
				text = Regex.Replace(text, @"(\[image=""?)(\S+?)(""?\])", String.Empty, RegexOptions.IgnoreCase);

			// simple tags
			text = text.Replace("[i]", "<em>");
			text = text.Replace("[/i]", "</em>");
			text = text.Replace("[b]", "<strong>");
			text = text.Replace("[/b]", "</strong>");
			text = text.Replace("[code]", "<code>");
			text = text.Replace("[/code]", "</code>");
			text = text.Replace("[pre]", "<pre>");
			text = text.Replace("[/pre]", "</pre>");
			text = text.Replace("[li]", "<li>");
			text = text.Replace("[/li]", "</li>");
			text = text.Replace("[ol]", "<ol>");
			text = text.Replace("[/ol]", "</ol>");
			text = text.Replace("[ul]", "<ul>");
			text = text.Replace("[/ul]", "</ul>");

			// line breaks and block elements
			text = Regex.Replace(text, @"(\r\n){3,}", "\r\n\r\n");
			if (!text.StartsWith("[quote]") && !string.IsNullOrWhiteSpace(text)) text = "<p>" + text;
			if (!text.EndsWith("[/quote]") && !string.IsNullOrWhiteSpace(text)) text += "</p>";
			text = text.Replace("[quote]", "<blockquote>");
			text = text.Replace("[/quote]", "</blockquote>");
			text = Regex.Replace(text, @"(?<!(</blockquote>))\r\n\r\n(?!(<p>|<blockquote>|</blockquote>))", "</p><p>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(?<=(</p>|<blockquote>|</blockquote>|\A))(\r\n)*<blockquote>", "<blockquote>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(\r\n)+<blockquote>", "</p><blockquote>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<blockquote>\r\n(?!(<p>|<blockquote>|</blockquote>))", "<blockquote><p>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(?<!([</p>|<blockquote>|</blockquote>](\r\n)*))</blockquote>", "</p></blockquote>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</blockquote>(\r\n){2,}(?!(</p>|<blockquote>|</blockquote>))", "</blockquote><p>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</blockquote>(\r\n)*</blockquote>", "</blockquote></blockquote>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(?<!(</p>|<blockquote>|</blockquote>|\A))<blockquote>", "</p><blockquote>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"<blockquote>(?!(<p>|<blockquote>|</blockquote>))", "<blockquote><p>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"(?<!(</p>|<blockquote>|</blockquote>))(\r\n)*</blockquote>", "</p></blockquote>", RegexOptions.IgnoreCase);
			text = Regex.Replace(text, @"</blockquote>(\r\n)*(?!(<p>|<blockquote>|</blockquote>|\Z))", "</blockquote><p>", RegexOptions.IgnoreCase);
			text = text.Replace("\r\n", "<br />");

			return text;
		}

		public string RemoveForumCode(string text)
		{
			text = TagPattern.Replace(text, "");
			return text;
		}

		private string ParseYouTubeTags(string text)
		{
			var width = _settingsManager.Current.YouTubeWidth;
			var height = _settingsManager.Current.YouTubeHeight;
			var youTubeTag = new Regex(@"(\[youtube=""?)(\S+?)(""?\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var matches = youTubeTag.Matches(text);
			foreach (Match item in matches)
			{
				var url = item.Groups[2].Value;
				var uri = new Uri(url);
				if (uri.Host.Contains("youtube"))
				{
					var q = uri.Query.Remove(0, 1).Split('&').Where(x => x.Contains("=")).Select(x => new KeyValuePair<string, string>(x.Split('=')[0], x.Split('=')[1]));
					var dictionary = q.ToDictionary(pair => pair.Key, pair => pair.Value);
					if (dictionary.Any(x => x.Key == "v"))
					{
						text = text.Replace(item.Value, String.Format(@"<iframe width=""{1}"" height=""{2}"" src=""https://www.youtube.com/embed/{0}"" frameborder=""0"" allowfullscreen></iframe>", dictionary["v"], width, height));
					}
				}
				else if (uri.Host.Contains("youtu.be"))
				{
					var v = uri.Segments[1];
					text = text.Replace(item.Value, String.Format(@"<iframe width=""{1}"" height=""{2}"" src=""https://www.youtube.com/embed/{0}"" frameborder=""0"" allowfullscreen></iframe>", v, width, height));
				}
			}
			return text;
		}
	}
}