/*
 Generalised event handler class using delegates
 Author: Miltiadis Nedelkos, nedelkosm at gmail com
 Date: May 2017
*/
using System;
using System.Collections.Generic;

public class EventHandler {
    private readonly IDictionary<Type, List<Object>> events = new Dictionary<Type, List<Object>>();

    /// <summary>
    /// Removes all listeners and all registered event types.
    /// </summary>
    internal void Reset() {
        foreach (var key in events.Keys) {
            events[key].Clear();
        }
        events.Clear();
    }

    /// <summary>
    /// Start listening to a specific event.
    /// </summary>
    /// <typeparam name="T">The type of event to subscribe to. Must be a delegate</typeparam>
    /// <param name="listener">The listener that will be called when the event triggers</param>
    public void Subscribe<T>(T listener) where T : MulticastDelegate {
        var key = typeof(T);
        if (!events.ContainsKey(key)) { events.Add(key, new List<System.Object>()); }
        if (!events[key].Contains(listener)) events[key].Add(listener);
    }

    /// <summary>
    /// Stop listening to a specific event.
    /// </summary>
    /// <typeparam name="T">The type of event to subscribe to. Must be a delegate</typeparam>
    /// <param name="listener">The listener</param>
    public void Unsubscribe<T>(T listener) where T : MulticastDelegate {
        var key = typeof(T);
        if (!events.ContainsKey(key)) { return; }
        events[key].Remove(listener);
    }

    /// <summary>
    /// Trigger an event of type T.
    /// Every listener that was added with Subscribe is automatically called with given parameters.
    /// </summary>
    /// <typeparam name="T">The type of event to be called. Must be a delegate</typeparam>
    /// <param name="parameters">Any parameters passed to this certain event. Must match the type of the delegate event.</param>
    internal void Trigger<T>(params object[] parameters) where T : MulticastDelegate {
        if (!events.ContainsKey(typeof(T))) { // Check if event is registered
            System.Diagnostics.Debug.WriteLine("Event " + typeof(T) + " is not registered (no listeners were added yet).");
            // Exception is not thrown because listeners can still be added later.
        } else { // Attempt to trigger event
            foreach (Delegate e in events[typeof(T)]) { // For every listener subscribed to this event
                if (e != null) { // If the listener still exists (Destroyed instances are not automatically unsubscribed. Tip: Add unsubscribe to finalizers
                    try { // Try to invoke listener 
                        e.DynamicInvoke(parameters); // Dynamic invoke automatically matches type
                    } catch {
                        System.Diagnostics.Debug.WriteLine("Could not call event [" + typeof(T) + "] on listener [" + e.GetMethodInfo().Name + "]. Method failed or wrong parameters.");
                    }
                } else { // Instance was destroyed
                    System.Diagnostics.Debug.WriteLine("Removing listener from event [" + typeof(T) + "] because it is null.");
                    events[typeof(T)].Remove(e);
                }
            }
        }
    }
}
