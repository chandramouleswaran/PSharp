﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using Microsoft.PSharp.ReliableServices;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace BankAccount
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();

            var stateManager = new StateManagerMock(null);
            stateManager.DisallowFailures();

            var config = Configuration.Create(); //.WithVerbosityEnabled(2);
            var origHost = RsmHost.Create(stateManager, "SinglePartition", config);
            origHost.ReliableCreateMachine<ClientMachine>(new RsmInitEvent());

            Console.ReadLine();
        }

        [TestInit]
        public static void TestStart()
        {
            System.Diagnostics.Debugger.Launch();
        }

        [Test]
        public static void Execute(PSharpRuntime runtime)
        {
            runtime.RegisterMonitor(typeof(SafetyMonitor));
            var stateManager = new StateManagerMock(runtime);
            stateManager.DisallowFailures();

            var origHost = RsmHost.CreateForTesting(stateManager, "SinglePartition", runtime);
            origHost.ReliableCreateMachine<ClientMachine>(new RsmInitEvent());
        }
    }
}