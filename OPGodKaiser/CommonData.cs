using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OPGodKaiser
{
    class CommonData
    {
        //protected static readonly Obj_AI_Hero Player = ObjectManager.Player;
        
        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        //Spells
        protected Spell Q;
        protected Spell W;
        protected Spell E;
        protected Spell R;
        protected readonly List<Spell> SpellList = new List<Spell>();

        protected SpellSlot IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

        //Damage

        //Mana
        protected int[] QMana = { 0, 0, 0, 0, 0 };
        protected int[] WMana = { 0, 0, 0, 0, 0 };
        protected int[] EMana = { 0, 0, 0, 0, 0 };
        protected int[] RMana = { 0, 0, 0, 0, 0 };

        //Menu
        public static Menu config;

        //predict
        Vector3 predictPos = new Vector3(0,0,0);
        Vector3 _ca;

        //OrbWalk
        protected static Orbwalking.Orbwalker Orbwalker;

        //Vector2 waypoint;

        protected CommonData()
        {
            CommonMenu();
            InitPluginEvents();
        }

        private void InitPluginEvents()
        {
            Game.OnUpdate += OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Orbwalking.BeforeAttack += OnBeforeAttack;
            Orbwalking.AfterAttack += OnAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast +=Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Hero.OnPlayAnimation += Obj_AI_Hero_OnPlayAnimation;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Obj_AI_Base.OnDelete += Obj_AI_Base_OnDelete;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;

            //Game.OnGameSendPacket += OnSendPacket;
            //Game.OnGameProcessPacket += OnProcessPacket;
        }

        protected virtual void SpecialMenu()
        {
            var SpecialMenu = new Menu("Kaiser's Special Ability", "Kaiser");

            SpecialMenu.AddItem(new MenuItem("WaypointActive", "Draw Enemy Waypoints").SetValue(true));
            SpecialMenu.AddItem(new MenuItem("debugM", "Develop Envirment-Debug Msg").SetValue(false));
            
            config.AddSubMenu(SpecialMenu);
        }

        protected void CommonMenu()
        {
            config = new Menu(Player.ChampionName, Player.ChampionName, true);
            
            //OrbWalk
            Orbwalker = new Orbwalking.Orbwalker(config.SubMenu("Orbwalking"));

            //Target selector
            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            {
                TargetSelector.AddToMenu(TargetSelectorMenu);

                config.AddSubMenu(TargetSelectorMenu);
            }

            //Key
            var key = new Menu("Key", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "Combo", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "Harass", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKey", "LastHit", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LaneClearActive", "Lane Clear", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

                config.AddSubMenu(key);
            }

            SpecialMenu();

            config.AddToMainMenu();
        }

 
        /// <Prediction>
        ///     Predict
        /// </summary>
        /// <param name="target"></param>
        /// <param name="spell"></param>
        /// <param name="_aoe"></param>
        /// <param name="_collision"></param>
        /// <param name="requireHitCount"></param>
        /// <returns></returns>

        protected bool GetPredicted(Obj_AI_Hero target, Spell spell, bool _aoe, bool _collision, int requireHitCount = 1)
        {
            //normal
            bool collision = spell.CastIfHitchanceEquals(target, HitChance.Collision);
            var GetPredict = spell.GetPrediction(target, _aoe);
            HitChance hitchance = GetPredict.Hitchance;
            bool result = false;

            //aoe
            var aoetarget = GetPredict.AoeTargetsHit;
            var AoetargetCount = GetPredict.AoeTargetsHitCount;

            //special
            bool Dashing = spell.CastIfHitchanceEquals(target, HitChance.Dashing);
            bool Immobile = spell.CastIfHitchanceEquals(target, HitChance.Immobile);

            //test
            var c = GetPredict.UnitPosition.To2D();
            var WaypointsList = target.GetWaypoints();

            //test
            /*
            foreach (Vector2 waypoint in WaypointsList)
            {
                if (waypoint == c)
                {
                    Game.PrintChat("[test] waypoint == predict pos");
                }
            }*/

            //predict
            if (target.IsValid && target != null && !target.IsDead)
            {
                if (_aoe) // check aoe
                {
                    if (Player.Distance(target) < spell.Range && spell.IsReady() && isValidTarget(target))
                    {
                        if (Immobile)
                        {
                            result = true;
                        }
                        else if (Dashing)
                        {
                            if (_collision)
                            {
                                if (!collision && hitchance >= HitChance.High)
                                {
                                    result = true;
                                }
                            }
                            else
                            {
                                if (hitchance >= HitChance.High)
                                {
                                    result = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (Obj_AI_Hero targetable in aoetarget)
                            {
                                if (targetable == target)
                                {
                                    if (_collision)
                                    {
                                        if (!collision && hitchance >= HitChance.High && AoetargetCount >= requireHitCount)
                                        {
                                            result = true;
                                        }
                                    }
                                    else
                                    {
                                        if (hitchance >= HitChance.High && AoetargetCount >= requireHitCount)
                                        {
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else // check aoe
                {
                    if (Player.Distance(target) < spell.Range && spell.IsReady() && isValidTarget(target))
                    {
                        if (Immobile)
                        {
                            result = true;
                        }
                        else if (Dashing)
                        {
                            if (_collision)
                            {
                                if (!collision && hitchance >= HitChance.High)
                                {
                                    result = true;
                                }
                            }
                            else
                            {
                                if (hitchance >= HitChance.High)
                                {
                                    result = true;
                                }
                            }
                        }
                        else
                        {
                            if (_collision)
                            {
                                if (!collision && hitchance >= HitChance.High)
                                {
                                    result = true;
                                }
                            }
                            else
                            {
                                if (hitchance >= HitChance.High)
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected Vector3 GetPredictedPos(Obj_AI_Hero target, float _range, float _speed, float _delay, float _width)
        {
            // dek prediction  
            // lots bugs about get distance
            if (isValidTarget(target))
            {
                float range = _range;
                float speed = _speed;
                float delay = _delay;
                float width = 0;
                if (_width > 0)
                {
                    width = _width / 2;
                }
                var distance = Player.Distance(target);
                var Time = distance / speed;
                if (speed != float.MaxValue && speed > 0)
                {
                    Time = Time + delay;
                }
                else
                {
                    speed = distance / delay;
                    Time = distance / speed;
                }

                //Vector3 predictPos;
                var targetPos1 = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                var targetPos2 = new Vector3(target.Position.X, target.Position.Y, target.Position.Z);
                var canmoveDistance = target.MoveSpeed * Time;
                var dba = canmoveDistance;
                //Vector3 _ca;
                //Vector3 aca;
                List<Vector2> waypoints = target.GetWaypoints();

                if (target.IsMoving)
                {
                    bool DashingStatus = target.IsDashing();
                    float DashingSpeed = target.GetDashInfo().Speed;
                    Vector3 DashingEndPos = target.GetDashInfo().EndPos.To3D();
                    float DashingDuration = target.GetDashInfo().Duration;

                    if (DashingStatus)
                    {
                        //Game.PrintChat("Debug : Dashing Pred");
                        var DashingDistance = DashingSpeed * Time;
                        targetPos2 = new Vector3(DashingEndPos.X, DashingEndPos.Y, DashingEndPos.Z);
                        predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * DashingDistance;
                        if (target.Distance(predictPos) > target.Distance(DashingEndPos))
                        {
                            predictPos = new Vector3(DashingEndPos.X, DashingEndPos.Y, DashingEndPos.Z);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < waypoints.Count - 1; i++)
                        {
                            _ca = waypoints[i].To3D();
                            if (_ca.IsValid())
                            {
                                //aca = aca || _ca;
                                var DashingDistance = target.Distance(_ca);

                                dba = dba - DashingDistance;
                                if (dba <= 0)
                                {
                                    targetPos2 = new Vector3(_ca.X, _ca.Y, _ca.Z);
                                    predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * canmoveDistance;
                                    //Game.PrintChat("Debug : Normal Pred");
                                    break;
                                }
                            }
                        }
                        if (dba > 0 && _ca.IsValid())
                        {
                            var DashingDistance = target.Distance(_ca);
                            var acc = dba - DashingDistance;
                            if (width >= acc)
                            {
                                targetPos2 = new Vector3(_ca.X, _ca.Y, _ca.Z);
                                predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * canmoveDistance;
                                //Game.PrintChat("Debug : width Pred");
                            }
                            else
                            {
                                predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * canmoveDistance;
                                //Game.PrintChat("Debug : other Pred");
                            }
                        }
                    }
                }
                else
                {
                    //Game.PrintChat("Debug : Not moving Pred");
                    predictPos = targetPos1;
                }
                if (predictPos.IsValid())
                {
                    if (range >= Player.Distance(predictPos))
                    {
                        return predictPos;
                    }
                }
                return predictPos = new Vector3(0, 0, 0);
            }
            else
            {
                return predictPos = new Vector3(0, 0, 0);
            }
            //return predictPos;
        }

        protected Vector3 GetPredictedPos2(Obj_AI_Hero target, float _range, float _delay, float _width, float _speed = float.MaxValue)
        {
            //beta ver
            predictPos = new Vector3(0, 0, 0);
            float Time = 0;
            if (_speed != float.MaxValue && _speed > 0)
            {
                Time = Time + _delay;
            }
            else
            {
                Time = _delay;
            }

            List<Vector2> waypoints = target.GetWaypoints();
            bool DashingStatus = target.IsDashing();
            float DashingSpeed = target.GetDashInfo().Speed;
            Vector3 DashingEndPos = target.GetDashInfo().EndPos.To3D();
            float DashingDuration = target.GetDashInfo().Duration;

            if (DashingStatus)
            {
                Game.PrintChat("Debug : Dashing Pred");
                var DashingDistance = DashingSpeed * Time;
                var targetPos2 = new Vector3(DashingEndPos.X, DashingEndPos.Y, DashingEndPos.Z);
                predictPos = target.Position + targetPos2 - target.Position.Normalized() * DashingDistance;
                if (target.Distance(predictPos) > target.Distance(DashingEndPos))
                {
                    predictPos = new Vector3(DashingEndPos.X, DashingEndPos.Y, DashingEndPos.Z);
                }
            }
            else if (waypoints.Count > 1)
            {
                for (int i = 0; i < waypoints.Count - 1; i++)
                {
                    var a = waypoints[i];
                    var b = waypoints[i + 1];
                    var UnitVector = (b - a).Normalized();

                    for (int j = 0; j < 10; j++)
                    {
                        Time = Time + 0.05f;
                        var dd = (a + UnitVector * target.MoveSpeed * Time).To3D();
                        var ff = Time * _speed;

                        if (Math.Abs(Player.Distance(dd) - ff) < 20)
                        {
                            Game.PrintChat("Debug : Normal Pred");
                            predictPos = dd;
                            break;
                        }
                    }
                    if (!predictPos.IsZero)
                    {
                        break;
                    }
                }
            }
            else
            {
                Game.PrintChat("Debug : Not moving Pred");
                predictPos = target.Position;
            }

            if (!predictPos.IsZero)
            {
                if (_range >= Player.Distance(predictPos))
                {
                    return predictPos;
                }
                else
                {
                    return predictPos = new Vector3(0, 0, 0);
                }
            }
            else
            {
                return predictPos = new Vector3(0, 0, 0);
            }
        }

        protected PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, float delay, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay + delay,
                Radius = spell.Width,
                Speed = spell.Speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }

        protected PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = spell.Width,
                Speed = spell.Speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = Player.ServerPosition,
                Aoe = aoe,
            });
        }

        protected PredictionOutput GetPCircle(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = 1,
                Speed = float.MaxValue,
                From = pos,
                Range = float.MaxValue,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = pos,
                Aoe = aoe,
            });
        }


        /// <Target>
        ///     Target Info
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>
        /// 

        protected Obj_AI_Hero GetTarget(Spell _spell, TargetSelector.DamageType _DamageType, bool IsCharge = false)
        {
            if (IsCharge)
            {
                var a = TargetSelector.GetTarget(_spell.ChargedMaxRange, _DamageType);
                return a;
            }
            else if (!IsCharge)
            {
                var a = TargetSelector.GetTarget(_spell.Range, _DamageType);
                return a;
            }
            else
                return null;
        }

        protected static bool isValidTarget(Obj_AI_Hero hero)
        {
            if (hero.IsValid && !hero.IsDead && hero.IsVisible && !isUntargetable(hero))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool isUntargetable(Obj_AI_Hero hero)
        {
            var untargetable = false;

            string[] untargetableBuffs = new string[] { "aatroxpassivedeath", "elisespidere", "KarthusDeathDefiedBuff", "JudicatorIntervention", "kogmawicathiansurprise", "sionpassivezombie", "UndyingRage", "VladimirSanguinePool", "woogletswitchcap", "zhonyasringshield", "chronorevive", "ChronoShift", "zyrapqueenofthorns" };

            if (hero == null)
            {
                return untargetable;
            }

            foreach (string buffnames in untargetableBuffs)
            {
                if (hero != null && hero.HasBuff(buffnames))
                {
                    untargetable = true;
                    break;
                }
            }

            if (hero.BaseSkinName == "Fizz" && !hero.IsTargetable)
            {
                untargetable = true;
            }

            return untargetable;
        }

        protected bool EnemyHasShield(Obj_AI_Hero target)
        {
            var status = false;
            if (target.HasBuff("blackshield"))
            {
                status = true;
            }

            if (target.HasBuff("sivire"))
            {
                status = true;
            }

            if (target.HasBuff("nocturneshroudofdarkness"))
            {
                status = true;
            }

            if (target.HasBuff("bansheesveil"))
            {
                status = true;
            }
            return status;
        }

        private struct ChampionPower
        {
            public const float AttackDamage = 36f;
            public const float AbilityPower = 21.75f;
            public const float Health = -1.66f;
            public const float Armor = -10f;
        }

        private struct PowerInfo
        {
            public static Obj_AI_Hero Champion;
            public static float power;
        }

        private List<PowerInfo> PowerList = new List<PowerInfo>();

        protected Obj_AI_Hero FindDmgStrongHero(List<Obj_AI_Hero> target, float range)
        {
            foreach(var hero in target.Where(x => Player.Distance(x) < range))
            {
                Single value = 0;
                value += hero.TotalAttackDamage * ChampionPower.AttackDamage;
                value += hero.TotalMagicalDamage * ChampionPower.AbilityPower;
                value += hero.MaxHealth * ChampionPower.Health;
                value += hero.Armor * ChampionPower.Armor;
                //Console.WriteLine("{0} : {1}", hero.ChampionName, value);
                if (PowerInfo.Champion == null)
                {
                    PowerInfo.Champion = hero;
                    PowerInfo.power = value;
                }
                else if (value > PowerInfo.power)
                {
                    PowerInfo.Champion = hero;
                    PowerInfo.power = value;
                }
            }

            return PowerInfo.Champion;
        }

        /// <Collision>
        ///     Collision
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>

        protected List<Obj_AI_Base> GetWallCollision(List<Vector3> positions, Vector3 From)
        {
            var result = new List<Obj_AI_Base>();

            foreach (var position in positions)
            {
                var step = position.Distance(From) / 20;

                for (var i = 0; i < 20; i++)
                {
                    var p = From.To2D().Extend(position.To2D(), step * i);
                    if (NavMesh.GetCollisionFlags(p.X, p.Y).HasFlag(CollisionFlags.Wall))
                    {
                        result.Add(ObjectManager.Player);
                    }
                }
            }

            return result.Distinct().ToList();
        }

        // Tks xSalice VectorSegment code
        protected Object[] VectorPointProjectionOnLineSegment(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float cx = v3.X;
            float cy = v3.Y;
            float ax = v1.X;
            float ay = v1.Y;
            float bx = v2.X;
            float by = v2.Y;
            float rL = ((cx - ax) * (bx - ax) + (cy - ay) * (by - ay)) /
                       ((float)Math.Pow(bx - ax, 2) + (float)Math.Pow(by - ay, 2));
            var pointLine = new Vector2(ax + rL * (bx - ax), ay + rL * (by - ay));
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }
            bool isOnSegment = rS.CompareTo(rL) == 0;
            Vector2 pointSegment = isOnSegment ? pointLine : new Vector2(ax + rS * (bx - ax), ay + rS * (@by - ay));
            return new object[] { pointSegment, pointLine, isOnSegment };
        }

        /// <summary>
        ///     CountNearHero
        /// </summary>
        /// <param name="args"></param>

        protected int countEnemiesNearTarget(Obj_AI_Hero target)
        {
            var count = -1;
            if (target != null)
            {
                if (isValidTarget(target))
                {
                    foreach (Obj_AI_Hero enemyhero in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => isValidTarget(enemy) && enemy != target && enemy.IsEnemy))
                    {
                        if (Geometry.Distance(target, enemyhero) <= 1500)
                        {
                            count = count + 1;
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        ///     GetCOmboDmg
        /// </summary>
        /// <param name="target"></param>
        /// <param name="ally"></param>
        /// <returns></returns>

        protected double GetAlliesComboDmg(Obj_AI_Hero target, Obj_AI_Hero ally)
        {
            return ally.GetSpellDamage(target, SpellSlot.Q) + ally.GetSpellDamage(target, SpellSlot.W) + ally.GetSpellDamage(target, SpellSlot.E) + ally.GetSpellDamage(target, SpellSlot.R);
        }

        /// <Drwaing>
        ///     Drwaing
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        
        private void DrawCross(float x, float y, float size, float thickness, Color color)
        {
            var topLeft = new Vector2(x - 10 * size, y - 10 * size);
            var topRight = new Vector2(x + 10 * size, y - 10 * size);
            var botLeft = new Vector2(x - 10 * size, y + 10 * size);
            var botRight = new Vector2(x + 10 * size, y + 10 * size);

            Drawing.DrawLine(topLeft.X, topLeft.Y, botRight.X, botRight.Y, thickness, color);
            Drawing.DrawLine(topRight.X, topRight.Y, botLeft.X, botLeft.Y, thickness, color);
        }

        /// <Debug && Develop Envirment>
        ///     Debug
        /// </summary>

        protected void debug<T>(string sector, T str)
        {
            if (!config.Item("debugM").GetValue<bool>())
                return;

            Game.PrintChat("Debug-{0} : {1}",sector ,str);
        }

        protected void Spelldebug(SpellSlot a)
        {
            // SData
            var data = Player.Spellbook.GetSpell(a).SData;

            // Range
            var range = data.CastRange;
            var range2 = data.CastRadius;
            var range3 = data.CastRangeDisplayOverride;
            
            Console.WriteLine("range1 : " + range);
            Console.WriteLine("range2 : " + range2);
            Console.WriteLine("range3 : " + range3);


            // Delay
            var delay = data.CastFrame / 30f;
            Console.WriteLine("delay : " + delay);

            // Speed
            var speed = data.MissileSpeed;
            var speed2 = data.MissileMaxSpeed;
            var speed3 = data.MissileMaxSpeed;
            Console.WriteLine("speed : " + speed);
            Console.WriteLine("speed2 : " + speed2);
            Console.WriteLine("speed3 : " + speed3);

            // Width
            var width = data.LineWidth;
            Console.WriteLine("width : " + width);
        }

        /// <summary>
        ///     Virtual Processes
        /// </summary>
        /// <param name="args"></param>

        protected virtual void OnProcessPacket(GamePacketEventArgs args)
        {
        }

        protected virtual void OnSendPacket(GamePacketEventArgs args)
        {
        }

        protected virtual void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            
        }

        protected virtual void OnPossibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
        }

        protected virtual void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
        }

        protected virtual void OnUpdate(EventArgs args)
        {
            
        }

        protected virtual void OnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
        }

        protected virtual void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
        }
        //base
        protected virtual void OnLoad(EventArgs args)
        {
        }
        //base
        protected virtual void OnDraw(EventArgs args)
        {
            if (config.Item("WaypointActive").GetValue<bool>())
            {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy.IsEnemy && !enemy.IsDead && enemy.IsValid && enemy.IsVisible)
                    {
                        List<Vector2> waypoints = enemy.GetWaypoints();

                        Vector2 waypoint = Drawing.WorldToScreen(waypoints[0].To3D());
                        Vector2 waypoint2 = Drawing.WorldToScreen(waypoints[waypoints.Count - 1].To3D());

                        if (waypoint2.IsValid())
                        {
                            Drawing.DrawLine(waypoint[0], waypoint[1], waypoint2[0], waypoint2[1], 3, System.Drawing.Color.IndianRed);
                        }
                        
                        if (waypoints.Count > 1)
                        {
                            DrawCross(waypoint2[0], waypoint2[1], 1.0f, 3.0f, System.Drawing.Color.Yellow);
                            Drawing.DrawText(waypoint2[0], waypoint2[1], System.Drawing.Color.FromArgb(255, 255, 255, 255), enemy.ChampionName);
                        }
                    }
                }
            }
            // Debug
            if (!predictPos.IsZero)
            {
                Drawing.DrawCircle(predictPos,200,System.Drawing.Color.Red);
            }
        }
        
        protected virtual void Drawing_OnEndScene(EventArgs args)
        {
            if (config.Item("WaypointActive").GetValue<bool>())
            {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy.IsEnemy && !enemy.IsDead && enemy.IsValid && enemy.IsVisible)
                    {
                        List<Vector2> waypoints = enemy.GetWaypoints();

                        Vector2 waypoint = Drawing.WorldToMinimap(waypoints[0].To3D());
                        Vector2 waypoint2 = Drawing.WorldToMinimap(waypoints[waypoints.Count - 1].To3D());

                        if (waypoint2.IsValid())
                        {
                            Drawing.DrawLine(waypoint[0], waypoint[1], waypoint2[0], waypoint2[1], 2, System.Drawing.Color.Red);
                        }
                    }
                }
            }
        }

        protected virtual void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            
        }

        protected virtual void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            
        }

        protected virtual void Game_OnWndProc(WndEventArgs args)
        {
            
        }

        protected virtual void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            
        }

        protected virtual void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            
        }

        protected virtual void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            
        }
    }
}
