// BookshelfLabelsViewModel.cs
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
using picibird.hbs.config;
using picibird.hbs.ldu;
using picibird.hbs.viewmodels.book3D;
using picibird.hbs.viewmodels.infoShield;
using picibits.core.controls;
using picibits.core.export.instances;
using picibits.core.helper;
using picibits.core.math;
using picibits.core.models;
using picibits.core.mvvm;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace picibird.hbs.viewmodels.shelf
{
    public class BookshelfLabelsViewModel : ItemsViewModel
    {
        public BookshelfLabelsViewModel(Bookshelf3DViewModel bookShelf3D)
        {
            Bookshelf3D = bookShelf3D;
            Style = new ViewStyle("ShelfLabelItemsViewStyle");
            CreateLabels();
        }

        #region Bookshelf3D

        public Bookshelf3DViewModel Bookshelf3D { get; }

        #endregion Bookshelf3D

        public void CreateLabels()
        {
            var rows = HBS.RowCount;
            var columns = HBS.ColumnCount;
            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var book3D = Bookshelf3D.GetBook(x, y);
                    var label = new LabelViewModel(x, y, book3D);
                    Items.Add(label);
                }
            }
        }
    }

    public class Availability
    {
        public int Available { get; set; }
        public int Existing { get; set; }
    }

    public class LabelViewModel : ViewModel
    {
        public LabelViewModel(int x, int y, Book3DViewModel book3D)
        {
            Visibility = false;
            X = x;
            Y = y;
            Book3D = book3D;
            Book3D.PropertyChanged += OnBook3DPropertyChanged;
            Style = new ViewStyle("ShelfLabelStyle");
            //Search.Instance.SearchStarting += OnSearchStarting;
        }

        #region Book3D

        public Book3DViewModel Book3D { get; }

        #endregion Book3D

        #region X

        public int X { get; private set; }

        #endregion X

        #region Y

        public int Y { get; private set; }

        #endregion Y

        //private void OnSearchStarting(object sender, SearchStartingEventArgs e)
        //{
        //    Search.Instance.SearchRequest.SortingChanged += SearchRequest_SortingChanged;
        //}

        private void SearchRequest_SortingChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnModelChanged(Model oldModel, Model newModel)
        {
            base.OnModelChanged(oldModel, newModel);
            if (oldModel != null)
            {
                var hit = oldModel as Hit;
                hit.PropertyChanged -= OnModelHitPropertyChanged;
                Medium = "";
                IsOnlineAvailable = false;
                Availability = null;
                Visibility = false;
            }
            if (newModel != null)
            {
                var hit = newModel as Hit;
                hit.PropertyChanged += OnModelHitPropertyChanged;
                Medium = GetMedium(hit);
                IsOnlineAvailable = GetIsOnlineAvailable(hit);
                Availability = GetAvailability(hit);
                Visibility = true;
                Info = UpdateInfo(hit);
            }
        }

        private void OnModelHitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("relevance"))
            {
                if (Model is Hit)
                    Info = UpdateInfo(Model as Hit);
            }
        }

        private string UpdateInfo(Hit hit)
        {
            var searchRequest = HBS.Search.SearchRequest;
            var sortOrder = HBS.Search.SearchRequest.SortOrder;
            SortOrderFunction sortOrderFunction = null;
            if (sortOrder == SortOrder.relevance)
                sortOrderFunction = SortOrderFunction.GetSingleton(SortOrder.date);
            else
                sortOrderFunction = SortOrderFunction.GetSingleton(sortOrder);
            return sortOrderFunction.GetRepresentative(hit);
        }

        private string GetMedium(Hit hit)
        {
            return hit.medium;
        }

        private bool GetIsOnlineAvailable(Hit hit)
        {
            return hit.medium == null ? false : hit.medium.Equals("ebook");
        }

        private Availability GetAvailability(Hit hit)
        {
            var medium = hit.medium?.RemoveWhitespace();
            Availability availability = null;
            if (medium != null)
            {
                if (medium.Equals("ebook"))
                {
                    availability = null;
                }
                if (medium.Equals("book") || medium.Equals("video"))
                {
                    var loc = hit.GetLocationWithSource(Sources.SWB);
                    if (loc != null)
                    {
                        availability = new Availability {Available = loc.countAvailable, Existing = loc.countExisting};
                    }
                }
            }
            return availability;
        }

        private void OnAvailabilityChanged(Availability av)
        {
            if (av == null)
                AvailabilityIconWidth = 22;
            else
            {
                if (av.Available < 10 && av.Existing < 10)
                    AvailabilityIconWidth = 22;
                else
                    AvailabilityIconWidth = 36;
                if (av.Available > 99 || av.Existing > 99)
                    AvailabilityIconWidth = 48;
            }
        }

        private void OnBook3DPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Bounds2D"))
            {
                UpdatePosition(Book3D.Bounds2D);
            }
            if (e.PropertyName.Equals("Model"))
            {
                Model = Book3D.Model;
            }
        }

        protected override void OnActualSizeChanged(Size oldActualSize, Size newActualSize)
        {
            base.OnActualSizeChanged(oldActualSize, newActualSize);
            if (Book3D.Bounds2D != null)
                UpdatePosition(Book3D.Bounds2D);
        }

        private void UpdatePosition(Rect bookBounds)
        {
            var x = bookBounds.X + 6 + (bookBounds.Width - ActualSize.Width)*0.5;
            var y = bookBounds.Y + bookBounds.Height + 18 + Config.Shelf3D.ShelfBoardHeight;
            var transform = MxM.Create(x, y);
            TransformMatrix = transform;
        }

        #region Medium

        private string mMedium;

        public string Medium
        {
            get { return mMedium; }
            set
            {
                if (mMedium != value)
                {
                    var old = mMedium;
                    mMedium = value;
                    RaisePropertyChanged("Medium", old, value);
                }
            }
        }

        #endregion Medium

        #region Availability

        private Availability mAvailability;

        public Availability Availability
        {
            get { return mAvailability; }
            set
            {
                if (mAvailability != value)
                {
                    var old = mAvailability;
                    mAvailability = value;
                    RaisePropertyChanged("Availability", old, value);
                    OnAvailabilityChanged(value);
                }
            }
        }

        #endregion Availability

        #region AvailabilityIconWidth

        private double mAvailabilityIconWidth = 22;

        public double AvailabilityIconWidth
        {
            get { return mAvailabilityIconWidth; }
            set
            {
                if (mAvailabilityIconWidth != value)
                {
                    var old = mAvailabilityIconWidth;
                    mAvailabilityIconWidth = value;
                    RaisePropertyChanged("AvailabilityIconWidth", old, value);
                }
            }
        }

        #endregion AvailabilityIconWidth

        #region Info

        private string mInfo;

        public string Info
        {
            get { return mInfo; }
            set
            {
                if (mInfo != value)
                {
                    var old = mInfo;
                    mInfo = value;
                    RaisePropertyChanged("Info", old, value);
                }
            }
        }

        #endregion Info

        #region IsOnlineAvailable

        private bool mIsOnlineAvailable;

        public bool IsOnlineAvailable
        {
            get { return mIsOnlineAvailable; }
            set
            {
                if (mIsOnlineAvailable != value)
                {
                    var old = mIsOnlineAvailable;
                    mIsOnlineAvailable = value;
                    RaisePropertyChanged("IsOnlineAvailable", old, value);
                }
            }
        }

        #endregion IsOnlineAvailable
    }
}