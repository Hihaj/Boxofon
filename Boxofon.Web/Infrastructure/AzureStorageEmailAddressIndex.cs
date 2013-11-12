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
    public class AzureStorageEmailAddressIndex : IEmailAddressIndex, IRequireInitialization, ISubscriber, IDisposable
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly List<TinyMessageSubscriptionToken> _subscriptionTokens = new List<TinyMessageSubscriptionToken>();

        public AzureStorageEmailAddressIndex()
        {
            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        public void RegisterSubscriptions(ITinyMessengerHub hub)
        {
            _subscriptionTokens.Add(hub.Subscribe<AddedEmailToUser>(msg => AddEmailAddress(msg.Email, msg.UserId)));
            _subscriptionTokens.Add(hub.Subscribe<RemovedEmailFromUser>(msg => RemoveEmailAddress(msg.Email, msg.UserId)));
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
            return _storageAccount.CreateCloudTableClient().GetTableReference("EmailAddresses");
        }

        public Guid? GetBoxofonUserId(string email)
        {
            var op = TableOperation.Retrieve<EmailAddressEntity>(email, email);
            var result = Table().Execute(op);
            return result.Result == null ? (Guid?)null : ((EmailAddressEntity)result.Result).UserId;
        }

        protected void AddEmailAddress(string email, Guid userId)
        {
            var entity = new EmailAddressEntity(email, userId);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);
        }

        protected void RemoveEmailAddress(string email, Guid userId)
        {
            var table = Table();
            var retrieveOp = TableOperation.Retrieve<EmailAddressEntity>(email, email);
            var retrieveResult = table.Execute(retrieveOp);
            var entity = (EmailAddressEntity)retrieveResult.Result;
            if (entity != null)
            {
                var deleteOp = TableOperation.Delete(entity);
                table.Execute(deleteOp);
            }
        }

        public class EmailAddressEntity : TableEntity
        {
            public string Email { get { return PartitionKey; } }
            public Guid UserId { get; set; }

            public EmailAddressEntity()
            {
            }

            public EmailAddressEntity(string email, Guid userId)
            {
                PartitionKey = email;
                RowKey = email;
                UserId = userId;
            }
        }
    }
}