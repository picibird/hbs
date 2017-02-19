using picibird.hbs.ldu.pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace picibird.hbs.ldu
{
    public static class ShelfhubHelper
    {

        public static IShelfhubSearch Search { get; set; }

    }

    public interface IShelfhubSearch
    {
        Page LoadPage(int index);
    }
}
