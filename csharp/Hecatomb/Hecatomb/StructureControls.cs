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
            if (MyStructure.Researching == null)
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
                    MenuTop.Add(alphabet[i] + ") " + choices[i].ListOnMenu());
                }
            }
            else
            {
                MenuTop = new List<ColoredText>() {
                    "{orange}**Esc: Cancel**.",
                    ("{yellow}Structure: "+char.ToUpper(s.MenuHeader[0]).ToString()+s.MenuHeader.Substring(1)),
                    "Tab) Next structure.",
                    "Researching "+s.Researching+" ("+s.ResearchTurns+" turns; Delete to cancel.)"
                };
            }
            Game.MenuPanel.Dirty = true;
        }
        public StructureControls(Structure s) : base(s)
        {
            MyStructure = s;
            s.Highlight();
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
