﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Algorithms.Default.EdgeBased;
using Itinero.Data.Network;
using Itinero.Test.Profiles;

namespace Itinero.Test.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// Executes tests
    /// </summary>
    [TestFixture]
    class ManyToManyTests
    {
        /// <summary>
        /// Tests many-to-many path calculations on just one edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m----(1) @ 100km/h
        /// </remarks>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedProfile(MockProfile.CarMock());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0, 0);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var algorithm = new ManyToMany(new Router(routerDb), MockProfile.CarMock().Default(), (x) => new uint[0][],
                new RouterPoint[] { new RouterPoint(0, 0, 0, 0) }, 
                new RouterPoint[] { new RouterPoint(1, 1, 0, ushort.MaxValue) }, float.MaxValue);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            var path = algorithm.GetPath(0, 0);
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.AreEqual(MockProfile.CarMock().Factor(null).Item1.Value * 100, path.Weight);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Vertex);
            Assert.AreEqual(0, path.Weight);
            path = path.From;
            Assert.IsNull(path);
        }

        /// <summary>
        /// Tests many-to-many path calculations on within one edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m---(1) @ 100km/h
        /// </remarks>
        [Test]
        public void TestWithinOneEdge()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedProfile(MockProfile.CarMock());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0, 0);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var algorithm = new ManyToMany(new Router(routerDb), MockProfile.CarMock().Default(), (x) => new uint[0][],
                new RouterPoint[] { new RouterPoint(0, 0, 0, ushort.MaxValue / 10) },
                new RouterPoint[] { new RouterPoint(1, 1, 0, ushort.MaxValue / 10 * 9) }, float.MaxValue);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            var path = algorithm.GetPath(0, 0);
            Assert.IsNotNull(path);
            Assert.AreEqual(Constants.NO_VERTEX, path.Vertex);
            Assert.AreEqual(MockProfile.CarMock().Factor(null).Item1.Value * 80, path.Weight, 0.01);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(Constants.NO_VERTEX, path.Vertex);
            Assert.AreEqual(0, path.Weight);
            path = path.From;
            Assert.IsNull(path);
        }

        /// <summary>
        /// Tests many to many calculations between vertices on a triangle.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)----100m----(1)
        ///   \             /
        ///    \           /           
        ///     \         /
        ///     100m    100m
        ///       \     /
        ///        \   /
        ///         (2)
        /// 
        /// Result:
        /// 
        ///     [  0,100,100]
        ///     [100,  0,100]
        ///     [100,100,  0]
        ///     
        /// </remarks>
        [Test]
        public void TestThreeEdges()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedProfile(MockProfile.CarMock());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(2, 0, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm (0, 1, 2)->(0, 1, 2).
            var algorithm = new ManyToMany(new Router(routerDb), MockProfile.CarMock().Default(), (x) => new uint[0][],
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                }, float.MaxValue);
            algorithm.Run(); Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            var weights = algorithm.Weights;
            Assert.IsNotNull(weights);
            Assert.AreEqual(3, weights.Length);
            Assert.AreEqual(3, weights[0].Length);
            Assert.AreEqual(0, weights[0][0], 0.001);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, weights[0][1], 0.001);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, weights[0][2], 0.001);
            Assert.AreEqual(3, weights[1].Length);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, weights[1][0], 0.001);
            Assert.AreEqual(0, weights[1][1], 0.001);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, weights[1][2], 0.001);
            Assert.AreEqual(3, weights[2].Length);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, weights[2][0], 0.001);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, weights[2][1], 0.001);
            Assert.AreEqual(0, weights[2][2], 0.001);

            var path = algorithm.GetPath(0, 0);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = algorithm.GetPath(0, 1);
            Assert.IsNotNull(path);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, path.Weight, 0.001);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = algorithm.GetPath(0, 2);
            Assert.IsNotNull(path);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, path.Weight, 0.001);
            Assert.AreEqual(2, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = algorithm.GetPath(1, 0);
            Assert.IsNotNull(path);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, path.Weight, 0.001);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNull(path); 
            
            path = algorithm.GetPath(1, 1);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = algorithm.GetPath(1, 2);
            Assert.IsNotNull(path);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, path.Weight, 0.001);
            Assert.AreEqual(2, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = algorithm.GetPath(2, 0);
            Assert.IsNotNull(path);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, path.Weight, 0.001);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(2, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = algorithm.GetPath(2, 1);
            Assert.IsNotNull(path);
            Assert.AreEqual(100 * MockProfile.CarMock().Factor(null).Item1.Value, path.Weight, 0.001);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(2, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = algorithm.GetPath(2, 2);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001);
            Assert.AreEqual(2, path.Vertex);
            path = path.From;
            Assert.IsNull(path); 
        }
    }
}