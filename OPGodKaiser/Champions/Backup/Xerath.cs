using System;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OPGodKaiser.Champions
{
    class Xerath : CommonData
    {
        public Xerath()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1550);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1150);
            R = new Spell(SpellSlot.R, 675);

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);
        }

        private bool IsCastingR
        {
            get 
            { 
                return Player.HasBuff("XerathLocusOfPower2", true) || (Player.LastCastedSpellName() == "XerathLocusOfPower2" && Utils.TickCount - Player.LastCastedSpellT() < 500);
            }
        }

        private static class Ult
        {
            public static int CastTime;
            public static int shoot;
            public static bool TapkeyPressed;
            public static Vector3 CastPosition;
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
                harassmenu.AddItem(new MenuItem("H-UseW", "Use W", true).SetValue(false));

                config.AddSubMenu(harassmenu);
            }

            var Rmenu = new Menu("R", "R");
            {
                Rmenu.AddItem(new MenuItem("UseR", "Use R", true).SetValue(true));
                Rmenu.AddItem(new MenuItem("RMode", "RMode").SetValue(new StringList(new[] { "Normal", "Custom delays", "OnTap" })));
                Rmenu.AddItem(new MenuItem("RModeKey", "OnTap key").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));
                Rmenu.AddItem(new MenuItem("Custom delays", "Custom delays"));
                for (int i = 1; i <= 3; i++)
                    Rmenu.AddItem(new MenuItem("Delay" + i, "Delay" + i).SetValue(new Slider(0, 1500, 0)));
                Rmenu.AddItem(new MenuItem("MovementBlock", "Block Movement While Casting R", true).SetValue(true));

                config.AddSubMenu(Rmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("UseEGapCloser", "Use E On Gap Closer", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseEInterrupt", "Use E On Interrupt", true).SetValue(true));

                config.AddSubMenu(Miscmenu);
            }

            var Drawingmenu = new Menu("Drawings", "Drawings");
            {
                Drawingmenu.AddItem(new MenuItem("QRange", "Q range", true).SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
                Drawingmenu.AddItem(new MenuItem("WRange", "W range", true).SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
                Drawingmenu.AddItem(new MenuItem("ERange", "E range", true).SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
                Drawingmenu.AddItem(new MenuItem("RRange", "R range", true).SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
                Drawingmenu.AddItem(new MenuItem("RRangeM", "R range On minimap", true).SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));

                config.AddSubMenu(Drawingmenu);
            }
        }

        private void Combo()
        {
            var Qtarget = GetTarget(Q, TargetSelector.DamageType.Magical, true);
            var Wtarget = GetTarget(W, TargetSelector.DamageType.Magical);
            var Etarget = GetTarget(E, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("C-UseQ", true).GetValue<bool>();
            var UseW = config.Item("C-UseW", true).GetValue<bool>();
            var UseE = config.Item("C-UseE", true).GetValue<bool>();

            if (UseW && W.IsReady() && Wtarget != null)
            {
                var a = GetPredicted(Wtarget, W, true, false);

                if (a)
                {
                    W.Cast(Wtarget);
                }
            }

            if (UseE && E.IsReady() && Etarget != null)
            {
                var a = GetPredicted(Etarget, E, false, true);
                
                if (a)
                {
                    E.Cast(Etarget);
                }
            }

            if (UseQ && Q.IsReady() && Qtarget != null)
            {
                if (Q.IsCharging)
                {
                    var a = GetPredicted(Qtarget, Q, true, false);

                    if (a)
                    {
                        Q.Cast(Qtarget);
                    }
                }
                else if (!UseW || !W.IsReady() || Player.Distance(Qtarget) > W.Range)
                {
                    var a = GetPredicted(Qtarget, Q, true, false);

                    if (a)
                    {
                        Q.StartCharging();
                    }
                }
            }
        }

        private void Harass()
        {
            var Qtarget = GetTarget(Q, TargetSelector.DamageType.Magical, true);
            var Wtarget = GetTarget(W, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("H-UseQ", true).GetValue<bool>();
            var UseW = config.Item("H-UseW", true).GetValue<bool>();

            if (UseW && W.IsReady() && Wtarget != null)
            {
                var a = GetPredicted(Wtarget, W, true, false);

                if (a)
                {
                    W.Cast(Wtarget);
                }
            }

            if (UseQ && Q.IsReady() && Qtarget != null)
            {
                if (Q.IsCharging)
                {
                    var a = GetPredicted(Qtarget, Q, true, false);
                    
                    if (a)
                    {
                        //Delay Qcast Casuse = I don't like in game noise
                        Utility.DelayAction.Add(500, () => Q.Cast(Qtarget));
                        //Q.Cast(Qtarget);
                    }
                }
                else if (!UseW || !W.IsReady() || Player.Distance(Qtarget) > W.Range)
                {
                    var a = GetPredicted(Qtarget, Q, true, false);

                    if (a)
                    {
                        Q.StartCharging();
                    }
                }
            }
        }

        private void CastR()
        {
            if (!config.Item("UseR", true).GetValue<bool>()) 
                return;

            var rMode = config.Item("RMode").GetValue<StringList>().SelectedIndex;
            var target = GetTarget(R, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                //Wait at least 0.6f if the target is going to die or if the target is to far away
                if (target.Health - R.GetDamage(target) < 0)
                    if (Utils.TickCount - Ult.CastTime <= 700) return;

                if ((Ult.shoot != 0 && target.Distance(Ult.CastPosition) > 1000))
                    if (Utils.TickCount - Ult.CastTime <= Math.Min(2500, target.Distance(Ult.CastPosition) - 1000)) return;
                
                switch (rMode)
                {
                    case 0://Normal
                        R.Cast(target, true);
                        break;

                    case 1://Selected delays.
                        var delay = config.Item("Delay" + (Ult.shoot + 1)).GetValue<Slider>().Value;
                        if (Utils.TickCount - Ult.CastTime > delay)
                            R.Cast(target, true);
                        break;

                    case 2://On tap
                        if (Ult.TapkeyPressed)
                            R.Cast(target, true);
                        break;
                }
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else if (config.Item("HarassActive", true).GetValue<KeyBind>().Active || config.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
            {
                Harass();
            }

            R.Range = 2000 + R.Level * 1200;

            if (IsCastingR)
            {
                Orbwalker.SetMovement(false);
                CastR();
                return;
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (args.SData.Name == "XerathLocusOfPower2")
            {
                Ult.CastTime = 0;
                Ult.shoot = 0;
                Ult.TapkeyPressed = false;
                Ult.CastPosition = new Vector3();
            }
            else if (args.SData.Name == "xerathlocuspulse")
            {
                Ult.CastTime = Utils.TickCount;
                Ult.shoot += Ult.shoot;
                Ult.TapkeyPressed = false;
                Ult.CastPosition = args.End;
            }
        }

        protected override void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (config.Item("MovementBlock", true).GetValue<bool>())
            {
                if (sender.IsMe && IsCastingR)
                {
                    args.Process = false;
                }
            }
        }

        protected override void Game_OnWndProc(WndEventArgs args)
        {
            if (IsCastingR && args.Msg == (uint)WindowsMessages.WM_KEYUP)
            {
                Ult.TapkeyPressed = true;
            }
        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("UseEGapCloser", true).GetValue<bool>())
                return;

            if (Player.Distance(gapcloser.Sender) < E.Range)
            {
                E.Cast(gapcloser.Sender);
            }
        }

        protected override void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!config.Item("UseEInterrupt", true).GetValue<bool>())
                return;

            if (Player.Distance(sender) < E.Range)
            {
                E.Cast(sender);
            }
        }

        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            foreach (Spell spell in SpellList)
            {
                if (config.Item(spell.Slot + "Range", true).GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, config.Item(spell.Slot + "Range", true).GetValue<Circle>().Color);
                }
            }

            if (config.Item("RRangeM", true).GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, config.Item("RRangeM", true).GetValue<Circle>().Color, 1, true);
            }
        }

    }
}
