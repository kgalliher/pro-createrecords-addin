using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using DCAD.GIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pro_createrecords_addin.DataProviders
{
    public class AFCLogProvider
    {

        #region Constants

        private static readonly string Instance = "DCADSQLVM02";

        private static readonly string Database = "GEDT";

        private const AuthenticationMode Authentication = AuthenticationMode.OSA;

        private static readonly string Version = "dbo.DEFAULT";

        private static readonly string AFCView = "ADM.AFC_LOG_VW";

        private const string Yes = "Y";

        private const string Blank = "";

        private static readonly string TileLayerName = "DCAD Tiles";

        private static readonly int CleanupRecordType = 28;

        #endregion

        #region Constructor

        public AFCLogProvider()
        {

        }

        #endregion

        #region Methods

        #region Populates AFCLog Collection

        /// <summary>
        /// Pulls data from the view retrieving
        /// AFC logs from the Mars database and
        /// puts them into an AFC Log collection
        /// for binding to the dock pane.
        /// </summary>
        /// <param name="_searchString">A set of characters used to filter
        /// the current AFC logs assigned to the authenticated user.</param>
        /// <returns></returns>
        public async Task PopulateAFCLogCollectionAsync(string _searchString)
        {
            // Define columns to be included in

            // query filter

            string _instNum = "INSTRUMENT_NUM";

            string _seqNum = "SEQ_NUM";

            string _afcLogID = "AFC_LOG_ID";

            string _whereClause = Blank;

            bool _acctNumBlank = false;

            int _docNumType = 1;         // Helps the SetForegroundColor method know the color type for DOC_NUM.

            int _acctNumTYpe = 2;        // Helps the SetForegroundColor method know the color type for ACCOUNT_NUM.

            // Define where clause based

            // on search string contents

            if (_searchString != Blank)
            {
                _whereClause = String.Format("{0} LIKE '%{1}%' OR {2} LIKE '%{1}%'", _instNum, _searchString, _seqNum);
            }

            // Multi-threaded synchronization

            //private Object _lockObj = new object();

            //BindingOperations.EnableCollectionSynchronization(_afclogsRO, _lockObj);

            try
            {
                await QueuedTask.Run(() => {

                    // Opening a Non-Versioned SQL Server instance.
                    DatabaseConnectionProperties connectionProperties =
                    CreateNewConnectionPropObj(EnterpriseDatabaseType.SQLServer,
                    Authentication, Database, Instance, Version);

                    using (Geodatabase geodatabase = new Geodatabase(
                        connectionProperties))
                    {
                        using (Table table = geodatabase.OpenDataset<Table>(AFCView))
                        {

                            QueryFilter queryFilter = new QueryFilter
                            {
                                WhereClause = _whereClause,

                                SubFields = "*",

                                PostfixClause = String.Format("ORDER BY {0} ASC", _afcLogID)
                            };

                            using (RowCursor rowCursor = table.Search(queryFilter, false))
                            {


                                /* ***********************************
                                 * Search through returned rows from *
                                 * query filter and create a new     *
                                 * AFC Log object and bind to the    *
                                 * AFCLogs observable collection.    *
                                 * ***********************************/
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        AFCLog afcLog = new AFCLog();

                                        // Determine AFC NOTE length and truncate if longer than 35 chars

                                        int _afcNoteLength = Convert.ToString(row["AFC_NOTE"]).Length;

                                        if (_afcNoteLength > 35) _afcNoteLength = 35;

                                        afcLog.AFC_LOG_ID = Convert.ToInt32(row["AFC_LOG_ID"]);

                                        afcLog.AFC_STATUS_CD = Convert.ToInt32(row["AFC_STATUS_CD"]);

                                        afcLog.AFC_TYPE_CD = Convert.ToInt32(row["AFC_TYPE_CD"]);

                                        afcLog.AFC_YEAR = Convert.ToInt32(row["AFC_YEAR"]);

                                        afcLog.AFC_NOTE = Convert.ToString(row["AFC_NOTE"]).Substring(0, _afcNoteLength);

                                        afcLog.TILE_NO = Convert.ToInt32(row["TILE_NO"]);

                                        afcLog.DRAFTER_EMPL_ID = Convert.ToString(row["DRAFTER_EMPL_ID"]);

                                        afcLog.DRAFTER_COMP_DT = Convert.ToDateTime(row["DRAFTER_COMP_DT"]);

                                        afcLog.FILE_DATE = Convert.ToDateTime(row["FILE_DATE"]);

                                        afcLog.EFFECTIVE_DT = Convert.ToDateTime(row["EFFECTIVE_DT"]);

                                        afcLog.INSTRUMENT_NUM = Convert.ToString(row["INSTRUMENT_NUM"]);

                                        afcLog.SEQ_NUM = Convert.ToString(row["SEQ_NUM"]);

                                        afcLog.RUSH_IND = Convert.ToString(row["RUSH_IND"]) == Yes ? true : false;

                                        // Determine if Account Number is provided

                                        if (Convert.ToString(row["ACCOUNT_NUM"]).Equals(Blank)) _acctNumBlank = true;

                                        if (!_acctNumBlank) afcLog.ACCOUNT_NUM = Convert.ToString(row["ACCOUNT_NUM"]);

                                        //afcLog.ACCT_LIST = Convert.ToString(row["ACCT_LIST"]);

                                        afcLog.DOC_TYPE = Convert.ToString(row["DOC_TYPE"]);

                                        afcLog.SetImageSource();    // Method sets the image source for the afc log type

                                        afcLog.SetDocumentNumber(); // Method sets the document number for the afc log type

                                        afcLog.SetRecordType();     // Method sets the record type for the afc log

                                        // Set the record status based on

                                        // the AFC status code

                                        afcLog.SetRecordStatus();   // Method that sets the record status for the afc log

                                        /***************************************
                                        * Subscribe to AFCRecordCreated Event  *
                                        * in the AFCRecord class.              *
                                        * *********************************** */

                                        afcLog.AFCRecordCreatedEvent += OnAFCRecordCreated;

                                        /***********************************************
                                        * Set the foreground color for the document    *
                                        * and account number properties based on the   *
                                        * RUSH_IND (if yes == RED else Black/Gray      *
                                        * *********************************************/

                                        afcLog.SetForegroundColor(_docNumType);

                                        afcLog.SetForegroundColor(_acctNumTYpe);

                                        // Reads and Writes should be made from within the lock

                                        lock (_lockObj)
                                        {
                                            _afclogs.Add(afcLog);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // If completed, add psuedo afc log if count is zero

                    AddEmptyListAFCLog(_afclogs.Count);
                });
            }
            catch (GeodatabaseFieldException fieldException)
            {
                // One of the fields in the where clause might not exist. 

                // There are multiple ways this can be handled:

                // Handle error appropriately

                OS.WriteLogEntry(OS.EventLogSourceName, fieldException.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            catch (Exception exception)
            {
                OS.WriteLogEntry(OS.EventLogSourceName, exception.Message, System.Diagnostics.EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// When a record is created, the dock pane should
        /// refresh and remove the newly created AFC log
        /// from the observable collection based on
        /// the database view. The database view involves
        /// a join on the records feature class and removes
        /// any AFC Logs whose document number matches an
        /// existing record in the feature class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAFCRecordCreated(object sender, AFCRecordCreatedEventArgs e)
        {
            try
            {

                    await SearchForPotentialRecordsAsync();

            }
            catch (Exception exception)
            {

                OS.WriteLogEntry(OS.EventLogSourceName, exception.Message, System.Diagnostics.EventLogEntryType.Error);
            }

        }



        #region Add Empty Result List AFC Log
        /// <summary>
        /// Adds a psuedo afc log entry to the
        /// observable collection to send a message
        /// to the user to assign and afc log.
        /// </summary>
        /// <param name="_afcCount"></param>
        private void AddEmptyListAFCLog(int _afcCount)
        {

            /***********************************************
            * DEFAULT BEHAVIOR WHEN NO AFC LOG ASSIGNED   *
            * *********************************************
            * Displays a default afc log row in the       *
            * observable collection. See details in       *
            * the CleanupRecord class.                    *
            * This type of afc log is not valid, but just *
            * displays an empty marker with message.      *
            **********************************************/

            if (_afcCount == 0)
            {
                AFCLog afcLog = new AFCLog();

                afcLog.AFC_LOG_ID = 1;

                afcLog.AFC_STATUS_CD = 99;

                afcLog.VALID_AFC_LOG = false;

                afcLog.SetImageSource();

                afcLog.SetDocumentNumber();


                lock (_lockObj)
                {
                    _afclogs.Add(afcLog);
                }


            }


        }
        #endregion

        #region Create New Connection Properties Object

        /// <summary>
        /// Creates a new connection
        /// properties object
        /// from defined parameters
        /// for the enterprise
        /// database connection.
        /// </summary>
        /// <param name="dbms">The RDBMS type (e.g., SQL Server, PostgresSQL, etc.).</param>
        /// <param name="auth">The authentication mode (e.g., OSA or Database).</param>
        /// <param name="db">The name of the database. </param>
        /// <param name="instance">The name of the database server instance.</param>
        /// <param name="version">The name of the geodatabase version.</param>
        /// <returns></returns>
        public DatabaseConnectionProperties CreateNewConnectionPropObj(
                        EnterpriseDatabaseType dbms,
                        AuthenticationMode auth,
                        string db,
                        string instance,
                        string version)
        {
            DatabaseConnectionProperties connectionProperties =
                new DatabaseConnectionProperties(dbms)
                {
                    AuthenticationMode = auth,

                    Database = db,

                    Instance = instance,

                    Version = version
                };

            return connectionProperties;
        }

        #endregion

        #endregion

        #endregion



    }
}
