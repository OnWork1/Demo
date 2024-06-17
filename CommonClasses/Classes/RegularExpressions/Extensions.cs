using System.Text.RegularExpressions;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.RegularExpressions
{
	public static class Extensions
	{
		// Constants

		// Delegates

		// Events

		// Private Fields

		// Constructors
		

		// Private Properties

		// Protected Properties

		// Public Properties

		// Private Methods

		// Protected Methods

		// Public Methods
		#region IsMatch(this string stringToCheck, string regExPatern)
		public static bool IsMatch(this string stringToCheck, string regExPatern)
		{
			return stringToCheck.IsMatch(regExPatern, false);
		}
		#endregion

		#region IsMatch(this string stringToCheck, string regExPatern, bool ignoreCase)
		public static bool IsMatch(this string stringToCheck, string regExPatern, bool ignoreCase)
		{
			Regex regEx = ignoreCase ?
				new Regex(regExPatern, RegexOptions.IgnoreCase) :
				new Regex(regExPatern);

			return stringToCheck.IsMatch(regEx);
		}
		#endregion

		#region IsMatch(this string stringToCheck, Regex regEx)
		public static bool IsMatch(this string stringToCheck, Regex regEx)
		{
			return regEx.Matches(stringToCheck).Count > 0;
		}
		#endregion

		#region Match(string regExPatern, string stringToCheck)
		public static Match Match(this string stringToCheck, string regExPatern)
		{
			return stringToCheck.Match(regExPatern, false);
		}
		#endregion

		#region Match(this string stringToCheck, string regExPatern, bool ignoreCase)
		public static Match Match(this string stringToCheck, string regExPatern, bool ignoreCase)
		{
			Regex regEx = ignoreCase
			              	? new Regex(regExPatern, RegexOptions.IgnoreCase)
			              	: new Regex(regExPatern);

			return regEx.Match(stringToCheck);
		}
		#endregion

		#region Match(this string stringToCheck, string regExPatern, int beginning, int length)
		public static Match Match(this string stringToCheck, string regExPatern, int beginning, int length)
		{
			return stringToCheck.Match(regExPatern, beginning, length, false);
		}
		#endregion

		#region Match(this string stringToCheck, string regExPatern, int beginning, int length, bool ignoreCase)
		public static Match Match(this string stringToCheck, string regExPatern, int beginning, int length, bool ignoreCase)
		{
			Regex regEx = ignoreCase ?
				new Regex(regExPatern, RegexOptions.IgnoreCase) :
				new Regex(regExPatern);

			return regEx.Match(stringToCheck, beginning, length);
		}
		#endregion

		#region Match(string regExPatern, string stringToCheck, int startAt)
		public static Match Match(string stringToCheck, string regExPatern, int startAt)
		{
			return stringToCheck.Match(regExPatern, startAt, false);
		}
		#endregion

		#region Match(this string stringToCheck, string regExPatern,  int startAt, bool ignoreCase)
		public static Match Match(this string stringToCheck, string regExPatern,  int startAt, bool ignoreCase)
		{
			Regex regEx = ignoreCase ?
				new Regex(regExPatern, RegexOptions.IgnoreCase) :
				new Regex(regExPatern);

			return regEx.Match(stringToCheck, startAt);
		}
		#endregion
		// Event Handlers
	}
}
