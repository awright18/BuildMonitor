using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BuildMonitor.Domain;
using Growl.Connector;
using Growl.CoreLibrary;
namespace BuildMonitor.Growl
{
    public sealed class SendBuildCompleteGrowlNotification
    {
        private string _solutionName;
        private string _time;

        public SendBuildCompleteGrowlNotification(string solutionName,TimeSpan buildTime)
        {
            _solutionName = solutionName;
            
            if (!string.IsNullOrWhiteSpace(_time) && buildTime.Hours > 0)
            {
                _time = string.Format("{0} hours, {1} minutes, {2} seconds",buildTime.Hours, buildTime.Minutes, buildTime.Seconds);
            }

            if (!string.IsNullOrWhiteSpace(_time) && buildTime.Minutes > 0)
            {
                _time = string.Format("{0} minutes, {1} seconds", buildTime.Minutes, buildTime.Seconds);
            }

            if (!string.IsNullOrWhiteSpace(_time) && buildTime.Seconds > 0)
            {
                _time = string.Format("{0} seconds", buildTime.Seconds);
            }
            else
            {
                _time = string.Format("{0} milliseconds", buildTime.Milliseconds);
            }
            
           
        }

        public void Execute()
        {
            
            GrowlNotifier.Instance.Notify(new SolutionBuiltGrowlNotification(_solutionName, _time));
        }
    }
}
