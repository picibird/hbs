// BookshelfViewModel.cs
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using picibird.hbs.ldu;
using picibird.hbs.ldu.pages;
using picibird.hbs.viewmodels.infoShield;
using picibits.app.transition;
using picibits.core.mvvm;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.shelf
{
    public class BookshelfViewModel : ViewModel
    {
        public BookshelfViewModel(Bookshelf3DViewModel books3D)
        {
            Books3D = books3D;
            Labels = new BookshelfLabelsViewModel(books3D);
            Style = new ViewStyle("BookshelfStyle");
            Attached.CollectionChanged += OnAttachedCollectionChanged;
            HBS.Search.SearchStarting += OnSearchStarting;
        }

        #region Books3D

        public Bookshelf3DViewModel Books3D { get; }

        #endregion Books3D

        #region Labels

        public BookshelfLabelsViewModel Labels { get; private set; }

        #endregion Labels

        private void UpdatePageIndex()
        {
            //Pici.Log.debug(typeof(BookshelfViewModel), String.Format("{0} -> {1}", PageIndex, rotatedPageIndex));
            if (RotatedIndex >= 0)
            {
                var pageIndex = (int) Math.Round(RotatedIndex);
                if (Page == null || Page.Index != pageIndex)
                {
                    Page = HBS.Search.Pages.RequestPage(pageIndex);
                }
            }
        }

        private void UpdateVisibility()
        {
            if (RotatedIndex < 0)
                Visibility = false;
            else if (RotatedIndex > MaxPageIndex)
            {
                Visibility = false;
            }
            else
            {
                Visibility = true;
            }
        }

        //EaseObject opacityAni;

        //protected override void OnVisibilityChanged(bool oldVisibility, bool newVisibility)
        //{
        //    base.OnVisibilityChanged(oldVisibility, newVisibility);
        //    if (opacityAni != null)
        //        opacityAni.Stop();
        //    if (newVisibility = true)
        //    {
        //        Opacity = 0;
        //        opacityAni = ArtefactAnimator.AddEase(this, new[] { "Opacity" }, new object[] { 1 }, 0.5);
        //    }
        //}

        private void OnSearchStarting(object sender, SearchStartingEventArgs e)
        {
            //reset all model null
            foreach (var vm in Books3D.Items)
                vm.Model = null;

            MaxPageIndex = HBS.Search.Callback.MaxPageIndex;
            UpdateVisibility();
            HBS.Search.Callback.MaxPageIndexChanged += OnMaxPageIndexChanged;
        }

        protected void OnMaxPageIndexChanged(object sender, PropertyChangedEventArgs e)
        {
            MaxPageIndex = HBS.Search.Callback.MaxPageIndex;
            UpdateVisibility();
        }

        public event SimplePropertyChangedHandler<BookshelfViewModel, Page> PageUpdated;

        private void OnPageListUpdated(PiciObservableCollectionWithCancellationToken<Hit> sender)
        {
            if (PageUpdated != null)
            {
                PageUpdated(this, Page);
            }
            //set new models for every hit in Page
            for (var i = 0; i < Page.Hits.Count; i++)
            {
                var item = Page.Hits.ElementAt(i);
                var bookVM = Books3D.Items.ElementAt(i + 1);
                item.CoverPriority = Page.Index;
                //bookVM.Model = item;
                if (bookVM.Model == null || !(bookVM.Model as Hit).recid.Equals(item.recid))
                {
                    bookVM.Model = item;
                }
                else
                {
                    (bookVM.Model as Hit).relevance = item.relevance;
                }
            }
            //reset unset Books in shelf
            for (var i = Page.Hits.Count + 1; i < Books3D.Items.Count; i++)
            {
                var bookVM = Books3D.Items.ElementAt(i);
                bookVM.Model = null;
            }
        }

        protected void OnAttachedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (KeyValuePair<string, object> newItem in e.NewItems)
                {
                    //rotated index
                    if (newItem.Key.Equals(TransitionItemsViewModel.ROTATED_INDEX_ATTACHED_PROPERTY_NAME))
                    {
                        RotatedIndex = GetRotatedIndex();
                        UpdatePageIndex();
                    }
                }
            }
        }

        protected double GetRotatedIndex()
        {
            object attachedRotIdx = null;
            Attached.TryGetValue(TransitionItemsViewModel.ROTATED_INDEX_ATTACHED_PROPERTY_NAME, out attachedRotIdx);
            return Convert.ToDouble(attachedRotIdx);
        }

        #region ShelfDrawViewModel

        private ShelfDrawViewModel mShelfDrawViewModel;

        public ShelfDrawViewModel ShelfDrawViewModel
        {
            get
            {
                if (mShelfDrawViewModel == null)
                    mShelfDrawViewModel = new ShelfDrawViewModel();
                return mShelfDrawViewModel;
            }
        }

        #endregion ShelfDrawViewModel

        #region InfoShield

        private InfoShieldVM mInfoShield;

        public InfoShieldVM InfoShield
        {
            get
            {
                if (mInfoShield == null)
                {
                    mInfoShield = new InfoShieldVM(ShelfDrawViewModel);
                    mInfoShield.Model = this;
                }
                return mInfoShield;
            }
        }

        #endregion InfoShield

        #region Page

        private Page mPage;

        public Page Page
        {
            get { return mPage; }
            set
            {
                var old = mPage;
                mPage = value;
                OnPageChanged(old, value);
            }
        }

        protected virtual void OnPageChanged(Page oldPage, Page newPage)
        {
            RaisePropertyChanged("Page", oldPage, newPage);
            if (oldPage != null)
            {
                oldPage.Hits.ListUpdated -= OnPageListUpdated;
                HBS.Search.Pages.ReleasePage(oldPage);
            }
            if (newPage != null)
            {
                newPage.Hits.ListUpdated += OnPageListUpdated;
            }
            OnPageListUpdated(null);
        }

        #endregion Page

        #region RotatedIndex

        private double mRotatedIndex;

        public double RotatedIndex
        {
            get { return mRotatedIndex; }
            set
            {
                if (mRotatedIndex != value)
                {
                    var old = mRotatedIndex;
                    mRotatedIndex = value;
                    RaisePropertyChanged("RotatedIndex", old, value);
                    UpdateVisibility();
                }
            }
        }

        #endregion RotatedIndex

        #region MaxPageIndex

        private int mMaxPageIndex;

        public int MaxPageIndex
        {
            get { return mMaxPageIndex; }
            set
            {
                if (mMaxPageIndex != value)
                {
                    var old = mMaxPageIndex;
                    mMaxPageIndex = value;
                    RaisePropertyChanged("MaxPageIndex", old, value);
                }
            }
        }

        #endregion MaxPageIndex
    }
}