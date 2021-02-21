﻿using System;
using System.Threading.Tasks;
using Disqord.Serialization.Json;

namespace Disqord.Gateway.Default.Dispatcher
{
    public abstract class Handler<TModel, TEventArgs> : Handler<TEventArgs>
        where TModel : JsonModel
        where TEventArgs : EventArgs
    {
        protected Handler()
        { }

        public override async Task HandleDispatchAsync(IJsonToken data)
        {
            var model = data.ToType<TModel>();
            var task = HandleDispatchAsync(model);
            if (task == null)
                throw new InvalidOperationException($"The dispatch handler {GetType()} returned a null handle task.");

            var eventArgs = await task.ConfigureAwait(false);
            if (eventArgs == null || eventArgs == EventArgs.Empty)
                return;

            if (Event != null)
            {
                // This is the case for most handlers - the dispatch maps to a single event.
                await Event.InvokeAsync(Dispatcher, eventArgs).ConfigureAwait(false);
            }
            else
            {
                // The dispatch maps to multiple events. We get the event for the type of the event args.
                if (!_events.TryGetValue(eventArgs.GetType(), out var @event))
                    throw new InvalidOperationException($"The dispatch handler {GetType()} returned an invalid instance of event args: {eventArgs.GetType()}.");

                await @event.InvokeAsync(Dispatcher, eventArgs).ConfigureAwait(false);
            }
        }

        public abstract Task<TEventArgs> HandleDispatchAsync(TModel model);
    }
}