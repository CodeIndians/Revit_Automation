using Revit_Automation.CustomTypes;

namespace Revit_Automation.Source.Interfaces
{
    public interface ICollisionInterface
    {
        bool HandleCollision(CollisionObject collisionObject);
        void PlaceObjectInClearSpace();
    }
}
