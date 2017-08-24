//------------------------------------------------------------------------------
// <copyright file="BuildDemangledOutput.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildDemangledOutput
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BuildDemangledOutput : IDisposable
    {
        private BuildEvents buildEvents = null;
        private IVsOutputWindowPane buildDemangledPane = null;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("dcf69293-85bd-448a-b5c6-a5acf7d7136f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildDemangledOutput"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private BuildDemangledOutput(Package package)
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
                var menuItem = new MenuCommand(this.BuildDemangledOutputCommandHandler, menuCommandID);
                commandService.AddCommand(menuItem);
            }

            buildEvents = new BuildEvents();
            buildEvents.BuildStartedEvent += OnBuildStarted;
            buildEvents.BuildFinishedEvent += OnBuildFinished;
        }

        public void Dispose()
        {
            buildEvents.BuildStartedEvent -= OnBuildStarted;
            buildEvents.BuildFinishedEvent -= OnBuildFinished;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BuildDemangledOutput Instance
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
            Instance = new BuildDemangledOutput(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void BuildDemangledOutputCommandHandler(object sender, EventArgs e)
        {
            CreatePane(new Guid(), "Build (demangled)", visible: true, clearWithSolution: false);
        }

        private void CreatePane(Guid paneGuid, string title, bool visible, bool clearWithSolution)
        {
            if (buildDemangledPane != null)
                return;

            var output = (IVsOutputWindow)this.ServiceProvider.GetService(typeof(SVsOutputWindow));

            // Create a new pane.  
            output.CreatePane(
                ref paneGuid,
                title,
                Convert.ToInt32(visible),
                Convert.ToInt32(clearWithSolution));

            // Retrieve the new pane.  
            output.GetPane(ref paneGuid, out buildDemangledPane);
        }

        private string GetBuildMessage()
        {
            var dte = (EnvDTE80.DTE2)this.ServiceProvider.GetService(typeof(EnvDTE.DTE));
            var windows = dte.ToolWindows.OutputWindow;

            var buildWindow = windows.OutputWindowPanes.Item("Build");
            var buildMessageTextDoc = buildWindow.TextDocument;
            var messageBegin = buildMessageTextDoc.StartPoint.CreateEditPoint();

            var buildMessage = messageBegin.GetText(buildMessageTextDoc.EndPoint);
            return buildMessage;
        }

        private void OnBuildStarted(object sender)
        {
            if (buildDemangledPane != null)
                buildDemangledPane.Clear();
        }

        private void OnBuildFinished(object sender)
        {
            if (buildDemangledPane != null)
            {
                var buildMeassage = GetBuildMessage();
                if (buildMeassage != null)
                {
                    var demangledBuildMessage = CppDemangler.Demangle(buildMeassage);
                    buildDemangledPane.OutputString(demangledBuildMessage);
                }
            }
        }
    }
}
