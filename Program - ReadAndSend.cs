using System;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;

namespace EH_Console_Sender
{
    class Program
    {
        private static EventHubClient eventHubClient;
        private const string EventHubConnectionString = //>>>Put your Event Hub Shared Access Key here. It should look like this: "Endpoint=sb://eventhubname.servicebus.windows.net/;SharedAccessKeyName=SASName;SharedAccessKey=qwertyyuiopasdfghjkl123456789c=;EntityPath=eventhubName";
        private const string EventHubName = "arduinoeventhub";
        private static string currentMessage = string.Empty;
        static async Task Main(string[] args)
        {
            int errorCounter = 0;
            var url = "https://api.thingspeak.com/channels/123456789/feeds.json?minutes=1"; //Read the last 1 minute of data doc:https://www.mathworks.com/help/thingspeak/channel-settings.html#keys
            using var client = new HttpClient();

            while (errorCounter <= 120) // Una hora ejecutando sin datos
            {
                var content = await client.GetStringAsync(url);
                if (content.Length > 450) // Menos de 600 caracteres es porque es solo el emcabezado. No hay datos
                    {
                    Console.WriteLine("Message Length: {0}", content.Length);

                    var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
                    {
                        EntityPath = EventHubName
                    };

                    eventHubClient =
                    EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
                    Console.WriteLine("We are about to send a message to event hub");

                    _ = SendMessagesToEventHub(content);

                    Thread.Sleep(30000);
                    errorCounter = 0;
                }
                else
                {
                    errorCounter++;
                    Console.WriteLine("There is no data! -> Length: {0}", content.Length);
                    Console.WriteLine("Error count: {0}",errorCounter);
                    Thread.Sleep(30000);
                }
            }
            
        }
        private static async Task SendMessagesToEventHub(string myMessage)
        {
            if (myMessage is null)
            {
                throw new ArgumentNullException(nameof(myMessage));
            }
            try
            {
                Console.WriteLine($"Sending message: {myMessage}");
                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(myMessage)));
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }
            await Task.Delay(10);
        }


    }
}
