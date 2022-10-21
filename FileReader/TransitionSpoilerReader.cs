using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Reference;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ITransitionSpoilerReader : ILogReader
    {
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea(bool useDestination);
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea(bool useDestination);
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom(bool useDestination, bool useAltSceneName);
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea(bool useDestination, bool useAltSceneName);
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea(bool useDestination, bool useAltSceneName);
        public List<TransitionWithDestination> GetTransitions();
    }

    public class TransitionSpoilerReader : ITransitionSpoilerReader
    {
        private readonly SceneNameDictionary _sceneNameDictionary;
        private readonly IResourceLoader _resourceLoader;
        private readonly List<TransitionWithDestination> _spoilerTransitions = new();

        public bool IsFileFound { get; private set; }

        public TransitionSpoilerReader(IResourceLoader resourceLoader, SceneNameDictionary sceneNameDictionary)
        {
            _sceneNameDictionary = sceneNameDictionary;
            _resourceLoader = resourceLoader;
            LoadData();
        }

        public void LoadData()
        {
            IsFileFound = File.Exists(TransitionSpoilerLogPath);
            if (!IsFileFound)
            {
                return;
            }
            var transitionSpoilerData = File.ReadAllLines(TransitionSpoilerLogPath).ToList();
            LoadTransitionSpoiler(transitionSpoilerData);
        }

        public void OpenFile()
        {
            if (File.Exists(TransitionSpoilerLogPath)) Process.Start("notepad.exe", TransitionSpoilerLogPath);
        }

        private void LoadTransitionSpoiler(List<string> transitionSpoilerData)
        {
            _spoilerTransitions.Clear();
            var start = transitionSpoilerData.IndexOf("[");
            if (start < 0)
            {
                return;
            }
            var end = transitionSpoilerData.IndexOf("]", start);
            if (end < 0)
            {
                return;
            }

            var transitionSpoilerString = string.Join("", transitionSpoilerData.Where((_, i) => i >= start && i <= end));

            var spoilerTransitions = JsonConvert.DeserializeObject<List<SpoilerTransition>>(transitionSpoilerString);
            spoilerTransitions.ForEach(x =>
            {
                var sourceDetails = _resourceLoader.ReferenceTransitions.FirstOrDefault(y => y.Name == x.Source)
                    ?? new ReferenceTransition
                    {
                        SceneName = Regex.Match(x.Source, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                        DoorName = Regex.Match(x.Source, "(\\S+)\\[(\\S+)\\]").Groups[2].Value,
                        MapArea = "> Unrecognised Transitions",
                        TitledArea = "> Unrecognised Transitions",
                    };
                var destinationDetails = _resourceLoader.ReferenceTransitions.FirstOrDefault(y => y.Name == x.Target)
                    ?? new ReferenceTransition
                    {
                        SceneName = Regex.Match(x.Target, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                        DoorName = Regex.Match(x.Target, "(\\S+)\\[(\\S+)\\]").Groups[2].Value,
                        MapArea = "> Unrecognised Transitions",
                        TitledArea = "> Unrecognised Transitions",
                    };

                _spoilerTransitions.Add(
                       new TransitionWithDestination
                       {
                           Source = new Transition
                           {
                               SceneName = sourceDetails.SceneName,
                               DoorName = sourceDetails.DoorName,
                               MapArea = sourceDetails.MapArea,
                               TitledArea = sourceDetails.TitledArea,
                           },
                           Destination = new Transition
                           {
                               SceneName = destinationDetails.SceneName,
                               DoorName = destinationDetails.DoorName,
                               MapArea = destinationDetails.MapArea,
                               TitledArea = destinationDetails.TitledArea,
                           }
                       });
            });
        }

        private class SpoilerTransition
        {
            public string Source { get; set; }
            public string Target { get; set; }
        }

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea(bool useDestination) =>
            _spoilerTransitions.GroupBy(x => useDestination ? x.Destination.TitledArea : x.Source.TitledArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea(bool useDestination) =>
            _spoilerTransitions.GroupBy(x => useDestination ? x.Destination.MapArea : x.Source.MapArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom(bool useDestination, bool useAltSceneName) =>
            _spoilerTransitions.GroupBy(x => useDestination ? x.Destination.SceneName : x.Source.SceneName)
                .OrderBy(x => useAltSceneName ? _sceneNameDictionary.GetAltSceneName(x.Key) : x.Key)
                .ToDictionary(x => useAltSceneName ? _sceneNameDictionary.GetAltSceneName(x.Key) : x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea(bool useDestination, bool useAltSceneName) =>
            _spoilerTransitions.GroupBy(x => useDestination ? x.Destination.TitledArea : x.Source.TitledArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => useDestination ? x.Destination.SceneName : x.Source.SceneName)
                .ToDictionary(x => useAltSceneName ? _sceneNameDictionary.GetAltSceneName(x.Key) : x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea(bool useDestination, bool useAltSceneName) =>
            _spoilerTransitions.GroupBy(x => useDestination ? x.Destination.MapArea : x.Source.MapArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => useDestination ? x.Destination.SceneName : x.Source.SceneName)
                .ToDictionary(x => useAltSceneName ? _sceneNameDictionary.GetAltSceneName(x.Key) : x.Key, x => x.ToList()));

        public List<TransitionWithDestination> GetTransitions() =>
            _spoilerTransitions.ToList();
    }
}
