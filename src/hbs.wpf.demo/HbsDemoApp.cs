// HbsDemoApp.cs
// Date Created: 27.01.2016
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

using picibird.hbs.wpf.demo;
using picibits.app;
using picibits.core;

[assembly: AppExport(typeof(HbsDemoApp), "HybridBookshelf")]

namespace picibird.hbs.wpf.demo
{
    public class HbsDemoApp : HbsApp
    {
        private string _lduUrl = "http://os-ldu.bsz-bw.de/pazpar2/search.pz2";
        private string _coverProviderUrl = "http://hbs-kn.ldu.bsz-bw.de/coverdata.php";

        protected override void ApplySettings()
        {
            UseProxyUrlsNotForPublicGithub();
            ValidateDemoSettings();
            base.ApplySettings();
        }

        private void UseProxyUrlsNotForPublicGithub()
        {
            //TODO Remove before going public
            _lduUrl = "http://72270b9f929a3ef6-eu-west-1.getstatica.com/pazpar2/search.pz2";
            _coverProviderUrl = "http://72270b9f929a3ef6-eu-west-1.getstatica.com/coverdata.php";
            var pazpar2 = Pici.Settings.Get<LduSettings>();
            pazpar2.Pazpar2Url = _lduUrl;
            var cover = Pici.Settings.Get<CoverSettings>();
            cover.CoverProviderUrl = _coverProviderUrl;
        }

        protected virtual void ValidateDemoSettings()
        {
            //set ldu defaults
            var pazpar2 = Pici.Settings.Get<LduSettings>();
            if (string.IsNullOrEmpty(pazpar2.Pazpar2Url))
                pazpar2.Pazpar2Url = _lduUrl;
            //set cover provider and histomat defaults
            var cover = Pici.Settings.Get<CoverSettings>();
            if (string.IsNullOrEmpty(cover.CoverProviderUrl))
                cover.CoverProviderUrl = _coverProviderUrl;
        }
    }
}