using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OPGodKaiser.Champions
{
    class Vladimir : CommonData
    {
        public Vladimir()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OPGodKaiserProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 610);
            R = new Spell(SpellSlot.R, 700);

            R.SetSkillshot(0.25f, 175, 700, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private void LoadMenu()
        {
            var MiscKey = new Menu("MiscKey", "MiscKey");
            {
                MiscKey.AddItem(new MenuItem("_StackE", "Auto Stack E (toggle)", true).SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));

                config.AddSubMenu(MiscKey);
            }

            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("C-UseQ", "Use Q", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseW", "Use W", true).SetValue(true));
                combomenu.AddItem(new MenuItem("minHP", "use AutoW min HP", true).SetValue(new Slider(40, 1, 100)));
                combomenu.AddItem(new MenuItem("C-UseE", "Use E", true).SetValue(true));
                combomenu.AddItem(new MenuItem("C-UseR", "Use R", true).SetValue(false));
                combomenu.AddItem(new MenuItem("AutoR", "Use AutoR", true).SetValue(true));
                combomenu.AddItem(new MenuItem("AutoROnlyComboActive", "Use AutoR (Only Combo Active)", true).SetValue(true));
                combomenu.AddItem(new MenuItem("minNoEnemies", "Min No. Of Enemies", true).SetValue(new Slider(2, 1, 5)));
                //combomenu.AddItem(new MenuItem("minNoKillEnemies", "Min No. Of KS Enemies", true).SetValue(new Slider(2, 1, 5)));
                config.AddSubMenu(combomenu);
            }

            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("H-UseQ", "Use Q", true).SetValue(true));
                harassmenu.AddItem(new MenuItem("H-UseE", "Use E", true).SetValue(false));
                //harassmenu.AddItem(new MenuItem("CheckMinions", "Don't Use E if min Minions have in Erange", true).SetValue(new Slider(2, 0, 7)));

                config.AddSubMenu(harassmenu);
            }

            var farmmenu = new Menu("Farm", "Farm");
            {
                farmmenu.AddItem(new MenuItem("LastHit Setting", "LastHit Setting", true));
                farmmenu.AddItem(new MenuItem("LH-UseQ", "Use Q", true).SetValue(true));

                farmmenu.AddItem(new MenuItem("LaneClear Setting", "LaneClear Setting"));
                farmmenu.AddItem(new MenuItem("LC-UseQ", "Use Q", true).SetValue(true));
                farmmenu.AddItem(new MenuItem("LC-UseE", "Use E", true).SetValue(true));

                config.AddSubMenu(farmmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("UseKS", "Use Smart KillSteal", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("UseWGapCloser", "Use W On Gap Closer", true).SetValue(true));
                //Miscmenu.AddItem(new MenuItem("UseEavdeW", "Use W On Very Dangerous Situations", true).SetValue(true));

                config.AddSubMenu(Miscmenu);
            }

            var Drawingmenu = new Menu("Drawings", "Drawings");
            {
                Drawingmenu.AddItem(new MenuItem("Qcircle", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                Drawingmenu.AddItem(new MenuItem("Ecircle", "E Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
                Drawingmenu.AddItem(new MenuItem("Rcircle", "R Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));


                //Damage after combo:
                var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo", true).SetValue(true);
                Drawingmenu.AddItem(dmgAfterComboItem);
                Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
                dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

                config.AddSubMenu(Drawingmenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (E.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.E);
            if (IgniteSlot.IsReady())
                comboDamage += Player.GetSpellDamage(target, IgniteSlot);
            if (R.IsReady())
            {
                comboDamage += Player.GetSpellDamage(target, SpellSlot.R);
                comboDamage += comboDamage * 1.12;
            }
            else if (target.HasBuff("vladimirhemoplaguedebuff", true))
            {
                comboDamage += comboDamage * 1.12;
            }

            return (float)(comboDamage + Player.GetAutoAttackDamage(target));
        }

        private void Combo()
        {
            var Qtarget = GetTarget(Q, TargetSelector.DamageType.Magical);
            var Etarget = GetTarget(E, TargetSelector.DamageType.Magical);

            var CanCastQ = GetPredicted(Qtarget, Q, false, false);
            var CanCastE = GetPredicted(Etarget, E, true, false);

            var useQ = config.Item("C-UseQ", true).GetValue<bool>();
            var useW = config.Item("C-UseW", true).GetValue<bool>();
            var useE = config.Item("C-UseE", true).GetValue<bool>();
            var useR = config.Item("C-UseR", true).GetValue<bool>();
            UltCheck();
            if (useR && R.IsReady())
            {
                UltCheck();
                if (useE && CanCastE)
                {
                    E.Cast();
                }
                if (useQ && CanCastQ)
                {
                    Q.Cast(Qtarget);
                }
                if (useW)
                {
                    AutoW();
                }
            }
            else
            {
                if (useE && CanCastE)
                {
                    E.Cast();
                }
                if (useQ && CanCastQ)
                {
                    Q.Cast(Qtarget);
                }
                if (useW)
                {
                    AutoW();
                }
            }
        }

        private void Harass()
        {
            var Qtarget = GetTarget(Q, TargetSelector.DamageType.Magical);
            var Etarget = GetTarget(E, TargetSelector.DamageType.Magical);

            var CanCastQ = GetPredicted(Qtarget, Q, false, false);
            var CanCastE = GetPredicted(Etarget, E, true, false);

            var useQ = config.Item("H-UseQ", true).GetValue<bool>();
            var useE = config.Item("H-UseE", true).GetValue<bool>();

            if (useQ && CanCastQ)
            {
                Q.Cast();
            }

            if (useE && CanCastE)
            {
                //var mincount = config.Item("CheckMinions", true).GetValue<Slider>().Value;
                //var minioncount = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy).Count;
                /*
                if (mincount >= minioncount)
                {
                    E.Cast();
                }
                return;*/
                E.Cast();
            }
        }

        private void Farm()
        {
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            if (Q.IsReady() && config.Item("LH-UseQ", true).GetValue<bool>())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1400)) < Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        Q.CastOnUnit(minion);
                        return;
                    }
                }
            }
        }

        private void LaneClear()
        {
            var rangedMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);

            var UseQ = config.Item("LC-UseQ", true).GetValue<bool>();
            var UseE = config.Item("LC-UseE", true).GetValue<bool>();

            if (UseQ)
                Farm();

            if (UseE && E.IsReady())
            {
                if (rangedMinionsE.Count >= 2)
                    E.Cast();
            }
        }

        private void CheckKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.IsValidTarget(1300)).OrderByDescending(GetComboDamage))
            {
                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E) > target.Health && Q.IsReady() && E.IsReady())
                {
                    E.Cast();
                    Q.Cast(target);
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }

                if (Player.Distance(target.ServerPosition) <= E.Range && Player.GetSpellDamage(target, SpellSlot.E) > target.Health && E.IsReady())
                {
                    E.Cast();
                    return;
                }
            }
        }

        private void AutoW()
        {
            if (W.IsReady() && config.Item("minHP", true).GetValue<Slider>().Value >= Player.HealthPercentage())
            {
                W.Cast();
            }
        }

        private void AutoEStack()
        {
            if (E.IsReady() && Environment.TickCount - E.LastCastAttemptT >= 9900)
            {
                E.Cast();
            }
        }

        private void AutoUlt()
        {
            if (config.Item("AutoROnlyComboActive", true).GetValue<bool>())
                return;

            UltCheck();
        }

        private void UltCheck()
        {
            foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && !enemy.IsDead && isValidTarget(enemy) && Player.Distance(enemy) <= R.Range))
            {
                var Rpredict = R.GetPrediction(enemyhero, true);
                var min = config.Item("minNoEnemies", true).GetValue<Slider>().Value;

                if (Rpredict.AoeTargetsHitCount >= min)
                {
                    R.Cast(enemyhero);
                }
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            //check KS
            if (config.Item("UseKS", true).GetValue<bool>())
            {
                CheckKs();
            }

            //combo
            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                //harass
                if (config.Item("HarassActive", true).GetValue<KeyBind>().Active)
                {
                    Harass();
                }

                //farm
                if (config.Item("LastHitKey", true).GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                //lane clear
                if (config.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                {
                    LaneClear();
                }

            }

            //AutoR
            if (config.Item("AutoR", true).GetValue<bool>() && R.IsReady())
            {
                AutoUlt();
            }

            //Auto Stack E
            if (config.Item("_StackE", true).GetValue<KeyBind>().Active && !Player.IsRecalling())
            {
                AutoEStack();
            }

            //comboDmg drawing
            if (config.Item("DamageAfterCombo", true).GetValue<bool>())
            {
                GetComboDamage(TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical));
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "VladimirTidesofBlood")
                {
                    E.LastCastAttemptT = Environment.TickCount + 250;
                }
            }
        }

        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("UseWGapCloser", true).GetValue<bool>())
                return;

            if (W.IsReady() && gapcloser.Sender.Distance(Player) < 300)
            {
                W.Cast();
            }
        }

        protected override void OnDraw(EventArgs args)
        {
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
            base.Drawing_OnEndScene(args);
        }
    }
}

