using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsTripPlanner
{
    public abstract class Schedule : HashSet<Game>
    {
        protected string yearCode;
    }
}
