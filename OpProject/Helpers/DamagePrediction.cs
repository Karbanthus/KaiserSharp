using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace OpProject.Helpers
{
    class DamagePrediction
    {
        public delegate void DmgPrediction(Obj_AI_Hero sender, Obj_AI_Hero target, float dmg);

        public static event DmgPrediction OnSpellDmg;

        static DamagePrediction()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (OnSpellDmg == null)
                return;

            if (!(sender is Obj_AI_Hero))
                return;

            if (!(args.Target is Obj_AI_Hero))
                return;

            var _sender = sender as Obj_AI_Hero;
            var target = args.Target as Obj_AI_Hero;
            var dmg = Orbwalking.IsAutoAttack(args.SData.Name) ? (float)sender.GetAutoAttackDamage(target) : GetDmg(_sender, target, _sender.GetSpellSlot(args.SData.Name));
            
            if (dmg != 0)
            {
                OnSpellDmg(_sender, target, dmg);
            }
        }

        private static float GetDmg(Obj_AI_Hero Sender, Obj_AI_Hero target, SpellSlot SpellSlot)
        {
            return (float)Sender.GetSpellDamage(target, SpellSlot);
        }
    }
}
