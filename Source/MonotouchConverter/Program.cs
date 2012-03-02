using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using MonotouchConverter.Properties;
using log4net;
using log4net.Config;

namespace MonotouchConverter
{
    internal class Program
    {
        private const string MonotouchProjectType =
            "{6BC8ED88-2882-458C-8E55-DFD12B67127B};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        private const string ProjectGuidElement = "ProjectGuid";
        private const string ProjectTypeGuidsElement = "ProjectTypeGuids";
        private static readonly Settings _settings = Settings.Default;
        private static ILog _log;

        private static void ConvertToMonoTouch()
        {
            _log = LogManager.GetLogger(typeof (Program));
            Parallel.ForEach(GetProjectFiles(), DoConvertToMonotouch);
        }

        private static void DoConvertToMonotouch(string projectcFilePath)
        {
            XElement project = XElement.Load(projectcFilePath);
            XNamespace xNamespace = project.GetDefaultNamespace();
            List<XElement> descendants = project.Descendants().ToList();
            XElement projectTypeElement = descendants
                .SingleOrDefault(x => x.Name == xNamespace + ProjectTypeGuidsElement);
            if (projectTypeElement != null)
            {
                _log.WarnFormat("File: {0} already in Monotouch format", projectcFilePath);
                return;
            }
            XElement projectGuid = descendants.Single(x => x.Name == xNamespace + ProjectGuidElement);
            projectGuid.AddAfterSelf(new XElement(xNamespace + ProjectTypeGuidsElement, MonotouchProjectType));
            project.Save(projectcFilePath);
            _log.InfoFormat("File: {0} has been converted to Monotouch format", projectcFilePath);
        }

        private static IEnumerable<string> GetProjectFiles()
        {
            if (!Directory.Exists(_settings.SourcePath))
            {
                _log.ErrorFormat("Directory is absent: {0}", _settings.SourcePath);
                return new List<string>();
            }
            return Directory.EnumerateFiles(_settings.SourcePath, "*.csproj", SearchOption.AllDirectories);
        }

        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            _log = LogManager.GetLogger(typeof (Program));
            _log.Info("MonotouchConverter is running. Press <Esc> or <Enter> to stop.");
            _log.DebugFormat("Press M for convert to MonoTouch");
            _log.DebugFormat("Press V for convert to VisualStudio");
            Console.Write(Environment.NewLine);
            while (ProcessSpecialKey(Console.ReadKey()))
            {
            }
        }

        private static bool ProcessSpecialKey(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                case ConsoleKey.Escape:
                    return false;
                case ConsoleKey.M:
                    ConvertToMonoTouch();
                    return true;
            }
            return true;
        }
    }
}