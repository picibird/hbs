// OpenedBookLayerViewModel.cs
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

using picibird.hbs.behaviours;
using picibird.hbs.ldu;
using picibird.hbs.viewmodels.book;
using picibits.app.animation;
using picibits.app.behaviour;
using picibits.bib;
using picibits.core;
using picibits.core.helper;
using picibits.core.models;
using picibits.core.mvvm;
using System;

namespace picibird.hbs.viewmodels
{
    public class OpenedBookLayerViewModel : ViewModel
    {
        public OpenedBookLayerViewModel()
        {
            Visibility = false;
            Style = new ViewStyle("OpenedBookLayerStyle");

            BookSwipeBehaviour = new OpenBookSwipeBehaviour();
            Behaviours.Add(BookSwipeBehaviour);

            LeftLayer.Style = new ViewStyle("LeftLayerStyle");
            TopLayer.Style = new ViewStyle("TopLayerStyle");
            RightLayer.Style = new ViewStyle("RightLayerStyle");
            BottomLayer.Style = new ViewStyle("BottomLayerStyle");

            BookSwipeBehaviour.Starting += OnBookSwipeBehaviourStarting;
            BookVM.BookChanged += OnBookChanged;
        }

        private void OnBookChanged(models.Book oldBook, models.Book newBook)
        {
            var hit = newBook?.Hit;
            LeftLayer.Hit = hit;
            RightLayer.Hit = hit;
            RightLayer.IsEnabled = hit != null;
            LeftLayer.IsEnabled = hit != null;
        }

        private void OnBookSwipeBehaviourStarting(transition.OpenBookSwipeTransition transition)
        {
            transition.ProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(object sender, double e)
        {
            if (e == 0 || Math.Abs(e) == 1)
            {
                RightLayer.IsEnabled = true;
                LeftLayer.IsEnabled = true;
            }
            else
            {
                RightLayer.IsEnabled = false;
                LeftLayer.IsEnabled = false;
            }
        }

        public OpenBookSwipeBehaviour BookSwipeBehaviour { get; }

        public void UpdatePositionAndSize(Rect OpenBookRect)
        {
            LeftLayer.Width = OpenBookRect.X;
            LeftLayer.Height = ActualSize.Height;

            RightLayer.Width = ActualSize.Width - OpenBookRect.X - OpenBookRect.Width;
            RightLayer.Height = ActualSize.Height;

            var topBottomWidth = ActualSize.Width - LeftLayer.Width - RightLayer.Width;

            TopLayer.Width = topBottomWidth;
            TopLayer.Height = OpenBookRect.Y;

            BottomLayer.Width = topBottomWidth;
            BottomLayer.Height = ActualSize.Height - OpenBookRect.Y - OpenBookRect.Height;
        }

        private void OnLeftTap(object sender, System.EventArgs e)
        {
            HBS.ViewModel.IsHitTestVisible = false;
            BookSwipeBehaviour.OnSwipeStarting(5, 55);
            BookSwipeBehaviour.OnSwipeDelta(5, 55);
            BookSwipeBehaviour.OnSwipeCompleted(5, 55, () =>
            {
                HBS.ViewModel.IsHitTestVisible = true;
            });
        }

        private void OnRightTap(object sender, System.EventArgs e)
        {
            HBS.ViewModel.IsHitTestVisible = false;
            BookSwipeBehaviour.OnSwipeStarting(-5, -55);
            BookSwipeBehaviour.OnSwipeDelta(-5, -55);
            BookSwipeBehaviour.OnSwipeCompleted(-5, -55, () =>
            {
                HBS.ViewModel.IsHitTestVisible = true;
            });
        }

        #region LeftLayer

        private LeftRightVM mLeftLayer;

        public LeftRightVM LeftLayer
        {
            get
            {
                if (mLeftLayer == null)
                {
                    mLeftLayer = new LeftRightVM();
                    TapBehaviour tap = new TapBehaviour();
                    mLeftLayer.Behaviours.Add(tap);
                    mLeftLayer.Pointing.IsEnabled = true;
                    tap.Tap += OnLeftTap;
                }
                return mLeftLayer;
            }
        }

        #endregion LeftLayer

        #region TopLaywer

        private ViewModel mTopLaywer;

        public ViewModel TopLayer
        {
            get
            {
                if (mTopLaywer == null)
                    mTopLaywer = new ViewModel();
                return mTopLaywer;
            }
        }

        #endregion TopLaywer

        #region RightLayer

        private LeftRightVM mRightLayer;

        public LeftRightVM RightLayer
        {
            get
            {
                if (mRightLayer == null)
                {
                    mRightLayer = new LeftRightVM();
                    TapBehaviour tap = new TapBehaviour();
                    mRightLayer.Behaviours.Add(tap);
                    mRightLayer.Pointing.IsEnabled = true;
                    tap.Tap += OnRightTap;

                }
                return mRightLayer;
            }
        }



        #endregion RightLayer

        #region BottomLayer

        private ViewModel mBottomLayer;

        public ViewModel BottomLayer
        {
            get
            {
                if (mBottomLayer == null)
                    mBottomLayer = new ViewModel();
                return mBottomLayer;
            }
        }

        #endregion BottomLayer

        #region OpenedBook

        private OpenedBookViewModel mBookVM;

        public OpenedBookViewModel BookVM
        {
            get
            {
                if (mBookVM == null)
                    return mBookVM = Pici.Factory.Create<OpenedBookViewModel>();
                return mBookVM;
            }
        }

        #endregion OpenedBook
    }

    public class LeftRightVM : ViewModel
    {

        #region HistomatColorScheme

        private HistomatColorScheme mCoverColorScheme = HistomatColorScheme.DEFAULT;

        public HistomatColorScheme CoverColorScheme
        {
            get { return mCoverColorScheme; }
            set
            {
                if (mCoverColorScheme != value)
                {
                    var old = mCoverColorScheme;
                    mCoverColorScheme = value;
                    RaisePropertyChanged("CoverColorScheme", old, value);
                }
            }
        }

        #endregion HistomatColorScheme


        #region Hit

        private Hit mHit;

        public Hit Hit
        {
            get { return mHit; }
            set
            {
                if (mHit != value)
                {
                    var old = mHit;
                    mHit = value;
                    RaisePropertyChanged("Hit", old, value);
                    OnHitChanged();
                }
            }
        }

        #endregion Hit

        public LeftRightVM() : base()
        {
            IsEnabled = false;
        }

        public void OnHitChanged()
        {
            if (Hit != null)
            {
                CoverColorScheme = Hit.CoverColorScheme;
            }
            else
            {
                CoverColorScheme = HistomatColorScheme.DEFAULT;
            }
        }

        public override void RaisePropertyChanged(string name, object oldValue = null, object newValue = null)
        {
            base.RaisePropertyChanged(name, oldValue, newValue);
            if (name.Equals(nameof(ViewModel.IsEnabled)))
            {
                if (IsEnabled)
                {
                    Events.OnIdleOnce(() =>
                    {
                        var ease = AnimationTransitions.CubicEaseIn;
                        IsHitTestVisible = true;
                        ArtefactAnimator.AddEase(this, "Opacity", 1.0d, 0.3, ease, 0.2)
                        .Complete += (obj, percent) =>
                        {
                        };
                    });
                }
                else
                {
                    IsHitTestVisible = false;
                    ArtefactAnimator.AddEase(this, "Opacity", 0.0d, 0.3);
                }
            }
        }

    }

}