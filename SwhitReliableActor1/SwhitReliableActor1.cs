using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using SwhitReliableActor1.Interfaces;

namespace SwhitReliableActor1
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class SwhitReliableActor1 : Actor, ISwhitReliableActor1
    {
        /// <summary>
        /// Initializes a new instance of SwhitReliableActor1
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public SwhitReliableActor1(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activating....");
            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            const int initialValue = 0;
            var added = await this.StateManager.TryAddStateAsync("count", initialValue);
            ActorEventSource.Current.ActorMessage(this,$"attempt to add actor with state-name 'count' of initial value [{initialValue}] returned {added}");
            if (!added)
            {
                //var preExistingValue = await ISwhitReliableActor1.GetCountAsync(CancellationToken.None);
                var preExistingValue = await ((ISwhitReliableActor1) this).GetCountAsync(CancellationToken.None);
                ActorEventSource.Current.ActorMessage(this, $"initial-value-add failure indicates pre-existing value exists. That value is {preExistingValue}");
            }
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
        }

        protected override async Task OnDeactivateAsync()
        {
            const int finalValue = 2112;
            ActorEventSource.Current.ActorMessage(this, "Actor deactivating....");
            ActorEventSource.Current.ActorMessage(this, $"DEACTIVATION - BEGIN attempt to save actor state with state-name 'count' of final value [{finalValue}]...");
            await this.StateManager.SetStateAsync("count", finalValue);
            ActorEventSource.Current.ActorMessage(this, $"DEACTIVATION -   END attempt to save actor state with state-name 'count' of final value [{finalValue}] - no exceptions thrown.");
            ActorEventSource.Current.ActorMessage(this, "Actor deactivated.");
            return;
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> ISwhitReliableActor1.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task ISwhitReliableActor1.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }
    }
}
