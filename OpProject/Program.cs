using System;
using LeagueSharp;
using LeagueSharp.Common;

/* Version Histroy 
 * 1.0.0.0 : Initial release Prject
 * 1.0.1.0 : Add Champions Thresh , Vladmir 
 * 1.0.1.1 : Fixed Vladimir Menu problem , DebugChat Deledted (2015-03-29)
 * 1.0.1.2 : Fixed Compile error (2015-03-29)
 * 1.0.2.0 : Add enemies Waypoints && Fixed Vladimir : Auto Stack E,Ult (2015-04-02)
 * 1.0.2.1 : Fixed Loader for L# sandbox
 * 1.0.4.0 : Add Champions Udyr , Nautilus
 * 1.0.5.0 : Add Champions Amumu
 * 1.0.6.0 : Add Champions Karthus
 * 1.0.6.2 : Fixed Udyr Don't work Combo
 * 1.0.6.3 : Fixed Karthus LastHit
 * 1.1.0.0 : Add Evade,Helpers , Nunu,Morgana,Sivir
 */


namespace OpProject
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
                var type = Type.GetType("OpProject.Champions." + ObjectManager.Player.ChampionName);

                if (type != null)
                {
                    new Loader();
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
