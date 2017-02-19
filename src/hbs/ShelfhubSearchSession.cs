using picibird.hbs.ldu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace picibird.hbs
{
    public class ShelfhubSearchSession : SearchSession
    {

        protected override Task<int> PP2_Init()
        {
            return base.PP2_Init();
        }

        protected override Task PP2_Query(SearchRequest searchRequest)
        {
            return base.PP2_Query(searchRequest);
        }

        public override Task<PazPar2Show> PP2_Show(int pageIdx, int itemCount, CancellationToken cancelToken)
        {
            return base.PP2_Show(pageIdx, itemCount, cancelToken);
        }

        protected override Task<SearchStatus> PP2_Stat(CancellationToken cancelToken)
        {
            return base.PP2_Stat(cancelToken);
        }

    }
}
