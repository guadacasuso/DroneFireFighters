using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus.Messaging;

using Newtonsoft.Json;

namespace DF.Emulator
{

    class TelemetryMessage
    {

        public string MessageId { get; set; }
        public Drone DroneData { get; set; }

    }
    class MessageSender
    {
        static string eventHubName = "drone-firefighters";
        static string connectionString = ConfigurationManager.AppSettings["ehConnectionString"];
        public static void sendMessage(Drone drone)
        {

            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            TelemetryMessage tmessage = new TelemetryMessage() { DroneData = drone, MessageId = Guid.NewGuid().ToString() };
            string message = JsonConvert.SerializeObject(tmessage);
            try
            {

                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, message);
                eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(message)));
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                Console.ResetColor();
            }

        }

    }
}
