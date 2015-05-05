using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace OpProject
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
                case "nautilus":
                    new Champions.Nautilus();
                    break;
                case "amumu":
                    new Champions.Amumu();
                    break;
                case "karthus":
                    new Champions.Karthus();
                    break;
                case "nunu":
                    new Champions.Nunu();
                    break;
                case "sivir":
                    new Champions.Sivir();
                    break;
                case "morgana":
                    new Champions.Morgana();
                    break;
                case "ryze":
                    new Champions.Ryze();
                    break;
                case "cassiopeia":
                    new Champions.Cassiopeia();
                    break;


                /*
                case "thresh":
                    new Champions.Thresh();
                    break;
                case "orianna":
                    new Champions.Orianna();
                    break;*/
                 

                    /*
                case "reksai":
                    new Champions.RekSai();
                    break;
                case "xerath":
                    new Champions.Xerath();
                    break;
                case "ezreal":
                    new Champions.Ezreal();
                    break;
                case "twistedfate":
                    new Champions.TwistedFate();
                    break;*/
            }
        }
    }
}
