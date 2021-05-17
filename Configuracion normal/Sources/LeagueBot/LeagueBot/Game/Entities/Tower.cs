using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueBot.Game.Entities
{
    public class Tower : IEntity
    {
        public Point Position
        {
            get;
            private set;
        }

        public Tower(Point position)
        {
            this.Position = position;
        }
    }
}
