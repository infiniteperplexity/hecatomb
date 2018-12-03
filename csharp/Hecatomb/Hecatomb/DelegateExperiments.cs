using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
        public S Get<S>() where S : T
        {
            return (S) dict[typeof(S)];
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
    public delegate void OnSpawn(SpawnEvent e);
    public delegate void OnSpawn2(SpawnEvent e);

    class DelegateExperiments
    {
        public DelegateExperiments()
        {
            TypeDictionary<Delegate> testing = new TypeDictionary<Delegate>();
            OnSpawn e = (SpawnEvent se) => { Debug.WriteLine("testing"); };
            //OnSpawn2 e2 = (OnSpawn2) e;
            testing.Set(e);
            OnSpawn f = testing.Get<OnSpawn>();
            //testing.Set( (SpawnEvent e)=>{ }); 
        }
    }
}
