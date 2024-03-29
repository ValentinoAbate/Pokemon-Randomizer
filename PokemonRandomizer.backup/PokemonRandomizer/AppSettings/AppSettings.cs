﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.AppSettings
{
    using Backend.Randomization;
    using Backend.EnumTypes;
    using UI.Models;
    using PokemonRandomizer.Backend.DataStructures;
    using static PokemonRandomizer.Backend.Randomization.PokemonVariantRandomizer;

    public class AppSettings : HardCodedSettings
    {
        public RomMetadata Metadata { get; set; }
        private TmHmTutorModel tmHmTutorData;
        private VariantPokemonDataModel variantData;
        private PokemonTraitsModel pokemonData;
        private InGameTradesDataModel tradeData;
        private StartersDataModel starterData;
        private GiftPokemonDataModel giftData;
        private DreamTeamDataModel dreamTeamData;
        private WildEncounterDataModel wildEncounterData;
        private Dictionary<TrainerCategory, TrainerDataModel> trainerData;
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
            dreamTeamData = specialPokemonData.DreamTeamData;
            variantData = data.VariantPokemonData;
            tmHmTutorData = data.TmHmTutorData;
            pokemonData = data.PokemonData;
            wildEncounterData = data.WildEncounterData;
            trainerData = data.TrainerDataModels.ToDictionary((tData) => tData.Category);
            itemData = data.ItemData;
            weatherData = data.WeatherData;
            miscData = data.MiscData;
        }

        private static double RandomChance(bool enabled, double chance) => enabled ? chance : 0;

        private static byte DoubleToByte(double d) => (byte)Math.Round(d * byte.MaxValue);

        private static byte DoubleToByteInverse(double d) => (byte)(byte.MaxValue - Math.Round(d * byte.MaxValue));

        #region TMs, HMs, and Move Tutors

        public override MoveCompatOption TmCompatSetting => tmHmTutorData.TmCompatOption;
        public override MoveCompatOption MtCompatSetting => tmHmTutorData.TutorCompatOption;
        public override MoveCompatOption HmCompatSetting => tmHmTutorData.HmCompatOption;
        public override double MoveCompatTrueChance => tmHmTutorData.RandomCompatTrueChance;
        public override double MoveCompatNoise => tmHmTutorData.IntelligentCompatNoise;
        public override bool PreventHmMovesInTMsAndTutors => tmHmTutorData.NoHmMovesInTMsAndTutors;
        public override bool PreventDuplicateTMsAndTutors => tmHmTutorData.NoDuplicateTMsAndTutors;
        public override bool KeepImportantTMsAndTutors => tmHmTutorData.KeepImportantTmsAndTutors;
        // TODO:
        // public override HashSet<Move> ImportantTMsAndTutors => (No UI yet)
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

        #endregion

        #region Catch Rates

        public override CatchRateOption CatchRateSetting => pokemonData.CatchRateSetting;
        public override bool KeepLegendaryCatchRates => pokemonData.KeepLegendaryCatchRates;
        public override byte CatchRateConstant => DoubleToByteInverse(pokemonData.CatchRateConstantDifficulty);

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

        #region Trade Pokemon

        public override double TradePokemonGiveRandChance => RandomChance(tradeData.RandomizeTradeGive, tradeData.TradePokemonGiveRandChance);
        public override double TradePokemonRecievedRandChance => RandomChance(tradeData.RandomizeTradeRecieve, tradeData.TradePokemonRecievedRandChance);
        public override PokemonSettings TradeSpeciesSettingsGive => tradeData.TradeSpeciesSettingsGive;
        public override PokemonSettings TradeSpeciesSettingsReceive => tradeData.TradeSpeciesSettingsRecieve;
        public override double TradeHeldItemRandChance => RandomChance(tradeData.RandomizeHeldItems, tradeData.HeldItemRandChance);
        public override ItemRandomizer.Settings TradeHeldItemSettings => tradeData.TradeHeldItemSettings;

        #endregion

        #region Trainers

        public override TrainerSettings GetTrainerSettings(TrainerCategory trainerClass)
        {
            static TrainerSettings TrainerDataToSettings(TrainerDataModel model)
            {
                return new TrainerSettings()
                {
                    PokemonRandChance = RandomChance(model.RandomizePokemon, model.PokemonRandChance),
                    PokemonStrategy = model.PokemonStrategy,
                    PokemonSettings = model.PokemonSettings,
                    BattleTypeRandChance = RandomChance(model.RandomizeBattleType, model.BattleTypeRandChance),
                    BattleTypeStrategy = model.BattleTypeStrategy,
                    DoubleBattleChance = model.DoubleBattleChance,
                };
            }
            if (trainerData.ContainsKey(trainerClass))
            {
                return TrainerDataToSettings(trainerData[trainerClass]);
            }
            if(trainerData.ContainsKey(TrainerCategory.Trainer))
            {
                return TrainerDataToSettings(trainerData[TrainerCategory.Trainer]);
            }
            return base.GetTrainerSettings(trainerClass);
        }

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
                var dict = new Dictionary<Map.Type, double>(3);
                if (weatherData.RandomizeRouteWeather)
                {
                    dict.Add(Map.Type.Route, weatherData.RouteWeatherRandChance);
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
        public override HailHackOption HailHackSetting
        {
            get
            {
                if (Metadata == null || WeatherSetting == WeatherOption.Unchanged)
                    return HailHackOption.None;
                if (Metadata.IsEmerald)
                    return HailHackOption.Snow;
                if (Metadata.IsFireRed)
                    return HailHackOption.Both;
                return HailHackOption.None;
            }
        }

        #endregion

        #region Items

        public override ItemRandomizer.RandomizerSettings ItemRandomizationSettings => new ItemRandomizer.RandomizerSettings()
        {
            SkipCategories = ItemData.Categories.KeyItem | itemData.SkipCategories,
            BannedCategories = ItemData.Categories.KeyItem | itemData.BannedCategories,
            OccurenceWeightedCategories = itemData.ReduceDuplicatesCategories,
            SameCategoryCategories = itemData.KeepCategoryCategories,
            SameCategoryChance = itemData.SameCategoryChance,
            AllowBannedItemsWhenKeepingCategory = itemData.AllowBannedItemsWhenKeepingCategory,
        };

        public override PcItemOption PcPotionOption => itemData.PcPotionOption;
        public override Item CustomPcItem => itemData.CustomPcItem;
        public override ItemRandomizer.Settings PcItemSettings => itemData.PcItemSettings;

        public override bool AddCustomItemToPokemarts => itemData.AddItemToPokemarts;
        public override Item CustomMartItem => itemData.CustomMartItem;
        public override bool OverrideCustomMartItemPrice => itemData.OverrideCustomMartItemPrice;
        public override int CustomMartItemPrice => (int)itemData.CustomMartItemPrice;

        public override double FieldItemRandChance => RandomChance(itemData.RandomizeFieldItems, itemData.FieldItemRandChance);
        public override ItemRandomizer.Settings FieldItemSettings => itemData.FieldItemSettings;
        public override bool UseSeperateHiddenItemSettings => itemData.UseSeperateHiddenItemSettings;
        public override double HiddenItemRandChance => RandomChance(itemData.RandomizeHiddenItems, itemData.HiddenItemRandChance);
        public override ItemRandomizer.Settings HiddenItemSettings => itemData.HiddenItemSettings;

        public override double PickupItemRandChance => RandomChance(itemData.RandomizePickupItems, itemData.PickupItemRandChance);
        public override ItemRandomizer.Settings PickupItemSettings => itemData.PickupItemSettings;

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

        // FRLG Hacks and Tweaks
        public override bool EvolveWithoutNationalDex => miscData.EvolveWithoutNationalDex;

        // RSE Hacks and Tweaks
        public override bool EasyFirstRivalBattle => miscData.EasyFirstRivalbattle && Metadata.IsRubySapphireOrEmerald;

        public override bool RandomizeWallyAce => miscData.RandomizeWallyAce;

        // Randomizer Settings
        public override bool CountRelicanthAsFossil => miscData.CountRelicanthAsFossil;

        #endregion

    }
}
