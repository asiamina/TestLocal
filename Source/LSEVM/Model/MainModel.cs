using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using EnvDTE;

using Microsoft.CodeAnalysis;

using VSIXProjectThesis.Annotations;

using Expression = EnvDTE.Expression;


namespace VSIXProjectThesis.Model
{
    public class MainModel:INotifyPropertyChanged
    {

        public DTE DTE { get; set; }

        public MainModel(){


            this.DisplayText = string.Empty;
        }

        public MethodInspector MethodInspector { get; set; }
        
        public ICommand CmdRun
        {
            get
            {
                return
                    new Command((o) => {

                        if (this.MethodInspector == null)
                        {
                            MessageBox.Show("Please select a method for processing.");
                            return;
                        }

                        DTE.ExecuteCommand("Debug.Start");

                    });
            }
        }

        public bool StopAtBreakPoint { get; set; }

        public ICommand CmdInitialize
        {
            get
            {
                return
                    new Command((o) => {

                        if (this.MethodInspector == null){
                            MessageBox.Show("Please select a method for processing.");
                            return;
                        }                        
                        foreach (EnvDTE.Breakpoint bp in DTE.Debugger.Breakpoints){
                            bp.Delete();
                        }
                        SetInitialBreakpoint();
                        actions.Clear();
                        curAction = null;
                        this.DisplayText = string.Empty;
                    });
            }
        }

        private void SetInitialBreakpoint(){


            SetBreakpointAtLine(this.MethodInspector.StartLineNumber);
        }

        private void SetBreakpointAtLine(int lineNumber){
            EnvDTE.Debugger debugger = (EnvDTE.Debugger)DTE.Debugger;
            debugger.Breakpoints.Add("", MethodInspector.FileName, lineNumber, 1, "",
                EnvDTE.dbgBreakpointConditionType.dbgBreakpointConditionTypeWhenTrue,
                "C#", "", 0, "", 0, EnvDTE.dbgHitCountType.dbgHitCountTypeNone);
        }

        private void LoadActions(){

            try{
                //drop Z3 files in VS folder. C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE
                Z3Link z3 = new Z3Link();
                z3.ProcessSyntaxTree(MethodInspector.SyntaxTree);
                IDictionary<string, List<object>> varValues = z3.Solve();
                //get local variables
                //create action for all singles
                //create action for all cross join variables

                foreach (Variable v in MethodInspector.LoadVariables()){

                    foreach (int assignmentLocation in v.VariableAssignmentLocations){
                        Action a = new Action(){
                            Variable = v,
                            BreakPointLocation = assignmentLocation,
                            ValueToAssign = varValues[v.VariableName].FirstOrDefault(),
                        };
                        actions.Enqueue(a);
                    }
                }
            }
            catch (Exception e){

            }
        }

        public ICommand CmdClear
        {
            get
            {
                return
                    new Command((o) => {

                        

                    });
            }
        }

        private string m_displayText;

        public string DisplayText
        {
            get { return this.m_displayText; }
            set { this.m_displayText = value;
                OnPropertyChanged();
            }
        }

        private Queue<Action> actions = new Queue<Action>();
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null){
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RuntimeRunMode(){
            
        }
        private Action curAction = null;

        public dbgExecutionAction RuntimeBreakMode(){
            foreach (EnvDTE.Breakpoint breakpoint in this.DTE.Debugger.AllBreakpointsLastHit){
                if (breakpoint.File.ToLower() == this.MethodInspector.FileName.ToLower())
                    if (breakpoint.FileLine == this.MethodInspector.StartLineNumber){
                        if (actions.Count == 0){
                            
                            InitializeActions();
                            Action a = this.actions.Dequeue();
                            ProcessAction(a);
                        }
                        breakpoint.Delete();                        

                    }
                    else{
                        
                        Action a = null;
                        if (actions.Count>0)
                            a = this.actions.Dequeue();
                        ProcessAction(a);
                        breakpoint.Delete();
                        
                    }
            }
            if(!this.StopAtBreakPoint)
                return dbgExecutionAction.dbgExecutionActionGo;
            else
                return dbgExecutionAction.dbgExecutionActionDefault;
        }
        private Debugger debugger;
        private StackFrame stackFrame;
        private EnvDTE.Expressions locals;
        public EnvDTE.Expression GetDebugVariable(string varName)
        {
            
            DTE dte = this.DTE;
            debugger = dte.Debugger;
            stackFrame = debugger.CurrentStackFrame;
            if (stackFrame != null)
            {
                locals = stackFrame.Locals;
                foreach (EnvDTE.Expression local in locals)
                {
                    string name = local.Name;
                    if (name == varName)
                        return local;                    
                }
            }
            return null;
        }

        private void GetPaths(){
         ///new Microsoft.Z3.
        }
        
        private void ProcessAction(Action action){

            if (curAction != null){
                Expression variable = GetDebugVariable(curAction.Variable.VariableName);
                if (variable == null){
                    MessageBox.Show(string.Format("Unable to find variable '{0}'", curAction.Variable.VariableName));

                }
                else{
                    try{
                        if (variable.Type == "float")
                            variable.Value = curAction.ValueToAssign.ToString() + "f";
                        if (variable.Type == "char")
                            variable.Value = string.Format("'{0}'", Convert.ToChar(Convert.ToInt16(curAction.ValueToAssign)).ToString());
                        else
                        variable.Value = curAction.ValueToAssign.ToString();
                        DisplayText += string.Format("Set value of variable '{0}' to '{1}' at line '{2}' {3}", curAction.Variable.VariableName, curAction.ValueToAssign, curAction.BreakPointLocation, Environment.NewLine);
                    }
                    catch(Exception e){
                        MessageBox.Show("Unable to set debugger variable");
                        throw;
                    }
                }

            }
            if(action!= null)
                SetBreakpointAtLine(action.BreakPointLocation);
            curAction = action;
        }

        public void AddDisplayText(string message){
            DisplayText += message+=Environment.NewLine;
        }

        private void InitializeActions(){
            LoadActions();
        }

        public void ExceptionThrown(){
            DisplayText += string.Format("Exception Thrown {0}", Environment.NewLine);
        }

    }
}
