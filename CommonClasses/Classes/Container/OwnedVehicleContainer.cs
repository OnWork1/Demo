using BMW.IntegrationService.CrmGenerated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Container
{
   public class OwnedVehicleContainer
    {
        public bmw_vehicle Vehicle { get; set; }
        public Connection VehicleConnection { get; set; }

        public OwnedVehicleContainer(bmw_vehicle vehicle, Connection connection)
        {
            this.Vehicle = vehicle;
            this.VehicleConnection = connection;
        }
    }
}
