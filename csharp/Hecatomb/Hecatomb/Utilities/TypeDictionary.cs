﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    public class TypeDictionary<T>
    {
        Dictionary<Type, T> dict;

        public TypeDictionary()
        {
            dict = new Dictionary<Type, T>();
        }

        public int Count
        {
            get { return dict.Count(); }
        }
        public Dictionary<Type, T>.KeyCollection Keys
        {
            get { return dict.Keys; }
        }
        public Dictionary<Type, T>.ValueCollection Values
        {
            get { return dict.Values; }
        }
        public void Add(T value)
        {
            dict.Add(typeof(T), value);
        }
        public bool Remove(Type key)
        {
            return dict.Remove(key);
        }
        public bool Remove<S>() where S : T
        {
            return dict.Remove(typeof(S));
        }
        public void Clear()
        {
            dict.Clear();
        }
        public bool ContainsValue(T value)
        {
            return dict.ContainsValue(value);
        }
        public bool ContainsKey(Type t)
        {
            return dict.ContainsKey(t);
        }
        public bool ContainsKey<S>() where S : T
        {
            return dict.ContainsKey(typeof(S));
        }
        public void Set(T value)
        {
            dict[value.GetType()] = value;
        }
        // use with caution, typically only when advanced implicit casting is needed
        public void Set<S>(S value) where S : T
        {
            dict[typeof(S)] = value;
        }
        public S Get<S>() where S : T
        {
            return (S)dict[typeof(S)];
        }
        public S Find<S>() where S : T
        {
            return Get<S>();
        }
        public T Get(Type t)
        {
            return dict[t];
        }
        public T Find(Type t)
        {
            return Get(t);
        }
    }
}
