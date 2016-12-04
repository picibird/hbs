// HbsApp.cs
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

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using picibird.hbs.viewmodels;
using picibits.app;
using picibits.core;
using picibits.core.export.services;
using picibits.core.helper;

namespace picibird.hbs
{
    public abstract class HbsApp : App
    {
        protected HbsApp()
        {
            Pici.Resources.CultureInfo = new CultureInfo("de-DE");
            var storage = Pici.Services.Get<IStorage>();
            storage.AppFragments = new[] {"picibird", "hbs"};
            //Host.Style = new ViewStyle("AppHostViewboxStyle");
            //Host.HostWidth = 1920;
            //Host.HostHeight = 1080;
            InitResources();
        }

        public override void Start()
        {
            StartHbs();
        }

        protected virtual void StartHbs()
        {
            ApplySettings();

            Events.OnIdleOnce(() =>
            {
                var settings = Pici.Settings.Get<GeneralSettings>();
                var searchString = settings.StartupSearch;
                if (Pici.CommandLineArgs.ContainsKey("query"))
                {
                    searchString = Pici.CommandLineArgs["query"];
                }

                var hbsVm = Model.Content as HBSViewModel;
                Debug.Assert(hbsVm != null, "hbsVm != null");
                hbsVm.SearchBoxTextViewModel.SearchText = searchString;
                HBS.Search.Start(searchString)
                    .ContinueWith(success => { Pici.Log.info(typeof(HbsApp), $"initial search {success}"); });
            });
        }

        public override void Suspend()
        {
        }

        public override void Resume()
        {
        }

        public override void Terminate()
        {
        }

        private void IsDebug()
        {
        }

        #region Resources

        public void InitResources()
        {
            var hbsCoreAssembly = typeof(HbsApp).GetTypeInfo().Assembly;
            Pici.Resources.AddManager("picibird.hbs.resources.hbs", hbsCoreAssembly);
            Pici.Resources.AddManager("picibird.hbs.resources.medium", hbsCoreAssembly);
            Pici.Resources.AddManager("picibird.hbs.resources.language", hbsCoreAssembly);
            Pici.Resources.AddManager("picibird.hbs.resources.department", hbsCoreAssembly);
            Pici.Resources.AddManager("picibird.hbs.resources.sortorder", hbsCoreAssembly);
        }

        #endregion Resources

        #region Settings

        protected virtual void ApplySettings()
        {
            ApplyGeneralSettings();
            ApplyPazpar2Settings();
            ApplyCoverSettings();
        }

        protected virtual void ApplyGeneralSettings()
        {
            var settings = Pici.Settings.Get<GeneralSettings>();
            //apply language
            switch (settings.Language)
            {
                case LanguageEnum.German:
                    Pici.Resources.CultureInfo = new CultureInfo("de-DE");
                    break;
                case LanguageEnum.English:
                    Pici.Resources.CultureInfo = new CultureInfo("en-US");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void ApplyPazpar2Settings()
        {
            var settings = Pici.Settings.Get<LduSettings>();
            picibird.hbs.ldu.Pazpar2Settings.PAZPAR2_URL = settings.Pazpar2Url;
            picibird.hbs.ldu.Pazpar2Settings.MAX_RECORDS = settings.MaxRecords;
            picibird.hbs.ldu.Pazpar2Settings.LduSourceUsage = settings.LduSourceUsage;
        }

        private void ApplyCoverSettings()
        {
            var settings = Pici.Settings.Get<CoverSettings>();
            picibird.hbs.ldu.Pazpar2Settings.COVER_PROVIDER_URL = settings.CoverProviderUrl;
            Host.TouchVisualizer.IsVisualizerEnabled = true;
        }

        #endregion Settings

        #region Model

        private AppViewModel mModel;

        public override AppViewModel Model
        {
            get
            {
                if (mModel == null)
                    mModel = CreateAppViewModel();
                return mModel;
            }
        }

        protected virtual AppViewModel CreateAppViewModel()
        {
            return new HbsAppViewModel();
        }

        #endregion Model
    }
}