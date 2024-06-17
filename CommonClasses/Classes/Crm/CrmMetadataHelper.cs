
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using BMW.IntegrationService.CommonClassesAndEnums.Classes.Expressions;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
	public class CrmMetadataHelper
	{
		private static OptionSetMetadata GetOptionSetOptions(string entityLogicalName, string attributeName, IOrganizationService service)
		{
			string AttributeName = attributeName;
			string EntityLogicalName = entityLogicalName;
			RetrieveEntityRequest retrieveDetails = new RetrieveEntityRequest
			{
				EntityFilters = EntityFilters.All,
				LogicalName = EntityLogicalName
			};
			RetrieveEntityResponse retrieveEntityResponseObj = (RetrieveEntityResponse)service.Execute(retrieveDetails);
			Microsoft.Xrm.Sdk.Metadata.EntityMetadata metadata = retrieveEntityResponseObj.EntityMetadata;
			Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata picklistMetadata = metadata.Attributes.FirstOrDefault(attribute => String.Equals
							(attribute.LogicalName, attributeName, StringComparison.OrdinalIgnoreCase)) as Microsoft.Xrm.Sdk.Metadata.PicklistAttributeMetadata;
			return picklistMetadata.OptionSet;
		}

		#region GetObjectTypeCode(string strEntityName, IOrganizationService service)

		/// <summary>
		/// Get object type code for specifiec entityName.
		/// </summary>
		/// <param name="service">Instance of <see cref="IOrganizationService"/></param>
		/// <param name="strEntityName">name of entity</param>
		/// <returns>Value of object type code if entity metadata found, otherwise null</returns>
		public static int? GetObjectTypeCode(IOrganizationService service, string strEntityName)
		{
			if (service == null)
				return null;

			// Execute RetrieveEntityMetadata.

			RetrieveEntityRequest entityRequest = new RetrieveEntityRequest();

			entityRequest.EntityFilters = EntityFilters.Entity;

			entityRequest.LogicalName = strEntityName;

			RetrieveEntityResponse entityResponse = (RetrieveEntityResponse) service.Execute(entityRequest);

			if (entityResponse == null || entityResponse.EntityMetadata == null)
				return null;

			EntityMetadata entity = entityResponse.EntityMetadata;

			return entity.ObjectTypeCode;
		}

		#endregion

        #region GetGlobalOptionSetLabel<T>(IOrganizationService service, int optionSetValue)
		/// <summary>
		/// Retrieves optionset label for specified global optionset and its value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="service"></param>
		/// <param name="optionSetValue"></param>
		/// <returns></returns>
		public static string GetGlobalOptionSetLabel<T>(IOrganizationService service, int optionSetValue)
		{
			RetrieveOptionSetRequest retrieveOptionSetRequest =
				new RetrieveOptionSetRequest
					{
                        Name = typeof(T).Name
					};

			RetrieveOptionSetResponse retrieveOptionSetResponse =
                (RetrieveOptionSetResponse)service.Execute(
					retrieveOptionSetRequest);			

			// Access the retrieved OptionSetMetadata.
			OptionSetMetadata retrievedOptionSetMetadata =
                (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

			// Get the current options list for the retrieved attribute.
			OptionMetadata[] optionList =
				retrievedOptionSetMetadata.Options.ToArray();

			Label optionSetLabel = (from optionMetadata in optionList
			                        where optionMetadata.Value == optionSetValue
			                        select optionMetadata.Label).FirstOrDefault();

			if (optionSetLabel != null)
			{
				return optionSetLabel.LocalizedLabels.FirstOrDefault().Label;
			}

			return null;
		}
        #endregion

        #region GetGlobalOptionSetValue<T>(IOrganizationService service, string label)
        /// <summary>
        /// Retrieves optionset value for specified global optionset and its label
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static int? GetGlobalOptionSetValue<T>(IOrganizationService service, string label)
        {
            RetrieveOptionSetRequest retrieveOptionSetRequest =
                new RetrieveOptionSetRequest
                {
                    Name = typeof(T).Name
                };

            RetrieveOptionSetResponse retrieveOptionSetResponse =
                (RetrieveOptionSetResponse)service.Execute(
                    retrieveOptionSetRequest);

            // Access the retrieved OptionSetMetadata.
            OptionSetMetadata retrievedOptionSetMetadata =
                (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

            // Get the current options list for the retrieved attribute.
            OptionMetadata[] optionList =
                retrievedOptionSetMetadata.Options.ToArray();

            foreach (OptionMetadata option in optionList)
            {
                if (
                    option.Label.LocalizedLabels.Any(
                        p => String.Compare(p.Label, label, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    return option.Value;
                }
            }

            return null;
        } 
        #endregion

	
		public static string[] GetGlobalOptionSetLabels<T>(IOrganizationService service)
		{
			RetrieveOptionSetRequest retrieveOptionSetRequest =
				new RetrieveOptionSetRequest
				{
					Name = typeof(T).Name
				};

			RetrieveOptionSetResponse retrieveOptionSetResponse =
				(RetrieveOptionSetResponse)service.Execute(
					retrieveOptionSetRequest);

			// Access the retrieved OptionSetMetadata.
			OptionSetMetadata retrievedOptionSetMetadata =
				(OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

			// Get the current options list for the retrieved attribute.
			OptionMetadata[] optionList =
				retrievedOptionSetMetadata.Options.ToArray();

			Label[] optionSetLabels = (from optionMetadata in optionList
									select optionMetadata.Label).ToArray();

			if (optionSetLabels != null && optionSetLabels.Length>0)
			{
				return (from l in optionSetLabels
												   select l.LocalizedLabels.FirstOrDefault().Label).ToArray();
			}

			return null;
		}
		
		public static string GetOptionSetText(string entityLogicalName, string attributeName, int optionSetValue, IOrganizationService service)
		{
			Microsoft.Xrm.Sdk.Metadata.OptionSetMetadata options = GetOptionSetOptions(entityLogicalName, attributeName, service);
			IList<OptionMetadata> OptionsList = (from o in options.Options
												 where o.Value.Value == optionSetValue
												 select o).ToList();
			string optionsetLabel = (OptionsList.First()).Label.UserLocalizedLabel.Label;
			return optionsetLabel;
		}

		public static string[] GetOptionSetTexts(string entityLogicalName, string attributeName, IOrganizationService service)
		{
			Microsoft.Xrm.Sdk.Metadata.OptionSetMetadata options = GetOptionSetOptions(entityLogicalName, attributeName, service);
			IList<OptionMetadata> OptionsList = (from o in options.Options
												 select o).ToList();
			string[] optionsetLabels = (from l in OptionsList select l.Label.UserLocalizedLabel.Label).ToArray();
			return optionsetLabels;
		}

		#region GetAttributeLogicalName<TTarget, TValue>(Expression<Func<TTarget, TValue>> attributeSelector) where TTarget : Entity
		public static string GetAttributeLogicalName<TTarget, TValue>(Expression<Func<TTarget, TValue>> attributeSelector) where TTarget : Entity
		{
			if (attributeSelector == null)
				throw new ArgumentNullException("attributeSelector");

			MemberExpression expression;
			if (attributeSelector.Body is ConditionalExpression)
			{
				expression = ExpressionHelper.GetMemberExpressionFromCondition(attributeSelector.Body as ConditionalExpression);
			}
			else
			{
				expression = attributeSelector.Body as MemberExpression;
			}

			if (expression == null)
				throw new Exception(string.Format("Selector is not valid"));

			object[] attr = expression.Member.GetCustomAttributes(typeof(AttributeLogicalNameAttribute), false);
			if (attr.Length < 1)
				throw new Exception(string.Format("Logical name was not found for {0} of {1}", expression.Member.Name, typeof(TTarget).Name));

			return ((AttributeLogicalNameAttribute)attr[0]).LogicalName;
		}
		#endregion

		
	
	}
}
