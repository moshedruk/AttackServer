using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Collections;
using System.Net.NetworkInformation;



namespace Attackserver
{
  
    using System.IO;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using System.Net;
    using System.Text.Json;
    using System.Net.WebSockets;
    using System.Threading;    
    //using System.Collections.Concurrent;
    

    
class WebSocketServer
    {
        public static ConcurrentQueue<Item> items = new ConcurrentQueue<Item>();
        public static List<string> messageName = new List<string>();
        public static string _json;
        public class Coordinate
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }
        }

        public class Item
        {
            public string name { get; set; }
            public string speed { get; set; }
            public int muss { get; set; }

            public int time { get; set; }

            public Coordinate origin { get; set; }
            public Coordinate ungle { get; set; }
            public int damage { get; set; }
        }

        public class RootObject
        {
            public List<Item> list { get; set; }
        }
       
        private ConcurrentBag<WebSocket> _webSockets = new ConcurrentBag<WebSocket>();

        public async Task Start(string uriPrefix)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(uriPrefix);
            listener.Start();
            Console.WriteLine("Server started at " + uriPrefix);

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = wsContext.WebSocket;
                    _webSockets.Add(webSocket); // שמור את החיבור ברשימה
                    Console.WriteLine("WebSocket connection established");

                    _ = HandleWebSocketConnection(webSocket); // התחל את טיפול בהודעות
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _webSockets.TryTake(out _); // הסר את החיבור מהרשימה
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received: {message}");                   

                   
                        byte[] response = Encoding.UTF8.GetBytes($"");
                        //Console.WriteLine($"Sending: {Encoding.UTF8.GetString(response)}"); // הדפסת ההודעה שנשלחת
                        await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                    
                    _json = message;

                    Processmessage(_json);

                }
            }
            
        }
        private void Processmessage(string json)
        {
            // כאן ניתן לעבד את ה-JSON שלך
            Dictionary<string, int> mapdamage = new Dictionary<string, int>
        {
            { "Tomahawk", 200 },
            { "Scud", 10 },
            { "Minuteman", 150 }
        };

            // המרה לאובייקט
            Item result = JsonConvert.DeserializeObject<Item>(json);



                result.damage = mapdamage.ContainsKey(result.name) ? mapdamage[result.name] : 0;
                Console.WriteLine($"Name: {result.name}");
                Console.WriteLine($"Speed: {result.speed}");
                Console.WriteLine($"Muss: {result.muss}");
                Console.WriteLine($"Origin: x={result.origin.x}, y={result.origin.y}, z={result.origin.z}");
                Console.WriteLine($"Ungle: x={result.ungle.x}, y={result.ungle.y}, z={result.ungle.z}");
                Console.WriteLine($"Damage: {result.damage}");
            
                items.Enqueue( result );
                

        }

        public async Task SendMissileListToAllClients(RootObject result)
        {
            string json = JsonConvert.SerializeObject(result);
            await SendMessageToAllClients(json);
        }

        public async Task SendMessageToAllClients(string message)
        {
            byte[] response = Encoding.UTF8.GetBytes(message);
            Console.WriteLine($"Sending to all clients: {message}"); // הדפסת ההודעה שנשלחת לכל הלקוחות

            foreach (var webSocket in _webSockets)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending message: {ex.Message}");
                        _webSockets.TryTake(out _); // הסר את החיבור במקרה של שגיאה
                    }
                }
            }
        }
        public static int RandomMissil() 
        {
            var random = new Random();
          
            return random.Next(0, 2);
        }
        public static async Task<Item> MissileManager() 
        {
           await Task.Delay(20000);
            if(items.TryDequeue(out Item result)) 
            {
                return result;
            }
            Console.WriteLine(result.ToString());

            return result;
        }   

        public static void Main(string[] args)
        {
            

            WebSocketServer server = new WebSocketServer();
            Task serverTask = server.Start("http://localhost:5064/");

            // תהליך לשליחת הודעה מהקונסול
            Task.Run(async () =>
            {
                while (true)
                {
                    if (items.Count > 0) 
                    {

                        Item i =  await MissileManager();
                        if (RandomMissil() == 0)
                        {
                            string message = $"The missile :{i.name} fell"; // קלט מהקונסול
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                await server.SendMessageToAllClients(message);

                            }
                        }
                        else 
                        {                            
                            string message = $"The missile :{i.name} is intercepted"; // קלט מהקונסול
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                            await server.SendMessageToAllClients(message);
                        
                            }
                        }
                    }
                }
            }).Wait();

            serverTask.Wait();
            
        }
    }

}


   







