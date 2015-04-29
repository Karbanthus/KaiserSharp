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
    class Morgana : CommonData
    {
        public Morgana()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OpProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 280f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            QMana = new[] { 50, 60, 70, 80, 90 };
            WMana = new[] { 70, 85, 100, 115, 130 };
            EMana = new[] { 50, 50, 50, 50, 50 };
            RMana = new[] { 100, 100, 100 };

        }

        private void LoadMenu()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                var Qmenu = new Menu("Q", "Q");
                {
                    Qmenu.AddItem(new MenuItem("C-UseQ", "Use Q", true).SetValue(true));
                    Qmenu.AddItem(new MenuItem("AutoQ", "Use Auto Q Immobile", true).SetValue(true));
                    combomenu.AddSubMenu(Qmenu);
                }
                var Wmenu = new Menu("W", "W");
                {
                    Wmenu.AddItem(new MenuItem("C-UseW", "Use W", true).SetValue(true));
                    Qmenu.AddItem(new MenuItem("CheckQ", "Only Use If Q Hit", true).SetValue(true));
                    Wmenu.AddItem(new MenuItem("AutoW", "Use Auto W Immobile", true).SetValue(true));
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
                                    EDetail.AddItem(new MenuItem(spell.ChampionName.ToString() + spell.Slot.ToString(), enemyhero.ChampionName.ToString() + " - " + spell.Slot.ToString(), true)).SetValue(true);
                                }
                            }
                        }
                        Emenu.AddSubMenu(EDetail);
                    }

                    var EDetail2 = new Menu("Use for Him", "Use for Him");
                    {
                        foreach (Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly))
                        {
                            EDetail2.AddItem(new MenuItem(allyhero.ChampionName.ToLower().ToString(), allyhero.ChampionName.ToString(), true)).SetValue(true);
                        }
                        Emenu.AddSubMenu(EDetail2);
                    }

                    combomenu.AddSubMenu(Emenu);
                }
                var Rmenu = new Menu("R", "R");
                {
                    Rmenu.AddItem(new MenuItem("C-UseR", "Use Auto R", true).SetValue(true));
                    Rmenu.AddItem(new MenuItem("Rcount", "Min of Enemies", true).SetValue(new Slider(2, 1, 5)));
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
                var Wmenu = new Menu("W", "W");
                {
                    Wmenu.AddItem(new MenuItem("H-UseW", "Use W", true).SetValue(true));
                    harassmenu.AddSubMenu(Wmenu);
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
                KSmenu.AddItem(new MenuItem("KS-UseW", "Use W KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseR", "Use R KS", true).SetValue(false));

                config.AddSubMenu(KSmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("ManaM", "Mana Manager", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("Use-Anti", "Use Q Antigapclose", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("Use-Interrupt", "Use Q Interrupt", true).SetValue(true));

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
                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
                }
                if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady() && !config.Item("CheckQ", true).GetValue<bool>())
                {
                    CastW(target);
                }
                KSCheck(target);
            }

            if (config.Item("C-UseR", true).GetValue<bool>() && R.IsReady())
            {
                CastR();
            }
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
                if (config.Item("H-UseW", true).GetValue<bool>() && W.IsReady())
                {
                    CastW(target);
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
                if (config.Item("KS-UseW", true).GetValue<bool>())
                {
                    var myDmg = Player.GetSpellDamage(target, SpellSlot.W);
                    if (myDmg >= target.Health)
                    {
                        CastW(target);
                    }
                }
                if (config.Item("KS-UseR", true).GetValue<bool>())
                {
                    var myDmg = Player.GetSpellDamage(target, SpellSlot.R);
                    if (myDmg >= target.Health)
                    {
                        CastR();
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
                if (UseQ)
                {
                    var minions = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All, MinionTeam.NotAlly);

                    foreach (var minion in minions.Where(x => x.Health < Q.GetDamage(x) - 15))
                    {
                        Q.Cast(minion);
                    }
                }
                if (UseW)
                {
                    var minions = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                    List<Vector2> minionsPos = new List<Vector2>();

                    foreach (var x in minions)
                    {
                        minionsPos.Add(x.Position.To2D());
                    }

                    var predict = MinionManager.GetBestCircularFarmLocation(minionsPos, W.Width, W.Range);

                    if (predict.MinionsHit > 2)
                    {
                        W.Cast(predict.Position);
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

            var predict = Q.GetPrediction(target);

            if (predict.Hitchance >= HitChance.High)
            {
                debug<string>("CastQ", target.GetWaypoints().Count.ToString());
                Q.Cast(target);
            }
        }

        private void AutoQ()
        {
            if (!config.Item("AutoQ", true).GetValue<bool>() || !Q.IsReady())
                return;

            foreach (var enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x) && Player.Distance(x.Position) < Q.Range))
            {
                var predict = Q.GetPrediction(enemyhero, true);

                if (predict.Hitchance == HitChance.Immobile)
                {
                    Q.Cast(enemyhero);
                }
            }
        }

        /// <W>
        /// <param name="target"></param>

        private void CastW(Obj_AI_Hero target)
        {
            if (!W.IsReady() || !ManaManager())
                return;

            var predict = W.GetPrediction(target, true);

            if (predict.Hitchance >= HitChance.High)
            {
                W.Cast(predict.CastPosition);
            }
        }

        private void AutoW()
        {
            if (!config.Item("AutoW", true).GetValue<bool>() || !W.IsReady())
                return;

            foreach (var enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x) && Player.Distance(x.Position) < W.Range))
            {
                var predict = W.GetPrediction(enemyhero, true);

                if (predict.Hitchance == HitChance.Immobile)
                {
                    W.Cast(enemyhero);
                }
            }
        }

        /// <E>
        /// <param name="target"></param>

        private void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || target == null || !isValidTarget(target))
                return;

            E.CastOnUnit(target);
        }

        /// <R>
        /// <returns></returns>

        private void CastR()
        {
            if (!R.IsReady())
                return;

            var count = config.Item("Rcount", true).GetValue<Slider>().Value;

            if (Helpers.Helper.IsEnemyInRange(R.Range - 35).Count() >= count)
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
                var Emana = E.Level == 0 ? 0 : EMana[E.Level - 1];
                var Rmana = R.Level == 0 ? 0 : RMana[R.Level - 1];

                if (!R.IsReady())
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, Emana, Rmana, 1, 0, 1, 0);
                else
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, Emana, Rmana, 1, 0, 1, 1);
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
            AutoQ();
            AutoW();
        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("Use-Anti", true).GetValue<bool>() || !Q.IsReady())
                return;

            if (Player.Distance(gapcloser.Sender.Position) < Q.Range && gapcloser.Sender.IsEnemy)
            {
                CastQ(gapcloser.Sender);
            }
        }

        protected override void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsEnemy || !Q.IsReady() || !config.Item("Use-Interrupt", true).GetValue<bool>())
                return;

            if (args.DangerLevel == Interrupter2.DangerLevel.High && Player.Distance(sender.Position) < Q.Range)
            {
                CastQ(sender);
            }
        }

        protected override void TargetedDetector_TSDetector(Evade.Targeted.TargetSpell spell)
        {
            if (Player.IsDead)
                return;

            if (spell.IsActive && spell.Sender.IsEnemy && (spell.Target.IsMe || spell.Target.IsAlly))
            {
                if (!config.Item(spell.Spell.ChampionName.ToLower().ToString() + spell.Spell.Slot.ToString(), true).GetValue<bool>())
                    return;

                if (!config.Item(spell.Target.ChampionName.ToLower().ToString(), true).GetValue<bool>())
                    return;

                if (E.IsReady() && Player.Distance(spell.Target.Position) < E.Range)
                {
                    debug<string>("TSDetector", spell.Spell.ChampionName + " - " + spell.Spell.Slot.ToString());
                    CastE(spell.Target);
                }
            }
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
