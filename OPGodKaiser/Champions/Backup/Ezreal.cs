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
    class Ezreal : CommonData
    {
        public Ezreal()
        {
            LoadSpellData();

            Game.PrintChat("<font color=\"#66CCFF\" >Kaiser's </font><font color=\"#CCFFFF\" >{0}</font> - " +
               "<font color=\"#FFFFFF\" >Version " + Assembly.GetExecutingAssembly().GetName().Version + "</font>",Player.ChampionName);
        }

        private void LoadSpellData()
        {
            Q = new Spell(SpellSlot.Q, 1200);

            Q.SetSkillshot(0.25f, 50f, 2000, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (config.Item("ComboActive",true).GetValue<KeyBind>().Active)
            {
                var target =TargetSelector.GetTarget(1500,TargetSelector.DamageType.Physical);
                if (target != null && Q.IsReady())
                {
                    var predictpos = GetPredictedPos2(target, 1200f, 0.25f, 50f, 2000f);

                    if (!predictpos.IsZero && Q.IsReady())
                    {
                        Q.Cast(predictpos);
                    }
                }
            }
        }
    }
}
