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
    public class StructureControls : MenuChoiceControls
    {
        public Structure MyStructure;

        public override void RefreshContent()
        {
            var s = MyStructure;
            if (s.Researching == null)
            {
                MenuTop = new List<ColoredText>() {
                "{orange}**Esc: Cancel**.",
                ("{yellow}Structure: "+char.ToUpper(s.MenuHeader[0]).ToString()+s.MenuHeader.Substring(1)),
                "Tab) Next structure."
                };
                var choices = s.MenuChoices;
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
                    ColoredText ct = choices[i].ListOnMenu();
                    ct.Text = (alphabet[i] + ") " + ct.Text);
                    MenuTop.Add(ct);
                }
            }
            else
            {
                ResearchTask rt = s.Researching;
                string txt = "Researching " + Research.Types[rt.Makes].Name + " (" + rt.Labor + " turns; Delete to cancel.)";
                if (rt.Ingredients.Count>0)
                {
                    txt = "Researching " + Research.Types[rt.Makes].Name + " (gathering ingredients)";
                }
                MenuTop = new List<ColoredText>() {
                    "{orange}**Esc: Cancel**.",
                    ("{yellow}Structure: "+char.ToUpper(s.MenuHeader[0]).ToString()+s.MenuHeader.Substring(1)),
                    "Tab) Next structure.",
                    txt
                };
            }
            Game.MenuPanel.Dirty = true;
        }
        public StructureControls(Structure s) : base(s)
        {
            MyStructure = s;
            s.HighlightSquares();
            Game.MainPanel.Dirty = true;
            var Commands = Game.Commands;
            KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.Escape] =
                () =>
                {
                    s.Unhighlight();
                    Reset();
                };
            KeyMap[Keys.Tab] = () => { /* NextStructure */};
            RefreshContent();
        }
    }
}
