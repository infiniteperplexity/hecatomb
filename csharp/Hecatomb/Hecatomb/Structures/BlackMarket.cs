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

        public List<TradeTask> Trades;

        public BlackMarket() : base()
        {
            Trades = new List<TradeTask>();
            Symbols = new char[]
            {
                '\u00A3','.','.',
                '.','\u2696','$',
                '\u00A2','\u20AA','\u00A4'
            };
            FGs = new string[]
            {
                "#552222", "FLOORFG","FLOORFG",
                "FLOORFG", "#888844","#888844",
                "#225522","#333333","#222266"
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
            MenuName = "black market";
            Name = "black market";
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
            HighlightSquares();
            if (Game.World.Tasks[X, Y, Z] != null)
            {
                // maybe give the option to cancel the trade?
                menu.Choices = new List<IMenuListable>();
            }
            else
            {
                var list = new List<IMenuListable>();
                // somehow we need to decide what trades are on offer
                menu.Choices = list;
            }
        }

        public override void FinishMenu(MenuChoiceControls menu)
        {
            menu.MenuTop.Insert(2, "Tab) Next structure.");
            if (Researching != null)
            {
                TradeTask t = Trading;
                string txt = "Trading " + t.Describe() + " (" + t.Labor + " turns; Delete to cancel.)";
                if (t.Ingredients.Count > 0)
                {
                    txt = "Trading " + t.Describe() + " ($: " + t.Ingredients + ")";
                }
                menu.MenuTop = new List<ColoredText>() {
                    "{orange}**Esc: Cancel**.",
                    "{yellow}Structure: "+Describe(),
                    "Tab) Next structure.",
                    txt
                };
            }
            Game.MenuPanel.Dirty = true;

            menu.KeyMap[Keys.Escape] =
               () =>
               {
                    Unhighlight();
                    Game.Controls.Reset();
                };
            menu.KeyMap[Keys.Tab] = () => { /* NextStructure */};
        }
    }
}
