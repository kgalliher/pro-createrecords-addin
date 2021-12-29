using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCAD.Data
{
    public class DataLibrary
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
