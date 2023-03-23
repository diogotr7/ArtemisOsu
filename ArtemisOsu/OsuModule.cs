﻿using Artemis.Core;
using Artemis.Core.Modules;
using ArtemisOsu.DataModels;
using System.Collections.Generic;
using Serilog;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;

namespace ArtemisOsu
{
    [PluginFeature(Name = "ArtemisOsu")]
    public class OsuModule : Module<OsuDataModel>
    {
        private readonly ILogger _logger;
        private readonly OsuBaseAddresses _baseAddresses;
        private StructuredOsuMemoryReader? _osuMemoryReader;
        
        public OsuModule(ILogger logger)
        {
            _logger = logger;
            _baseAddresses = new();
        }

        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new()
        {
            new ProcessActivationRequirement("osu!"),
        };
        
        public override void Enable()
        {
        }

        public override void Disable()
        {
        }

        public override void ModuleActivated(bool isOverride)
        {
            _osuMemoryReader = new StructuredOsuMemoryReader();
        }
        
        public override void ModuleDeactivated(bool isOverride)
        {
            _osuMemoryReader?.Dispose();
            _osuMemoryReader = null;
        }

        public override void Update(double deltaTime)
        {
            if (_osuMemoryReader is null)
                return;
            
            _osuMemoryReader.TryRead(_baseAddresses.Beatmap);
            _osuMemoryReader.TryRead(_baseAddresses.Skin);
            _osuMemoryReader.TryRead(_baseAddresses.GeneralData);
            
            if (_baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.SongSelect)
                _osuMemoryReader.TryRead(_baseAddresses.SongSelectionScores);
            else
                _baseAddresses.SongSelectionScores.Scores.Clear();

            if (_baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.ResultsScreen)
                _osuMemoryReader.TryRead(_baseAddresses.ResultsScreen);

            if (_baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing)
            {
                _osuMemoryReader.TryRead(_baseAddresses.Player);
                //TODO: flag needed for single/multi player detection (should be read once per play in singleplayer)
                _osuMemoryReader.TryRead(_baseAddresses.LeaderBoard);
                _osuMemoryReader.TryRead(_baseAddresses.KeyOverlay);
            }
            else
            {
                _baseAddresses.LeaderBoard.Players.Clear();
            }
        }
    }
}