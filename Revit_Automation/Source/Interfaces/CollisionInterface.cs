using Autodesk.Revit.DB;
using Revit_Automation.CustomTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit_Automation.Source.Interfaces
{
    public interface ICollisionInterface
    {
        void HandleCollision(CollisionObject collisionObject);
        void PlaceObjectInClearSpace();
    }
}
