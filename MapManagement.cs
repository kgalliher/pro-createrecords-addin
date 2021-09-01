#region CLASS DOCUMENTATION
/*
 * Project:            DCADGISTools
 * Class:              MapManagement.cs
 * Author:             Craig Browne
 * Date Created:       07/07/2021
 * Target Application: ArcGIS Pro 2.6 +
 * Dependency:         None
 * Revisions:          mm/dd/yyyy -programmer-: Summary of modifications to code or user interface.
 * **************************************************************************************************
 * 
 * Purpose: Contains methods to confirm and control what maps and map layers that are in the ArcGIS Pro application.
 * 
 * **************************************************************************************************
 */
#endregion

using ArcGIS.Desktop.Mapping;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;

namespace pro_createrecords_addin
{
    class MapManagement
    {
        #region Constants

        public const string mapName = "ParcelEditor_Map";                // Name of the map that contains the 
                                                                   // parcel_view feature layer
            
        #endregion

        // Can be used to open a specific project map by it's name.
        #region OpenMap()- Activate a map based on the map's name.
        internal static void OpenMap()
        {
            var mapPanes = ProApp.Panes.OfType<IMapPane>();
            foreach (Pane pane in mapPanes)
            {
                if (pane.Caption == mapName)
                {
                    pane.Activate();
                    break;
                }
            }
        }
        #endregion

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

    }
}