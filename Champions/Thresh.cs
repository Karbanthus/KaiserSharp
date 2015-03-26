using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OPGodKaiser.Champions
{
    class Thresh : CommonData
    {
        Obj_AI_Hero catchedUnit = null;
        int qTimer;
        Vector3 __a;

        public Thresh()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>",Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 380);

            Q.SetSkillshot(0.5f, 70f, 1900, true, SkillshotType.SkillshotLine);


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
                combomenu.AddItem(new MenuItem("C-UseHW", "Use Hooeked W", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseR", "Use R", true).SetValue(true));
                combomenu.AddItem(new MenuItem("minNoEnemies", "Min No. Of Enemies R", true).SetValue(new Slider(2, 1, 5)));
                config.AddSubMenu(combomenu);
            }

            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("H-UseQ", "Use Q", true).SetValue(true));
                harassmenu.AddItem(new MenuItem("H-UseE", "Use E", true).SetValue(false));

                config.AddSubMenu(harassmenu);
            }

            var KSmenu = new Menu("KS", "KS");
            {
                KSmenu.AddItem(new MenuItem("KS-UseQ", "Use Q KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseE", "Use E KS", true).SetValue(true));
                KSmenu.AddItem(new MenuItem("KS-UseR", "Use R KS", true).SetValue(true));

                config.AddSubMenu(KSmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("UseEGapCloser", "Use E On Gap Closer", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseQInterrupt", "Use Q On Interrupt", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseEInterrupt", "Use E On Interrupt", true).SetValue(true));

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
            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active && target.IsValid && target != null)
            {
                if (CanActiveQ2())
                {
                    //CastCatchedLatern();
                }
                //CastCatchedLatern();
                if (config.Item("C-UseE", true).GetValue<bool>())
                {
                    CastE(target);
                }
                if (config.Item("C-UseQ", true).GetValue<bool>())
                {
                    CastQ(target);
                }
            }
        }

        private void Harass()
        {
            
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
                if (config.Item("KS-UseR", true).GetValue<bool>())
                {
                    var myDmg = Player.GetSpellDamage(target, SpellSlot.R);
                    if (myDmg >= target.Health)
                    {
                        if (Player.Distance(target) <= R.Range)
                        {
                            R.Cast();
                        }
                    }
                }
            }
        }
       
        /// <Q>
        /// <param name="target"></param>

        private void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null)
                return;

            bool Catched;
            Obj_AI_Hero CatchedQtarget;
            isPulling(out Catched, out CatchedQtarget);

            if (!Catched && qTimer == 0)
            {
                if (!E.IsReady() || (E.IsReady() && E.Range < Player.Distance(target)))
                {
                    
                    var b = Q.GetPrediction(target);
                    if (b.Hitchance >= HitChance.High)
                    {
                        Q.Cast(target);
                    }
                    /*
                    var PredictedPos = GetPredictedPos(target, Q.Range, Q.Speed, Q.Delay, Q.Width);
                    if (PredictedPos.IsValid())
                    {
                        var minionColl = Q.GetPrediction(target);
                        if (minionColl.Hitchance >= HitChance.High)
                        {
                            Q.Cast(PredictedPos);
                        }
                    }*/
                }
            }
            else if (Catched && Environment.TickCount > qTimer - 200 && CanActiveQ2() && CatchedQtarget.Type == target.Type)
            {
                Q.Cast();
            }
        }

        private bool CanActiveQ2()
        {
            bool status = false;
            bool Catched;
            Obj_AI_Hero CatchedQtarget;
            isPulling(out Catched, out CatchedQtarget);

            if (Catched && CatchedQtarget != null && CatchedQtarget.Type == Player.Type && !CatchedQtarget.UnderTurret())
            {
                var NearEnemiesCount = countEnemiesNearTarget(CatchedQtarget);
                int NearAlliesCount;
                bool CanHeSoonDead;
                countAlliesNearTarget(CatchedQtarget, out NearAlliesCount, out CanHeSoonDead);

                Game.PrintChat("Debug Enemies count : " + NearEnemiesCount);
                Game.PrintChat("Debug Allies count : " + NearAlliesCount);

                if (CanHeSoonDead)
                {
                    NearEnemiesCount = NearEnemiesCount - 1;
                }
                if (NearEnemiesCount == 0)
                {
                    status = true;
                }
                else if (NearAlliesCount >= NearEnemiesCount)
                {
                    status = true;
                }
                // Ai turret return true
                /*else if (E.IsReady() && b.tu)
                {Obj_AI_Turret.
                    return true;
                }*/
            }
            return status;
        }

        private void countAlliesNearTarget(Obj_AI_Hero target, out int NearAlliesCount, out bool CanHeSoonDead)
        {
            NearAlliesCount = 0;
            CanHeSoonDead = false;

            if (!isValidTarget(target))
                return;

            foreach (Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(ally => isValidTarget(ally) && ally.IsAlly && !ally.IsMe && !ally.IsDead))
            {
                if (allyhero.Distance(target) <= 800)
                {
                    NearAlliesCount = NearAlliesCount + 1;

                    var myQDmg = Player.GetDamageSpell(target, SpellSlot.Q).CalculatedDamage;
                    var myEDmg = Player.GetDamageSpell(target, SpellSlot.E).CalculatedDamage;
                    var myRDmg = Player.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;
                    var myAADmg = Player.GetAutoAttackDamage(target);
                    var myDmg = (myQDmg + myEDmg + myRDmg + myAADmg);
                    var allyDmg = GetAlliesComboDmg(target, allyhero) * 3;

                    if (E.IsReady())
                    {
                        myDmg = myDmg * 2;
                        allyDmg = GetAlliesComboDmg(target, allyhero) * 4;
                    }

                    var OurTeamDmg = myDmg + allyDmg;

                    if (OurTeamDmg >= target.Health)
                    {
                        CanHeSoonDead = true;
                    }
                }
            }
        }

        /// <W>
        /// <param name="target"></param>

        private void CastW(Obj_AI_Hero target)
        {

        }

        private void CastCatchedLatern()
        {
            if (!W.IsReady() || !config.Item("C-UseHW").GetValue<bool>())
                return;

            bool Catched;
            Obj_AI_Hero CatchedQtarget;
            isPulling(out Catched, out CatchedQtarget);

            if (Catched && CatchedQtarget != null && CatchedQtarget.Type == Player.Type)
            {
                var range = Player.Distance(CatchedQtarget);
                if (range > 700)
                {
                    var Lanterntarget = GetMostADAlly();
                    if (Lanterntarget != null)
                    {
                        var PredictedPos =W.GetPrediction(Lanterntarget).CastPosition;
                        W.Cast(PredictedPos);
                    }
                }
                else
                {
                    var Lanterntarget = GetFurthestAlly(true, CatchedQtarget);
                    if (Lanterntarget != null)
                    {
                        var PredictedPos = W.GetPrediction(Lanterntarget).CastPosition;
                        if (Player.Distance(Lanterntarget) <= W.Range)
                        {
                            W.Cast(PredictedPos);
                        }
                        else
                        {
                            var myPos = Player.Position;
                            var PredictedPosition = PredictedPos;
                            var castPos = myPos + PredictedPosition - myPos.Normalized() * W.Range;
                            if (castPos.IsValid())
                            {
                                W.Cast(castPos);
                            }
                        }
                    }
                }
            }
        }

        private Obj_AI_Hero GetMostADAlly()
        {
            Obj_AI_Hero Getad = null;
            float dmg = 0;
            foreach(Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsMe && !ally.IsDead && Player.Distance(ally) < 1300))
            {
                if (Getad == null)
                {
                    Getad = allyhero;
                    dmg = allyhero.TotalAttackDamage;
                }
                else
                {
                    if (dmg < allyhero.TotalAttackDamage)
                    {
                        Getad = allyhero;
                        dmg = allyhero.TotalAttackDamage; 
                    }
                }
            }
            return Getad;
        }

        private Obj_AI_Hero GetFurthestAlly(bool status, Obj_AI_Hero CatchedQtarget)
        {
            if (!W.IsReady())
                return null;

            Obj_AI_Hero LanternTarget = null;
            float a_a = 0;
            foreach(Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsMe && !ally.IsDead))
            {
                if (Player.Distance(allyhero) <= 1500 && CatchedQtarget != null && Geometry.Distance(allyhero, CatchedQtarget) > Player.Distance(CatchedQtarget))
                {
                    var d_a = Player.Distance(allyhero);

                    if (a_a ==0)
                    {
                        LanternTarget = allyhero;
                        a_a = Player.Distance(allyhero);
                    }
                    else if (d_a > a_a)
                    {
                        LanternTarget = allyhero;
                        a_a = Player.Distance(allyhero);
                    }
                }
            }
            return LanternTarget;
        }

        /// <E>
        /// <param name="target"></param>

        private void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !isValidTarget(target) || target == null)
                return;

            bool Catched;
            Obj_AI_Hero CatchedQtarget;
            isPulling(out Catched, out CatchedQtarget);
            //Game.PrintChat("bool : {0}", Catched);
            //Game.PrintChat("target : {0}", CatchedQtarget);
            //Game.PrintChat("qtimer : {0}", qTimer);
            if (!Catched && qTimer == 0 && target.IsMoving)
            {
                if (Player.Distance(target) <= E.Range)
                {
                    var pos = target.Position.Extend(Player.Position, Player.Distance(target) + 200);
                    E.Cast(pos);
                }
            }
            else if (Catched) //Player.Distance(target) <= E.Range
            {
                //var pos = target.Position.Extend(Player.Position, Player.Distance(target) + 200);
                Game.PrintChat("catched");
                if (Environment.TickCount > qTimer - 200 && Player.Distance(target) <= E.Range)
                {
                    var pos = target.Position.Extend(Player.Position, Player.Distance(target) + 200);
                    E.Cast(pos);
                }
            }
        }

        private bool isFleeing(Obj_AI_Hero target)
        {
            var status = false;

            if (!isValidTarget(target))
                return status;

            var WayPoints = target.GetWaypoints();
            
            foreach (Vector3 aa in WayPoints)
            {
                __a = aa;
            }
            if (__a.IsValid())
            {
                if (Player.Distance(target) < Player.Distance(__a))
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

        /// <R>
        /// <returns></returns>

        private void AutoR()
        {
            if (!R.IsReady() && config.Item("C-UseR", true).GetValue<bool>())
                return;

            // Menu Count
            int RequireCount = config.Item("minNoEnemies", true).GetValue<Slider>().Value;

            // Enemeis count in R range
            var hit = HeroManager.Enemies.Where(i => i.IsValidTarget(R.Range)).ToList();
            int count = hit.Count;

            if (RequireCount <= count && R.IsReady())
            {
                R.Cast();
            }
        }

        /// <Check Buff>
        /// <param name="CatchedQtarget"></param>

        private void isPulling(out bool Catched, out Obj_AI_Hero CatchedQtarget)
        {
            if (catchedUnit != null)
            {
                Catched = true;
                CatchedQtarget = catchedUnit;
            }
            else
            {
                Catched = false;
                CatchedQtarget = null;
            }
        }

        private void CheckBuff()
        {
            if (Player.IsDead)
                return;

            foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && !enemy.IsDead && enemy.IsValid && enemy.Type == Player.Type))
            {
                if (enemyhero.HasBuff("ThreshQ") || enemyhero.HasBuff("threshqfakeknockup"))
                {
                    catchedUnit = enemyhero;
                    return;
                }
            }

            if (catchedUnit != null)
            {
                if (!catchedUnit.HasBuff("ThreshQ"))
                {
                    catchedUnit = null;
                    //Game.PrintChat("Lost Buff :" + Environment.TickCount);
                }
            }
        }

        /// <Events>
        /// <param name="args"></param>

        protected override void OnUpdate(EventArgs args)
        {
            //Check Thresh Q Buff
            CheckBuff();
            Combo();
            AutoR();
        }

        protected override void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseEInterrupt").GetValue<bool>())
            {
                if (Player.Distance(sender) < E.Range)
                {
                    if (E.IsReady())
                    {
                        Game.PrintChat("Debug : EInterrupt");
                        E.Cast(sender);
                    }
                }
            }

            if (config.Item("UseQInterrupt").GetValue<bool>())
            {
                if (Player.Distance(sender) < Q.Range)
                {
                    var b = Q.GetPrediction(sender, false);
                    if (Q.IsReady() && b.Hitchance >= HitChance.High)
                    {
                        Game.PrintChat("Debug : QInterrupt");
                        Q.Cast(sender);
                    }
                }
            }
        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("UseEGapCloser").GetValue<bool>())
                return;

            if (Player.Distance(gapcloser.Sender) < E.Range)
            {
                if (E.IsReady())
                {
                    Game.PrintChat("Debug : AntiGapCloser");
                    E.Cast(gapcloser.Sender);
                }
            }
        }

        protected override void OnDraw(EventArgs args)
        {
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

        protected override void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Animation.ToLower() == "spell1_in")
                {
                    //Game.PrintChat("hi:{0} , TickCount:{1}", args.Animation, Environment.TickCount);
                    qTimer = Environment.TickCount + 1200;
                }
                else if (args.Animation.ToLower() == "spell1_out")
                {
                    //Game.PrintChat("hi:{0} , TickCount:{1}", args.Animation, Environment.TickCount);
                    qTimer = 0;
                }
                else if (args.Animation.ToLower() == "spell1_pull1")
                {
                    //Game.PrintChat("hi:{0} , TickCount:{1}", args.Animation, Environment.TickCount);
                    qTimer = Environment.TickCount + 900;
                }
                else if (args.Animation.ToLower() == "spell1_pull2")
                {
                    //Game.PrintChat("hi:{0} , TickCount:{1}", args.Animation, Environment.TickCount);
                    qTimer = Environment.TickCount + 900;
                }
                else if (qTimer > 0 && Environment.TickCount > qTimer)
                {
                    qTimer = 0;
                }
            }
        }
    }
}
