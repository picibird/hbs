// HBS.cs
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
using picibird.hbs.ldu;
using picibird.hbs.models;
using picibird.hbs.viewmodels;
using picibird.hbs.viewmodels.book3D;
using picibits.app.animation;
using SimpleInjector;

namespace picibird.hbs
{
    public static class HBS
    {
        public static bool IsAnimating;


        /*config*/
        public static readonly double BlackBlendingOpacity = 0;
        public static readonly double AnimationSeconds = 0.6;
        public static readonly int RowCount = 3;
        public static readonly int ColumnCount = 6;

        public static readonly PercentHandler AnimationEaseInOut = AnimationTransitions.QuadEaseInOut;
        public static readonly PercentHandler AnimationEaseIn = AnimationTransitions.QuadEaseIn;
        public static readonly PercentHandler AnimationEaseOut = AnimationTransitions.QuadEaseOut;

        static HBS()
        {
            Injections = new Container();
        }

        public static Container Injections { get; private set; }

        public static HBSViewModel ViewModel { get; set; }


        public static Book CreateBook(Book3DViewModel book3D)
        {
            if (!(book3D.Model is Hit))
                throw new ArgumentException("book3D.Model is not of type Hit");
            var book = new Book
            {
                Hit = book3D.Model as Hit,
                CoverWidth = book3D.CoverWidth,
                CoverHeight = book3D.CoverHeight,
                SpineWidth = book3D.SpineWidth
            };

            book.LoadQrCode();
            return book;
        }

        #region Search

        private static Search mSearch;

        public static Search Search
        {
            get
            {
                if (mSearch == null)
                    mSearch = new Search();
                return mSearch;
            }
        }

        #endregion Search
    }
}