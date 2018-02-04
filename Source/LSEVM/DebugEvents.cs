using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace VSIXProjectThesis
{
    public static class DebugEvents{

        private static EnvDTE.DebuggerEvents s_debugEvents;
        private static Events s_events;
        private static DTE s_dte;
        public static void SetDebugEvents(DTE dte)
        {
            s_dte = dte;
            s_events = dte.Events;
            s_debugEvents = s_events.DebuggerEvents;

            s_debugEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(DebugEvents_OnEnterRunMode);
            s_debugEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(DebugEvents_OnEnterDesignMode);
            s_debugEvents.OnEnterBreakMode += new _dispDebuggerEvents_OnEnterBreakModeEventHandler(DebugEvents_OnEnterBreakMode);
            s_debugEvents.OnExceptionThrown += new _dispDebuggerEvents_OnExceptionThrownEventHandler(DebugEvents_OnExceptionThrown);
            s_debugEvents.OnExceptionNotHandled += new _dispDebuggerEvents_OnExceptionNotHandledEventHandler(DebugEvents_OnExceptionNotHandled);
            s_debugEvents.OnContextChanged += new _dispDebuggerEvents_OnContextChangedEventHandler(DebugEvents_OnContextChanged);
            
        }

        public static void DebugEvents_OnContextChanged(Process NewProcess, Program NewProgram, Thread NewThread, StackFrame NewStackFrame)
        {
            
        }

        public static void DebugEvents_OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            
        }

        public static void DebugEvents_OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            
        }

        //https://technet.microsoft.com/en-us/library/envdte.debuggereventsclass.oncontextchanged(v=vs.110).aspx
        public static void DebugEvents_OnEnterRunMode(dbgEventReason Reason)
        {
            new Model.ViewModelLocator().MainModel.RuntimeRunMode();            
        }

        public static void DebugEvents_OnEnterDesignMode(dbgEventReason Reason)
        {
            
        }

        public static void DebugEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction){

            switch (Reason){
                case dbgEventReason.dbgEventReasonNone:
                case dbgEventReason.dbgEventReasonGo:
                case dbgEventReason.dbgEventReasonAttachProgram:
                case dbgEventReason.dbgEventReasonDetachProgram:
                case dbgEventReason.dbgEventReasonLaunchProgram:
                case dbgEventReason.dbgEventReasonEndProgram:
                case dbgEventReason.dbgEventReasonStopDebugging:
                case dbgEventReason.dbgEventReasonStep:
                case dbgEventReason.dbgEventReasonBreakpoint:
                case dbgEventReason.dbgEventReasonUserBreak:
                case dbgEventReason.dbgEventReasonContextSwitch:
                    ExecutionAction = new Model.ViewModelLocator().MainModel.RuntimeBreakMode();
                    break;
                case dbgEventReason.dbgEventReasonExceptionThrown:
                case dbgEventReason.dbgEventReasonExceptionNotHandled:
                    new Model.ViewModelLocator().MainModel.ExceptionThrown();
                    //ExecutionAction = dbgExecutionAction.dbgExecutionActionGo;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Reason), Reason, null);
            }

            
            

        }

       
    }
}
