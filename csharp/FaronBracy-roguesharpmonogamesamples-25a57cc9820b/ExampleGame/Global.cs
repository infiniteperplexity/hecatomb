using RogueSharp.Random;

namespace ExampleGame
{
   public enum GameStates
   {
      None = 0,
      PlayerTurn = 1,
      EnemyTurn = 2,
      Debugging = 3
   }
   public class Global
   {
      public static readonly Camera Camera = new Camera();  
      public static readonly IRandom Random = new DotNetRandom();
      public static GameStates GameState { get; set; }
      public static readonly int MapWidth = 50;
      public static readonly int MapHeight = 30;
      public static readonly int SpriteWidth = 64;
      public static readonly int SpriteHeight = 64;
      public static CombatManager CombatManager;
   }
}
