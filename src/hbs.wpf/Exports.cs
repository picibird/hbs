// Exports.cs
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

using System.Windows.Media.Media3D;
using picibird.hbs;
using picibird.hbs.viewmodels.book3D;
using picibird.hbs.viewmodels.shelf;
using picibird.hbs.wpf;
using picibird.hbs.wpf.view;
using picibird.wpf.core.views;
using picibits.core.export.views;
using picibits.core.export.services;
using picibits.app.services;
using picibird.hbs.wpf.qrcode;

// VIEWS

[assembly: ViewExport(typeof(HbsAppViewModel), typeof(HbsAppView))]
[assembly: ViewExport(typeof(Book3DViewModel), typeof(ContainerUIElement3D), typeof(ItemsView3DAdapter))]
[assembly: ViewExport(typeof(Bookshelf3DViewModel), typeof(Bookshelf3DView))]

[assembly: ServiceExport(typeof(IQRCodes), typeof(QRCodes))]
[assembly: ServiceExport(typeof(IWindows), typeof(HbsWindows))]