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
        public string SlightColor;
        public string MediumColor;
        public string SevereColor;
    
        public Decaying() : base()
        {
            TotalDecay = 2500;
            Decay = TotalDecay;
            AddListener<TurnBeginEvent>(DecayTurn);
            SlightColor = "olive";
            MediumColor = "brown";
            SevereColor = "purple";
        }
       
        public double GetFraction()
        {
           return (double)Decay / (double)TotalDecay;
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