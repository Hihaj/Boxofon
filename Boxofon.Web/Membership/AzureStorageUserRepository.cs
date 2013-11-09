using System;
using System.Web.Configuration;
using Boxofon.Web.Infrastructure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ServiceStack.Text;

namespace Boxofon.Web.Membership
{
    public class AzureStorageUserRepository : IUserRepository, IRequireInitialization
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageUserRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(WebConfigurationManager.AppSettings["azure:StorageConnectionString"]);
        }

        public void Initialize()
        {
            Table().CreateIfNotExists();
        }

        protected CloudTable Table()
        {
            return _storageAccount.CreateCloudTableClient().GetTableReference("Users");
        }

        public User GetById(Guid id)
        {
            var op = TableOperation.Retrieve<UserEntity>(id.ToString(), id.ToString());
            var result = Table().Execute(op);
            return result.Result == null ? null : ((UserEntity)result.Result).GetUser();
        }

        public void Save(User user)
        {
            var entity = new UserEntity(user);
            var op = TableOperation.InsertOrReplace(entity);
            Table().Execute(op);
        }

        public class UserEntity : TableEntity
        {
            public string Data { get; set; }

            public UserEntity()
            {
            }

            public UserEntity(User user)
            {
                PartitionKey = user.Id.ToString();
                RowKey = user.Id.ToString();
                Data = JsonSerializer.SerializeToString(user);
            }

            public User GetUser()
            {
                if (string.IsNullOrEmpty(Data))
                {
                    return null;
                }
                return JsonSerializer.DeserializeFromString<User>(Data);
            }
        }
    }
}