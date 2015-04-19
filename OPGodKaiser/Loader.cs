using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace OPGodKaiser
{
    class Loader
    {
        public Loader()
        {
            switch(ObjectManager.Player.ChampionName.ToLower())
            {
                case "vladimir":
                    new Champions.Vladimir();
                    break;
                case "udyr":
                    new Champions.Udyr();
                    break;
                    /*
                case "thresh":
                    new Champions.Thresh();
                    break;
                case "xerath":
                    new Champions.Xerath();
                    break;
                case "ezreal":
                    new Champions.Ezreal();
                    break;
                case "twistedfate":
                    new Champions.TwistedFate();
                    break;
                case "orianna":
                    new Champions.Orianna();
                    break;*/
                case "nautilus":
                    new Champions.Nautilus();
                    break;
            }
        }
    }
}
