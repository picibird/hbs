// BookFlyout3dVM.cs
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
using picibits.core.controls3D;
using picibits.core.helper;
using picibits.core.math3D;
using picibits.core.models;

namespace picibird.hbs.viewmodels.shelf
{
    public class BookFlyout3dVM : ItemsViewport3DModel
    {
        private readonly Vector3D xAxis = new Vector3D(1, 0, 0);
        private readonly Vector3D yAxis = new Vector3D(0, 1, 0);
        private readonly Vector3D zAxis = new Vector3D(0, 0, 1);

        public OpenedBook3DViewModel OpenedBook;

        private readonly Quaternion openedBookTargetQuaternion = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
        private Quaternion SelectedBookRotation3D;

        private Vector3D SelectedBookScale3D;
        private Vector3D SelectedBookTranslation3D;

        public BookFlyout3dVM()
        {
            OpenedBook = new OpenedBook3DViewModel();
            OpenedBook.CreatBookSides();
            Items.Add(OpenedBook);
        }

        public void Recreate3D()
        {
            Items.Remove(OpenedBook);
            OpenedBook = new OpenedBook3DViewModel();
            OpenedBook.CreatBookSides();
            Items.Add(OpenedBook);
        }

        protected virtual void OnBookChanged(Book oldBook, Book newBook)
        {
            RaisePropertyChanged("Book", oldBook, newBook);
            if (oldBook != null)
            {
                OpenedBook.Model = null;
            }
            if (newBook != null)
            {
                OpenedBook.Model = newBook.Hit;
            }
        }

        protected virtual void OnProgressChanged(double oldProgress, double newProgress)
        {
            RaisePropertyChanged("Progress", oldProgress, newProgress);
            OpenedBook.OpenedProgress = newProgress;
            UpdateOpenedBookTransform((float)newProgress);
        }


        private void UpdateTransformValues()
        {
            ClosedBook3dMatrix.Decompose(out SelectedBookScale3D, out SelectedBookRotation3D,
                out SelectedBookTranslation3D);
            UpdateOpenedBookRect();
        }

        private void UpdateOpenedBookRect()
        {
            if (Book != null)
            {
                var openedBookWidth = (float)(2 * Book.CoverWidth + Book.SpineWidth);
                var openedBookHeight = (float)Book.CoverHeight;

                var targetX = (float)((ActualSize.Width - openedBookWidth) * 0.5);
                var targetY = (float)((ActualSize.Height - openedBookHeight) * 0.5);

                OpenedBookRect = new Rect(targetX, targetY, openedBookWidth, openedBookHeight);
            }
        }

        private void UpdateOpenedBookTransform(float progress)
        {
            if (OpenedBookRect == null) return;
            //scale
            var openedProgress = 1 - progress;
            var openedScale = MathUtility.Lerp(SelectedBookScale3D.X, 1, openedProgress);
            var scaleMatrix = Matrix3D.CreateScale(openedScale);

            //rotation
            var openedBookQuaternion = Matrix3DHelper.Slerp(SelectedBookRotation3D, openedBookTargetQuaternion,
                openedProgress, true);
            var rotationM3D = Matrix3D.CreateFromQuaternion(openedBookQuaternion);

            //translation
            var openedX = MathUtility.Lerp(SelectedBookTranslation3D.X, OpenedBookRect.X + (float)Book.CoverWidth,
                openedProgress);
            var openedY = MathUtility.Lerp(SelectedBookTranslation3D.Y, OpenedBookRect.Y, openedProgress);
            var translationMatrix = Matrix3D.CreateTranslation(openedX, openedY, 0);

            OpenedBook.TransformMatrix3D = rotationM3D * scaleMatrix * translationMatrix;
        }

        #region OpenedBookProgress

        private double mProgress;

        public double Progress
        {
            get { return mProgress; }
            set
            {
                if (mProgress != value)
                {
                    var old = mProgress;
                    mProgress = value;
                    OnProgressChanged(old, value);
                }
            }
        }

        #endregion OpenedBookProgress

        #region Book

        private Book mBook;

        public Book Book
        {
            get { return mBook; }
            set
            {
                if (mBook != value)
                {
                    var old = mBook;
                    mBook = value;
                    OnBookChanged(old, value);
                    UpdateTransformValues();
                }
            }
        }

        #endregion Book

        #region ClosedBook3dMatrix

        private Matrix3D mClosedBook3dMatrix;

        public Matrix3D ClosedBook3dMatrix
        {
            get { return mClosedBook3dMatrix; }
            set
            {
                if (mClosedBook3dMatrix != value)
                {
                    var old = mClosedBook3dMatrix;
                    mClosedBook3dMatrix = value;
                    RaisePropertyChanged("ClosedBook3dMatrix", old, value);
                    UpdateTransformValues();
                }
            }
        }

        #endregion ClosedBook3dMatrix

        #region OpenedBookRect

        private Rect mOpenedBookRect;

        public Rect OpenedBookRect
        {
            get { return mOpenedBookRect; }
            set
            {
                if (mOpenedBookRect != value)
                {
                    var old = mOpenedBookRect;
                    mOpenedBookRect = value;
                    RaisePropertyChanged("OpenedBookRect", old, value);
                }
            }
        }

        #endregion OpenedBookRect
    }
}