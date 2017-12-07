using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace GeovisAttUncer
{
    public class CPPS : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public CPPS()
        {
        }

        protected override void OnClick()
        {
            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.CPPS_win;

            IDockableWindow dockWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);

            if (dockWindow == null)
                return;

            dockWindow.Show(!dockWindow.IsVisible());
        }

        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }
}
