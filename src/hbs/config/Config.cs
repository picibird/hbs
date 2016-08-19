// Config.cs
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

using System.Threading.Tasks;
using picibits.core;

namespace picibird.hbs.config
{
    public static class Config
    {
        #region Pointer

        private static PointerConf mPointer;

        public static PointerConf Pointer
        {
            get
            {
                if (mPointer == null)
                {
                    mPointer = new PointerConf();
                }
                return mPointer;
            }
        }

        #endregion Pointer

        #region Cover

        private static CoverConf mCover;

        public static CoverConf Cover
        {
            get
            {
                if (mCover == null)
                {
                    mCover = Task.Run<CoverConf>(async () => await Pici.Config.ReadOrCreate<CoverConf>())
                        .GetAwaiter().GetResult();
                }
                return mCover;
            }
        }

        #endregion Cover

        #region BookConf

        private static BookConf mShelf3D;

        public static BookConf Shelf3D
        {
            get
            {
                if (mShelf3D == null)
                {
                    //mShelf3D = Pici.Config.ReadOrCreate<BookConf>();
                    mShelf3D = new BookConf();
                }
                return mShelf3D;
            }
        }

        #endregion BookConf

        #region KioskConfig

        private static KioskConf mKioskConf;

        public static KioskConf KioskConf
        {
            get
            {
                if (mKioskConf == null)
                {
                    mKioskConf = Task.Run<KioskConf>(async () => await Pici.Config.ReadOrCreate<KioskConf>())
                        .GetAwaiter().GetResult();
                }
                return mKioskConf;
            }
        }

        #endregion KioskConfig

        #region Histomat

        private static HistomatConf mHistomat;

        public static HistomatConf Histomat
        {
            get
            {
                if (mHistomat == null)
                {
                    //mHistomat = Pici.Config.ReadOrCreate<HistomatConf>();
                    mHistomat = new HistomatConf();
                }
                return mHistomat;
            }
        }

        #endregion Histomat
    }
}