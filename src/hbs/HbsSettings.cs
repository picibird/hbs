// HbsSettings.cs
// Date Created: 20.01.2016
// 
// Copyright (c) 2016, picibird GmbH 
// All rights reserved.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Threading.Tasks;
using Newtonsoft.Json;
using picibird.hbs.ldu;
using picibits.core;
using picibits.core.helper;
using picibits.core.settings;
using picibits.core.util;

namespace picibird.hbs
{
    public enum ResetAction
    {
        StartupSearch,
        BibshelfList
    }

    [JsonObject(MemberSerialization.OptIn, Title = "Hybrid Bookshelf Settings")]
    public class HbsSettings : SettingsModel
    {
        public static event SimpleEventHandler<SettingsModel> Loaded;
        public static event SimpleEventHandler<SettingsModel> Saved;

        public HbsSettings()
        {
            General = Pici.Settings.Get<GeneralSettings>();
            //Ldu = Pici.Settings.Get<LduSettings>();
            //Cover = Pici.Settings.Get<CoverSettings>();
        }

        [JsonProperty(Required = Required.Always)]
        public GeneralSettings General { get; set; }

        //[JsonProperty(Required = Required.Always)]
        public LduSettings Ldu { get; set; }

        //[JsonProperty(Required = Required.Always)]
        public CoverSettings Cover { get; set; }

        public override async Task Save()
        {
            await General.Save();
            //await Ldu.Save();
            //await Cover.Save();
            Saved?.Invoke(this);
        }


    }


    [JsonObject(MemberSerialization.OptIn, Title = "General")]
    public class GeneralSettings : SettingsModel
    {

        [JsonProperty("Startup Search", Required = Required.Always)]
        public string StartupSearch { get; set; } = "User Experience";

        [JsonProperty("Language")]
        public LanguageEnum Language { get; set; } = LanguageEnum.German;
    }

    [JsonObject(MemberSerialization.OptIn, Title = "Library Data Unifier")]
    public class LduSettings : SettingsModel
    {
        public LduSettings()
        {
            //Test Sysstem
            //Pazpar2Url = "http://hbs-kn.ldu.bsz-bw.de/pazpar2/search.pz2";
            //Produktiv System
            Pazpar2Url = "";
            MaxRecords = 150;
            LduSourceUsage = LduSourceUsage.ALL;
        }

        [JsonProperty("URL", Required = Required.Always)]
        public string Pazpar2Url { get; set; }

        [JsonProperty("Maximum Records", Required = Required.Always)]
        public int MaxRecords { get; set; }

        [JsonProperty("Source Database Filter", Required = Required.Always)]
        public LduSourceUsage LduSourceUsage { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn, Title = "Covers")]
    public class CoverSettings : SettingsModel
    {
        public CoverSettings()
        {
            IsLoadCoverEnabled = BoolEnum.Yes;
            CoverProviderUrl = "";
        }

        [JsonProperty("Load Covers")]
        public BoolEnum IsLoadCoverEnabled { get; set; }

        [JsonProperty("Cover Provider URL", Required = Required.Always)]
        public string CoverProviderUrl { get; set; }
    }
}