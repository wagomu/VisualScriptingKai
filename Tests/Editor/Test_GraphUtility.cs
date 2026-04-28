using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CHM.VisualScriptingKai.Editor.Tests
{
    public class Test_GraphUtility
    {
        const string TestDirectory = "Packages/com.chocola-mint.visual-scripting-kai/Tests/Editor/";

        private enum GraphLensSearchEnum
        {
            AlphaMember,
            BetaMember,
            GammaMember
        }

        [Flags]
        private enum GraphLensFlagEnum
        {
            None = 0,
            FirstFlag = 1,
            SecondFlag = 2
        }

        private sealed class EnumDefaultValueUnit : Unit
        {
            public ValueInput input { get; private set; }

            protected override void Definition()
            {
                input = ValueInput(nameof(input), GraphLensSearchEnum.BetaMember);
            }
        }

        [Test]
        public void TestGraphQueries()
        {
            // We use scenes instead of graph assets so users won't see testcase graphs in the fuzzy search.
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(TestDirectory + "TestScene");
            AssetDatabase.OpenAsset(sceneAsset);
            // Now we've opened the test scene, this will find all graph scripts in the scene.
            var testcases = GraphUtility.FindAllRuntimeGraphSources();
            Assert.That(
                testcases.Count(), Is.EqualTo(2));
            // In MyStateMachine.
            Assert.That(
                GraphUtility.FindNodes(testcases, "Add").Count(),
                Is.EqualTo(1));
            // In MyScriptMachine.
            Assert.That(
                GraphUtility.FindNodes(testcases, "Assert").Count(),
                Is.EqualTo(1));
            Assert.That(
                GraphUtility.FindStickyNotes(testcases, "TODO").Count(),
                Is.EqualTo(3));
            Assert.That(
                GraphUtility.FindStickyNotes(testcases, "TODO 1").Count(),
                Is.EqualTo(1));
            Assert.That(
                GraphUtility.FindStates(testcases, "State").Count(),
                Is.EqualTo(4 + 1)); // There's one in the Script Machine as well.
            Assert.That(
                GraphUtility.FindStateTransitions(testcases, "Transition").Count(),
                Is.EqualTo(2));
        }

        [Test]
        public void FindNodes_EnumLiteralValueName_MatchesNode()
        {
            var graph = CreateGraphWithUnits(new Literal(typeof(GraphLensSearchEnum), GraphLensSearchEnum.AlphaMember));

            Assert.That(CountNodeMatches(graph, nameof(GraphLensSearchEnum.AlphaMember)), Is.EqualTo(1));
        }

        [Test]
        public void FindNodes_EnumDefaultValueName_MatchesNode()
        {
            var graph = CreateGraphWithUnits(new EnumDefaultValueUnit());

            Assert.That(CountNodeMatches(graph, nameof(GraphLensSearchEnum.BetaMember)), Is.EqualTo(1));
        }

        [Test]
        public void FindNodes_EnumBranchNames_MatchesSwitchAndSelectNodes()
        {
            var graph = CreateGraphWithUnits(
                new SwitchOnEnum { enumType = typeof(GraphLensSearchEnum) },
                new SelectOnEnum { enumType = typeof(GraphLensSearchEnum) });

            Assert.That(CountNodeMatches(graph, nameof(GraphLensSearchEnum.GammaMember)), Is.EqualTo(2));
        }

        [Test]
        public void FindNodes_FlagEnumValuePartName_MatchesNode()
        {
            var graph = CreateGraphWithUnits(new Literal(
                typeof(GraphLensFlagEnum),
                GraphLensFlagEnum.FirstFlag | GraphLensFlagEnum.SecondFlag));

            Assert.That(CountNodeMatches(graph, nameof(GraphLensFlagEnum.SecondFlag)), Is.EqualTo(1));
        }

        private static FlowGraph CreateGraphWithUnits(params IUnit[] units)
        {
            var graph = new FlowGraph();
            foreach(var unit in units)
                graph.units.Add(unit);
            return graph;
        }

        private static int CountNodeMatches(FlowGraph graph, string pattern)
        {
            var graphAsset = ScriptableObject.CreateInstance<ScriptGraphAsset>();
            try
            {
                graphAsset.graph = graph;
                return GraphUtility.FindNodes(new GraphSource(graphAsset), pattern).Count();
            }
            finally
            {
                Object.DestroyImmediate(graphAsset);
            }
        }
    }
}
