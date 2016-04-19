// Location.cs
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
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;

using picibits.core;
using picibits.core.json;

namespace picibird.hbs.ldu
{
    public class Location
    {

        //<location id="z3950n.bsz-bw.de:20210/swb367"
        //   name="Z3950-SWB Lokale Sicht UB Konstanz" checksum="160722249">
        // <md-id>304413836</md-id>
        // <md-author>Einstein, Albert</md-author>
        // <md-date>2009</md-date>
        // <md-title>The collected papers of Albert Einstein</md-title>
        // <md-title-number-section>vol. 11</md-title-number-section>
        // <md-medium>book</md-medium>
        //</location>
        [XmlAttribute("id")]
        [JsonProperty("id")]
        public string locationId { get; set; }

        [XmlElement("md-parent-id")]
        [JsonProperty("md_parent_id")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> parentId { get; set; }

        [XmlElement("md-id")]
        [JsonProperty("md_id")]
        public string ppn { get; set; }

        [XmlElement("md-edition")]
        [JsonProperty("md_edition")]
        public string edition { get; set; }

        [XmlElement("md-oclc-number")]
        [JsonProperty("md_oclc_number")]
        public string oclcNumber { get; set; }

        [XmlElement("md-journal-title")]
        [JsonProperty("md_journal_title")]
        public string journalTitle { get; set; }

        [XmlElement("md-journal-subpart")]
        [JsonProperty("md_journal_subpart")]
        public string journalSubpart { get; set; }

        [XmlElement("md-journal-volume-number")]
        [JsonProperty("md_journal_volume_number")]
        public string journalVolumeNumber { get; set; }

        [XmlElement("md-available")]
        [JsonProperty("md_available")]
        [JsonConverter(typeof(BoolConverter))]
        public bool available { get; set; }
        
        [XmlElement("md-avdata")]
        [JsonProperty("md-avdata")]
        public string avdata { get; set; }

        [XmlElement("md-author")]
        [JsonProperty("md_author")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> author { get; set; }

        [XmlElement("md-title")]
        [JsonProperty("md_title")]
        public string title { get; set; }

        [XmlElement("md-title-complete")]
        [JsonProperty("md_title_complete")]
        public string titleComplete { get; set; }

        [XmlElement("md-title-number-section")]
        [JsonProperty("md_title_number_section")]
        public string titleNumber { get; set; }

        [XmlElement("md-title-remainder")]
        [JsonProperty("md_title_remainder")]
        public string titleRemainder { get; set; }

        [XmlElement("md-title-responsibility")]
        [JsonProperty("md_title_responsibility")]
        public string titleResponsibility { get; set; }

        [XmlElement("md-publication-name")]
        [JsonProperty("md_publication_name")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> publicationName { get; set; }

        [XmlElement("md-physical-extent")]
        [JsonProperty("md_physical_extent")]
        public string physicalExtent { get; set; }

        [XmlElement("md-publication-place")]
        [JsonProperty("md_publication_place")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> publicationPlaces { get; set; }

        [XmlElement("md-medium")]
        [JsonProperty("md_medium")]
        public string medium { get; set; }

        [XmlElement("md-isbn")]
        [JsonProperty("md_isbn")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> isbn { get; set; }

        [XmlElement("md-department")]
        [JsonProperty("md_department")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> department { get; set; }

        [XmlElement("md-countEx")]
        [JsonProperty("md_countEx")]
        public int countExisting { get; set; }

        [XmlElement("md-countAvail")]
        [JsonProperty("md_countAvail")]
        public int countAvailable { get; set; }

        [XmlElement("md-has-fulltext")]
        [JsonProperty("md_has_fulltext")]
        public string hasFulltext { get; set; }

        [XmlElement("md-link-resolver")]
        [JsonProperty("md_link_resolver")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> linkResolver { get; set; }

        [XmlIgnore]
        public bool hasFulltextBool
        {
            get
            {
                try
                {
                    bool result;
                    bool r = Boolean.TryParse(hasFulltext, out result);
                    return r && result;
                }
                catch (Exception ex)
                {
                    Pici.Log.error(typeof(Location), "Error while parsing 'hasFulltext' of " + this, ex);
                }
                return false;
            }
        }

        [XmlElement("md-electronic-url")]
        [JsonProperty("md_electronic_url")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> urls { get; set; }

        [XmlElement("md-electronic-format-type")]
        [JsonProperty("md_electronic_format_type")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> electronicFormatType { get; set; }

        [XmlElement("md-electronic-text")]
        [JsonProperty("md_electronic_text")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> texts { get; set; }

        public string GetPrimaryKey()
        {
            return locationId;
        }


    }
}
