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
   
    public class SplashControls : ControlContext
    {
        public Action MyCallback;
        public SplashControls()
        {
            MenuSelectable = false;
            UseKeyFallback = true;
        }
        public override void HandleClick(int x, int y)
        {
            if (MyCallback != null)
            {
                MyCallback();
            }
            else
            {
                Reset();
            }
        }
        public override void HandleHover(int x, int y)
        {
        }
        public override void HandleKeyFallback()
        {
            Reset();
        }

    }
}
