using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAD.GIS
{

    #region Class Library Documentation

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

    public class Data
    {


        #region Obtaining Geodatabase from FeatureLayer

        public async Task ObtainingGeodatabaseFromFeatureLayer()
        {
            IEnumerable<Layer> layers = MapView.Active.Map.Layers.Where(layer => layer is FeatureLayer);

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {
                foreach (FeatureLayer featureLayer in layers)
                {
                    using (Table table = featureLayer.GetTable())
                    using (Datastore datastore = table.GetDatastore())
                    {
                        if (datastore is UnknownDatastore)
                            continue;

                        Geodatabase geodatabase = datastore as Geodatabase;
                    }
                }
            });
        }

        #endregion Obtaining Geodatabase from FeatureLayer

    }
}
