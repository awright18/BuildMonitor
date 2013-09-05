using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growl.Connector;

namespace BuildMonitor.Growl
{
    public class SolutionLoadedGrowlNotification:Notification
    {
        public SolutionLoadedGrowlNotification(string solutionName)
            : base("Build Monitor", "SolutionLoaded", null, "Solution Loaded", string.Format("Finished Loading {0}", solutionName))
        {
            base.Icon = icon.build_icon.ToBitmap();

        }
    }
}
