
namespace BMW.IntegrationService.CommonClassesAndEnums.Enums
{
	public enum PluginActivityTypeEnum
	{
		/// <summary>
		/// No activity.
		/// </summary>
		None,
		/// <summary>
		/// Get data from source.
		/// </summary>
		GetDataFromSource,
		/// <summary>
		/// Transfer data to output plugin.
		/// </summary>
		TransferDataToOutputPlugin,
		/// <summary>
		/// Convert data.
		/// </summary>
		ConvertData,
		/// <summary>
		/// Transfer data to output.
		/// </summary>
		TransferDataToOutput,
		/// <summary>
		/// Saving data to destination.
		/// </summary>
		SavingDataToDestination
	}
}
