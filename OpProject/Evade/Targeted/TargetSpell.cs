using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OpProject.Evade.Targeted
{
    public class TargetSpell
    {
        public Obj_AI_Hero Sender { get; set; }
        public Obj_AI_Hero Target { get; set; }
        public TargetedSpellData Spell { get; set; }
        public int StartTick { get; set; }
        public Vector3 StartPosition { get; set; }

        public Vector3 EndPosition
        {
            get { return Target.ServerPosition; }
        }

        public int EndTick
        {
            get { return (int) (StartTick + Spell.Delay + 1000 * (StartPosition.Distance(EndPosition) / Spell.Speed)); }
        }

        public bool IsActive
        {
            get { return Environment.TickCount < EndTick; }
        }
    }

    public class TargetedSpellData
    {
        public string ChampionName { get; set; }
        public string SpellName { get; set; }
        public SpellSlot Slot { get; set; }
        public float Range { get; set; }
        public float Delay { get; set; }
        public float Speed { get; set; }
        public DangerousLevel DangerousLevel { get; set; }
    }

    public enum DangerousLevel
    {
        VeryHigh,
        High,
        Medium,
        Low
    }
}
