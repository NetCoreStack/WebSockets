using Microsoft.Extensions.Options;
using System;

namespace NetCoreStack.WebSockets
{
    internal class DefaultServerInvocatorContextFactory<TInvocator> : IServerInvocatorContextFactory<TInvocator>
        where TInvocator : IServerWebSocketCommandInvocator
    {
        private readonly ServerSocketOptions<TInvocator> _options;

        public DefaultServerInvocatorContextFactory(IOptions<ServerSocketOptions<TInvocator>> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
        }

        public InvocatorContext CreateInvocatorContext()
        {
            return new InvocatorContext(_options.Invocator);
        }
    }
}