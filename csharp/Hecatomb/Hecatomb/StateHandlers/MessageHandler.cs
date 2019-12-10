using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{

    public class MessageHandler : StateHandler
    {
        public List<ColoredText> MessageHistory;
        public bool Unread;
        public string UnreadColor;

        public MessageHandler()
        {
            MessageHistory = new List<ColoredText>();
            UnreadColor = "white";
        }
    }
}
