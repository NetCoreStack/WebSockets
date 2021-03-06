﻿using NetCoreStack.WebSockets.Internal;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets
{
    public interface IConnectionManager
    {
        ConcurrentDictionary<string, WebSocketTransport> Connections { get; }

        Task ConnectAsync(WebSocket webSocket, string connectionId, string connectorName = "", CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Text message broadcaster
        /// </summary>
        /// <param name="context">Data</param>
        /// <returns></returns>
        Task BroadcastAsync(WebSocketMessageContext context);

        /// <summary>
        /// Text message broadcaster
        /// </summary>
        /// <param name="inputs">Byte content</param>
        /// <returns></returns>
        Task BroadcastAsync(byte[] inputs);

        /// <summary>
        /// Binary message broadcaster
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="properties">Extra properties, header</param>
        /// <returns></returns>
        Task BroadcastBinaryAsync(byte[] input, IDictionary<string, object> properties = null);

        /// <summary>
        /// Binary message broadcaster with inline serializer
        /// </summary>
        /// <param name="context">WebSocket message context</param>
        /// <returns></returns>
        Task BroadcastBinaryAsync(WebSocketMessageContext context);

        /// <summary>
        /// Send text message to specified connection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task SendAsync(string connectionId, WebSocketMessageContext context);

        /// <summary>
        /// Send binary message to specified connection
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="input">Data</param>
        /// <param name="properties">Additional transport data</param>
        /// <param name="compression">Compression status of the data, default value is true</param>
        /// <returns></returns>
        Task SendBinaryAsync(string connectionId, byte[] input, IDictionary<string, object> properties);

        /// <summary>
        /// Close the specified connection
        /// </summary>
        /// <param name="connectionId"></param>
        void CloseConnection(string connectionId);
    }

    public interface IConnectionManager<TInvocator> where TInvocator : IServerWebSocketCommandInvocator
    {

    }
}