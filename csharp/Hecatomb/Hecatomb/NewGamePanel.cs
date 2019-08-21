/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/5/2018
 * Time: 9:57 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{

    public class RevisedInterfaceElement
    {
        // in some cases the entire background, but sometimes tiled
        public Texture2D BG;
        int? selfX0;
        int? selfY0;

        public RevisedInterfaceElement Parent;
        protected List<RevisedInterfaceElement> children;

        public bool IsFixed()
        {
            if (selfX0 != null && selfY0 != null)
            {
                return true;
            }
            return false;
        }
        public int X0
        {
            get
            {
                if (Parent == null)
                {
                    return (int)selfX0;
                }
                else if (selfX0 != null)
                {
                    return (int)selfX0 + Parent.X0;
                }
                else
                {
                    return Parent.X0;
                }
            }
            set
            {
                selfX0 = value;
                if (selfY0 == null)
                {
                    selfY0 = 0;
                }
            }
        }

        public int Y0
        {
            get
            {
                if (Parent == null)
                {
                    return (int)selfY0;
                }
                else if (selfY0 != null)
                {
                    return (int)selfY0 + Parent.Y0;
                }
                else
                {
                    return Parent.Y0;
                }
            }
            set
            {
                selfY0 = value;
                if (selfX0 == null)
                {
                    selfX0 = 0;
                }
            }
        }
        public void AddChild(RevisedInterfaceElement el)
        {
            el.Parent = this;
            children.Add(el);
        }
    }

    public class NewGamePanel
    {
        public GraphicsDeviceManager Graphics;
        public SpriteBatch Sprites;
        public Texture2D BG;
        public int X0;
        public int Y0;
        public int PixelWidth;
        public int PixelHeight;
        public List<InterfaceElement> Children;


        public NewGamePanel(GraphicsDeviceManager graphics, SpriteBatch sprites, int x0, int y0)
        {
            Graphics = graphics;
            Sprites = sprites;
            X0 = x0;
            Y0 = y0;
            Children = new List<InterfaceElement>();
        }

        public virtual void DrawElements() {
            int y1 = 0;
            foreach (var el in Children)
            {
                if (el.Dirty || el is MainViewElement)
                {
                    el.Draw(y1);
                    el.Dirty = false;
                }
                y1 += el.Height;
            }
        }

        public void AddElement(InterfaceElement ie)
        {
            ie.Panel = this;
            Children.Add(ie);
        }
    }
}