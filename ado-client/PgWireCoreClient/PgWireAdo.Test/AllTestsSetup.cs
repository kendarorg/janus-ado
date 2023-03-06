using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.Test.Utils;

namespace PgWireAdo
{
    [SetUpFixture]
    public class AllTestsSetup : TestBase
    {
        private Process _process;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            TbRunBeforeAnyTests();
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            TbRunAfterAnyTests();
            Trace.Flush();
        }
        
    }
}
