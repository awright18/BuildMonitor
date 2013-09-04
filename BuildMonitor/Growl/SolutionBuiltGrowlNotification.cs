using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growl.Connector;

namespace BuildMonitor.Growl
{
    public sealed class SolutionBuiltGrowlNotification:Notification
    {
        public SolutionBuiltGrowlNotification(string solutionName, string time):base("Build Monitor","DoneBuildingSolution",null,"Done Building Solution",string.Format("Finished Building {0} in {1}",solutionName,time))
        {
            base.Icon = icon.build_icon.ToBitmap();

        }

    }
}
