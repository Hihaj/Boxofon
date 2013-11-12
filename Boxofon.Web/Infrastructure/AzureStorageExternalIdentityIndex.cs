using System;
using System.Collections.Generic;
using System.Web.Configuration;
using Boxofon.Web.Indexes;
using Boxofon.Web.Messages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TinyMessenger;

namespace Boxofon.Web.Infrastructure
{
    public class AzureStorageExternalIdentityIndex : IExternalIdentityIndex, IRequireInitialization, ISubscriber, IDisposable
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly List<TinyMessageSubscriptionToken> _subscriptionTokens = new List<TinyMessageSubscriptionToken>();

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
            _subscriptionTokens.Add(hub.Subscribe<UserCreated>(msg => AddExternalIdentity(msg.ExternalIdentity.ProviderName, msg.ExternalIdentity.ProviderUserId, msg.UserId)));
            _subscriptionTokens.Add(hub.Subscribe<AddedExternalIdentityToUser>(msg => AddExternalIdentity(msg.ProviderName, msg.ProviderUserId, msg.UserId)));
            _subscriptionTokens.Add(hub.Subscribe<RemovedExternalIdentityFromUser>(msg => RemoveExternalIdentity(msg.ProviderName, msg.ProviderUserId, msg.UserId)));
        }

        public void Dispose()
        {
            foreach (var token in _subscriptionTokens)
            {
                token.Dispose();
            }
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

        protected void RemoveExternalIdentity(string providerName, string providerUserId, Guid userId)
        {
            var table = Table();
            var retrieveOp = TableOperation.Retrieve<ExternalIdentityEntity>(providerUserId, providerName);
            var retrieveResult = table.Execute(retrieveOp);
            var entity = (ExternalIdentityEntity)retrieveResult.Result;
            if (entity != null)
            {
                var deleteOp = TableOperation.Delete(entity);
                table.Execute(deleteOp);
            }
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