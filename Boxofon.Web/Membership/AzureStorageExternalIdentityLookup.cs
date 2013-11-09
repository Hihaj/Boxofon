using System;
using System.Web.Configuration;
using Boxofon.Web.Infrastructure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public class AzureStorageExternalIdentityLookup : ExternalIdentityLookupBase, IRequireInitialization
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageExternalIdentityLookup()
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
            var op = TableOperation.Retrieve<ExternalIdentityEntity>(providerUserId, providerName);
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
            public string ProviderName { get { return RowKey; } }
            public string ProviderUserId { get { return PartitionKey; } }
            public Guid UserId { get; set; }

            public ExternalIdentityEntity()
            {
            }

            public ExternalIdentityEntity(string providerName, string providerUserId, Guid userId)
            {
                PartitionKey = providerUserId;
                RowKey = providerName;
                UserId = userId;
            }
        }
    }
}