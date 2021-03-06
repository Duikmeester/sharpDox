﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using SharpDox.ConsoleHelper;
using SharpDox.Local;
using SharpDox.Sdk.Local;
using SharpDox.Sdk.UI;

namespace SharpDox.Core
{
    internal class BootStrapper
    {
        private IContainer _container;
        private List<IShell> _shells;
        private SharpDoxStrings _strings;

        private readonly string[] _args;
        private readonly string _sdVersion;

        public BootStrapper(string[] args)
        {
            _args = args;
            _sdVersion = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(AppEntry)).Location).FileVersion;

            BuildDIContainer();
        }

        public void StartSharpDox()
        {
            ConsoleWriter.PrintConsoleHeader("sharpDox " + _sdVersion);
            Console.WriteLine(_strings.StartSD);

            if (AnyShellExists())
            {
                InitLocal();
                _shells = GetAllSchells();
                StartShell();
            }
            else
            {
                ConsoleWriter.PrintConsoleHeader("sharpDox " + _sdVersion);
                Console.WriteLine(_strings.NoShells);
                Console.ReadKey();
            }
        }

        private void BuildDIContainer()
        {
            ConsoleWriter.PrintConsoleHeader("sharpDox " + _sdVersion);

            var setupContext = new ContainerConfig();
            _container = setupContext.BuildContainer();
            _strings = _container.Resolve<SharpDoxStrings>();
        }

        private bool AnyShellExists()
        {
            return _container.IsRegistered<IShell>();
        }

        private void InitLocal()
        {
            _container.Resolve<LocalController>();
        }

        private List<IShell> GetAllSchells()
        {
            return _container.Resolve<IEnumerable<IShell>>().ToList();
        }

        private void StartShell()
        {
            if (_shells.Count > 1)
            {
                var shellNr = PrintAndGetShellSelection();
                ViewShell(_shells[shellNr], _sdVersion);
            }
            else
            {
                ViewShell(_shells[0], _sdVersion);
            }
        }

        private int PrintAndGetShellSelection()
        {
            int shellNr;
            var input = string.Empty;

            while (!int.TryParse(input, out shellNr) || shellNr > _shells.Count - 1 || shellNr < 0)
            {
                ConsoleWriter.PrintConsoleHeader("sharpDox " + _sdVersion);
                input = PrintShellSelection();
            }

            return shellNr;
        }

        public string PrintShellSelection()
        {
            Console.WriteLine(_strings.MoreShells);
            Console.WriteLine();

            int i = 0;
            foreach (var shell in _shells)
            {
                Console.WriteLine(string.Format(" {0}) - {1}", i, shell.GetType().Namespace));
                i++;
            }

            Console.WriteLine();

            return Console.ReadLine();
        }

        private void ViewShell(IShell shell, string sharpDoxVersion)
        {
            if (shell.IsGui)
            {
                ConsoleHider.HideConsoleWindow();
            }
            else
            {
                ConsoleWriter.PrintConsoleHeader("sharpDox " + sharpDoxVersion);
            }

            shell.Start(_args);
        }
    }
}
