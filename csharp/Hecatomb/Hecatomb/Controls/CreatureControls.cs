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
    public class CreatureControls : MenuChoiceControls
    {
        public Creature MyCreature;

        public override void RefreshContent()
        {
            var c = MyCreature;
            MenuTop = new List<ColoredText>() {
                "{orange}**Esc: Cancel**.",
                ("{yellow}Structure: "+char.ToUpper(c.MenuHeader[0]).ToString()+c.MenuHeader.Substring(1)),
                "Tab) Next structure."
            };
            if (c.TryComponent<Minion>()!=null)
            {
                Task t = c.GetComponent<Minion>().Task;
                if (t!=null)
                {
                    MenuTop.Add("Working on "+t.Describe());
                }  
            }
            if (c.TryComponent<Inventory>() != null)
            {
                Item item = c.GetComponent<Inventory>().Item;
                if (item != null)
                {
                    MenuTop.Add("Carrying " + item.Describe());
                }
            }
            var choices = c.MenuChoices;
            // not the real thing to do...
            for (int i = 0; i < choices.Count; i++)
            {
                IMenuListable choice = choices[i];
                KeyMap[Alphabet[i]] =
                    () =>
                    {
                        choice.ChooseFromMenu();
                        RefreshContent();
                    };
                MenuTop.Add(alphabet[i] + ") " + choices[i].ListOnMenu());
            }
            Game.MenuPanel.Dirty = true;
        }
        public CreatureControls(Creature c) : base(c)
        {
            MyCreature = c;
            c.Highlight = "lime green";
            Game.MainPanel.Dirty = true;
            var Commands = Game.Commands;
            KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.Escape] =
                () =>
                {
                    c.Highlight = null;
                    Reset();
                };
            KeyMap[Keys.Tab] = () => { /* NextStructure */};
            RefreshContent();
        }
    }
}
