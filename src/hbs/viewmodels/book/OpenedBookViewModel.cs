// OpenedBookViewModel.cs
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

using picibird.hbs.models;
using picibird.hbs.viewmodels.book3D;

using picibits.app.animation;
using picibits.core;
using picibits.core.helper;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.book
{
    public class OpenedBookViewModel : ViewModel
    {
        private EaseObject DropShadowAnimation;

        public OpenedBookViewModel()
        {
            Visibility = false;
            Style = new ViewStyle("OpenedBookStyle");
        }

        private void UpdateModels(Book book)
        {
            if (book != null)
            {
                FrontCover.Model = book.Hit;
                Spine.Model = book.Hit;
                Spine.VisualState = "Open";
                BackCover.Model = book.Hit;
                if (ButtonVM != null) ButtonVM.Model = book.Hit;
            }
            else
            {
                FrontCover.Model = null;
                Spine.Model = null;
                Spine.VisualState = "Closed";
                BackCover.Model = null;
                if (ButtonVM != null) ButtonVM.Model = null;
            }
        }

        public void AnimateToDropShadow()
        {
            if (DropShadowAnimation != null)
                DropShadowAnimation.Stop();
            if (DropShadowOpacity < 1)
            {
                Events.OnIdleOnce(
                    () =>
                    {
                        DropShadowAnimation = ArtefactAnimator.AddEase(this, new[] {"DropShadowOpacity"},
                            new object[] {1.0}, 0.25d);
                    });
            }
        }

        private void UpdateVisibility(Book book)
        {
            Visibility = book != null;
        }

        private void UpdatePositionAndSize(Book book)
        {
            double coverWidth = 0;
            double spineWidth = 0;
            double coverHeight = 0;

            if (book != null)
            {
                coverWidth = book.CoverWidth;
                spineWidth = book.SpineWidth;
                coverHeight = book.CoverHeight;
            }

            BackCover.Width = coverWidth;
            BackCover.Height = coverHeight;

            Spine.Width = spineWidth;
            Spine.Height = coverHeight;

            FrontCover.Width = coverWidth;
            FrontCover.Height = coverHeight;

            Height = coverHeight;
            Width = coverWidth + spineWidth + coverWidth;
        }

        #region Book

        private Book mBook;

        public Book Book
        {
            get { return mBook; }
            set
            {
                if (mBook == null && value != null ||
                    mBook != null && value == null ||
                    value != null && !mBook.Hit.recid.Equals(value.Hit.recid))
                {
                    var old = mBook;
                    mBook = value;
                    OnSelectedBook3DChanged(old, value);
                }
            }
        }

        protected virtual void OnSelectedBook3DChanged(Book oldBook, Book newBook)
        {
            RaisePropertyChanged("Book", oldBook, newBook);
            if (oldBook != null)
            {
                UpdateVisibility(null);
                UpdateModels(null);
                UpdatePositionAndSize(null);
            }
            if (newBook != null)
            {
                UpdateVisibility(newBook);
                UpdateModels(newBook);
                UpdatePositionAndSize(newBook);
            }
        }

        #endregion Book

        #region Book3D

        private Book3DViewModel mBook3D;

        public Book3DViewModel Book3D
        {
            get { return mBook3D; }
            set
            {
                if (mBook3D != value)
                {
                    var old = mBook3D;
                    mBook3D = value;
                    RaisePropertyChanged("Book3D", old, value);
                }
            }
        }

        #endregion Book3D

        #region BackCover

        private BackCoverViewModel mBackCover;

        public BackCoverViewModel BackCover
        {
            get
            {
                if (mBackCover == null)
                    mBackCover = new BackCoverViewModel();
                return mBackCover;
            }
        }

        #endregion BackCover

        #region Spine

        private SpineViewModel mSpine;

        public SpineViewModel Spine
        {
            get
            {
                if (mSpine == null)
                    mSpine = Pici.Factory.Create<SpineViewModel>();
                return mSpine;
            }
        }

        #endregion Spine

        #region FrontCover

        private ViewModel mFrontCover;

        public ViewModel FrontCover
        {
            get
            {
                if (mFrontCover == null)
                    mFrontCover = new ViewModel();
                return mFrontCover;
            }
        }

        #endregion FrontCover

        #region DropShadowOpacity

        private double mDropShadowOpacity;

        public double DropShadowOpacity
        {
            get { return mDropShadowOpacity; }
            set
            {
                if (mDropShadowOpacity != value)
                {
                    var old = mDropShadowOpacity;
                    mDropShadowOpacity = value;
                    RaisePropertyChanged("DropShadowOpacity", old, value);
                }
            }
        }

        #endregion DropShadowOpacity

        #region ButtonVM

        private ViewModel mButtonVM;

        public ViewModel ButtonVM
        {
            get { return mButtonVM; }
            set
            {
                if (mButtonVM != value)
                {
                    var old = mButtonVM;
                    mButtonVM = value;
                    RaisePropertyChanged("ButtonVM", old, value);
                }
            }
        }

        #endregion ButtonVM
    }
}