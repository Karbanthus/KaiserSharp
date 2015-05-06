using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace OpProject
{
    class CommonData
    {
        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        //Spells
        protected Spell Q { get; set; }
        protected Spell W { get; set; }
        protected Spell E { get; set; }
        protected Spell R { get; set; }
        protected readonly List<Spell> SpellList = new List<Spell>();

        protected SpellSlot IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

        //Mana
        protected int[] QMana = { 0, 0, 0, 0, 0 };
        protected int[] WMana = { 0, 0, 0, 0, 0 };
        protected int[] EMana = { 0, 0, 0, 0, 0 };
        protected int[] RMana = { 0, 0, 0, 0, 0 };

        //Menu
        public static Menu config;

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
            Helpers.DamagePrediction.OnSpellDmg += DamagePrediction_OnSpellDmg;
            Evade.Targeted.TargetedDetector.TSDetector += TargetedDetector_TSDetector;
            Obj_AI_Base.OnDamage += Obj_AI_Base_OnDamage;
            Game.OnProcessPacket += Game_OnProcessPacket;
            Game.OnSendPacket += Game_OnSendPacket;
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

        /// <Target>
        ///     Target Info
        /// </summary>
        /// <param name="hero"></param>
        /// <returns></returns>
        /// 

        public static bool isValidTarget(Obj_AI_Base hero)
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

        private static bool isUntargetable(Obj_AI_Base hero)
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

        protected void CS(Obj_AI_Base target, bool aoe, Spell spell)
        {
            if (target == null || target.IsDead || !isValidTarget(target))
                return;

            var a = spell.GetPrediction(target, aoe);
            var b = target.GetWaypoints().Count;

            if (a.Hitchance == HitChance.Immobile)
            {
                spell.Cast(target);
            }
            else if (b <= 2)
            {
                spell.CastIfHitchanceEquals(target, HitChance.High);
            }
            else if (b > 2)
            {
                spell.CastIfHitchanceEquals(target, HitChance.VeryHigh);
            }
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

            Console.WriteLine("Debug-{0} : {1}", sector, str);
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

            Console.WriteLine("------------------");
        }

        /// <summary>
        ///     Virtual Processes
        /// </summary>
        /// <param name="args"></param>

        protected virtual void Game_OnProcessPacket(GamePacketEventArgs args)
        {
        }

        protected virtual void Game_OnSendPacket(GamePacketEventArgs args)
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

        protected virtual void DamagePrediction_OnSpellDmg(Obj_AI_Hero sender, Obj_AI_Hero target, float dmg)
        {
        }

        protected virtual void TargetedDetector_TSDetector(Evade.Targeted.TargetSpell spell)
        {
        }

        protected void Obj_AI_Base_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {

        }
    }
}
