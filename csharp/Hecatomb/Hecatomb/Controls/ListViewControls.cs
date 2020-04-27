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
            MenuCommandsSelectable = false;
            populater = p;
            RefreshContent();
        }

        public override void RefreshContent()
        {
            AlwaysPaused = true;
            KeyMap[Keys.Space] = WaitOrReconstruct;
            KeyMap[Keys.Escape] = Reset;
            MenuTop = populater.GetLines();
            MenuTop.Insert(0, " ");
            MenuTop.Insert(0, "{orange}**Esc) Back**.");
        }

        
    }
}
