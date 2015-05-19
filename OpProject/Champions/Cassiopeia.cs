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
    class Cassiopeia : CommonData
    {
        #region Init

        public Cassiopeia()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OpProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private float ECastTime { get; set; }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 825);
            W = new Spell(SpellSlot.W, 825);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 825);

            Q.SetSkillshot(0.6f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f,90f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(0.5f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            QMana = new[] { 40, 50, 60, 70, 80 };
            WMana = new[] { 40, 50, 60, 70, 80 };
            EMana = new[] { 50, 60, 70, 80, 90 };
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
                    combomenu.AddSubMenu(Wmenu);
                }
                var Emenu = new Menu("E", "E");
                {
                    Emenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));
                    Emenu.AddItem(new MenuItem("AutoE", "Auto Use E Enemy Has Poison", true).SetValue(true));
                    Emenu.AddItem(new MenuItem("UseEOnlyPosion", "Use E Only Enemey has Posion", true).SetValue(true));
                    Emenu.AddItem(new MenuItem("EDelay", "E Delay (ms)", true).SetValue<Slider>(new Slider(700, 0, 2000)));
                    combomenu.AddSubMenu(Emenu);
                }
                var Rmenu = new Menu("R", "R");
                {
                    Rmenu.AddItem(new MenuItem("C-UseR", "Use R", true).SetValue(true));
                    Rmenu.AddItem(new MenuItem("RCount", "Min Enemies RCount", true).SetValue<Slider>(new Slider(3, 1, 5)));
                    Rmenu.AddItem(new MenuItem("RFaceCount", "Min Enemies IsFacing Count", true).SetValue<Slider>(new Slider(2, 1, 5)));
                    Rmenu.AddItem(new MenuItem("RKillCount", "Min Enemies Killable Count", true).SetValue<Slider>(new Slider(2, 1, 5)));
                    combomenu.AddSubMenu(Rmenu);
                }
                config.AddSubMenu(combomenu);
            }

            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("HarassT", "Harass Toggle Key", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));

                var Qmenu = new Menu("Q", "Q");
                {
                    Qmenu.AddItem(new MenuItem("H-UseQ", "Use Q", true).SetValue(true));
                    harassmenu.AddSubMenu(Qmenu);
                }
                var Wmenu = new Menu("W", "W");
                {
                    Wmenu.AddItem(new MenuItem("H-UseW", "Use W", true).SetValue(false));
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
                var LastHitMenu = new Menu("LastHit", "LastHit");
                {
                    LastHitMenu.AddItem(new MenuItem("LH-UseQ", "Use Q", true).SetValue(false));
                    LastHitMenu.AddItem(new MenuItem("LH-UseE", "Use E", true).SetValue(true));
                    LastHitMenu.AddItem(new MenuItem("LH-UseQE", "Use QE Burst LastHit", true).SetValue(true));
                    LastHitMenu.AddItem(new MenuItem("LH-UseEOnlyPosion", "Use E Only Minion has Posion", true).SetValue(false));
                    Farmmenu.AddSubMenu(LastHitMenu);
                }

                var LaneClearMenu = new Menu("LaneClear", "LaneClear");
                {
                    LaneClearMenu.AddItem(new MenuItem("LC-UseQ", "Use Q", true).SetValue(true));
                    LaneClearMenu.AddItem(new MenuItem("LC-UseW", "Use W", true).SetValue(true));
                    LaneClearMenu.AddItem(new MenuItem("LC-UseE", "Use E", true).SetValue(true));
                    LaneClearMenu.AddItem(new MenuItem("LC-UseEOnlyPosion", "Use E Only Minion has Posion", true).SetValue(true));
                    Farmmenu.AddSubMenu(LaneClearMenu);
                }

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
                Miscmenu.AddItem(new MenuItem("ManaM", "Mana Manager", true).SetValue(false));
                Miscmenu.AddItem(new MenuItem("BlockAA", "Block AA While ECast", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("BlockR", "Block R", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("Inter-UseR", "Use Interrupt R", true).SetValue(true));

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

        #region Logic

        private void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
                }
                if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady() && !CheckPoison(target) && !Q.IsReady())
                {
                    CastW(target);
                }
                if (config.Item("C-UseE", true).GetValue<bool>() && E.IsReady())
                {
                    CastE(target);
                }
                if (config.Item("C-UseR", true).GetValue<bool>() && R.IsReady())
                {
                    CastR();
                }
            }

            KSCheck(target);
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if ((config.Item("HarassActive", true).GetValue<KeyBind>().Active || config.Item("HarassT", true).GetValue<KeyBind>().Active) && target != null)
            {
                if (config.Item("H-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
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
            if (target != null)
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
                if (config.Item("KS-UseE", true).GetValue<bool>() && !Q.IsReady() && !W.IsReady())
                {
                    var myDmg = Player.GetSpellDamage(target, SpellSlot.E);
                    if (myDmg >= target.Health && 
                        Player.Distance(target.Position) <= E.Range &&
                        Helpers.Helper.IsEnemyInRange(800).Count() < 1)
                    {
                        E.CastOnUnit(target);
                    }
                }
            }
        }

        private void Farm()
        {
            var LHUseQ = config.Item("LH-UseQ", true).GetValue<bool>();
            var LHUseE = config.Item("LH-UseE", true).GetValue<bool>();
            var LHUseQE = config.Item("LH-UseQE", true).GetValue<bool>();

            var LCUseQ = config.Item("LC-UseQ", true).GetValue<bool>();
            var LCUseW = config.Item("LC-UseW", true).GetValue<bool>();
            var LCUseE = config.Item("LC-UseE", true).GetValue<bool>();

            switch(Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    var LHMinions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
                    
                    if (Q.IsReady())
                    {
                        if (LHUseQE && E.IsReady())
                        {
                            foreach (var minion in LHMinions.Where(x => HealthPrediction.GetHealthPrediction(x, (int)(E.Delay + Player.Distance(x.Position) / E.Speed)) + 25 < E.GetDamage(x) + Q.GetDamage(x) && !x.IsDead))
                            {
                                if (Player.Distance(minion.Position) < E.Range)
                                {
                                    CastQ(minion);
                                    CastE(minion);
                                }
                            }
                        }
                        else if (LHUseQ)
                        {
                            foreach (var minion in LHMinions.Where(x => HealthPrediction.GetHealthPrediction(x, (int)(Q.Delay + Player.Distance(x.Position) / Q.Speed)) + 15 < Q.GetDamage(x) && !x.IsDead))
                            {
                                if (Player.Distance(minion.Position) < Q.Range)
                                {
                                    CastQ(minion);
                                }
                            }
                        }
                    }

                    if (LHUseE && E.IsReady())
                    {
                        var EMinions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);
                        var UseEOnlyPosion = config.Item("LH-UseEOnlyPosion", true).GetValue<bool>();

                        foreach (var minion in EMinions.Where(x => HealthPrediction.GetHealthPrediction(x, (int)(E.Delay + Player.Distance(x.Position) / E.Speed)) + 15 < E.GetDamage(x) && !x.IsDead))
                        {
                            if (UseEOnlyPosion && CheckPoison(minion))
                            {
                                E.CastOnUnit(minion);
                            }
                            else if (!UseEOnlyPosion)
                            {
                                E.CastOnUnit(minion);
                            }
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    var LineMinions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly);

                    if (LCUseW && W.IsReady())
                    {
                        var predict = W.GetCircularFarmLocation(LineMinions);
                        
                        if (predict.MinionsHit > 2)
                        {
                            W.Cast(predict.Position);
                        }
                    }

                    if (LCUseQ && Q.IsReady())
                    {
                        var NonQPoisoned = LineMinions.Where(x => !CheckPoison(x)).ToList();
                        
                        if (NonQPoisoned.Count() > 0)
                        {
                            var predict = Q.GetCircularFarmLocation(NonQPoisoned);

                            if (predict.MinionsHit > 0)
                            {
                                Q.Cast(predict.Position);
                            }
                        }
                    }

                    if (LCUseE && E.IsReady())
                    {
                        var EMinions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);
                        var UseEOnlyPosion = config.Item("LC-UseEOnlyPosion", true).GetValue<bool>();

                        foreach (var minion in EMinions.Where(x => HealthPrediction.GetHealthPrediction(x, (int)(E.Delay + Player.Distance(x.Position) / E.Speed)) + 15 < E.GetDamage(x) && !x.IsDead))
                        {
                            if (UseEOnlyPosion && CheckPoison(minion))
                            {
                                E.CastOnUnit(minion);
                            }
                            else if (!UseEOnlyPosion)
                            {
                                E.CastOnUnit(minion);
                            }
                        }
                    }
                    if (LCUseE && E.IsReady())
                    {
                        var EMinions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral);

                        foreach (var minion in EMinions.Where(x => !x.IsDead))
                        {
                            if (CheckPoison(minion))
                            {
                                E.CastOnUnit(minion);
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Q

        private void CastQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null || !ManaManager())
                return;

            var predict = Q.GetPrediction(target, true);

            if (predict.Hitchance >= HitChance.High &&
                Player.Distance(target.ServerPosition) < Q.Range)
            {
                Q.Cast(predict.CastPosition);
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

        #endregion

        #region W

        private void CastW(Obj_AI_Base target)
        {
            if (!W.IsReady() || !ManaManager() || target == null || !isValidTarget(target))
                return;

            var predict = W.GetPrediction(target, true);

            if (predict.Hitchance >= HitChance.High &&
                Player.Distance(target.ServerPosition) < W.Range)
            {
                W.CastIfHitchanceEquals(target, HitChance.High);
            }
        }

        #endregion

        #region E

        private void CastE(Obj_AI_Base target)
        {
            if (!E.IsReady() || target == null || !isValidTarget(target) && !ManaManager())
                return;

            var UseOnlyPosion = config.Item("UseEOnlyPosion", true).GetValue<bool>();

            if (UseOnlyPosion)
            {
                if (CheckPoison(target))
                {
                    E.CastOnUnit(target);
                }
            }
            else
            {
                E.CastOnUnit(target);
            }
        }

        private void AutoE()
        {
            if (!config.Item("AutoE", true).GetValue<bool>() || !E.IsReady())
                return;

            foreach (var enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x) && Player.Distance(x.Position) < E.Range))
            {
                if (CheckPoison(enemyhero))
                {
                    E.CastOnUnit(enemyhero);
                }
            }
        }

        #endregion

        #region R

        private void CastR()
        {
            if (!R.IsReady())
                return;

            var info = UltInfo();

            var RRCount = config.Item("RCount", true).GetValue<Slider>().Value;
            var RIsFacingCount = config.Item("RFaceCount", true).GetValue<Slider>().Value;
            var RKillableCount = config.Item("RKillCount", true).GetValue<Slider>().Value;

            if (info.Item1 != null)
            {
                if (info.Item2 >= RRCount)
                {
                    debug<string>("CastR", "Rcount");
                    R.Cast(info.Item1);
                }
                else if (info.Item3 >= RIsFacingCount)
                {
                    debug<string>("CastR", "IsFacing");
                    R.Cast(info.Item1);
                }
                else if (info.Item4 >= RKillableCount)
                {
                    debug<string>("CastR", "Killable");
                    R.Cast(info.Item1);
                }
            }
        }

        private Tuple<Obj_AI_Hero, int, int, int> UltInfo()
        {
            var Rcount = 0;
            var IsFacingCount = 0;
            var KillableCount = 0;
            Obj_AI_Hero target = null;

            foreach(var enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && 
                Player.Distance(x.ServerPosition) < R.Range && 
                isValidTarget(x) &&
                !x.IsDead))
            {
                var predict = R.GetPrediction(enemyhero, true);
                var tmpCount = predict.AoeTargetsHitCount;
                
                if (enemyhero.IsFacing(Player))
                {
                    IsFacingCount += 1;
                }

                if (enemyhero.Health + 30 < R.GetDamage(enemyhero))
                {
                    KillableCount += 1;
                }

                if (target == null && Rcount == 0)
                {
                    target = enemyhero;
                    Rcount = tmpCount;
                }
                else if (tmpCount > Rcount)
                {
                    target = enemyhero;
                    Rcount = tmpCount;
                }
            }

            return new Tuple<Obj_AI_Hero, int, int, int>(target, Rcount, IsFacingCount, KillableCount);
        }

        #endregion

        #region Others

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
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, Emana, Rmana, 2, 1, 3, 0);
                else
                    status = Helpers.Helper.ManaManager(Qmana, Wmana, Emana, Rmana, 2, 1, 3, 1);
            }
            else
            {
                status = true;
            }

            return status;
        }
        
        private bool CheckPoison(Obj_AI_Base target)
        {
            var status = false;

            if (target.Buffs.Any(x => x.Type == BuffType.Poison && Game.Time + E.Delay < x.EndTime))
            {
                status = true;
            }

            return status;
        }

        #endregion

        #region Events

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            Combo();
            Harass();

            if (ManaManager())
            {
                Farm();
            }

            AutoQ();
            AutoE();
        }

        protected override void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var BlockAA = config.Item("BlockAA", true).GetValue<bool>();

            if (!BlockAA)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (args.Target is Obj_AI_Hero)
                {
                    var target = args.Target as Obj_AI_Hero;
                    var delay = config.Item("EDelay", true).GetValue<Slider>().Value;

                    if (!(ECastTime + delay < Utils.TickCount) && target.HasBuffOfType(BuffType.Poison) && isValidTarget(target) && Player.Distance(target.Position) < E.Range)
                    {
                        args.Process = false;
                    }
                }
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (Player.GetSpellSlot(args.SData.Name) == SpellSlot.E)
            {
                ECastTime = Utils.TickCount;
            }
        }

        protected override void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            var delay = config.Item("EDelay", true).GetValue<Slider>().Value;

            if (args.Slot == SpellSlot.E)
            {
                if (!(ECastTime + delay < Utils.TickCount))
                {
                    args.Process = false;
                    //debug<string>("SpellBook", "Work");
                }
            }

            if (config.Item("BlockR", true).GetValue<bool>())
            {
                if (args.Slot == SpellSlot.R)
                {
                    var info = UltInfo();

                    if (info.Item1 == null)
                    {
                        args.Process = false;
                    }
                }
            }
        }

        protected override void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!sender.IsEnemy)
                return;

            if (!(Player.Distance(sender.Position) < R.Range))
                return;

            if (!(args.DangerLevel == Interrupter2.DangerLevel.High))
                return;

            if (R.CastIfHitchanceEquals(sender, HitChance.High))
                debug<string>("OnPossibleToInterrupt", "Work");

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
