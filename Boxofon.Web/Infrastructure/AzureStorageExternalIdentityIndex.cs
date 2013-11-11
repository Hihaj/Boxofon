using System;
using System.Web.Configuration;
using Boxofon.Web.Indexes;
using Boxofon.Web.Messages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TinyMessenger;

namespace Boxofon.Web.Infrastructure
{
    public class AzureStorageExternalIdentityIndex : IExternalIdentityIndex, IRequireInitialization, ISubscriber
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageExternalIdentityIndex()
        {
            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        public void RegisterSubscriptions(ITinyMessengerHub hub)
        {
            hub.Subscribe<UserCreated>(msg => AddExternalIdentity(msg.ExternalIdentity.ProviderName, msg.ExternalIdentity.ProviderUserId, msg.UserId));
            hub.Subscribe<AddedExternalIdentityToUser>(msg => AddExternalIdentity(msg.ProviderName, msg.ProviderUserId, msg.UserId));
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("ExternalIdentities");
        }

        public Guid? GetBoxofonUserId(string providerName, string providerUserId)
        {
            var op = TableOperation.Retrieve<ExternalIdentityEntity>(providerUserId, providerName);
            var result = Table().Execute(op);
            return result.Result == null ? (Guid?)null : ((ExternalIdentityEntity)result.Result).UserId;
        }

        protected void AddExternalIdentity(string providerName, string providerUserId, Guid userId)
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