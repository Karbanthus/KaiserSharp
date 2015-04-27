using System;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace OPGodKaiser.Champions
{
    class Amumu : CommonData
    {
        public Amumu()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OPGodKaiserProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.25f, 100f, 2000f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 550f, float.MaxValue, false, SkillshotType.SkillshotCircle);

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

            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("H-UseQ", "Use Q", true).SetValue(true));

                config.AddSubMenu(harassmenu);
            }

            var KSmenu = new Menu("KS", "KS");
            {
                KSmenu.AddItem(new MenuItem("KS-UseQ", "Use Q KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseE", "Use E KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseR", "Use R KS", true).SetValue(false));

                config.AddSubMenu(KSmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("AutoUlt", "Use AutoUlt", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UltCount", "Min Enemies Count", true).SetValue(new Slider(2, 1, 5)));
                Miscmenu.AddItem(new MenuItem("BlockR", "Block R (IF no Enemy in Range)", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("safemana", "Use W Off (If no enemies or minion in Wrange)", true).SetValue(true));
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
            var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var Wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            AutoR();
            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady() && Qtarget != null)
                {
                    CastQ(Qtarget);
                }
                if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady() && Wtarget != null)
                {
                    CastW();
                }
                if (config.Item("C-UseE", true).GetValue<bool>() && E.IsReady() && Etarget != null)
                {
                    CastE(Etarget);
                }
                KSCheck();
            }
        }

        private void Harass()
        {
            var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (config.Item("HarassActive", true).GetValue<KeyBind>().Active && Qtarget != null)
            {
                if (config.Item("H-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(Qtarget);
                }
            }
        }

        private void KSCheck()
        {
            var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var Rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (config.Item("KS-UseQ", true).GetValue<bool>() && Qtarget != null)
                {
                    var myDmg = Player.GetSpellDamage(Qtarget, SpellSlot.Q);
                    if (myDmg >= Qtarget.Health)
                    {
                        CastQ(Qtarget);
                    }
                }
                if (config.Item("KS-UseE", true).GetValue<bool>() && Etarget != null)
                {
                    var myDmg = Player.GetSpellDamage(Etarget, SpellSlot.E);
                    if (myDmg >= Etarget.Health)
                    {
                        CastE(Etarget);
                    }
                }
                if (config.Item("KS-UseR", true).GetValue<bool>() && Rtarget != null)
                {
                    var myDmg = Player.GetSpellDamage(Rtarget, SpellSlot.R);
                    if (myDmg >= Rtarget.Health && Player.Distance(Rtarget) < R.Range - 15)
                    {
                        R.Cast();
                    }
                }
        }

        /// <Q>
        /// <param name="target"></param>

        private void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null || EnemyHasShield(target))
                return;

            var predict = Q.GetPrediction(target);

            if (predict.Hitchance >= HitChance.High)
            {
                Q.Cast(target);
            }
        }

        /// <W>
        /// <param name="target"></param>

        private void CastW()
        {
            if (!W.IsReady() || Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1) // 1 = off , 2 = on
                return;

            var Range = W.Range + 20;
            var Wtarget = TargetSelector.GetTarget(Range, TargetSelector.DamageType.Magical);

            if (W.IsReady() && Wtarget != null)
            {
                W.Cast();
            }
        }

        private void SafeMana()
        {
            if (!W.IsReady() || Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 2 || !config.Item("safemana", true).GetValue<bool>()) // 1 = off , 2 = on
                return;

            var Range = W.Range + 100;
            var Wtarget = TargetSelector.GetTarget(Range, TargetSelector.DamageType.Magical);
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Range, MinionTypes.All, MinionTeam.NotAlly);

            if (W.IsReady() && Wtarget == null && minions.Count == 0)
            {
                W.Cast();
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
            }
        }

        /// <R>
        /// <returns></returns>

        private void AutoR()
        {
            if (!R.IsReady() || !config.Item("AutoUlt", true).GetValue<bool>())
                return;

            var count = Utility.CountEnemiesInRange(R.Range);
            var ReqCount = config.Item("UltCount", true).GetValue<Slider>().Value;

            if (count >= ReqCount)
            {
                R.Cast();
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

            if (config.Item("safemana", true).GetValue<bool>())
            {
                SafeMana();
            }
        }

        protected override void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (!R.IsReady() || !config.Item("BlockR", true).GetValue<bool>())
                return;

            var a = Utility.CountEnemiesInRange(Player.Position, R.Range - 25);
            if (args.Slot == SpellSlot.R && a < 1)
            {
                args.Process = false;
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
