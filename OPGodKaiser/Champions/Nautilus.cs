using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OPGodKaiser.Champions
{
    class Nautilus  : CommonData
    {
        public Nautilus()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OPGodKaiserProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1080);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 825);

            Q.SetSkillshot(0.25f, 80f, 1200f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.1f, 400f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("C-UseQ", "Use Q", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseW", "Use W", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));
                config.AddSubMenu(combomenu);
            }

            var initatorR = new Menu("R Initiator", "R Initiator");
            {
                initatorR.AddItem(new MenuItem("C-UseR", "Use R", true).SetValue(true));
                foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                {
                    initatorR.AddItem(new MenuItem(enemyhero.ChampionName, enemyhero.ChampionName, true)).SetValue(true);
                }
                combomenu.AddSubMenu(initatorR);
            }

            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("H-UseQ", "Use Q", true).SetValue(true));
                harassmenu.AddItem(new MenuItem("H-UseW", "Use W", true).SetValue(true));
                harassmenu.AddItem(new MenuItem("H-UseE", "Use E", true).SetValue(false));

                config.AddSubMenu(harassmenu);
            }

            var KSmenu = new Menu("KS", "KS");
            {
                KSmenu.AddItem(new MenuItem("KS-UseQ", "Use Q KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseE", "Use E KS", true).SetValue(true));

                config.AddSubMenu(KSmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("AutoShield", "W AutoShield", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseWGapCloser", "Use W On Gap Closer", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseQInterrupt", "Use Q On Interrupt", true).SetValue(true));

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
                StunLogic();
                if (config.Item("C-UseR", true).GetValue<bool>() && R.IsReady())
                {
                    CastR();
                }
                if (config.Item("C-UseE", true).GetValue<bool>() && E.IsReady())
                {
                    CastE(target);
                }
                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
                }
                if (!Q.IsReady() && !E.IsReady() && config.Item("C-UseW", true).GetValue<bool>() && W.IsReady() && Player.Distance(target) < 700)
                {
                    CastW();
                }
                KSCheck(target);
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (config.Item("HarassActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("H-UseE", true).GetValue<bool>() && E.IsReady())
                {
                    CastE(target);
                }
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

        private void StunLogic()
        {
            foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && Player.Distance(x) < 1000 && isValidTarget(x)))
            {
                if (!enemyhero.HasBuff("nautiluspassivecheck"))
                {
                    if (Orbwalker.InAutoAttackRange(enemyhero) && Orbwalking.CanAttack())
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, enemyhero);
                    }
                }
            }
        }

        /// <Q>
        /// <param name="target"></param>

        private void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null || EnemyHasShield(target))
                return;

            if (!E.IsReady() || (E.IsReady() && E.Range < Player.Distance(target)))
            {
                var predict = Q.GetPrediction(target);
                var positions = new List<Vector3> { predict.UnitPosition, predict.CastPosition, target.Position };
                var coll = GetWallCollision(positions, Player.Position);

                //Console.WriteLine("Collision : " + coll.Count);

                if (predict.Hitchance >= HitChance.High && coll.Count < 1)
                {
                    Q.Cast(target);
                    if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady())
                    {
                        CastW();
                    }
                }
            }
        }

        /// <W>
        /// <param name="target"></param>

        private void CastW()
        {
            if (!W.IsReady())
                return;

            W.Cast();
        }

        private void AutoShield()
        {
            if (!W.IsReady() || !config.Item("AutoShield", true).GetValue<bool>())
                return;
            
            Single CurHpPer = 0;
            var PastHpPer = Player.Health / Player.MaxHealth * 100;
            Utility.DelayAction.Add(500, () => CurHpPer = Player.Health / Player.MaxHealth * 100);
            //Console.WriteLine("curhp :" + CurHpPer);
            //Console.WriteLine("pasthp :" + PastHpPer);
            if (CurHpPer != 0)
            {
                if (PastHpPer - CurHpPer > 10)
                {
                    debug<string>("AutoShield", "Work");
                    W.Cast();
                    CurHpPer = 0;
                }
                else
                    CurHpPer = 0;
            }
        }

        /// <E>
        /// <param name="target"></param>

        private void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !isValidTarget(target) || target == null)
                return;

            if (Player.Distance(target) < E.Range)
            {
                E.Cast();
                if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady())
                {
                    CastW();
                }
            }
        }

        /// <R>
        /// <returns></returns>
        
        private void CastR()
        {
            if (!R.IsReady())
                return;

            var Rtarget = FindDmgStrongHero(HeroManager.Enemies, 1650);

            if (Rtarget != null && Player.Distance(Rtarget) < R.Range)
            {
                R.CastOnUnit(Rtarget);
            }
        }

        /// <Events>
        /// <param name="args"></param>

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            Combo();
            Harass();

            AutoShield();

        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("UseWGapCloser", true).GetValue<bool>())
                return;

            if (W.IsReady() && gapcloser.Sender.IsEnemy)
            {
                debug<string>("OnEnemyGapcloser", "GapCloser");
                W.Cast();
            }
        }

        protected override void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (!config.Item("UseQInterrupt", true).GetValue<bool>())
                return;

                if (Player.Distance(sender.Position) < Q.Range && sender.IsEnemy && sender != null)
                {
                    if (Q.IsReady() && Q.GetPrediction(sender).Hitchance >= HitChance.High)
                    {
                        debug<string>("OnPossibleToInterrupt", "Interrupt");
                        Q.Cast(sender);
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
