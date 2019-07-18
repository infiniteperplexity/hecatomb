using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
    using static HecatombAliases;

  
    public class ImageOverride
    {
        protected GraphicsDeviceManager Graphics;
        protected SpriteBatch Sprites;
        protected Texture2D image;

        public ImageOverride(GraphicsDeviceManager graphics, SpriteBatch sprites)
        {
            image = Game.MyContentManager.Load<Texture2D>("necromancer");
            Graphics = graphics;
            Sprites = sprites;
        }
        public virtual void Draw()
        {
            Sprites.Draw(
                image,
                null,
                new Rectangle(24, 24, 540, 540), //destination
                new Rectangle(75, 20, 600, 600), Vector2.Zero, //source
                0,
                Vector2.One,
                Color.White,
                SpriteEffects.None,
                0
            );
        }
    }

    


}
