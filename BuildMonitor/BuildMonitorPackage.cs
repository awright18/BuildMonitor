﻿using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using BuildMonitor.Domain;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Constants = EnvDTE.Constants;

namespace BuildMonitor
{
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidBuildMonitorPkgString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    public class BuildMonitorPackage : Package, IVsUpdateSolutionEvents2
    {
        private DTE dte;
        private readonly Monitor monitor;
        private Domain.Solution solution;

        private IVsSolutionBuildManager2 sbm;
        private uint updateSolutionEventsCookie;
        private OutputWindowPane outputWindowPane;
        private SolutionEvents events;
        private IVsSolution2 vsSolution;

        public BuildMonitorPackage()
        {
            var factory = new BuildFactory();
            var repository = new BuildRepository(Settings.RepositoryPath);

            monitor = new Monitor(factory, repository);
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Get solution build manager
            sbm = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            if (sbm != null)
            {
                sbm.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);
            }

            // Must hold a reference to the solution events object or the events wont fire, garbage collection related
            events = GetDTE().Events.SolutionEvents;
            events.Opened += Solution_Opened;

            PrintLine("Build monitor initialized");
            PrintLine(string.Format("Path to persist data: {0}", Settings.RepositoryPath));

            monitor.SolutionBuildFinished = b =>
            {
                Print(string.Format("[{0}] Time Elapsed: {1}ms  \t\t", b.SessionBuildCount, b.SolutionBuildTime));
                PrintLine(string.Format("Session build time: {0}ms", b.SessionMillisecondsElapsed));
            };
        }

        #region Solution open and close events

        private void Solution_Opened()
        {
            solution = new Domain.Solution {Name = GetSolutionName()};
            PrintLine("\nSolution loaded:  \t{0}", solution.Name);
            PrintLine("{0}", 60.Times("-"));
        }
        
        #endregion

        #region Print to output window pane

        private OutputWindowPane GetOutputWindowPane()
        {
            if (outputWindowPane == null)
            {
                var outputWindow = (OutputWindow)GetDTE().Windows.Item(Constants.vsWindowKindOutput).Object;
                outputWindowPane = outputWindow.OutputWindowPanes.Add("Build monitor");
            }
            return outputWindowPane;
        }

        private void Print(string format, params object[] args)
        {
            GetOutputWindowPane().OutputString(string.Format(format, args));
        }

        private void PrintLine(string format, params object[] args)
        {
            Print(format + '\n', args);
        }

        private void Debug(string input, params object[] args)
        {
            Print("-- " + input + '\n', args);
        }

        #endregion

        private DTE GetDTE()
        {
            if (dte == null)
            {
                var serviceContainer = this as IServiceContainer;
                dte = serviceContainer.GetService(typeof(SDTE)) as DTE;
            }
            return dte;
        }

        private void SetVsSolution()
        {
            if(vsSolution == null)
                vsSolution = ServiceProvider.GlobalProvider.GetService(typeof (SVsSolution)) as IVsSolution2;
        }

        private string GetSolutionName()
        {
            SetVsSolution();
            object solutionName;
            vsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionBaseName, out solutionName);
            return (string)solutionName;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            // This method is called when the entire solution starts to build.
            monitor.SolutionBuildStart(solution);

            return VSConstants.S_OK;
        }


        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            // This method is called when a specific project begins building.
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            // This method is called when a specific project finishes building.
            return VSConstants.S_OK;
        }


        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            // This method is called when the entire solution is done building.
            monitor.SolutionBuildStop();
            return VSConstants.S_OK;
        }

        #region empty impl. of solution events interface

        int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Unadvise all events
            if (sbm != null && updateSolutionEventsCookie != 0)
                sbm.UnadviseUpdateSolutionEvents(updateSolutionEventsCookie);
        }
    }

    public static class IntExtensions
    {
        public static string Times(this int i, string s)
        {
            return string.Join("", Enumerable.Range(0, i).Select(d => s));
        }
    }
}