using System.Collections;
using NUnit.Framework;
using Rumores.Presentation;
using UnityEngine;

namespace Rumores.Tests
{
    public class PartyTests
    {
        public static IEnumerable Names
        {get {return PartyChecks.Cases(Resources.Load<PartyCatalogAsset>("PartyCatalog").Load()).Keys;}}
        [TestCaseSource(nameof(Names))]
        public void PartyContractWithJsonUtility(string name)
        {PartyChecks.Cases(Resources.Load<PartyCatalogAsset>("PartyCatalog").Load())[name]();}
    }
}
