using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using picibird.hbs.ldu;
using picibird.shelfhub;

namespace picibird.hbs
{
    public static class ShelfhubExtensions
    {
        public static Hit ToHit(this ShelfhubItem item)
        {

            Hit hit = new Hit()
            {
                recid = item.Id,
                title = item.Title,
                title_remainder = item.Subtitle,
                series_title = new string[] { item.SeriesTitle }.ToList(),
                publicationPlaces = item.Publisher,
                date = item.PublicationDate,
                author = item.Authors?.ToList(),
                language = new string[] { item.Language }.ToList(),
                pages_number = item.NumberOfPages,
                QRCodeImage = 
                shelfhubItem = item
            };
            if (item.Isbn != null)
                hit.ISBNs = String.Join("\n", item.Isbn);
            else
            {
                hit.ISBNs = String.Empty;
            }
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
