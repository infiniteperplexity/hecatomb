using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Inventory : Component
    {
        public ListenerHandledEntityHandle<Item>? Item;

        public Inventory()
        {
            AddListener<DespawnEvent>(OnDespawn);
            AddListener<DestroyEvent>(OnDestroy);
        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (Item != null && de.Entity == Item.UnboxBriefly())
            {
                Item = null;
            }
            base.OnDespawn(ge);
            return ge;
        }

        public GameEvent OnDestroy(GameEvent ge)
        {
            DestroyEvent de = (DestroyEvent)ge;
            if (Entity != null && de.Entity == Entity.UnboxBriefly())
            {
                Drop();
            }
            return ge;
        }
        public void Drop()
        {
            if (Entity?.UnboxBriefly() != null && Entity.UnboxBriefly()!.Placed && Item?.UnboxBriefly() != null)
            {
                var item = Item.UnboxBriefly()!;
                if (item.Resource == Resource.Gold)
                {
                    Publish(new AchievementEvent() { Action = "FoundGold" });
                }
                var (x, y, z) = Entity.UnboxBriefly()!.GetVerifiedCoord();
                item.DropOnValidTile((int)x!, (int)y!, (int)z!);
                Item = null;
            }
        }
    }
}
