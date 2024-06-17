using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Strings
{
    public static class StringExtensions
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Private Properties

		// Protected Properties

		// Public Properties

		// Private Methods

		// Protected Methods

		// Public static methods

		#region ToBool(this string txt)
		public static bool? ToBool(this string txt)
		{
			if (String.IsNullOrEmpty(txt))
				return null;

			txt = txt.ToLower().Trim();
			if (txt == "1" || txt == "y" || txt == "true" || txt == "ano") return true;
			if (txt == "0" || txt == "n" || txt == "false" || txt == "ne") return false;

			return null;
		} 
		#endregion

		#region CutEnd(this string input, int newLength)
		/// <summary>
		/// returns the input string, cuts all characters that exceed the given number
		/// </summary>
		/// <param name="input">string to work with</param>
		/// <param name="newLength">maximal length of final string</param>
		/// <returns></returns>
		public static string CutEnd(this string input, int newLength)
		{
			if (String.IsNullOrEmpty(input)) return input;
			return input.Length > newLength ? input.Substring(0, newLength) : input;
		} 
		#endregion

		#region CutBeginning(this string input, int newLength)
		public static string CutBeginning(this string input, int newLength)
		{
			if (String.IsNullOrEmpty(input)) return input;
			return input.Length > newLength ? input.Substring(input.Length - newLength) : input;
		} 
		#endregion
		
		#region MakeNumeric<T>(this string source, T defaultValue)
		public static T MakeNumeric<T>(this string source, T defaultValue)
		{
			if (String.IsNullOrWhiteSpace(source))
				return defaultValue;

			string stringNumber = Regex.Replace(source, "[^.0-9]", "");


			if (String.IsNullOrWhiteSpace(stringNumber))
				return defaultValue;

			try
			{
				return (T)Convert.ChangeType(stringNumber, typeof(T));
			}
			catch
			{
				return defaultValue;
			}
		}
		#endregion

		#region Right(this string value, int length)
		/// <summary>
		/// Get substring of specified number of characters on the right.
		/// </summary>
		public static string Right(this string value, int length)
		{
			return String.IsNullOrWhiteSpace(value) ? String.Empty : value.Substring(value.Length - length);
		}

		#endregion

		#region RemoveWhiteSpaces(this string source)
		public static string RemoveWhiteSpaces(this string source)
		{
			return String.IsNullOrWhiteSpace(source) ? source : Regex.Replace(source, @"\s", "");
		} 
		#endregion
		
		#region RemoveHtmlTags(this string source, bool removeWhiteSpaces, bool replaceExcaping)
		public static string RemoveHtmlTags(this string source, bool removeWhiteSpaces, bool replaceEscaping)
		{		

			if (String.IsNullOrWhiteSpace(source))
				return String.Empty;
			
			var regex = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

			source = regex.Replace(source, "");

			source = source.Replace("<br />", "\r");

			if (removeWhiteSpaces)
			{
				source = Regex.Replace(source.Replace("\r", " ").Replace("\n", " "), @"\s+", " ");
			}

			if(replaceEscaping)
			{
				source = source.Replace("&gt;", "/>");
				source = source.Replace("&lt;", "<");
				source = source.Replace("&nbsp;", " ");
			}

			
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			foreach (char @let in source)
			{
				if (@let == '<')
				{
					inside = true;
					continue;
				}
				if (@let == '>')
				{
					inside = false;
					continue;
				}

				if (inside)
					continue;

				array[arrayIndex] = @let;
				arrayIndex++;
			}			
			return new string(array, 0, arrayIndex);
		}
		#endregion

		#region Contains(this string source, string value, StringComparison compareMode)
		public static bool Contains(this string source, string value, StringComparison compareMode)
		{
			if (string.IsNullOrEmpty(source))
				return false;

			return source.IndexOf(value, compareMode) >= 0;
		} 
		#endregion

        #region ToHexString(this string input)

	    /// <summary>
	    /// Converts string into hexadecimal representation of characters
	    /// </summary>
	    /// <param name="input"></param>
	    /// <param name="encoding"></param>
	    /// <returns></returns>
	    public static string ToHexString(this string input, Encoding encoding)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            return encoding.GetBytes(input).Aggregate(string.Empty, (current, byteToWrite) => current + byteToWrite.ToString("X2"));

        } 
        #endregion

        #region FromHexString(this string hexInput)

	    /// <summary>
	    /// Converts hexadecimal representation of string into string 
	    /// </summary>
	    /// <param name="hexInput">Value to convert. Without whitespaces. Each character must have two digits (leading 0 if it has only one digit).</param>
	    /// <param name="encoding"></param>
	    /// <returns></returns>
	    public static string FromHexString(this string hexInput, Encoding encoding)
        {
            if (String.IsNullOrEmpty(hexInput))
            {
                return hexInput;
            }

            byte[] bytes = new byte[hexInput.Length/2];
            for (int i = 0; i + 1 < hexInput.Length; i += 2)
            {
                bytes[i/2] = Convert.ToByte(hexInput.Substring(i, 2), 16);
            }

            return encoding.GetString(bytes);
        } 
        #endregion

        #region FromHexString(this string hexInput)
        public static string FromHexString(this string hexInput)
        {
            return hexInput.FromHexString(Encoding.UTF8);
        } 
        #endregion

        #region ToHexString(this string input)
        public static string ToHexString(this string input)
        {
            return input.ToHexString(Encoding.UTF8);
        } 
        #endregion

	    // Event Handlers

	}
}
