using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using picibird.wpf.app;

namespace picibird.hbs.wpf
{
    public class HbsWindow : PiciWindow
    {

        public HbsWindow() : base()
        {

        }

        protected override void DisableWpfTabletSupport()
        {
            //base.DisableWpfTabletSupport();
        }
    }
}