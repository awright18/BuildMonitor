using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growl.Connector;

namespace BuildMonitor.Growl
{
    public sealed class GrowlNotifier
    {
        private bool _isRegistered;
        private GrowlConnector _connector;

        private GrowlNotifier()
        {
             _connector = new GrowlConnector();

             NotificationType buildNotificationType = new NotificationType("DoneBuildingSolution", "Done Building Solution", icon.build_icon.ToBitmap(), true);

            _connector.Register(new Application("Build Monitor"){ Icon= icon.build_icon.ToBitmap()}, new[] { buildNotificationType });
        }

        public void Notify(Notification notification)
        {
            _connector.Notify(notification);
        }

        static GrowlNotifier()
        {
        }

        public static GrowlNotifier Instance
        {
            get { return NestedGrowlNotifier.instance; }
        }

        private class NestedGrowlNotifier
        {
            public static readonly GrowlNotifier instance = new GrowlNotifier();
        }
    }
}
