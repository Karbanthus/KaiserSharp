using System;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace OpProject.Champions
{
    class Nunu : CommonData
    {
        public Nunu()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OpProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 125);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 650);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            if (Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite") || Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
            {
                Console.WriteLine("SmiteName : " + Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name);
                Console.WriteLine("SmiteName : " + Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name);
            }
        }

        private void LoadMenu()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("C-UseQ", "Use Q (If LowHp)", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseW", "Use W", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseR", "Use R", true).SetValue(true));
                combomenu.AddItem(new MenuItem("RCount", "Use R", true).SetValue(new Slider(3, 1, 5)));
                config.AddSubMenu(combomenu);
            }

            var W = new Menu("CheckW", "Check Use W");
            {
                foreach (Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && !x.IsMe))
                {
                    W.AddItem(new MenuItem(allyhero.ChampionName, allyhero.ChampionName, true)).SetValue(true);
                }
                combomenu.AddSubMenu(W);
            }

            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("H-UseE", "Use E", true).SetValue(false));

                config.AddSubMenu(harassmenu);
            }

            var farmmenu = new Menu("Farm", "Farm");
            {
                farmmenu.AddItem(new MenuItem("LC-UseQ", "Use Q", true).SetValue(false));
                farmmenu.AddItem(new MenuItem("LC-UseW", "Use W", true).SetValue(false));
                farmmenu.AddItem(new MenuItem("LC-UseE", "Use E", true).SetValue(false));

                config.AddSubMenu(farmmenu);
            }

            var KSmenu = new Menu("KS", "KS");
            {
                KSmenu.AddItem(new MenuItem("KS-UseE", "Use E KS", true).SetValue(true));

                config.AddSubMenu(KSmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("UseEGapCloser", "Use E On Gap Closer", true).SetValue(true));

                config.AddSubMenu(Miscmenu);
            }

            var Drawingmenu = new Menu("Drawings", "Drawings");
            {
                Drawingmenu.AddItem(new MenuItem("Qcircle", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                Drawingmenu.AddItem(new MenuItem("Ecircle", "E Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
                Drawingmenu.AddItem(new MenuItem("Rcircle", "R Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

                config.AddSubMenu(Drawingmenu);
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                if (config.Item("C-UseE", true).GetValue<bool>() && E.IsReady() && target != null)
                {
                    CastE(target);
                }
                if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady())
                {
                    CastW();
                }
                KSCheck(target);

                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    AutoQ();
                }
                if (config.Item("C-UseR", true).GetValue<bool>() && R.IsReady())
                {
                    CastR();
                }
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (config.Item("HarassActive", true).GetValue<KeyBind>().Active && target != null)
            {
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
            if (config.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
            {
                var UseQ = config.Item("LC-UseQ", true).GetValue<bool>();
                var UseW = config.Item("LC-UseW", true).GetValue<bool>();
                var UseE = config.Item("LC-UseE", true).GetValue<bool>();
                var minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

                foreach (var minion in minions)
                {
                    if (UseQ)
                    {
                        CastQ(minion);
                    }

                    if (UseW)
                    {
                        CastW();
                    }

                    if (UseE)
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
        }

        /// <Q>
        /// <param name="target"></param>

        private void CastQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || target == null)
                return;

            if (Player.Distance(target.Position) < Q.Range)
            {
                Q.CastOnUnit(target);
            }
        }

        private void AutoQ()
        {
            var minions = MinionManager.GetMinions(500, MinionTypes.All, MinionTeam.Enemy);
            var HealthPer = Player.Health / Player.MaxHealth * 100;

            if (minions.Count > 0 && HealthPer < 40)
            {
                foreach (var minion in minions)
                {
                    if (minion.Health < Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        debug<string>("AutoQ", "Work");
                        CastQ(minion);
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

            var MostAD = Helpers.Helper.GetMostAD(true, W.Range);

            if (MostAD != null && config.Item(MostAD.ChampionName, true).GetValue<bool>())
            {
                W.CastOnUnit(MostAD);
            }
            else if (!Helpers.Helper.IsAllyInRange(2000))
            {
                W.CastOnUnit(Player);
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
                E.CastOnUnit(target);
            }
        }

        /// <R>
        /// <returns></returns>

        private void CastR()
        {
            if (!R.IsReady())
                return;

            var Rcount = Utility.CountEnemiesInRange(Player, R.Range);
            var ReqRcount = config.Item("RCount", true).GetValue<Slider>().Value;

            if (ReqRcount <= Rcount)
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
            Farm();
        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("UseEGapCloser", true).GetValue<bool>())
                return;

            if (E.IsReady() && gapcloser.Sender.IsEnemy && Player.Distance(gapcloser.Sender.Position) < E.Range)
            {
                debug<string>("OnEnemyGapcloser", "GapCloser");
                E.CastOnUnit(gapcloser.Sender);
            }
        }

        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            base.OnDraw(args);
            var QCircle = config.Item("Qcircle").GetValue<Circle>();
            var ECircle = config.Item("Ecircle").GetValue<Circle>();
            var RCircle = config.Item("Rcircle").GetValue<Circle>();

            if (QCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, QCircle.Color);
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

        protected override void DamagePrediction_OnSpellDmg(Obj_AI_Hero sender, Obj_AI_Hero target, float dmg)
        {
            if (!sender.IsMe)
                return;

            debug<string>("DamagePrediction_OnSpellDmg-", target.ChampionName);
            debug<string>("DamagePrediction_OnSpellDmg-", dmg.ToString());
        }
    }
}
