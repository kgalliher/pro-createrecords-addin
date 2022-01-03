using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DCAD.GIS;
using ArcGIS.Core.Data;

namespace pro_createrecords_addin.DataProviders
{
    public class TileProvider
    {

        #region Constants

        private static readonly string TileLayerName = "DCAD Tiles";


        #endregion

        #region Constructor
        public TileProvider()
        {

        }
        #endregion

        #region  Methods

        #region Searching a Feature Layer using SpatialQueryFilter

        /// <summary>
        /// Searches for DCAD tiles within
        /// the active MapView's extent and
        /// displays them in the dockpane.
        /// </summary>
        /// <param name="mapView">The active MapView.</param>
        /// <returns></returns>
        public async Task SearchingAFeatureLayer(MapView mapView)
        {

            // Feature Layer Variables
            string _tileNumField = "Number";                 // Tile Number field name
            string _cleanupRecordType = "CLEANUP RECORD";    // Defines the record type
            int _afcTypeCd = 4;                              // Not a legal change AFC Type code
            int _afcCount = 0;                               // Counter for total tiles added to pane
            bool _lyrExists;                                 // Boolean variable showing the Tile layer exists


            // This returns a collection of layers of the "name" specified.

            // You can use any Linq expression to query the collection.  

            // If the tile layer exists
            // then create a spatial filter
            // to query the layer
            _lyrExists = Layers.ConfirmTOCLayer(TileLayerName);

            if (_lyrExists)
            {

                await QueuedTask.Run(() => {

                    // Access tile layer
                    FeatureLayer dcadTileLayer = MapView.Active.Map.GetLayersAsFlattenedList()
                               .OfType<FeatureLayer>().Where(f => f.Name == TileLayerName).FirstOrDefault();

                    // Using a spatial query filter to find all features

                    // which intersect the current map extent.

                    SpatialQueryFilter spatialQueryFilter = new SpatialQueryFilter
                    {

                        FilterGeometry = mapView.Extent,

                        SpatialRelationship = SpatialRelationship.Intersects

                    };

                    using (RowCursor tileCursor = dcadTileLayer.GetFeatureClass().Search(spatialQueryFilter, false))
                    {

                        while (tileCursor.MoveNext())
                        {

                            using (Feature feature = (Feature)tileCursor.Current)
                            {
                                // Check if cleanup record exists

                                // Add the tile name to the AFC logs

                                // observable collection

                                AFCLog afcLog = new AFCLog();

                                afcLog.AFC_TYPE_CD = _afcTypeCd;

                                afcLog.TILE_NO = Convert.ToInt32(feature[_tileNumField]);

                                afcLog.DRAFTER_EMPL_ID = AFCLog.GetCurrentUser();

                                afcLog.DOC_TYPE = _cleanupRecordType;

                                afcLog.SetImageSource();    // Method sets the image source for the afc log type

                                afcLog.SetDocumentNumber(); // Method sets the document number for the afc log type

                                afcLog.SetRecordType();     // Method sets the record type for the afc log

                                // Set the record status based on

                                // the AFC status code

                                afcLog.SetRecordStatus();   // Method that sets the record status for the afc log

                                /***************************************
                                 * If a cleanup record already exists  *
                                 * for this tile and was created today *
                                 * having the same record name, then   *
                                 * color the text red and assign the   *
                                 * message to account number property  *
                                 * ************************************/



                                /***************************************
                                * Subscribe to AFCRecordCreated Event  *
                                * in the AFCRecord class.              *
                                * *********************************** */

                                afcLog.AFCRecordCreatedEvent += OnAFCRecordCreated;

                                _afcCount += 1;             // Increment afc count variable

                                // Reads and Writes should be made from within the lock

                                lock (_lockObj)
                                {
                                    _afclogs.Add(afcLog);
                                }
                            }
                        }
                    }
                });
            }
            else
            {
                MessageBox.Show($"The layer {TileLayerName} does not exist in the map. " +
                                $"Please check your layers and try again.");
            }
        }


        #endregion Searching a Feature Layer using SpatialQueryFilter

        #endregion

    }
}
