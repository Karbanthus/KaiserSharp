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
    class RekSai : CommonData
    {
        private Obj_AI_Hero AfterAttackHero;

        public RekSai()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's OPGodKaiserProject Champ:</font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>", Player.ChampionName);
        }

        private Spell QNormal, WNormal, ENormal;
        private Spell QBurrow, WBurrow, EBurrow;
        
        public Spell Q
        {
            get { return (Burrowed()) ? QBurrow : QNormal; }
        }

        private void LoadSpellData()
        {
            // normal
            QNormal = new Spell(SpellSlot.Q, 300);
            WNormal = new Spell(SpellSlot.W);
            ENormal = new Spell(SpellSlot.E, 250);

            // Burrow
            QBurrow = new Spell(SpellSlot.Q, 1500);
            QBurrow.SetSkillshot(0.125f, 60f, 1950f, true, SkillshotType.SkillshotLine);
            
            WBurrow = new Spell(SpellSlot.W);
            
            EBurrow = new Spell(SpellSlot.E, 750);
            EBurrow.SetSkillshot(0, 60f, 1600f, false, SkillshotType.SkillshotLine);

            //Init
            W = (Burrowed()) ? WBurrow : WNormal;
            E = (Burrowed()) ? EBurrow : ENormal;
            R = new Spell(SpellSlot.R);
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
            var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (config.Item("ComboActive", true).GetValue<KeyBind>().Active && Qtarget != null)
            {
                if (Burrowed())
                {
                    Orbwalker.SetAttack(false);

                    if (config.Item("C-UseQ", true).GetValue<bool>() && Q.IsReady() && Qtarget != null)
                    {
                        CastQ(Qtarget);
                    }
                }
                else
                {

                }

                KSCheck(Qtarget);
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
                if (!enemyhero.HasBuff("RekSaiKnockupImmune"))
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
            if (!Q.IsReady() || !isValidTarget(target) || target == null)
                return;

            if (Burrowed())
            {
                var Predict = Q.GetPrediction(target);

                if (Predict.Hitchance >= HitChance.High)
                {
                    Q.Cast(target);
                }
            }
            else
            {
                if (Player.Distance(AfterAttackHero) < Q.Range && AfterAttackHero != null && isValidTarget(AfterAttackHero))
                {
                    Q.Cast();
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

        }

        /// <Buff>
        ///     Buff
        /// </summary>
        /// <returns></returns>

        private bool Burrowed()
        {
            return Player.Buffs.Any<BuffInstance>(x => x.Caster.IsMe && x.DisplayName == "RekSaiW");
        }

        private bool QNormalBuff()
        {
            return Player.Buffs.Any(x => x.Caster.IsMe && x.DisplayName == "RekSaiQ");
        }

        private bool TunnelReady()
        {
            return !Player.Buffs.Any(x => x.Caster.IsMe && x.DisplayName == "RekSaiECooldown");
        }

        private bool HasFullFury()
        {
            return Player.MaxMana == Player.Mana;
        }

        /// <Events>
        /// <param name="args"></param>

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            Combo();
            Harass();

        }

        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && target.IsEnemy && target.Type == Player.Type)
            {
                var a = (Obj_AI_Hero)target;

                if (a != null)
                {
                    AfterAttackHero = a;
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
