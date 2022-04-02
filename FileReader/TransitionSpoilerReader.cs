using HK_Rando_4_Log_Display.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ITransitionSpoilerReader : ILogReader
    {
        public List<TransitionWithDestination> GetTransitions();
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea();

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea();

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom();

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea();

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea();
    }

    public class TransitionSpoilerReader : ITransitionSpoilerReader
    {
        private readonly IResourceLoader _resourceLoader;

        public bool IsFileFound { get; private set; }

        public TransitionSpoilerReader(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public void LoadData()
        {
            var filepath = Constants.TransitionSpoilerLogPath;
            if (!File.Exists(filepath))
            {
                IsFileFound = false;
                return;
            }

            IsFileFound = true;
            var transitionSpoilerData = File.ReadAllLines(filepath).ToList();

            LoadTransitionSpoiler(transitionSpoilerData);
        }

        private List<TransitionWithDestination> _spoilerTransitions = new List<TransitionWithDestination>();

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
                var sourceMatches = Regex.Match(x.Source, "(.*)\\[(.*)\\]");
                var sceneName = sourceMatches.Groups[1].Value;
                var doorName = sourceMatches.Groups[2].Value;
                var targetMatches = Regex.Match(x.Target, "(.*)\\[(.*)\\]");
                var destinationSceneName = targetMatches.Groups[1].Value;
                var destinationDoorName = targetMatches.Groups[2].Value;
                var referenceTransition = _resourceLoader.Transitions.FirstOrDefault(y => y.SceneName == sceneName && y.DoorName == doorName) ?? new Transition { SceneName = sceneName, DoorName = doorName, MapArea = "undefined", TitledArea = "undefined" };

                var transitionWithDestination = new TransitionWithDestination(referenceTransition, destinationSceneName, destinationDoorName);
                _spoilerTransitions.Add(transitionWithDestination);
            });
        }

        private class SpoilerTransition
        {
            public string Source { get; set; }
            public string Target { get; set; }
        }

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea() =>
            _spoilerTransitions.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea() =>
            _spoilerTransitions.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom() =>
            _spoilerTransitions.GroupBy(x => x.SceneName).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea() =>
            _spoilerTransitions.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea() =>
            _spoilerTransitions.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public List<TransitionWithDestination> GetTransitions() 
            => _spoilerTransitions;
    }
}
