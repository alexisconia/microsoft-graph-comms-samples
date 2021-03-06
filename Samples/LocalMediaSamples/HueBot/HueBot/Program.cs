﻿// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

namespace HueBot
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph.Core.Common;
    using Microsoft.Graph.Core.Telemetry;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// Main entry.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Graph logger instance.
        /// </summary>
        private static readonly GraphLogger Logger = new GraphLogger("Sample.HueBot");

        /// <summary>
        /// Observer subscription.
        /// </summary>
        private static IDisposable subscription = Logger.CreateObserver(OnNext);

        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // Log unhandled exceptions.
                AppDomain.CurrentDomain.UnhandledException += (_, e) => Logger.Error(e.ExceptionObject as Exception, $"Unhandled exception");
                TaskScheduler.UnobservedTaskException += (_, e) => Logger.Error(e.Exception, "Unobserved task exception");

                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.
                ServiceRuntime.RegisterServiceAsync(
                    "HueBotType",
                    context => new HueBot(context, Logger)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(HueBot).Name);

                // Prevents this host process from terminating so services keeps running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Default log event handler.
        /// </summary>
        /// <param name="logEvent">Log event.</param>
        private static void OnNext(LogEvent logEvent)
        {
            var text = $"{logEvent.Component}({logEvent.CallerInfoString}) {logEvent.Timestamp:O}: {logEvent.Message}, Properties: {logEvent.PropertiesString}";
            ServiceEventSource.Current.Message(text);
        }
    }
}
