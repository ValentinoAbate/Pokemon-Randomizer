using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.AppSettings
{
    using Backend.Randomization;
    using Backend.EnumTypes;
    using UI.Models;
    using PokemonRandomizer.Backend.DataStructures;
    using static PokemonRandomizer.Backend.Randomization.PokemonVariantRandomizer;
    using PokemonRandomizer.Backend.Utilities;
    using PokemonRandomizer.Backend.DataStructures.Trainers;

    public class AppSettings : HardCodedSettings
    {
        public RomMetadata Metadata { get; set; }
        private TmHmTutorModel tmHmTutorData;
        private VariantPokemonDataModel variantData;
        private PokemonTraitsModel pokemonData;
        private InGameTradesDataModel tradeData;
        private StartersDataModel starterData;
        private GiftPokemonDataModel giftData;
        private StaticPokemonDataModel staticEncounterData;
        private DreamTeamDataModel dreamTeamData;
        private WildEncounterDataModel wildEncounterData;
        private TrainerDataModel trainerData;
        private TrainerOrganizationDataModel trainerOrgData;
        private ItemDataModel itemData;
        private WeatherDataModel weatherData;
        private MiscDataModel miscData;

        public AppSettings(ApplicationDataModel data) : base(data) { }

        public override void UpdateData(ApplicationDataModel data)
        {
            base.UpdateData(data);
            var specialPokemonData = data.SpecialPokemonData;
            starterData = specialPokemonData.StarterData;
            tradeData = specialPokemonData.TradeData;
            giftData = specialPokemonData.GiftData;
            staticEncounterData = specialPokemonData.StaticPokemonData;
            dreamTeamData = specialPokemonData.DreamTeamData;
            variantData = data.VariantPokemonData;
            tmHmTutorData = data.TmHmTutorData;
            pokemonData = data.PokemonData;
            wildEncounterData = data.WildEncounterData;
            trainerData = data.TrainerData;
            trainerOrgData = data.TrainerOrgData;
            itemData = data.ItemData;
            weatherData = data.WeatherData;
            miscData = data.MiscData;
        }

        private static double RandomChance(bool enabled, double chance) => enabled ? chance : 0;

        private static byte DoubleToByte(double d) => (byte)Math.Round(d * byte.MaxValue);

        private static byte DoubleToByteInverse(double d) => (byte)(byte.MaxValue - Math.Round(d * byte.MaxValue));

        #region TMs, HMs, and Move Tutors

        public override MoveCompatOption MoveCompatSetting => tmHmTutorData.MoveCompatOption;
        public override double MoveCompatTrueChance => tmHmTutorData.RandomCompatTrueChance;
        public override double IntelligentCompatNormalTrueChance => tmHmTutorData.IntelligentCompatNormalRandChance;
        public override double IntelligentCompatTrueChance => tmHmTutorData.IntelligentCompatNoise;
        public override bool ForceFullHmCompatibility => tmHmTutorData.ForceFullHmCompat;
        public override bool PreventHmMovesInTMsAndTutors => tmHmTutorData.NoHmMovesInTMsAndTutors;
        public override bool PreventDuplicateTMsAndTutors => tmHmTutorData.NoDuplicateTMsAndTutors;
        public override bool KeepImportantTMsAndTutors => tmHmTutorData.KeepImportantTmsAndTutors;
        private static readonly HashSet<Move> rseImportantTMTutorMoves = new()
        {
            Move.SECRET_POWER, // Needed for secret bases
            Move.DIG, // Needed to unlock the regis at buried relic
        };
        private static readonly HashSet<Move> genIVImportantTMTutorMoves = new()
        {
            Move.HEADBUTT, // Needed for headbutting trees in HGSS
            Move.FLASH, // Not an HM in this gen
        };
        private static readonly HashSet<Move> genIIImportantTMTutorMoves = new()
        {
            Move.HEADBUTT, // Needed for headbutting trees
            Move.ROCK_SMASH, // Not an HM in this gen
        };
        public override HashSet<Move> ImportantTMsAndTutors
        {
            get
            {
                if(Metadata == null)
                {
                    return new HashSet<Move>();
                }
                if (Metadata.IsRubySapphireOrEmerald)
                {
                    return rseImportantTMTutorMoves;
                }
                if (Metadata.Gen == Generation.IV)
                {
                    return genIVImportantTMTutorMoves;
                }
                if (Metadata.Gen == Generation.II)
                {
                    return genIIImportantTMTutorMoves;
                }
                return new HashSet<Move>();

            }
        }
        public override double TMRandChance => RandomChance(tmHmTutorData.RandomizeTMs, tmHmTutorData.TMRandChance);
        public override double MoveTutorRandChance => RandomChance(tmHmTutorData.RandomizeMoveTutors, tmHmTutorData.MoveTutorRandChance);

        #endregion

        #region Pokemon Base Stats

        #region Variants

        public static WeightedSet<TypeTransformation> SingleTypeVariantTransformationDefaultWeights { get; } = new WeightedSet<TypeTransformation>()
        {
            {TypeTransformation.GainSecondaryType, 0.40f},
            {TypeTransformation.PrimaryTypeReplacement, 0.55f},
            {TypeTransformation.DoubleTypeReplacement, 0.05f},
        };

        private static WeightedSet<TypeTransformation> SingleTypeVariantTransformationEvenWeights { get; } = new WeightedSet<TypeTransformation>()
        {
            {TypeTransformation.GainSecondaryType, 1f},
            {TypeTransformation.PrimaryTypeReplacement, 1f},
            {TypeTransformation.DoubleTypeReplacement, 1f},
        };

        public static  WeightedSet<TypeTransformation> DualTypeVariantTransformationDefaultWeights { get; } = new WeightedSet<TypeTransformation>()
        {
            {TypeTransformation.PrimaryTypeReplacement, 0.35f},
            {TypeTransformation.SecondaryTypeReplacement, 0.5f},
            {TypeTransformation.DoubleTypeReplacement, 0.075f},
            {TypeTransformation.TypeLoss, 0.025f},
        };

        public static WeightedSet<TypeTransformation> DualTypeVariantTransformationEvenWeights { get; } = new WeightedSet<TypeTransformation>()
        {
            {TypeTransformation.PrimaryTypeReplacement, 1},
            {TypeTransformation.SecondaryTypeReplacement, 1},
            {TypeTransformation.DoubleTypeReplacement, 1},
            {TypeTransformation.TypeLoss, 1},
        };

        public override Settings VariantSettings => new PokemonVariantRandomizer.Settings()
        {
            SingleTypeTransformationWeights = variantData.TypeTransformationOption.Value switch
            {
                VariantPokemonDataModel.TypeTransformationWeightOption.Random => SingleTypeVariantTransformationEvenWeights,
                VariantPokemonDataModel.TypeTransformationWeightOption.Balanced => SingleTypeVariantTransformationDefaultWeights,
                VariantPokemonDataModel.TypeTransformationWeightOption.Custom => variantData.SingleTypeTransformationWeights,
                _ => throw new NotImplementedException(),
            },
            DualTypeTransformationWeights = variantData.TypeTransformationOption.Value switch
            {
                VariantPokemonDataModel.TypeTransformationWeightOption.Random => DualTypeVariantTransformationEvenWeights,
                VariantPokemonDataModel.TypeTransformationWeightOption.Balanced => DualTypeVariantTransformationDefaultWeights,
                VariantPokemonDataModel.TypeTransformationWeightOption.Custom => variantData.DualTypeTransformationWeights,
                _ => throw new NotImplementedException(),
            },
            InvertChanceOfSecondaryTypeChangingForFlyingTypes = true,
            AdjustAttackStats = variantData.AdjustAttackStats,
            GiveBonusStats = variantData.GiveBonusStats,
            StatChangeMean = variantData.BonusStatAmountMean,
            StatChangeStdDev = variantData.BonusStatAmountStdDev,
        };

        public override double VariantChance => RandomChance(variantData.CreateVariants, variantData.VariantChance);

        #endregion

        #region Evolution

        public override bool FixImpossibleEvos => pokemonData.FixImpossibleEvos;
        public override bool ConsiderEvolveByBeautyImpossible => pokemonData.ConsiderEvolveByBeautyImpossible;
        public override double ImpossibleEvoLevelStandardDev => pokemonData.ImpossibleEvoLevelStandardDev;
        public override TradeItemPokemonOption TradeItemEvoSetting => pokemonData.TradeItemEvoSetting;
        public override double DunsparsePlaugeChance => RandomChance(pokemonData.DunsparsePlague, pokemonData.DunsparsePlaugeChance);
        public override bool ApplyDunsparsePlagueToFriendshipEvos => pokemonData.DunsparsePlagueFriendship;

        #endregion

        #region Catch / Hatch Rates

        public override CatchRateOption CatchRateSetting => pokemonData.CatchRateSetting;
        public override bool KeepLegendaryCatchRates => pokemonData.KeepLegendaryCatchRates;
        public override byte CatchRateConstant => DoubleToByteInverse(pokemonData.CatchRateConstantDifficulty);
        public override bool FastHatching => pokemonData.FastHatching;

        #endregion

        #region Exp / EV Yields

        public override double BaseExpYieldMultiplier => pokemonData.BaseExpYieldMultiplier;
        public override bool ZeroBaseEVs => pokemonData.ZeroBaseEVs;

        #endregion

        #region Learnsets

        public override bool BanSelfdestruct => pokemonData.BanSelfdestruct;
        public override bool AddMoves => pokemonData.AddMoves;    
        public override bool DisableAddingHmMoves => pokemonData.DisableAddingHmMoves;
        public override double AddMovesChance => pokemonData.AddMovesChance;
        public override double NumMovesStdDeviation => pokemonData.NumMovesStdDeviation;
        public override double NumMovesMean => pokemonData.NumMovesMean;
        public override WeightedSet<AddMoveSource> AddMoveSourceWeights => pokemonData.AddMoveSourceWeights;

        #endregion

        #endregion

        #region Starter Pokemon

        public override StarterPokemonOption StarterSetting => starterData.StarterSetting;
        public override Pokemon[] CustomStarters => starterData.CustomStarters;
        public override bool StrongStarterTypeTriangle => starterData.StrongStarterTypeTriangle;
        public override PokemonSettings StarterPokemonSettings => new PokemonSettings()
        {
            BanLegendaries = starterData.BanLegendaries,
        };
        // TODO:
        // public override PokemonMetric[] StarterMetricData { get; } = new PokemonMetric[0]; (No UI for metric data yet) 
        public override bool SafeStarterMovesets => starterData.SafeStarterMovesets;

        #endregion

        #region Gift Pokemon
        public override double GiftPokemonRandChance => RandomChance(giftData.RandomizeGiftPokemon, giftData.GiftPokemonRandChance);
        public override PokemonSettings GiftSpeciesSettings => giftData.GiftSpeciesSettings;
        public override bool EnsureFossilRevivesAreFossilPokemon => giftData.EnsureFossilRevivesAreFossilPokemon;
        public override bool EnsureGiftEggsAreBabyPokemon => giftData.EnsureGiftEggsAreBabyPokemon;
        #endregion

        #region Static Encounters

        public override double StaticEncounterRandChance => RandomChance(staticEncounterData.RandomizeStatics, staticEncounterData.StaticRandChance);
        public override PokemonSettings StaticEncounterSettings => staticEncounterData.Settings;
        public override LegendaryRandSetting StaticLegendaryRandomizationStrategy => staticEncounterData.LegendarySetting;
        public override bool PreventDuplicateStaticEncounters => staticEncounterData.PreventDupes;
        public override bool RemapStaticEncounters => staticEncounterData.Remap;

        #endregion

        #region Trade Pokemon

        public override double TradePokemonGiveRandChance => RandomChance(tradeData.RandomizeTradeGive, tradeData.TradePokemonGiveRandChance);
        public override double TradePokemonRecievedRandChance => RandomChance(tradeData.RandomizeTradeRecieve, tradeData.TradePokemonRecievedRandChance);
        public override PokemonSettings TradeSpeciesSettingsGive => new PokemonSettings()
        {
            RestrictIllegalEvolutions = false,
            ForceHighestLegalEvolution = false,
            BanLegendaries = tradeData.BanLegendariesGive,
            Data = tradeData.TryMatchPowerGive ? new List<MetricData>() { new MetricData(PokemonMetric.powerIndividual) } : new List<MetricData>(),
        };
        public override PokemonSettings TradeSpeciesSettingsReceive => new PokemonSettings()
        {
            RestrictIllegalEvolutions = false,
            ForceHighestLegalEvolution = false,
            BanLegendaries = tradeData.BanLegendariesRecieve,
            Data = tradeData.TryMatchPowerRecieve ? new List<MetricData>() { new MetricData(PokemonMetric.powerIndividual) } : new List<MetricData>(),
        };
        public override TradePokemonIVSetting TradePokemonIVOption => tradeData.IVSetting;
        public override double TradeHeldItemRandChance => RandomChance(tradeData.RandomizeHeldItems, tradeData.HeldItemRandChance);
        public override ItemRandomizer.Settings TradeHeldItemSettings => tradeData.TradeHeldItemSettings;

        #endregion

        #region Trainers

        // Trainer Pokemon Settings
        public override bool RandomizeTrainerPokemon => trainerData.RandomizePokemon;
        public override bool TrainerTypeTheming => trainerData.TypeTheming;
        protected override TrainerSettings.TrainerTypeDataSource TrainerTypeDataSource => trainerData.TypeDataSource;
        protected override bool BanLegendariesTrainer => trainerData.BanLegendaries;
        protected override bool BanLegendariesMiniboss => trainerData.BanLegendariesMiniboss;
        protected override bool BanLegendariesBoss => trainerData.BanLegendariesBoss;
        public override bool TrainerRestrictIllegalEvolutions => trainerData.RestrictIllegalEvolutions;
        public override bool TrainerForceHighestLegalEvolution => trainerData.ForceHighestLegalEvolution;
        public override double TrainerPokemonNoise => trainerData.PokemonNoise;
        public override double TrainerPokemonDuplicateReductionMultiplier
        {
            get => trainerData.DuplicateReduction.Value switch
            {
                TrainerDataModel.DuplicateReductionOption.Weak => 0.75,
                TrainerDataModel.DuplicateReductionOption.Moderate => 0.5,
                TrainerDataModel.DuplicateReductionOption.Strong => 0.25,
                TrainerDataModel.DuplicateReductionOption.Strict => 0,
                _ => 1,
            };
        }
        public override TrainerSettings.PokemonPcgStrategy RecurringTrainerPokemonStrategy => trainerData.PokemonStrategy;
        protected override bool TrainerForceCustomMovesets => trainerData.ForceCustomMoves;
        protected override int BonusPokemon => (int)(trainerData.NumBonusPokemon.Value);
        protected override bool ApplyBonusPokemonTrainer => trainerData.BonusPokemon;
        protected override bool ApplyBonusPokemonMiniboss => trainerData.BonusPokemonMiniboss;
        protected override bool ApplyBonusPokemonBoss => trainerData.BonusPokemonBoss;

        // Battle Type Settings
        public override bool RandomizeTrainerBattleType => trainerData.RandomizeBattleType;
        public override TrainerSettings.BattleTypePcgStrategy RecurringTrainerBattleTypeStrategy => trainerData.BattleTypeStrategy;
        public override double DoubleBattleChance => trainerData.DoubleBattleChance;

        // Difficulty Settings
        public override double TrainerPokemonLevelMultiplier => trainerData.LevelMult;
        public override double TrainerPokemonMinIV => trainerData.MinIVs;
        public override bool UseSmartAI => trainerData.SmartAI;

        // Trainer Organization Settings
        public override TrainerOrgTypeTheme GymTypeTheming => trainerOrgData.GymTypeTheming;

        public override TrainerOrgTypeTheme EliteFourTheming => trainerOrgData.EliteFourTheming;
        public override TrainerOrgTypeTheme ChampionTheming => trainerOrgData.ChampionTheming;
        public override GymEliteFourPreventDupesSetting GymEliteFourDupePrevention => trainerOrgData.GymAndEliteDupePrevention;
        public override TrainerOrgTypeTheme TeamTypeTheming => trainerOrgData.TeamTypeTheming;
        public override bool GruntTheming => trainerOrgData.GruntTheming;
        public override double TeamDualTypeChance => trainerOrgData.TeamDualTypeChance;
        public override Trainer.Category PriorityThemeCategory => trainerOrgData.PriorityCategory;
        public override TrainerOrgTypeTheme SmallOrgTypeTheming => trainerOrgData.SmallOrgTypeTheming;

        #endregion

        #region Wild Pokemon

        public override WildEncounterRandomizer.Strategy EncounterStrategy => wildEncounterData.Strategy;
        public override PokemonSettings EncounterSettings => wildEncounterData.PokemonSettings;

        #endregion

        #region Maps

        public override WeatherOption WeatherSetting => weatherData.WeatherSetting;
        protected override WeightedSet<Map.Weather> CustomWeatherWeights => weatherData.CustomWeatherWeights;
        public override bool OverrideAllowGymWeather => weatherData.RandomizeGymWeather;
        public override double GymWeatherRandChance => weatherData.GymWeatherRandChance;
        public override Dictionary<Map.Type, double> WeatherRandChance
        {
            get
            {
                var dict = new Dictionary<Map.Type, double>(4);
                if (weatherData.RandomizeRouteWeather)
                {
                    dict.Add(Map.Type.Route, weatherData.RouteWeatherRandChance);
                    dict.Add(Map.Type.OceanRoute, weatherData.RouteWeatherRandChance);
                }
                if (weatherData.RandomizeTownWeather)
                {
                    dict.Add(Map.Type.Village, weatherData.TownWeatherRandChance);
                    dict.Add(Map.Type.City, weatherData.TownWeatherRandChance);
                }
                return dict;
            }
        }
        public override bool OnlyChangeClearWeather => weatherData.KeepExistingWeather;
        public override bool BanFlashingWeather => weatherData.BanFlashing;
        public override HailHackOption HailHackSetting
        {
            get
            {
                if (Metadata == null || WeatherSetting == WeatherOption.Unchanged)
                    return HailHackOption.None;
                if (Metadata.IsEmerald || Metadata.IsRubyOrSapphire)
                    return HailHackOption.Snow;
                if (Metadata.IsFireRedOrLeafGreen)
                    return HailHackOption.Both;
                return HailHackOption.None;
            }
        }

        public override WeightedSet<Map.Weather> WeatherWeights
        {
            get
            {
                var weights = base.WeatherWeights;
                if (Metadata.IsFireRedOrLeafGreen)
                {
                    weights.RemoveIfContains(Map.Weather.RainSometimes1);
                    weights.RemoveIfContains(Map.Weather.RainSometimes2);
                }
                if (!Metadata.IsEmerald)
                {
                    weights.RemoveIfContains(Map.Weather.Chaos);
                }
                return weights;
            }
        }

        #endregion

        #region Items

        public override ItemRandomizer.RandomizerSettings ItemRandomizationSettings => new()
        {
            SkipCategories = ItemData.Categories.KeyItem | itemData.SkipCategories,
            BannedCategories = ItemData.Categories.KeyItem | itemData.BannedCategories,
            OccurenceWeightedCategories = itemData.ReduceDuplicatesCategories,
            OccurenceWeightPower = itemData.DupeReductionStrength.Value switch
            {
                ItemDataModel.DuplicateReductionOption.Weak => 1.5f,
                ItemDataModel.DuplicateReductionOption.Moderate => 3,
                ItemDataModel.DuplicateReductionOption.Strong => 10,
                _ => 10,
            },
            SameCategoryCategories = itemData.KeepCategoryCategories,
            SameCategoryChance = itemData.SameCategoryChance,
            AllowBannedItemsWhenKeepingCategory = itemData.AllowBannedItemsWhenKeepingCategory,
        };

        public override PcItemOption PcPotionOption => itemData.PcPotionOption;
        public override Item CustomPcItem => itemData.CustomPcItem;
        public override ItemRandomizer.Settings PcItemSettings => itemData.PcItemSettings;

        public override bool AddCustomItemToPokemarts => itemData.AddItemToPokemarts;
        public override Item CustomMartItem
        {
            get
            {
                // TMs and HMs cannot be added to shops with other items in FRLG
                if (Metadata.IsFireRedOrLeafGreen && ItemUtils.IsTM(itemData.CustomMartItem) || ItemUtils.IsHM(itemData.CustomMartItem))
                    return Item.None;
                return itemData.CustomMartItem;
            }
        }
        public override bool OverrideCustomMartItemPrice => itemData.OverrideCustomMartItemPrice;
        public override int CustomMartItemPrice => (int)itemData.CustomMartItemPrice;

        public override double FieldItemRandChance => RandomChance(itemData.RandomizeFieldItems, itemData.FieldItemRandChance);
        public override ItemRandomizer.Settings FieldItemSettings => itemData.FieldItemSettings;
        public override bool UseSeperateHiddenItemSettings => itemData.UseSeperateHiddenItemSettings;
        public override double HiddenItemRandChance => RandomChance(itemData.RandomizeHiddenItems, itemData.HiddenItemRandChance);
        public override ItemRandomizer.Settings HiddenItemSettings => itemData.HiddenItemSettings;

        public override double PickupItemRandChance => RandomChance(itemData.RandomizePickupItems, itemData.PickupItemRandChance);
        public override ItemRandomizer.Settings PickupItemSettings => itemData.PickupItemSettings;

        public override double BerryTreeRandChance => RandomChance(miscData.RandomizeBerryTrees, miscData.BerryTreeRandomizationChance);
        public override bool BanMinigameBerries => miscData.BanMinigameBerries;
        public override bool BanEvBerries => miscData.BanEvBerries;
        public override bool RemapBerries => miscData.RemapBerries;

        #endregion

        #region Dream Team

        public override DreamTeamSetting DreamTeamOption => dreamTeamData.DreamTeamOption;

        public override Pokemon[] CustomDreamTeam => dreamTeamData.CustomDreamTeam;

        public override DreamTeamSettings DreamTeamOptions => new DreamTeamSettings()
        {
            PrioritizeVariants = dreamTeamData.PrioritizeVariants,
            BanLegendaries = dreamTeamData.BanLegendaries,
            BanIllegalEvolutions = dreamTeamData.BanIllegalEvolutions,
            BstSetting = dreamTeamData.UseTotalBST,
            BstLimit = dreamTeamData.UseTotalBST.Value switch
            {
                DreamTeamBstTotalOption.TotalMax => (float)dreamTeamData.BstTotalUpperBound,
                DreamTeamBstTotalOption.TotalMin => (float)dreamTeamData.BstTotalLowerBound,
                DreamTeamBstTotalOption.IndividualMax => (float)dreamTeamData.BstIndividualUpperBound,
                DreamTeamBstTotalOption.IndividualMin => (float)dreamTeamData.BstIndividualLowerBound,
                _ => 0
            },
            UseTypeFilter = dreamTeamData.UseTypeFilter,
            TypeFilter = (new List<PokemonType>() { dreamTeamData.AllowedType1, dreamTeamData.AllowedType2, dreamTeamData.AllowedType3 }).Where(t => t != (PokemonType)19).ToArray()
        };

        #endregion

        #region Misc

        // Gen II-IV Hacks and Tweaks
        public override bool UpdateDOTMoves => miscData.UpdateDOTMoves;

        // Gen III Hacks and Tweaks
        public override bool RunIndoors => miscData.RunIndoors;
        public override bool EnableMysteyGiftEvents => miscData.EnableEvents;
        public override MysteryGiftItemSetting MysteryGiftItemAcquisitionSetting => EnableMysteyGiftEvents ? miscData.EventItemSetting : MysteryGiftItemSetting.None;

        // FRLG Hacks and Tweaks
        public override bool EvolveWithoutNationalDex => miscData.EvolveWithoutNationalDex;

        // RSE Hacks and Tweaks
        public override bool EasyFirstRivalBattle => miscData.EasyFirstRivalbattle && Metadata.IsRubySapphireOrEmerald;

        public override bool RandomizeWallyAce => miscData.RandomizeWallyAce;

        // FRLG + E Hacks and Tweaks
        public override bool DeoxysMewObeyFix => miscData.DeoxysMewObeyFix;

        // Randomizer Settings
        public override bool CountRelicanthAsFossil => miscData.CountRelicanthAsFossil;

        public override TypeChartRandomizer.Option TypeChartRandomizationSetting => miscData.TypeChartSetting;

        #endregion

    }
}
