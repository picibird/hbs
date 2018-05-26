using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using picibird.hbs.ldu;
using picibird.shelfhub;
using picibits.core.collection;
using picibits.core.controls3D;
using Link = picibird.hbs.ldu.Link;
using Newtonsoft.Json.Linq;
using picibits.core;

namespace picibird.hbs
{
    public static class ShelfhubExtensions
    {
        public static PiciObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> source)
        {
            return new PiciObservableCollection<TSource>(source);
        }

        public static Dictionary<string, string> ToDictionary(this object dict)
        {
            return (dict as JObject).ToObject<Dictionary<string, string>>();
        }

        public static Hit ToHit(this ShelfhubItem item)
        {
            try
            {
                Hit hit = new Hit()
                {
                    id = item.Id,
                    recid = item.Id,
                    medium = item.Medium?.Title,

                    mediumCode = item.Medium?.Type?.ToString() ?? item.Medium?.Code,
                    title = item.Title,
                    title_remainder = item.Subtitle,
                    series_title = new string[] { item.SeriesTitle }.ToList(),
                    author = item.Authors?.ToList(),
                    language = new string[] { item.Language }.ToList(),
                    Department = item.Department,
                    publicationPlaces = item.Publisher,
                    date = item.PublicationDate,
                    pages_number = item.NumberOfPages,
                    shelfhubItem = item,
                    description = new string[] { item.Abstract }.ToList()
                };
                if (item.Callnumber != null && item.Callnumber.Count > 0)
                {
                    hit.Callnumber = String.Join("\n", item.Callnumber);
                }
                if (!String.IsNullOrEmpty(item.Department))
                {
                    item.Department = String.Join("\n", item.Department.Replace(", ", ";").Split(';'));
                }

                if (item.Actions != null)
                {
                    foreach (ShelfhubAction action in item.Actions)
                    {
                        var queryParams = QueryParams.FromJson(action.Params.ToString());
                        var client = ShelfhubSearch.createShelfhubClient();
                        client.QueryAsync(queryParams).ContinueWith((t) =>
                        {
                            if (t.Status == TaskStatus.RanToCompletion)
                            {
                                QueryResponse response = t.Result;
                                hit.MultiVolumeLinks = new PiciObservableCollection<Link>();
                                foreach (ShelfhubItem subitem in response.Items)
                                {
                                    var url = "https://baselbern.swissbib.ch/Record/" + subitem.Id + "/HierarchyTree?recordID=" + subitem.Id;
                                    hit.MultiVolumeLinks.Add(new Link("Band", url, subitem.Title, "", hit));
                                }
                            }
                        });
                    }
                }

                //prepare extras
                if (item.Extras == null) item.Extras = new ObservableCollection<KeyValues>();
                //set ISBNS
                if (item.Isbn != null)
                    hit.ISBNs = String.Join("\n", item.Isbn);
                else
                {
                    hit.ISBNs = String.Empty;
                }

                //add links
                if (item.Links != null && item.Links.Count > 0)
                {
                    hit.Links = new PiciObservableCollection<Link>();
                    foreach (var link in item.Links)
                    {
                        var title = Pici.Resources.Find(link.Title);
                        hit.Links.Add(new Link(link.Type.ToString(), link.Url, title, "", hit, link.Note));
                    }
                }
                //add availability links
                if (item.Locations != null)
                {
                    //var avExtra = item.Extras.FirstOrDefault(kv => kv.Key == "availabilities");
                    //if (avExtra != null)
                    //{
                    //    if (hit.Links == null) hit.Links = new PiciObservableCollection<Link>();
                    //    foreach (var value in avExtra.Values)
                    //    {
                    //        hit.Links.Add(new Link("", value, "Verfügbarkeit", "", hit));
                    //    }
                    //    item.Extras.Remove(avExtra);
                    //}
                }

                //set qrcode link
                hit.WebshelfUris = new List<Url>();
                if(item.Links != null && item.Links.Count > 0)
                {
                    var qrCodeLink = item.Links.FirstOrDefault((l) => l.Type == LinkType.Main);
                    if(qrCodeLink != null)
                    {
                        var qrCodeUrl = new Url(qrCodeLink.Url);
                        hit.WebshelfUris.Add(qrCodeUrl);
                    }
                }
                return hit;
            }
            catch (Exception ex)
            {
                Pici.Log.error(typeof(ShelfhubExtensions), "shelfhub parsing ShelfhubItem failed", ex, item);
                throw ex;
            }
        }

        public static ItemList<Hit> ToHits(this IList<ShelfhubItem> items, SynchronizationContext syncContext = null)
        {
            var hits = new ItemList<Hit>(syncContext);
            foreach (ShelfhubItem item in items)
            {
                hits.Add(item.ToHit());
            }
            return hits;
        }

    }
}
