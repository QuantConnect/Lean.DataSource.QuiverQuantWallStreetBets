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
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NodaTime;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.DataSource;
using QuantConnect.Data.Market;

namespace QuantConnect.DataLibrary.Tests
{
    [TestFixture]
    public class QuiverWallStreetBetsUniverseTests
    {
        [Test]
        public void JsonRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();
            var serialized = JsonConvert.SerializeObject(expected);
            var result = JsonConvert.DeserializeObject(serialized, type);

            AssertAreEqual(expected, result);
        }

        [Test]
        public void Selection()
        {
            var datum = CreateNewSelection();

            var expected = from d in datum
                            where d.Sentiment > 1500 && d.Mentions > 7
                            select d.Symbol;
            var result = new List<Symbol> {Symbol.Create("HWM", SecurityType.Equity, Market.USA)};

            AssertAreEqual(expected, result);
        }

        private void AssertAreEqual(object expected, object result, bool filterByCustomAttributes = false)
        {
            foreach (var propertyInfo in expected.GetType().GetProperties())
            {
                // we skip Symbol which isn't protobuffed
                if (filterByCustomAttributes && propertyInfo.CustomAttributes.Count() != 0)
                {
                    Assert.AreEqual(propertyInfo.GetValue(expected), propertyInfo.GetValue(result));
                }
            }
            foreach (var fieldInfo in expected.GetType().GetFields())
            {
                Assert.AreEqual(fieldInfo.GetValue(expected), fieldInfo.GetValue(result));
            }
        }

        private BaseData CreateNewInstance()
        {
            return new QuiverWallStreetBetsUniverse
                {
                    Mentions = 5,
                    Rank = 100,
                    Sentiment = 10000m,

                    Symbol = new Symbol(SecurityIdentifier.Parse("A RPTMYV3VC57P"), "A"),
                    Time = DateTime.Today
                };
        }
        
        private IEnumerable<QuiverWallStreetBetsUniverse> CreateNewSelection()
        {
            return new []
            {
                new QuiverWallStreetBetsUniverse
                {
                    Mentions = 5,
                    Rank = 100,
                    Sentiment = 10000m,

                    Symbol = new Symbol(SecurityIdentifier.Parse("A RPTMYV3VC57P"), "A"),
                    Time = DateTime.Today
                },
                new QuiverWallStreetBetsUniverse
                {
                    Mentions = 10,
                    Rank = 100,
                    Sentiment = 10000m,

                    Symbol = new Symbol(SecurityIdentifier.Parse("AA R735QTJ8XC9X"), "HWM"),
                    Time = DateTime.Today
                }
            };
        }
    }
}