using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    class DebugSpell : Spell, IDisplayInfo
    {
        public List<Type> Spells;

        public DebugSpell()
        {
            MenuName = "(debugging spells)";
            Spells = new List<Type>()
            {
                typeof(DebugHealSpell),
                typeof(ParticleTestDebugSpell),
                typeof(DebugFlowerSpell),
                typeof(DebugBanditSpell),
                typeof(SummonBanditsDebugSpell),
                typeof(CrashDebugSpell)  
            };
            _cost = 0;
        }

        public override ColoredText ListOnMenu()
        {
            return MenuName;
        }
        public override void ChooseFromMenu()
        {
            var c = new InfoDisplayControls(this);
            c.MenuCommandsSelectable = false;
            InterfaceState.SetControls(c);
        }

        public void BuildInfoDisplay(InfoDisplayControls info)
        {
            info.Header = "Choose a spell:";
            List<IMenuListable> spells = new List<IMenuListable>();
            // only if we have the prerequisite structures / technologies...
            var structures = Structure.ListStructureTypes();
            var researched = GetState<ResearchHandler>().Researched;
            var caster = Player.GetComponent<SpellCaster>();
            foreach (Type sp in Spells)
            {
                var spell = (Spell)Activator.CreateInstance(sp)!;
                spell.Caster = Player;
                spell.Component = caster;
                spells.Add(spell);
            }
            info.Choices = spells;
        }

        public void FinishInfoDisplay(InfoDisplayControls info)
        {
            info.InfoTop.Insert(1, Player.GetComponent<SpellCaster>().GetSanityText());
            info.InfoTop.Insert(1, " ");
        }

        public class SummonBanditsDebugSpell : Spell
        {
            public SummonBanditsDebugSpell()
            {
                MenuName = "summon bandits";
                _cost = 0;
            }

            public override void ChooseFromMenu()
            {
                Cast();
                GetState<SiegeHandler>().BanditAttack(debugCloser: true);

            }
        }

        public class CrashDebugSpell : Spell
        {
            public CrashDebugSpell()
            {
                MenuName = "throw an exception";
                _cost = 0;
            }

            public override void ChooseFromMenu()
            {
                throw new Exception("debugging exception");
            }
        }

        public class ParticleTestDebugSpell : Spell, ISelectsTile
        {
            public ParticleTestDebugSpell()
            {
                MenuName = "particle test";
                _cost = 0;
            }

            public override void ChooseFromMenu()
            {
                var c = new SelectTileControls(this);
                c.SelectedMenuCommand = "Spells";
                c.MenuCommandsSelectable = false;
                InterfaceState.SetControls(c);
            }

            public void SelectTile(Coord c)
            {
                int z = GameState.World!.GetBoundedGroundLevel(c.X, c.Y);
                ParticleEmitter emitter = new ParticleEmitter();
                Debug.WriteLine($"Emitting particles at Z={z}");
                emitter.Place(c.X, c.Y, z);
            }

            public void TileHover(Coord c)
            {
            }
        }

        public class DebugFlowerSpell : Spell, ISelectsTile
        {
            public DebugFlowerSpell() : base()
            {
                MenuName = "spawn flower";
                _cost = 0;
            }

            public override void ChooseFromMenu()
            {
                var c = new SelectTileControls(this);
                c.SelectedMenuCommand = "Spells";
                c.MenuCommandsSelectable = false;
                InterfaceState.SetControls(c);
            }

            static int NextFlower = 0;
            public void SelectTile(Coord c)
            {
                Feature f = Flower.Spawn(Resource.Flowers[NextFlower]);
                f.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                NextFlower = (NextFlower + 1) % Resource.Flowers.Count;
            }

            public void TileHover(Coord c)
            {

            }
        }

        public class DebugHealSpell : Spell
        {
            public DebugHealSpell()
            {
                MenuName = "self heal";
                _cost = 0;
            }

            public override void ChooseFromMenu()
            {
                Cast();
            }
            public override void Cast()
            {
                Caster!.GetComponent<Defender>().Wounds = 0;
            }
        }

        public class DebugBanditSpell : Spell, ISelectsTile
        {
            public DebugBanditSpell() : base()
            {
                MenuName = "spawn bandit";
                _cost = 0;
            }

            public override void ChooseFromMenu()
            {
                var c = new SelectTileControls(this);
                c.SelectedMenuCommand = "Spells";
                c.MenuCommandsSelectable = false;
                InterfaceState.SetControls(c);
            }

            public void SelectTile(Coord c)
            {
                Creature bandit = Entity.Spawn<Bandit>();
                bandit.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
            }

            public void TileHover(Coord c)
            {
            }
        }
    }
}
