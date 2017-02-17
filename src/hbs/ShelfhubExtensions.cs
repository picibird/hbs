﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using picibird.hbs.ldu;
using picibird.shelfhub;
using picibits.core.collection;
using picibits.core.controls3D;
using Link = picibird.hbs.ldu.Link;

namespace picibird.hbs
{
    public static class ShelfhubExtensions
    {
        public static Hit ToHit(this ShelfhubItem item)
        {

            Hit hit = new Hit()
            {
                recid = item.Id,
                medium = item.Medium,
                title = item.Title,
                title_remainder = item.Subtitle,
                series_title = new string[] { item.SeriesTitle }.ToList(),
                author = item.Authors?.ToList(),
                language = new string[] { item.Language }.ToList(),
                Department = item.Department,
                publicationPlaces = item.Publisher,
                date = item.PublicationDate,
                pages_number = item.NumberOfPages,
                shelfhubItem = item
            };
            //prepare extras
            if (item.Extras == null) item.Extras = new ObservableCollection<KeyValues>();
            //set ISBNS
            if (item.Isbn != null)
                hit.ISBNs = String.Join("\n", item.Isbn);
            else
            {
                hit.ISBNs = String.Empty;
            }
            //add locations aka signatures
            if (item.Locations != null)
                item.Extras.Add(new KeyValues()
                {
                    Key = "Standorte",
                    Values = item.Locations
                });
            //add links
            if (item.Links != null && item.Links.Count > 0)
            {
                hit.Links = new PiciObservableCollection<Link>();
                foreach (var link in item.Links)
                {
                    hit.Links.Add(new Link(link.Type.ToString(), link.Url, link.Title, "", hit));
                }
            }
            //add availability links
            var avExtra = item.Extras.FirstOrDefault(kv => kv.Key == "availabilities");
            if (avExtra != null)
            {
                if (hit.Links == null) hit.Links = new PiciObservableCollection<Link>();
                foreach (var value in avExtra.Values)
                {
                    hit.Links.Add(new Link("", value, "Verfügbarkeit", "", hit));
                }
                item.Extras.Remove(avExtra);
            }

            //set qrcode link
            hit.WebshelfUris = new List<Url>();
            var qrCodeLink = item.Links.First((l) => l.Type == LinkType.Main);
            var qrCodeUrl = new Url(qrCodeLink.Url);
            hit.WebshelfUris.Add(qrCodeUrl);

            return hit;
        }

        public static List<Hit> ToHits(this IList<ShelfhubItem> items)
        {
            var hits = new List<Hit>(items.Count);
            foreach (ShelfhubItem item in items)
            {
                hits.Add(item.ToHit());
            }
            return hits;
        }

    }
}