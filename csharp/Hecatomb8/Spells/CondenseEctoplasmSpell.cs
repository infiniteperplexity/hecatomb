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

    public class CondenseEctoplasmSpell : Spell
    {
        public CondenseEctoplasmSpell()
        {
            // maybe have costs descend by number on the map that are owned?
            MenuName = "condense ectoplasm";
            _cost = 15;
            RequiresResearch = new[] { Research.CondenseEctoplasm };
            RequiresStructures = new[] { typeof(Sanctum) };
        }

        public override void ChooseFromMenu()
        {
            CommandLogger.LogCommand(command: "CondenseEctoplasm");
            Cast();
            var (x, y, z) = Caster!.GetPlacedCoordinate()!;
            ParticleEmitter emitter = new ParticleEmitter();
            emitter.Place(x, y, z);
            Item ecto = Item.SpawnNewResource(Resource.Ectoplasm, 1);
            ecto.DropOnValidTile(x, y, z);
            InterfaceState.ResetControls();
        }
    }
}
