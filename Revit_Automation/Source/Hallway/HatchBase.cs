using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Hallway
{
    internal abstract class HatchBase
    {
        protected abstract void PlaceHatches();
        protected abstract void DeleteHatches();

        public void CreateHatching()
        {
            PlaceHatches();
            DeleteHatches();
        }
    }
}
