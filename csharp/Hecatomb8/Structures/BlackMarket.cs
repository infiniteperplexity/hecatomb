using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Input;


namespace Hecatomb8
{
    using static HecatombAliases;
    using static Resource; 
    public class BlackMarket : Structure
    {
        public static List<(Resource r1, int n1, Resource r2, int n2)> PotentialTrades;
        // wow this is really weird...we serialize non-spawned entities here?  that sound like a terrible idea
        public List<int> AvailableTradeIndexes;


        public void AddTrade()
        {
            int MaxTrades = 4;
            int r = GameState.World!.Random.Next(PotentialTrades.Count);
            if (AvailableTradeIndexes.Count >= MaxTrades)
            {
                AvailableTradeIndexes.RemoveAt(MaxTrades - 1);
            }
            AvailableTradeIndexes.Insert(0, r);
        }
        public override GameEvent OnTurnBegin(GameEvent ge)
        {
            if (GameState.World!.Random.Next(100)==0)
            {
                AddTrade();
            }
            return ge;
        }

        static BlackMarket()
        {
            PotentialTrades = new List<(Resource r1, int n1, Resource r2, int n2)>()
            {
                (Gold, 1, Flint, 2),
                (Gold, 1, Coal, 2),
                (Gold, 1, Bone, 2),
                (Gold, 1, TinOre, 1),
                (Gold, 1, CopperOre, 1),
                (Gold, 1, Silk, 2),
                (Flint, 3, Gold, 1),
                (Coal, 3, Gold, 1),
                (Silk, 3, Gold, 1),
                (TinOre, 2, Gold, 1),
                (CopperOre, 2, Gold, 1)
            };
            // maybe add dyes here as well?
        }
        public BlackMarket() : base()
        {
            AvailableTradeIndexes = new List<int>();
            Symbols = new char[]
            {
                '\u00A3','.','\u20AA',
                '.','.','$',
                '\u00A2','.','\u00A4'
            };
            FGs = new string[]
            {
                "#552222", "FLOORFG","#333333",
                "FLOORFG", "#888844","#888844",
                "#225522","FLOORFG","#222266"
            };
            _bg = "#555544";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Rock, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Wood, 1}},
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Gold, 1}}, new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>() {{Wood, 1}}, new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Rock, 1}}
            };
            Harvests = new Dictionary<Resource, float>[]
            {
                new Dictionary<Resource, float>(), new Dictionary<Resource, float>(), new Dictionary<Resource, float>(),
                new Dictionary<Resource, float>(), new Dictionary<Resource, float>() {{Gold, 1}}, new Dictionary<Resource, float>(),
                new Dictionary<Resource, float>(), new Dictionary<Resource, float>(), new Dictionary<Resource, float>()
            };
            _name = "black market";
            UseHint = "(trade goods for gold or vice versa.)";
            AddListener<PlaceEvent>(OnPlace);


        }

        public GameEvent OnPlace(GameEvent ge)
        {
            PlaceEvent pe = (PlaceEvent)ge;
            if (pe.Entity == this)
            {
                AddTrade();
                AddTrade();
            }
            return ge;
        }

        [JsonIgnore]
        public TradeTask? Trading
        {
            get
            {
                if (!Placed || !Spawned)
                {
                    return null;
                }
                foreach (int? eid in Features)
                {
                    Feature? f = GetEntity<Feature>(eid);
                    if (f is null || !f.Spawned || !f.Placed)
                    {
                        continue;
                    }
                    var (x, y, z) = f.GetVerifiedCoord();
                    Task? t = Tasks.GetWithBoundsChecked(x, y, z);
                    if (t is TradeTask)
                    {
                        return (TradeTask)t;
                    }
                }
                return null;
            }
            set
            {

            }
        }

        public override void BuildInfoDisplay(InfoDisplayControls menu)
        {
            // might want to format htis guy a bit...like add coordinates?
            menu.SelectedEntity = this;
            if (Trading != null)
            {
                // maybe give the option to cancel the trade?
                menu.Choices = new List<IMenuListable>();
            }
            else
            {
                var list = new List<IMenuListable>();
                // somehow we need to decide what trades are on offer
                foreach (int i in AvailableTradeIndexes)
                {
                    var tuple = PotentialTrades[i];
                    TradeTask t = Entity.Mock<TradeTask>();
                    t.Ingredients[tuple.r1] = tuple.n1;
                    t.Trading[tuple.r2] = tuple.n2;
                    list.Add(t);
                }
                menu.Choices = list;

            }
        }

        public override void FinishInfoDisplay(InfoDisplayControls menu)
        {
            menu.InfoTop.Insert(2, "Tab) Next structure.");
            menu.InfoTop.Insert(3, " ");
            menu.InfoTop.Insert(4, "{yellow}" + Describe(capitalized: true));
            menu.InfoTop.Insert(5, "{light cyan}" + UseHint);

            if (Trading != null)
            {
                TradeTask t = Trading;
                var txt = t.Name!;
                txt = "T" + txt.Substring(1);
                menu.InfoTop = new List<ColoredText>() {
                    "{orange}**Esc: Cancel**.",
                    " ",
                    "Tab) Next structure.",
                    " ",
                    "{yellow}" + Describe(capitalized: true),
                    "{light cyan}" + UseHint,
                    " ",
                    txt,
                    "(Backspace/Del to Cancel)"
                };
            }
            menu.KeyMap[Keys.Tab] = NextStructure;
            menu.KeyMap[Keys.Delete] = CancelTrade;
            menu.KeyMap[Keys.Back] = CancelTrade;
        }

        public void CancelTrade()
        {
            if (Trading != null)
            {
                Trading.Cancel();
                Trading = null;
            }
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();
        }

        public override GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity is Feature)
            {
                Feature f = (Feature)de.Entity;
                if (Placed && Features.Contains(f.EID))
                {
                    CancelResearch();
                }
            }
            return base.OnDespawn(ge);
        }
    }
}
