using System;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static HecatombAliases;
    using static Resource;
    public class GuardPost : Structure
    {
        public GuardPost() : base()
        {
            Symbols = new char[]
            {
                '\u2694','.','\u2658',
                '.','.','.',
                '\u2658','.','\u2694'
            };
            FGs = new string[]
            {
                "WALLFG","FLOORFG","WALLFG",
                "FLOORFG","WALLFG","FLOORFG",
                "WALLFG","FLOORFG","WALLFG",
            };
            _bg = "#555577";
            BGs = new string[]
            {
                "WALLBG","FLOORBG","WALLBG",
                "FLOORBG","FLOORBG","FLOORBG",
                "WALLBG","FLOORBG","WALLBG",
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Wood, 1}},new Dictionary<Resource, int>(), new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Rock, 1}}, new Dictionary<Resource, int>(),
                new Dictionary<Resource, int>(), new Dictionary<Resource, int>(), new Dictionary<Resource, int>() {{Wood, 1}}
            };
            _name = "guard post";
            UseHint = "(enables forbid and patrol tasks; improves minion evasion.)";
            //AddListener<AttackEvent>(OnAttack);
        }

        //public GameEvent OnAttack(GameEvent ge)
        //{
        //    AttackEvent ae = (AttackEvent)ge;
        //    TypedEntity c = (TypedEntity)ae.Attacker.Entity.Unbox();
        //    if (c.GetComponent<Actor>().Team != Teams.Friendly)
        //    {
        //        // problem...this can double-increase
        //        ae.EvasionModifier += 1;
        //        System.Diagnostics.Debug.WriteLine("We're adding to evasion based on GuardPost");

        //    }
        //    return ge;
        //}
    }
}
