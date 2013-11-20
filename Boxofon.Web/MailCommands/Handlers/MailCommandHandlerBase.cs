using System;
using System.Collections.Generic;
using Boxofon.Web.Infrastructure;
using TinyMessenger;

namespace Boxofon.Web.MailCommands.Handlers
{
    public abstract class MailCommandHandlerBase<TMailCommand> : ISubscriber, IDisposable where TMailCommand : class, IMailCommand
    {
        private readonly List<TinyMessageSubscriptionToken> _subscriptionTokens = new List<TinyMessageSubscriptionToken>();

        public void RegisterSubscriptions(ITinyMessengerHub hub)
        {
            _subscriptionTokens.Add(hub.Subscribe<Messages.MailCommand<TMailCommand>>(msg => Handle(msg.Command)));
        }

        public void Dispose()
        {
            foreach (var token in _subscriptionTokens)
            {
                token.Dispose();
            }
        }

        protected abstract void Handle(TMailCommand command);
    }
}