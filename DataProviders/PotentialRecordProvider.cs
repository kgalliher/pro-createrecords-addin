using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using pro_createrecords_addin.Commands;

namespace pro_createrecords_addin.DataProviders
{
    #region CLASS DOCUMENTATION
    /*
     * ***************************************************************************************************************************************************************
     * Project:         Name of the Project
     * Class:           Name of the Class in the Project
     * Version:         1.0
     * Author:          Name of the person(s) who wrote the script
     * Date Created:    Date Project Created (mm/dd/yyyy)
     * Date Launched:   Date Project launched for general use (mm/dd/yyyy)
     * Dept:            GIS Division
     * Location:        Project file location (...\ArcAdmin\ProjectFolder...)
     * Revisions:       mm/dd/yyyy -programmer-: Summary of modifications to code or Docked Window
     * ***************************************************************************************************************************************************************
     * 
     * CLASS
     * PURPOSE:     A brief explanation for, why this class is needed in the project.
     *             (example- This class contains methods used for querying data in GPUB.)
     *
     * CLASS
     * DESCRIPTION: Describe the functionality or controls contained in the class
     *              (example- This class accepts parameters from AnotherClass.cs to query GPUB and populate a ListArray, which is used by YetAnotherClass.cs.)
     *
     * CLASS
     * PROPERTIES:   Describe the properties (protected or otherwise) for this class.
     *              (example- layer internal variable from AnotherClass.cs)
     *
     * CLASS 
     * METHODS:     Provide a list of the Methods available in this class and a brief explanation of their function.
     *              (example- GetSomeQuery() method accepts parameters from layer variable & accesses xyz feature class in GPUB and returns results in a ListArray.)
     *              (example- ReturnResults() method cycles through the ListArray and populates the message box for the user to review.)
     *
     * CLASS
     * EVENTS:      Provide a list of the Events available in this class and a brief explanation of the actions they represent.
     *              (example- DatabaseConnected event is triggered when a database connection is attempted along with the result.)
     *              (example- RecordRetrieved event is triggered when a database record was returned from a retrieve operation.)
     *
     * CLASS
     * USER
     * INTERFACE:   If the class provides a user interface, describe what the user can do and should expect 
     *              from the interface, otherwise if no user interface provided, leave blank.
     *              (example- This class provides a button control that, when activated, initiates the query.  Results returned are provided in a message box.)
     *
     * SUPPORTING
     * CLASSES
     * AND
     * INTERFACES:  If this class is dependent on other classes or interfaces, list those classes with a brief explanation of their dependency.
     *              (example- a). DCADUtils.cs ==> General functions & methods.)
     *              (example- b). ErrorLog.cs ==> User Event Log controls.)
     *
     * SOURCE
     * DATA
     * CONDITIONS:  Describe if there are specific conditions to be considered for internal/external data access or data formatting
     *              (example- xyz feature class must have Address field populated for the query to return successful results.)
     *
     *
     * SUPPORTING
     * ONLINE
     * DOCUMENTATION: If online documentation was used to create code in this file, then list them with a brief description here. Use https://bit.ly/ to minimize the URL. 
     *                 (example- (1)) List<double> - https://bit.ly/2wFEESu. A system.collections.generic list object of type double.
     *                 (example- (2)) foreach - https://bit.ly/2T16AZT. An iterator for any object type.
     *
     *
     * APPLICABLE
     * STANDARDS: If standards were considered as part of the development they should be listed here with a link if available.
     *            (example- (1) C# Coding Standards - https://bit.ly/r398u779. DCAD standards for C# development.
     *
     *
     * ***************************************************************************************************************************************************************
     * ***************************************************************************************************************************************************************
     */
    #endregion

    public class PotentialRecordProvider 
    {

        #region Constants

        private const string Blank = "";

        private PotentialRecord.PotentialRecordType _potentialRecordType;

        #endregion

        #region Variables

        private ObservableCollection<PotentialRecord> _potentialRecords = new ObservableCollection<PotentialRecord>();

        #endregion

        #region Constructor
        public PotentialRecordProvider(PotentialRecord.PotentialRecordType potentialRecordType)
        {

            // Assign potential record type
            if (potentialRecordType == PotentialRecord.PotentialRecordType.AFC)
            {

                PotentialRecordType = PotentialRecord.PotentialRecordType.AFC;

            }

            else if (potentialRecordType == PotentialRecord.PotentialRecordType.Tile)
            {

                PotentialRecordType = PotentialRecord.PotentialRecordType.Tile;

            }

                   

        }
        #endregion

        #region Properties

        /// <summary>
        /// Edited by data providers
        /// used to add AFC Logs or
        /// Tile records to the
        /// observable collection
        /// bound to the dock pane.
        /// </summary>
        public ObservableCollection<PotentialRecord> PotentialRecords
        {
            get { return _potentialRecords; }
            set { _potentialRecords = value; }
        }

        public PotentialRecord.PotentialRecordType PotentialRecordType
        {

            get { return _potentialRecordType; }
            set { _potentialRecordType = value; }

        }


        #endregion

        #region Methods

        #region Search for Potential Records

        /// <summary>
        /// Update the create records list given the current search text.
        /// If the potential record has already had a record created during the 
        /// current session, then skip this potential records and do not add it
        /// to the Create Records Pane collection. 
        /// </summary>
        public async Task SearchPotentialRecordsAsync()
        {
            // TODO: Create an event or delegate that will execute:
            //       (1) NotifyPropertyChanged => PotentialRecords
            //       (2) Some other task

            if (PotentialRecords.Count > 0)
            {

                ClearAFCLogsCollection();

            }

            await QueuedTask.Run(async () =>
            {
                /***********************************************************************
                * Get a list of Potential Records                                      *
                * **********************************************************************
                * If a cleanup record is requested, this will return tiles that could  *
                * be used to create a cleanup record. Otherwise, AFC logs form the     *
                * database view will be returned.                                      *
                * *********************************************************************/

                if (PotentialRecordType == PotentialRecord.PotentialRecordType.Tile)
                {

                    await SearchingAFeatureLayer(_mapView);

                }

                else
                {

                    await PopulateAFCLogCollection(_searchString);

                }


                // Search for AFC Logs

                // and apply search string

                // if provided

                // TODO: Fix the search capability (duplicates entries when backspacing?)

                IEnumerable<AFCLog> linqResults;

                if (_searchString != Blank)
                {
                    linqResults = potentialRecords.Where(pr => pr.RecordName.Contains(_searchString));

                }
                else
                {
                    linqResults = potentialRecords.Where(pr => pr.TileNumber > 0);
                }



                // Create a temporary observable collection

                // for filtering

                ObservableCollection<PotentialRecord> _tempPotentialRecords;

                // Filter the items in the existing observable collection

                _tempPotentialRecords = new ObservableCollection<PotentialRecord>(linqResults);

                // Compare temporary collection with the original.

                // Remove any items from the original collection

                // that do not appear in the temporary collection.

                for (int i = _afclogs.Count - 1; i >= 0; i--)
                {
                    var item = _afclogs[i];
                    if (!_tempPotentialRecords.Contains(item))
                    {
                        lock (_lockObj)
                        {
                            _afclogs.Remove(item);
                        }

                    }
                }

                // Now add any items that are included in

                // the temporary collection that are not in

                // the original collection in the case of a

                // backspace

                foreach (var item in _tempPotentialRecords)
                {
                    if (!_afclogs.Contains(item))
                    {
                        lock (_lockObj)
                        {
                            _afclogs.Add(item);
                        }

                    }
                }

                /**********************************************
                 * Remove any items that are included in
                 * the records collection because these
                 * have already had a record created
                 * during this session.
                 * *******************************************/

                // Remove temporary observable collection

                _tempPotentialRecords = null;



            });

            // Call NotifyPropertyChanged and pass in the AFCLogs property

            //NotifyPropertyChanged(() => PotentialRecords);


        }

        #endregion

        #endregion
    }
}
