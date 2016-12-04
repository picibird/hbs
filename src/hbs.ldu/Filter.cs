// Filter.cs
// Date Created: 21.01.2016
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

using System.Xml.Serialization;
using picibits.core;

namespace picibird.hbs.ldu
{
    public class Filter
    {
        //// id is only for <list name=""xtargets""> (source of pazpar2 data) 
        [XmlElement("id")]
        public string TargetId { get; set; }

        [XmlElement("name")]
        public string Id { get; set; }

        [XmlElement("frequency")]
        public int Frequency { get; set; }

        [XmlIgnore]
        public FilterCategoryId Catgegory { get; set; }

        [XmlIgnore]
        public string CatgegoryKey
        {
            get { return FilterDictionary.GetCategoryKey(this); }
        }

        [XmlIgnore]
        public string Name
        {
            get
            {
                string name = Pici.Resources.Find(this.Id);
                return name;
            }
        }

        [XmlIgnore]
        public string Label
        {
            get { return ToString(); }
        }

        public override string ToString()
        {
            return Name + " : " + Frequency;
        }

        public override bool Equals(object obj)
        {
            if (obj is Filter)
            {
                Filter o = obj as Filter;
                return (this.CatgegoryKey.Equals(o.CatgegoryKey) && this.Id.Equals(o.Id));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (this.CatgegoryKey + this.Id + "").GetHashCode();
        }
    }
}