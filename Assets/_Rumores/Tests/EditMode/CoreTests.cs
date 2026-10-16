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
        public static IEnumerable SaveNames
        {
            get {return SaveChecks.Cases(Resources.Load<RumoresCatalog>("RumoresCatalog").Load(),new UnitySaveJson()).Keys;}
        }
        [TestCaseSource(nameof(SaveNames))]
        public void SaveContractWithJsonUtility(string name)
        {SaveChecks.Cases(Resources.Load<RumoresCatalog>("RumoresCatalog").Load(),new UnitySaveJson())[name]();}
    }
}
