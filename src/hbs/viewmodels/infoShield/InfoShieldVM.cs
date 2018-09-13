// InfoShieldVM.cs
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

using System.Linq;
using picibird.hbs.ldu;
using picibird.hbs.ldu.pages;
using picibird.hbs.viewmodels.shelf;
using picibird.shelfhub;
using picibits.app.animation;
using picibits.core.helper;
using picibits.core.math;
using picibits.core.mvvm;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.infoShield
{
    public class InfoShieldVM : ViewModel
    {
        private static readonly Sorting SortingInstance = new Sorting();

        public InfoShieldVM(ShelfDrawViewModel drawViewModel)
        {
            DrawViewModel = drawViewModel;
            DrawViewModel.PropertyChanged += OnDrawViewModelPropertyChanged;
            Style = new ViewStyle("InfoShieldStyle");
            UpdateSizeAndPosition();
            Sorting.IsSortingEnabledChanged += (value) => IsSortingActivated = value;
        }

        public Sorting Sorting { get; set; } = SortingInstance;

        public ShelfDrawViewModel DrawViewModel { get; }

        private void OnDrawViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("RowHeight"))
            {
                UpdateSizeAndPosition();
            }
        }

        protected override void OnModelChanged(Model oldModel, Model newModel)
        {
            base.OnModelChanged(oldModel, newModel);
            if (oldModel is BookshelfViewModel)
            {
                var bsVM = oldModel as BookshelfViewModel;
                bsVM.PageUpdated -= OnPageUpdated;
            }
            if (newModel is BookshelfViewModel)
            {
                var bsVM = newModel as BookshelfViewModel;
                bsVM.PageUpdated += OnPageUpdated;
            }
        }

        private void OnPageUpdated(BookshelfViewModel sender, Page page)
        {
            UpdateShelfContentInfo(page, Sorting.SelectedSortOrderFunction, Sorting.SelectedSortDirection);
        }

        private void UpdateShelfContentInfo(Page page, SortOrderFunction order, SortDirection direction)
        {
            if (page == null) return;
            Info.Text = null;
            Info.From = null;
            Info.To = null;
            if (page.Hits.Count > 0)
            {
                if (order.EnumValue.Type == SortFieldType.Date)
                {
                    //get first and last valid value
                    var first = page.Hits.FirstOrDefault(hit => order.HasProperty(hit));
                    var last = page.Hits.LastOrDefault(hit => order.HasProperty(hit));
                    //get string representatives
                    var firstString = "";
                    if (first != null)
                        firstString = order.GetRepresentative(first);
                    var lastString = "";
                    if (last != null)
                        lastString = order.GetRepresentative(last);
                    //set info properties
                    Info.From = firstString;
                    Info.To = lastString;
                }
                else
                {
                    Info.Text = HBS.Search.SearchText;
                }
            }
            IsInfoVisible = !string.IsNullOrEmpty(Info.From) || !string.IsNullOrEmpty(Info.To);
        }

        private void UpdateSizeAndPosition()
        {
            Width = DrawViewModel.InfoShieldWidth;
            Height = DrawViewModel.RowHeight - DrawViewModel.ShelfHeight;
            TransformMatrix = MxM.Create(0, 0);
        }

        #region Info

        private ShelfContentInfo mInfo;

        public ShelfContentInfo Info
        {
            get
            {
                if (mInfo == null)
                    mInfo = new ShelfContentInfo();
                return mInfo;
            }
        }

        #endregion Info

        #region IsSortingActivated

        private bool mIsSortingActivated = false;

        public bool IsSortingActivated
        {
            get { return mIsSortingActivated; }
            set
            {
                if (mIsSortingActivated != value)
                {
                    var old = mIsSortingActivated;
                    mIsSortingActivated = value;
                    RaisePropertyChanged(nameof(IsSortingActivated), old, value);
                    Events.OnIdleOnce(() =>
                    {
                        var ani = ArtefactAnimator.AddEase(this, new[] { "SortingUiOpacity" },
                            new object[] { mIsSortingActivated ? 1.0d : 0.0d }, 0.3d);
                        ani.Complete += (s, e) =>
                        {

                        };
                    });
                }
            }
        }

        #endregion IsSortingActivated

        #region SortingUiOpacity

        private double mSortingUiOpacity = 0.0;

        public double SortingUiOpacity
        {
            get { return mSortingUiOpacity; }
            set
            {
                if (mSortingUiOpacity != value)
                {
                    var old = mSortingUiOpacity;
                    mSortingUiOpacity = value;
                    RaisePropertyChanged(nameof(SortingUiOpacity), old, value);
                }
            }
        }

        #endregion SortingUiOpacity

        #region IsInfoVisible

        private bool mIsInfoVisible;

        public bool IsInfoVisible
        {
            get { return mIsInfoVisible; }
            set
            {
                if (mIsInfoVisible != value)
                {
                    var old = mIsInfoVisible;
                    mIsInfoVisible = value;
                    RaisePropertyChanged("IsInfoVisible", old, value);
                }
            }
        }

        #endregion IsInfoVisible
    }
}