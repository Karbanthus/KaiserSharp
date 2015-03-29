using System;
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
 * 1.0.1.1 : Fixed Vladimir Menu problem , DebugChat Deledted (2015-03-29)
 * 1.0.1.2 : Fixed Compile error (2015-03-29)
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
