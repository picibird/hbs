// OpenBookSwipeTransition.cs
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
using System.Linq;
using picibird.hbs.Intents;
using picibird.hbs.viewmodels.book3D;
using picibird.hbs.viewmodels.shelf;

using picibits.app.transition;
using picibits.core;
using picibits.core.helper;
using picibits.core.intent;
using picibits.core.math;

namespace picibird.hbs.transition
{
    public enum SwipeMode
    {
        Book,
        Shelf
    }

    public class OpenBookSwipeTransition : TransitionBase
    {
        private TransitionBase activeTransition;

        private OpenBookTransition openBookTransition;

        public OpenBookSwipeTransition(BookshelfViewModel bookshelf, Book3DViewModel book3D, Direction direction)
        {
            Bookshelf = bookshelf;
            Book3D = book3D;
            Direction = direction;
        }


        public BookshelfViewModel Bookshelf { get; }
        public Book3DViewModel Book3D { get; }

        public BookshelfViewModel NextBookshelfIndex { get; private set; }

        public int NextBookIndex { get; private set; }
        public Book3DViewModel NextBook3D { get; private set; }

        public Book3DViewModel LastNextBook3D { get; private set; }

        public SwipeMode SwipeMode { get; set; }

        public void OnDirectionChanged(Direction oldDirection, Direction newDirection)
        {
            if (Bookshelf == null || Book3D == null)
            {
                throw new ArgumentException("bookshelf and book3d arguments cannot be null");
            }


            if (newDirection == Direction.Center)
            {
                LastNextBook3D = NextBook3D;
                NextBook3D = Book3D;
                SwipeMode = SwipeMode.Book;
            }
            else
            {
                if (NextBook3D != null && NextBook3D.Model != null)
                {
                    NextBook3D.Visibility = true;
                }
                NextBookIndex = Bookshelf.Books3D.Items.IndexOf(Book3D);
                if (newDirection == Direction.Left)
                    NextBookIndex--;
                if (newDirection == Direction.Right)
                    NextBookIndex++;

                if (NextBookIndex > 0 && NextBookIndex < Bookshelf.Books3D.Items.Count &&
                    Bookshelf.Books3D.Items.ElementAt(NextBookIndex).Model != null)
                {
                    SwipeMode = SwipeMode.Book;
                    NextBook3D = (Book3DViewModel) Bookshelf.Books3D.Items.ElementAt(NextBookIndex);
                }
                else
                {
                    SwipeMode = SwipeMode.Shelf;
                }
            }

            if (SwipeMode == SwipeMode.Book)
            {
                var book = HBS.CreateBook(NextBook3D);
                openBookTransition = new OpenBookTransition(NextBook3D, book);

                //only left or right would actually open a new book
                if (newDirection != Direction.Center)
                    openBookTransition.OnTransitionStarting();
                activeTransition = openBookTransition;
            }
            if (SwipeMode == SwipeMode.Shelf)
            {
                activeTransition = new ShelfSwipeTransition(HBS.ViewModel.ShelfViewModel);
                HBS.ViewModel.BookFlyout3dVM.Visibility = false;
            }
        }

        public override void OnTransitionStarting()
        {
            HBS.ViewModel.Opened.BookVM.IsBitmapCacheEnabled = true;
            HBS.ViewModel.Opened.BookVM.TransformMatrix = MxM.Identity;
            activeTransition.OnTransitionStarting();
        }

        public override void OnTransitionProgress(double progress)
        {
            var offsetX = HBS.ViewModel.Opened.ActualSize.Width*progress;
            var matrix = MxM.Create(offsetX, 0);
            HBS.ViewModel.Opened.BookVM.TransformMatrix = matrix;
            if (SwipeMode == SwipeMode.Book)
                progress = Math.Abs(progress);
            activeTransition.OnTransitionProgress(progress);
        }

        public override void OnTranistionCompleted()
        {
            HBS.ViewModel.Opened.BookVM.IsBitmapCacheEnabled = false;
            HBS.ViewModel.Opened.BookVM.TransformMatrix = MxM.Identity;
            activeTransition.OnTranistionCompleted();

            if (SwipeMode == SwipeMode.Book)
            {
                if (Direction == Direction.Center)
                {
                    if (LastNextBook3D != null)
                    {
                        LastNextBook3D.Visibility = true;
                        LastNextBook3D.AnimateOpacity(0, 1);
                    }
                }
                else
                {
                    Book3D.Visibility = true;
                    Book3D.AnimateOpacity(0, 1);
                }
            }
            if (SwipeMode == SwipeMode.Shelf)
            {
                Events.OnIdleOnce(() =>
                {
                    HBS.ViewModel.Opened.BookVM.Book = null;
                    HBS.IsAnimating = false;
                    Book3D.Visibility = true;

                    var intent = new Intent(HbsIntents.ACTION_OPEN_BOOK);

                    var selectedShelf = HBS.ViewModel.ShelfViewModel.GetSelectedBookshelf();
                    var index = Bookshelf.RotatedIndex < selectedShelf.RotatedIndex
                        ? 1
                        : selectedShelf.Books3D.Items.Count - 1;
                    var book3D = (Book3DViewModel) selectedShelf.Books3D.Items[index];

                    intent.AddExtra("book3D", book3D);
                    Pici.Intent.Send(intent);
                });
            }
        }

        #region Direction

        private Direction mDirection;

        public Direction Direction
        {
            get { return mDirection; }
            set
            {
                if (mDirection != value)
                {
                    var old = mDirection;
                    mDirection = value;
                    RaisePropertyChanged("Direction", old, value);
                    OnDirectionChanged(old, value);
                }
            }
        }

        #endregion Direction
    }
}