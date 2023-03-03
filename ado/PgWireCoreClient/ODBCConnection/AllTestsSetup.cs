using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ODBCConnection
{
    [SetUpFixture]
    public class AllTestsSetup 
    {
        private Process _process;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            Trace.Flush();
        }
        
    }
}
