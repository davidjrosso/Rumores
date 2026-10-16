using System.Collections;
using NUnit.Framework;
using Rumores.Presentation;
using UnityEngine;

namespace Rumores.Tests
{
    public class CoreTests
    {
        public static IEnumerable Names
        {
            get {return CoreChecks.Cases(Resources.Load<RumoresCatalog>("RumoresCatalog").Load()).Keys;}
        }
        [TestCaseSource(nameof(Names))]
        public void CoreContract(string name)
        {CoreChecks.Cases(Resources.Load<RumoresCatalog>("RumoresCatalog").Load())[name]();}
    }
}
