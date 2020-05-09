using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{

    public class GameLog : StateHandler, IDisplayInfo
    {
        public List<ColoredText> MessageHistory;
        public bool Unread;
        public string UnreadColor;

        public int SelectedMessage;
        public GameLog()
        {
            MessageHistory = new List<ColoredText>();
            UnreadColor = "white";
        }

        public void PushMessage(ColoredText ct)
        {
            int MaxArchive = 100;
            MessageHistory.Insert(0, ct);
            while (MessageHistory.Count > MaxArchive)
            {
                MessageHistory.RemoveAt(MaxArchive);
            }
            SelectedMessage = 0;
            Unread = true;
            if (ct.Colors.ContainsKey(0))
            {
                var order = new Dictionary<string, int>
                {
                    ["white"] = 0,
                    ["yellow"] = 1,
                    ["magenta"] = 2,
                    ["orange"] = 3,
                    ["red"] = 4
                };
                var c = ct.Colors[0];
                if (order.ContainsKey(c))
                {
                    if (order[c] > order[UnreadColor])
                    {
                        UnreadColor = c;
                    }
                }
            }
            InterfaceState.DirtifyTextPanels();
        }

        public void ScrollUp()
        {
            if (SelectedMessage > 0)
            {
                SelectedMessage -= 1;
            }
            InterfaceState.DirtifyTextPanels();
        }

        public void ScrollDown()
        {
            int maxVisible = 4;
            if (SelectedMessage < MessageHistory.Count - maxVisible)
            {
                SelectedMessage += 1;
            }
            InterfaceState.DirtifyTextPanels();
        }

        public void BuildInfoDisplay(InfoDisplayControls menu)
        {
            
        }

        public void FinishInfoDisplay(InfoDisplayControls menu)
        {
            var Commands = InterfaceState.Commands!;
            //KeyMap[Keys.Space] = WaitOrReconstruct;
            menu.KeyMap[Keys.PageUp] = ScrollUp;
            menu.KeyMap[Keys.PageDown] = ScrollDown;
            // add these back in, having removed them in the constructor
            menu.KeyMap[Keys.Z] = Commands.ChooseSpell;
            menu.KeyMap[Keys.J] = Commands.ChooseTask;
            menu.KeyMap[Keys.R] = Commands.ShowResearch;
            menu.KeyMap[Keys.V] = Commands.ShowAchievements;
            int MaxVisible = Math.Min(MessageHistory.Count, 4);
            var controls = new List<ColoredText>()
            {
                "{orange}**Esc) Back**.",
                " ",
                "{yellow}Message Log:",
                "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
            };
            var list = MessageHistory.GetRange(SelectedMessage, MaxVisible).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new ColoredText(list[i]);
            }
            if (list.Count > 1 && list[1].Colors.Count == 0)
            {
                // this used to reference list[1] on the right hand side, I assume that was just a bug
                list[0] = new ColoredText(list[0].Text, "cyan");
            }
            if (list.Count == 0)
            {
                list.Add("{cyan}Welcome to Hecatomb!");
            }
            foreach (var txt in list)
            {
                txt.Text = "- " + txt.Text;
            }
            list = controls.Concat(list).ToList();
            list.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            list.Add("Scroll: Page/Fn Up/Down");
            menu.InfoTop = list.ToList();
        }

        public void MarkAsRead()
        {
            Unread = false;
            UnreadColor = "white";
        }
    }
}

