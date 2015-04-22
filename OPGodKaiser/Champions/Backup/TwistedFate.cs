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
    class TwistedFate : CommonData
    {
        private static float Qangle = 28 * (float)Math.PI / 180;
        private static int CastQTick;
        public TwistedFate()
        {
            LoadSpellData();
            LoadMenu();
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1450);
            Q.SetSkillshot(0.25f, 40f, 1000f, false, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W);
            SpellList.Add(Q);
            SpellList.Add(W);
        }

        private void LoadMenu()
        {
            var q = new Menu("Q - Wildcards", "Q");
            {
                q.AddItem(new MenuItem("AutoQI", "Auto-Q immobile").SetValue(true));
                q.AddItem(new MenuItem("AutoQD", "Auto-Q dashing").SetValue(true));
                q.AddItem(new MenuItem("CastQ", "Cast Q (tap)").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
                config.AddSubMenu(q);
            }

            var w = new Menu("W - Pick a card", "W");
            {
                w.AddItem(
                new MenuItem("SelectYellow", "Select Yellow").SetValue(new KeyBind("W".ToCharArray()[0],
                    KeyBindType.Press)));
                w.AddItem(
                    new MenuItem("SelectBlue", "Select Blue").SetValue(new KeyBind("E".ToCharArray()[0], KeyBindType.Press)));
                w.AddItem(
                    new MenuItem("SelectRed", "Select Red").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                config.AddSubMenu(w);
            }

            var r = new Menu("R - Destiny", "R");
            {
                r.AddItem(new MenuItem("AutoY", "Select yellow card after R").SetValue(true));
                config.AddSubMenu(r);
            }

            var misc = new Menu("Misc", "Misc");
            {
                misc.AddItem(new MenuItem("AutoHarass", "Auto W-AA Harass").SetValue(true));
                config.AddSubMenu(misc);
            }

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            var Drawings = new Menu("Drawings", "Drawings");
            {
                Drawings.AddItem(new MenuItem("Qcircle", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                Drawings.AddItem(new MenuItem("Rcircle", "R Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 255, 255))));
                Drawings.AddItem(new MenuItem("Rcircle2", "R Range (minimap)").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
                Drawings.AddItem(dmgAfterComboItem);
                config.AddSubMenu(Drawings);
            }
        }

        protected override void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero)
                args.Process = Champions.Util.CardSelector.Status != Champions.Util.SelectStatus.Selecting && Utils.TickCount - Champions.Util.CardSelector.LastWSent > 300;
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "gate" && config.Item("AutoY").GetValue<bool>())
            {
                Champions.Util.CardSelector.StartSelecting(Util.Cards.Yellow);
            }
        }

        protected override void Drawing_OnEndScene(EventArgs args)
        {
            base.Drawing_OnEndScene(args);
            var rCircle2 = config.Item("Rcircle2").GetValue<Circle>();
            if (rCircle2.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, 5500, rCircle2.Color, 1, 23, true);
            }
        }

        protected override void OnDraw(EventArgs args)
        {
            base.OnDraw(args);
            var qCircle = config.Item("Qcircle").GetValue<Circle>();
            var rCircle = config.Item("Rcircle").GetValue<Circle>();

            if (qCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, qCircle.Color);
            }

            if (rCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 5500, rCircle.Color);
            }
        }

        private int CountHits(Vector2 position, List<Vector2> points, List<int> hitBoxes)
        {
            var result = 0;

            var startPoint = ObjectManager.Player.ServerPosition.To2D();
            var originalDirection = Q.Range * (position - startPoint).Normalized();
            var originalEndPoint = startPoint + originalDirection;

            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];

                for (var k = 0; k < 3; k++)
                {
                    var endPoint = new Vector2();
                    if (k == 0) endPoint = originalEndPoint;
                    if (k == 1) endPoint = startPoint + originalDirection.Rotated(Qangle);
                    if (k == 2) endPoint = startPoint + originalDirection.Rotated(-Qangle);

                    if (point.Distance(startPoint, endPoint, true, true) <
                        (Q.Width + hitBoxes[i]) * (Q.Width + hitBoxes[i]))
                    {
                        result++;
                        break;
                    }
                }
            }

            return result;
        }

        private void CastQ(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0)
        {
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            var startPoint = ObjectManager.Player.ServerPosition.To2D();
            var originalDirection = Q.Range * (unitPosition - startPoint).Normalized();

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    var pos = Q.GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int)enemy.BoundingRadius);
                    }
                }
            }


            var posiblePositions = new List<Vector2>();

            for (var i = 0; i < 3; i++)
            {
                if (i == 0) posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1) posiblePositions.Add(startPoint + originalDirection.Rotated(Qangle));
                if (i == 2) posiblePositions.Add(startPoint + originalDirection.Rotated(-Qangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (var i = 0; i < 3; i++)
                {
                    var pos = posiblePositions[i];
                    var direction = (pos - startPoint).Normalized().Perpendicular();
                    var k = (2 / 3 * (unit.BoundingRadius + Q.Width));
                    posiblePositions.Add(startPoint - k * direction);
                    posiblePositions.Add(startPoint + k * direction);
                }
            }

            var bestPosition = new Vector2();
            var bestHit = -1;

            foreach (var position in posiblePositions)
            {
                var hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit + 1 <= minTargets)
                return;

            Q.Cast(bestPosition.To3D(), true);
        }

        private void AutoH()
        {
            var target = TargetSelector.GetTarget(500, TargetSelector.DamageType.Magical);

            if (W.IsReady() && target != null)
            {
                W.Cast();
                W.Cast();
                if (Orbwalker.InAutoAttackRange(target) && Util.CardSelector.Status == Util.SelectStatus.Selected)
                {
                    debug<string>("AutoH()", "hey");
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            var dmg = 0d;
            dmg += Player.GetSpellDamage(hero, SpellSlot.Q) * 2;
            dmg += Player.GetSpellDamage(hero, SpellSlot.W);
            dmg += Player.GetSpellDamage(hero, SpellSlot.Q);

            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }

            return (float)dmg;
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (config.Item("CastQ").GetValue<KeyBind>().Active)
            {
                CastQTick = Utils.TickCount;
            }

            if (Utils.TickCount - CastQTick < 500)
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }

            if (config.Item("AutoHarass").GetValue<bool>() && W.IsReady())
            {
                AutoH();
            }

            var combo = config.Item("ComboActive", true).GetValue<KeyBind>().Active;

            //Select cards.
            if (config.Item("SelectYellow").GetValue<KeyBind>().Active ||
                combo)
            {
                Util.CardSelector.StartSelecting(Util.Cards.Yellow);
            }

            if (config.Item("SelectBlue").GetValue<KeyBind>().Active)
            {
                Util.CardSelector.StartSelecting(Util.Cards.Blue);
            }

            if (config.Item("SelectRed").GetValue<KeyBind>().Active)
            {
                Util.CardSelector.StartSelecting(Util.Cards.Red);
            }

            //Auto Q
            var autoQI = config.Item("AutoQI").GetValue<bool>();
            var autoQD = config.Item("AutoQD").GetValue<bool>();

            
            if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready && (autoQD || autoQI))
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy.IsValidTarget(Q.Range * 2))
                    {
                        var pred = Q.GetPrediction(enemy);
                        if ((pred.Hitchance == HitChance.Immobile && autoQI) ||
                            (pred.Hitchance == HitChance.Dashing && autoQD))
                        {
                            CastQ(enemy, pred.UnitPosition.To2D());
                        }
                    }
                }
        }
    }
}
