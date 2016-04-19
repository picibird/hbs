// ShelfDrawCanvas.cs
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using picibird.hbs.viewmodels.shelf;

namespace picibird.hbs.wpf.controls
{
    public class ShelfDrawCanvas : Canvas
    {
        private static bool StyleValuesApplied;

        public ShelfDrawCanvas()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private static Brush ShelfBrush { get; set; }
        private static Brush ShelfShadowBrush { get; set; }

        public ShelfDrawViewModel VM { get; private set; }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ShelfDrawViewModel)
            {
                VM = DataContext as ShelfDrawViewModel;
                ReadStyleValues();
                UpdateShelfDrawing();
            }
            else
                VM = null;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateShelfDrawing();
        }

        private static void ReadStyleValues()
        {
            if (!StyleValuesApplied)
            {
                var app = Application.Current;
                ShelfBrush = (Brush) app.FindResource("HBS_WHITE_BRUSH");
                ShelfShadowBrush = (Brush) app.FindResource("HBS_GRAY_LIGHT_BRUSH");
                StyleValuesApplied = true;
            }
        }

        public void UpdateShelfDrawing()
        {
            Children.Clear();
            var aw = ActualWidth;
            var ah = ActualHeight;
            if (aw > 0 && ah > 0)
            {
                var rowHeight = ah/VM.Rows;
                var rowShelfY = rowHeight - VM.ShelfHeight;

                VM.RowHeight = rowHeight;


                //draw shelf
                for (var i = 0; i < VM.Rows; i++)
                {
                    var shelf = new Rectangle();
                    shelf.Width = aw;
                    shelf.Height = VM.ShelfHeight;
                    SetTop(shelf, rowShelfY);
                    shelf.Fill = ShelfBrush;
                    Children.Add(shelf);


                    rowShelfY += rowHeight;
                }
                //draw shadow
                var rowShadowY = rowHeight - VM.ShelfHeight - VM.DepthY;
                for (var i = 0; i < VM.Rows; i++)
                {
                    var shadow = new Rectangle();
                    shadow.Width = aw;
                    shadow.Height = VM.ShelfHeight;
                    SetTop(shadow, rowShadowY);
                    SetZIndex(shadow, -1);
                    shadow.Fill = ShelfShadowBrush;
                    Children.Add(shadow);


                    rowShadowY += rowHeight;
                }

                //draw left stand
                double standX = 0;
                double standY = 0;
                var standW = VM.ShelfStandWidth;
                var standH = ah;
                var standLeft = new Rectangle();
                standLeft.Width = standW;
                standLeft.Height = standH;
                SetTop(standLeft, standY);
                SetZIndex(standLeft, 1);
                standLeft.Fill = ShelfBrush;
                Children.Add(standLeft);
                //draw right stand
                standX = aw - VM.ShelfStandWidth;
                var standRight = new Rectangle();
                standRight.Width = standW;
                standRight.Height = standH;
                SetTop(standRight, standY);
                SetLeft(standRight, standX);
                SetZIndex(standRight, 1);
                standRight.Fill = ShelfBrush;
                Children.Add(standRight);

                //stand shadows

                //draw left shadow
                standX = VM.ShelfStandWidth;
                standY = 0;
                standW = standX + VM.DepthX;
                var leftShadow = new Polygon();
                leftShadow.Points.Add(new Point(standX, standY));
                leftShadow.Points.Add(new Point(standW, standY + VM.DepthY));
                leftShadow.Points.Add(new Point(standW, ah));
                leftShadow.Points.Add(new Point(standX, ah));
                SetZIndex(leftShadow, -2);
                leftShadow.Fill = ShelfShadowBrush;
                Children.Add(leftShadow);

                //draw right shadow
                standX = aw - VM.ShelfStandWidth - VM.DepthX;
                standY = 0;
                standW = standX + VM.DepthX;
                var rightShadow = new Polygon();
                rightShadow.Points.Add(new Point(standX, standY + VM.DepthY));
                rightShadow.Points.Add(new Point(standW, standY));
                rightShadow.Points.Add(new Point(standW, ah));
                rightShadow.Points.Add(new Point(standX, ah));
                SetZIndex(rightShadow, -2);
                rightShadow.Fill = ShelfShadowBrush;
                Children.Add(rightShadow);

                //info shield
                rowShelfY = rowHeight - VM.ShelfHeight;
                standW = VM.InfoShieldWidth;
                standH = rowShelfY + VM.ShelfHeight*0.25;
                var infoShield = new Rectangle();
                infoShield.Width = standW;
                infoShield.Height = standH;
                SetZIndex(infoShield, 3);
                infoShield.Fill = ShelfBrush;
                Children.Add(infoShield);

                //info shield shadow
                standX = standW;
                standY = 0;
                standW = standX + VM.DepthX;
                var infoShielfShadow = new Polygon();
                infoShielfShadow.Points.Add(new Point(standX, standY));
                infoShielfShadow.Points.Add(new Point(standW, standY + VM.DepthY));
                infoShielfShadow.Points.Add(new Point(standW, standH));
                infoShielfShadow.Points.Add(new Point(standX, standH));
                SetZIndex(infoShielfShadow, -2);
                infoShielfShadow.Fill = ShelfShadowBrush;
                Children.Add(infoShielfShadow);
            }
        }

        private void DrawShelf(double aw, double ah)
        {
        }
    }
}