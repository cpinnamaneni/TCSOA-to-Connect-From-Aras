using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teamcenter.ClientX;
using Teamcenter.Services.Strong.Core;
using Teamcenter.Services.Strong.Query;
//using Teamcenter.Soa.Client.Model.Strong.User;
using User = Teamcenter.Soa.Client.Model.Strong.User;
using Item = Teamcenter.Soa.Client.Model.Strong.Item;
using ItemRevision = Teamcenter.Soa.Client.Model.Strong.ItemRevision;


///using Teamcenter.Services.Strong.Core._2007_01.DataManagement;
using Teamcenter.Soa.Client.Model.Strong;
using Teamcenter.Soa.Client.Model;
using Teamcenter.Services.Strong.Core._2007_01.Session;
using Teamcenter.Services.Strong.Core._2006_03.FileManagement;
using System.Collections;
using Teamcenter.Services.Strong.Core._2007_12.Session;
using Teamcenter.Services.Strong.Core._2010_04.Session;

using Teamcenter.Services.Strong.Query._2006_03.SavedQuery;
using Teamcenter.Schemas.Soa._2006_03.Exceptions;

using ImanQuery = Teamcenter.Soa.Client.Model.Strong.ImanQuery;
using SavedQueriesResponse = Teamcenter.Services.Strong.Query._2007_09.SavedQuery.SavedQueriesResponse;
using QueryInput = Teamcenter.Services.Strong.Query._2008_06.SavedQuery.QueryInput;
using QueryResults = Teamcenter.Services.Strong.Query._2007_09.SavedQuery.QueryResults;
using Teamcenter.Services.Strong.Query._2010_04.SavedQuery;
using Teamcenter.Services.Strong.Core._2010_09.DataManagement;
using Teamcenter.Services.Strong.Core._2008_06.DataManagement;
using Teamcenter.Services.Strong.Core._2007_01.DataManagement;
using System.Globalization;

namespace TCSOA_To_Connect_TC_from_Aras
{
    class UpdateInTC
    {
        private String userid = null;
        private String password = null;
        private String group = null;
        private String role = "";
        private String volume = "";

        private String serverHost = null;
        private Teamcenter.ClientX.Session session;
        private static DataManagementService dmService;
        private SessionService sessionService;
        private SavedQueryService queryService;
        private FileManagementService fileMgtService;

        static String localUserId = null;
        static String localPasswd = null;
        static String localGroup = null;
        static String localServerHost = null;

        static String actionStr = null;
        static String outputDir = null;

        private static User user = null;

        public UpdateInTC(String tcUserid, String tcPassword, String tcGroup, String tcServerHost)
        {
            userid = tcUserid;
            password = tcPassword;
            group = tcGroup;
            serverHost = tcServerHost;
            initialize();
            user = login();
            byPassPrivileges();
        }


        public void initialize()
        {
            session = new Teamcenter.ClientX.Session(serverHost);

            dmService = DataManagementService.getService(Teamcenter.ClientX.Session.getConnection());
            //prefService = PreferenceManagementService.getService(Session.getConnection());
            sessionService = SessionService.getService(Teamcenter.ClientX.Session.getConnection());
            queryService = SavedQueryService.getService(Teamcenter.ClientX.Session.getConnection());
            fileMgtService = FileManagementService.getService(Teamcenter.ClientX.Session.getConnection());
        }

        public User login()
        {
            return session.login(userid, password, group, role);
            //return session.login();
        }

        public void logout()
        {
            session.logout();
        }

        public void byPassPrivileges()
        {
            try
            {
                GetTCSessionInfoResponse sessionInfoResponse = sessionService.GetTCSessionInfo();
                if (sessionInfoResponse != null)
                {
                    if (!(sessionInfoResponse.Bypass))
                    {
                        System.Console.WriteLine("Bypass is not set.....");
                        System.Console.WriteLine("Bypassing the privileges....");

                        StateNameValue[] stateName = new StateNameValue[1];
                        stateName[0] = new StateNameValue();
                        //stateName[0].setName("bypassFlag");
                        //stateName[0].setValue("true");

                        stateName[0].Name = "bypassFlag";
                        stateName[0].Value = "true";
                        sessionService.SetUserSessionState(stateName);

                        sessionInfoResponse = sessionService.GetTCSessionInfo();
                        if (sessionInfoResponse.Bypass)
                            System.Console.WriteLine("Successfully set the bypass...");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }


        static void Main(string[] args)
        {

            UpdateInTC UpdateInTCObj = new UpdateInTC("georpras", "georpras", "", "http://detmscplmdev08.magna.global:8080/tc");


            createECNObject();            
        }

        private static void createECNObject()
        {
            // The create input for the ChangeNotice Item
            Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateInput itemCreateIn = new Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateInput();
            itemCreateIn.BoName = "A9_AutoCN";
            
            itemCreateIn.StringProps.Add("object_name", "Name of A9_AutoCN");
            itemCreateIn.StringProps.Add("object_desc", "Description of A9_AutoCN");


            // The create input for the ChangeNoticeRevision
            Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateInput revisionCreateIn = new Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateInput();
            revisionCreateIn.BoName = "A9_AutoCNRevision";
            //revisionCreateIn.StringProps.Add("item_revision_id", "A");

            DateTime currentdate = DateTime.Now;
            DateTime SyncStartdate = new DateTime(currentdate.Year, currentdate.Month, currentdate.Day, currentdate.Hour, currentdate.Minute, currentdate.Second);
            String SyncStartdateStr = SyncStartdate.ToString("yyyyMMMddHHmmsssss");

            //Calendar cal = Calendar..getInstance();
            //cal.SetTime(new Date());

            DateTime dateValue = Teamcenter.Soa.Client.Model.Property.ParseDate(currentdate.ToString());


            revisionCreateIn.DateProps.Add("a9_EstImpDate", dateValue);


            // Tie the Revision CreateInput to the Item CreateInput
            itemCreateIn.CompoundCreateInput.Add("revision", new Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateInput[] { revisionCreateIn });

            // The data for the createObjects call
            Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateIn cnCreateIn = new Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateIn();
            cnCreateIn.ClientId = "Create ECN-10000";
            cnCreateIn.Data = itemCreateIn;

            CreateResponse createResponse = dmService.CreateObjects(new Teamcenter.Services.Strong.Core._2008_06.DataManagement.CreateIn[] { cnCreateIn });
            if (createResponse.ServiceData.sizeOfPartialErrors() > 0)
            {
                //logErrors(createResponse.serviceData);
            }
            else
            {
                //for (DataManagement.CreateOut createOut : createResponse.output)
                //{
                //    logger.info("Response for client ID " + createOut.clientId);
                //    for (ModelObject modelObject : createOut.objects)
                //    {
                //        logger.info("Created Object " + modelObject.getTypeObject().getName() + " : " + modelObject.getUid());
                //    }
                //}
            }
        }
    }
}
