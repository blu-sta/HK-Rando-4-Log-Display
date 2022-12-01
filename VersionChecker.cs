using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display
{
    public interface IVersionChecker
    {
        public Task<string> GetNewVersionOrDefault();
        public Task<string> GetNewBetaVersionOrDefault();
    }
    public class VersionChecker : IVersionChecker
    {
        public async Task<string> GetNewVersionOrDefault()
        {
            try
            {
                var latestVersionString = await GetLatestVersion();
                var latestVersion = GetVersion(latestVersionString);
                var currentVersion = GetVersion(AppVersion);
                return latestVersion.CompareTo(currentVersion) == 1 ? latestVersionString : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<string> GetNewBetaVersionOrDefault()
        {
            try
            {
                var latestVersionString = await GetLatestBetaVersion();
                var latestVersion = GetVersion(latestVersionString);
                var currentVersion = GetVersion(AppVersion);
                return latestVersion.CompareTo(currentVersion) == 1 ? latestVersionString : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static Version GetVersion(string appVersion) =>
            new(string.Join('.', Regex.Matches(appVersion, "\\d+").Select(x => x.Groups[0].Value)));

        private static async Task<string> GetLatestVersion()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "blu-sta");
            var response = await httpClient.GetStringAsync(new Uri("https://api.github.com/repos/blu-sta/HK-Rando-4-Log-Display/releases/latest"));
            var jObject = JsonConvert.DeserializeObject<JObject>(response);
            return jObject["tag_name"]?.Value<string>();
        }

        private static async Task<string> GetLatestBetaVersion()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "blu-sta");
            var response = await httpClient.GetStringAsync(new Uri("https://api.github.com/repos/blu-sta/HK-Rando-4-Log-Display/releases"));
            var jObject = JsonConvert.DeserializeObject<JObject[]>(response).FirstOrDefault();
            return jObject["tag_name"]?.Value<string>();
        }
    }
}
