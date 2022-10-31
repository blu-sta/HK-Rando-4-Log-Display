using HK_Rando_4_Log_Display.DTO;
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
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom(bool useDestination, bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea(bool useDestination, bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea(bool useDestination, bool useSceneDescription);
        public List<TransitionWithDestination> GetTransitions();
    }

    public class TransitionSpoilerReader : ITransitionSpoilerReader
    {
        private readonly IResourceLoader _resourceLoader;
        private readonly List<TransitionWithDestination> _spoilerTransitions = new();

        public bool IsFileFound { get; private set; }

        public TransitionSpoilerReader(IResourceLoader resourceLoader)
        {
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

            // TODO: Load safe try-catch
            LoadTransitionSpoiler(transitionSpoilerData);
        }

        public void OpenFile()
        {
            if (File.Exists(TransitionSpoilerLogPath)) Process.Start(new ProcessStartInfo(TransitionSpoilerLogPath) { UseShellExecute = true });
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
                        SceneDescription = Regex.Match(x.Source, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                        SceneName = Regex.Match(x.Source, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                        DoorName = Regex.Match(x.Source, "(\\S+)\\[(\\S+)\\]").Groups[2].Value,
                        MapArea = "> Unrecognised Transitions",
                        TitledArea = "> Unrecognised Transitions",
                    };
                var destinationDetails = _resourceLoader.ReferenceTransitions.FirstOrDefault(y => y.Name == x.Target)
                    ?? new ReferenceTransition
                    {
                        SceneDescription = Regex.Match(x.Target, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
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
                               SceneDescription = sourceDetails.SceneDescription,
                               DoorName = sourceDetails.DoorName,
                               MapArea = sourceDetails.MapArea,
                               TitledArea = sourceDetails.TitledArea,
                           },
                           Destination = new Transition
                           {
                               SceneName = destinationDetails.SceneName,
                               SceneDescription = destinationDetails.SceneDescription,
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

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom(bool useDestination, bool useSceneDescription) =>
            _spoilerTransitions.GroupBy(x => GetTransitionKey(useDestination, useSceneDescription, x))
                .OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea(bool useDestination, bool useSceneDescription) =>
            _spoilerTransitions.GroupBy(x => useDestination ? x.Destination.TitledArea : x.Source.TitledArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => GetTransitionKey(useDestination, useSceneDescription, x))
                .ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea(bool useDestination, bool useSceneDescription) =>
            _spoilerTransitions.GroupBy(x => useDestination ? x.Destination.MapArea : x.Source.MapArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => GetTransitionKey(useDestination, useSceneDescription, x))
                .ToDictionary(x => x.Key, x => x.ToList()));

        private static string GetTransitionKey(bool useDestination, bool useSceneDescription, TransitionWithDestination x)
        {
            var transition = useDestination ? x.Destination : x.Source;
            return useSceneDescription ? transition.SceneDescription : transition.SceneName;
        }

        public List<TransitionWithDestination> GetTransitions() =>
            _spoilerTransitions.ToList();
    }
}
