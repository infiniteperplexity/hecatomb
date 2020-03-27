using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public class TypedEntity : TileEntity
    {
        public string TypeName;
        public Dictionary<string, EntityField<Component>> Components;


        public TypedEntity() : base()
        {
            Components = new Dictionary<string, EntityField<Component>>();
        }

        public T GetComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (Components.ContainsKey(t))
            {
                int eid = Components[t];
                return (T)Entities[eid];
            }
            else
            {
                throw new InvalidOperationException(String.Format("{0} has no component of type {1}", this, t));
            }
        }


        public T TryComponent<T>() where T : Component
        {
            string t = typeof(T).Name;
            if (Components.ContainsKey(t))
            {
                int eid = Components[t];
                if (eid==-1)
                {
                    // maybe this can happen when despawns trigger other despawns?
                    return null;
                }
                return (T)Entities[eid];
            }
            else
            {
                return default(T);
            }
        }

        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            base.Place(x1, y1, z1, fireEvent);
            foreach (Component c in Components.Values)
            {
                if (c==null)
                {
                    Debug.WriteLine("found an error");
                    Debug.WriteLine("EID was " + EID);
                    Debug.WriteLine(Components.Count);
                    Debug.WriteLine(Components.Values.ToList()[0].EID);
                    foreach (var key in Components.Keys)
                        Debug.WriteLine(key);

                    foreach (var key in Components.Values)
                        Debug.WriteLine(key.EID);


                }
                c.AfterSelfPlace(x1, y1, z1);
            }
        }

        public override void Despawn()
        {
            if (Components != null)
            {
                foreach (int c in Components.Values)
                {
                    // does this somehow not get updated if they have already despawned?
                    if (Entities.ContainsKey(c))
                    {
                        Entities[c].Despawn();
                    }
                }
            }
            base.Despawn();
        } 
        
        // developmental
        public Component AddComponent(Component c)
        {
            c.AddToEntity(this);
            return c;
        }

        public T AddComponent<T>(Component c) where T: Component
        {
            c.AddToEntity(this);
            return (T) c;
        }

        
        public override string GetDisplayName()
        {
            var palette = TryComponent<RandomPaletteComponent>();
            if (palette != null)
            {
                return GetComponent<RandomPaletteComponent>().GetDisplayName();
            }
            return base.GetDisplayName();
        }
        public string GetCalculatedFG()
        {
            //string rotten = "#774422";
            // unused
            var palette = TryComponent<RandomPaletteComponent>();
            if (palette != null)
            {
                string s = GetComponent<RandomPaletteComponent>().GetFG();
                Debug.WriteLine(s);
                return s;
            }

            if (TryComponent<Decaying>()!=null)
            {
                var d = GetComponent<Decaying>();
                double decay = d.GetFraction();
                if (decay >= 0.75)
                {
                    return FG;
                }
                else if (decay >= 0.5)
                {
                    return d.SlightColor;
                }
                else if (decay >= 0.25)
                {
                    return d.MediumColor;
                }
                else
                {
                    return d.SevereColor;
                }
                //return Game.Colors.Interpolate(rotten, FG, decay);
            }
            var cc = TryComponent<CustomizedComponent>();
            if (cc?.FG != null)
            {
                return cc.FG;
            }
            return FG;
        }


        public virtual string GetCalculatedBG()
        {
            if (ControlContext.Selection == this)
            {
                return "lime green";
            }
            if (TryComponent<Defender>() != null)
            {
                int Wounds = GetComponent<Defender>().Wounds;
                if (Wounds >= 6)
                {
                    return "red";
                }
                else if (Wounds >= 4)
                {
                    return "orange";
                }
                else if (Wounds >= 2)
                {
                    return "yellow";
                }
            }
            var cc = TryComponent<CustomizedComponent>();
            if (cc?.BG != null)
            {
                return cc.BG;
            }
            return BG;
        }

        public override void Leave()
        {
            if (TryComponent<Actor>()!=null)
            {
                GetComponent<Actor>().Spend();
            }
            base.Leave();
        }
    }
}
