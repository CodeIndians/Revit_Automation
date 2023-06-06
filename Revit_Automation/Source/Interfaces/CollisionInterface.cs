using Revit_Automation.CustomTypes;

namespace Revit_Automation.Source.Interfaces
{
    public interface ICollisionInterface
    {
        void HandleCollision(CollisionObject collisionObject);
        void PlaceObjectInClearSpace();
    }
}
