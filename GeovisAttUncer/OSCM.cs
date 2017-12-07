using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace GeovisAttUncer
{
    public class OSCM : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public OSCM()
        {
        }

        protected override void OnClick()
        {
            //
            //  TODO: Sample code showing how to access button host
            //
            ArcMap.Application.CurrentTool = null;
            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.OSCM_win;

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
