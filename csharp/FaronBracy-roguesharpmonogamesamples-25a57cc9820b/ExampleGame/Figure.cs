using Microsoft.Xna.Framework.Graphics;
using RogueSharp.DiceNotation;

namespace ExampleGame
{
   public class Figure
   {
      public int X { get; set; }
      public int Y { get; set; }
      public Texture2D Sprite { get; set; }
      public int AttackBonus { get; set; }
      public int ArmorClass { get; set; }
      public DiceExpression Damage { get; set; }
      public int Health { get; set; }
      public string Name { get; set; }
   }
}
