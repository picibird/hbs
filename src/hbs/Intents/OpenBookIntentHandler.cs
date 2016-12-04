// OpenBookIntentHandler.cs
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

using picibird.hbs.transition;
using picibird.hbs.viewmodels.book3D;
using picibits.app.transition;
using picibits.core.intent;

namespace picibird.hbs.Intents
{
    public class OpenBookIntentHandler : IIntentHandler
    {
        public bool CanHandleIntent(Intent intent)
        {
            return intent.Action == HbsIntents.ACTION_OPEN_BOOK;
        }

        public void HandleIntent(Intent intent)
        {
            if (HBS.ViewModel.Opened.BookVM.Book == null && !HBS.IsAnimating)
            {
                var Book3D = intent.GetExtra<Book3DViewModel>("book3D");
                var book = HBS.CreateBook(Book3D);
                Transition.Animate(new OpenBookTransition(Book3D, book), 1, HBS.AnimationSeconds, HBS.AnimationEaseInOut);
            }
        }
    }
}