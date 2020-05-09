using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Feature : ComposedEntity
    {
        // this name is actually a bit misleading...solid means like doors, where only friendlies can pass through
        [JsonIgnore] public bool Solid;
        public override void PlaceInValidEmptyTile(int x, int y, int z)
        {
            if (GameState.World!.Features.GetWithBoundsChecked(x, y, z) != null)
            {
                if (HecatombOptions.NoisyErrors)
                {
                    throw new InvalidOperationException($"Can't place {Describe()} at {x} {y} {z} because {GameState.World!.Features.GetWithBoundsChecked(x, y, z)!.Describe()} is already there.");
                }
                else
                {
                    GameState.World!.Features.GetWithBoundsChecked(x, y, z)!.Despawn();
                }
            }
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Features.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
            base.PlaceInValidEmptyTile(x, y, z);
            if (Spawned)
            {
                GameState.World!.Features.SetWithBoundsChecked(x, y, z, this);
                Publish(new AfterPlaceEvent() { Entity = this, X = x, Y = y, Z = z });
            }
        }

        public override void Remove()
        {
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Features.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
            base.Remove();
        }

        public static Coord? FindPlace(int x, int y, int z, int max = 5, int min = 0, bool groundLevel = true, int expand = 0)
        {
            return Tiles.NearbyTile(x, y, z, max: max, min: min, groundLevel: groundLevel, expand: expand, valid: (fx, fy, fz) => {
                return
                    (Features.GetWithBoundsChecked(fx, fy, fz) is null)
                    && (!Covers.GetWithBoundsChecked(fx, fy, fz).Liquid)
                    && (Terrains.GetWithBoundsChecked(fx, fy, fz) == Terrain.FloorTile)
                    ;
            });
        }

        protected override string? getFG()
        {
            var cosmetic = TryComponent<CosmeticComponent>();
            if (cosmetic != null && cosmetic.FG != null)
            {
                return cosmetic.FG;
            }
            return base.getFG();
        }

        protected override string? getBG()
        {
            var cosmetic = TryComponent<CosmeticComponent>();
            if (cosmetic != null && cosmetic.BG != null)
            {
                return cosmetic.BG;
            }
            return base.getBG();
        }

        protected override string? getName()
        {
            if (HasComponent<Defender>())
            {
                int Wounds = GetComponent<Defender>().Wounds;
                if (Wounds >= 6)
                {
                    return "severely damaged " + base.getName();
                }
                else if (Wounds >= 4)
                {
                    return "damaged " + base.getName();
                }
                else if (Wounds >= 2)
                {
                    return "slightly damaged " + base.getName();
                }
            }
            return base.getName();
        }
    }
}
