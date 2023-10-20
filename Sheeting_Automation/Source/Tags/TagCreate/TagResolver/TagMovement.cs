using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheeting_Automation.Source.Tags.TagCreate.TagResolver
{
    /// <summary>
    /// Base class for different tag movement implementations 
    /// </summary>
    public class TagMovement
    {
        protected enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right,
            UpLeft,
            UpRight,
            DownLeft,
            DownRight
        };
    }
}
