<picture >
  <source media="(prefers-color-scheme: dark)" srcset="https://user-images.githubusercontent.com/79997186/190704543-581838c9-4c83-43dc-9abf-aeefcd7ff0fd.png">
  <source media="(prefers-color-scheme: light)" srcset="https://user-images.githubusercontent.com/79997186/190704618-4b09e405-02e8-4774-8480-d896537caab2.png">
  <img alt="Dataset Integration">
</picture>
  
&nbsp;

### Table of Contents
- [Introduction](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#introduction)
- [About](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#about)
    - [Introduction](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#introduction-1)
    - [About the Provider](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#about-the-provider)
    - [Getting Started](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#getting-started)
    - [Using the Dataset](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#using-the-dataset)
        - [Backtesting with VSCode User Interace](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#backtesting-with-vscode-user-interface)
        - [Backtesting with LEAN CLI](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#backtesting-with-lean-cli)
    - [Data Summary](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#data-summary)
    - [Example Applications](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#example-applications)
    - [Data Point Attributes](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#data-point-attributes)
- [Documentation](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#documentation)
    - [Requesting Data](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#requesting-data)
    - [Accessing Data](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#accessing-data)
    - [Historical Data](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#historical-data)
    - [Universe Selection](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#universe-selection)
    - [Remove Subscriptions](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#remove-subscriptions)
- [Lean DataSource SDK](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#lean-datasource-sdk)
    - [Introduction](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#introduction-2)
    - [Implementing your own data source](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#implementing-your-own-data-source)
- [Contributions](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#contributions)
- [Code of Conduct](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#code-of-conduct)
- [License Model](https://github.com/QuantConnect/Lean.DataSource.QuiverQuantWallStreetBets#license-model)


## Introduction
 
This repository hosts the WallStreetBets dataset by Quiver Quantitative Integration with the QuantConnect LEAN Algorithmic Trading Engine.

[LEAN](https://github.com/QuantConnect/Lean) is maintained primarily by [QuantConnect](https://www.quantconnect.com), a US based technology company hosting a cloud algorithmic trading platform. QuantConnect has successfully hosted more than 200,000 live algorithms since 2015, and trades more than $1B volume per month.
  
## About

### Introduction

The WallStreetBets dataset by Quiver Quantitative tracks daily mentions of different equities on Reddit’s popular WallStreetBets forum. The data covers 6,000 Equities, starts in August 2018, and is delivered on a daily frequency. The dataset is created by scraping the daily discussion threads on r/WallStreetBets and parsing the comments for ticker mentions.

This dataset depends on the  [US Equity Security Master](https://www.quantconnect.com/datasets/quantconnect-us-equity-security-master)  dataset because the US Equity Security Master dataset contains information on splits, dividends, and symbol changes.

### About the Provider

[Quiver Quantitative](https://www.quiverquant.com/) was founded by two college students in February 2020 with the goal of bridging the information gap between Wall Street and non-professional investors. Quiver allows retail investors to tap into the power of big data and have access to actionable, easy to interpret data that hasn’t already been dissected by Wall Street.

### Getting Started

The following snippet demonstrates how to request data from the WallStreetBets dataset:

Python:
```py
self.symbol = self.AddEquity("AAPL",  Resolution.Daily).Symbol
self.dataset_symbol = self.AddData(QuiverWallStreetBets, self.symbol).Symbol

self.AddUniverse(QuiverWallStreetBetsUniverse,  "QuiverWallStreetBetsUniverse",  Resolution.Daily, self.UniverseSelection)
```
C#:
```cs
_symbol = AddEquity("AAPL", Resolution.Daily).Symbol;
_datasetSymbol = AddData<QuiverWallStreetBets>(_symbol).Symbol;

AddUniverse<QuiverWallStreetBetsUniverse>("QuiverWallStreetBetsUniverse", Resolution.Daily, UniverseSelection);
```

### Using the Dataset
#### Backtesting with VSCode User Interace

The following video shows an example of using the dataset in QuantConnect Cloud:

![wallstreetbets-cloud-demo](https://user-images.githubusercontent.com/38889814/193858736-c1c80b53-cd2c-456e-8f58-88b99048cf79.gif)


#### Backtesting with LEAN CLI
Follow these steps to use the dataset locally through the LEAN CLI:
1. [Create a new project](https://www.quantconnect.com/docs/v2/lean-cli/projects/project-management#02-Create-Projects).
   ```
   $ lean create-project "My Project"
   ```
2. Edit the code files in your project so the algorithm subscribes to the data and logs it.

3. If you don't have the data on your local machine, backtest the algorithm with the [ApiDataProvider](https://www.quantconnect.com/docs/v2/lean-cli/backtesting/deployment#04-Download-Datasets-During-Backtests).
   ```
   $ lean backtest "My Project" --download-data
   ```
4. If you have the data on your local machine, backtest the algorithm with the local data provider.
   ```
   $ lean backtest "My Project" --data-provider Local
   ```
   You can [use the CLI to download the data](https://www.quantconnect.com/datasets/quiver-quantitative-wallstreetbets/cli).

![wallstreetbets-local-demo](https://user-images.githubusercontent.com/38889814/193858579-6ff2e076-091c-4fba-b803-d411cc1fffba.gif)

### Data Summary

The following table describes the dataset properties:

| Property  | Value |
| ----------- | ----------- |
|Start Date|August 2018|
|Asset Coverage|6,000 US Equities|
|Data Density|Sparse|
|Resolution|Daily|
|Timezone|UTC|

### Example Applications
The WallStreetBets dataset enables you to create strategies using the latest activity on the WallStreetBets daily discussion thread. Examples include the following strategies:

-   Trading any security that is being mentioned
-   Trading securities that are receiving more/less mentions than they were previously
-   Trading the security that is being mentioned the most/least for the day
  
### Data Point Attributes

The WallStreetBets dataset provides  **QuiverWallStreetBets**  and  **QuiverWallStreetBetsUniverse**  objects. To view the members of these objects, see [Data Point Attributes](https://www.quantconnect.com/docs/v2/writing-algorithms/datasets/quiver-quantitative/wallstreetbets#05-Data-Point-Attributes) in the QuantConnect documentation.

## Documentation

### Requesting Data

To add WallStreetBets data to your algorithm, call the **AddData** method. Save a reference to the dataset **Symbol** so you can access the data later in your algorithm.

Python:
```py
class  QuiverWallStreetBetsDataAlgorithm(QCAlgorithm):
    def  Initialize(self)  ->  None:
        self.SetStartDate(2019,  1,  1)
        self.SetEndDate(2020,  6,  1)
        self.SetCash(100000)
        self.symbol = self.AddEquity("AAPL",  Resolution.Daily).Symbol
        self.dataset_symbol = self.AddData(QuiverWallStreetBets, self.symbol).Symbol
```
C#:
```cs
namespace QuantConnect
{
    public  class  QuiverWallStreetBetsDataAlgorithm  :  QCAlgorithm
    {
        private  Symbol _symbol, _datasetSymbol;
        public  override  void  Initialize()
        {
            SetStartDate(2019,  1,  1);
            SetEndDate(2020,  6,  1);
            SetCash(100000);
            _symbol =  AddEquity("AAPL",  Resolution.Daily).Symbol;
            _datasetSymbol =  AddData<QuiverWallStreetBets(_symbol).Symbol;
        }
    }
}
```

### Accessing Data

To get the current WallStreetBets data, index the current  [**Slice**](https://www.quantconnect.com/docs/v2/writing-algorithms/key-concepts/time-modeling/timeslices)  with the dataset  **Symbol**. Slice objects deliver unique events to your algorithm as they happen, but the  **Slice**  may not contain data for your dataset at every time step. To avoid issues, check if the  **Slice**  contains the data you want before you index it.

Python:
```py
def  OnData(self, slice:  Slice)  ->  None:
    if slice.ContainsKey(self.dataset_symbol):
        data_point = slice[self.dataset_symbol]
        self.Log(f"{self.dataset_symbol} mentions at {slice.Time}: {data_point.Mentions}")
```
C#:
```cs
public  override  void  OnData(Slice slice)
{
    if  (slice.ContainsKey(_datasetSymbol))
    {
        var dataPoint = slice[_datasetSymbol];
        Log($"{_datasetSymbol} mentions at {slice.Time}: {dataPoint.Mentions}");
    }
}
```
To iterate through all of the dataset objects in the current **Slice**, call the **Get** method.
Python:
```py
def  OnData(self, slice:  Slice)  ->  None:
    for dataset_symbol, data_point in slice.Get(QuiverWallStreetBets).items():
        self.Log(f"{dataset_symbol} mentions at {slice.Time}: {data_point.Mentions}")
```
C#:
```cs
public  override  void  OnData(Slice slice)
{
    foreach  (var kvp in slice.Get<QuiverWallStreetBets>())
    {
        var datasetSymbol = kvp.Key;
        var dataPoint = kvp.Value;
        Log($"{datasetSymbol} mentions at {slice.Time}: {dataPoint.Mentions}");
    }
}
```

### Historical Data

To get historical WallStreetBets data, call the **History** method with the dataset **Symbol**. If there is no data in the period you request, the history result is empty.

Python:
```py
# DataFrame
history_df = self.History(self.dataset_symbol,  100,  Resolution.Daily)

# Dataset objects
history_bars = self.History[QuiverWallStreetBets](self.dataset_symbol,  100,  Resolution.Daily)
```
C#:
```cs
var history =  History<QuiverWallStreetBets>(_datasetSymbol,  100,  Resolution.Daily);
```
For more information about historical data, see [History Requests](https://www.quantconnect.com/docs/v2/writing-algorithms/historical-data/history-requests).

### Universe Selection

To select a dynamic universe of US Equities based on WallStreetBets data, call the  **AddUniverse**  method with the  **QuiverWallStreetBetsUniverse**  class and a selection function.

Python:
```py
def  Initialize(self)  ->  None:
    self.AddUniverse(QuiverWallStreetBetsUniverse,  "QuiverWallStreetBetsUniverse",  Resolution.Daily, self.UniverseSelection)

def  UniverseSelection(self, alt_coarse:  List[QuiverWallStreetBetsUniverse])  ->  List[Symbol]:
    return  [d.Symbol  for d in alt_coarse
         if d.Mentions  >  100 and d.Rank  <  100]
```
C#:
```cs
public  override  void  Initialize()
{
    AddUniverse("QuiverWallStreetBetsUniverse",  Resolution.Daily,
        altCoarse =>
        {  
            return from d in altCoarse
                where d.Mentions  >  10 && d.Rank  >  10
                select d.Symbol;
        });
}
```

For more information about dynamic universes, see [Universes](https://www.quantconnect.com/docs/v2/writing-algorithms/universes/key-concepts).

### Remove Subscriptions

To remove a subscription, call the  **RemoveSecurity**  method.

Python
```py
self.RemoveSecurity(self.dataset_symbol)
```
C#:
```cs
RemoveSecurity(_datasetSymbol);
```

If you subscribe to WallStreetBets data for assets in a dynamic universe, remove the dataset subscription when the asset leaves your universe. To view a common design pattern, see  [Track Security Changes](https://www.quantconnect.com/docs/v2/writing-algorithms/algorithm-framework/alpha/key-concepts#05-Track-Security-Changes).

&nbsp;

&nbsp;

&nbsp;

## Lean DataSource SDK
[![Build Status](https://github.com/QuantConnect/LeanDataSdk/workflows/Build%20%26%20Test/badge.svg)](https://github.com/QuantConnect/LeanDataSdk/actions?query=workflow%3A%22Build%20%26%20Test%22)

### Introduction

The Lean Data SDK is a cross-platform template repository for developing custom data types for Lean. These data types will be consumed by [QuantConnect](https://www.quantconnect.com/) trading algorithms and research environment, locally or in the cloud.

It is composed by example .Net solution for the data type and converter scripts.

### Implementing your own data source

To learn more about implementing your own data source for our marketplace, visit the QuantConnect Documentation on [Datasets](https://www.quantconnect.com/docs/v2/our-platform/datasets/) for more information.


## Contributions

Contributions are warmly very welcomed but we ask you to read the existing code to see how it is formatted, commented and ensure contributions match the existing style. All code submissions must include accompanying tests. Please see the [contributor guide lines](https://github.com/QuantConnect/Lean/blob/master/CONTRIBUTING.md).
 
## Code of Conduct

We ask that our users adhere to the community [code of conduct](https://www.quantconnect.com/codeofconduct) to ensure QuantConnect remains a safe, healthy environment for high quality quantitative trading discussions.

## License Model

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.