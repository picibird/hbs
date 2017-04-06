using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using picibird.wpf.app;
using picibits.app.services;
using picibits.app.window;

namespace picibird.hbs.wpf
{
    public class HbsWindows : IWindows
    {
        private IWindow mMainWindow;
        public IWindow MainWindow
        {
            get
            {
                if (mMainWindow == null)
                {
                    mMainWindow = new HbsWindow();
                }
                return mMainWindow;
            }
        }


        public IWindow CreateWindow()
        {
            return new HbsWindow();
        }
    }
}
