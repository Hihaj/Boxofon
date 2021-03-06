﻿using System;
using System.IO;
using System.Web.Configuration;
using Boxofon.Web.Helpers;
using Boxofon.Web.Model;
using Boxofon.Web.Services;
using Boxofon.Web.Twilio;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Boxofon.Web.Infrastructure
{
    public class PhoneNumberVerificationService : IPhoneNumberVerificationService, IRequireInitialization
    {
        private static readonly Random Random = new Random();
        private readonly CloudStorageAccount _storageAccount;
        private readonly ITwilioClientFactory _twilioClientFactory;

        public PhoneNumberVerificationService(ITwilioClientFactory twilioClientFactory)
        {
            if (twilioClientFactory == null)
            {
                throw new ArgumentNullException("twilioClientFactory");
            }
            _twilioClientFactory = twilioClientFactory;

            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("PhoneNumberVerifications");
        }

        public void BeginVerification(User user, string phoneNumber)
        {
            if (string.IsNullOrEmpty(user.TwilioPhoneNumber))
            {
                throw new ArgumentException("The user must have a Twilio phone number.");
            }
            phoneNumber = phoneNumber.ToE164();
            var code = GenerateCode();
            var entity = new VerificationEntity(user.Id, phoneNumber, code);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);

            var twilio = _twilioClientFactory.GetClientForUser(user);
            twilio.SendSmsMessage(user.TwilioPhoneNumber, phoneNumber, code);
        }

        public bool TryCompleteVerification(User user, string phoneNumber, string code)
        {
            if (string.IsNullOrEmpty(code) || !phoneNumber.IsPossiblyValidPhoneNumber())
            {
                return false;
            }
            phoneNumber = phoneNumber.ToE164();
            var op = TableOperation.Retrieve<VerificationEntity>(user.Id.ToString(), phoneNumber);
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

            public VerificationEntity(Guid userId, string phoneNumber, string code)
            {
                PartitionKey = userId.ToString();
                RowKey = phoneNumber;
                Code = code;
            }
        }
    }
}