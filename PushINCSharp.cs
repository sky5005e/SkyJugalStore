using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp.Google;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        string publicToken = "471dbe8ca387b7a5e3067b12ff0d9df4b88f2fe438ece05dd06c27370ba4a9cf";//"2c7c1cf5 1c5cbb6d 469f5b83 91c03d16 cd52fdee c70c698f 5052d6fc d29ffe81";
       // string publicToken = "471dbe8c a387b7a5 e3067b12 ff0d9df4 b88f2fe4 38ece05d d06c2737 0ba4a9cf";
         //string certPath = @"C:\Users\Surendar Yadav\AppData\Roaming\Skype\My Skype Received Files\pushnotificationcertificate2.p12";
         string certPath = @"C:\Users\Surendar Yadav\AppData\Roaming\Skype\My Skype Received Files\ICLDC Push Certificate.p12";
         string certpassword = "abc123!";
        // static string payload = "{\"aps\":{\"alert\":\"Testing.. (100)\",\"badge\":1,\"sound\":\"default\"}}";
         string payload = "{\"aps\":{ \"alert\":{ \"body\": \"Hello, world!\" }, \"badge\": 2  }}";

        int port = 2195;
        string hostname = "gateway.sandbox.push.apple.com";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] certfile = File.ReadAllBytes(certPath);

            // Configuration (NOTE: .pfx can also be used here)
            var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, certfile, certpassword);

            // Create a new broker
            var apnsBroker = new ApnsServiceBroker(config);

            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = (ApnsNotificationException)ex;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        MessageBox.Show($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException			
                        MessageBox.Show($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                MessageBox.Show("Apple Notification Sent!");
            };

            // Start the broker
            apnsBroker.Start();

            //foreach (var deviceToken in MY_DEVICE_TOKENS)
            // {
            // Queue a notification to send
            apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = publicToken,
                Payload = JObject.Parse(payload)// "{\"aps\":{\"badge\":7}}")
                });
            //}

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            apnsBroker.Stop();


        }

        private void button2_Click(object sender, EventArgs e)
        {
            string message = "Hi Sumit, I really appreciate your valuable time";
            string[] DeviceTokens = { "ffPOjYIhrf8:APA91bEfGReNLN33X5e1USC8ZZIvGvfhA0qW3OPsORyPhl5z48N9G9DS-18QU6MGAnBv0Xrin7FeJ0SPZRo1tgj5x2d1A_jm4yIQGvWHSwfyciQqhematIOs_PeOdWUissgwox_ltbFf",
                    "eyFT64TD_ps:APA91bFQqnWxKiwaSkIh4A3FrEtJbcO0bN6NAYNy4bkKMXXiEgdnOEm2ThlLH67bkjLpFiNrIKuiQvD1EUMH1tgk9_O2KwkY0elJ4bHfeB28VvSbmSfRKraGkOF83MGDkvR8E1kjfK8c",
                    "dnrlzZhqozA:APA91bHvkA81l9xgirAJcArQfpeBB5_-OES41O42YLiiySYZaq1jgu4QH6V-t-YdAFPQPUFedfYWSxHXwQwmugt4EHPao1RIwLxSoMkGY124va6UiWdOdcLAQnWN0d4sTR5IL0L0iUuD"
            };
            foreach (var item in DeviceTokens)
            {


                FirebaseExtension.SendPushNotification(item, message);
            }
            /*
            // Configuration
            var config = new  GcmConfiguration("GCM-SENDER-ID", "AUTH-TOKEN", null);

            // Create a new broker
            var gcmBroker = new GcmServiceBroker(config);

            // Wire up events
            gcmBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle(ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is GcmNotificationException)
                    {
                        var notificationException = (GcmNotificationException)ex;

                        // Deal with the failed notification
                        var gcmNotification = notificationException.Notification;
                        var description = notificationException.Description;

                        Console.WriteLine($"GCM Notification Failed: ID={gcmNotification.MessageId}, Desc={description}");
                    }
                    else if (ex is GcmMulticastResultException)
                    {
                        var multicastException = (GcmMulticastResultException)ex;

                        foreach (var succeededNotification in multicastException.Succeeded)
                        {
                            Console.WriteLine($"GCM Notification Succeeded: ID={succeededNotification.MessageId}");
                        }

                        foreach (var failedKvp in multicastException.Failed)
                        {
                            var n = failedKvp.Key;
                           // var e = failedKvp.Value;

                           // Console.WriteLine($"GCM Notification Failed: ID={n.MessageId}, Desc={e.Description}");
                        }

                    }
                    else if (ex is DeviceSubscriptionExpiredException)
                    {
                        var expiredException = (DeviceSubscriptionExpiredException)ex;

                        var oldId = expiredException.OldSubscriptionId;
                        var newId = expiredException.NewSubscriptionId;

                        Console.WriteLine($"Device RegistrationId Expired: {oldId}");

                        if (!string.IsNullOrWhiteSpace(newId))
                        {
                            // If this value isn't null, our subscription changed and we should update our database
                            Console.WriteLine($"Device RegistrationId Changed To: {newId}");
                        }
                    }
                    else if (ex is RetryAfterException)
                    {
                        var retryException = (RetryAfterException)ex;
                        // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                        Console.WriteLine($"GCM Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
                    }
                    else
                    {
                        Console.WriteLine("GCM Notification Failed for some unknown reason");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            gcmBroker.OnNotificationSucceeded += (notification) => {
                Console.WriteLine("GCM Notification Sent!");
            };

            // Start the broker
            gcmBroker.Start();

            //foreach (var regId in MY_REGISTRATION_IDS)
            // {
            var regId = "";
                // Queue a notification to send
                gcmBroker.QueueNotification(new GcmNotification
                {
                    RegistrationIds = new List<string> {
            regId
        },
                    Data = JObject.Parse("{ \"Hello\" : \"Hicom Hello\" }")
                });
            //}

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            gcmBroker.Stop();
            */
        }
    }

    public class FirebaseExtension
    {
        private readonly string _message;
        private readonly List<object> _tokenList;

        public FirebaseExtension(List<object> tokenList, string Message)
        {
            _tokenList = tokenList;
            _message = Message;
            var thread = new Thread(Autofunc);
            thread.Start();
        }

        public void Autofunc()
        {
            //int q = tokenList.Count/100;
            //int r = tokenList.Count%100;
            //for (int i = 1; i <= q+r; i++)
            //{

            //}

            foreach (string token in _tokenList)
            {
                SendPushNotification(token, _message);
            }
        }

        public static void SendPushNotification(string token, string message)
        {
            try
            {
                var serverKey = "AAAAr0V7SL4:APA91bHAaq_wU5eWUOXNtQGc9DEQ-QD-fVp4hcpV-AAxfV0zN6Z8_HbmuOsKrBw3nLjDHQCPCbnQ6DSpT_aFXp_3OIHLbNipU6AT0yNhG3yudH0Bde2Q2zfqXy5GCJmF127k0ap1lefp"; //Constants.ServerKey;
                var senderId = "752784984254";//Constants.SenderId;
                var deviceId = token;//"eyFT64TD_ps:APA91bFQqnWxKiwaSkIh4A3FrEtJbcO0bN6NAYNy4bkKMXXiEgdnOEm2ThlLH67bkjLpFiNrIKuiQvD1EUMH1tgk9_O2KwkY0elJ4bHfeB28VvSbmSfRKraGkOF83MGDkvR8E1kjfK8c";//token;
                var tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                var data = new
                {
                    to = deviceId,
                    notification = new
                    {
                        body = message,
                        title = "Hicom",
                        sound = "Enabled"
                    }
                };

                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(data);
                var byteArray = Encoding.UTF8.GetBytes(json);
                tRequest.Headers.Add($"Authorization: key={serverKey}");
                tRequest.Headers.Add($"Sender: id={senderId}");
                tRequest.ContentLength = byteArray.Length;
                using (var dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (var tResponse = tRequest.GetResponse())
                    {
                        using (var dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (var tReader = new StreamReader(dataStreamResponse))
                            {
                                var sResponseFromServer = tReader.ReadToEnd();
                                var str = sResponseFromServer;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //new ExceptionExtension(exception);
            }
        }

        public static void SendPushNotificationIOS(string token, string message)
        {
            try
            {
                var serverKey = "AAAAr0V7SL4:APA91bHAaq_wU5eWUOXNtQGc9DEQ-QD-fVp4hcpV-AAxfV0zN6Z8_HbmuOsKrBw3nLjDHQCPCbnQ6DSpT_aFXp_3OIHLbNipU6AT0yNhG3yudH0Bde2Q2zfqXy5GCJmF127k0ap1lefp"; //Constants.ServerKey;
                var senderId = "752784984254";//Constants.SenderId;
                var deviceId = token;//"eyFT64TD_ps:APA91bFQqnWxKiwaSkIh4A3FrEtJbcO0bN6NAYNy4bkKMXXiEgdnOEm2ThlLH67bkjLpFiNrIKuiQvD1EUMH1tgk9_O2KwkY0elJ4bHfeB28VvSbmSfRKraGkOF83MGDkvR8E1kjfK8c";//token;
                var tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                var data = new
                {
                    to = deviceId,
                    notification = new
                    {
                        body = message,
                        title = "Hicom",
                        sound = "Enabled"
                    }
                };

                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(data);
                var byteArray = Encoding.UTF8.GetBytes(json);
                tRequest.Headers.Add($"Authorization: key={serverKey}");
                tRequest.Headers.Add($"Sender: id={senderId}");
                tRequest.ContentLength = byteArray.Length;
                using (var dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (var tResponse = tRequest.GetResponse())
                    {
                        using (var dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (var tReader = new StreamReader(dataStreamResponse))
                            {
                                var sResponseFromServer = tReader.ReadToEnd();
                                var str = sResponseFromServer;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //new ExceptionExtension(exception);
            }
        }
    }
}
