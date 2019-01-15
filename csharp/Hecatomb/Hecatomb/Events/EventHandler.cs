/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/28/2018
 * Time: 9:54 AM
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace Hecatomb
{
    public class EventHandler
    {
        public Dictionary<string, Dictionary<int, Func<GameEvent, GameEvent>>> ListenerTypes;

            public EventHandler()
            {
                ListenerTypes = new Dictionary<string, Dictionary<int, Func<GameEvent, GameEvent>>>();
                var events = typeof(Game).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(GameEvent))).ToList();
                foreach (var e in events)
                {
                    ListenerTypes[e.Name] = new Dictionary<int, Func<GameEvent, GameEvent>>();
                }

            }

            public void Subscribe(Type t, Entity g, Func<GameEvent, GameEvent> f, float priority = 0)
            {
            if (!g.Spawned)
            {
                throw new InvalidOperationException(String.Format("Unspawned {0} cannot listen to events", g));
            }
            var listeners = ListenerTypes[t.Name];
            if (!listeners.ContainsKey(g.EID))
            {
                listeners[g.EID] = f;
            }
        }

        public void Subscribe<T>(Entity g, Func<GameEvent, GameEvent> f, float priority = 0)
        {
            if (!g.Spawned)
            {
                throw new InvalidOperationException(String.Format("Unspawned {0} cannot listen to events", g));
            }
            else
            {
                var listeners = ListenerTypes[typeof(T).Name];
                if (!listeners.ContainsKey(g.EID))
                {
                    listeners[g.EID] = f;
                }
            }
        }


        public void Unsubscribe<T>(Entity g)
        {
            var listeners = ListenerTypes[typeof(T).Name];
            if (listeners.ContainsKey(g.EID))
            {
                listeners.Remove(g.EID);
            }
        }

        public void UnsubscribeAll(Entity g)
        {
            foreach (var listeners in ListenerTypes.Values)
            {
                if (listeners.ContainsKey(g.EID))
                {
                    listeners.Remove(g.EID);
                }
            }
        }

        public void Publish(GameEvent g)
        {
            var listeners = ListenerTypes[g.GetType().Name];
            foreach (var listener in listeners.Values.ToList())
            {
                g = listener(g);
            }
            g.Fire();
        }

        public Dictionary<string, Dictionary<int, string>> StringifyListeners()
        {
            var jsonready = new Dictionary<string, Dictionary<int, string>>();
            foreach (var type in ListenerTypes.Keys)
            {
                jsonready[type] = new Dictionary<int, string>();
                foreach (var eid in ListenerTypes[type].Keys)
                {
                    jsonready[type][eid] = ListenerTypes[type][eid].Method.Name;
                }
            }
            jsonready["GameEvent"] = new Dictionary<int, string>();
            return jsonready;
        }
    }
}