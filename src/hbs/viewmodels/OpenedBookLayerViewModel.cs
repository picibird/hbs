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
using picibird.hbs.viewmodels.book;
using picibits.core;
using picibits.core.models;
using picibits.core.mvvm;

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

        #region LeftLayer

        private ViewModel mLeftLayer;

        public ViewModel LeftLayer
        {
            get
            {
                if (mLeftLayer == null)
                    mLeftLayer = new ViewModel();
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

        private ViewModel mRightLayer;

        public ViewModel RightLayer
        {
            get
            {
                if (mRightLayer == null)
                    mRightLayer = new ViewModel();
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
}