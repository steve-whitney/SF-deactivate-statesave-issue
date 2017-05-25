using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using SwhitReliableActor1.Interfaces;

namespace SwhitReliableActor1
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see https://aka.ms/servicefabricactorsplatform

                ActorRuntime.RegisterActorAsync<SwhitReliableActor1>(
                   (context, actorType) => new ActorService(context, actorType, settings: debugActorServiceSettings)).GetAwaiter().GetResult();

                Thread.Sleep(TimeSpan.FromSeconds(5));
                Program.Client();
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static ActorServiceSettings defaultActorServiceSettings => new ActorServiceSettings();
        private static ActorServiceSettings debugActorServiceSettings => new ActorServiceSettings()
        {
            ActorGarbageCollectionSettings =
                                    new ActorGarbageCollectionSettings(10, 1)
        };

        private static async Task Client()
        {
            while (true)
            {
                ISwhitReliableActor1 actor = ActorProxy.Create<ISwhitReliableActor1>(new ActorId("ou812"),
                    "fabric:/SwhitSfApp1");
                int v1 = await actor.GetCountAsync(CancellationToken.None);
                ActorEventSource.Current.Message($"START actor interactions    - v1={v1}");
                await actor.SetCountAsync((v1 + 17), CancellationToken.None);
                int v2 = await actor.GetCountAsync(CancellationToken.None);
                ActorEventSource.Current.Message($"END   actor interactions    - v2={v2}");
                ActorEventSource.Current.Message("START wait for deactivation...");
                await Task.Delay(TimeSpan.FromSeconds(12));
                ActorEventSource.Current.Message("END  wait for deactivation.");
            }
        }


    }
}
