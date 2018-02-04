using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using EnvDTE;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VSIXProjectThesis
{
    public class MethodInspector
    {

        public string FileName { get; private set; }

        public int StartLineNumber
        {
            get { return this.m_startLineNumber; }
        }

        public SyntaxTree SyntaxTree { get; set; }

        private readonly TextSelection m_textSelection;

        private readonly CodeElement m_method;
        
        private readonly int m_startLineNumber = 0;

        public MethodInspector(string fileName,TextSelection textSelection, int startLine){
            this.m_startLineNumber = startLine;    
            this.FileName = fileName;           
            this.m_textSelection = textSelection;
            TextPoint textPoint = textSelection.ActivePoint;
            string elems = ""; 
            vsCMElement scopes = 0;
            this.m_method = textPoint.CodeElement[vsCMElement.vsCMElementFunction];
            

        }

        //https://msdn.microsoft.com/en-us/library/envdte.codevariable.initexpression.aspx
       



        private VariableDeclarationSyntax[] variableDeclarations;
        private AssignmentExpressionSyntax[] variableAssignments;

        public Dictionary<string, EnvDTE.Expression> GetLocalVariables(){
            Dictionary<string, EnvDTE.Expression> debugVariables = new Dictionary<string, Expression>(StringComparer.CurrentCultureIgnoreCase);
            DTE dte = m_textSelection.DTE;
            if (dte.Debugger.CurrentStackFrame != null)
            {
                EnvDTE.Expressions locals = dte.Debugger.CurrentStackFrame.Locals;
                foreach (EnvDTE.Expression local in locals){
                    string name = local.Name;
                    debugVariables[name] = local;                    
                }
            }
            return debugVariables;
        }

        
        public Variable[] LoadVariables(){
           
            Dictionary<string, List<AssignmentExpressionSyntax>> vas = new Dictionary<string, List<AssignmentExpressionSyntax>>(StringComparer.CurrentCultureIgnoreCase);

            foreach (VariableDeclarationSyntax declare in variableDeclarations){                
                SyntaxToken identifier = declare.Variables.FirstOrDefault().Identifier;
                string variableName = identifier.Text;
                if (!vas.ContainsKey(variableName))
                    vas.Add(variableName, new List<AssignmentExpressionSyntax>());
                
            }
            foreach (AssignmentExpressionSyntax assign in variableAssignments){
                string variableName = string.Empty;
                ExpressionStatementSyntax ess = assign.Parent as ExpressionStatementSyntax;
                if (ess != null){
                    variableName = ess.Expression.GetFirstToken().Text;

                    if (!vas.ContainsKey(variableName))
                        vas.Add(variableName, new List<AssignmentExpressionSyntax>());

                    vas[variableName].Add(assign);
                }
            }
            return GetVariablesLocal(vas);
        }

        public Variable[] GetVariablesLocal(Dictionary<string, List<AssignmentExpressionSyntax>> vas){
            List<Variable> variables = new List<Variable>(variableDeclarations.Length);
            Dictionary<string, Expression> debuggerVariables = GetLocalVariables();
            foreach (VariableDeclarationSyntax variableDeclaration in variableDeclarations){
                
                SyntaxToken identifier = variableDeclaration.Variables.FirstOrDefault().Identifier;
                string variableName = identifier.Text;

                List<int> assignLocations = new List<int>();
                assignLocations.Add(variableDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line + this.m_startLineNumber + 1);
                assignLocations.AddRange(vas[variableName].Select(o => o.GetLocation().GetLineSpan().StartLinePosition.Line + this.m_startLineNumber + 1).ToArray());
                int[] variableAssignments = assignLocations.ToArray();;
                
                variables.Add(new Variable(debuggerVariables[variableName], variableAssignments));
            }

            return variables.ToArray();
        }

        public CodeVariable GetVariable(FileLinePositionSpan location){
            m_textSelection.MoveToLineAndOffset(location.StartLinePosition.Line+this.StartLineNumber-1, 1);
            m_textSelection.SelectLine();
            CodeVariable var =
                (CodeVariable)m_textSelection.ActivePoint.get_CodeElement(
                    vsCMElement.vsCMElementVariable);
            
            return var;

        }

        public void ProcessMethod(){
            string method = GetMethodText(vsCMPart.vsCMPartWhole);
            SyntaxTree syntaxTree = this.SyntaxTree= CSharpSyntaxTree.ParseText(method);
            
            CompilationUnitSyntax root = (CompilationUnitSyntax)syntaxTree.GetRoot();
            variableDeclarations = root.DescendantNodes().OfType<VariableDeclarationSyntax>().ToArray();

            variableAssignments = root.DescendantNodes().OfType<AssignmentExpressionSyntax>().ToArray();
           
        }
        public string GetMethodText(vsCMPart part)
        {
            TextPoint startPoint = m_method.GetStartPoint();
            int endLine = m_method.GetEndPoint().Line;

            m_textSelection.MoveToPoint(startPoint);
            StringBuilder sb = new StringBuilder();
            int count = 99999;
            while (count-- != 0 && m_textSelection.CurrentLine != endLine)
            {
                m_textSelection.SelectLine();
                sb.Append(m_textSelection.Text);

            }
            return sb.ToString();
        }
        public string GetBodyPart(){
            TextPoint startPoint = m_method.GetStartPoint(vsCMPart.vsCMPartBody);
            int endLine = m_method.GetEndPoint(vsCMPart.vsCMPartBody).Line;

            m_textSelection.MoveToPoint(startPoint);
            StringBuilder sb = new StringBuilder();
            int count = 99999;
            while (count-- != 0 && m_textSelection.CurrentLine != endLine)
            {
                m_textSelection.SelectLine();
                sb.AppendLine(m_textSelection.Text);

            }
            return sb.ToString();
        }

    }
}
