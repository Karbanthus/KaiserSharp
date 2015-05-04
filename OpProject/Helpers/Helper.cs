using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OpProject.Helpers
{
    class Helper
    {
        private static Obj_AI_Hero player
        {
            get { return ObjectManager.Player; }
        }


        private static bool HasTargonItem
        {
            get { return player.InventoryItems.Any(x => x.Id == ItemId.Face_of_the_Mountain || x.Id == ItemId.Relic_Shield || x.Id == ItemId.Targons_Brace); }
        }

        private static float GetItemDmg()
        {
            var dmg = 0;

            if (Items.HasItem(3401, player))
            {
                dmg = 400;
            }
            else if (Items.HasItem(3302, player))
            {
                dmg = 240;
            }
            else if (Items.HasItem(3097, player))
            {
                dmg = 200;
            }

            return dmg;
        }

        static Helper()
        {

        }

        public static Tuple<bool, Obj_AI_Base> IsCanLastHit(Vector3 from, float range, MinionTypes _MinionType, MinionTeam _MinionTeam, MinionOrderTypes _MinionOrderType, bool _IsRanged)
        {
            bool result = false;
            Obj_AI_Base target = null;

            if (HasTargonItem && !player.IsDead && player.Buffs.Any(x => x.Count > 0 && x.Name == "talentreaperdisplay") && IsAllyInRange(1500))
            {
                var minions = MinionManager.GetMinions(from, range, _MinionType, _MinionTeam, _MinionOrderType);
                var itemNum = player.InventoryItems.Any(x => x.Id == ItemId.Face_of_the_Mountain || x.Id == ItemId.Relic_Shield || x.Id == ItemId.Targons_Brace);

                foreach(var minion in minions)
                {
                    var a = player.Buffs.Any(x => x.DisplayName.Contains("ThreshPassiveSoulsGain")) ? player.Buffs.Find(x => x.DisplayName.Contains("ThreshPassiveSoulsGain")).Count : 0;
                    var dmg = _IsRanged ? player.GetAutoAttackDamage(minion) + a : GetItemDmg();

                    if (minion.Health < dmg && player.CanAttack && Orbwalking.InAutoAttackRange(minion))
                    {
                        result = true;
                        target = minion;
                    }
                }
            }

            return new Tuple<bool, Obj_AI_Base>(result, target);
        }

        public static bool IsAllyInRange(float _range)
        {
            var status = false;

            foreach (var allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => !x.IsMe && x.IsAlly && x.IsValid && !x.IsDead))
            {
                if (player.Distance(allyhero.Position) < _range)
                {
                    status = true;
                }
            }

            return status;
        }

        public static IEnumerable<Obj_AI_Hero> IsEnemyInRange(float _range)
        {
            return HeroManager.Enemies.Where(x => x.IsEnemy && !x.IsDead && x.IsValid && player.Distance(x.Position) < _range);
        }

        public static IEnumerable<Obj_AI_Hero> IsEnemyInRange(float _range, Obj_AI_Hero from)
        {
            return HeroManager.Enemies.Where(x => x.IsEnemy && !x.IsDead && x.IsValid && from.Position.Distance(x.Position) < _range);
        }

        public static bool IsUnderTurret(Vector3 position, bool IsEnemyTower)
        {
            if (IsEnemyTower)
                return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950, true, position));
            else
                return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950, false, position) && turret.IsAlly);
        }

        public static bool IsUnderTurret(Obj_AI_Hero hero, bool IsEnemyTower)
        {
            if (IsEnemyTower)
                return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950, true, hero.Position));
            else
                return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950, false, hero.Position) && turret.IsAlly);
        }

        public static Obj_AI_Hero GetMostAD(bool IsAllyTeam, float range)
        {
            Obj_AI_Hero MostAD = null;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => (IsAllyTeam ? x.IsAlly : x.IsEnemy) && CommonData.isValidTarget(x) && !x.IsMe))
            {
                if (player.Distance(hero.Position) < range)
                {
                    if (MostAD == null)
                    {
                        MostAD = hero;
                    }
                    else if (MostAD != null && MostAD.TotalAttackDamage < hero.TotalAttackDamage)
                    {
                        MostAD = hero;
                    }
                }
            }

            return MostAD;
        }

        public static bool ManaManager(int Qmana = 0, int Wmana = 0, int Emana = 0, int Rmana = 0, int Qt = 1, int Wt = 1, int Et = 1, int Rt = 1)
        {
            var status = false;

            if (player.MaxHealth * 0.3 > player.Health)
            {
                status = true;
            }
            else
            {
                int mana = 0;
                mana += Qmana * Qt;
                mana += Wmana * Wt;
                mana += Emana * Et;
                mana += Rmana * Rt;

                if (player.Mana > mana)
                {
                    status = true;
                }
                else
                {
                    status = false;
                }
            }

            return status;
        }
    }
}
