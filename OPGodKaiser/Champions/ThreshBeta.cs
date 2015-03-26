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
    class ThreshBeta : CommonData
    {
        Obj_AI_Hero catchedUnit = null;
        int qTimer;
        Vector3 checkBackEPos;

        public ThreshBeta()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>",Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 380);

            Q.SetSkillshot(0.5f, 80f, 1900f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 265f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0f, 150f, 900f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 0f, float.MaxValue, false, SkillshotType.SkillshotCircle);

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
                Miscmenu.AddItem(new MenuItem("UseWGapCloser", "Use W On Gap Closer", true).SetValue(true));

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
            var target = GetTarget(Q, TargetSelector.DamageType.Magical);
            var Etarget = GetTarget(E, TargetSelector.DamageType.Magical);
            if (CanActiveQ2())
            {
                //CastCatchedLatern();
            }
            //CastLantern();
            //Game.PrintChat("debug first");
            if (target != null && target.IsValid && config.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                if (config.Item("C-UseE", true).GetValue<bool>())
                {
                    CastE(Etarget);
                }
                if (config.Item("C-UseQ", true).GetValue<bool>())
                {
                    CastQ(target);
                } 
            }

            if (target != null && target.IsValid)
            {
                KSCheck(target);
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
                        var CanUseR = GetPredicted(target, R, true, false);
                        if (CanUseR)
                        {
                            R.Cast();
                        }
                    }
                }
                var UltCount = GetPredicted(target, R, true, true, 2);
                if (UltCount)
                {
                    R.Cast();
                }
            }
        }

        private void CastQ(Obj_AI_Hero target)
        {
            if (target == null || !isValidTarget(target) || !Q.IsReady())
                return;

            if (qTimer == 0)
            {
                if (!E.IsReady() || (E.IsReady() && Player.Distance(target) > E.Range))
                {
                    var PredictedPos = GetPredictedPos(target, Q.Range, Q.Speed, Q.Delay, Q.Width);
                    if (PredictedPos.IsValid())
                    {
                        var minionColl = Q.GetPrediction(target);
                        if (minionColl.Hitchance >= HitChance.High)
                        {
                            Q.Cast(PredictedPos);
                        }
                    }
                }
            }
            else if (Environment.TickCount > qTimer - 100 && CanActiveQ2())
            {
                Q.Cast();
            }
        }

        private void CastE(Obj_AI_Hero target)
        {
            if (target == null || !isValidTarget(target) || !E.IsReady())
                return;

            bool Catched;
            Obj_AI_Hero CatchedQtarget;
            isPulling(out Catched, out CatchedQtarget);

            if (!Catched && qTimer == 0 && target.IsMoving)
            {
                var PredictedPos = GetPredictedPos(target, E.Range, E.Speed, E.Delay, E.Width);
                if (PredictedPos.IsValid())
                {
                    E.Cast(PredictedPos);
                }
            }
            else if (E.Range >= Player.Distance(target) && Catched && CatchedQtarget == target && CatchedQtarget.IsValid && Environment.TickCount > qTimer - 130)
            {
                if (Player.Distance(target) <= E.Range && isValidTarget(target))
                {
                    Vector3 checkBackEPos = BackE(target);
                    Game.PrintChat("Debug : BackE first");
                    if (checkBackEPos.IsValid())
                    {
                        Game.PrintChat("Debug : BackE");
                        E.Cast(checkBackEPos);
                    }
                }
            }
            if (Environment.TickCount > qTimer - 130)
            {
                Game.PrintChat("Debug : hmm");
            }
        }

        private Vector3 BackE(Obj_AI_Hero target)
        {
            //Vector3 checkBackEPos;
            if (Player.Distance(target) <= E.Range)
            {
                var targetPos = target.Position;
                var myPos = Player.Position;
                var backPos = myPos + myPos - targetPos.Normalized() * 250;
                checkBackEPos = backPos;
            }
            return checkBackEPos;
        }

        private int CountUltEnemies()
        {
            int count = 0;

            foreach(Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && !enemy.IsDead && enemy.IsValid && Player.Distance(enemy) < R.Range+30))
            {
                if (isValidTarget(enemyhero))
                {
                    var Rpred = GetPredicted(enemyhero, R, true, false);
                    if (Rpred)
                    {
                        count = count + 1;
                    }
                }
            }
            return count;
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
                countcountAlliesNearTarget(CatchedQtarget, out NearAlliesCount, out CanHeSoonDead);
                Game.PrintChat("Debug Enemies count : " + NearEnemiesCount);
                Game.PrintChat("Debug Allies count : " + NearAlliesCount);
                //Game.PrintChat("Debug Allies count : " + NearAlliesCount);
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

        private void countcountAlliesNearTarget(Obj_AI_Hero target, out int NearAlliesCount, out bool CanHeSoonDead)
        {
            NearAlliesCount = 0;
            CanHeSoonDead = false;

            if (!isValidTarget(target))
                return;

            foreach(Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(ally => isValidTarget(ally) && ally.IsAlly && !ally.IsMe))
            {
                if (allyhero.Distance(target) <= 800)
                {
                    NearAlliesCount += 1;

                    var myQDmg = Player.GetDamageSpell(target, SpellSlot.Q).CalculatedDamage;
                    var myEDmg = Player.GetDamageSpell(target, SpellSlot.E).CalculatedDamage;
                    var myRDmg = Player.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;
                    var myAADmg = Player.GetAutoAttackDamage(target);
                    var myDmg = (myQDmg + myEDmg + myRDmg + myAADmg);
                    var allyDmg = GetAlliesComboDmg(target, allyhero) * 3 ;

                    if (E.IsReady())
                    {
                        myDmg = myDmg * 2 ;
                        allyDmg = GetAlliesComboDmg(target, allyhero)  * 4 ;
                    }

                    var OurTeamDmg = myDmg + allyDmg;

                    if (OurTeamDmg >= target.Health)
                    {
                        CanHeSoonDead = true;
                    }
                }
            }
        }
        /*
        private void CastCatchedLatern()
        {
            if (!W.IsReady() || !config.Item("useWHook").GetValue<bool>())
                return;

            bool Catched;
            Obj_AI_Hero CatchedQtarget;
            isPulling(out Catched, out CatchedQtarget);

            if (Catched && CatchedQtarget != null && CatchedQtarget.Type == Player.Type)
            {
                var range = Player.Distance(CatchedQtarget);

                if (range > 700)
                {
                    var Lanterntarget = ;
                    if 
                }
            }
        }
        */
        private void GetMostADAlly()
        {
            
        }

        private void CheckBuff()
        {
            if (Player.IsDead)
                return;

            foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && !enemy.IsDead && enemy.IsValid))
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
                }
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            //Check Thresh Q Buff
            CheckBuff();
            Combo();
            //Game.PrintChat("qtiem : {0}", qTimer);
            //Game.PrintChat("countUltEnemies : {0}", CountUltEnemies());
            //Game.PrintChat("configcount : {0}", config.Item("minNoEnemies", true).GetValue<Slider>().Value);

        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            
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
                    //Game.PrintChat("hi:{0}", args.Animation);
                    qTimer = Environment.TickCount + 1200;
                }
                else if (args.Animation.ToLower() == "spell1_out")
                {
                    //Game.PrintChat("hi:{0}", args.Animation);
                    qTimer = 0;
                }
                else if (args.Animation.ToLower() == "spell1_pull1")
                {
                    //Game.PrintChat("hi:{0}", args.Animation);
                    qTimer = Environment.TickCount + 900;
                }
                else if (args.Animation.ToLower() == "spell1_pull2")
                {
                    //Game.PrintChat("hi:{0}", args.Animation);
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
