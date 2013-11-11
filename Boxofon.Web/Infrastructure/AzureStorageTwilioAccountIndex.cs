using System;
using System.Web.Configuration;
using Boxofon.Web.Indexes;
using Boxofon.Web.Messages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TinyMessenger;

namespace Boxofon.Web.Infrastructure
{
    public class AzureStorageTwilioAccountIndex : ITwilioAccountIndex, IRequireInitialization, ISubscriber
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageTwilioAccountIndex()
        {
            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        public void RegisterSubscriptions(ITinyMessengerHub hub)
        {
            hub.Subscribe<LinkedTwilioAccountToUser>(msg => AddTwilioAccount(msg.TwilioAccountSid, msg.UserId));
            hub.Subscribe<UnlinkedTwilioAccountFromUser>(msg => RemoveTwilioAccount(msg.TwilioAccountSid, msg.UserId));
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("TwilioAccounts");
        }

        public Guid? GetBoxofonUserId(string twilioAccountSid)
        {
            var op = TableOperation.Retrieve<TwilioAccountEntity>(twilioAccountSid, twilioAccountSid);
            var result = Table().Execute(op);
            return result.Result == null ? (Guid?)null : ((TwilioAccountEntity)result.Result).UserId;
        }

        protected void AddTwilioAccount(string twilioAccountSid, Guid userId)
        {
            var entity = new TwilioAccountEntity(twilioAccountSid, userId);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);
        }

        protected void RemoveTwilioAccount(string twilioAccountSid, Guid userId)
        {
            var table = Table();
            var retrieveOp = TableOperation.Retrieve<TwilioAccountEntity>(twilioAccountSid, twilioAccountSid);
            var retrieveResult = table.Execute(retrieveOp);
            var entity = (TwilioAccountEntity)retrieveResult.Result;
            if (entity != null)
            {
                var deleteOp = TableOperation.Delete(entity);
                table.Execute(deleteOp);
            }
        }

        public class TwilioAccountEntity : TableEntity
        {
            public string TwilioAccountSid { get { return PartitionKey; } }
            public Guid UserId { get; set; }

            public TwilioAccountEntity()
            {
            }

            public TwilioAccountEntity(string twilioAccountSid, Guid userId)
            {
                PartitionKey = twilioAccountSid;
                RowKey = twilioAccountSid;
                UserId = userId;
            }
        }
    }
}