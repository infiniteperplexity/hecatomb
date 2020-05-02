using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class ShadowHopSpell : Spell
    {
        public ShadowHopSpell()
        {
            MenuName = "shadow hop";
            _cost = 10;
            RequiresResearch = new[] { Research.ShadowHop };
            RequiresStructures = new[] { typeof(Sanctum) };
        }

        public override void ChooseFromMenu()
        {
            base.ChooseFromMenu();
            if (Cost > Component!.Sanity)
            {
                Debug.WriteLine("cannot cast spell");
            }
            else
            {
                Cast();
            }
        }
        public override void Cast()
        {
            CommandLogger.LogCommand(command: "ShadowHop");
            var (x, y, z) = Caster!.GetVerifiedCoord();
            ParticleEmitter emitter1 = new ParticleEmitter();
            emitter1.Place(x, y, z);
            var m = Caster.GetComponent<Movement>();
            Coord? cc = Tiles.NearbyTile(x, y, z, max: 8, min: 3, valid: (int x1, int y1, int z1) => (m.CanStandBounded(x1, y1, z1)));
            if (cc != null && Creatures.GetWithBoundsChecked(((Coord)cc).X, ((Coord)cc).Y, ((Coord)cc).Z) is null)
            {
                Coord c = (Coord)cc!;
                Caster.GetComponent<Movement>().StepToValidEmptyTile(c.X, c.Y, c.Z);
                Caster.GetComponent<Actor>().Spend();
                InterfaceState.Camera!.Center(c.X, c.Y, c.Z);
                InterfaceState.ResetControls();
                ParticleEmitter emitter2 = new ParticleEmitter();
                emitter2.Place(c.X, c.Y, c.Z);
                base.Cast();
                PushMessage("You vanish and reappear nearby.");
            }
            else
            {
                PushMessage("The spell fizzles.");
            }
        }
    }
}
