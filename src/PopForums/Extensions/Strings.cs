using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PopForums.Extensions
{
	public static class Strings
	{
        public static string GetSHA256Hash(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			var input = Encoding.UTF8.GetBytes(text);
			using (var sha256 = SHA256.Create())
			{
				var output = sha256.ComputeHash(input);
				return Convert.ToBase64String(output);
			}
		}

		public static string GetSHA256Hash(this string text, Guid salt)
		{
			var concatString = text + salt;
			return GetSHA256Hash(concatString);
		}

		public static string GetMD5Hash(this string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			var input = Encoding.UTF8.GetBytes(text);
			using (var md5 = MD5.Create())
			{
				var output = md5.ComputeHash(input);
				return Convert.ToBase64String(output);
			}
		}

		public static string GetMD5Hash(this string text, Guid salt)
		{
			var concatString = text + salt;
			return GetMD5Hash(concatString);
		}

		public static bool IsEmailAddress(this string text)
		{
			return Regex.IsMatch(text, @"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$", RegexOptions.IgnoreCase);
		}

		public static string ToUrlName(this string text)
		{
			if (text == null)
				throw new Exception("Can't Url convert a null string.");
			var result = text.Replace(" ", "-");
			var replacer = new Regex(@"[^\w\-]", RegexOptions.None);
			result = replacer.Replace(result, "").ToLower();
			return result;
		}

		public static string ToUniqueUrlName(this string name, List<string> matchingStartsWith)
		{
			var urlName = name.ToUrlName();
			var originalName = urlName;
			var matchTest = urlName.Replace("-", @"\-");
			var count = matchingStartsWith.Count(m => Regex.IsMatch(m, @"^(" + matchTest + @")(\-\d)?$"));
			if (count > 0)
				urlName = urlName + "-" + (count + 1);
			while (matchingStartsWith.Exists(x => x == urlName))
			{
				count++;
				urlName = originalName + "-" + (count + 1);
			}
			return urlName;
		}

		public static string Trimmer(this string stringToTrim, int maxLength)
		{
			if (maxLength < 20) maxLength = 20;
			if (stringToTrim.Length <= maxLength) return stringToTrim;
			return stringToTrim.Substring(0, maxLength - 13) + "..." +
				   stringToTrim.Substring(stringToTrim.Length - 10, 10);
		}
	}
}
