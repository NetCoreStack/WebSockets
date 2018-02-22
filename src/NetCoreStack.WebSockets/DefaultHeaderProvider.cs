using NetCoreStack.WebSockets.Internal;
using System.Collections.Generic;

namespace NetCoreStack.WebSockets
{
    public class DefaultHeaderProvider : IHeaderProvider
    {
        public void Invoke(IDictionary<string, object> header)
        {
            if (header == null)
            {
                return;
            }

            if (!header.TryGetValue(NCSConstants.WSFQN, out object host))
            {
                header.Add(NCSConstants.WSFQN, FQNHelper.Name);
            }
        }
    }
}
