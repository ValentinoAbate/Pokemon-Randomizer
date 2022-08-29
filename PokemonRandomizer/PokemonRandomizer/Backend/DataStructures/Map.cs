namespace PokemonRandomizer.Backend.DataStructures
{
    public class Map
    {
        public enum Type : byte
        {
            Village = 1,
            City,
            Route,
            Underground,
            Underwater,
            /// 0x06 - 0x07: Unknown/unused,
            Inside = 8,
            SecretBase,
            /// 0x0A - 0xFF: Unknown/unused
        }

        public enum Weather : byte
        {
            House,
            ClearWithCloudsInWater,
            Clear,
            Rain,
            Snow,
            RainThunderstorm,
            MistSteady,
            FallingAsh,
            Sandstorm,
            MistFromTopRight,
            MistDenseBright,
            Cloudy,
            StrongSunlight,
            RainHeavyThunderstrorm,
            UnderwaterMist,
            Chaos,                    // Groudon / Kyogre weather cycle (Emerald Only)
            RainSometimes1 = 20,      // Used in Route 119, clear 1/4 days, rain 2/4 days, and thunder 1/4 days
            RainSometimes2 = 21       // Used in Hoenn Route 123
        }

        public static bool WeatherAffectsBattle(Weather w, Settings.HailHackOption hailHack)
        {
            if(w is Weather.FallingAsh)
            {
                return hailHack.HasFlag(Settings.HailHackOption.FallingAsh);
            }
            else if(w is Weather.Snow)
            {
                return hailHack.HasFlag(Settings.HailHackOption.Snow);
            }
            return w is Weather.Rain or Weather.RainThunderstorm or Weather.RainHeavyThunderstrorm or Weather.Sandstorm
                or Weather.StrongSunlight or Weather.Chaos or Weather.RainSometimes1 or Weather.RainSometimes2;
        }


        // Header data!

        //Example from https://datacrystal.romhacking.net/wiki/Pok%C3%A9mon_3rd_Generation
        //      Data Type      |           Content          |   Example (from fire red)   |
        //---------------------|----------------------------|-----------------------------|
        //Pointer               Map data                     0x082DD4C0
        //Pointer               Event data                   0x083B4E50
        //Pointer               Map scripts                  0x0816545A
        //Pointer               Connections                  0x0835276C
        //Little endian Short   Music index                  0x012C (Pallet Town)
        //Little endian Short   Map index                    0x004E
        //Byte                  Label index                  0x58 ("Pallet Town")
        //Byte                  Visibility(i.e HM Flash)     0x00
        //Byte                  Weather                      0x02
        //Byte                  Map type(City? Village? etc) 0x01
        //Little endian Short   ???                          0x0601
        //Byte                  Show label on entry          0
        //Byte                  In-battle field model id     0

        // Data structures have been figured out by ShinyQuagsire and are available at
        // https://github.com/shinyquagsire23/MEH/tree/master/src/us/plxhack/MEH (MEH source)
        public int mapDataOffset;
        public int eventDataOffset;
        public int mapScriptsOffset;
        public int connectionOffset;
        public int music;
        /// <summary>
        /// The map's index (starts at 1). Seems to be meainingless.
        /// </summary>
        public int mapIndex;
        /// <summary>
        /// Index of this map's label / name in the table
        /// </summary> 
        public byte labelIndex;
        /// <summary>
        /// Vibility/Flash usage state
        /// In Emerald (info from advance-map: http://ampage.no-ip.info/):
        /// 0x00: normal visibility,
        /// 0x01: dark, flash usable,
        /// 0x02: dark, flash unusable,
        /// 0x03 - 0xFF: unknown/unused
        /// </summary>
        public byte visibility;
        /// <summary>
        /// Weather type. Affects in-battle weather
        /// In Emerald (info from advance-map: http://ampage.no-ip.info/): 
        /// 0x00: In-house weather,
        /// 0x01: Sunny weather with clouds in water,
        /// 0x02: Regular weather,
        /// 0x03: Rainy weather (Rain in battle),
        /// 0x04: Three snow flakes (Hail in battle w/ hail hack),
        /// 0x05: Rain with thunder storm (Rain in battle),
        /// 0x06: Steady mist,
        /// 0x07: Steady snow (Hail in battle w/ hail hack),
        /// 0x08: Sand storm (Sandstorm in battle),
        /// 0x09: Mist from top right corner,
        /// 0x0A: Dense bright mist,
        /// 0x0B: Cloudy,
        /// 0x0C: Underground flashes (Strong sunlight in battle),
        /// 0x0D: Heavy rain with thunderstorm (Rain in battle),
        /// 0x0E: Underwater mist,
        /// 0x0F - 0xFF: Unknown/unused,
        /// </summary>
        public Weather weather;
        /// <summary>
        /// I'm not sure what this value actually changes. Maybe label style?
        /// In Emerald (info from advance-map: http://ampage.no-ip.info/): 
        /// 0x00: Unknown/unused,
        /// 0x01: Village,
        /// 0x02: City,
        /// 0x03: Route,
        /// 0x04: Underground,
        /// 0x05: Underwater,
        /// 0x06 - 0x07: Unknown/unused,
        /// 0x08: Inside,
        /// 0x09: Secret base,
        /// 0x0A - 0xFF: Unknown/unused
        /// </summary>
        public Type mapType;
        public byte unknown;
        public byte unknown2;
        /// <summary>
        /// I'm not sure what the difference between "show name" and "show village/city names is"
        /// In Emerald (info from advance-map: http://ampage.no-ip.info/): 
        /// 0x00: Do not show name,
        /// 0x01: Show name,
        /// 0x02 - 0x05: Unknown/unused,
        /// 0x06: Show village names,
        /// 0x07 - 0x0C: Unknown/unused,
        /// 0x0D: Show city names,
        /// 0x0E - 0xFF: Unknown/unused
        /// </summary>
        public byte showLabelOnEntry;
        /// <summary>
        /// I suspect this value is actually the battle transition type but I'm not sure
        /// In Emerald (info from advance-map: http://ampage.no-ip.info/): 
        /// 0x00: Random encounter,
        /// 0x01: Gym battle,
        /// 0x02: Rocket,
        /// 0x03: Unknown/unused
        /// 0x04: 1. Top 4
        /// 0x05: 2. Top 4
        /// 0x06: 3. Top 4
        /// 0x07: 4. Top 4
        /// 0x08: Big Red Pokeball
        /// </summary>
        public byte battleField;

        // Non-Header Data
        public MapData mapData;
        public MapEventData eventData;
        public MapScriptData scriptData;
        public ConnectionData connections;

        public string Name { get; set; }

        public bool IsOutdoors => mapType is Type.Route or Type.City or Type.Village;

        public bool IsGym => battleField == 0x01;

        public bool HasClearWeather => IsWeatherClear(weather);

        public bool IsNonHeaderWeatherSeparateRoute { get; set; } = false;

        public static bool IsWeatherClear(Weather weather) => weather is Weather.Clear or Weather.Cloudy or Weather.House;

        public void SetOriginalValues()
        {

        }

        public override string ToString()
        {
            return Name ?? "Error: no name";
        }
    }
}
