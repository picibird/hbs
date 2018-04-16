// Hit.cs
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Flurl;
using Newtonsoft.Json;
using picibits.app.bitmap;
using picibits.bib;
using picibits.core;
using picibits.core.collection;
using picibits.core.json;
using picibits.core.mvvm;

namespace picibird.hbs.ldu
{
    public class CoverData
    {
        public IBitmapImage Image;
        public HistomatColorScheme ColorScheme;
    }

    public class Hit : Model, IEquatable<Hit>, IEqualityComparer<Hit>
    {
        private object _shelfhubItem;
        public object shelfhubItem
        {
            get { return _shelfhubItem; }
            set { _shelfhubItem = value; }
        }

        public enum CoverSizes
        {
            small,
            medium,
            large
        };

        #region Pazpar2Properties

        [XmlElement("md-language")]
        [JsonProperty("md_language")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> language { get; set; }

        [XmlElement("md-id")]
        [JsonProperty("md_id")]
        public string id { get; set; }

        [XmlElement("md-author")]
        [JsonProperty("md_author")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> author { get; set; }

        [XmlElement("md-merge-author")]
        [JsonProperty("md_merge_author")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> merge_author { get; set; }

        [XmlElement("md-other-person")]
        [JsonProperty("md_other_person")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> other_person { get; set; }

        [XmlElement("md-date")]
        [JsonProperty("md_date")]
        public string date { get; set; }

        [XmlElement("md-title")]
        [JsonProperty("md_title")]
        public string title { get; set; }

        [XmlElement("md-title-remainder")]
        [JsonProperty("md_title_remainder")]
        public string title_remainder { get; set; }

        [XmlElement("md-title-responsability")]
        [JsonProperty("md_title_responsability")]
        public string title_responsability { get; set; }

        [XmlElement("md-medium")]
        [JsonProperty("md_medium")]
        public string medium { get; set; }
        public string mediumCode { get; set; }

        [XmlElement("md-series-title")]
        [JsonProperty("md_series_title")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> series_title { get; set; }

        [XmlElement("md-journal-title")]
        [JsonProperty("md_journal_title")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> journal_title { get; set; }


        [XmlElement("md-description")]
        [JsonProperty("md_description")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> description { get; set; }

        [XmlElement("md-pages-number")]
        [JsonProperty("md_pages_number")]
        public string pages_number { get; set; }

        [XmlIgnore]
        public int pages_numberInt
        {
            get
            {
                try
                {
                    int number;
                    bool result = Int32.TryParse(pages_number, out number);
                    return (result) ? number : 0;
                }
                catch (Exception ex)
                {
                    Pici.Log.error(typeof(Hit),
                        String.Format("Error parsing page-number \"{0}\" of Hit: \r\n{1}", pages_number, this.ToString()),
                        ex);
                }
                return 0;
            }
        }

        [XmlElement("md-zdb-number")]
        [JsonProperty("md_zdb_number")]
        public string zdb_number { get; set; }

        [XmlElement("md-issn")]
        [JsonProperty("md_issn")]
        [JsonConverter(typeof(ValueToListConverter<string>))]
        public List<string> issn { get; set; }

        [XmlElement("md-doi")]
        [JsonProperty("md_doi")]
        public string doi { get; set; }

        [XmlElement("md-multivolume-title")]
        [JsonProperty("md_multivolume_title")]
        public string multivolume_title { get; set; }

        [XmlElement("recid")]
        [JsonProperty("recid")]
        public string recid { get; set; }

        [XmlElement("count")]
        [JsonProperty("count")]
        public string count { get; set; }


        #region relevance

        private string mrelevance;

        [XmlElement("relevance")]
        [JsonProperty("relevance")]
        public string relevance
        {
            get { return mrelevance; }
            set
            {
                if (mrelevance != value)
                {
                    string old = mrelevance;
                    mrelevance = value;
                    RaisePropertyChanged("relevance", old, value);
                }
            }
        }

        #endregion relevance

        [XmlElement("location")]
        [JsonProperty("location")]
        [JsonConverter(typeof(ValueToListConverter<Location>))]
        public List<Location> locations { get; set; } = new List<Location>();

        #endregion

        private List<Url> m_WebshelfUris;
        [XmlIgnore]
        public List<Url> WebshelfUris
        {
            get
            {
                if (m_WebshelfUris == null)
                {
                    m_WebshelfUris = new List<Url>();
                    foreach (var loc in locations)
                    {
                        Url url = new Url(Pazpar2Settings.BIBSHELF_URL)
                            .AppendPathSegment("m")
                            .AppendPathSegments(loc.locationId)
                            .AppendPathSegment(loc.ppn);
                        m_WebshelfUris.Add(url);
                    }
                }
                return m_WebshelfUris;
            }
            set { m_WebshelfUris = value; }
        }

        [XmlIgnore]
        public string CoverUrl_S
        {
            get { return GetCoverUrl(CoverSizes.small); }
        }

        [XmlIgnore]
        public string CoverUrl_M
        {
            get { return GetCoverUrl(CoverSizes.medium); }
        }

        [XmlIgnore]
        public string CoverUrl_L
        {
            get { return GetCoverUrl(CoverSizes.large); }
        }

        #region Author String

        [XmlIgnore]
        public string authorString
        {
            get
            {
                IEnumerable<string> authorList = author.Concat(other_person);
                string authors = String.Join("\n", authorList);
                return authors;
            }
        }

        #endregion Author String

        #region Merge Author String

        [XmlIgnore]
        public string mergeAuthorString
        {
            get { return String.Join("; ", merge_author); }
        }

        #endregion Author String

        #region Author Cover String

        [XmlIgnore]
        public string authorCoverString
        {
            get
            {
                IEnumerable<string> authorList = GetMergedAuthors();
                bool manyAuthors = authorList.Count() > 3;
                if (manyAuthors)
                    authorList = authorList.Take(3);
                string authors = String.Join("\n", authorList);
                if (manyAuthors)
                    authors += " [...]";
                return authors;
            }
        }

        #endregion Author Cover String

        #region Author Back Cover String

        [XmlIgnore]
        public string authorBackCoverString
        {
            get
            {
                IEnumerable<string> authorList = GetMergedAuthors();
                return String.Join("\n", authorList);
                ;
            }
        }

        #endregion Author Back Cover String

        #region JournalTitle

        private string mJournalTitle;

        public string JournalTitle
        {
            get
            {
                if (mJournalTitle == null && journal_title != null && journal_title.Count > 0)
                {
                    mJournalTitle = journal_title.First();
                }
                return mJournalTitle;
            }
        }

        #endregion JournalTitle

        #region SeriesTitle

        private string mSeriesTitle;

        public string SeriesTitle
        {
            get
            {
                if (mSeriesTitle == null && series_title != null && series_title.Count > 0)
                {
                    mSeriesTitle = series_title.First();
                }
                return mSeriesTitle;
            }
        }

        #endregion SeriesTitle

        private IEnumerable<string> GetMergedAuthors()
        {
            IEnumerable<string> merged = new List<string>();
            if (author != null)
                merged = merged.Concat(author);
            if (other_person != null)
                merged = merged.Concat(other_person);
            return merged;
        }

        #region CoverImage

        public event EventHandler<IBitmapImage> CoverImageChanged;

        private IBitmapImage mCoverImage;

        [XmlIgnore]
        public IBitmapImage CoverImage
        {
            get { return mCoverImage; }
            set
            {
                if (mCoverImage != value)
                {
                    IBitmapImage old = mCoverImage;
                    mCoverImage = value;
                    RaisePropertyChanged("CoverImage", old, value);
                    if (CoverImageChanged != null)
                        CoverImageChanged(this, mCoverImage);
                }
            }
        }

        #endregion CoverImage

        #region CoverPriority

        private int mCoverPriority = 1;

        public int CoverPriority
        {
            get { return mCoverPriority; }
            set
            {
                if (mCoverPriority != value)
                {
                    int old = mCoverPriority;
                    mCoverPriority = value;
                    RaisePropertyChanged("CoverPriority", old, value);
                }
            }
        }

        #endregion CoverPriority

        #region CoverColorScheme

        public event EventHandler<HistomatColorScheme> CoverColorSchemeChanged;

        private HistomatColorScheme mCoverColorScheme = HistomatColorScheme.DEFAULT;

        [XmlIgnore]
        public HistomatColorScheme CoverColorScheme
        {
            get { return mCoverColorScheme; }
            set
            {
                if (mCoverColorScheme != value)
                {
                    HistomatColorScheme old = mCoverColorScheme;
                    mCoverColorScheme = value;
                    RaisePropertyChanged("CoverColorScheme", old, value);
                    if (CoverColorSchemeChanged != null)
                        CoverColorSchemeChanged(this, mCoverColorScheme);
                }
            }
        }

        #endregion CoverColorScheme

        #region CoverSearchColors

        private HistomatSearchColors mCoverSearchColors = HistomatSearchColors.DEFAULT;

        [XmlIgnore]
        public HistomatSearchColors CoverSearchColors
        {
            get { return mCoverSearchColors; }
            set
            {
                if (mCoverSearchColors != value)
                {
                    HistomatSearchColors old = mCoverSearchColors;
                    mCoverSearchColors = value;
                    RaisePropertyChanged("CoverSearchColors", old, value);
                }
            }
        }

        #endregion CoverSearchColors

        #region CoverImageUrl

        private string mCoverImageUrl;

        [XmlIgnore]
        public string CoverImageUrl
        {
            get { return mCoverImageUrl; }
            set
            {
                if (mCoverImageUrl != value)
                {
                    string old = mCoverImageUrl;
                    mCoverImageUrl = value;
                    OnCoverImageUrlChanged(old, value);
                }
            }
        }


        protected virtual void OnCoverImageUrlChanged(string oldCoverImageUrl, string newCoverImageUrl)
        {
            RaisePropertyChanged("CoverImageUrl", oldCoverImageUrl, newCoverImageUrl);

            if (String.IsNullOrEmpty(newCoverImageUrl))
                return;
            var loader = Pici.Injections.GetInstance<IHitCoverLoader>();
            loader.Load(this, newCoverImageUrl);
        }

        #endregion CoverImageUrl

        #region QRCodeImage

        private IBitmapImage mQRCodeImage;

        [XmlIgnore]
        public IBitmapImage QRCodeImage
        {
            get { return mQRCodeImage; }
            set
            {
                if (mQRCodeImage != value)
                {
                    IBitmapImage old = mQRCodeImage;
                    mQRCodeImage = value;
                    RaisePropertyChanged("QRCodeImage", old, mQRCodeImage);
                }
            }
        }

        #endregion QRCodeImage

        #region publicationName

        public string publicationName
        {
            get
            {
                if (locations == null || locations.Count == 0 || locations[0].publicationName == null ||
                    locations[0].publicationName.Count == 0)
                    return "";
                return locations[0].publicationName[0];
            }
        }

        #endregion publicationName

        #region publicationPlaces

        private string m_publicationPlaces;
        public string publicationPlaces
        {
            get
            {
                if (m_publicationPlaces == null)
                {
                    if (locations == null || locations.Count == 0)
                        return "";
                    List<string> places = locations[0].publicationPlaces;
                    if (places == null || places.Count == 0)
                        return "";
                    m_publicationPlaces = String.Join("\n", places);
                }
                return m_publicationPlaces;
            }
            set { m_publicationPlaces = value; }
        }

        #endregion publicationPlaces

        #region Callnumber

        private string m_Callnumber;
        public string Callnumber
        {
            set {
                m_Callnumber = value; }
            get
            {
                return m_Callnumber;
            }
        }


        #endregion Callnumber

        #region ISBNs

        private string m_ISBNs;
        public string ISBNs
        {
            set { m_ISBNs = value; }
            get
            {
                if (m_ISBNs == null)
                {
                    m_ISBNs = String.Join("\n", GetISBNs());
                }

                return m_ISBNs;
            }
        }

        public string CoverIsbn { get; set; }

        public IEnumerable<string> GetISBNs()
        {
            if (locations == null || locations.Count == 0)
                return Enumerable.Empty<string>();
            IEnumerable<string> isbns = locations
                .Where(l => l.isbn != null)
                .SelectMany(l => l.isbn)
                .Distinct();
            return isbns;
        }

        #endregion ISBNs

        #region Description

        public string Description
        {
            get
            {
                if (description == null || description.Count == 0)
                    return "";
                string desc = String.Join("\n\n", description);
                return desc;
            }
        }

        #endregion Description

        #region Departement

        private string m_Department;
        public string Department
        {
            get
            {
                if (String.IsNullOrEmpty(m_Department))
                {
                    if (locations == null || locations.Count == 0)
                        return "";
                    Location loc = GetMainLocation();
                    if (loc != null && loc.department != null)
                    {
                        StringBuilder departements = new StringBuilder();
                        foreach (string dep in loc.department)
                        {
                            string depString = Pici.Resources.Find(dep);
                            departements.AppendLine(depString);
                        }
                        string departementString = departements.ToString();
                        m_Department = departementString;
                    }
                }

                return m_Department;
            }
            set { m_Department = value; }
        }

        #endregion Departement

        #region Language

        public string Language
        {
            get
            {
                if (language != null && language.Count > 0)
                {
                    string langString = Pici.Resources.Find(language.First());
                    return langString;
                }
                return "";
            }
        }

        #endregion Language

        #region Medium

        public string Medium
        {
            get
            {
                if (string.IsNullOrEmpty(medium)) return "";
                string medium_string = Pici.Resources.Find(medium);
                return medium_string;
            }
        }

        #endregion Medium

        #region Links

        [XmlIgnore]
        private bool mLinksFullTextResolved;
        [XmlIgnore]
        private PiciObservableCollection<Link> mLinksFullText;

        [XmlIgnore]
        public PiciObservableCollection<Link> LinksFullText
        {
            get
            {
                if (!mLinksFullTextResolved)
                {
                    PiciObservableCollection<Link> links = new PiciObservableCollection<Link>();
                    Location main = GetMainLocation();
                    //add main location electronic urls
                    string link = "";
                    if (main?.linkResolver != null)
                        link = main.linkResolver.FirstOrDefault((l) => !String.IsNullOrEmpty(l));
                    if (!String.IsNullOrEmpty(link))
                        links.Add(new Link(Link.TYPE_TEXT, link, title, title_remainder, this));
                    //set as links if has any
                    if (links.Count > 0)
                        mLinksFullText = links;
                    //mark links as resolved
                    mLinksFullTextResolved = true;
                }
                return mLinksFullText;
            }
        }

        [XmlIgnore]
        private bool mLinksResolved;
        [XmlIgnore]
        private PiciObservableCollection<Link> mLinks;

        [XmlIgnore]
        public PiciObservableCollection<Link> Links
        {
            get
            {
                if (!mLinksResolved)
                {
                    PiciObservableCollection<Link> links = new PiciObservableCollection<Link>();
                    Location main = GetMainLocation();
                    //add main location electronic urls
                    if (main?.urls != null)
                    {
                        foreach (string url in main.urls)
                            links.Add(new Link(Link.TYPE_LINK, url, title, title_remainder, this));
                    }
                    //set as links if has any
                    if (links?.Count > 0)
                        mLinks = links;
                    //mark links as resolved
                    mLinksResolved = true;
                }
                return mLinks;
            }
            set
            {
                mLinks = value;
                mLinksResolved = mLinks != null;
            }
        }

        [XmlIgnore]
        public PiciObservableCollection<Link> MultiVolumeLinks { get; set; }

        #endregion Links

        public async Task<Record> GetDetailsAsync(SearchSession session)
        {
            return await session.PP2_Record(recid);
        }

        public Location GetMainLocation()
        {
            //return first if only one
            if (locations.Count == 1)
                return locations.First();
            //get location with most info
            Location loc = GetLocationWithSource(Sources.SWB);
            if (loc == null)
                loc = GetLocationWithDepartement();
            if (loc == null)
                loc = locations.FirstOrDefault();
            return loc;
        }

        public Location GetLocationWithSource(string source)
        {
            if (locations == null)
                return null;
            return locations.Where(loc => loc.locationId.Equals(source)).FirstOrDefault();
        }

        public Location GetLocationWithDepartement()
        {
            return
                locations.Where(loc => loc.department != null && !String.IsNullOrEmpty(loc.department.FirstOrDefault()))
                    .FirstOrDefault();
        }

        CancellationTokenSource LoadCoverTokenSource;


        public string GetCoverUrl(CoverSizes size)
        {
            IEnumerable<string> isbns = GetISBNs();
            if (isbns.Count() > 0)
            {
                Url url = new Url(Pazpar2Settings.COVER_PROVIDER_URL)
                    .SetQueryParam("isn", isbns.First())
                    .SetQueryParam("size", size);
                return url;
            }
            return null;
        }

        public string GetPrimaryKey()
        {
            return recid;
        }

        public override string ToString()
        {
            return title;
        }

        public bool Equals(Hit other)
        {
            return this.recid.Equals(other.recid);
        }

        public bool Equals(Hit x, Hit y)
        {
            return x.recid.Equals(y.recid);
        }

        public int GetHashCode(Hit obj)
        {
            return (obj.GetPrimaryKey()).GetHashCode();
        }


        // ===============END METHODS=============================
    }
}