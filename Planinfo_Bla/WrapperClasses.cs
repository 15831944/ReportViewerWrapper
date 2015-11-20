﻿
namespace COR_Reports
{


    public class ReportDataSource
    {

        public string DataMember { get; set; }

        public string DataSourceId { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }


        public ReportDataSource()
        { }

        public ReportDataSource(string pName)
        {
            this.Name = pName;
        }

        public ReportDataSource(string pName, object dataSourceValue)
            : this(pName)
        {
            this.Value = dataSourceValue;
        }

        public ReportDataSource(string pName, string pDataSourceId)
            : this(pName)
        {
            this.DataSourceId = pDataSourceId;
        }

    } // End Class ReportDataSource 


    public class ReportParameter
    {

        public string Name;
        public string[] Values;
        public bool Visible;


        public ReportParameter()
        { }

        public ReportParameter(string pName)
        {
            this.Name = pName;
        }


        public ReportParameter(string pName, string[] pValues)
            : this(pName)
        {
            this.Values = pValues;
        }


        public ReportParameter(string pName, string pValue)
            : this(pName, new string[] { pValue })
        { }


        public ReportParameter(string pName, string pValue, bool pVisible)
            : this(pName, pValue)
        {
            this.Visible = pVisible;
        }

        public ReportParameter(string pName, string[] pValues, bool pVisible)
            : this(pName, pValues)
        {
            this.Visible = pVisible;
        }

    } // End Class ReportParameter


    public class ReportViewer : System.IDisposable
    {

        private Microsoft.Reporting.WebForms.ReportViewer m_Viewer;


        public ReportViewer()
        {
            this.m_Viewer = new Microsoft.Reporting.WebForms.ReportViewer();
            this.DataSources = new cDataSource(this.m_Viewer);
            this.EnableFormat("HTML4.0");
        }


        // http://web.archive.org/web/20120101201615/http://beaucrawford.net/post/Enable-HTML-in-ReportViewer-LocalReport.aspx
        private void EnableFormat(string formatName)
        {
            const System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;


            System.Reflection.FieldInfo m_previewService = this.m_Viewer.LocalReport.GetType().GetField
            (
                "m_previewService",
                Flags
            );

            System.Reflection.MethodInfo ListRenderingExtensions = m_previewService.FieldType.GetMethod
            (
                "ListRenderingExtensions",
                Flags
            );

            object previewServiceInstance = m_previewService.GetValue(this.m_Viewer.LocalReport);

            System.Collections.IList extensions = ListRenderingExtensions.Invoke(previewServiceInstance, null) as System.Collections.IList;

            System.Reflection.PropertyInfo name = extensions[0].GetType().GetProperty("Name", Flags);

            foreach (object extension in extensions)
            {
                if (string.Compare(name.GetValue(extension, null).ToString(), formatName, true) == 0)
                {
                    System.Reflection.FieldInfo m_isVisible = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    System.Reflection.FieldInfo m_isExposedExternally = extension.GetType().GetField("m_isExposedExternally", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    m_isVisible.SetValue(extension, true);
                    m_isExposedExternally.SetValue(extension, true);
                    break;
                }
            }
        }


        public string this[string name]
        {
            get
            {
                return this.m_Viewer.Style[name];
            }
            set
            {
                this.m_Viewer.Style[name] = value;
            }

        }

        public void SetParameters(System.Collections.Generic.List<ReportParameter> lsParameters)
        {
            SetParameters(lsParameters.ToArray());
        }


        public void SetParameters(ReportParameter[] parameters)
        {

            Microsoft.Reporting.WebForms.ReportParameter[] para = new Microsoft.Reporting.WebForms.ReportParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                para[i] = new Microsoft.Reporting.WebForms.ReportParameter(parameters[i].Name, parameters[i].Values, parameters[i].Visible);
            }

            this.m_Viewer.LocalReport.SetParameters(para);
        }


        public class cDataSource
        {
            private Microsoft.Reporting.WebForms.ReportViewer mm_Viewer;

            public cDataSource(Microsoft.Reporting.WebForms.ReportViewer vwr)
            {
                this.mm_Viewer = vwr;
            }


            public void Add(ReportDataSource pRDS)
            {
                Microsoft.Reporting.WebForms.ReportDataSource rds = new Microsoft.Reporting.WebForms.ReportDataSource();
                rds.DataMember = pRDS.DataMember;
                rds.DataSourceId = pRDS.DataSourceId;
                rds.Name = pRDS.Name;
                rds.Value = pRDS.Value;

                this.mm_Viewer.LocalReport.DataSources.Add(rds);
            }
        }

        public cDataSource DataSources;


        public string ReportPath
        {
            get{
                return this.m_Viewer.LocalReport.ReportPath;
            }
            set{
                this.m_Viewer.LocalReport.ReportPath = value;
            }
        }


        public void LoadReportDefinition(System.IO.Stream report)
        {
            this.m_Viewer.LocalReport.LoadReportDefinition(report);
        }

        public void LoadReportDefinition(System.IO.TextReader report)
        {
            this.m_Viewer.LocalReport.LoadReportDefinition(report);
        }


        public void LoadEmbeddedReportDefinition(string reportName)
        {
            using (System.IO.Stream strm = ReportRepository.GetEmbeddedReport(reportName))
            {
                this.LoadReportDefinition(strm);
            } // End Using strm 
        }


        public byte[] Render(string FormatInfo)
        {
            Microsoft.Reporting.WebForms.Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string extension;

            return this.m_Viewer.LocalReport.Render(FormatInfo, "", out mimeType, out encoding, out extension, out streamids, out warnings);
        }
        


        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DataSources = null;

                // free managed resources
                if (this.m_Viewer != null)
                    this.m_Viewer.Dispose();
            }
            // free native resources if there are any.
        }

    } // End Class 


} // End Namespace COR_Reports
