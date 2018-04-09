// CloseBookTransition.cs
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
using picibird.hbs.models;
using picibird.hbs.viewmodels.book3D;
using picibits.app.transition;
using picibits.core;
using picibits.core.intent;

namespace picibird.hbs.transition
{
    public class CloseBookTransition : TransitionBase
    {
        public CloseBookTransition(Book3DViewModel book3D, Book book)
        {
            if (book == null || book3D == null)
                throw new ArgumentException("book and book3d cannot be null");
            Book3D = book3D;
            Book = book;
        }

        public Book3DViewModel Book3D { get; set; }
        public Book Book { get; set; }

        public override void OnTransitionStarting()
        {
            HBS.IsAnimating = true;

            HBS.ViewModel.Opened.Visibility = false;
            HBS.ViewModel.Opened.BookVM.Book = null;
            HBS.ViewModel.Opened.BookVM.DropShadowOpacity = 0;

            HBS.ViewModel.BookFlyout3dVM.Visibility = true;
            HBS.ViewModel.BookFlyout3dVM.Book = Book;

            HBS.ViewModel.ShelfViewModel.IsBitmapCacheEnabled = true;
        }

        public override void OnTransitionProgress(double progress)
        {
            HBS.ViewModel.BlackBlendingViewModel.Opacity = (1 - progress)*HBS.BlackBlendingOpacity;
            HBS.ViewModel.BookFlyout3dVM.Progress = progress;
        }

        public override void OnTranistionCompleted()
        {
            HBS.ViewModel.BookFlyout3dVM.Visibility = false;
            HBS.ViewModel.BookFlyout3dVM.Book = null;
            HBS.ViewModel.BookFlyout3dVM.Recreate3D();

            HBS.ViewModel.ShelfViewModel.IsBitmapCacheEnabled = false;
            HBS.ViewModel.BlackBlendingViewModel.IsHitTestVisible = false;


            Book3D.Visibility = true;
            HBS.IsAnimating = false;

            Pici.Intent.Send(new Intent(Intent.ACTION_WRITE_NFC_URI, null));
            ShelfhubSearch.TrackClose("book", Book.Hit.id);
        }
    }
}