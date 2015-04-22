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
    class Orianna : CommonData
    {
        public enum BallStatus
        {
            Me,
            Ally,
            Land
        }

        public static class BallManager
        {
            public static Vector3 BallPos;
            public static BallStatus HasBall;
            public static Obj_AI_Hero hero;
            public static bool Moving;
        }

        public Orianna()
        {
            LoadSpellData();
            LoadMenu();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>",Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 820);
            W = new Spell(SpellSlot.W, 225);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 375);

            Q.SetSkillshot(0.1f, 125f, 1300f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.15f, 225f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        
        private void LoadMenu()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("Predict", "Set Predict", true).SetValue(new StringList(new[] { "LSharpPredict", "KaiserPredict"})));
                //Q
                combomenu.AddItem(new MenuItem("Q", "Q"));
                combomenu.AddItem(new MenuItem("useQT", "Use Spacebar", true).SetValue(true));
                //W
                combomenu.AddItem(new MenuItem("W", "W"));
                combomenu.AddItem(new MenuItem("useWH", "Use Harass", true).SetValue(true));
                combomenu.AddItem(new MenuItem("useWT", "Use Spacebar", true).SetValue(true));
                combomenu.AddItem(new MenuItem("minNoWEnemies", "Min No. Of Enemies In W Range", true).SetValue(new Slider(1, 1, 5)));
                //E
                combomenu.AddItem(new MenuItem("E", "E"));
                combomenu.AddItem(new MenuItem("useEQCombo", "Use E>Q Combo", true).SetValue(true));
                combomenu.AddItem(new MenuItem("useEHitLine", "Use E If Can Hit", true).SetValue(true));
                combomenu.AddItem(new MenuItem("useEAOE", "Use E>W or E>R", true).SetValue(true));
                //R
                combomenu.AddItem(new MenuItem("R", "R"));
                combomenu.AddItem(new MenuItem("ultRange", "Set R Range", true).SetValue(new Slider(370, 1, 400)));
                combomenu.AddItem(new MenuItem("minNoEnemies", "Min No. Of Enemies", true).SetValue(new Slider(2, 1, 5)));
                combomenu.AddItem(new MenuItem("minNoKillEnemies", "Min No. Of KS Enemies", true).SetValue(new Slider(1, 1, 5)));

                config.AddSubMenu(combomenu);
            }

            var initator = new Menu("Initiator", "Initiator");
            {
                initator.AddItem(new MenuItem("useInitiator", "Use Initiator", true)).SetValue(true);
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
                {
                    foreach (Initiator.Initiatorinfo x in Initiator.InitiatorList)
                    {
                        if (x.Hero == hero.BaseSkinName)
                        {
                            initator.AddItem(new MenuItem(x.Spell, x.Spell, true)).SetValue(true);
                        }
                    }
                }
                config.AddSubMenu(initator);
            }

            var Miscmenu = new Menu("Misc", "Misc");
            {
                //Miscmenu.AddItem(new MenuItem("UseEGapCloser", "Use E On Gap Closer", true).SetValue(true));
                Miscmenu.AddItem(new MenuItem("BlockR", "Block R IF no Enemy", true).SetValue(true));

                config.AddSubMenu(Miscmenu);
            }
            
            var Drawingmenu = new Menu("Drawings", "Drawings");
            {
                Drawingmenu.AddItem(new MenuItem("Qcircle", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
                
                config.AddSubMenu(Drawingmenu);
            }
        }

        /// <CastSkill>
        ///     CastSkill
        /// </summary>
        /// <param name="target"></param>

        private void Combo()
        {
            var a = EAllyAOE();
            var WT = a.Item1;
            var RT = a.Item2;
            if (WT != null)
            {
                debug<string>("Combo-WT",WT.ChampionName);
                E.CastOnUnit(WT);    
            }
            if (RT != null)
            {
                debug<string>("Combo-RT", RT.ChampionName);
                E.CastOnUnit(RT);
            }

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null && config.Item("ComboActive",true).GetValue<KeyBind>().Active)
            {
                if (config.Item("useEQCombo", true).GetValue<bool>())
                {
                    CastE(target);
                }
                if (config.Item("useQT", true).GetValue<bool>())
                {
                    CastQ(target);
                }
            }
            else if (config.Item("useWT", true).GetValue<bool>() && config.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                GetDoomBallTarget("W", true);
            }
            else if (config.Item("useWH", true).GetValue<bool>() && (config.Item("HarassActive", true).GetValue<KeyBind>().Active || config.Item("HarassActiveT", true).GetValue<KeyBind>().Active))
            {
                GetDoomBallTarget("W", true);
            }
            GetDoomBallTarget("W", false);
            GetDoomBallTarget("R", false);
            ELineCheckHit();
        }

        private void GetDoomBallTarget(string a, bool b)
        {
            if (a == "W" && !W.IsReady())
                return;
            if (a == "R" && !R.IsReady())
                return;

            var dd = 0;
            var __a = 0;
            var a_a = 0;

            foreach(Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                if (isValidTarget(enemyhero))
                {
                    if (a == "W" && W.IsReady())
                    {
                        if (b)
                        {
                            CastW(enemyhero);
                        }
                        else if (a == "W" && isTargetInWRange(enemyhero))
                        {
                            dd = dd + 1;
                        }
                    }
                    if (a == "R" && R.IsReady())
                    {
                        if (isTargetInRRange(enemyhero))
                        {
                            __a = __a + 1;
                        }
                        if (isTargetInRRange(enemyhero, true))
                        {
                            a_a = a_a + 1;
                        }
                    }
                }
            }

            if (dd >= config.Item("minNoWEnemies", true).GetValue<Slider>().Value && W.IsReady())
            {
                W.Cast();
            }
            if (__a >= config.Item("minNoEnemies", true).GetValue<Slider>().Value && R.IsReady())
            {
                R.Cast();
            }
            else if (a_a >= config.Item("minNoKillEnemies", true).GetValue<Slider>().Value && R.IsReady())
            {
                R.Cast();
            }

        }

        /// <Q>
        ///     Q
        /// </summary>
        /// <param name="target"></param>

        private void CastQ(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !isValidTarget(target) || target == null)
                return;

            var predict = GetP(BallManager.BallPos, Q, target, 0.1f, true);
            if (predict.Hitchance >= HitChance.High)
            {
                Q.Cast(predict.CastPosition);
            }
        }

        /// <W>
        ///     W
        /// </summary>
        /// <param name="target"></param>

        private void CastW(Obj_AI_Hero target)
        {
            if (!W.IsReady() || !isValidTarget(target) || target == null || BallManager.Moving)
                return;
            
            PredictionOutput predict = GetPCircle(BallManager.BallPos, W, target, true);
            if (predict.UnitPosition.Distance(BallManager.BallPos) < W.Width)
            {
                W.Cast();
            }

        }

        private bool isTargetInWRange(Obj_AI_Hero target)
        {
            if (!isValidTarget(target))
                return false;

            var predict = GetPCircle(BallManager.BallPos, W, target, true);
            if (predict.UnitPosition.Distance(BallManager.BallPos) < W.Width)
            {
                return true;
            }
            else
                return false;
        }

        /// <E>
        ///     E
        /// </summary>
        /// <param name="target"></param>

        private void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !isValidTarget(target) || target == null)
                return;

            if (Q.IsReady())
            {
                if (BallManager.HasBall == BallStatus.Ally)
                {
                    var closestAlly = closestUnitToTarget(target, BallManager.hero);
                    
                    if (closestAlly != null && closestAlly != BallManager.hero)
                    {
                        debug<string>("CastE-Ally", closestAlly.ChampionName);
                        E.CastOnUnit(closestAlly);
                    }
                }
                else if (BallManager.HasBall == BallStatus.Land)
                {
                    var closestAlly = closestUnitToTarget(target, BallManager.BallPos);
                    
                    if (closestAlly != null)
                    {
                        debug<string>("CastE-Land", closestAlly.ChampionName);
                        E.CastOnUnit(closestAlly);
                    }
                }
            }
        }

        private void ELineCheckHit()
        {
            if (!E.IsReady() || !config.Item("useEHitLine", true).GetValue<bool>())
                return;
            
            foreach(Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x)))
            {
                if (BallManager.HasBall == BallStatus.Land)
                {
                    PredictionOutput prediction = GetP(BallManager.BallPos, E, enemyhero, true);
                    Object[] obj = VectorPointProjectionOnLineSegment(BallManager.BallPos.To2D(), Player.ServerPosition.To2D(), prediction.UnitPosition.To2D());
                    var isOnseg = (bool)obj[2];
                    var pointLine = (Vector2)obj[1];

                    if (E.IsReady() && isOnseg && prediction.UnitPosition.Distance(pointLine.To3D()) < E.Width)
                    {
                        debug<string>("ELineCheckHit-Land", "Work");
                        E.CastOnUnit(Player);
                        return;
                    }
                }
                else if (BallManager.HasBall == BallStatus.Ally)
                {
                    PredictionOutput prediction = GetP(BallManager.BallPos, E, enemyhero, true);
                    Object[] obj = VectorPointProjectionOnLineSegment(BallManager.BallPos.To2D(), Player.ServerPosition.To2D(), prediction.UnitPosition.To2D());
                    var isOnseg = (bool)obj[2];
                    var pointLine = (Vector2)obj[1];

                    if (E.IsReady() && isOnseg && prediction.UnitPosition.Distance(pointLine.To3D()) < E.Width)
                    {
                        debug<string>("ELineCheckHit-Ally", "Work");
                        E.CastOnUnit(Player);
                        return;
                    }
                }
            }
        }

        private Obj_AI_Hero closestUnitToTarget(Obj_AI_Hero target, Obj_AI_Hero hasDoomballUnit)
        {
            Obj_AI_Hero ClosetAlly = null;
            foreach(Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && x.IsMe && isValidTarget(x) && Player.Distance(x) <= E.Range))
            {
                var targetTOball = target.Distance(hasDoomballUnit);
                var targetTOally = target.Distance(allyhero);
                var Gap = targetTOball - targetTOally;

                if (Gap >= 150)
                {
                    if (ClosetAlly == null)
                    {
                        ClosetAlly = allyhero;
                    }
                    else if (Player.Distance(allyhero) < Player.Distance(ClosetAlly))
                    {
                        ClosetAlly = allyhero;
                    }
                }
            }
            return ClosetAlly;
        }

        private Obj_AI_Hero closestUnitToTarget(Obj_AI_Hero target, Vector3 ballpos)
        {
            Obj_AI_Hero ClosetAlly = null;
            foreach (Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && x.IsMe && isValidTarget(x) && Player.Distance(x) <= E.Range))
            {
                var targetTOball = target.Distance(ballpos);
                var targetTOally = target.Distance(allyhero);
                var gap = targetTOball - targetTOally;

                if (gap >= 150)
                {
                    if (ClosetAlly == null)
                    {
                        ClosetAlly = allyhero;
                    }
                    else if (Player.Distance(allyhero) < Player.Distance(ClosetAlly))
                    {
                        ClosetAlly = allyhero;
                    }
                }
            }
            return ClosetAlly;
        }

        private Tuple<Obj_AI_Hero,Obj_AI_Hero> EAllyAOE()
        {
            if (E.IsReady() && config.Item("useEAOE", true).GetValue<bool>())
            {
                Obj_AI_Hero ForWTarget = null;
                Obj_AI_Hero ForRTarget = null;
                var tmpW = 0;
                var tmpR = 0;
                var DoomBallCountTmpW = 0;
                var DoomBallCountTmpR = 0;

                foreach(Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && x.IsMe && isValidTarget(x) && Player.Distance(x) <= E.Range))
                {
                    var a = CountAllyAOETarget(allyhero);
                    var CanHitW = a.Item1;
                    var CanHitR = a.Item2;

                    //W
                    if (tmpW == 0 && CanHitW > 0)
                    {
                        ForWTarget = allyhero;
                        tmpW = CanHitW;
                    }
                    else if (CanHitW > tmpW)
                    {
                        ForWTarget = allyhero;
                    }

                    //R
                    if (tmpR == 0 && CanHitR > 0)
                    {
                        ForRTarget = allyhero;
                        tmpR = CanHitR;
                    }
                    else if (CanHitR > tmpR)
                    {
                        ForRTarget = allyhero;
                        tmpR = CanHitR;
                    }
                    
                    switch (BallManager.HasBall)
                    {
                        case BallStatus.Me:
                            break;
                        case BallStatus.Ally:
                            var c = CountAllyAOETarget(BallManager.hero);
                            var AllyCanHitW = c.Item1;
                            var AllyCanHitR = c.Item2;

                            //W
                            if (DoomBallCountTmpW == 0 && AllyCanHitW > 0)
                            {
                                DoomBallCountTmpW = AllyCanHitW;
                            }
                            else if (AllyCanHitW > DoomBallCountTmpW)
                            {
                                DoomBallCountTmpW = AllyCanHitW;
                            }

                            //R
                            if (DoomBallCountTmpR == 0 && AllyCanHitR > 0)
                            {
                                DoomBallCountTmpR = AllyCanHitR;
                            }
                            else if (AllyCanHitR > DoomBallCountTmpR)
                            {
                                DoomBallCountTmpR = AllyCanHitR;
                            }
                            break;
                        case BallStatus.Land:
                            var b = CountAllyAOETarget(BallManager.BallPos);
                            var LanCanHitW = b.Item1;
                            var LanCanHitR = b.Item2;

                            //W
                            if (DoomBallCountTmpW == 0 && LanCanHitW > 0)
                            {
                                DoomBallCountTmpW = LanCanHitW;
                            }
                            else if (b.Item1 > DoomBallCountTmpW)
                            {
                                DoomBallCountTmpW = LanCanHitW;
                            }

                            //R
                            if (DoomBallCountTmpR == 0 && LanCanHitR > 0)
                            {
                                DoomBallCountTmpR = LanCanHitR;
                            }
                            else if (LanCanHitR > DoomBallCountTmpR)
                            {
                                DoomBallCountTmpR = LanCanHitR;
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (tmpR > DoomBallCountTmpR && tmpR >= config.Item("minNoEnemies", true).GetValue<Slider>().Value)
                {
                    //E.CastOnUnit(ForRTarget);
                    return new Tuple<Obj_AI_Hero,Obj_AI_Hero>(null, ForRTarget);
                }
                else if (tmpW > DoomBallCountTmpW && tmpW >= config.Item("minNoWEnemies", true).GetValue<Slider>().Value)
                {
                    //E.CastOnUnit(ForWTarget);
                    return new Tuple<Obj_AI_Hero, Obj_AI_Hero>(ForWTarget, null);
                }
                else
                {
                    return new Tuple<Obj_AI_Hero, Obj_AI_Hero>(null, null);
                }
            }
            else
                return new Tuple<Obj_AI_Hero, Obj_AI_Hero>(null, null);
        }

        private Tuple<int,int> CountAllyAOETarget(Obj_AI_Hero target)
        {
            var CanHitW = 0;
            var CanHitR = 0;

            foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x)))
            {
                if (W.IsReady() && target.Distance(enemyhero) <= W.Range)
                {
                    CanHitW = CanHitW + 1;
                }
                if (R.IsReady() && target.Distance(enemyhero) <= R.Range)
                {
                    CanHitR = CanHitR + 1;
                }
            }
            return new Tuple<int, int>(CanHitW, CanHitR);
        }

        private Tuple<int, int> CountAllyAOETarget(Vector3 target)
        {
            var CanHitW = 0;
            var CanHitR = 0;

            foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && isValidTarget(x)))
            {
                if (W.IsReady() && target.Distance(enemyhero.Position) <= W.Range)
                {
                    CanHitW = CanHitW + 1;
                }
                if (R.IsReady() && target.Distance(enemyhero.Position) <= R.Range)
                {
                    CanHitR = CanHitR + 1;
                }
            }
            return new Tuple<int, int>(CanHitW, CanHitR);
        }

        /// <R>
        ///     R
        /// </summary>
        /// <param name="target"></param>

        private void CastR(Obj_AI_Hero target)
        {
            if (!R.IsReady() || !isValidTarget(target) || target == null)
                return;

            PredictionOutput predict = GetPCircle(BallManager.BallPos, R, target, true);
            if (predict.UnitPosition.Distance(BallManager.BallPos) < R.Width)
            {
                R.Cast();
            }
        }

        private bool isTargetInRRange(Obj_AI_Hero target, bool cd = false)
        {
            if (!isValidTarget(target))
                return false;

            var dd = false;
            if (!cd)
            {
                dd = true;
            }
            else if (cd && isTargetKillable(target))
            {
                dd = true;
            }

            if (dd)
            {
                PredictionOutput predict = GetPCircle(BallManager.BallPos, R, target, true);
                if (predict.UnitPosition.Distance(BallManager.BallPos) < R.Width)
                {
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        private bool isTargetKillable(Obj_AI_Hero target)
        {
            if (!isValidTarget(target))
                return false;

            var status = false;
            foreach(Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                if (isValidTarget(enemyhero) && target == enemyhero)
                {
                    var dmg = R.GetDamage(target);
                    
                    if (dmg >= target.Health)
                    {
                        status = true;
                        break;
                    }
                }
            }
            return status;
        }

        /// <Dmg>
        ///     Dmg
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>

        private float GetComboDamage(Obj_AI_Hero target)
        {
            var result = 0f;
            if (Q.IsReady())
            {
                result += 2 * Q.GetDamage(target);
            }

            if (W.IsReady())
            {
                result += W.GetDamage(target);
            }

            if (R.IsReady())
            {
                result += R.GetDamage(target);
            }

            result += 2 * (float)Player.GetAutoAttackDamage(target);

            return result;
        }

        /// <BallManager>
        ///     BallManager
        /// </summary>

        private void BallUpdater()
        {
            if (ObjectManager.Player.HasBuff("OrianaGhostSelf"))
            {
                BallManager.BallPos = Player.Position;
                BallManager.HasBall = BallStatus.Me;
                BallManager.hero = Player;
                BallManager.Moving = false;
            }

            foreach (Obj_AI_Hero allyhero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && !x.IsMe))
            {
                if (allyhero.HasBuff("OrianaGhost"))
                {
                    BallManager.BallPos = allyhero.Position;
                    BallManager.HasBall = BallStatus.Ally;
                    BallManager.hero = allyhero;
                    BallManager.Moving = false;
                    break;
                }
            }
        }

        protected override void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (sender.Name == "Orianna_Base_Q_yomu_ring_green.troy")
            {
                BallManager.BallPos = sender.Position;
                BallManager.HasBall = BallStatus.Land;
                BallManager.Moving = false;
            }
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (sender.IsMe)
            {
                switch (args.SData.Name)
                {
                    case "OrianaIzunaCommand":
                        BallManager.Moving = true;
                        break;
                    case "OrianaRedactCommand":
                        BallManager.Moving = true;
                        break;
                }
            }

            if (sender.IsAlly && config.Item("useInitiator", true).GetValue<bool>())
            {
                var a = from x in Initiator.InitiatorList
                        where x.Spelldata.ToLower() == args.SData.Name.ToLower() && config.Item(x.Spell, true).GetValue<bool>()
                        select x;
                if (a.Count() > 0)
                {
                    if (E.IsReady() && Player.Distance(sender) < E.Range)
                    {
                        E.CastOnUnit(sender);
                    }
                }
            }
        }

        /// <Event>
        ///     Event
        /// </summary>
        /// <param name="args"></param>

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            BallUpdater();
            Combo();
        }

        protected override void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (!R.IsReady() || !config.Item("BlockR", true).GetValue<bool>())
                return;

            var a = Utility.CountEnemiesInRange(BallManager.BallPos, R.Width - 15);
            if (args.Slot == SpellSlot.R && a < 1)
            {
                args.Process = false;
            }
        }

        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var QCircle = config.Item("Qcircle").GetValue<Circle>();

            if (QCircle.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, QCircle.Color);
                Drawing.DrawCircle(BallManager.BallPos, 150, System.Drawing.Color.Red);
            }
        }
    }
}
