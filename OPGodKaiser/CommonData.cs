using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OPGodKaiser
{
    class CommonData
    {
        protected static readonly Obj_AI_Hero Player = ObjectManager.Player;

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
        protected int[] RMana = { 0, 0, 0 };

        //Menu
        public static Menu config;

        //predict
        Vector3 predictPos;
        Vector3 _ca;

        //OrbWalk
        protected static Orbwalking.Orbwalker Orbwalker;

        protected CommonData()
        {
            CommonMenu();
            InitPluginEvents();
        }

        private void InitPluginEvents()
        {
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += OnBeforeAttack;
            Orbwalking.AfterAttack += OnAfterAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast +=Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Hero.OnPlayAnimation += Obj_AI_Hero_OnPlayAnimation;
            
            //Game.OnGameSendPacket += OnSendPacket;
            //Game.OnGameProcessPacket += OnProcessPacket;
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
                key.AddItem(new MenuItem("HarassActiveT", "Harass (toggle)", true).SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LastHitKey", "LastHit", true).SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitKeyT", "LastHit (toggle)", true).SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "Lane Clear", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LaneClearActiveT", "Lane Clear (toggle)", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("StackE", "StackE (toggle)", true).SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));

                config.AddSubMenu(key);
            }

            config.AddToMainMenu();
        }

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
                                if (collision && hitchance >= HitChance.High)
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
                                        if (collision && hitchance >= HitChance.High && AoetargetCount >= requireHitCount)
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
                                if (collision && hitchance >= HitChance.High)
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
                                if (collision && hitchance >= HitChance.High)
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
        
        protected Vector3 GetPredictedPos(Obj_AI_Hero target, float _range,float _speed, float _delay, float _width)
        {
            // dek prediction  
            // lots bugs
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
                var targetPos1 = target.Position;
                var targetPos2 = target.Position;
                var canmoveDistance = target.MoveSpeed * Time;
                var dba = canmoveDistance;
                //Vector3 _ca;
                Vector3 aca;
                var waypoints = target.GetWaypoints();

                if (target.IsMoving)
                {
                    bool DashingStatus = target.IsDashing();
                    float DashingSpeed = target.GetDashInfo().Speed;
                    Vector3 DashingEndPos = target.GetDashInfo().EndPos.To3D();
                    float DashingDuration = target.GetDashInfo().Duration;

                    if (DashingStatus)
                    {
                        Game.PrintChat("Debug : Dashing Pred");
                        var DashingDistance = DashingSpeed * Time;
                        targetPos2 = DashingEndPos;
                        predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * DashingDistance;
                        if (Geometry.Distance(predictPos, target.Position) > Geometry.Distance(DashingEndPos, target.Position))
                        {
                            predictPos = DashingEndPos;
                        }
                    }
                    else
                    {
                        foreach (Vector2 a in waypoints)
                        {
                            _ca = a.To3D();
                            if(_ca.IsValid())
                            {
                                //aca = aca || _ca;
                                var DashingDistance = Geometry.Distance(_ca, target.Position);
                                dba = dba - DashingDistance;
                                if (dba <= 0)
                                {
                                    targetPos2 = _ca;
                                    predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * canmoveDistance;
                                    Game.PrintChat("Debug : Normal Pred");
                                    break;
                                }
                            }
                        }
                        if (dba > 0 && _ca.IsValid())
                        {
                            var DashingDistance = Geometry.Distance(_ca, target.Position);
                            var acc = dba - DashingDistance;
                            if (width >= acc)
                            {
                                targetPos2 = _ca;
                                predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * canmoveDistance;
                                Game.PrintChat("Debug : width Pred");
                            }
                            else
                            {
                                predictPos = targetPos1 + targetPos2 - targetPos1.Normalized() * canmoveDistance;
                                Game.PrintChat("Debug : other Pred");
                            }
                        }
                    }
                }
                else
                {
                    Game.PrintChat("Debug : Not moving Pred");
                    predictPos = targetPos1;
                }
                if (predictPos.IsValid())
                {
                    if (range >= Player.Distance(predictPos))
                    {
                        return predictPos;
                    }
                }
                return predictPos;
            }
            else
            {
                return predictPos;
            }
            //return predictPos;
        }

        /// <summary>
        ///     Target Info
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>
        /// 
        protected Obj_AI_Hero GetTarget(Spell _spell,TargetSelector.DamageType _DamageType)
        {
            var a = TargetSelector.GetTarget(_spell.Range, _DamageType);
            return a;
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

        protected virtual void OnLoad(EventArgs args)
        {
        }

        protected virtual void OnDraw(EventArgs args)
        {
        }

        protected virtual void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            
        }
    }
}
