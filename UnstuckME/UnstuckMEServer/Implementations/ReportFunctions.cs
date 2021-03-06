﻿using System;
using System.Linq;
using System.Threading.Tasks;
using UnstuckMEServer;
using UnstuckME_Classes;

namespace UnstuckMEInterfaces
{
    public partial class UnstuckMEService : IUnstuckMEService, IUnstuckMEServer, IUnstuckMEFileStream
    {
        /// <summary>
        /// Submits a report to the database and notifies online admins of the sent report.
        /// </summary>
        /// <param name="reportDescription">The description of the report.</param>
        /// <param name="flaggerID">The unique identifier of the client who submitted the report.</param>
        /// <param name="reviewID">The unique idenitifer of the review that is being reported.</param>
        /// <returns>The unique identifier of the newly submitted report if successul, -1 if unsuccessful.</returns>
        public async Task<int> CreateReport(string reportDescription, int flaggerID, int reviewID)
        {
            int retVal = -1;

            try
            {
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.CreateReport(reportDescription, flaggerID, reviewID);

                    if (retVal != -1)
                        await Task.Factory.StartNew(() => AsyncNotifyAdmin(reportDescription, flaggerID, reviewID));
                }

                return retVal;
            }
            catch (Exception)
            {
                return retVal; //If Failure to create report
            }
        }

        /// <summary>
        /// Finds online admins and notifies them that a report has been submitted.
        /// </summary>
        /// <param name="reportDescription">The description of the report.</param>
        /// <param name="flaggerID">The unique identifier of the client who submitted the report.</param>
        /// <param name="reviewID">The unique idenitifer of the review that is being reported.</param>
        private void AsyncNotifyAdmin(string reportDescription, int flaggerID, int reviewID)
        {
            foreach (var client in _connectedClients)
            {
                if (client.Value.User.Privileges == Privileges.Admin)
                {
                    string flagger = GetUserDisplayName(flaggerID);
                    client.Value.Connection.GetMessageFromServer(string.Format("{0} [userID {1}] has reported a review [reviewID = {2}]. The report reads as follows:\n\n{3}",
                                                                               flagger, flaggerID, reviewID, reportDescription));
                }
            }
        }

        /// <summary>
        /// Delete a report submitted by a specific user. More than likely does not work, do not use.
        /// </summary>
        /// <param name="userID">The unique identifier of the user who submitted the report.</param>
        /// <param name="reportID">The unique identifier of the report to be removed.</param>
        /// <returns>Returns 0 if successful, -1 if unsuccessful</returns>
        public int DeleteReportByUser(int userID, int reportID)
        {
            int retVal = -1;

            try
            {
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    //var report = from u in db.Reports
                    //where reportID == u.ReportID
                    //select new { ReportID = u.ReportID, ReporterID = u.FlaggerID };

                    //if (report.First().ReporterID == userID)
                    //{
                    //	db.DeleteReportByReportID(reportID);
                    //}
                    //else
                    //{
                    //	throw new Exception();
                    //}

                    //not sure if this works
                    var report = db.GetReportsSubmittedByUser(userID);

                    for (int i = 0; report.ElementAt(i).ReportID == reportID; i++)
                    {
                        if (report.ElementAt(i).ReportID == reportID)
                            retVal = db.DeleteReportByReportID(reportID);
                    }
                }

                return retVal;
            }
            catch (Exception)
            {
                return retVal; //failure to find or delete report
            }
        }
    }
}