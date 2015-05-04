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
    class Karthus : CommonData
    {
        public Karthus()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OpProject : </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 875);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.66f, 160f, 2000, false, SkillshotType.SkillshotCircle); //160
            W.SetSkillshot(0.65f, 100f, 1600f, false, SkillshotType.SkillshotLine);

            QMana = new[] { 20, 20, 26, 32, 38, 44 };

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
                harassmenu.AddItem(new MenuItem("HarassT", "Harass Toggle Key", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
                harassmenu.AddItem(new MenuItem("H-UseQ", "Use Q", true).SetValue(true));
                harassmenu.AddItem(new MenuItem("H-UseW", "Use W", true).SetValue(false));
                harassmenu.AddItem(new MenuItem("H-UseE", "Use E", true).SetValue(false));

                config.AddSubMenu(harassmenu);
            }

            var KSmenu = new Menu("KS", "KS");
            {
                KSmenu.AddItem(new MenuItem("KS-UseQ", "Use Q KS", true).SetValue(true));

                config.AddSubMenu(KSmenu);
            }

            var Farmmenu = new Menu("Farm", "Farm");
            {
                Farmmenu.AddItem(new MenuItem("LH-UseQ", "Use Q LastHit", true).SetValue(true));
                Farmmenu.AddItem(new MenuItem("LC-UseQ", "Use Q Lane Clear", true).SetValue(true));
                Farmmenu.AddItem(new MenuItem("LC-UseE", "Use E Lane Clear", true).SetValue(true));

                config.AddSubMenu(Farmmenu);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                Miscmenu.AddItem(new MenuItem("QImmobile", "Auto Use Q Immobile", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("safemana", "Use E Off (If no enemies or minion in Erange)", true).SetValue(true));

                config.AddSubMenu(Miscmenu);
            }
            //
            var Drawingmenu = new Menu("Drawings", "Drawings");
            {
                Drawingmenu.AddItem(new MenuItem("Qcircle", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                Drawingmenu.AddItem(new MenuItem("Wcircle", "W Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
                Drawingmenu.AddItem(new MenuItem("Ecircle", "E Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
                Drawingmenu.AddItem(new MenuItem("UltDraw", "Draw Ult Killable Count", true).SetValue(true));
                
                config.AddSubMenu(Drawingmenu);
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("C-UseW", true).GetValue<bool>() && W.IsReady())
                {
                    CastW(target);
                }

                if (config.Item("C-UseE", true).GetValue<bool>() && E.IsReady())
                {
                    CastE();
                }

                if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
                }

                KSCheck(target);
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (config.Item("HarassT", true).GetValue<KeyBind>().Active && target != null)
            {
                CastQ(target);
            }

            if (config.Item("HarassActive", true).GetValue<KeyBind>().Active && target != null)
            {
                if (config.Item("H-UseW", true).GetValue<bool>() && W.IsReady())
                {
                    CastW(target);
                }

                if (config.Item("H-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    CastQ(target);
                }
                if (config.Item("H-UseE", true).GetValue<bool>() && E.IsReady())
                {
                    CastE();
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
            }
        }

        private void Farm()
        {
            if (config.Item("LastHitKey", true).GetValue<KeyBind>().Active)
            {
                var minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);

                foreach (var minion in minions)
                {
                    var helath = HealthPrediction.GetHealthPrediction(minion, 700);
                    var predict = Q.GetCircularFarmLocation(minions);
                    var minionPredict = Prediction.GetPrediction(minion, Q.Delay, 190, Q.Speed);

                    if (predict.MinionsHit == 1)
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.Q) - 10 > helath && minionPredict.Hitchance >= HitChance.High)
                        {
                            Q.Cast(minionPredict.CastPosition);
                        }
                    }
                    else
                    {
                        if (Player.GetSpellDamage(minion, SpellSlot.Q, 1) - 10 > helath && minionPredict.Hitchance >= HitChance.High)
                        {
                            Q.Cast(minionPredict.CastPosition);
                        }
                    }
                }
            }
            else if (config.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
            {
                var a = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                var aa = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                List<Vector2> b = new List<Vector2>();
                foreach (var x in a)
                {
                    b.Add(x.Position.To2D());
                }
                var predict = MinionManager.GetBestCircularFarmLocation(b, Q.Width, Q.Range);

                if (predict.Position.IsValid() && config.Item("LC-UseQ", true).GetValue<bool>() && Q.IsReady())
                {
                    Q.Cast(predict.Position);
                }

                if (config.Item("LC-UseE", true).GetValue<bool>() && E.IsReady() && Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1 && aa.Count > 2)
                {
                    E.Cast();
                }
            }
        }

        /// <Q>
        /// <param name="target"></param>

        private void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null)
                return;
            /*
            var predict = Q.GetPrediction(target, true);

            if (predict.Hitchance >= HitChance.High)
            {
                Q.Cast(predict.CastPosition);
            }*/
            var a = Q.GetPrediction(target, true);
            var b = target.GetWaypoints().Count;

            if (a.Hitchance == HitChance.Immobile || a.Hitchance == HitChance.Dashing)
            {
                Q.CastIfHitchanceEquals(target, HitChance.High);
            }
            else if (b <= 2)
            {
                Q.CastIfHitchanceEquals(target, HitChance.High);
            }
            else if (b > 2)
            {
                Q.CastIfHitchanceEquals(target, HitChance.VeryHigh);
            }
        }

        private void AutoQ()
        {
            if (!Q.IsReady() || !config.Item("QImmobile", true).GetValue<bool>() || Player.IsDead)
                return;
            
            foreach(Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => isValidTarget(x) && x.IsEnemy && Player.Distance(x) < Q.Range))
            {
                var predict = Q.GetPrediction(enemyhero, true);

                if (predict.Hitchance == HitChance.Immobile)
                {
                    Q.Cast(enemyhero);
                }
            }

        }

        /// <W>
        /// <param name="target"></param>

        private void CastW(Obj_AI_Hero target)
        {
            if (!W.IsReady() || !isValidTarget(target) || target == null)
                return;

            var predict = W.GetPrediction(target);

            if (predict.Hitchance >= HitChance.High)
            {
                W.Cast(target);
            }
        }

        private bool Wmana()
        {
            return Player.Mana > QMana[Q.Level - 1] * 5;
        }

        /// <E>
        /// <param name="target"></param>

        private void CastE()
        {
            if (!E.IsReady() || Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != 1) // 1 = off , 2 = on
                return;

            var Range = E.Range + 30;
            var Etarget = TargetSelector.GetTarget(Range, TargetSelector.DamageType.Magical);

            if (E.IsReady() && Etarget != null)
            {
                E.Cast();
            }
        }

        private void SafeMana()
        {
            if (!E.IsReady() || Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != 2 || !config.Item("safemana", true).GetValue<bool>()) // 1 = off , 2 = on
                return;

            if (config.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
            {
                var Range = E.Range + 100;
                var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Range, MinionTypes.All, MinionTeam.NotAlly);

                if (E.IsReady() && minions.Count < 2)
                {
                    E.Cast();
                }
            }
            else
            {
                var Range = E.Range + 100;
                var Etarget = TargetSelector.GetTarget(Range, TargetSelector.DamageType.Magical);

                if (E.IsReady() && Etarget == null)
                {
                    E.Cast();
                }
            }
        }

        /// <R>
        /// <returns></returns>

        private float GetUltDmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            dmg += Player.GetSpellDamage(target, SpellSlot.R);

            dmg -= target.HPRegenRate * 4;
            // solrari 3190,fountain 3401, serph 3040

            if (Items.HasItem(3155, target))
            {
                dmg -= 250;
            }

            if (Items.HasItem(3156, target))
            {
                dmg -= 400;
            }

            if (Items.HasItem(3040, target))
            {
                var a = target.Mana * 0.25 + 150;

                dmg -= a;
            }

            foreach(Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x)))
            {
                if (Items.HasItem(3190, enemyhero) && target.Distance(enemyhero) < 900)
                {
                    var shield = 50 + enemyhero.Level * 10;

                    dmg -= shield;
                }

                if (Items.HasItem(3401, enemyhero) && target.Distance(enemyhero) < 900)
                {
                    var shield = enemyhero.MaxHealth / 10;

                    dmg -= shield;
                }
            }

            return (float)dmg;
        }

        private void RKillable()
        {
            if (!R.IsReady() || !config.Item("UltDraw", true).GetValue<bool>())
                return;

            var killable = 0;

            foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x) && !EnemyHasShield(x)))
            {
                if (GetUltDmg(enemyhero) > enemyhero.Health)
                {
                    killable += 1;
                }
            }

            if (killable > 0)
            {
                Vector2 pos = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.Red, "R Killable Count" + killable);
            }
        }

        /// <Events>
        /// <param name="args"></param>

        protected override void OnUpdate(EventArgs args)
        {
            Combo();
            Harass();
            Farm();

            AutoQ();
            SafeMana();
        }

        protected override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            var QCircle = config.Item("Qcircle").GetValue<Circle>();
            var WCircle = config.Item("Wcircle").GetValue<Circle>();
            var ECircle = config.Item("Ecircle").GetValue<Circle>();

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
            
            RKillable();
        }

        protected override void Drawing_OnEndScene(EventArgs args)
        {
            if (Player.IsDead)
                return;

            base.Drawing_OnEndScene(args);
        }

    }
}
