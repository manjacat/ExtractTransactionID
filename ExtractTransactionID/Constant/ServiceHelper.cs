using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTransactionID.Constant
{
    public enum ServiceCode
    {
        BCRM_Update,
        Create_CPP,
        Create_SNC,
        Update_CPP,
        Update_SNC,
        ERMS_Update
    }

    public class ServiceHelper
    {
        public ServiceCode ServiceCode { get; set; }
        public string FolderName { get; set; }
        public string InboundServiceName { get; set; }
        public string ServerPath { get; set; }
        public string FileNameContains { get; set; }

        /*
        * UpdateDistProjectStatusService = update CPP
        * UpdateDistSNCProjectStatusService  = update SNC
        * CreateNewDistProjectService = Create CPP
        * UpdateCustomerAttributesService = BCRM
        * UpdateAssetAttribService = ERMS
        * CreateDistSNCProjectService = Create SNC
        * 
        * BCRMFolder = ConfigurationManager.AppSettings["BCRMFolder"].ToString();
        * DistProjectFolder = ConfigurationManager.AppSettings["DistProjectFolder"].ToString();
        * DistProjectUpdateFolder = ConfigurationManager.AppSettings["DistProjectUpdateFolder"].ToString();
        * ERMSFolder = ConfigurationManager.AppSettings["ErmsFolder"].ToString();
        */

        public string ServiceName { get; set; }

        public ServiceHelper(ServiceCode code)
        {
            switch (code)
            {
                case (ServiceCode.BCRM_Update):
                    ServiceCode = ServiceCode.BCRM_Update;
                    InboundServiceName = "%UpdateCustomerAttributes%";
                    ServiceName = "UpdateCustomerAttributesService";
                    FolderName = ConfigurationManager.AppSettings["BCRMFolder"].ToString();
                    FileNameContains = "Update_BCRM_Records";
                    break;
                case (ServiceCode.Create_CPP):
                    ServiceCode = ServiceCode.Create_CPP;
                    InboundServiceName = "%CreateDistProject%";
                    ServiceName = "CreateNewDistProjectService";
                    FolderName = ConfigurationManager.AppSettings["DistProjectFolder"].ToString();
                    FileNameContains = "Create_CPP_Projects";
                    break;
                case (ServiceCode.Create_SNC):
                    ServiceCode = ServiceCode.Create_SNC;
                    InboundServiceName = "%CreateDistSNCProject%";
                    ServiceName = "CreateDistSNCProjectService";
                    FolderName = ConfigurationManager.AppSettings["DistProjectFolder"].ToString();
                    FileNameContains = "Create_SNC_Projects";
                    break;
                case (ServiceCode.Update_CPP):
                    ServiceCode = ServiceCode.Update_CPP;
                    InboundServiceName = "%UpdateDistProjectStatus%";
                    ServiceName = "UpdateDistProjectStatusService";
                    FolderName = ConfigurationManager.AppSettings["DistProjectUpdateFolder"].ToString();
                    FileNameContains = "Update_CPP_Records";
                    break;
                case (ServiceCode.Update_SNC):
                    ServiceCode = ServiceCode.Update_SNC;
                    InboundServiceName = "%UpdateDistSNCProject%";
                    ServiceName = "UpdateDistSNCProjectStatusService";
                    FolderName = ConfigurationManager.AppSettings["DistProjectUpdateFolder"].ToString();
                    FileNameContains = "Update_SNC_Records";
                    break;
                case (ServiceCode.ERMS_Update):
                default:
                    ServiceCode = ServiceCode.ERMS_Update;
                    InboundServiceName = "%UpdateERMSPMAssetAttrib%";
                    ServiceName = "UpdateAssetAttribService";
                    FolderName = ConfigurationManager.AppSettings["ErmsFolder"].ToString();
                    FileNameContains = "Update_Asset_Attrib_Service";
                    break;
            }
        }
    }
}
