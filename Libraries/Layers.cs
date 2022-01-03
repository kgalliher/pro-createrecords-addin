using ArcGIS.Desktop.Framework.Threading.Tasks;
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


    public static class Layers
    {

        #region Methods


        // Searches the Table of Contents for matching layer name to see if it exists
        #region ConfirmTOCLayer()- Checks Active Map to confirm if a query layer exists in the Table of Contents
        /// <summary>
        /// Confirms if a feature layer is listed in the Active Map's TOC.
        /// </summary>
        /// <param name="fLyr">Feature layer name as it appears in the TOC</param>
        /// <returns>true or false</returns>
        public static bool ConfirmTOCLayer(string fLyr)
        {
            var activeMap = MapView.Active.Map;
            var mapName = activeMap.Name.ToString();

            // Check the Table of Contents to confirm that the query layer exists.
            var maplyrExists = activeMap.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(f => f.Name == fLyr);

            return maplyrExists;
        }
        #endregion

        // Selects a specific layer in the Table of Contents
        #region SelectTOCLayer()- selects a feature layer in the TOC. (WORKING)
        /// <summary>
        /// Selects a feature layer in the Active Map's TOC.  !Important!- This selects ONLY feature layers NOT nested in a group.
        /// </summary>
        /// <param name="fLyr">Feature layer name as it appears in the TOC.</param>
        public static void SelectTOCLayer(string fLyr)
        {
            var mapView = MapView.Active;

            var flyr = mapView.Map.Layers.OfType<FeatureLayer>().Where(f => f.Name == fLyr);

            mapView.SelectLayers(flyr.ToList());

        }
        #endregion

        // Flashes a selected feature in the map.
        #region FlashSelectedFeaturesAsync()- Flashes selected features in map. (WORKING)
        public static Task FlashSelectedFeaturesAsync()
        {
            return QueuedTask.Run(() =>
            {
                //Get the active map view.
                var mapView = MapView.Active;
                if (mapView == null)
                    return;

                //Get the selected features from the map and filter out the standalone table selection.
                var selectedFeatures = mapView.Map.GetSelection()
                  .Where(kvp => kvp.Key is BasicFeatureLayer)
                  .ToDictionary(kvp => (BasicFeatureLayer)kvp.Key, kvp => kvp.Value);

                //Flash the collection of features.
                mapView.FlashFeature(selectedFeatures);
            });

        }
        #endregion

        // Confirms that a parcel fabric layer exists in the map.
        #region Verify Parcel Fabric Exists in the Map

        /// <summary>
        /// This method gets a flattended list
        /// of map layers that are of ParcelLayer
        /// types. If the result is not null,
        /// a parcel fabric exists in the map.
        /// </summary>
        public static bool VerifyParcelFabricInMap()
        {
            bool _parcelFabricExists = false;

            try
            {

                ParcelLayer myParcelFabricLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ParcelLayer>().FirstOrDefault();

                //if there is no fabric in the map then bail

                if (myParcelFabricLayer == null)
                {

                    _parcelFabricExists = false;

                }

                else
                {
                    _parcelFabricExists = true;
                }


            }
            catch (Exception ex)
            {

                OS.WriteLogEntry(OS.EventLogSourceName, ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }


            return _parcelFabricExists;


        }

        #endregion

        #endregion

    }
}
