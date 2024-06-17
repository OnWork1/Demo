using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
//using BMW.IntegrationService.CommonClassesAndEnums.Classes.Operations;
using BMW.IntegrationService.CrmGenerated;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Crm
{
    #region ConnectionRoleTypeEnum
    [Flags]
    public enum ConnectionRoleTypeEnum
    {
        /// <summary>
        /// Owner role.
        /// </summary>
        Owner = 1,
        /// <summary>
        /// Keeper role.
        /// </summary>
        Keeper = 2,
        /// <summary>
        /// User role.
        /// </summary>
        User = 4,
        /// <summary>
        /// Coowner role.
        /// </summary>
        CoOwner = 8,
        /// <summary>
		/// Payer role.
		/// </summary>
		Payer = 5,
        /// <summary>
        /// Signer role.
        /// </summary>
        Signer = 6,
        /// <summary>
        /// Unknown role.
        /// </summary>
        None = 16
    }
    #endregion

    public class CrmEntityConnectionHelper
    {
        private const string OwnerRoleName = "Owner";
        private const string OwnedByRoleName = "Owned By";
        private const string CoOwnerRoleName = "Co-Owner";
        private const string CoOwnedByRoleName = "Co-owned By";
        private const string KeptByRoleName = "Kept By";
        private const string KeeperRoleName = "Keeper";
        private const string UsedByRoleName = "Used By";
        private const string UserRoleName = "User";
        private const string PayedByRoleName = "Payed By";
        private const string PayerRoleName = "Payer";
        private const string SignedByRoleName = "Signed By";
        private const string SignerRoleName = "Signer";

        private Guid? ownerConnectionRoleId;
        private Guid? coOwnerConnectionRoleId;
        private Guid? keeperConnectionRoleId;
        private Guid? userConnectionRoleId;

        private Guid? ownedByConnectionRoleId;
        private Guid? coOwnedByConnectionRoleId;
        private Guid? keptByConnectionRoleId;
        private Guid? usedByConnectionRoleId;

        private Guid? payedByConnectionRoleId;
        private Guid? signedByConnectionRoleId;
        private Guid? payerConnectionRoleId;
        private Guid? signerConnectionRoleId;

        private readonly IOrganizationService organizationService;

        // Constructors

        #region CrmEntityConnectionHelper(IOrganizationService organizationService, OperationResult operationResult)
        public CrmEntityConnectionHelper(IOrganizationService organizationService/*, OperationResult operationResult*/)
        {
            if (organizationService == null)
            {
                throw new ArgumentNullException("organizationService");
            }

            this.organizationService = organizationService;
        }
        #endregion

        // Public properties
        #region CoOwnerConnectionRoleId
        public Guid CoOwnerConnectionRoleId
        {
            get
            {
                if (!this.coOwnerConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(CoOwnerRoleName);
                    if (!connectionId.HasValue)
                    {                     
                        return Guid.Empty;
                    }
                    this.coOwnerConnectionRoleId = connectionId;
                }
                return this.coOwnerConnectionRoleId.Value;
            }
        }
        #endregion

        #region OwnerConnectionRoleId
        public Guid OwnerConnectionRoleId
        {
            get
            {
                if (!this.ownerConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(OwnerRoleName);
                    if (!connectionId.HasValue)
                    {                      
                        return Guid.Empty;
                    }
                    this.ownerConnectionRoleId = connectionId;                  
                }
                return this.ownerConnectionRoleId.Value;
            }
        }
        #endregion

        #region KeeperConnectionRoleId
        public Guid KeeperConnectionRoleId
        {
            get
            {
                if (!this.keeperConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(KeeperRoleName);
                    if (!connectionId.HasValue)
                    {                  
                        return Guid.Empty;
                    }
                    this.keeperConnectionRoleId = connectionId;
                }
                return this.keeperConnectionRoleId.Value;
            }
        }
        #endregion

        #region UserConnectionRoleId
        public Guid UserConnectionRoleId
        {
            get
            {
                if (!this.userConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(UserRoleName);
                    if (!connectionId.HasValue)
                    {
                        return Guid.Empty;
                    }
                    this.userConnectionRoleId= connectionId;               
                }
                return this.userConnectionRoleId.Value;
            }
        }
        #endregion

        #region OwnedByConnectionRoleId
        public Guid OwnedByConnectionRoleId
        {
            get
            {
                if (!this.ownedByConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(OwnedByRoleName);
                    if (!connectionId.HasValue)
                    {
                        return Guid.Empty;
                    }
                    this.ownedByConnectionRoleId= connectionId;                }
                return this.ownedByConnectionRoleId.Value;
            }
        }
        #endregion

        #region KeptByConnectionRoleId
        public Guid KeptByConnectionRoleId
        {
            get
            {
                if (!this.keptByConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(KeptByRoleName);
                    if (!connectionId.HasValue)
                    {                     
                        return Guid.Empty;
                    }
                    this.keptByConnectionRoleId= connectionId;
                }
                return this.keptByConnectionRoleId.Value;
            }
        }
        #endregion

        #region UsedByConnectionRoleId
        public Guid UsedByConnectionRoleId
        {
            get
            {
                if (!this.usedByConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(UsedByRoleName);
                    if (!connectionId.HasValue)
                    {
                        return Guid.Empty;
                    }
                    this.usedByConnectionRoleId= connectionId;
                }
                return this.usedByConnectionRoleId.Value;
            }
        }
        #endregion

        #region PayerConnectionRoleId
        public Guid PayerConnectionRoleId
        {
            get
            {
                if (!this.payerConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(PayerRoleName);
                    if (!connectionId.HasValue)
                    {
                        return Guid.Empty;
                    }
                    this.payerConnectionRoleId = connectionId;
                }
                return this.payerConnectionRoleId.Value;
            }
        }
        #endregion

        #region PayedByConnectionRoleId
        public Guid PayedByConnectionRoleId
        {
            get
            {
                if (!this.payedByConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(PayedByRoleName);
                    if (!connectionId.HasValue)
                    {
                        return Guid.Empty;
                    }
                    this.payedByConnectionRoleId = connectionId;
                }
                return this.payedByConnectionRoleId.Value;
            }
        }
        #endregion

        #region SignerConnectionRoleId
        public Guid SignerConnectionRoleId
        {
            get
            {
                if (!this.signerConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(SignerRoleName);
                    if (!connectionId.HasValue)
                    {                        
                        return Guid.Empty;
                    }
                    this.signerConnectionRoleId= connectionId;
                }
                return this.signerConnectionRoleId.Value;
            }
        }
        #endregion

        #region SignedByConnectionRoleId
        public Guid SignedByConnectionRoleId
        {
            get
            {
                if (!this.signedByConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(SignedByRoleName);
                    if (!connectionId.HasValue)
                    {
                        return Guid.Empty;
                    }
                    this.signedByConnectionRoleId = connectionId;
                }
                return this.signedByConnectionRoleId.Value;
            }
        }
        #endregion

        #region CoOwnedByConnectionRoleId
        public Guid CoOwnedByConnectionRoleId
        {
            get
            {
                if (!this.coOwnedByConnectionRoleId.HasValue)
                {
                    Guid? connectionId = this.GetConnectionIdByName(CoOwnedByRoleName);
                    if (!connectionId.HasValue)
                    {                    
                        return Guid.Empty;
                    }
                    this.coOwnedByConnectionRoleId = connectionId;
                }
                return this.coOwnedByConnectionRoleId.Value;
            }
        }
        #endregion

        // Private methods

        #region GetConnectionIdByName(string connectionName)
        private Guid? GetConnectionIdByName(string connectionName)
        {
            QueryByAttribute query = new QueryByAttribute(ConnectionRole.EntityLogicalName);

            query.AddAttributeValue("name", connectionName);

            query.PageInfo = new PagingInfo { Count = 1, PageNumber = 1 };

            EntityCollection result = this.organizationService.RetrieveMultiple(query);

            if (result == null || result.Entities == null)
            {
                return null;
            }

            Entity entity = result.Entities.FirstOrDefault();

            if (entity == null)
                return null;

            return entity.Id;
        }
        #endregion

        #region GetConnectionRoleType(string connectionRoleName)
        private ConnectionRoleTypeEnum GetConnectionRoleType(string connectionRoleName)
        {
            if (String.IsNullOrEmpty(connectionRoleName))
            {
                return ConnectionRoleTypeEnum.None;
            }

            switch (connectionRoleName)
            {
                case OwnerRoleName:
                    return ConnectionRoleTypeEnum.Owner;

                case KeeperRoleName:
                    return ConnectionRoleTypeEnum.Keeper;

                case UserRoleName:
                    return ConnectionRoleTypeEnum.User;

                default:                  
                    return ConnectionRoleTypeEnum.None;
            }

        }
        #endregion

        // Public methods
        #region GetExistingConnection(Connection connection, ColumnSet columnSet)
        public List<Connection> GetExistingConnection(Connection connection, ColumnSet columnSet)
        {
            return this.GetExistingConnection(connection.Record1Id, connection.Record2Id, connection.Record1RoleId,
                                              connection.Record2RoleId, columnSet);
        }
        #endregion

        #region GetExistingConnection(EntityReference entity1, EntityReference entity2, EntityReference role1, EntityReference role2, ColumnSet columnSet)
        public List<Connection> GetExistingConnection(EntityReference entity1, EntityReference entity2, EntityReference role1, EntityReference role2, ColumnSet columnSet)
        {
            if (entity1 == null && entity2 == null && role1 == null && role2 == null)
                return null;

            QueryExpression query = new QueryExpression
            {
                EntityName = Connection.EntityLogicalName,
                NoLock = true,
                PageInfo = new PagingInfo { Count = 500, PageNumber = 1 },
                ColumnSet = columnSet
            };
            if (entity1 != null)
                query.Criteria.AddCondition("record1id", ConditionOperator.Equal, entity1.Id);
            if (entity2 != null)
                query.Criteria.AddCondition("record2id", ConditionOperator.Equal, entity2.Id);
            if (role1 != null)
                query.Criteria.AddCondition("record1roleid", ConditionOperator.Equal, role1.Id);
            if (role2 != null)
                query.Criteria.AddCondition("record2roleid", ConditionOperator.Equal, role2.Id);

            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            EntityCollection entities = this.organizationService.RetrieveMultiple(query);

            return entities == null || entities.Entities == null || entities.Entities.Count == 0
                       ? null
                       : entities.Entities.Cast<Connection>().ToList();
        }
        #endregion

        #region GetConnectionForRoleName(EntityReference entityReference1, EntityReference entityReference2, string connectionName)
        public Connection GetConnectionForRoleName(EntityReference entityReference1, EntityReference entityReference2, string connectionName)
        {
            ConnectionRoleTypeEnum connectionRoleType = this.GetConnectionRoleType(connectionName);

            return this.GetConnectionForRoleEnum(entityReference1, entityReference2, connectionRoleType);

        }
        #endregion

        #region GetConnectionForRoleEnum(EntityReference entityReference1, EntityReference entityReference2, ConnectionRoleTypeEnum connectionRoleType)
        public Connection GetConnectionForRoleEnum(EntityReference entityReference1, EntityReference entityReference2, ConnectionRoleTypeEnum connectionRoleType)
        {
            if (entityReference1 == null || entityReference2 == null)
            {
                return null;
            }

            Guid? connectionRole1Id = null;
            Guid? connectionRole2Id = null;

            switch (connectionRoleType)
            {
                case ConnectionRoleTypeEnum.None:
                    return null;

                case ConnectionRoleTypeEnum.Keeper:
                    connectionRole1Id = this.KeptByConnectionRoleId;
                    connectionRole2Id = this.KeeperConnectionRoleId;
                    break;

                case ConnectionRoleTypeEnum.Owner:
                    connectionRole1Id = this.OwnedByConnectionRoleId;
                    connectionRole2Id = this.OwnerConnectionRoleId;
                    break;

                case ConnectionRoleTypeEnum.User:
                    connectionRole1Id = this.UsedByConnectionRoleId;
                    connectionRole2Id = this.UserConnectionRoleId;

                    break;

                case ConnectionRoleTypeEnum.CoOwner:
                    connectionRole1Id = this.CoOwnedByConnectionRoleId;
                    connectionRole2Id = this.CoOwnerConnectionRoleId;

                    break;
            }

            if (!connectionRole1Id.HasValue)
            {
                return null;
            }

            Connection connection = new Connection
            {
                Record1Id = entityReference1,
                Record1RoleId = new EntityReference(Connection.EntityLogicalName, connectionRole1Id.Value),

                Record2Id = entityReference2,
                Record2RoleId = new EntityReference(Connection.EntityLogicalName, connectionRole2Id.Value)
            };


            return connection;
        }
        #endregion
    }
}

