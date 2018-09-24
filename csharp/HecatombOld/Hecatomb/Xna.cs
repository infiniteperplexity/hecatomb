/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 9/23/2018
 * Time: 8:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
	
	/// <summary>
	/// Description of Xna.
	/// </summary>
	public class XnaGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
      	SpriteBatch spriteBatch;
      	public XnaGame() : base()
		{
			graphics = new GraphicsDeviceManager( this );
        	Content.RootDirectory = "Content";
		}
	}
}
