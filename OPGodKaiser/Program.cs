﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

/* Version Histroy 
 * 1.0.0.0 : Initial release Prject
 * 1.0.1.0 : Add Champions Thresh , Vladmir 
 */


namespace OPGodKaiser
{
    class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        static void Game_OnGameStart(EventArgs args)
        {
            try
            {
                var type = Type.GetType("OPGodKaiser.Champions." + ObjectManager.Player.ChampionName);

                if (type != null)
                {
                    Activator.CreateInstance(type);
                    return;
                }
                Game.PrintChat(ObjectManager.Player.ChampionName + " not supported");
            }
            catch (Exception e)
            {
                Game.PrintChat("",e);
            }
        }

    }
}