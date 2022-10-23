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
        public Task<bool> IsUpdateAvailable();
    }
    public class VersionChecker : IVersionChecker
    {
        public async Task<bool> IsUpdateAvailable()
        {
            try
            {
                var currentVersion = GetVersion(AppVersion);
                var latestVersion = GetVersion(await GetLatestVersion());
                return latestVersion.CompareTo(currentVersion) == 1;
            }
            catch
            {
                return false;
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
    }
}
