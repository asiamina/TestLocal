using System;
using System.ComponentModel.Design;
using System.Globalization;

using EnvDTE;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSIXProjectThesis
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RightClickEditorTestMethod
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("17a5c833-8fda-4cf7-9059-aa13c0af4d08");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RightClickEditorTestMethod"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private RightClickEditorTestMethod(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RightClickEditorTestMethod Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new RightClickEditorTestMethod(package);
        }

        private bool eventsLinked = false;
       

        public void SetDte()
        {
            new Model.ViewModelLocator().MainModel.DTE = GetDte();
        }

        
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e){

            SetDte();
            TextSelection textSelection = (TextSelection)GetDte().ActiveDocument.ActiveWindow.Selection;

            MethodInspector mi = new MethodInspector(GetDte().ActiveDocument.FullName, textSelection, textSelection.TopLine);
            mi.ProcessMethod();
            new Model.ViewModelLocator().MainModel.MethodInspector = mi;
        }

        private DTE GetDte(){
            return ((EnvDTE.DTE)this.ServiceProvider.GetService(typeof(EnvDTE.DTE)));
        }

    }
}
