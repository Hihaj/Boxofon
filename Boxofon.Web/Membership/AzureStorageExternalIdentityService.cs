using System;
using System.Web.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public class AzureStorageExternalIdentityService : ExternalIdentityServiceBase
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageExternalIdentityService(ITinyMessengerHub hub) : base(hub)
        {
            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("ExternalIdentities");
        }

        public override Guid? GetBoxofonUserId(string providerName, string providerUserId)
        {
            var op = TableOperation.Retrieve<ExternalIdentityEntity>(providerName, providerUserId);
            var result = Table().Execute(op);
            return result.Result == null ? (Guid?)null : ((ExternalIdentityEntity)result.Result).UserId;
        }

        protected override void AddExternalIdentity(string providerName, string providerUserId, Guid userId)
        {
            var entity = new ExternalIdentityEntity(providerName, providerUserId, userId);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);
        }

        public class ExternalIdentityEntity : TableEntity
        {
            public string ProviderName { get { return PartitionKey; } }
            public string ProviderUserId { get { return RowKey; } }
            public Guid UserId { get; set; }

            public ExternalIdentityEntity()
            {
            }

            public ExternalIdentityEntity(string providerName, string providerUserId, Guid userId)
            {
                PartitionKey = providerName;
                RowKey = providerUserId;
                UserId = userId;
            }
        }
    }
}