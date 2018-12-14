using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hecatomb
{
    public class Item : TileEntity
    {

        public bool Owned;
        public Dictionary<string, int> Resources;
        public Dictionary<string, int> Claims;

        public Item() : base()
        {
            TypeName = "Item";
            Resources = new Dictionary<string, int>();
            Claims = new Dictionary<string, int>();
        }

        public ValueTuple<char, string, string> GetGlyph()
        {
            List<string> ordered = Resources.Keys.OrderByDescending(k => Resources[k]).ThenBy(k => k).ToList();
            // this is done by small letter...urgh...
            Resource resource = Resource.Types[ordered[0]];
            return (resource.Symbol, resource.FG, resource.BG);
        }

        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            Item e = Game.World.Items[x1, y1, z1];
            if (e == null)
            {
                Game.World.Items[x1, y1, z1] = this;
                base.Place(x1, y1, z1, fireEvent);
            }
            else
            {
                throw new InvalidOperationException(String.Format(
                    "Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
                ));
            }
        }
        public override void Remove()
        {
            int x0 = X;
            int y0 = Y;
            int z0 = Z;
            base.Remove();
            Game.World.Items[x0, y0, z0] = null;
        }

        public void AddResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                AddResource((resource, resources[resource]));
            }
        }
        public void AddResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                AddResource(tuple);
            }
        }
        public void AddResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                AddResource(tuple);
            }
        }
        public void AddResource(ValueTuple<string, int> resource)
        {
            if (!Resources.ContainsKey(resource.Item1))
            {
                Resources[resource.Item1] = 0;
            }
            Resources[resource.Item1] += resource.Item2;
        }

        public void RemoveResource(ValueTuple<string, int> resource)
        {
            if (!Resources.ContainsKey(resource.Item1))
            {
                throw new InvalidOperationException();
            }
            else if (Resources[resource.Item1] < resource.Item2)
            {
                throw new InvalidOperationException();
            }
            else
            {
                Resources[resource.Item1] -= resource.Item2;
                if (Resources[resource.Item1] == 0)
                {
                    Resources.Remove(resource.Item1);
                }
                if (Resources.Keys.Count == 0)
                {
                    Despawn();
                }
            }
        }
        public void RemoveResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                RemoveResource(tuple);
            }
        }
        public void RemoveResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                RemoveResource(tuple);
            }
        }
        public void RemoveResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                RemoveResource((resource, resources[resource]));
            }
        }

        public void UnclaimResource(ValueTuple<string, int> resource)
        {
            if (!Claims.ContainsKey(resource.Item1))
            {
                throw new InvalidOperationException();
            }
            else if (Claims[resource.Item1] < resource.Item2)
            {
                throw new InvalidOperationException();
            }
            else
            {
                Claims[resource.Item1] -= resource.Item2;
                if (Claims[resource.Item1] == 0)
                {
                    Claims.Remove(resource.Item1);
                }
            }
        }
        public void UnclaimResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                UnclaimResource(tuple);
            }
        }
        public void UnclaimResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                UnclaimResource(tuple);
            }
        }
        public void UnclaimResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                UnclaimResource((resource, resources[resource]));
            }
        }


        public bool HasResource(ValueTuple<string, int> resource)
        {
            if (!Resources.ContainsKey(resource.Item1))
            {
                return false;
            }
            else
            {
                return (Resources[resource.Item1] >= resource.Item2);
            }
        }
        public bool HasResources(ValueTuple<string, int>[] resources)
        {
            foreach (var tuple in resources)
            {
                if (!HasResource(tuple))
                {
                    return false;
                }
            }
            return true;
        }
        public bool HasResources(List<ValueTuple<string, int>> resources)
        {
            foreach (var tuple in resources)
            {
                if (!HasResource(tuple))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasResources(Dictionary<string, int> resources)
        {
            foreach (string resource in resources.Keys)
            {
                if (!HasResource((resource, resources[resource])))
                {
                    return false;
                }
            }
            return true;
        }

        public static void PlaceResource(ValueTuple<string, int> resource, int x, int y, int z)
        {
            Item it = Game.World.Items[x, y, z];
            if (it == null)
            {
                it = Entity.Spawn<Item>();
                it.Place(x, y, z);
            }
            it.AddResource(resource);
        }
        public static void PlaceResources(ValueTuple<string, int>[] resources, int x, int y, int z)
        {
            foreach (var tuple in resources)
            {
                PlaceResource(tuple, x, y, z);
            }
        }
        public static void PlaceResources(List<ValueTuple<string, int>> resources, int x, int y, int z)
        {
            foreach (var tuple in resources)
            {
                PlaceResource(tuple, x, y, z);
            }
        }
        public static void PlaceResources(Dictionary<string, int> resources, int x, int y, int z)
        {
            foreach (string resource in resources.Keys)
            {
                PlaceResource((resource, resources[resource]), x, y, z);
            }
        }
    }
    
}
