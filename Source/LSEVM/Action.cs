using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace VSIXProjectThesis
{
    public class Action
    {

        public Variable Variable { get; set; }        
        public object ValueToAssign { get; set; }
        public int BreakPointLocation { get; set; }

    }
}
