namespace ExampleGame
{
   /// <summary>
   /// Static class representing layer depths to draw different sprite batchs at.
   /// Useful for not having to remember magic floats.
   /// </summary>
   public static class LayerDepth
   {
      public static readonly float Cells = 0.8f;
      public static readonly float Paths = 0.6f;
      public static readonly float Figures = 0.5f;
   }
}
