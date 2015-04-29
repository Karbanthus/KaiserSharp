using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace OpProject.Evade.Targeted
{
    class TargetedDetector
    {
        public delegate void TSDetectorH(TargetSpell spell);

        public static event TSDetectorH TSDetector;

        public static List<TargetSpell> ActiveTargeted = new List<TargetSpell>();
        
        static TargetedDetector()
        {
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (TSDetector == null)
                return;

            ActiveTargeted.RemoveAll(x => !x.IsActive);

            foreach (var spell in ActiveTargeted)
            {
                TSDetector(spell);
            }
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (TSDetector == null)
                return;

            if (!(sender is Obj_AI_Hero))
                return;

            if (ObjectManager.Player.Distance(sender.Position) > 2500)
                return;

            if (!(args.Target is Obj_AI_Hero))
                return;

            if (args.SData.Name.ToLower().Contains("summoner") || args.SData.Name.ToLower().Contains("recall"))
                return;

            if (Orbwalking.IsAutoAttack(args.SData.Name))
                return;

            var _sender = sender as Obj_AI_Hero;

            if (args.Target == null)
            {
                Console.WriteLine("NonTarget Spell : " + _sender.GetSpellSlot(args.SData.Name).ToString());
            }

            if (!IsFind(args))
                return;

            var target = args.Target as Obj_AI_Hero;
            var spelldata = TargetedSpellDatabase.TargetedSpellDB.FirstOrDefault(x => x.SpellName == args.SData.Name.ToLower());

            ActiveTargeted.Add(new TargetSpell
                {
                    Sender = _sender,
                    Target = target,
                    Spell = spelldata,
                    StartTick = Environment.TickCount,
                    StartPosition = args.Start
                });
        }

        private static bool IsFind(GameObjectProcessSpellCastEventArgs args)
        {
            return TargetedSpellDatabase.TargetedSpellDB.Any(x => x.SpellName == args.SData.Name.ToLower());
        }

    }
}
