// Bookshelf3DViewModel.cs
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
using picibird.hbs.config;
using picibird.hbs.helper;
using picibird.hbs.Intents;
using picibird.hbs.viewmodels.book3D;

using picibits.app.transition;
using picibits.core;
using picibits.core.export.instances;
using picibits.core.intent;
using picibits.core.math3D;
using picibits.core.models;
using picibits.core.mvvm;

namespace picibird.hbs.viewmodels.shelf
{
    public class Bookshelf3DViewModel : TransitionItemsViewModel
    {
        public Bookshelf3DViewModel()
        {
            Style = new ViewStyle("Bookshelf3DStyle");
        }

        public void CreateBooks()
        {
            for (var y = 0; y < HBS.RowCount; y++)
            {
                for (var x = 0; x < HBS.ColumnCount; x++)
                {
                    var book3DModel = new Book3DViewModel();
                    book3DModel.TransformsUpdated += OnBookTransformsUpdated;
                    book3DModel.TapBehaviour.Tap += OnBookTap;
                    book3DModel.Name = string.Format("x={0} y={1}", x, y);
                    book3DModel.CreatBookSides();
                    Items.Add(book3DModel);
                }
            }
        }

        private void OnBookTap(object sender, EventArgs e)
        {
            var book3d = sender as Book3DViewModel;
            var intent = new Intent(HbsIntents.ACTION_OPEN_BOOK);
            intent.AddExtra("book3D", book3d);
            Pici.Intent.Send(intent);
        }

        private void OnBookTransformsUpdated(Book3DViewModel book3D)
        {
            UpdateBookTransform(book3D);
        }

        private void UpdateTransforms(Size shelfSize)
        {
            for (var y = 0; y < HBS.RowCount; y++)
            {
                for (var x = 0; x < HBS.ColumnCount; x++)
                {
                    var i = y*HBS.ColumnCount + x;
                    if (Items.Count > i)
                    {
                        var book3DModel = (Book3DViewModel) Items.ElementAt(i);
                        UpdateBookTransform(book3DModel, x, y);
                    }
                }
            }
        }

        public Book3DViewModel GetBook(int x, int y)
        {
            var i = y*HBS.ColumnCount + x;
            return (Book3DViewModel) Items.ElementAt(i);
        }

        private void UpdateBookTransform(Book3DViewModel bookVM)
        {
            var i = Items.IndexOf(bookVM);
            var x = i%HBS.ColumnCount;
            var y = i/HBS.ColumnCount;
            UpdateBookTransform(bookVM, x, y);
        }

        private void UpdateBookTransform(Book3DViewModel bookVM, int iX, int iY)
        {
            var bounds3D = bookVM.GetBounds3D();

            var margLeft = Config.Shelf3D.ShelfMarginLeft;
            var margTop = Config.Shelf3D.ShelfMarginTop;
            var margRight = Config.Shelf3D.ShelfMarginRight;
            var margBottom = Config.Shelf3D.ShelfMarginBottom;

            var shelfWidth = ActualSize.Width - margLeft - margRight;
            var shelfHeight = ActualSize.Height - margTop - margBottom;
            if (shelfWidth == 0 || shelfHeight == 0)
            {
                return;
            }

            var boardHeight = Config.Shelf3D.ShelfBoardHeight;

            double bookX = bounds3D.Min.X;
            double bookY = bounds3D.Min.Y;
            double bookWidth = bounds3D.Size.X;
            double bookHeight = bounds3D.Size.Y;

            if (bookWidth > 0 && bookHeight > 0)
            {
                var bookSize = new Size(bookWidth, bookHeight);

                var frameWidth = shelfWidth/HBS.ColumnCount;
                var frameHeight = shelfHeight/HBS.RowCount;
                frameWidth = Math.Max(0, frameWidth);
                frameHeight = Math.Max(0, frameHeight) - boardHeight;

                var frameX = frameWidth*iX + margLeft;
                var frameY = (frameHeight + boardHeight)*iY + margTop;
                var frameSize = new Size(frameWidth, frameHeight);

                var bookScale = MathHelper.ScaleSize(bookSize, frameWidth, frameHeight)*
                                Config.Shelf3D.ShelfAdditionalScale;

                var scaledBookWidth = bookWidth*bookScale;
                var scaledBookHeight = bookHeight*bookScale;

                var x = frameX + (frameWidth - scaledBookWidth)*0.5d + bookX;
                var y = frameY + (frameHeight - scaledBookHeight) + bookY;

                var translationX = x;
                //y gets inverted here since 3d origin is bottom left not top left
                var translationY = shelfHeight - scaledBookHeight - y;

                var m3D = Matrix3D.Identity;
                var rotY = Matrix3D.CreateRotationY(MathUtility.ToRadians((float) Config.Shelf3D.ShelfRotationY));
                var rotX = Matrix3D.CreateRotationX(MathUtility.ToRadians((float) Config.Shelf3D.ShelfRotationX));
                var scale = Matrix3D.CreateScale((float) bookScale);
                var translate = Matrix3D.CreateTranslation(new Point3D((float) translationX, (float) translationY, 0f));

                var bounds2D = new Rect((float) x, (float) y, (float) scaledBookWidth, (float) scaledBookHeight);
                bookVM.Bounds2D = bounds2D;
                bookVM.TransformMatrix3D = m3D*rotY*rotX*scale*translate;
            }
        }

        protected override void OnActualSizeChanged(Size oldActualSize, Size newActualSize)
        {
            base.OnActualSizeChanged(oldActualSize, newActualSize);
            UpdateTransforms(newActualSize);
        }
    }
}