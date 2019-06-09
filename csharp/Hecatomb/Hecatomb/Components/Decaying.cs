/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/29/2018
 * Time: 10:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
    /// <summary>
    /// Description of Combat.
    /// </summary>
    public class Decaying : Component
    {
        public int TotalDecay;
        public int Decay;
    
        public Decaying() : base()
        {
            TotalDecay = 2500;
            Decay = TotalDecay;
            AddListener<TurnBeginEvent>(DecayTurn);
        }
       
        public GameEvent DecayTurn(GameEvent ge)
        {
            Decay -= 1;
            if (Decay<=0)
            {
                Entity.Unbox().Destroy();
            }
            return ge;
        }
    }
}