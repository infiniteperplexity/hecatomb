using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Input;


namespace Hecatomb
{
    /// <summary>
    /// Description of GuardPost.
    /// </summary>
    public class BlackMarket : Structure
    {
        public static List<TradeTask> PotentialTrades;

        public List<TradeTask> AvailableTrades;


        public void AddTrade()
        {
            // should I call this several times upon Placement?
            int MaxTrades = 4;
            int r = Game.World.Random.Next(PotentialTrades.Count);
            TradeTask newTrade = Entity.Mock<TradeTask>();
            newTrade.Ingredients = PotentialTrades[r].Ingredients;
            newTrade.Trading = PotentialTrades[r].Trading;
            newTrade.Structure = this;

            if (AvailableTrades.Count >= MaxTrades)
            {
                AvailableTrades.RemoveAt(MaxTrades-1);
            }
            AvailableTrades.Insert(0,newTrade);

        }
        public override GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnBeginEvent turn = (TurnBeginEvent)ge;
            if (Game.World.Random.Next(100)==0)
            {
                AddTrade();
            }
            return ge;
        }

        static BlackMarket()
        {
            PotentialTrades = new List<TradeTask>();
            TradeTask t;
            t = Entity.Mock<TradeTask>();
            t.Ingredients["Flint"] = 2;
            t.Trading["TradeGoods"] = 1;
            PotentialTrades.Add(t);
            t = Entity.Mock<TradeTask>();
            t.Ingredients["Wood"] = 3;
            t.Trading["Coal"] = 1;
            PotentialTrades.Add(t);
            t = Entity.Mock<TradeTask>();
            t.Ingredients["TradeGoods"] = 1;
            t.Trading["Flint"] = 2;
            PotentialTrades.Add(t);
        }
        public BlackMarket() : base()
        {
            AvailableTrades = new List<TradeTask>();
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
            BG = "#555544";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<string, int>[]
            {
                new Dictionary<string, int>() {{"Rock", 1}}, null, new Dictionary<string, int>() {{"Wood", 1}},
                null, new Dictionary<string, int>() {{"TradeGoods", 1}}, null,
                new Dictionary<string, int>() {{"Wood", 1}}, null, new Dictionary<string, int>() {{"Rock", 1}}
            };
            Harvests = new Dictionary<string, float>[]
            {
                null, null, null,
                null, new Dictionary<string, float>() {{"TradeGoods", 1}}, null,
                null, null, null
            };
            MenuName = "black market";
            Name = "black market";
            UseHint = "(trade goods of one type for another.)";
            AddListener<PlaceEvent>(OnPlace);


        }

        public GameEvent OnPlace(GameEvent ge)
        {
            PlaceEvent pe = (PlaceEvent)ge;
            if (pe.Entity==this)
            {
                AddTrade();
                AddTrade();
            }
            return ge;
        }

        [JsonIgnore]
        public TradeTask Trading
        {
            get
            {
                foreach (Feature f in Features)
                {
                    var (x, y, z) = f;
                    Task t = Game.World.Tasks[x, y, z];
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

        public override void BuildMenu(MenuChoiceControls menu)
        {
            // might want to format htis guy a bit...like add coordinates?
            menu.Header = "Structure: " + Describe();
            ControlContext.Selection = this;
            if (Trading != null)
            {
                // maybe give the option to cancel the trade?
                menu.Choices = new List<IMenuListable>();
            }
            else
            {
                var list = new List<IMenuListable>();
                // somehow we need to decide what trades are on offer
                foreach (TradeTask t in AvailableTrades)
                {
                    list.Add(t);
                }
                menu.Choices = list;

            }
        }

        public override void FinishMenu(MenuChoiceControls menu)
        {
            menu.MenuTop.Insert(2, "Tab) Next structure.");
            menu.MenuTop.Insert(3, " ");
            menu.MenuTop.Insert(4, "{yellow}" + Describe(capitalized: true));
            menu.MenuTop.Insert(5, "{light cyan}" + UseHint);
            
            if (Trading != null)
            {
                TradeTask t = Trading;
                var txt = t.GetHoverName();
                txt = "T" + txt.Substring(1);
                menu.MenuTop = new List<ColoredText>() {
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
            Game.InfoPanel.Dirty = true;
            menu.KeyMap[Keys.Escape] =
               () =>
               {
                   ControlContext.Selection = null;
                    ControlContext.Reset();
                };
            menu.KeyMap[Keys.Tab] = NextStructure;
            menu.KeyMap[Keys.Delete] = CancelTrade;
            menu.KeyMap[Keys.Back] = CancelTrade;
        }

        public void CancelTrade()
        {
            if (Trading != null)
            {
                Task t = Game.World.Tasks[X, Y, Z];
                t?.Cancel();
                Trading = null;
            }
            InterfacePanel.DirtifyUsualPanels();
        }
    }
}
