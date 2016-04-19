// ShelfViewModel.cs
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
using picibird.hbs.behaviours;
using picibird.hbs.viewmodels.shelf;

using picibits.app.behaviour;
using picibits.app.transition;
using picibits.core;
using picibits.core.export.services;
using picibits.core.helper;
using picibits.core.mvvm;
using picibits.core.pointer;

namespace picibird.hbs.viewmodels
{
    public class ShelfViewModel : TransitionItemsViewModel
    {
        public ShelfViewModel()
        {
            Transition = new SwipeTransition();
            IsRotating = true;
            Pointing.IsEnabled = true;

            var tapBehaviour = new TapBehaviour();
            tapBehaviour.Tap += OnTap;
            Behaviours.Add(tapBehaviour);
            Behaviours.Add(new ShelfSwipeBehaviour());

            HBS.Search.SearchStarting += OnSearchStarting;
        }

        private void OnTap(object sender, EventArgs e)
        {
            var pe = e as PointerEventArgs;
            var pount = pe.Pointer.GetPosition(View);
            if (pount.X > 150)
                Events.OnIdleOnce(() => Pici.Services.Get<IKeyboard>().ClearFocus());
        }

        private void OnSearchStarting(object sender, SearchStartingEventArgs args)
        {
            ResetSearch();
        }

        public BookshelfViewModel GetSelectedBookshelf()
        {
            return (BookshelfViewModel) SelectedItem;
        }

        private void ResetSearch()
        {
            SelectedIndex = 0;
            HBS.Search.SearchRequest.PageIdx = 0;
        }

        protected override void OnIsSelectedIndexEvenChanged(bool oldIsSelectedIndexEven, bool newIsSelectedIndexEven)
        {
            base.OnIsSelectedIndexEvenChanged(oldIsSelectedIndexEven, newIsSelectedIndexEven);

            if (newIsSelectedIndexEven)
            {
                var indexRounded = (int) Math.Round(SelectedIndex);
                indexRounded = Math.Min(HBS.Search.Callback.MaxPageIndex, indexRounded);
                indexRounded = Math.Max(0, indexRounded);
                var pageIndex = indexRounded;
                if (pageIndex != HBS.Search.SearchRequest.PageIdx)
                {
                    HBS.Search.SearchRequest.PageIdx = pageIndex;
                    Pici.Log.debug(typeof (BookshelfViewModel),
                        string.Format("idx={0}  pageIdx={1}", SelectedIndex, pageIndex));
                }
            }
        }

        public override void UpdateTransition(ViewModel model)
        {
            base.UpdateTransition(model);
            UpdateOnTransitionChanged();
        }

        private void UpdateOnTransitionChanged()
        {
            foreach (BookshelfViewModel bookshelf in Items)
            {
                var rotatedIndex = bookshelf.RotatedIndex;
                var indexDistance = Math.Abs(SelectedIndex - rotatedIndex);
                //update is hittestvisible
                if (indexDistance == 0)
                {
                    bookshelf.Books3D.IsHitTestVisible = true;
                    bookshelf.IsBitmapCacheEnabled = false;
                }
                else
                {
                    bookshelf.Books3D.IsHitTestVisible = false;
                    bookshelf.IsBitmapCacheEnabled = true;
                }

                //update opacity
                if (indexDistance < 1.5 && rotatedIndex >= 0)
                {
                    var opacity = 1 - Math.Min(1, indexDistance*0.8);
                    bookshelf.Opacity = opacity;
                }
            }
        }

        public void CreateShelf()
        {
            var bookshelfCount = 4;
            for (var i = 0; i < bookshelfCount; i++)
            {
                var bookshelf3D = new Bookshelf3DViewModel();
                bookshelf3D.CreateBooks();
                var bookshelf = new BookshelfViewModel(bookshelf3D);
                Items.Add(bookshelf);
            }
        }
    }
}