// Book3DViewModel.cs
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
using picibird.hbs.config;
using picibird.hbs.helper;
using picibird.hbs.ldu;
using picibird.hbs.viewmodels.book;

using picibits.app.animation;
using picibits.app.behaviour;
using picibits.app.bitmap;
using picibits.core;
using picibits.core.controls3D;
using picibits.core.export.instances;
using picibits.core.helper;
using picibits.core.math3D;
using picibits.core.math3D.Objects3D;
using picibits.core.models;
using picibits.core.mvvm;
using picibits.core.util;
using Interpolation = picibits.app.animation.interpolation.Interpolation;

namespace picibird.hbs.viewmodels.book3D
{
    public class Book3DViewModel : ItemsView3DModel
    {
        private readonly float HalfPi = MathUtility.PI_OVER_2;

        private readonly float Pi = MathUtility.PI;

        private readonly Vector3D xAxis = new Vector3D(1, 0, 0);
        private readonly Vector3D yAxis = new Vector3D(0, 1, 0);
        private readonly Vector3D zAxis = new Vector3D(0, 0, 1);

        private EaseObject activeOpacityAnimation;

        private readonly Random R = new Random();

        public Book3DViewModel()
        {
            Pointing.IsEnabled = true;
            OpenedProgress = 1d;
            Visibility = false;
            CoverWidth = Config.Shelf3D.DefaultCoverWidth;
            CoverHeight = Config.Shelf3D.DefaultCoverHeight;
            SpineWidth = Config.Shelf3D.DefaultSpineWidth;
        }

        public double CoverWidth { get; private set; }
        public double CoverHeight { get; private set; }
        public double SpineWidth { get; private set; }

        public void AnimateOpacity(double from, double to)
        {
            if (Visibility)
            {
                FrontCover3D.AnimateOpacity(from, to);
                Spine3D.AnimateOpacity(from, to);
                BackCover3D.AnimateOpacity(from, to);
                TopBackCoverPages3D.AnimateOpacity(from, to);
                TopSpinePages3D.AnimateOpacity(from, to);
                TopFrontCoverPages3D.AnimateOpacity(from, to);
                InnerPageRight3D.AnimateOpacity(from, to);
                //InnerPageLeft3D.AnimateOpacity(from, to);
            }
        }

        protected override void OnModelChanged(Model oldModel, Model newModel)
        {
            base.OnModelChanged(oldModel, newModel);

            if (oldModel != null)
            {
                var hit = oldModel as Hit;
                FrontCover.Model = null;
                Spine.Model = null;
                BackCover.Model = null;
                if (Pici.Settings.Get<CoverSettings>().IsLoadCoverEnabled == BoolEnum.Yes)
                {
                    OnHitCoverImageChanged(hit, null);
                    hit.CoverImageChanged -= OnHitCoverImageChanged;
                    hit.CoverImageUrl = null;
                    //hit.CoverImageUrl = null;
                }
            }
            if (newModel != null)
            {
                var hit = newModel as Hit;
                UpdateSpineWidthFromPages(hit.pages_numberInt);
                FrontCover.Model = newModel;
                Spine.Model = newModel;
                BackCover.Model = newModel;
                Visibility = true;
                if (Pici.Settings.Get<CoverSettings>().IsLoadCoverEnabled == BoolEnum.Yes)
                {
                    OnHitCoverImageChanged(hit, hit.CoverImage);
                    hit.CoverImageChanged += OnHitCoverImageChanged;
                    hit.CoverImageUrl = hit.CoverUrl_L;
                }
            }
            else
            {
                Visibility = false;
            }
        }

        protected override void OnVisibilityChanged(bool oldVisibility, bool newVisibility)
        {
            base.OnVisibilityChanged(oldVisibility, newVisibility);
            //UpdateOpacity(newVisibility);
        }

        protected void UpdateOpacity(bool visibility)
        {
            if (activeOpacityAnimation != null)
                activeOpacityAnimation.Stop();
            if (visibility == false)
                Opacity = 0;
            else
            {
                var seconds = 0.5;
                activeOpacityAnimation = ArtefactAnimator.AddEase(this, new[] {"Opacity"}, new object[] {1}, seconds);
                activeOpacityAnimation.Complete += (easeObj, percent) => { activeOpacityAnimation = null; };
            }
        }

        public virtual void CreatBookSides()
        {
            //Front Cover
            FrontCover.Name = Name;
            FrontCover.Style = new ViewStyle("GenericFrontCoverStyle");
            FrontCover3D.Content = FrontCover;
            Items.Add(FrontCover3D);


            //Spine
            Spine.Name = "Spine";
            Spine.Style = new ViewStyle("SpineWithoutContentStyle");
            Spine3D.Content = Spine;
            Items.Add(Spine3D);


            //Back Cover
            BackCover.Name = "BackCover";
            BackCover.Style = new ViewStyle("CoverDebugStyle");
            BackCover3D.Content = BackCover;
            Items.Add(BackCover3D);

            //Top Back
            TopBackCoverPages.Name = "TopBackPages";
            TopBackCoverPages.Style = new ViewStyle("EdgePagesStyle");
            TopBackCoverPages3D.Content = TopBackCoverPages;
            Items.Add(TopBackCoverPages3D);

            //Top Spine
            TopSpinePages.Name = "TopBackPages";
            TopSpinePages.Style = new ViewStyle("EdgePagesStyle");
            TopSpinePages3D.Content = TopSpinePages;
            Items.Add(TopSpinePages3D);

            //Top Front
            TopFrontCoverPages.Name = "TopBackPages";
            TopFrontCoverPages.Style = new ViewStyle("EdgePagesStyle");
            TopFrontCoverPages3D.Content = TopFrontCoverPages;
            Items.Add(TopFrontCoverPages3D);

            //inner page right
            InnerPageRight.Name = "InnerPagesRight";
            InnerPageRight.Style = new ViewStyle("CoverDebugStyle");
            InnerPageRight3D.Content = InnerPageRight;
            Items.Add(InnerPageRight3D);

            //inner page right
            //InnerPageLeft.Name = "InnerPagesLeft";
            //InnerPageLeft.Style = new ViewStyle("CoverDebugStyle");
            //InnerPageLeft3D.Content = InnerPageLeft;
            //Items.Add(InnerPageLeft3D);

            //Top Front
            //RightFrontCoverPages.Title = "TopSpinePages";
            //RightFrontCoverPages.Name = "TopBackPages";
            //RightFrontCoverPages.Style = new ViewStyle("EdgePagesStyle");
            //RightFrontCoverPages.Width = SpineWidth * 0.5;
            //RightFrontCoverPages.Height = CoverHeight;
            //RightFrontCoverPages3D.Content = RightFrontCoverPages;
            //Items.Add(RightFrontCoverPages3D);

            UpdateBookSizeRandomized(CoverWidth, CoverHeight, SpineWidth);
        }

        public void UpdateBookSizeRandomized(double coverWidth, double coverHeight, double spineWidth = 0)
        {
            var min = 0.8d;
            var max = 1.2d;
            var rx = (double) Interpolation.Double.Interpolate(min, max, R.NextDouble());
            var ry = (double) Interpolation.Double.Interpolate(min, max, R.NextDouble());

            //Pici.Log.info(typeof(Book3DViewModel), String.Format("rx = {0}, ry={1}", rx, ry));

            //UpdateBookSize(rx * coverWidth, ry * coverHeight, spineWidth);
            UpdateBookSize(coverWidth, coverHeight, spineWidth);
        }

        public void UpdateBookSize(double coverWidth, double coverHeight, double spineWidth = 0)
        {
            if (coverWidth == 0)
                coverWidth = CoverWidth;
            if (coverHeight == 0)
                coverHeight = CoverHeight;
            if (spineWidth == 0)
                spineWidth = SpineWidth;

            CoverWidth = coverWidth;
            CoverHeight = coverHeight;
            SpineWidth = spineWidth;

            FrontCover.Width = coverWidth;
            FrontCover.Height = coverHeight;

            Spine.Width = spineWidth;
            Spine.Height = coverHeight;

            BackCover.Width = coverWidth;
            BackCover.Height = coverHeight;

            TopFrontCoverPages.Width = coverWidth;
            TopFrontCoverPages.Height = spineWidth*0.5;

            TopSpinePages.Width = spineWidth;
            TopSpinePages.Height = spineWidth*0.5;

            TopBackCoverPages.Width = coverWidth;
            TopBackCoverPages.Height = spineWidth*0.5;

            InnerPageRight.Width = coverWidth;
            InnerPageRight.Height = coverHeight;

            //InnerPageLeft.Width = coverWidth;
            //InnerPageLeft.Height = coverHeight;

            UpdateBookPlaneTransforms(coverWidth, coverHeight, spineWidth);
        }


        public event SimpleEventHandler<Book3DViewModel> TransformsUpdated;

        protected virtual void OnOpenedProgressChanged(double progress)
        {
            UpdateBookPlaneTransforms();
        }

        public void UpdateBookPlaneTransforms(double coverWidth = 0, double coverHeight = 0, double spineWidth = 0)
        {
            if (coverWidth == 0)
                coverWidth = CoverWidth;
            if (coverHeight == 0)
                coverHeight = CoverHeight;
            if (spineWidth == 0)
                spineWidth = SpineWidth;

            float x, y, rotX, rotY;
            Point3D rotCenter;
            Quaternion rotQuartY;
            Matrix3D rotMatrixX, rotMatrixY, translationMatrix;

            //front cover
            x = (float) spineWidth;
            rotY = (float) (OpenedProgress*HalfPi);
            rotMatrixY = Matrix3D.CreateRotationY(rotY);
            translationMatrix = Matrix3D.CreateTranslation(new Point3D(x, 0, 0));
            FrontCover3D.TransformMatrix3D = rotMatrixY*translationMatrix;
            //spine
            Spine3D.TransformMatrix3D = Matrix3D.Identity;
            //back cover
            x = (float) -coverWidth;
            rotY = (float) (-OpenedProgress*HalfPi);
            rotQuartY = Quaternion.CreateFromAxisAngle(yAxis, rotY);
            rotCenter = new Point3D((float) coverWidth, 0, 0);
            rotMatrixY = Matrix3DHelper.CreateRotationMatrix(ref rotQuartY, ref rotCenter);
            translationMatrix = Matrix3D.CreateTranslation(new Point3D(x, 0, 0));
            BackCover3D.TransformMatrix3D = rotMatrixY*translationMatrix;
            //top back cover
            x = (float) -coverWidth;
            y = (float) coverHeight;
            rotX = -HalfPi;
            rotY = (float) (-OpenedProgress*HalfPi);
            rotCenter = new Point3D((float) coverWidth, 0, 0);
            rotQuartY = Quaternion.CreateFromAxisAngle(yAxis, rotY);
            rotMatrixX = Matrix3D.CreateRotationX(rotX);
            rotMatrixY = Matrix3DHelper.CreateRotationMatrix(ref rotQuartY, ref rotCenter);
            translationMatrix = Matrix3D.CreateTranslation(new Point3D(x, y, 0));
            TopBackCoverPages3D.TransformMatrix3D = rotMatrixX*rotMatrixY*translationMatrix;
            //top spine pages
            x = 0f;
            y = (float) (coverHeight - 0.1);
            rotX = -HalfPi;
            rotMatrixX = Matrix3D.CreateRotationX(rotX);
            translationMatrix = Matrix3D.CreateTranslation(new Point3D(x, y, 0));
            TopSpinePages3D.TransformMatrix3D = rotMatrixX*translationMatrix;
            //top front cover pages
            x = (float) spineWidth;
            y = (float) coverHeight;
            rotX = -HalfPi;
            rotY = (float) (OpenedProgress*HalfPi);
            rotMatrixX = Matrix3D.CreateRotationX(rotX);
            rotMatrixY = Matrix3D.CreateRotationY(rotY);
            translationMatrix = Matrix3D.CreateTranslation(new Point3D(x, y, 0));
            TopFrontCoverPages3D.TransformMatrix3D = rotMatrixX*rotMatrixY*translationMatrix;
            //inner page right
            x = (float) (spineWidth*0.5d);
            rotY = Pi - (float) (OpenedProgress*HalfPi);
            rotMatrixY = Matrix3D.CreateRotationY(rotY);
            translationMatrix = Matrix3D.CreateTranslation(new Point3D(x, 0, 0));
            InnerPageRight3D.TransformMatrix3D = rotMatrixY*translationMatrix;

            if (TransformsUpdated != null)
                TransformsUpdated(this);
        }

        public AxisAlignedBox3D GetBounds3D()
        {
            var cW = (float) CoverWidth;
            var cH = (float) CoverHeight;
            var sW = (float) SpineWidth;
            var bookPoints = new List<Point3D>
            {
                FrontCover3D.TransformMatrix3D.Transform(Point3D.Zero),
                FrontCover3D.TransformMatrix3D.Transform(new Point3D(cW, cH, 0f)),
                Spine3D.TransformMatrix3D.Transform(Point3D.Zero),
                Spine3D.TransformMatrix3D.Transform(new Point3D(sW, cH, 0f)),
                BackCover3D.TransformMatrix3D.Transform(Point3D.Zero),
                BackCover3D.TransformMatrix3D.Transform(new Point3D(cW, cH, 0f)),
                TopBackCoverPages3D.TransformMatrix3D.Transform(Point3D.Zero),
                TopBackCoverPages3D.TransformMatrix3D.Transform(new Point3D(cW, sW*0.5f, 0f)),
                TopSpinePages3D.TransformMatrix3D.Transform(Point3D.Zero),
                TopSpinePages3D.TransformMatrix3D.Transform(new Point3D(sW, sW*0.5f, 0f)),
                TopFrontCoverPages3D.TransformMatrix3D.Transform(Point3D.Zero),
                TopFrontCoverPages3D.TransformMatrix3D.Transform(new Point3D(cW, sW*0.5f, 0f)),
                InnerPageRight3D.TransformMatrix3D.Transform(Point3D.Zero),
                InnerPageRight3D.TransformMatrix3D.Transform(new Point3D(cW, cH, 0f))
            };

            var bounds3D = new AxisAlignedBox3D(bookPoints);
            var rotY = Matrix3D.CreateFromAxisAngle(yAxis, MathUtility.ToRadians((float) Config.Shelf3D.ShelfRotationY));
            var rotX = Matrix3D.CreateFromAxisAngle(xAxis, MathUtility.ToRadians((float) Config.Shelf3D.ShelfRotationX));
            var boundsTransform = rotY*rotX;
            return bounds3D.CreateTransformedBoundingVolume(boundsTransform);
        }

        protected void UpdateSpineWidthFromPages(int pagesCount)
        {
            if (pagesCount > 1)
            {
                double spineWidth = pagesCount;
                //UpdateBookSize(0, 0, spineWidth);
            }
        }

        protected void OnHitCoverImageChanged(object sender, IBitmapImage image)
        {
            if (image != null)
            {
                //FrontCover.Style = new ViewStyle("FrontCoverStyle");
                var coverWidth = image.Width;
                var coverHeight = image.Height;

                var scale = MathHelper.ScaleSize(new Size(coverWidth, coverHeight), Config.Shelf3D.DefaultCoverWidth,
                    Config.Shelf3D.DefaultCoverHeight);

                UpdateBookSize(coverWidth*scale, coverHeight*scale);
            }
            else
            {
                FrontCover.Style = new ViewStyle("GenericFrontCoverStyle");
                UpdateBookSizeRandomized(Config.Shelf3D.DefaultCoverWidth, Config.Shelf3D.DefaultCoverHeight,
                    Config.Shelf3D.DefaultSpineWidth);
            }
        }

        #region OpenedProgress

        private double mOpenedProgress;

        public double OpenedProgress
        {
            get { return mOpenedProgress; }
            set
            {
                if (mOpenedProgress != value)
                {
                    var old = mOpenedProgress;
                    mOpenedProgress = value;
                    OnOpenedProgressChanged(old, value);
                }
            }
        }

        protected virtual void OnOpenedProgressChanged(double oldOpenedProgress, double newOpenedProgress)
        {
            RaisePropertyChanged("OpenedProgress", oldOpenedProgress, newOpenedProgress);
            OnOpenedProgressChanged(newOpenedProgress);
        }

        #endregion OpenedProgress

        #region BackCover3D

        private BookPlane3DViewModel mBackCover3D;

        public BookPlane3DViewModel BackCover3D
        {
            get
            {
                if (mBackCover3D == null)
                    mBackCover3D = new BookPlane3DViewModel();
                return mBackCover3D;
            }
        }

        #endregion BackCover3D

        #region BackCover

        private BookPlaneViewModel mBackCover;

        public BookPlaneViewModel BackCover
        {
            get
            {
                if (mBackCover == null)
                    mBackCover = new BookPlaneViewModel();
                return mBackCover;
            }
        }

        #endregion BackCover

        #region Spine3D

        private BookPlane3DViewModel mSpine3D;

        public BookPlane3DViewModel Spine3D
        {
            get
            {
                if (mSpine3D == null)
                    mSpine3D = new BookPlane3DViewModel();
                return mSpine3D;
            }
        }

        #endregion Spine3D

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

        #region FrontCover3D

        private BookPlane3DViewModel mFrontCover3D;

        public BookPlane3DViewModel FrontCover3D
        {
            get
            {
                if (mFrontCover3D == null)
                    mFrontCover3D = new BookPlane3DViewModel();
                return mFrontCover3D;
            }
        }

        #endregion FrontCover3D

        #region FrontCover

        private FrontCoverViewModel mFrontCover;

        public FrontCoverViewModel FrontCover
        {
            get
            {
                if (mFrontCover == null)
                    mFrontCover = new FrontCoverViewModel();
                return mFrontCover;
            }
        }

        #endregion FrontCover

        #region TopBackCoverPages3D

        private BookPlane3DViewModel mTopBackCoverPages3D;

        public BookPlane3DViewModel TopBackCoverPages3D
        {
            get
            {
                if (mTopBackCoverPages3D == null)
                    mTopBackCoverPages3D = new BookPlane3DViewModel();
                return mTopBackCoverPages3D;
            }
        }

        #endregion TopBackCoverPages3D

        #region TopBackCoverPages

        private BookPlaneViewModel mTopBackCoverPages;

        public BookPlaneViewModel TopBackCoverPages
        {
            get
            {
                if (mTopBackCoverPages == null)
                    mTopBackCoverPages = new BookPlaneViewModel();
                return mTopBackCoverPages;
            }
        }

        #endregion TopBackCoverPages

        #region TopSpinePages3D

        private BookPlane3DViewModel mTopSpinePages3D;

        public BookPlane3DViewModel TopSpinePages3D
        {
            get
            {
                if (mTopSpinePages3D == null)
                    mTopSpinePages3D = new BookPlane3DViewModel();
                return mTopSpinePages3D;
            }
        }

        #endregion TopSpinePages3D

        #region TopSpinePages

        private BookPlaneViewModel mTopSpinePages;

        public BookPlaneViewModel TopSpinePages
        {
            get
            {
                if (mTopSpinePages == null)
                    mTopSpinePages = new BookPlaneViewModel();
                return mTopSpinePages;
            }
        }

        #endregion TopSpinePages

        #region TopFrontCoverPages

        private BookPlaneViewModel mTopFrontCoverPages;

        public BookPlaneViewModel TopFrontCoverPages
        {
            get
            {
                if (mTopFrontCoverPages == null)
                    mTopFrontCoverPages = new BookPlaneViewModel();
                return mTopFrontCoverPages;
            }
        }

        #endregion TopFrontCoverPages

        #region TopFrontCoverPages3D

        private BookPlane3DViewModel mTopFrontCoverPages3D;

        public BookPlane3DViewModel TopFrontCoverPages3D
        {
            get
            {
                if (mTopFrontCoverPages3D == null)
                    mTopFrontCoverPages3D = new BookPlane3DViewModel();
                return mTopFrontCoverPages3D;
            }
        }

        #endregion TopFrontCoverPages3D

        #region RightFrontCoverPages

        private BookPlaneViewModel mRightFrontCoverPages;

        public BookPlaneViewModel RightFrontCoverPages
        {
            get
            {
                if (mRightFrontCoverPages == null)
                    mRightFrontCoverPages = new BookPlaneViewModel();
                return mRightFrontCoverPages;
            }
        }

        #endregion RightFrontCoverPages

        #region RightFrontCoverPages3D

        private BookPlane3DViewModel mRightFrontCoverPages3D;

        public BookPlane3DViewModel RightFrontCoverPages3D
        {
            get
            {
                if (mRightFrontCoverPages3D == null)
                    mRightFrontCoverPages3D = new BookPlane3DViewModel();
                return mRightFrontCoverPages3D;
            }
        }

        #endregion RightFrontCoverPages3D

        #region InnerPageRight

        private BookPlaneViewModel mInnerPageRight;

        public BookPlaneViewModel InnerPageRight
        {
            get
            {
                if (mInnerPageRight == null)
                    mInnerPageRight = new BookPlaneViewModel();
                return mInnerPageRight;
            }
        }

        #endregion InnerPageRight

        #region InnerPageRight3D

        private BookPlane3DViewModel mInnerPageRight3D;

        public BookPlane3DViewModel InnerPageRight3D
        {
            get
            {
                if (mInnerPageRight3D == null)
                    mInnerPageRight3D = new BookPlane3DViewModel();
                return mInnerPageRight3D;
            }
        }

        #endregion InnerPageRight3D

        #region InnerPageLeft

        private BookPlaneViewModel mInnerPageLeft;

        public BookPlaneViewModel InnerPageLeft
        {
            get
            {
                if (mInnerPageLeft == null)
                    mInnerPageLeft = new BookPlaneViewModel();
                return mInnerPageLeft;
            }
        }

        #endregion InnerPageLeft

        #region InnerPageLeft3D

        private BookPlane3DViewModel mInnerPageLeft3D;

        public BookPlane3DViewModel InnerPageLeft3D
        {
            get
            {
                if (mInnerPageLeft3D == null)
                    mInnerPageLeft3D = new BookPlane3DViewModel();
                return mInnerPageLeft3D;
            }
        }

        #endregion InnerPageLeft3D

        #region Bounds2D

        private Rect mBounds2D;

        public Rect Bounds2D
        {
            get { return mBounds2D; }
            set
            {
                if (mBounds2D != value)
                {
                    var old = mBounds2D;
                    mBounds2D = value;
                    RaisePropertyChanged("Bounds2D", old, value);
                }
            }
        }

        #endregion Bounds2D

        #region TapBehaviour

        private TapBehaviour mTapBehaviour;

        public TapBehaviour TapBehaviour
        {
            get
            {
                if (mTapBehaviour == null)
                {
                    mTapBehaviour = new TapBehaviour();
                    Behaviours.Add(mTapBehaviour);
                }
                return mTapBehaviour;
            }
        }

        #endregion TapBehaviour
    }
}