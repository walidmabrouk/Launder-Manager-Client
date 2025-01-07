using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LaunderManagerClient.Entities;
using LaunderManagerClient.Converters;
using LaunderManagerClient.Entities;

namespace LaverieClient
{
    public class WebSocketManager
    {
        private readonly string _serverUrl;
        private readonly ClientWebSocket _client;

        public WebSocketManager(string serverUrl)
        {
            _serverUrl = serverUrl;
            _client = new ClientWebSocket();
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(new Uri(_serverUrl), CancellationToken.None);
            Console.WriteLine("Connexion WebSocket établie.");
        }

        public async Task SendAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Message envoyé.");
        }

        public async Task<List<Proprietor>> ReceiveAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new TimeSpanConverter() }
            };

            try
            {
                while (_client.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await _client.ReceiveAsync(buffer, CancellationToken.None);
                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                        } while (!result.EndOfMessage);

                        var message = Encoding.UTF8.GetString(ms.ToArray());

                        var proprietors = JsonSerializer.Deserialize<List<Proprietor>>(message, options);
                        return proprietors ?? new List<Proprietor>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] An error occurred in ReceiveAsync: {ex.Message}");
                Console.ResetColor();
            }

            return new List<Proprietor>(); // Return an empty list if there's an error
        }


        public async Task DisconnectAsync()
        {
            if (_client.State == WebSocketState.Open)
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Déconnexion demandée", CancellationToken.None);
                Console.WriteLine("Connexion WebSocket fermée.");
            }
        }
    }
}