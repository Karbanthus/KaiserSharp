using System.Collections.Generic;
using LeagueSharp;

namespace OpProject.Evade.Targeted
{
    class TargetedSpellDatabase
    {
        public static List<TargetedSpellData> TargetedSpellDB = new List<TargetedSpellData>();

        static TargetedSpellDatabase()
        {
            #region Akali

            TargetedSpellDB.Add(new TargetedSpellData
                {
                    ChampionName = "akali",
                    SpellName = "akalimota",
                    Slot = SpellSlot.Q,
                    Range = 600,
                    Delay = 650,
                    Speed = 1000,
                    DangerousLevel = DangerousLevel.Low
                });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "akali",
                SpellName = "akalishadowdance",
                Slot = SpellSlot.R,
                Range = 800,
                Delay = 250,
                Speed = 2200,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Akali

            #region Alistar

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "alistar",
                SpellName = "headbutt",
                Slot = SpellSlot.W,
                Range = 650,
                Delay = 200,
                Speed = 0,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Alistar

            #region Anivia

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "anivia",
                SpellName = "frostbite",
                Slot = SpellSlot.E,
                Range = 650,
                Delay = 250,
                Speed = 1450,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Anivia

            #region Annie

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "annie",
                SpellName = "disintegrate",
                Slot = SpellSlot.Q,
                Range = 623,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Annie

            #region Brand

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "brand",
                SpellName = "brandconflagration",
                Slot = SpellSlot.E,
                Range = 625,
                Delay = 0,
                Speed = 1800,
                DangerousLevel = DangerousLevel.High
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "brand",
                SpellName = "brandwildfire",
                Slot = SpellSlot.R,
                Range = 0,
                Delay = 0,
                Speed = 1000,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Brand

            #region Caitlyn

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "caitlyn",
                SpellName = "caitlynaceinthehole",
                Slot = SpellSlot.R,
                Range = 2500,
                Delay = 0,
                Speed = 1500,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Caitlyn

            #region Cassiopeia

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "cassiopeia",
                SpellName = "cassiopeiatwinfang",
                Slot = SpellSlot.E,
                Range = 700,
                Delay = 0,
                Speed = 1900,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Cassiopeia

            #region Chogath

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "chogath",
                SpellName = "feast",
                Slot = SpellSlot.R,
                Range = 230,
                Delay = 0,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Chogath

            #region Darius

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "darius",
                SpellName = "dariusexecute",
                Slot = SpellSlot.R,
                Range = 460,
                Delay = 200,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Darius

            #region Diana

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "diana",
                SpellName = "dianateleport",
                Slot = SpellSlot.R,
                Range = 800,
                Delay = 250,
                Speed = 1500,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Diana

            #region Elise

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "elise",
                SpellName = "elisehumanq",
                Slot = SpellSlot.Q,
                Range = 625,
                Delay = 550,
                Speed = 2200,
                DangerousLevel = DangerousLevel.Low
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "elise",
                SpellName = "elisespiderqcast",
                Slot = SpellSlot.Q,
                Range = 350,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Elise

            #region Fiddlesticks

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "fiddlesticks",
                SpellName = "terrify",
                Slot = SpellSlot.Q,
                Range = 575,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "fiddlesticks",
                SpellName = "drain",
                Slot = SpellSlot.W,
                Range = 575,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Medium
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "fiddlesticks",
                SpellName = "fiddlesticksdarkwind",
                Slot = SpellSlot.E,
                Range = 750,
                Delay = 500,
                Speed = 1100,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Fiddlesticks

            #region Fiora

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "fiora",
                SpellName = "fioraq",
                Slot = SpellSlot.Q,
                Range = 300,
                Delay = 500,
                Speed = 2200,
                DangerousLevel = DangerousLevel.Low
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "fiora",
                SpellName = "fioradance",
                Slot = SpellSlot.R,
                Range = 210,
                Delay = 500,
                Speed = 0,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Fiora

            #region Fizz

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "fizz",
                SpellName = "fizzpiercingstrike",
                Slot = SpellSlot.Q,
                Range = 550,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Fizz

            #region Gangplank

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "gangplank",
                SpellName = "parley",
                Slot = SpellSlot.Q,
                Range = 625,
                Delay = 500,
                Speed = 2000,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Gangplank

            #region Garen

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "garen",
                SpellName = "garenr",
                Slot = SpellSlot.R,
                Range = 400,
                Delay = 120,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Garen

            #region Irelia

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "irelia",
                SpellName = "ireliagatotsu",
                Slot = SpellSlot.Q,
                Range = 650,
                Delay = 150,
                Speed = 2200,
                DangerousLevel = DangerousLevel.Medium
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "irelia",
                SpellName = "ireliaequilibriumstrike",
                Slot = SpellSlot.E,
                Range = 325,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Irelia

            #region Janna

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "janna",
                SpellName = "eyeofthestorm",
                Slot = SpellSlot.E,
                Range = 800,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Janna

            #region Jax

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "jax",
                SpellName = "jaxleapstrike",
                Slot = SpellSlot.Q,
                Range = 210,
                Delay = 500,
                Speed = 0,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Jax

            #region Jayce

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "jayce",
                SpellName = "jaycetotheskies",
                Slot = SpellSlot.Q,
                Range = 600,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "jayce",
                SpellName = "jaycethunderingblow",
                Slot = SpellSlot.E,
                Range = 300,
                Delay = 0,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Jayce

            #region Karma

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "karma",
                SpellName = "karmaspiritbind",
                Slot = SpellSlot.W,
                Range = 700,
                Delay = 500,
                Speed = 2000,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Karma

            #region Kassadin

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "kassadin",
                SpellName = "nulllance",
                Slot = SpellSlot.Q,
                Range = 650,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Kassadin

            #region Katarina

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "katarina",
                SpellName = "katarinaq",
                Slot = SpellSlot.Q,
                Range = 675,
                Delay = 500,
                Speed = 1800,
                DangerousLevel = DangerousLevel.Low
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "katarina",
                SpellName = "katarinae",
                Slot = SpellSlot.E,
                Range = 700,
                Delay = 500,
                Speed = 0,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Katarina

            #region Kayle

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "kayle",
                SpellName = "judicatorreckoning",
                Slot = SpellSlot.Q,
                Range = 650,
                Delay = 500,
                Speed = 1500,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Kayle

            #region Khazix

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "khazix",
                SpellName = "khazixq",
                Slot = SpellSlot.Q,
                Range = 325,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Medium
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "khazix",
                SpellName = "khazixqlong",
                Slot = SpellSlot.Q,
                Range = 375,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Khazix

            #region LeBlanc

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "leblanc",
                SpellName = "leblancchaosorb",
                Slot = SpellSlot.Q,
                Range = 700,
                Delay = 500,
                Speed = 2000,
                DangerousLevel = DangerousLevel.Medium
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "leblanc",
                SpellName = "leblancchaosorbm",
                Slot = SpellSlot.R,
                Range = 700,
                Delay = 500,
                Speed = 2000,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion LeBlanc

            #region LeeSin

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "leesin",
                SpellName = "blindmonkrkick",
                Slot = SpellSlot.R,
                Range = 375,
                Delay = 150,
                Speed = 1500,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion LeeSin

            #region Lissandra

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "lissandra",
                SpellName = "lissandrar",
                Slot = SpellSlot.R,
                Range = 550,
                Delay = 0,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Lissandra

            #region Lulu

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "lulu",
                SpellName = "lulue",
                Slot = SpellSlot.E,
                Range = 650,
                Delay = 640,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Lulu

            #region Malphite

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "malphite",
                SpellName = "seismicshard",
                Slot = SpellSlot.Q,
                Range = 625,
                Delay = 500,
                Speed = 1200,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Malphite

            #region Malzahar

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "malzahar",
                SpellName = "alzaharnethergrasp",
                Slot = SpellSlot.R,
                Range = 700,
                Delay = 0,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Malzahar

            #region Maokai

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "maokai",
                SpellName = "maokaiunstablegrowth",
                Slot = SpellSlot.W,
                Range = 650,
                Delay = 550,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Maokai

            #region MasterYi

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "masteryi",
                SpellName = "alphastrike",
                Slot = SpellSlot.Q,
                Range = 600,
                Delay = 500,
                Speed = 4000,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion MasterYi

            #region MissFortune

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "missfortune",
                SpellName = "missfortunericochetshot",
                Slot = SpellSlot.Q,
                Range = 650,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion MissFortune

            #region Wukong

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "monkeyking",
                SpellName = "monkeykingnimbus",
                Slot = SpellSlot.E,
                Range = 625,
                Delay = 0,
                Speed = 2200,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Wukong

            #region Mordekaiser

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "mordekaiser",
                SpellName = "mordekaiserchildrenofthegrave",
                Slot = SpellSlot.R,
                Range = 850,
                Delay = 300,
                Speed = 1500,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Mordekaiser

            #region Nami

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "nami",
                SpellName = "namiw",
                Slot = SpellSlot.W,
                Range = 750,
                Delay = 500,
                Speed = 1100,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Nami

            #region Nasus

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "nasus",
                SpellName = "nasusw",
                Slot = SpellSlot.W,
                Range = 600,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Nasus

            #region Nautilus

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "nautilus",
                SpellName = "nautilusgandline",
                Slot = SpellSlot.R,
                Range = 1500,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Nautilus

            #region Nocturne

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "nocturne",
                SpellName = "nocturneunspeakablehorror",
                Slot = SpellSlot.E,
                Range = 500,
                Delay = 0,
                Speed = 0,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Nocturne

            #region Nunu

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "nunu",
                SpellName = "iceblast",
                Slot = SpellSlot.E,
                Range = 550,
                Delay = 500,
                Speed = 1000,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Nunu

            #region Olaf

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "olaf",
                SpellName = "olafrecklessstrike",
                Slot = SpellSlot.E,
                Range = 325,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Olaf

            #region Pantheon

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "pantheon",
                SpellName = "pantheonq",
                Slot = SpellSlot.Q,
                Range = 600,
                Delay = 500,
                Speed = 1500,
                DangerousLevel = DangerousLevel.Low
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "pantheon",
                SpellName = "pantheonw",
                Slot = SpellSlot.W,
                Range = 600,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Pantheon

            #region Poppy

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "poppy",
                SpellName = "poppyheroiccharge",
                Slot = SpellSlot.E,
                Range = 525,
                Delay = 500,
                Speed = 1450,
                DangerousLevel = DangerousLevel.High
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "poppy",
                SpellName = "poppydiplomaticimmunity",
                Slot = SpellSlot.R,
                Range = 900,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Poppy

            #region Quinn

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "quinn",
                SpellName = "quinne",
                Slot = SpellSlot.E,
                Range = 700,
                Delay = 500,
                Speed = 775,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Quinn

            #region Rammus

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "rammus",
                SpellName = "puncturingtaunt",
                Slot = SpellSlot.E,
                Range = 325,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Rammus

            #region RekSai

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "reksai",
                SpellName = "reksaie",
                Slot = SpellSlot.E,
                Range = 250,
                Delay = 0,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion RekSai

            #region Ryze

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "ryze",
                SpellName = "runeprison",
                Slot = SpellSlot.W,
                Range = 600,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "ryze",
                SpellName = "spellflux",
                Slot = SpellSlot.E,
                Range = 600,
                Delay = 500,
                Speed = 1000,
                DangerousLevel = DangerousLevel.Medium
            });


            #endregion Ryze

            #region Shaco

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "shaco",
                SpellName = "twoshivpoison",
                Slot = SpellSlot.E,
                Range = 625,
                Delay = 0,
                Speed = 1500,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Shaco

            #region Shen

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "shen",
                SpellName = "shenvorpalstar",
                Slot = SpellSlot.Q,
                Range = 475,
                Delay = 500,
                Speed = 1500,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Shen

            #region Singed

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "singed",
                SpellName = "fling",
                Slot = SpellSlot.E,
                Range = 125,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Singed

            #region Skarner

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "skarner",
                SpellName = "skarnerimpale",
                Slot = SpellSlot.R,
                Range = 350,
                Delay = 0,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Skarner

            #region Swain

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "swain",
                SpellName = "swaindecrepify",
                Slot = SpellSlot.Q,
                Range = 625,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Medium
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "swain",
                SpellName = "swaintorment",
                Slot = SpellSlot.E,
                Range = 625,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Swain

            #region Talon

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "talon",
                SpellName = "taloncutthroat",
                Slot = SpellSlot.E,
                Range = 750,
                Delay = 0,
                Speed = 1200,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Talon

            #region Taric

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "taric",
                SpellName = "dazzle",
                Slot = SpellSlot.E,
                Range = 625,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Taric

            #region Teemo

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "teemo",
                SpellName = "blindingdart",
                Slot = SpellSlot.Q,
                Range = 580,
                Delay = 500,
                Speed = 1500,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Teemo

            #region Tristana

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "tristana",
                SpellName = "tristanae",
                Slot = SpellSlot.E,
                Range = 625,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.Medium
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "tristana",
                SpellName = "tristanar",
                Slot = SpellSlot.R,
                Range = 700,
                Delay = 500,
                Speed = 1600,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Tristana

            #region Urgot

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "urgot",
                SpellName = "urgotswap2",
                Slot = SpellSlot.R,
                Range = 850,
                Delay = 500,
                Speed = 1800,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Urgot

            #region Vayne

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "vayne",
                SpellName = "vaynecondemnmissile",
                Slot = SpellSlot.E,
                Range = 450,
                Delay = 100,
                Speed = 1200,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Vayne

            #region Veigar

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "veigar",
                SpellName = "veigarprimordialburst",
                Slot = SpellSlot.R,
                Range = 650,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Veigar

            #region Vi

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "vi",
                SpellName = "vir",
                Slot = SpellSlot.R,
                Range = 800,
                Delay = 500,
                Speed = 0,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Vi

            #region Viktor

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "viktor",
                SpellName = "viktorpowertransfer",
                Slot = SpellSlot.Q,
                Range = 600,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Viktor

            #region Vladimir

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "vladimir",
                SpellName = "vladimirtransfusion",
                Slot = SpellSlot.Q,
                Range = 600,
                Delay = 500,
                Speed = 1400,
                DangerousLevel = DangerousLevel.Low
            });

            #endregion Vladimir

            #region Volibear

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "volibear",
                SpellName = "volibearw",
                Slot = SpellSlot.W,
                Range = 400,
                Delay = 500,
                Speed = 1450,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Volibear

            #region Warwirck

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "warwick",
                SpellName = "hungeringstrike",
                Slot = SpellSlot.Q,
                Range = 400,
                Delay = 0,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.Low
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "warwick",
                SpellName = "infiniteduress",
                Slot = SpellSlot.R,
                Range = 700,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Warwick

            #region XinZhao

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "xinzhao",
                SpellName = "xenzhaosweep",
                Slot = SpellSlot.E,
                Range = 600,
                Delay = 500,
                Speed = 1750,
                DangerousLevel = DangerousLevel.High
            });

            #endregion XinZhao

            #region Yasuo

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "yasuo",
                SpellName = "yasuodashwrapper",
                Slot = SpellSlot.E,
                Range = 475,
                Delay = 500,
                Speed = 20,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Yasuo

            #region Yorick

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "yorick",
                SpellName = "yorickravenous",
                Slot = SpellSlot.E,
                Range = 550,
                Delay = 500,
                Speed = float.MaxValue,
                DangerousLevel = DangerousLevel.High
            });

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "yorick",
                SpellName = "yorickreviveally",
                Slot = SpellSlot.R,
                Range = 900,
                Delay = 500,
                Speed = 1500,
                DangerousLevel = DangerousLevel.High
            });

            #endregion Yorick

            #region Zed

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "zed",
                SpellName = "zedult",
                Slot = SpellSlot.R,
                Range = 850,
                Delay = 500,
                Speed = 0,
                DangerousLevel = DangerousLevel.VeryHigh
            });

            #endregion Zed

            #region Zilean

            TargetedSpellDB.Add(new TargetedSpellData
            {
                ChampionName = "zilean",
                SpellName = "zileane",
                Slot = SpellSlot.E,
                Range = 700,
                Delay = 500,
                Speed = 1100,
                DangerousLevel = DangerousLevel.Medium
            });

            #endregion Zilean
        }
    }
}
