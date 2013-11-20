using System;
using Boxofon.Web.Model;

namespace Boxofon.Web.MailCommands
{
    public interface IMailCommand
    {
        Guid UserId { get; set; }
        string BoxofonNumber { get; set; }
    }
}