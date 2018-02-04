using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VSIXProjectThesis
{
    public class Variable{

        private int[] m_assignmentLocations;
        private EnvDTE.Expression m_debugVariable { get; set; }

        public int[] VariableAssignmentLocations
        {
            get { return this.m_assignmentLocations; }
        }

        public Variable(Expression debugVariable, int[] assignmentLocations){
            this.m_debugVariable = debugVariable;
            this.VariableName = debugVariable.Name;
            this.m_assignmentLocations = assignmentLocations;
        }

        public string VariableName { get; private set; }

        public void AssignValueToVariable(object val){
            m_debugVariable.Value = val.ToString();
        }

        
    }
}
