// HbsApplication.cs
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
using System.Windows;
using picibird.hbs.config;

using picibird.wpf.app;
using picibits.core;
using picibits.core.style;

namespace picibird.hbs.wpf
{
    public class HbsApplication : PiciApplication
    {
        protected override void ApplyStyles()
        {
            base.ApplyStyles();
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/Values.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/Buttons.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/Colors.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/Fonts.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/FontIcons.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/Images.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/HBSStyles.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/Search.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/book/Book.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(
                new StyleResource(new Uri("/hbs.wpf;component/styles/book/BookOpened.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(new StyleResource(new Uri("/hbs.wpf;component/styles/Shelf.xaml", UriKind.Relative)));
            Pici.StyleResources.Add(
                new StyleResource(new Uri("/hbs.wpf;component/styles/web/BookLinkBrowserStyles.xaml", UriKind.Relative)));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!Started) return;
        }
    }
}