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
 *
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using NodaTime;
using ProtoBuf;
using QuantConnect;
using QuantConnect.Data;
namespace QuantConnect.DataSource
{
    /// <summary>
    /// Universe Selection helper class for QuiverWallStreetBets dataset
    /// </summary>
    public class QuiverWallStreetBetsUniverse : BaseData
    {
        /// <summary>
        /// Symbol of data
        /// </summary>
        [ProtoMember(10)]
        public Symbol Symbol { get; set; }

        /// <summary>
        /// The number of mentions on the given date
        /// </summary>
        [ProtoMember(11)]
        [JsonProperty(PropertyName = "Mentions")]
        public int Mentions { get; set; }
        
        /// <summary>
        /// This ticker's rank on the given date (as determined by total number of mentions)
        /// </summary>
        [ProtoMember(12)]
        [JsonProperty(PropertyName = "Rank")]
        public int Rank { get; set; }
        
        /// <summary>
        /// Average sentiment of all comments containing the given ticker on this date. Sentiment is calculated using VADER sentiment analysis.
        /// The value can range between -1 and +1. Negative values imply negative sentiment, whereas positive values imply positive sentiment.
        /// </summary>
        [ProtoMember(13)]
        [JsonProperty(PropertyName = "Sentiment")]
        public decimal Sentiment { get; set; }

        /// <summary>
        /// The period of time that occurs between the starting time and ending time of the data point
        /// </summary>
        [ProtoMember(14)]
        public TimeSpan Period { get; set; }

        public override DateTime EndTime
        {
            // define end time as exactly 1 day after Time
            get { return Time + QuantConnect.Time.OneDay; }
            set { Time = value - QuantConnect.Time.OneDay; }
        }
        
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "alternative",
                    "quiver",
                    "wallstreetbets",
                    "universe",
                    $"{date.ToStringInvariant(DateFormat.EightCharacter)}.csv"
                ),
                SubscriptionTransportMedium.LocalFile
            );
        }

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var csv = line.Split(',');

            return new QuiverWallStreetBetsUniverse
            {
                Mentions = Parse.Int(csv[2]),
                Rank = Parse.Int(csv[3]),
                Sentiment = decimal.Parse(csv[4], NumberStyles.Any, CultureInfo.InvariantCulture),

                Symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]),
                Time = date.AddDays(-1),
            };
        }
    }
}