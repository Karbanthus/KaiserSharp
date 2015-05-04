using System;
using System.Collections.Generic;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace OpProject.Champions
{
    class Udyr : CommonData
    {
        private UdyrStance StanceStatus;
        //private int UdyrPassiveCount;
        private int PhoenixAACount;
        private bool PowerPhoenix;

        public struct Etarget
        {
            public string ChampionName;
            public float StartTime;
            public Obj_AI_Hero Champion;
        }

        private List<Etarget> StunAble = new List<Etarget>();

        private enum UdyrStance
        {
            None,
            Tiger,
            Turtle,
            Bear,
            Phoenix
        }

        public Udyr()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >OpProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            QMana = new[] { 47, 44, 41, 38, 35 };
            WMana = new[] { 47, 44, 41, 38, 35 };
            EMana = new[] { 47, 44, 41, 38, 35 };
            RMana = new[] { 47, 44, 41, 38, 35 };

        }

        private void LoadMenu()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("C-UseQ", "Use Q (I don't like use Q)", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseW", "Use W", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseR", "Use R", true).SetValue(true));
                //combomenu.AddItem(new MenuItem("StunCount", "Min Stun target", true).SetValue(new Slider(1, 1, 5)));
                config.AddSubMenu(combomenu);
            }

            /*
            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("UseWGapCloser", "Use W On Gap Closer", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseEGapCloser", "Use E On Gap Closer", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseEInterrupt", "Use E On Interrupt", true).SetValue(true));

                config.AddSubMenu(Miscmenu);
            }
            */

            var Drawingmenu = new Menu("Drawings", "Drawings");
            {
                Drawingmenu.AddItem(new MenuItem("EStunCircle", "E Stun Circle", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                Drawingmenu.AddItem(new MenuItem("TargetCircle", "TargetCircle", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                
                config.AddSubMenu(Drawingmenu);
            }
        }

        private void Combo()
        {
            var useQ = config.Item("C-UseQ", true).GetValue<bool>();
            var useW = config.Item("C-UseW", true).GetValue<bool>();
            var useE = config.Item("C-UseE", true).GetValue<bool>();
            var useR = config.Item("C-UseR", true).GetValue<bool>();
            var target = TargetSelector.GetTarget(700, TargetSelector.DamageType.Magical);
            var stuncount = 1; //config.Item("StunCount", true).GetValue<Slider>().Value;

            if (target != null)
            {
                if (Player.Distance(target) > 350)
                {
                    E.Cast();
                }

                if (StanceStatus != UdyrStance.None)
                {
                    switch (StanceStatus)
                    {
                        case UdyrStance.Bear:
                            if (StunAble.Count > stuncount - 1)
                            {
                                if (useR && R.IsReady())
                                {
                                    R.Cast();
                                }
                            }
                            break;
                        case UdyrStance.Phoenix:
                            if (StunAble.Count < stuncount)
                            {
                                if (useE && E.IsReady())
                                {
                                    E.Cast();
                                }
                            }
                            else if (PhoenixAACount == 1 && R.IsReady())
                            {
                                R.Cast();
                            }
                            else
                            {
                                if (useW && W.IsReady() && !PowerPhoenix && PhoenixAACount < 2)
                                {
                                    W.Cast();
                                }
                            }
                            break;
                        case UdyrStance.Turtle:
                            if (StunAble.Count < stuncount)
                            {
                                if (useE && E.IsReady())
                                {
                                    E.Cast();
                                }
                            }
                            else
                            {
                                if (useR && R.IsReady())
                                {
                                    R.Cast();
                                }
                            }
                            break;
                        case UdyrStance.Tiger:
                            if (useR && R.IsReady())
                            {
                                R.Cast();
                            }
                            break;
                    }
                }
            }
        }

        private void StunCheck()
        {
            foreach(Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemyhero.IsEnemy && !enemyhero.IsDead && enemyhero.HasBuff("udyrbearstuncheck", true) && enemyhero.IsVisible)
                {
                    bool b = StunAble.Exists(x => x.ChampionName.Contains(enemyhero.ChampionName));
                    if (!b)
                    {
                        StunAble.Add(
                            new Etarget
                            {
                                ChampionName = enemyhero.ChampionName,
                                StartTime = Environment.TickCount,
                                Champion = enemyhero
                            });
                    }
                }
            }
        }

        private void StunCheck2()
        {
            if (StunAble.Count > 0)
            {
                for (int i = 0; i < StunAble.Count; i++)
                {
                    var a = StunAble[i].StartTime;
                    if (Environment.TickCount > a + 5500 || StunAble[i].Champion.IsDead)
                    {
                        StunAble.RemoveAt(i);
                    }
                }
            }
        }

        private void CheckBuff()
        {
            if (Player.HasBuff("UdyrPhoenixStance"))
            {
                switch (Player.Buffs.Find(buff => buff.DisplayName == "UdyrPhoenixStance").Count)
                {
                    case 1:
                        PhoenixAACount = 1;
                        PowerPhoenix = false;
                        break;
                    case 2:
                        PhoenixAACount = 2;
                        PowerPhoenix = false;
                        break;
                    case 3:
                        PhoenixAACount = 3;
                        PowerPhoenix = true;
                        break;
                }
            }
            else
            {
                PhoenixAACount = 0;
                PowerPhoenix = false;
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            CheckBuff();
            StunCheck();
            StunCheck2();
            
            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;

            switch(args.SData.Name)
            {
                case "UdyrTigerStance":
                    StanceStatus = UdyrStance.Tiger;
                    break;
                case "UdyrTurtleStance":
                    StanceStatus = UdyrStance.Turtle;
                    break;
                case "UdyrBearStance":
                    StanceStatus = UdyrStance.Bear;
                    break;
                case "UdyrPhoenixStance":
                    StanceStatus = UdyrStance.Phoenix;
                    PhoenixAACount = 3;
                    break;
                /*default:
                    StanceStatus = UdyrStance.None;
                    break;*/
            }
        }

        protected override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            var EStunCircle = config.Item("EStunCircle", true).GetValue<Circle>();
            var TargetCircle = config.Item("TargetCircle", true).GetValue<Circle>();
            
            if (EStunCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 700 , EStunCircle.Color);
            }
            
            if (TargetCircle.Active)
            {
                if (Orbwalker.GetTarget() != null)
                {
                    var orbtarget = Orbwalker.GetTarget();
                    if (orbtarget.IsValid)
                    {
                        Render.Circle.DrawCircle(orbtarget.Position, 150, System.Drawing.Color.Red, 7);
                    }
                }
            }
            
        }

        protected override void Drawing_OnEndScene(EventArgs args)
        {
            base.Drawing_OnEndScene(args);
        }
    }
}
