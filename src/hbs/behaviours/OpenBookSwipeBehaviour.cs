// OpenBookSwipeBehaviour.cs
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
using picibird.hbs.transition;
using picibird.hbs.viewmodels.book3D;
using picibird.hbs.viewmodels.shelf;
using picibits.app.transition;
using picibits.core;

namespace picibird.hbs.behaviours
{
    public class OpenBookSwipeBehaviour : SwipeBehaviour
    {
        private Book3DViewModel Book3D;

        private BookshelfViewModel Bookshelf;

        public OpenBookSwipeTransition BookSwipeTransition;

        public override void OnSwipeStarting(double deltaX, double cummulatedX)
        {
            Bookshelf = HBS.ViewModel.ShelfViewModel.GetSelectedBookshelf();
            Book3D = HBS.ViewModel.Opened.BookVM.Book3D;

            var direction = deltaX > 0 ? Direction.Left : Direction.Right;
            BookSwipeTransition = new OpenBookSwipeTransition(Bookshelf, Book3D, direction);
            BookSwipeTransition.OnTransitionStarting();
        }

        public override void OnSwipeDelta(double deltaX, double cummulatedX)
        {
            var progress = cummulatedX/Attached.ActualSize.Width;
            BookSwipeTransition.Direction = progress > 0 ? Direction.Left : Direction.Right;
            BookSwipeTransition.Progress = progress;

            //Pici.Log.info(typeof(OpenBookSwipeBehaviour), "{0:0.#}", cummulatedX);
            //Pici.Log.info(typeof(OpenBookSwipeBehaviour), "Swipe Delta {0:0.##} {1:0.##}", deltaX, cummulatedX);
            //Pici.Log.info(typeof(OpenBookSwipeBehaviour), "{0}", nextBook.Model);
        }

        public override void OnSwipeCompleted(double deltaX, double cummulatedX)
        {
            Pici.Log.info(typeof(OpenBookSwipeBehaviour), "Swipe Completed");

            double completeProgress = 0;
            if (cummulatedX < -50 && Math.Sign(deltaX) == -1)
                completeProgress = -1;
            if (cummulatedX > 50 && Math.Sign(deltaX) == 1)
                completeProgress = 1;

            if (SwipeBehaviour.Direction == Direction.Right)
                completeProgress = 1;
            if (SwipeBehaviour.Direction == Direction.Left)
                completeProgress = -1;

            if (BookSwipeTransition.SwipeMode == SwipeMode.Shelf)
            {
                //check if there is another shelf to go to
                if (HBS.ViewModel.ShelfViewModel.SelectedIndex < 0 ||
                    HBS.ViewModel.ShelfViewModel.SelectedIndex > HBS.Search.Callback.MaxPageIndex)
                    completeProgress = 0;
                //check if there is a next valid book in shelf
                if (BookSwipeTransition.NextBookIndex > 0 &&
                    BookSwipeTransition.NextBookIndex < Bookshelf.Books3D.Items.Count &&
                    Bookshelf.Books3D.Items.ElementAt(BookSwipeTransition.NextBookIndex).Model == null)
                    completeProgress = 0;
            }


            var ani = Transition.Animate(BookSwipeTransition, completeProgress, HBS.AnimationSeconds*0.75,
                HBS.AnimationEaseOut);
            ani.Complete += (s, e) =>
            {
                if (completeProgress == 0)
                {
                    BookSwipeTransition.Direction = Direction.Center;
                }
                else
                {
                    HBS.ViewModel.Opened.BookVM.DropShadowOpacity = 0;
                }
            };
        }

        public override void OnSwipeAbort()
        {
        }
    }
}