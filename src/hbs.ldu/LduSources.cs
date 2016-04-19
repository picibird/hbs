// LduSources.cs
// Date Created: 28.01.2016
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
namespace picibird.hbs.ldu
{
    public enum LduSourceUsage
    {
        ALL, SWB, SUMMON
    }

    public static class LduSourceUsageHelper
    {
        public static string ToFilterString(LduSourceUsage lduSource)
        {
            if (lduSource == LduSourceUsage.SWB)
                return "boss2/swb";
            if (lduSource == LduSourceUsage.SUMMON)
                return "eiger.bsz-bw.de:8000/summon";
            return "";
        }
    }


}
