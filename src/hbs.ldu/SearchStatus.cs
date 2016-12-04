// SearchStatus.cs
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

using System;
using System.Xml.Serialization;

namespace picibird.hbs.ldu
{
    [XmlRoot("stat")]
    public class SearchStatus
    {
        [XmlElement("activeclients")]
        public int activeclients { get; set; }

        [XmlElement("hits")]
        public int hits { get; set; }

        [XmlElement("records")]
        public int records { get; set; }

        [XmlElement("clients")]
        public int clients { get; set; }

        [XmlElement("unconnected")]
        public int unconnected { get; set; }

        [XmlElement("connecting")]
        public int connecting { get; set; }

        [XmlElement("working")]
        public int working { get; set; }

        [XmlElement("idle")]
        public int idle { get; set; }

        [XmlElement("failed")]
        public int failed { get; set; }

        [XmlElement("error")]
        public int error { get; set; }

        [XmlElement("progress")]
        public double progress { get; set; }

        public int progressAsPercent()
        {
            return int.Parse((progress*100).ToString());
        }

        public SearchStatus()
        {
            progress = -1;
        }

        public override string ToString()
        {
            return String.Format("progress={0}; hits={1}; records={2}; clients={3}; actClients={4};", progress, hits,
                records, clients, activeclients);
        }
    }
}