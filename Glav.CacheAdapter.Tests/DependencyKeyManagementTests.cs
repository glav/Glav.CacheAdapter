using System;
using System.Collections.Generic;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.DependencyInjection;
using Glav.CacheAdapter.DependencyManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class DependencyKeyManagementTests
    {
        private const string PARENTKEYNAME = "TestCacheKeyGroup";

        [TestMethod]
        public void ShouldRegisterParentKey()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            mgr.RegisterParentDependencyDefinition(PARENTKEYNAME);

            var groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.IsNotNull(groupEntry, "Did not get a parent entry");
            Assert.AreEqual<int>(1,groupEntry.Count());
            Assert.AreEqual<string>(PARENTKEYNAME, groupEntry.First().CacheKey);
        }

        [TestMethod]
        public void ShouldRemoveParentKeyButReturnOneWhenRequested()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            mgr.RegisterParentDependencyDefinition(PARENTKEYNAME);

            var groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.IsNotNull(groupEntry, "Did not get a parent entry");
            Assert.AreEqual<int>(1, groupEntry.Count());
            Assert.AreEqual<string>(PARENTKEYNAME, groupEntry.First().CacheKey);

            // Remove it
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);
            // Ensure we can get the entry (as it is implicitly created)
            // but has not items
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME);
            Assert.IsNotNull(groupEntry);
            Assert.AreEqual<int>(0, groupEntry.Count());
            // Ensure we can also return the parent node definition when requested
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.IsNotNull(groupEntry);
            Assert.AreEqual<int>(1, groupEntry.Count());
        }

        [TestMethod]
        public void ShouldClearDependentItemsInParentListFromListButNotRemoveParent()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            mgr.RegisterParentDependencyDefinition(PARENTKEYNAME);

            mgr.AssociateDependentKeysToParent(PARENTKEYNAME, new string[3] {"one", "two", "three"});

            // Not including parentNode
            var groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME);
            Assert.IsNotNull(groupEntry, "Did not get a parent entry");
            Assert.AreEqual<int>(3, groupEntry.Count());

            // Including the parentNode
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.AreEqual<int>(4, groupEntry.Count());
            Assert.IsNotNull(groupEntry.FirstOrDefault(n => n.IsParentNode));

            // Clear the list
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);
            // Get just the dependencies again, not including parent node
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME);
            Assert.IsNotNull(groupEntry);
            Assert.AreEqual<int>(0, groupEntry.Count());
            // And get all dependencies including parent node
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.AreEqual<int>(1, groupEntry.Count());

        }

    }
}
