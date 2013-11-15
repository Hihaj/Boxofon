using System;
using System.IO;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Mailgun;
using Boxofon.Web.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Boxofon.Web.Infrastructure
{
    public class EmaillVerificationService : IEmailVerificationService, IRequireInitialization
    {
        private static readonly Random Random = new Random();
        private readonly CloudStorageAccount _storageAccount;
        private readonly IMailgunClient _mailgunClient;

        public EmaillVerificationService(IMailgunClient mailgunClient)
        {
            if (mailgunClient == null)
            {
                throw new ArgumentNullException("mailgunClient");
            }
            _mailgunClient = mailgunClient;

            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("EmailVerifications");
        }

        public void BeginVerification(Guid userId, string email)
        {
            var code = GenerateCode();
            var entity = new VerificationEntity(userId, email, code);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);

            _mailgunClient.SendNoReplyMessage(email, "Verifiering av e-postadress", string.Format("Din verifieringskod: <strong>{0}</strong>", code));
        }

        public bool TryCompleteVerification(Guid userId, string email, string code)
        {
            if (string.IsNullOrEmpty(code) || !email.IsValidEmail())
            {
                return false;
            }
            var op = TableOperation.Retrieve<VerificationEntity>(userId.ToString(), email);
            var entity = (VerificationEntity)Table().Execute(op).Result;
            if (entity != null && entity.Code == code)
            {
                var deleteOp = TableOperation.Delete(entity);
                Table().ExecuteAsync(deleteOp);
                return true;
            }
            return false;
        }

        private static string GenerateCode()
        {
            return Random.Next(1, 999999).ToString("000000");
        }

        public class VerificationEntity : TableEntity
        {
            public string Code { get; set; }

            public VerificationEntity()
            {
            }

            public VerificationEntity(Guid userId, string email, string code)
            {
                PartitionKey = userId.ToString();
                RowKey = email;
                Code = code;
            }
        }
    }
}