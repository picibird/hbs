// FilterDictionary.cs
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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace picibird.hbs.ldu
{
    public class FilterDictionary
    {
        private static readonly string JSON = @"
            {
              'subject': {
                'Name': 'subject',
                'DisplayName': 'Schlagwort',
                'Key': 'su',
                'Values': null
              },
              'author': {
                'Name': 'author',
                'DisplayName': 'Author',
                'Key': 'au',
                'Values': null
              },
              'xtargets': {
                'Name': 'xtargets',
                'DisplayName': 'Quellen',
                'Key': 'xtargets',
                'Values': null
              },
              'date': {
                'Name': 'date',
                'DisplayName': 'Jahr',
                'Key': 'date',
                'Values': null
              },
              'available': {
                'Name': 'available',
                'DisplayName': 'Verfügbar',
                'Key': 'available',
                'Values': {
                  '1': 'Nicht verfügbar',
                  '0': 'Verfügbar'
                }
              },
              'language': {
                'Name': 'language',
                'DisplayName': 'Sprache',
                'Key': 'language',
                'Values': {
                  'eng': 'Englisch',
                  'ger': 'Deutsch',
                  'spa': 'Spanisch',
                  'fre': 'Französisch',
                  'por': 'Portugiesisch',
                  'rus': 'Russisch',
                  'ita': 'Italienisch',
                  'chi': 'Chinesisch',
                  'jpn': 'Japanisch',
                  'pol': 'Polnisch',
                  'sonstig': 'Sonstige Sprachen'
                }
              },
              'department': {
                'Name': 'department',
                'DisplayName': 'Fachbereich',
                'Key': 'department',
                'Values': {
                  '10': 'Archäologie',
                  '20': 'Biologie',
                  '30': 'Buch- & Info-Wiss.',
                  '40': 'Chemie',
                  '50': 'Ethnologie',
                  '60': 'Geographie',
                  '70': 'Geologie',
                  '80': 'Geschichte',
                  '90': 'Informatik',
                  '100': 'Kunst',
                  '110': 'Mathematik',
                  '120': 'Medien & Komm.',
                  '130': 'Medizin',
                  '140': 'Musik',
                  '150': 'Pädagogik',
                  '160': 'Philosophie',
                  '170': 'Physik',
                  '180': 'Politik',
                  '190': 'Psychologie',
                  '200': 'Recht',
                  '210': 'Religion',
                  '220': 'Soziologie',
                  '230': 'Sport',
                  '240': 'Sprache & Literatur',
                  '250': 'Technik',
                  '260': 'Wirtschaft'
                }
              },
              'medium': {
                'Name': 'medium',
                'DisplayName': 'Medium',
                'Key': 'medium',
                'Values': {
                  'book': 'Buch',
                  'bookreview': 'Buchbesprechung',
                  'bookchapter': 'Buchkapitel',
                  'proceeding': 'Konferenzbericht',
                  'datamed': 'Datenträger',
                  'ebook': 'eBook',
                  'diss': 'Hochschulschrift',
                  'journal': 'Zeitschrift',
                  'ejournal': 'eJournal',
                  'article': 'Artikel',
                  'video': 'Film',
                  'audio': 'Tonträger',
                  'map': 'Karten/Bildmaterial',
                  'other': 'Sonstiges',
                }
              }
            }
        ";

        public class FilterDictionaryEntry
        {
            public FilterCategoryId Name { get; set; }
            public string DisplayName { get; set; }
            public string Key { get; set; }
            public Dictionary<string, string> Values { get; set; }
        }

        private static Dictionary<FilterCategoryId, FilterDictionaryEntry> DICT = initDictionary();


        private static Dictionary<FilterCategoryId, FilterDictionaryEntry> initDictionary()
        {
            Dictionary<FilterCategoryId, FilterDictionaryEntry> res =
                JsonConvert.DeserializeObject<Dictionary<FilterCategoryId, FilterDictionaryEntry>>(JSON);

#if !DEBUG
//res = ReadFromOrCreateJsonFile(res);
#endif

            return res;
        }

        public static string GetCategoryName(FilterCategory fc)
        {
            FilterDictionaryEntry dict;
            if (DICT.TryGetValue(fc.Id, out dict))
            {
                return dict.DisplayName;
            }
            return fc.Id.ToString();
        }

        public static string GetCategoryKey(Filter f)
        {
            FilterDictionaryEntry dict;
            if (DICT.TryGetValue(f.Catgegory, out dict))
            {
                return dict.Key;
            }
            return f.Catgegory.ToString();
        }

        public static string GetName(Filter filter)
        {
            FilterDictionaryEntry dict;
            if (DICT.TryGetValue(filter.Catgegory, out dict))
            {
                string res;
                if (dict.Values != null && dict.Values.TryGetValue(filter.Id, out res))
                {
                    return res;
                }
            }
            return filter.Id;
        }

        public static string GetName(FilterCategoryId category, string valueId)
        {
            FilterDictionaryEntry dict;
            if (DICT.TryGetValue(category, out dict))
            {
                string res;
                if (dict.Values != null && dict.Values.TryGetValue(valueId, out res))
                {
                    return res;
                }
            }
            return valueId;
        }
    }
}