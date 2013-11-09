using System;
using System.Web.Configuration;
using Boxofon.Web.Infrastructure;
using Boxofon.Web.Twilio;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using TinyMessenger;

namespace Boxofon.Web.Membership
{
    public class AzureStorageTwilioAccountLookup : TwilioAccountLookupBase, IRequireInitialization
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageTwilioAccountLookup()
        {
            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("TwilioAccounts");
        }

        public override Guid? GetBoxofonUserId(string twilioAccountSid)
        {
            var op = TableOperation.Retrieve<TwilioAccountEntity>(twilioAccountSid, twilioAccountSid);
            var result = Table().Execute(op);
            return result.Result == null ? (Guid?)null : ((TwilioAccountEntity)result.Result).UserId;
        }

        protected override void AddTwilioAccount(string twilioAccountSid, Guid userId)
        {
            var entity = new TwilioAccountEntity(twilioAccountSid, userId);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);
        }

        protected override void RemoveTwilioAccount(string twilioAccountSid, Guid userId)
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