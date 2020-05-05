using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
	using static Resource;
    class SpearTrap : Feature
    {
		public SpearTrap()
		{
			_symbol = '\u2963';
			_fg = "brown";
			_name = "spear trap";
			Components.Add(new Attacker());
			Components.Add(new Fixture()
			{
				Ingredients = new JsonArrayDictionary<Resource, int>() { [Wood] = 2, [Flint] = 1},
				RequiresStructures = new Type[] {typeof(Workshop)},
				RequiresResearch = new Research[] {Research.SpearTrap}
			});
			AddListener<StepEvent>(OnStep);
		}

		public GameEvent OnStep(GameEvent ge)
		{
			StepEvent se = (StepEvent)ge;
            if (!Spawned || !Placed)
            {
                return ge;
            }
            if (se.X == X && se.Y == Y && se.Z == Z)
            {
                Creature cr = se.Entity!;
                if (cr.GetComponent<Actor>().Team != Team.Friendly)
                {
                    GetComponent<Attacker>().Attack(cr);
                    if (GameState.World!.Random.Next(2)==0)
                    {
                        var item = Item.SpawnNewResource(Resource.Flint, 1);
                        item.DropOnValidTile(se.X, se.Y, se.Z);
                    }
                    else if (GameState.World!.Random.Next(2) == 0)
                    {
                        var item = Item.SpawnNewResource(Resource.Wood, 1);
                        item.DropOnValidTile(se.X, se.Y, se.Z);
                    }
                    Destroy();
                }
            }
            return ge;
		}
    }
}