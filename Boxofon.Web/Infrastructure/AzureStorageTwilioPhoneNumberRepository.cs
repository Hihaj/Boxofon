using System;
using System.Web.Configuration;
using Boxofon.Web.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ServiceStack.Text;

namespace Boxofon.Web.Infrastructure
{
    public class AzureStorageTwilioPhoneNumberRepository : ITwilioPhoneNumberRepository, IRequireInitialization
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageTwilioPhoneNumberRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("TwilioPhoneNumbers");
        }

        public TwilioPhoneNumber GetByPhoneNumber(string phoneNumber)
        {
            var op = TableOperation.Retrieve<TwilioPhoneNumberEntity>(phoneNumber, phoneNumber);
            var result = Table().Execute(op);
            return result.Result == null ? null : ((TwilioPhoneNumberEntity)result.Result).ToTwilioPhoneNumber();
        }

        public void Save(TwilioPhoneNumber phoneNumber)
        {
            var entity = new TwilioPhoneNumberEntity(phoneNumber);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);
        }

        public class TwilioPhoneNumberEntity : TableEntity
        {
            public string PhoneNumber { get { return PartitionKey; } }
            public string FriendlyName { get; set; }
            public string TwilioAccountSid { get; set; }
            public Guid UserId { get; set; }

            public TwilioPhoneNumberEntity()
            {
            }

            public TwilioPhoneNumberEntity(TwilioPhoneNumber phoneNumber)
            {
                PartitionKey = phoneNumber.PhoneNumber;
                RowKey = phoneNumber.PhoneNumber;
                FriendlyName = phoneNumber.FriendlyName;
                TwilioAccountSid = phoneNumber.TwilioAccountSid;
                UserId = phoneNumber.UserId;
            }

            public TwilioPhoneNumber ToTwilioPhoneNumber()
            {
                return new TwilioPhoneNumber
                {
                    PhoneNumber = PhoneNumber,
                    FriendlyName = FriendlyName,
                    TwilioAccountSid = TwilioAccountSid,
                    UserId = UserId
                };
            }
        }
    }
}