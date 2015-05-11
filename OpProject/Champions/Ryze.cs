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
    class Ryze : CommonData
    {
        #region Init

        public Ryze()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OpProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private bool PassiveCount { get; set; }
        private bool PassiveCharged { get; set; }

        #region SkillData

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine); // 1700 , 2000

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            QMana = new[] { 30, 35, 40, 45, 50 };
            WMana = new[] { 60, 70, 80, 90, 100 };
            EMana = new[] { 60, 70, 80, 90, 100 };
            RMana = new[] { 0, 0, 0 };
            
        }

        #endregion

        #region Menu

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
                    Wmenu.AddItem(new MenuItem("AutoW", "Use Auto W Immobile", true).SetValue(true));
                    Wmenu.AddItem(new MenuItem("AutoWInTower", "Use Auto W If In tower", true).SetValue(true));
                    combomenu.AddSubMenu(Wmenu);
                }
                var Emenu = new Menu("E", "E");
                {
                    Emenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));

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
                var Wmenu = new Menu("W", "W");
                {
                    Wmenu.AddItem(new MenuItem("H-UseW", "Use W", true).SetValue(true));
                    harassmenu.AddSubMenu(Wmenu);
                }
                var Emenu = new Menu("E", "E");
                {
                    Emenu.AddItem(new MenuItem("H-UseE", "Use E", true).SetValue(true));
                    harassmenu.AddSubMenu(Emenu);
                }
                config.AddSubMenu(harassmenu);
            }

            var Farmmenu = new Menu("Farm", "Farm");
            {
                Farmmenu.AddItem(new MenuItem("LH-UseQ", "LastHit Use Q", true).SetValue(true));
                Farmmenu.AddItem(new MenuItem("F-UseQ", "Use Q", true).SetValue(true));
                Farmmenu.AddItem(new MenuItem("F-UseW", "Use W", true).SetValue(false));
                Farmmenu.AddItem(new MenuItem("F-UseE", "Use E", true).SetValue(true));
                Farmmenu.AddItem(new MenuItem("F-UseR", "Use R", true).SetValue(true));
                config.AddSubMenu(Farmmenu);
            }

            var KSmenu = new Menu("KS", "KS");
            {
                KSmenu.AddItem(new MenuItem("KS-UseQ", "Use Q KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseW", "Use W KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseE", "Use E KS", true).SetValue(true));

                config.AddSubMenu(KSmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("ManaM", "Mana Manager", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("Use-Anti", "Use W Antigapclose", true).SetValue(true));

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

        #endregion

        #endregion

        #region Logic

        private void Combo()
        {
            var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady())
                {
                    CastW(target);
                }
                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady() && !WCheck(target))
                {
                    CastQ(Qtarget);
                }
                if (config.Item("C-UseE", true).GetValue<bool>() && E.IsReady() && !WCheck(target))
                {
                    CastE(target);
                }
                if (config.Item("C-UseR", true).GetValue<bool>() && R.IsReady() && !WCheck(target) && RCheck(target))
                {
                    CastR();
                }
                if (config.Item("C-UseR", true).GetValue<bool>() && R.IsReady() && (PassiveCharged || PassiveCount || Player.Health < Player.MaxHealth * 0.3))
                {
                    CastR();
                }
                KSCheck(target);
            }
        }
        
        private void Harass()
        {
            var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (config.Item("HarassActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("H-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(Qtarget);
                }
                if (config.Item("H-UseW", true).GetValue<bool>() && W.IsReady())
                {
                    CastW(target);
                }
                if (config.Item("H-UseE", true).GetValue<bool>() && E.IsReady())
                {
                    CastE(target);
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
                if (config.Item("KS-UseE", true).GetValue<bool>())
                {
                    var myDmg = Player.GetSpellDamage(target, SpellSlot.E);
                    if (myDmg >= target.Health)
                    {
                        CastE(target);
                    }
                }
            }
        }

        private void Farm()
        {
            var LHUseQ = config.Item("LH-UseQ", true).GetValue<bool>();
            var UseQ = config.Item("F-UseQ", true).GetValue<bool>();
            var UseW = config.Item("F-UseW", true).GetValue<bool>();
            var UseE = config.Item("F-UseE", true).GetValue<bool>();
            var UseR = config.Item("F-UseR", true).GetValue<bool>();
            var minions = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (UseR)
                {
                    if (minions.Count > 5)
                    {
                        CastR();
                    }
                }
                if (UseQ)
                {
                    foreach (var minion in minions)
                    {
                        if (HealthPrediction.GetHealthPrediction(minion, (int)(Q.Delay + Player.Distance(minion.Position) / Q.Speed)) + 15 < Q.GetDamage(minion))
                        {
                            CastQ(minion);
                        }
                    }
                }
                if (UseW)
                {
                    foreach (var minion in minions)
                    {
                        if (minion.Health + 15 < W.GetDamage(minion))
                        {
                            CastW(minion);
                        }
                    }
                }
                if (UseE)
                {
                    foreach (var minion in minions)
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                var minionss = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly);

                if (LHUseQ)
                {
                    foreach (var minion in minionss)
                    {
                        if (HealthPrediction.GetHealthPrediction(minion, (int)(Q.Delay + Player.Distance(minion.Position) / Q.Speed)) + 15 < Q.GetDamage(minion))
                        {
                            CastQ(minion);
                        }
                    }
                }
            }
        }

        #endregion

        #region Q

        private void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null || !ManaManager())
                return;

            var predict = Q.GetPrediction(target);
            var mode = PassiveCharged ? 0 : 1;

            if (mode == 1)
            {
                if (predict.Hitchance >= HitChance.High)
                {
                    Q.Cast(target);
                }
                else if (predict.CollisionObjects.Count > 1)
                {
                    Q.CastOnBestTarget();
                }
            }
            else if (mode == 0)
            {
                Q.CastOnBestTarget();
            }
        }

        private void CastQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || target == null || !ManaManager())
                return;

            var predict = Q.GetPrediction(target);

            if (predict.Hitchance >= HitChance.High)
            {
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

                if (predict.Hitchance == HitChance.Immobile && predict.CollisionObjects.Count < 1)
                {
                    Q.Cast(enemyhero);
                }
            }
        }

        #endregion 

        #region W

        private void CastW(Obj_AI_Hero target)
        {
            if (!W.IsReady() || !ManaManager() || target == null || !isValidTarget(target))
                return;

            if (Player.Distance(target.Position) < W.Range)
            {
                W.CastOnUnit(target);
            }
        }

        private void CastW(Obj_AI_Base target)
        {
            if (!W.IsReady() || !ManaManager() || target == null)
                return;

            W.CastOnUnit(target);
        }

        private bool WCheck(Obj_AI_Hero target)
        {
            var status = false;

            if (target != null && isValidTarget(target) && W.IsReady())
            {
                if (Player.Distance(target.Position) > 570 && Player.Distance(target.Position) <= 600)
                {
                    status = true;
                }
            }

            return status;
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

        private void InTower()
        {
            if (!config.Item("AutoWInTower", true).GetValue<bool>() || !W.IsReady())
                return;

            foreach (var enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x) && !x.IsDead))
            {
                if (Helpers.Helper.IsUnderTurret(enemyhero, true) && Player.Distance(enemyhero.Position) < W.Range)
                {
                    CastW(enemyhero);
                }
            }
        }

        #endregion

        #region E

        private void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || target == null || !isValidTarget(target))
                return;

            if (Player.Distance(target.Position) < E.Range)
            {
                E.CastOnUnit(target);
            }
        }

        #endregion

        #region R

        private void CastR()
        {
            if (!R.IsReady())
                return;

            R.Cast();
        }

        private bool RCheck(Obj_AI_Hero target)
        {
            var status = false;

            if (target != null && R.IsReady() && isValidTarget(target))
            {
                if (Helpers.Helper.IsEnemyInRange(200, target).Count() > 1)
                {
                    status = true;
                }
            }
            return status;
        }

        #endregion

        #region others

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
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, Emana, Rmana, 2, 1, 1, 0);
                else
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, Emana, Rmana, 2, 1, 1, 1);
            }
            else
            {
                status = true;
            }

            return status;
        }

        private void CheckBuff()
        {
            if (Player.Buffs.Any(x => x.Name.ToLower() == "ryzepassivestack" && x.Count > 3))
            {
                PassiveCount = true;
            }
            else
            {
                PassiveCount = false;
            }

            if (Player.Buffs.Any(x => x.Name.ToLower() == "ryzepassivecharged"))
            {
                PassiveCharged = true;
            }
            else
            {
                PassiveCharged = false;
            }
        }

        #endregion

        #region Events

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            Combo();
            Harass();
            Farm();

            AutoQ();
            AutoW();
            InTower();

            CheckBuff();
        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("Use-Anti", true).GetValue<bool>() || !W.IsReady())
                return;

            if (Player.Distance(gapcloser.Sender.Position) < W.Range && gapcloser.Sender.IsEnemy)
            {
                CastW(gapcloser.Sender);
            }
        }

        protected override void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!config.Item("ComboActive", true).GetValue<KeyBind>().Active)
                return;

            if (Q.IsReady() || W.IsReady() || E.IsReady())
                args.Process = false;
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

        #endregion

    }
}
