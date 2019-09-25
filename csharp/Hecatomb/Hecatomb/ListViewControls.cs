/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 1:08 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
    /// <summary>
    /// Description of MenuChoiceContext.
    /// </summary>
    /// 

    public interface IListPopulater
    {
        List<ColoredText> GetLines();
    }
    public class ListViewControls : ControlContext
    {
        IListPopulater populater;
        

        public ListViewControls(IListPopulater p) : base()
        {
            populater = p;
            RefreshContent();
        }

        public override void RefreshContent()
        {
            var Commands = Game.Commands;
            KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.Escape] = Reset;
            MenuTop = populater.GetLines();
        }
    }
}
