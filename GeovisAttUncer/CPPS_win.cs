﻿using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UncernVis.BivariateRenderer;

namespace GeovisAttUncer
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class CPPS_win : UserControl
    {
        public CPPS_win(object hook)
        {
            InitializeComponent();
            this.Hook = hook;
            try
            {
                IActiveView pActiveView = ArcMap.Document.ActiveView;

                for (int i = 0; i < pActiveView.FocusMap.LayerCount; i++)
                {
                    cboSourceLayer.Items.Add(pActiveView.FocusMap.get_Layer(i).Name);
                }
            }
            catch 
            {
                MessageBox.Show("Unexpected Error");
                return;
            }
        }

        /// <summary>
        /// Host object of the dockable window
        /// </summary>
        private object Hook
        {
            get;
            set;
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private CPPS_win m_windowUI;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new CPPS_win(this.Hook);
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }

        }

        private void cboSourceLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                clsSnippet pSnippet = new clsSnippet();
                IActiveView pActiveView = ArcMap.Document.ActiveView;
                string strLayerName = cboSourceLayer.Text;

                int intLIndex = pSnippet.GetIndexNumberFromLayerName(pActiveView, strLayerName);
                ILayer pLayer = pActiveView.FocusMap.get_Layer(intLIndex);

                IFeatureLayer pFLayer = pLayer as IFeatureLayer;
                ESRI.ArcGIS.Geodatabase.IFeatureClass pFClass = pFLayer.FeatureClass;

                IFields fields = pFClass.Fields;


                cboValueField.Items.Clear();
                cboUField.Items.Clear();

                for (int i = 0; i < fields.FieldCount; i++)
                {
                    cboValueField.Items.Add(fields.get_Field(i).Name);
                    cboUField.Items.Add(fields.get_Field(i).Name);
                }
            }
            catch
            {
                MessageBox.Show("Unexpected Error");
                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                //Declare variables
                clsSnippet pSnippet = new clsSnippet();
                string strLayerName = cboSourceLayer.Text;
                IActiveView pActiveView = ArcMap.Document.ActiveView;

                int intLIndex = pSnippet.GetIndexNumberFromLayerName(pActiveView, strLayerName);
                ILayer pLayer = pActiveView.FocusMap.get_Layer(intLIndex);

                IFeatureLayer pFLayer = pLayer as IFeatureLayer;
                ESRI.ArcGIS.Geodatabase.IFeatureClass pFClass = pFLayer.FeatureClass;

                string strOriRenderField = cboValueField.Text;
                string strUncernRenderField = cboUField.Text;
                double dblMinPtSize = Convert.ToDouble(nudSymbolSize.Value);

                //Create New Layer?
                IGeoFeatureLayer pGeofeatureLayer = null;
                if (chkNewLayer.Checked == true)
                {
                    IFeatureLayer pflOutput = new FeatureLayerClass();
                    pflOutput.FeatureClass = pFClass;
                    pflOutput.Name = txtNewLayer.Text;
                    pflOutput.Visible = true;
                    pGeofeatureLayer = (IGeoFeatureLayer)pflOutput;
                }
                else
                {
                    pGeofeatureLayer = (IGeoFeatureLayer)pFLayer;
                }

                //Find Fields
                ITable pTable = (ITable)pFClass;
                int intOriIdx = pTable.FindField(strOriRenderField);
                int intUncernIdx = pTable.FindField(strUncernRenderField);

                //Find Min and Max Ori Value
                IField pOriField = pTable.Fields.get_Field(intOriIdx);
                ICursor pCursor = pTable.Search(null, true);
                IDataStatistics pDataStat = new DataStatisticsClass();
                pDataStat.Field = pOriField.Name;
                pDataStat.Cursor = pCursor;
                IStatisticsResults pStatResults = pDataStat.Statistics;
                double dblMinOriValue = pStatResults.Minimum;
                double dblMaxOriValue = pStatResults.Maximum;
                pCursor.Flush();

                //Find Min and Max Uncern Vale
                IField pUncernField = pTable.Fields.get_Field(intUncernIdx);
                pCursor = pTable.Search(null, true);
                pDataStat = new DataStatisticsClass();
                pDataStat.Field = pUncernField.Name;
                pDataStat.Cursor = pCursor;
                pStatResults = pDataStat.Statistics;
                double dblMinUncernValue = pStatResults.Minimum;
                double dblMaxUncernValue = pStatResults.Maximum;

                pCursor.Flush();

                //To adjust minn value to 1, if the min value is zero
                double dbladjuctMinvalue = 0;
                if (dblMinOriValue <= 0)
                {
                    dbladjuctMinvalue = (0 - dblMinOriValue) + 1;
                    dblMinOriValue = dblMinOriValue + dbladjuctMinvalue;
                }

                IDisplay pDisplay = pActiveView.ScreenDisplay;

                IRgbColor pSymbolRgb = new RgbColorClass();
                pSymbolRgb.Red = picSymbolColor.BackColor.R;
                pSymbolRgb.Green = picSymbolColor.BackColor.G;
                pSymbolRgb.Blue = picSymbolColor.BackColor.B;

                IRgbColor pLineRgb = new RgbColorClass();
                pLineRgb.Red = picLineColor.BackColor.R;
                pLineRgb.Green = picLineColor.BackColor.G;
                pLineRgb.Blue = picLineColor.BackColor.B;

                int intMethods = 0;
                if (cboMethods.Text == "Saturation")
                    intMethods = 1;
                else if (cboMethods.Text == "Value")
                    intMethods = 2;
                else if (cboMethods.Text == "Whiteness")
                    intMethods = 3;

                IColoringProperties pPreUncern = new ColoringPropClass2();
                //IColoringProperties pPreUncern = new ColoringProp();
                pPreUncern.m_intMethods = intMethods;
                pPreUncern.m_intOriLegendCount = 3; //Needs to be changed
                pPreUncern.m_intUncernLegendCount = 3; //Needs to be changed


                pPreUncern.m_strOriRenderField = strOriRenderField;
                pPreUncern.m_strUncernRenderField = strUncernRenderField;

                pPreUncern.m_pSymbolRgb = pSymbolRgb;
                pPreUncern.m_pLineRgb = pLineRgb;
                pPreUncern.m_dblOutlineSize = Convert.ToDouble(nudLinewidth.Value);
                pPreUncern.m_dblAdjustedMinValue = dbladjuctMinvalue;
                pPreUncern.m_dblMinOriValue = dblMinOriValue;
                pPreUncern.m_dblMaxOriValue = dblMaxOriValue;
                pPreUncern.m_dblMinUncernValue = dblMinUncernValue;
                pPreUncern.m_dblMaxUncernValue = dblMaxUncernValue;
                pPreUncern.m_dblMinPtSize = dblMinPtSize;

                pPreUncern.CreateLegend();
                pGeofeatureLayer.Renderer = (IFeatureRenderer)pPreUncern;


                if (chkNewLayer.Checked == true)
                    pActiveView.FocusMap.AddLayer(pGeofeatureLayer);
                else
                {
                    pFLayer = (IFeatureLayer)pGeofeatureLayer;
                }

                pActiveView.ContentsChanged();
                pActiveView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error");
                return;
            }

        }

        private void chkNewLayer_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNewLayer.Checked)
                txtNewLayer.Enabled = true;
            else
            {
                txtNewLayer.Text = "";
                txtNewLayer.Enabled = false;
            }
        }

        private void picSymbolColor_Click(object sender, EventArgs e)
        {
            DialogResult DR = cdColor.ShowDialog();
            if (DR == DialogResult.OK)
                picSymbolColor.BackColor = cdColor.Color;
        }

        private void picLineColor_Click(object sender, EventArgs e)
        {
            DialogResult DR = cdColor.ShowDialog();
            if (DR == DialogResult.OK)
                picLineColor.BackColor = cdColor.Color;
        }
    }
}
