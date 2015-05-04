using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OpProject.Champions
{
    class Sivir : CommonData
    {
        public Sivir()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OpProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1240);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            QMana = new[] { 70, 80, 90, 100, 110 };
            WMana = new[] { 60, 60, 60, 60, 60 };
            RMana = new[] { 100, 100, 100 };
        }

        private void LoadMenu()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                var Qmenu = new Menu("Q", "Q");
                {
                    Qmenu.AddItem(new MenuItem("C-UseQ", "Use Q", true).SetValue(true));
                    combomenu.AddSubMenu(Qmenu);
                }
                var Wmenu = new Menu("W", "W");
                {
                    Wmenu.AddItem(new MenuItem("C-UseW", "Use W", true).SetValue(true));
                    combomenu.AddSubMenu(Wmenu);
                }
                var Emenu = new Menu("E", "E");
                {
                    Emenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));
                    
                    var EDetail = new Menu("Perfect Blockable Spells", "Perfect BLockable Spells");
                    {
                        foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                        {
                            foreach (var spell in Evade.Targeted.TargetedSpellDatabase.TargetedSpellDB)
                            {
                                if (spell.ChampionName == enemyhero.ChampionName.ToLower())
                                {
                                    EDetail.AddItem(new MenuItem(spell.ChampionName.ToString() + spell.Slot.ToString(), enemyhero.ChampionName.ToString() + spell.Slot.ToString(), true)).SetValue(true);
                                }
                            }
                        }
                        Emenu.AddSubMenu(EDetail);
                    }

                    combomenu.AddSubMenu(Emenu);
                }
                var Rmenu = new Menu("R", "R");
                {
                    Rmenu.AddItem(new MenuItem("C-UseR", "Use Auto R", true).SetValue(true));
                    combomenu.AddSubMenu(Rmenu);
                }
                config.AddSubMenu(combomenu);
            }

            var harassmenu = new Menu("Harass", "Harass");
            {
                var Qmenu = new Menu("Q", "Q");
                {
                    Qmenu.AddItem(new MenuItem("H-UseQ", "Use Q", true).SetValue(true));
                    harassmenu.AddSubMenu(Qmenu);
                }
                config.AddSubMenu(harassmenu);
            }

            var Farmmenu = new Menu("Farm", "Farm");
            {
                Farmmenu.AddItem(new MenuItem("F-UseQ", "Use Q", true).SetValue(false));
                Farmmenu.AddItem(new MenuItem("F-UseW", "Use W", true).SetValue(true));
                config.AddSubMenu(Farmmenu);
            }

            var KSmenu = new Menu("KS", "KS");
            {
                KSmenu.AddItem(new MenuItem("KS-UseQ", "Use Q KS", true).SetValue(true));

                config.AddSubMenu(KSmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("ManaM", "Mana Manager", true).SetValue(true));

                config.AddSubMenu(Miscmenu);
            }

            var Drawingmenu = new Menu("Drawings", "Drawings");
            {
                Drawingmenu.AddItem(new MenuItem("Qcircle", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                Drawingmenu.AddItem(new MenuItem("Wcircle", "W Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
                Drawingmenu.AddItem(new MenuItem("Ecircle", "E Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
                Drawingmenu.AddItem(new MenuItem("Rcircle", "R Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

                config.AddSubMenu(Drawingmenu);
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("C-UseR", true).GetValue<bool>() && R.IsReady())
                {
                    CastR(target);
                }
                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
                }
            }
            KSCheck(target);
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (config.Item("HarassActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("H-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
                }
                KSCheck(target);
            }
        }

        private void KSCheck(Obj_AI_Hero target)
        {
            if (target != null && target.Type == Player.Type)
            {
                if (config.Item("KS-UseQ", true).GetValue<bool>())
                {
                    var myDmg = Player.GetSpellDamage(target, SpellSlot.Q);
                    if (myDmg >= target.Health)
                    {
                        CastQ(target);
                    }
                }
            }
        }

        private void Farm()
        {
            var UseQ = config.Item("F-UseQ", true).GetValue<bool>();
            var UseW = config.Item("F-UseW", true).GetValue<bool>();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (UseW)
                {
                    var minions = MinionManager.GetMinions(Player.Position, 1100, MinionTypes.All, MinionTeam.NotAlly);

                    if (minions.Count() > 4)
                        CastW();
                }
                if (UseQ)
                {
                    var minions = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                    List<Vector2> minionsPos = new List<Vector2>();
                    
                    foreach (var x in minions)
                    {
                        minionsPos.Add(x.Position.To2D());
                    }

                    var predict = MinionManager.GetBestLineFarmLocation(minionsPos, Q.Width, Q.Range);

                    if (predict.MinionsHit > 4)
                    {
                        Q.Cast(predict.Position);
                    }
                }
            }
        }

        /// <Q>
        /// <param name="target"></param>

        private void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null || !ManaManager())
                return;

            var a = Q.GetPrediction(target, true);
            var b = target.GetWaypoints().Count;

            if (a.Hitchance == HitChance.Immobile || a.Hitchance == HitChance.Dashing)
            {
                Q.CastIfHitchanceEquals(target, HitChance.High);
            }
            else if (b <= 2)
            {
                Q.CastIfHitchanceEquals(target, HitChance.High);
            }
            else if (b > 2)
            {
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
            }
        }

        /// <W>
        /// <param name="target"></param>

        private void CastW()
        {
            if (!W.IsReady() || !ManaManager())
                return;

            W.Cast();
        }

        /// <E>
        /// <param name="target"></param>

        private void CastE()
        {
            if (!E.IsReady())
                return;

            E.Cast();
        }

        /// <R>
        /// <returns></returns>

        private void CastR(Obj_AI_Hero target)
        {
            if (!R.IsReady())
                return;

            if (Helpers.Helper.IsEnemyInRange(850).Count() > 2)
            {
                R.Cast();
            }
            else if (Helpers.Helper.IsEnemyInRange(850).Count() < 3 && !Q.IsReady() && Player.GetAutoAttackDamage(target) * 3 > target.Health)
            {
                R.Cast();
            }
        }

        /// <Others>
        ///     Others
        /// </summary>
        /// <returns></returns>

        private bool ManaManager()
        {
            var status = false;

            if (config.Item("ManaM", true).GetValue<bool>())
            {
                var Qmana = Q.Level == 0 ? 0 : QMana[Q.Level - 1];
                var Wmana = W.Level == 0 ? 0 : WMana[W.Level - 1];
                var Rmana = R.Level == 0 ? 0 : RMana[R.Level - 1];

                if (!R.IsReady())
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, 0, Rmana, 1, 1, 0, 0);
                else
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, 0, Rmana, 1, 1, 0, 1);
            }
            else
            {
                status = true;
            }

            return status;
        }

        /// <Events>
        /// <param name="args"></param>

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            Combo();
            Harass();
            Farm();
        }

        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
                return;

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active && target is Obj_AI_Hero)
                CastW();
            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical) != null)
                CastW();
            else if (target is Obj_AI_Hero)
                CastW();
        }

        protected override void TargetedDetector_TSDetector(Evade.Targeted.TargetSpell spell)
        {
            if (Player.IsDead)
                return;

            if (spell.IsActive && spell.Sender.IsEnemy && spell.Target.IsMe)
            {
                if (!IsFind(spell))
                    return;

                if (E.IsReady())
                {
                    debug<string>("TSDetector", spell.Spell.ChampionName + " - " + spell.Spell.Slot.ToString());
                    CastE();
                }
            }
        }

        private static bool IsFind(Evade.Targeted.TargetSpell args)
        {
            return config.Item(args.Spell.ChampionName.ToString() + args.Spell.Slot.ToString(), true).GetValue<bool>();
        }

        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            base.OnDraw(args);
            var QCircle = config.Item("Qcircle").GetValue<Circle>();
            var WCircle = config.Item("Wcircle").GetValue<Circle>();
            var ECircle = config.Item("Ecircle").GetValue<Circle>();
            var RCircle = config.Item("Rcircle").GetValue<Circle>();

            if (QCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, QCircle.Color);
            }

            if (WCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, WCircle.Color);
            }

            if (ECircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, ECircle.Color);
            }

            if (RCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, RCircle.Color);
            }
        }

        protected override void Drawing_OnEndScene(EventArgs args)
        {
            if (Player.IsDead)
                return;

            base.Drawing_OnEndScene(args);
        }

    }
}
