/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Data.Auxiliary;
using QuantConnect.DataSource;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Logging;
using QuantConnect.Util;

namespace QuantConnect.DataProcessing
{
    /// <summary>
    /// QuiverWallStreetBetsDataDownloader implementation. https://www.quiverquant.com/
    /// </summary>
    public class QuiverWallStreetBetsDataDownloader : IDisposable
    {
        public const string VendorName = "quiver";
        public const string VendorDataName = "wallstreetbets";

        private readonly string _destinationFolder;
        private readonly string _universeFolder;
        private readonly string _clientKey;
        private readonly string _dataFolder = Globals.DataFolder;
        private readonly bool _canCreateUniverseFiles;
        private readonly int _maxRetries = 5;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        /// <summary>
        /// Control the rate of download per unit of time.
        /// </summary>
        private readonly RateGate _indexGate;

        /// <summary>
        /// Creates a new instance of <see cref="QuiverWallStreetBets"/>
        /// </summary>
        /// <param name="destinationFolder">The folder where the data will be saved</param>
        /// <param name="apiKey">The QuiverQuant API key</param>
        public QuiverWallStreetBetsDataDownloader(string destinationFolder, string apiKey = null)
        {
            _destinationFolder = Path.Combine(destinationFolder, "alternative", VendorName, VendorDataName);
            _universeFolder = Path.Combine(_destinationFolder, "universe");
            _clientKey = apiKey ?? Config.Get("quiver-auth-token");
            _canCreateUniverseFiles = Directory.Exists(Path.Combine(_dataFolder, "equity", "usa", "map_files"));

            // Represents rate limits of 10 requests per 1.1 second
            _indexGate = new RateGate(10, TimeSpan.FromSeconds(1.1));

            Directory.CreateDirectory(_destinationFolder);
            Directory.CreateDirectory(_universeFolder);
        }

        /// <summary>
        /// Runs the instance of the object.
        /// </summary>
        /// <returns>True if process all downloads successfully</returns>
        public bool Run()
        {
            var stopwatch = Stopwatch.StartNew();
            var today = DateTime.UtcNow.Date;
            Log.Trace($"QuiverWallStreetBetsDataDownloader.Run(): Start downloading/processing QuiverQuant WallStreetBets data");

            try
            {
                var quiverWsbData = HttpRequester($"historical/wallstreetbets").SynchronouslyAwaitTaskResult();
                if (string.IsNullOrWhiteSpace(quiverWsbData))
                {
                    // We've already logged inside HttpRequester
                    return false;
                }

                var wsbMentionsByDate = JsonConvert.DeserializeObject<List<RawQuiverWallStreetBets>>(quiverWsbData, _jsonSerializerSettings)?
                    .OrderBy(x => x.Date.Date).ThenBy(x => x.Ticker).GroupBy(x => x.Date.Date);

                var wsbMentionsByTicker = new Dictionary<string, List<string>>();

                var mapFileProvider = new LocalZipMapFileProvider();
                mapFileProvider.Initialize(new DefaultDataProvider());

                foreach (var kvp in wsbMentionsByDate)
                {
                    var date = kvp.Key;
                    if (date == today) continue;

                    var universeCsvContents = new List<string>();

                    foreach (var wsbMention in kvp)
                    {
                        var ticker = wsbMention.Ticker.ToUpperInvariant();

                        if (!wsbMentionsByTicker.TryGetValue(ticker, out var wsbTickerMentions))
                        {
                            wsbMentionsByTicker.Add(ticker, new List<string>());
                        }

                        wsbMentionsByTicker[ticker].Add($"{date:yyyyMMdd},{wsbMention.Mentions},{wsbMention.Rank},{wsbMention.Sentiment}");

                        if (!_canCreateUniverseFiles) continue;

                        var sid = SecurityIdentifier.GenerateEquity(ticker, Market.USA, true, mapFileProvider, date);
                        universeCsvContents.Add($"{sid},{ticker},{wsbMention.Mentions},{wsbMention.Rank},{wsbMention.Sentiment}");
                    }

                    if (_canCreateUniverseFiles && universeCsvContents.Any())
                    {
                        SaveContentToFile(_universeFolder, $"{date:yyyyMMdd}", universeCsvContents);
                    }
                }

                wsbMentionsByTicker.DoForEach(kvp => SaveContentToFile(_destinationFolder, kvp.Key, kvp.Value));
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }

            Log.Trace($"QuiverWallStreetBetsDataDownloader.Run(): Finished in {stopwatch.Elapsed.ToStringInvariant(null)}");
            return true;
        }

        /// <summary>
        /// Sends a GET request for the provided URL
        /// </summary>
        /// <param name="url">URL to send GET request for</param>
        /// <returns>Content as string</returns>
        /// <exception cref="Exception">Failed to get data after exceeding retries</exception>
        private async Task<string> HttpRequester(string url)
        {
            for (var retries = 1; retries <= _maxRetries; retries++)
            {
                try
                {
                    using var client = new HttpClient();

                    client.BaseAddress = new Uri("https://api.quiverquant.com/beta/");
                    client.DefaultRequestHeaders.Clear();

                    // You must supply your API key in the HTTP header,
                    // otherwise you will receive a 403 Forbidden response
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _clientKey);

                    // Responses are in JSON: you need to specify the HTTP header Accept: application/json
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    // Makes sure we don't overrun Quiver rate limits accidentally
                    _indexGate.WaitToProceed();

                    var response = await client.GetAsync(Uri.EscapeUriString(url));
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            Log.Error($"QuiverWallStreetBetsDataDownloader.HttpRequester(): Files not found at url: {Uri.EscapeUriString(url)}");
                            response.DisposeSafely();
                            return string.Empty;
                        case HttpStatusCode.Unauthorized:
                            {
                                var finalRequestUri = response.RequestMessage.RequestUri; // contains the final location after following the redirect.
                                response = client.GetAsync(finalRequestUri).Result; // Reissue the request. The DefaultRequestHeaders configured on the client will be used, so we don't have to set them again.
                                break;
                            }
                    }

                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();
                    response.DisposeSafely();

                    return result;
                }
                catch (Exception e)
                {
                    Log.Error(e, $"QuiverWallStreetBetsDataDownloader.HttpRequester(): Error at HttpRequester. (retry {retries}/{_maxRetries})");
                    Thread.Sleep(1000);
                }
            }

            throw new Exception($"QuiverWallStreetBetsDataDownloader.HttpRequester(): Request failed with no more retries remaining (retry {_maxRetries}/{_maxRetries})");
        }

        /// <summary>
        /// Saves contents to disk
        /// </summary>
        /// <param name="destinationFolder">Final destination of the data</param>
        /// <param name="name">File name</param>
        /// <param name="contents">Contents to write</param>
        private void SaveContentToFile(string destinationFolder, string name, IEnumerable<string> contents)
        {
            var finalPath = Path.Combine(destinationFolder, $"{name.ToLowerInvariant()}.csv");
            
            var lines = new HashSet<string>(contents);
            if (File.Exists(finalPath))
            {
                foreach (var line in File.ReadAllLines(finalPath))
                {
                    lines.Add(line);
                }
            }

            var finalLines = destinationFolder.Contains("universe") ?
                lines.OrderBy(x => x) :
                lines.OrderBy(x => DateTime.ParseExact(x.Split(',').First(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));

            try
            {
                File.WriteAllLines(finalPath, finalLines);
            }
            catch (Exception e)
            {
                Log.Error(e, $"QuiverWallStreetBetsDataDownloader.HttpRequester(): Final Path: {finalPath}");
            }
        }


        /// <summary>
        /// Disposes of unmanaged resources
        /// </summary>
        public void Dispose() => _indexGate?.Dispose();

        private class RawQuiverWallStreetBets : QuiverWallStreetBets
        {
            [JsonProperty(PropertyName = "Ticker")]
            public string Ticker { get; set; }
        }
    }
}