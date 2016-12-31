﻿using IngressServiceAPI.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace IngressServiceAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set this up in your app.config
            string ingressServiceUrl = ConfigurationManager.AppSettings["IngressServiceUrl"];
            string producerToken = ConfigurationManager.AppSettings["ProducerToken"];

            IngressClient client = new IngressClient(ingressServiceUrl, producerToken);

            // Use compression when sending data.  For such small sample messages, compression doesn't 
            // save us much space, but we're doing it here for demonstration sake.
            client.UseCompression = true;


            // 1) Send the Types message
            client.CreateTypes(new string[] { SimpleType.JsonSchema });
            client.CreateTypes(new string[] { ComplexType.JsonSchema });


            // 2) Send the Streams message
            StreamInfo stream1 = new StreamInfo() { Id = "TestStream1", Type = "SimpleType" };
            StreamInfo stream2 = new StreamInfo() { Id = "TestStream2", Type = "SimpleType" };
            StreamInfo stream3 = new StreamInfo() { Id = "TestStream3", Type = "ComplexType" };

            client.CreateStreams(new StreamInfo[] { stream1, stream2, stream3 });


            // 3) Send the Values messages
            StreamValues complexValue = new StreamValues()
            {
                Stream = stream3.Id,
                Values = new List<ComplexType> { ComplexType.CreateSampleValue() }
            };

            client.SendValuesAsync(new StreamValues[] { complexValue }).Wait();

            // Here we loop indefinitely, sending 10 time series events to two streams every second.
            while (true)
            {
                // Create our set of values to send to our streams
                List<SimpleType> values = new List<SimpleType>();
                for(int i = 0; i < 10; i++)
                {
                    values.Add(new SimpleType() { Time = DateTime.UtcNow, Value = i });
                    Thread.Sleep(10);  // Offset the time-stamps by 10 ms
                }

                StreamValues vals1 = new StreamValues() { Stream = stream1.Id, Values = values };
                StreamValues vals2 = new StreamValues() { Stream = stream2.Id, Values = values };

                // Now send them
                client.SendValuesAsync(new StreamValues[] { vals1, vals2 }).Wait();

                Thread.Sleep(1000);
            }
        }
    }
}
