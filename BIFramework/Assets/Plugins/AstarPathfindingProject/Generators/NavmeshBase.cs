using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Pathfinding {
	using System.IO;
	using Pathfinding.Util;
	using Pathfinding.Serialization;
	using Math = System.Math;
	using System.Linq;
	using Pathfinding.Drawing;
	using Pathfinding.Jobs;
	using Unity.Jobs;

	/// <summary>Base class for <see cref="RecastGraph"/> and <see cref="NavMeshGraph"/></summary>
	public abstract class NavmeshBase : NavGraph, INavmesh, INavmeshHolder, ITransformedGraph
		, IRaycastableGraph {
#if ASTAR_RECAST_LARGER_TILES
		// Larger tiles
		public const int VertexIndexMask = 0xFFFFF;

		public const int TileIndexMask = 0x7FF;
		public const int TileIndexOffset = 20;
#else
		// Larger worlds
		public const int VertexIndexMask = 0xFFF;

		public const int TileIndexMask = 0x7FFFF;
		public const int TileIndexOffset = 12;
#endif

		/// <summary>Size of the bounding box.</summary>
		[JsonMember]
		public Vector3 forcedBoundsSize = new Vector3(100, 40, 100);

		/// <summary>Size of a tile in world units along the X axis</summary>
		public abstract float TileWorldSizeX { get; }

		/// <summary>Size of a tile in world units along the Z axis</summary>
		public abstract float TileWorldSizeZ { get; }

		/// <summary>
		/// Maximum (vertical) distance between the sides of two nodes for them to be connected across a tile edge.
		/// When tiles are connected to each other, the nodes sometimes do not line up perfectly
		/// so some allowance must be made to allow tiles that do not match exactly to be connected with each other.
		/// </summary>
		protected abstract float MaxTileConnectionEdgeDistance { get; }

		/// <summary>Show an outline of the polygons in the Unity Editor</summary>
		[JsonMember]
		public bool showMeshOutline = true;

		/// <summary>Show the connections between the polygons in the Unity Editor</summary>
		[JsonMember]
		public bool showNodeConnections;

		/// <summary>Show the surface of the navmesh</summary>
		[JsonMember]
		public bool showMeshSurface = true;

		/// <summary>Number of tiles along the X-axis</summary>
		public int tileXCount;
		/// <summary>Number of tiles along the Z-axis</summary>
		public int tileZCount;

		/// <summary>
		/// All tiles.
		///
		/// See: <see cref="GetTile"/>
		/// </summary>
		protected NavmeshTile[] tiles;

		/// <summary>
		/// Perform nearest node searches in XZ space only.
		/// Recomended for single-layered environments. Faster but can be inaccurate esp. in multilayered contexts.
		/// You should not use this if the graph is rotated since then the XZ plane no longer corresponds to the ground plane.
		///
		/// This can be important on sloped surfaces. See the image below in which the closest point for each blue point is queried for:
		/// [Open online documentation to see images]
		///
		/// You can also control this using a <see cref="Pathfinding.NNConstraint.distanceXZ field on an NNConstraint"/>.
		/// </summary>
		[JsonMember]
		public bool nearestSearchOnlyXZ;

		/// <summary>
		/// Should navmesh cuts affect this graph.
		/// See: <see cref="navmeshUpdateData"/>
		/// </summary>
		[JsonMember]
		public bool enableNavmeshCutting = true;

		/// <summary>
		/// Handles navmesh cutting.
		/// See: <see cref="enableNavmeshCutting"/>
		/// See: <see cref="Pathfinding.NavmeshUpdates"/>
		/// </summary>
		internal readonly NavmeshUpdates.NavmeshUpdateSettings navmeshUpdateData;

		/// <summary>Currently updating tiles in a batch</summary>
		bool batchTileUpdate;

		/// <summary>List of tiles updating during batch</summary>
		List<int> batchUpdatedTiles = new List<int>();

		/// <summary>List of nodes that are going to be destroyed as part of a batch update</summary>
		List<MeshNode> batchNodesToDestroy = new List<MeshNode>();

		/// <summary>
		/// Determines how the graph transforms graph space to world space.
		/// See: <see cref="CalculateTransform"/>
		///
		/// Warning: Do not modify this directly, instead use e.g. <see cref="RelocateNodes(GraphTransform)"/>
		/// </summary>
		public GraphTransform transform = GraphTransform.identityTransform;

		GraphTransform ITransformedGraph.transform { get { return transform; } }

		/// <summary>\copydoc Pathfinding::NavMeshGraph::recalculateNormals</summary>
		protected abstract bool RecalculateNormals { get; }

		/// <summary>
		/// Returns a new transform which transforms graph space to world space.
		/// Does not update the <see cref="transform"/> field.
		/// See: <see cref="RelocateNodes(GraphTransform)"/>
		/// </summary>
		public abstract GraphTransform CalculateTransform();

		/// <summary>
		/// Called when tiles have been completely recalculated.
		/// This is called after scanning the graph and after
		/// performing graph updates that completely recalculate tiles
		/// (not ones that simply modify e.g penalties).
		/// It is not called after NavmeshCut updates.
		/// </summary>
		public System.Action<NavmeshTile[]> OnRecalculatedTiles;

		/// <summary>
		/// Tile at the specified x, z coordinate pair.
		/// The first tile is at (0,0), the last tile at (tileXCount-1, tileZCount-1).
		///
		/// <code>
		/// var graph = AstarPath.active.data.recastGraph;
		/// int tileX = 5;
		/// int tileZ = 8;
		/// NavmeshTile tile = graph.GetTile(tileX, tileZ);
		///
		/// for (int i = 0; i < tile.nodes.Length; i++) {
		///     // ...
		/// }
		/// // or you can access the nodes like this:
		/// tile.GetNodes(node => {
		///     // ...
		/// });
		/// </code>
		/// </summary>
		public NavmeshTile GetTile (int x, int z) {
			return tiles[x + z * tileXCount];
		}

		/// <summary>
		/// Vertex coordinate for the specified vertex index.
		///
		/// Throws: IndexOutOfRangeException if the vertex index is invalid.
		/// Throws: NullReferenceException if the tile the vertex is in is not calculated.
		///
		/// See: NavmeshTile.GetVertex
		/// </summary>
		public Int3 GetVertex (int index) {
			int tileIndex = (index >> TileIndexOffset) & TileIndexMask;

			return tiles[tileIndex].GetVertex(index);
		}

		/// <summary>Vertex coordinate in graph space for the specified vertex index</summary>
		public Int3 GetVertexInGraphSpace (int index) {
			int tileIndex = (index >> TileIndexOffset) & TileIndexMask;

			return tiles[tileIndex].GetVertexInGraphSpace(index);
		}

		/// <summary>Tile index from a vertex index</summary>
		public static int GetTileIndex (int index) {
			return (index >> TileIndexOffset) & TileIndexMask;
		}

		public int GetVertexArrayIndex (int index) {
			return index & VertexIndexMask;
		}

		/// <summary>Tile coordinates from a tile index</summary>
		public void GetTileCoordinates (int tileIndex, out int x, out int z) {
			//z = System.Math.DivRem (tileIndex, tileXCount, out x);
			z = tileIndex/tileXCount;
			x = tileIndex - z*tileXCount;
		}

		/// <summary>
		/// All tiles.
		/// Warning: Do not modify this array
		/// </summary>
		public NavmeshTile[] GetTiles () {
			return tiles;
		}

		/// <summary>
		/// Returns a bounds object with the bounding box of a group of tiles.
		/// The bounding box is defined in world space.
		/// </summary>
		public Bounds GetTileBounds (IntRect rect) {
			return GetTileBounds(rect.xmin, rect.ymin, rect.Width, rect.Height);
		}

		/// <summary>
		/// Returns a bounds object with the bounding box of a group of tiles.
		/// The bounding box is defined in world space.
		/// </summary>
		public Bounds GetTileBounds (int x, int z, int width = 1, int depth = 1) {
			return transform.Transform(GetTileBoundsInGraphSpace(x, z, width, depth));
		}

		public Bounds GetTileBoundsInGraphSpace (IntRect rect) {
			return GetTileBoundsInGraphSpace(rect.xmin, rect.ymin, rect.Width, rect.Height);
		}

		/// <summary>Returns an XZ bounds object with the bounds of a group of tiles in graph space</summary>
		public Bounds GetTileBoundsInGraphSpace (int x, int z, int width = 1, int depth = 1) {
			var b = new Bounds();

			b.SetMinMax(
				new Vector3(x*TileWorldSizeX, 0, z*TileWorldSizeZ),
				new Vector3((x+width)*TileWorldSizeX, forcedBoundsSize.y, (z+depth)*TileWorldSizeZ)
				);
			return b;
		}

		/// <summary>
		/// Returns the tile coordinate which contains the specified position.
		/// It is not necessarily a valid tile (i.e it could be out of bounds).
		/// </summary>
		public Int2 GetTileCoordinates (Vector3 position) {
			position = transform.InverseTransform(position);
			position.x /= TileWorldSizeX;
			position.z /= TileWorldSizeZ;
			return new Int2((int)position.x, (int)position.z);
		}

		protected override void OnDestroy () {
			base.OnDestroy();

			// Cleanup
			TriangleMeshNode.SetNavmeshHolder(active.data.GetGraphIndex(this), null);

			if (tiles != null) {
				for (int i = 0; i < tiles.Length; i++) {
					Pathfinding.Util.ObjectPool<BBTree>.Release(ref tiles[i].bbTree);
				}
			}
		}

		public override void RelocateNodes (Matrix4x4 deltaMatrix) {
			RelocateNodes(deltaMatrix * transform);
		}

		/// <summary>
		/// Moves the nodes in this graph.
		/// Moves all the nodes in such a way that the specified transform is the new graph space to world space transformation for the graph.
		/// You usually use this together with the <see cref="CalculateTransform"/> method.
		///
		/// So for example if you want to move and rotate all your nodes in e.g a recast graph you can do
		/// <code>
		/// var graph = AstarPath.data.recastGraph;
		/// graph.rotation = new Vector3(45, 0, 0);
		/// graph.forcedBoundsCenter = new Vector3(20, 10, 10);
		/// var transform = graph.CalculateTransform();
		/// graph.RelocateNodes(transform);
		/// </code>
		/// This will move all the nodes to new positions as if the new graph settings had been there from the start.
		///
		/// Note: RelocateNodes(deltaMatrix) is not equivalent to RelocateNodes(new GraphTransform(deltaMatrix)).
		///  The overload which takes a matrix multiplies all existing node positions with the matrix while this
		///  overload does not take into account the current positions of the nodes.
		///
		/// See: <see cref="CalculateTransform"/>
		/// </summary>
		public void RelocateNodes (GraphTransform newTransform) {
			transform = newTransform;
			if (tiles != null) {
				// Move all the vertices in each tile
				for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++) {
					var tile = tiles[tileIndex];
					if (tile != null) {
						tile.vertsInGraphSpace.CopyTo(tile.verts, 0);
						// Transform the graph space vertices to world space
						transform.Transform(tile.verts);

						for (int nodeIndex = 0; nodeIndex < tile.nodes.Length; nodeIndex++) {
							tile.nodes[nodeIndex].UpdatePositionFromVertices();
						}
						tile.bbTree.RebuildFrom(tile);
					}
				}
			}
		}

		/// <summary>Creates a single new empty tile</summary>
		protected NavmeshTile NewEmptyTile (int x, int z) {
			return new NavmeshTile {
					   x = x,
					   z = z,
					   w = 1,
					   d = 1,
					   verts = new Int3[0],
					   vertsInGraphSpace = new Int3[0],
					   tris = new int[0],
					   nodes = new TriangleMeshNode[0],
					   bbTree = ObjectPool<BBTree>.Claim(),
					   graph = this,
			};
		}

		public override void GetNodes (System.Action<GraphNode> action) {
			if (tiles == null) return;

			for (int i = 0; i < tiles.Length; i++) {
				if (tiles[i] == null || tiles[i].x+tiles[i].z*tileXCount != i) continue;
				TriangleMeshNode[] nodes = tiles[i].nodes;

				if (nodes == null) continue;

				for (int j = 0; j < nodes.Length; j++) action(nodes[j]);
			}
		}

		/// <summary>
		/// Returns a rect containing the indices of all tiles touching the specified bounds.
		/// If a margin is passed, the bounding box in graph space is expanded by that amount in every direction.
		/// </summary>
		public IntRect GetTouchingTiles (Bounds bounds, float margin = 0) {
			bounds = transform.InverseTransform(bounds);

			// Calculate world bounds of all affected tiles
			var r = new IntRect(Mathf.FloorToInt((bounds.min.x - margin) / TileWorldSizeX), Mathf.FloorToInt((bounds.min.z - margin) / TileWorldSizeZ), Mathf.FloorToInt((bounds.max.x + margin) / TileWorldSizeX), Mathf.FloorToInt((bounds.max.z + margin) / TileWorldSizeZ));
			// Clamp to bounds
			r = IntRect.Intersection(r, new IntRect(0, 0, tileXCount-1, tileZCount-1));
			return r;
		}

		/// <summary>Returns a rect containing the indices of all tiles touching the specified bounds.</summary>
		/// <param name="rect">Graph space rectangle (in graph space all tiles are on the XZ plane regardless of graph rotation and other transformations, the first tile has a corner at the origin)</param>
		public IntRect GetTouchingTilesInGraphSpace (Rect rect) {
			// Calculate world bounds of all affected tiles
			var r = new IntRect(Mathf.FloorToInt(rect.xMin / TileWorldSizeX), Mathf.FloorToInt(rect.yMin / TileWorldSizeZ), Mathf.FloorToInt(rect.xMax / TileWorldSizeX), Mathf.FloorToInt(rect.yMax / TileWorldSizeZ));

			// Clamp to bounds
			r = IntRect.Intersection(r, new IntRect(0, 0, tileXCount-1, tileZCount-1));
			return r;
		}

		/// <summary>
		/// Returns a rect containing the indices of all tiles by rounding the specified bounds to tile borders.
		/// This is different from GetTouchingTiles in that the tiles inside the rectangle returned from this method
		/// may not contain the whole bounds, while that is guaranteed for GetTouchingTiles.
		/// </summary>
		public IntRect GetTouchingTilesRound (Bounds bounds) {
			bounds = transform.InverseTransform(bounds);

			//Calculate world bounds of all affected tiles
			var r = new IntRect(Mathf.RoundToInt(bounds.min.x / TileWorldSizeX), Mathf.RoundToInt(bounds.min.z / TileWorldSizeZ), Mathf.RoundToInt(bounds.max.x / TileWorldSizeX)-1, Mathf.RoundToInt(bounds.max.z / TileWorldSizeZ)-1);
			//Clamp to bounds
			r = IntRect.Intersection(r, new IntRect(0, 0, tileXCount-1, tileZCount-1));
			return r;
		}

		protected void ConnectTileWithNeighbours (NavmeshTile tile, bool onlyUnflagged = false) {
			if (tile.w != 1 || tile.d != 1) {
				throw new System.ArgumentException("Tile widths or depths other than 1 are not supported. The fields exist mainly for possible future expansions.");
			}

			// Loop through z and x offsets to adjacent tiles
			// _ x _
			// x _ x
			// _ x _
			for (int zo = -1; zo <= 1; zo++) {
				var z = tile.z + zo;
				if (z < 0 || z >= tileZCount) continue;

				for (int xo = -1; xo <= 1; xo++) {
					var x = tile.x + xo;
					if (x < 0 || x >= tileXCount) continue;

					// Ignore diagonals and the tile itself
					if ((xo == 0) == (zo == 0)) continue;

					var otherTile = tiles[x + z*tileXCount];
					if (!onlyUnflagged || !otherTile.flag) {
						ConnectTiles(otherTile, tile, TileWorldSizeX, TileWorldSizeZ, MaxTileConnectionEdgeDistance);
					}
				}
			}
		}

		protected void RemoveConnectionsFromTile (NavmeshTile tile) {
			if (tile.x > 0) {
				int x = tile.x-1;
				for (int z = tile.z; z < tile.z+tile.d; z++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
			if (tile.x+tile.w < tileXCount) {
				int x = tile.x+tile.w;
				for (int z = tile.z; z < tile.z+tile.d; z++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
			if (tile.z > 0) {
				int z = tile.z-1;
				for (int x = tile.x; x < tile.x+tile.w; x++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
			if (tile.z+tile.d < tileZCount) {
				int z = tile.z+tile.d;
				for (int x = tile.x; x < tile.x+tile.w; x++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
		}

		protected void RemoveConnectionsFromTo (NavmeshTile a, NavmeshTile b) {
			if (a == null || b == null) return;
			//Same tile, possibly from a large tile (one spanning several x,z tile coordinates)
			if (a == b) return;

			int tileIdx = b.x + b.z*tileXCount;

			for (int i = 0; i < a.nodes.Length; i++) {
				TriangleMeshNode node = a.nodes[i];
				if (node.connections == null) continue;
				for (int j = 0;; j++) {
					//Length will not be constant if connections are removed
					if (j >= node.connections.Length) break;

					var other = node.connections[j].node as TriangleMeshNode;

					//Only evaluate TriangleMeshNodes
					if (other == null) continue;

					int tileIdx2 = other.GetVertexIndex(0);
					tileIdx2 = (tileIdx2 >> TileIndexOffset) & TileIndexMask;

					if (tileIdx2 == tileIdx) {
						node.RemoveConnection(node.connections[j].node);
						j--;
					}
				}
			}
		}


		static readonly NNConstraint NNConstraintDistanceXZ = new NNConstraint { distanceXZ = true };

		public override NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
			return GetNearestForce(position, constraint != null && constraint.distanceXZ ? NNConstraintDistanceXZ : null);
		}

		public override NNInfoInternal GetNearestForce (Vector3 position, NNConstraint constraint) {
			if (tiles == null) return new NNInfoInternal();

			var tileCoords = GetTileCoordinates(position);

			// Clamp to graph borders
			tileCoords.x = Mathf.Clamp(tileCoords.x, 0, tileXCount-1);
			tileCoords.y = Mathf.Clamp(tileCoords.y, 0, tileZCount-1);

			int wmax = Math.Max(tileXCount, tileZCount);

			var best = new NNInfoInternal();
			float bestDistance = float.PositiveInfinity;

			bool xzSearch = nearestSearchOnlyXZ || (constraint != null && constraint.distanceXZ);

			// Search outwards in a diamond pattern from the closest tile
			//     2
			//   2 1 2
			// 2 1 0 1 2  etc.
			//   2 1 2
			//     2
			for (int w = 0; w < wmax; w++) {
				// Stop the loop when we can guarantee that no nodes will be closer than the ones we have already searched
				if (bestDistance < (w-2)*Math.Max(TileWorldSizeX, TileWorldSizeX)) break;

				int zmax = Math.Min(w+tileCoords.y +1, tileZCount);
				for (int z = Math.Max(-w+tileCoords.y, 0); z < zmax; z++) {
					// Solve for z such that abs(x-tx) + abs(z-tx) == w
					// Delta X coordinate
					int originalDx = Math.Abs(w - Math.Abs(z-tileCoords.y));
					var dx = originalDx;
					// Solution is dx + tx and -dx + tx
					// This loop will first check +dx and then -dx
					// If dx happens to be zero, then it will not run twice
					do {
						// Absolute x coordinate
						int x = -dx + tileCoords.x;
						if (x >= 0 && x < tileXCount) {
							NavmeshTile tile = tiles[x + z*tileXCount];

							if (tile != null) {
								if (xzSearch) {
									best = tile.bbTree.QueryClosestXZ(position, constraint, ref bestDistance, best);
								} else {
									best = tile.bbTree.QueryClosest(position, constraint, ref bestDistance, best);
								}
							}
						}

						dx = -dx;
					} while (dx != originalDx);
				}
			}

			best.node = best.constrainedNode;
			best.constrainedNode = null;
			best.clampedPosition = best.constClampedPosition;

			return best;
		}

		/// <summary>
		/// Finds the first node which contains position.
		/// "Contains" is defined as position is inside the triangle node when seen from above. So only XZ space matters.
		/// In case of a multilayered environment, which node of the possibly several nodes
		/// containing the point is undefined.
		///
		/// Returns null if there was no node containing the point. This serves as a quick
		/// check for "is this point on the navmesh or not".
		///
		/// Note that the behaviour of this method is distinct from the GetNearest method.
		/// The GetNearest method will return the closest node to a point,
		/// which is not necessarily the one which contains it in XZ space.
		///
		/// See: GetNearest
		/// </summary>
		public GraphNode PointOnNavmesh (Vector3 position, NNConstraint constraint) {
			if (tiles == null) return null;

			var tileCoords = GetTileCoordinates(position);

			// Graph borders
			if (tileCoords.x < 0 || tileCoords.y < 0 || tileCoords.x >= tileXCount || tileCoords.y >= tileZCount) return null;

			NavmeshTile tile = GetTile(tileCoords.x, tileCoords.y);

			if (tile != null) {
				GraphNode node = tile.bbTree.QueryInside(position, constraint);
				return node;
			}

			return null;
		}

		/// <summary>Fills graph with tiles created by NewEmptyTile</summary>
		protected void FillWithEmptyTiles () {
			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					tiles[z*tileXCount + x] = NewEmptyTile(x, z);
				}
			}
		}

		/// <summary>
		/// Create connections between all nodes.
		/// Version: Since 3.7.6 the implementation is thread safe
		/// </summary>
		protected static void CreateNodeConnections (TriangleMeshNode[] nodes) {
			List<Connection> connections = ListPool<Connection>.Claim();

			var nodeRefs = ObjectPoolSimple<Dictionary<Int2, int> >.Claim();

			nodeRefs.Clear();

			// Build node neighbours
			for (int i = 0; i < nodes.Length; i++) {
				TriangleMeshNode node = nodes[i];

				int av = node.GetVertexCount();

				for (int a = 0; a < av; a++) {
					// Recast can in some very special cases generate degenerate triangles which are simply lines
					// In that case, duplicate keys might be added and thus an exception will be thrown
					// It is safe to ignore the second edge though... I think (only found one case where this happens)
					var key = new Int2(node.GetVertexIndex(a), node.GetVertexIndex((a+1) % av));
					if (!nodeRefs.ContainsKey(key)) {
						nodeRefs.Add(key, i);
					}
				}
			}

			for (int i = 0; i < nodes.Length; i++) {
				TriangleMeshNode node = nodes[i];

				connections.Clear();

				int av = node.GetVertexCount();

				for (int a = 0; a < av; a++) {
					int first = node.GetVertexIndex(a);
					int second = node.GetVertexIndex((a+1) % av);
					int connNode;

					if (nodeRefs.TryGetValue(new Int2(second, first), out connNode)) {
						TriangleMeshNode other = nodes[connNode];

						int bv = other.GetVertexCount();

						for (int b = 0; b < bv; b++) {
							/// <summary>TODO: This will fail on edges which are only partially shared</summary>
							if (other.GetVertexIndex(b) == second && other.GetVertexIndex((b+1) % bv) == first) {
								connections.Add(new Connection(
									other,
									(uint)(node.position - other.position).costMagnitude,
									(byte)a
									));
								break;
							}
						}
					}
				}

				node.connections = connections.ToArrayFromPool();
				node.SetConnectivityDirty();
			}

			nodeRefs.Clear();
			ObjectPoolSimple<Dictionary<Int2, int> >.Release(ref nodeRefs);
			ListPool<Connection>.Release(ref connections);
		}

		protected struct JobConnectTiles : IJob {
			public System.Runtime.InteropServices.GCHandle tiles;
			public int tileIndex1;
			public int tileIndex2;
			public float tileWorldSizeX, tileWorldSizeZ;
			public float maxTileConnectionEdgeDistance;

			/// <summary>
			/// Schedule jobs to connect all the given tiles with each other while exploiting as much parallelism as possible.
			/// tilesHandle should be a GCHandle referring to a NavmeshTile[] array of size tileRect.Width*tileRect.Height.
			/// </summary>
			public static JobHandle ScheduleBatch (System.Runtime.InteropServices.GCHandle tilesHandle, JobHandle dependency, IntRect tileRect, Vector2 tileWorldSize, float maxTileConnectionEdgeDistance) {
				// First connect all tiles with an EVEN coordinate sum
				// This would be the white squares on a chess board.
				// Then connect all tiles with an ODD coordinate sum (which would be all black squares on a chess board).
				// This will prevent the different threads that do all
				// this in parallel from conflicting with each other.
				// The directions are also done separately
				// first they are connected along the X direction and then along the Z direction.
				// Looping over 0 and then 1
				var tileRectDepth = tileRect.Height;
				var tileRectWidth = tileRect.Width;
				var coordinateDependency = dependency;
				for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum++) {
					var dep = coordinateDependency;
					for (int direction = 0; direction <= 1; direction++) {
						for (int z = 0; z < tileRectDepth; z++) {
							for (int x = 0; x < tileRectWidth; x++) {
								var tileIndex = z*tileRectWidth + x;
								if ((x + z) % 2 == coordinateSum) {
									int tileIndex1 = x + z * tileRectWidth;
									int tileIndex2;
									if (direction == 0 && x < tileRectWidth - 1) {
										tileIndex2 = x + 1 + z * tileRectWidth;
									} else if (direction == 1 && z < tileRectDepth - 1) {
										tileIndex2 = x + (z + 1) * tileRectWidth;
									} else {
										continue;
									}

									var job = new JobConnectTiles {
										tiles = tilesHandle,
										tileIndex1 = tileIndex1,
										tileIndex2 = tileIndex2,
										tileWorldSizeX = tileWorldSize.x,
										tileWorldSizeZ = tileWorldSize.y,
										maxTileConnectionEdgeDistance = maxTileConnectionEdgeDistance,
									}.Schedule(coordinateDependency);
									dep = Unity.Jobs.JobHandle.CombineDependencies(dep, job);
								}
							}
						}

						coordinateDependency = dep;
					}
				}

				return coordinateDependency;
			}

			public void Execute () {
				var tiles = (NavmeshTile[])this.tiles.Target;

				ConnectTiles(tiles[tileIndex1], tiles[tileIndex2], tileWorldSizeX, tileWorldSizeZ, maxTileConnectionEdgeDistance);
			}
		}

		/// <summary>
		/// Generate connections between the two tiles.
		/// The tiles must be adjacent.
		/// </summary>
		protected static void ConnectTiles (NavmeshTile tile1, NavmeshTile tile2, float tileWorldSizeX, float tileWorldSizeZ, float maxTileConnectionEdgeDistance) {
			if (tile1 == null || tile2 == null) return;

			if (tile1.nodes == null) throw new System.ArgumentException("tile1 does not contain any nodes");
			if (tile2.nodes == null) throw new System.ArgumentException("tile2 does not contain any nodes");

			int t1x = Mathf.Clamp(tile2.x, tile1.x, tile1.x+tile1.w-1);
			int t2x = Mathf.Clamp(tile1.x, tile2.x, tile2.x+tile2.w-1);
			int t1z = Mathf.Clamp(tile2.z, tile1.z, tile1.z+tile1.d-1);
			int t2z = Mathf.Clamp(tile1.z, tile2.z, tile2.z+tile2.d-1);

			int coord, altcoord;
			int t1coord, t2coord;

			float tileWorldSize;

			// Figure out which side that is shared between the two tiles
			// and what coordinate index is fixed along that edge (x or z)
			if (t1x == t2x) {
				coord = 2;
				altcoord = 0;
				t1coord = t1z;
				t2coord = t2z;
				tileWorldSize = tileWorldSizeZ;
			} else if (t1z == t2z) {
				coord = 0;
				altcoord = 2;
				t1coord = t1x;
				t2coord = t2x;
				tileWorldSize = tileWorldSizeX;
			} else {
				throw new System.ArgumentException("Tiles are not adjacent (neither x or z coordinates match)");
			}

			if (Math.Abs(t1coord-t2coord) != 1) {
				throw new System.ArgumentException("Tiles are not adjacent (tile coordinates must differ by exactly 1. Got '" + t1coord + "' and '" + t2coord + "')");
			}

			// Midpoint between the two tiles
			int midpoint = (int)Math.Round((Math.Max(t1coord, t2coord) * tileWorldSize) * Int3.Precision);

#if ASTARDEBUG
			Vector3 v1 = new Vector3(-100, 0, -100);
			Vector3 v2 = new Vector3(100, 0, 100);
			v1[coord] = midpoint*Int3.PrecisionFactor;
			v2[coord] = midpoint*Int3.PrecisionFactor;

			Debug.DrawLine(v1, v2, Color.magenta);
#endif

			TriangleMeshNode[] nodes1 = tile1.nodes;
			TriangleMeshNode[] nodes2 = tile2.nodes;

			// Find all nodes of the second tile which are adjacent to the border between the tiles.
			// This is used to speed up the matching process (the impact can be very significant for large tiles, but is insignificant for small ones).
			TriangleMeshNode[] closeToEdge = ArrayPool<TriangleMeshNode>.Claim(nodes2.Length);
			int numCloseToEdge = 0;
			for (int j = 0; j < nodes2.Length; j++) {
				TriangleMeshNode nodeB = nodes2[j];
				int bVertexCount = nodeB.GetVertexCount();
				for (int b = 0; b < bVertexCount; b++) {
					// Note that we cannot use nodeB.GetVertexInGraphSpace because it might be the case that no graph even has this tile yet (common during updates/scanning the graph).
					// The node.GetVertexInGraphSpace will try to look up the graph it is contained in.
					// So we need to call NavmeshTile.GetVertexInGraphSpace instead.
					Int3 bVertex1 = tile2.GetVertexInGraphSpace(nodeB.GetVertexIndex(b));
					Int3 bVertex2 = tile2.GetVertexInGraphSpace(nodeB.GetVertexIndex((b+1) % bVertexCount));
					if (Math.Abs(bVertex1[coord] - midpoint) < 2 && Math.Abs(bVertex2[coord] - midpoint) < 2) {
						closeToEdge[numCloseToEdge] = nodes2[j];
						numCloseToEdge++;
						break;
					}
				}
			}


			// Find adjacent nodes on the border between the tiles
			for (int i = 0; i < nodes1.Length; i++) {
				TriangleMeshNode nodeA = nodes1[i];
				int aVertexCount = nodeA.GetVertexCount();

				// Loop through all *sides* of the node
				for (int a = 0; a < aVertexCount; a++) {
					// Vertices that the segment consists of
					Int3 aVertex1 = tile1.GetVertexInGraphSpace(nodeA.GetVertexIndex(a));
					Int3 aVertex2 = tile1.GetVertexInGraphSpace(nodeA.GetVertexIndex((a+1) % aVertexCount));

					// Check if it is really close to the tile border
					if (Math.Abs(aVertex1[coord] - midpoint) < 2 && Math.Abs(aVertex2[coord] - midpoint) < 2) {
						int minalt = Math.Min(aVertex1[altcoord], aVertex2[altcoord]);
						int maxalt = Math.Max(aVertex1[altcoord], aVertex2[altcoord]);

						// Degenerate edge
						if (minalt == maxalt) continue;

						for (int j = 0; j < numCloseToEdge; j++) {
							TriangleMeshNode nodeB = closeToEdge[j];
							int bVertexCount = nodeB.GetVertexCount();
							for (int b = 0; b < bVertexCount; b++) {
								Int3 bVertex1 = tile2.GetVertexInGraphSpace(nodeB.GetVertexIndex(b));
								Int3 bVertex2 = tile2.GetVertexInGraphSpace(nodeB.GetVertexIndex((b+1) % bVertexCount));
								if (Math.Abs(bVertex1[coord] - midpoint) < 2 && Math.Abs(bVertex2[coord] - midpoint) < 2) {
									int minalt2 = Math.Min(bVertex1[altcoord], bVertex2[altcoord]);
									int maxalt2 = Math.Max(bVertex1[altcoord], bVertex2[altcoord]);

									// Degenerate edge
									if (minalt2 == maxalt2) continue;

									if (maxalt > minalt2 && minalt < maxalt2) {
										// The two nodes seem to be adjacent

										// Test shortest distance between the segments (first test if they are equal since that is much faster and pretty common)
										if ((aVertex1 == bVertex1 && aVertex2 == bVertex2) || (aVertex1 == bVertex2 && aVertex2 == bVertex1) ||
											VectorMath.SqrDistanceSegmentSegment((Vector3)aVertex1, (Vector3)aVertex2, (Vector3)bVertex1, (Vector3)bVertex2) < maxTileConnectionEdgeDistance*maxTileConnectionEdgeDistance) {
											uint cost = (uint)(nodeA.position - nodeB.position).costMagnitude;

											nodeA.AddConnection(nodeB, cost, (byte)a);
											nodeB.AddConnection(nodeA, cost, (byte)b);
										}
									}
								}
							}
						}
					}
				}
			}

			ArrayPool<TriangleMeshNode>.Release(ref closeToEdge);
		}

		/// <summary>
		/// Start batch updating of tiles.
		/// During batch updating, tiles will not be connected if they are updating with ReplaceTile.
		/// When ending batching, all affected tiles will be connected.
		/// This is faster than not using batching.
		/// </summary>
		public void StartBatchTileUpdate () {
			if (batchTileUpdate) throw new System.InvalidOperationException("Calling StartBatchLoad when batching is already enabled");
			batchTileUpdate = true;
		}

		/// <summary>
		/// Destroy several nodes simultaneously.
		/// This is faster than simply looping through the nodes and calling the node.Destroy method because some optimizations
		/// relating to how connections are removed can be optimized.
		/// </summary>
		void DestroyNodes (List<MeshNode> nodes) {
			for (int i = 0; i < batchNodesToDestroy.Count; i++) {
				batchNodesToDestroy[i].TemporaryFlag1 = true;
			}

			for (int i = 0; i < batchNodesToDestroy.Count; i++) {
				var node = batchNodesToDestroy[i];
				for (int j = 0; j < node.connections.Length; j++) {
					var neighbour = node.connections[j].node;
					if (!neighbour.TemporaryFlag1) {
						neighbour.RemoveConnection(node);
					}
				}

				// Remove the connections array explicitly for performance.
				// Otherwise the Destroy method will try to remove the connections in both directions one by one which is slow.
				ArrayPool<Connection>.Release(ref node.connections, true);
				node.Destroy();
			}
		}

		void TryConnect (int tileIdx1, int tileIdx2) {
			// If both tiles were flagged, then only connect if tileIdx1 < tileIdx2 to make sure we don't connect the tiles twice
			// as this method will be called with swapped arguments as well.
			if (tiles[tileIdx1].flag && tiles[tileIdx2].flag && tileIdx1 >= tileIdx2) return;
			ConnectTiles(tiles[tileIdx1], tiles[tileIdx2], TileWorldSizeX, TileWorldSizeZ, MaxTileConnectionEdgeDistance);
		}

		/// <summary>
		/// End batch updating of tiles.
		/// During batch updating, tiles will not be connected if they are updated with ReplaceTile.
		/// When ending batching, all affected tiles will be connected.
		/// This is faster than not using batching.
		/// </summary>
		public void EndBatchTileUpdate () {
			if (!batchTileUpdate) throw new System.InvalidOperationException("Calling EndBatchTileUpdate when batching had not yet been started");

			batchTileUpdate = false;

			DestroyNodes(batchNodesToDestroy);
			batchNodesToDestroy.ClearFast();

			for (int i = 0; i < batchUpdatedTiles.Count; i++) tiles[batchUpdatedTiles[i]].flag = true;

			for (int i = 0; i < batchUpdatedTiles.Count; i++) {
				int x = batchUpdatedTiles[i] % tileXCount, z = batchUpdatedTiles[i] / tileXCount;
				if (x > 0) TryConnect(batchUpdatedTiles[i], batchUpdatedTiles[i] - 1);
				if (x < tileXCount - 1) TryConnect(batchUpdatedTiles[i], batchUpdatedTiles[i] + 1);
				if (z > 0) TryConnect(batchUpdatedTiles[i], batchUpdatedTiles[i] - tileXCount);
				if (z < tileZCount - 1) TryConnect(batchUpdatedTiles[i], batchUpdatedTiles[i] + tileXCount);
			}

			for (int i = 0; i < batchUpdatedTiles.Count; i++) tiles[batchUpdatedTiles[i]].flag = false;

			batchUpdatedTiles.ClearFast();
		}

		/// <summary>
		/// Clear the tile at the specified coordinate.
		/// Must be called during a batch update, see <see cref="StartBatchTileUpdate"/>.
		/// </summary>
		protected void ClearTile (int x, int z) {
			if (!batchTileUpdate) throw new System.Exception("Must be called during a batch update. See StartBatchTileUpdate");
			var tile = GetTile(x, z);
			if (tile == null) return;
			var nodes = tile.nodes;
			for (int i = 0; i < nodes.Length; i++) {
				if (nodes[i] != null) batchNodesToDestroy.Add(nodes[i]);
			}
			ObjectPool<BBTree>.Release(ref tile.bbTree);
			// TODO: Pool tile object and various arrays in it?
			tiles[x + z*tileXCount] = null;
		}

		/// <summary>Temporary buffer used in <see cref="PrepareNodeRecycling"/></summary>
		Dictionary<int, int> nodeRecyclingHashBuffer = new Dictionary<int, int>();

		/// <summary>
		/// Reuse nodes that keep the exact same vertices after a tile replacement.
		/// The reused nodes will be added to the recycledNodeBuffer array at the index corresponding to the
		/// indices in the triangle array that its vertices uses.
		///
		/// All connections on the reused nodes will be removed except ones that go to other graphs.
		/// The reused nodes will be removed from the tile by replacing it with a null slot in the node array.
		///
		/// See: <see cref="ReplaceTile"/>
		/// </summary>
		void PrepareNodeRecycling (int x, int z, Int3[] verts, int[] tris, TriangleMeshNode[] recycledNodeBuffer) {
			NavmeshTile tile = GetTile(x, z);

			if (tile == null || tile.nodes.Length == 0) return;
			var nodes = tile.nodes;
			var recycling = nodeRecyclingHashBuffer;
			for (int i = 0, j = 0; i < tris.Length; i += 3, j++) {
				recycling[verts[tris[i+0]].GetHashCode() + verts[tris[i+1]].GetHashCode() + verts[tris[i+2]].GetHashCode()] = j;
			}
			var connectionsToKeep = ListPool<Connection>.Claim();

			for (int i = 0; i < nodes.Length; i++) {
				var node = nodes[i];
				Int3 v0, v1, v2;
				node.GetVerticesInGraphSpace(out v0, out v1, out v2);
				var hash = v0.GetHashCode() + v1.GetHashCode() + v2.GetHashCode();
				int newNodeIndex;
				if (recycling.TryGetValue(hash, out newNodeIndex)) {
					// Technically we should check for a cyclic permutations of the vertices (e.g node a,b,c could become node b,c,a)
					// but in almost all cases the vertices will keep the same order. Allocating one or two extra nodes isn't such a big deal.
					if (verts[tris[3*newNodeIndex+0]] == v0 && verts[tris[3*newNodeIndex+1]] == v1 && verts[tris[3*newNodeIndex+2]] == v2) {
						recycledNodeBuffer[newNodeIndex] = node;
						// Remove the node from the tile
						nodes[i] = null;
						// Only keep connections to nodes on other graphs
						// Usually there are no connections to nodes to other graphs and this is faster than removing all connections them one by one
						for (int j = 0; j < node.connections.Length; j++) {
							if (node.connections[j].node.GraphIndex != node.GraphIndex) {
								connectionsToKeep.Add(node.connections[j]);
							}
						}
						ArrayPool<Connection>.Release(ref node.connections, true);
						if (connectionsToKeep.Count > 0) {
							node.connections = connectionsToKeep.ToArrayFromPool();
							node.SetConnectivityDirty();
							connectionsToKeep.Clear();
						}
					}
				}
			}

			recycling.Clear();
			ListPool<Connection>.Release(ref connectionsToKeep);
		}

		/// <summary>
		/// Replace tile at index with nodes created from specified navmesh.
		/// This will create new nodes and link them to the adjacent tile (unless batching has been started in which case that will be done when batching ends).
		///
		/// The vertices are assumed to be in 'tile space', that is being in a rectangle with
		/// one corner at the origin and one at (<see cref="TileWorldSizeX"/>, 0, <see cref="TileWorldSizeZ)"/>.
		///
		/// Note: The vertex and triangle arrays may be modified and will be stored with the tile data.
		/// do not modify them after this method has been called.
		///
		/// See: <see cref="StartBatchTileUpdate"/>
		/// </summary>
		public void ReplaceTile (int x, int z, Int3[] verts, int[] tris) {
			AssertSafeToUpdateGraph();
			int w = 1, d = 1;

			if (x + w > tileXCount || z+d > tileZCount || x < 0 || z < 0) {
				throw new System.ArgumentException("Tile is placed at an out of bounds position or extends out of the graph bounds ("+x+", " + z + " [" + w + ", " + d+ "] " + tileXCount + " " + tileZCount + ")");
			}

			if (tris.Length % 3 != 0) throw new System.ArgumentException("Triangle array's length must be a multiple of 3 (tris)");
			if (verts.Length > VertexIndexMask) {
				Debug.LogError("Too many vertices in the tile (" + verts.Length + " > " + VertexIndexMask +")\nYou can enable ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector to raise this limit. Or you can use a smaller tile size to reduce the likelihood of this happening.");
				verts = new Int3[0];
				tris = new int[0];
			}

			var wasNotBatching = !batchTileUpdate;
			if (wasNotBatching) StartBatchTileUpdate();
			Profiler.BeginSample("Tile Initialization");

			//Create a new navmesh tile and assign its settings
			var tile = new NavmeshTile {
				x = x,
				z = z,
				w = w,
				d = d,
				tris = tris,
				bbTree = ObjectPool<BBTree>.Claim(),
				graph = this,
			};

			if (!Mathf.Approximately(x*TileWorldSizeX*Int3.FloatPrecision, (float)Math.Round(x*TileWorldSizeX*Int3.FloatPrecision))) Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
			if (!Mathf.Approximately(z*TileWorldSizeZ*Int3.FloatPrecision, (float)Math.Round(z*TileWorldSizeZ*Int3.FloatPrecision))) Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");

			var offset = (Int3) new Vector3((x * TileWorldSizeX), 0, (z * TileWorldSizeZ));

			for (int i = 0; i < verts.Length; i++) {
				verts[i] += offset;
			}
			tile.vertsInGraphSpace = verts;
			tile.verts = (Int3[])verts.Clone();
			transform.Transform(tile.verts);

			Profiler.BeginSample("Clear Previous Tiles");

			// Create a backing array for the new nodes
			var nodes = tile.nodes = new TriangleMeshNode[tris.Length/3];
			// Recycle any nodes that are in the exact same spot after replacing the tile.
			// This also keeps e.g penalties and tags and other connections which might be useful.
			// It also avoids trashing the paths for the RichAI component (as it will have to immediately recalculate its path
			// if it discovers that its path contains destroyed nodes).
			PrepareNodeRecycling(x, z, tile.vertsInGraphSpace, tris, tile.nodes);
			// Remove previous tiles (except the nodes that were recycled above)
			ClearTile(x, z);

			Profiler.EndSample();
			Profiler.EndSample();

			Profiler.BeginSample("Assign Node Data");

			// Set tile
			tiles[x + z*tileXCount] = tile;
			batchUpdatedTiles.Add(x + z*tileXCount);

			// Create nodes and assign triangle indices
			CreateNodes(tile, tile.tris, x + z*tileXCount, (uint)active.data.GetGraphIndex(this), null, active, initialPenalty, RecalculateNormals);

			Profiler.EndSample();
			Profiler.BeginSample("AABBTree Rebuild");
			tile.bbTree.RebuildFrom(tile);
			Profiler.EndSample();

			Profiler.BeginSample("Create Node Connections");
			CreateNodeConnections(tile.nodes);
			Profiler.EndSample();

			Profiler.BeginSample("Connect With Neighbours");

			if (wasNotBatching) EndBatchTileUpdate();
			Profiler.EndSample();
		}

		protected static void CreateNodes (NavmeshTile tile, int[] tris, int tileIndex, uint graphIndex, uint[] tags, AstarPath astar, uint initialPenalty, bool recalculateNormals) {
			var nodes = tile.nodes;

			if (nodes == null || nodes.Length < tris.Length/3) throw new System.ArgumentException("nodes must be non null and at least as large as tris.Length/3");
			// This index will be ORed to the triangle indices
			tileIndex <<= TileIndexOffset;

			// Create nodes and assign vertex indices
			for (int i = 0; i < nodes.Length; i++) {
				var node = nodes[i];
				// Allow the nodes to be partially filled in already to allow for recycling nodes
				if (node == null) node = nodes[i] = new TriangleMeshNode(astar);

				// Reset all relevant fields on the node (even on recycled nodes to avoid exposing internal implementation details)
				node.Walkable = true;
				node.Tag = tags != null ? tags[i] : 0;
				node.Penalty = initialPenalty;
				node.GraphIndex = graphIndex;
				// The vertices stored on the node are composed
				// out of the triangle index and the tile index
				node.v0 = tris[i*3+0] | tileIndex;
				node.v1 = tris[i*3+1] | tileIndex;
				node.v2 = tris[i*3+2] | tileIndex;

				// Make sure the triangle is clockwise in graph space (it may not be in world space since the graphs can be rotated)
				// Note that we also modify the original triangle array because if the graph is cached then we will re-initialize the nodes from that array and assume all triangles are clockwise.
				var v0 = tile.GetVertexInGraphSpace(node.v0);
				var v1 = tile.GetVertexInGraphSpace(node.v1);
				var v2 = tile.GetVertexInGraphSpace(node.v2);

				if (recalculateNormals && !VectorMath.IsClockwiseXZ(tile.GetVertexInGraphSpace(node.v0), tile.GetVertexInGraphSpace(node.v1), tile.GetVertexInGraphSpace(node.v2))) {
					Memory.Swap(ref tris[i*3+0], ref tris[i*3+2]);
					Memory.Swap(ref node.v0, ref node.v2);
				}

				// This is equivalent to calling node.UpdatePositionFromVertices(), but that would require the tile to be attached to a graph, which it might not be at this stage.
				node.position = (tile.GetVertex(node.v0) + tile.GetVertex(node.v1) + tile.GetVertex(node.v2)) * 0.333333f;
			}
		}

		public NavmeshBase () {
			navmeshUpdateData = new NavmeshUpdates.NavmeshUpdateSettings(this);
		}

		/// <summary>
		/// Returns if there is an obstacle between origin and end on the graph.
		/// This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public bool Linecast (Vector3 origin, Vector3 end) {
			return Linecast(origin, end, null);
		}

		/// <summary>
		/// Returns if there is an obstacle between origin and end on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="origin">Point to linecast from.</param>
		/// <param name="end">Point to linecast to.</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo.</param>
		/// <param name="hint">You may pass the node closest to the start point if you already know it for a minor performance boost.
		/// 			   If null, a search for the closest node will be done. This parameter is mostly deprecated and should be avoided. Pass null instead.</param>
		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(this, origin, end, hint, out hit, null);
		}

		/// <summary>
		/// Returns if there is an obstacle between origin and end on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="origin">Point to linecast from.</param>
		/// <param name="end">Point to linecast to.</param>
		/// <param name="hint">You may pass the node closest to the start point if you already know it for a minor performance boost.
		/// 			   If null, a search for the closest node will be done. This parameter is mostly deprecated and should be avoided. Pass null instead.</param>
		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint) {
			GraphHitInfo hit;

			return Linecast(this, origin, end, hint, out hit, null);
		}

		/// <summary>
		/// Returns if there is an obstacle between origin and end on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="origin">Point to linecast from.</param>
		/// <param name="end">Point to linecast to.</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo.</param>
		/// <param name="hint">You may pass the node closest to the start point if you already know it for a minor performance boost.
		/// 			   If null, a search for the closest node will be done. This parameter is mostly deprecated and should be avoided. Pass null instead.</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes the linecast traverses.</param>
		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace) {
			return Linecast(this, origin, end, hint, out hit, trace);
		}

		/// <summary>
		/// Returns if there is an obstacle between origin and end on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="origin">Point to linecast from.</param>
		/// <param name="end">Point to linecast to.</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo.</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes the linecast traverses.</param>
		/// <param name="filter">If not null then the delegate will be called for each node and if it returns false the node will be treated as unwalkable and a hit will be returned.
		///               Note that unwalkable nodes are always treated as unwalkable regardless of what this filter returns.</param>
		public bool Linecast (Vector3 origin, Vector3 end, out GraphHitInfo hit, List<GraphNode> trace, System.Func<GraphNode, bool> filter) {
			return Linecast(this, origin, end, null, out hit, trace, filter);
		}

		/// <summary>
		/// Returns if there is an obstacle between origin and end on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="graph">The graph to perform the search on.</param>
		/// <param name="origin">Point to start from.</param>
		/// <param name="end">Point to linecast to.</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo.</param>
		/// <param name="hint">You may pass the node closest to the start point if you already know it for a minor performance boost.
		/// 			   If null, a search for the closest node will be done. This parameter is mostly deprecated and should be avoided. Pass null instead.</param>
		public static bool Linecast (NavmeshBase graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(graph, origin, end, hint, out hit, null);
		}

		/// <summary>Cached <see cref="Pathfinding.NNConstraint.None"/> with distanceXZ=true to reduce allocations</summary>
		static readonly NNConstraint NNConstraintNoneXZ = new NNConstraint {
			constrainWalkability = false,
			constrainArea = false,
			constrainTags = false,
			constrainDistance = false,
			graphMask = -1,
			distanceXZ = true,
		};

		/// <summary>Used to optimize linecasts by precomputing some values</summary>
		static readonly byte[] LinecastShapeEdgeLookup;

		static NavmeshBase () {
			// Want want to figure out which side of a triangle that a ray exists using.
			// There are only 3*3*3 = 27 different options for the [left/right/colinear] options for the 3 vertices of a triangle.
			// So we can precompute the result to improve the performance of linecasts.
			// For simplicity we reserve 2 bits for each side which means that we have 4*4*4 = 64 entries in the lookup table.
			LinecastShapeEdgeLookup = new byte[64];
			Side[] sideOfLine = new Side[3];
			for (int i = 0; i < LinecastShapeEdgeLookup.Length; i++) {
				sideOfLine[0] = (Side)((i >> 0) & 0x3);
				sideOfLine[1] = (Side)((i >> 2) & 0x3);
				sideOfLine[2] = (Side)((i >> 4) & 0x3);
				LinecastShapeEdgeLookup[i] = 0xFF;
				// Value 3 is an invalid value. So we just skip it.
				if (sideOfLine[0] != (Side)3 && sideOfLine[1] != (Side)3 && sideOfLine[2] != (Side)3) {
					// Figure out the side of the triangle that the line exits.
					// In case the line passes through one of the vertices of the triangle
					// there may be multiple alternatives. In that case pick the edge
					// which contains the fewest vertices that lie on the line.
					// This prevents a potential infinite loop when a linecast is done colinear
					// to the edge of a triangle.
					int bestBadness = int.MaxValue;
					for (int j = 0; j < 3; j++) {
						if ((sideOfLine[j] == Side.Left || sideOfLine[j] == Side.Colinear) && (sideOfLine[(j+1)%3] == Side.Right || sideOfLine[(j+1)%3] == Side.Colinear)) {
							var badness = (sideOfLine[j] == Side.Colinear ? 1 : 0) + (sideOfLine[(j+1)%3] == Side.Colinear ? 1 : 0);
							if (badness < bestBadness) {
								LinecastShapeEdgeLookup[i] = (byte)j;
								bestBadness = badness;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns if there is an obstacle between origin and end on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersections.
		///
		/// Note: This method only makes sense for graphs in which there is a definite 'up' direction. For example it does not make sense for e.g spherical graphs,
		/// navmeshes in which characters can walk on walls/ceilings or other curved worlds. If you try to use this method on such navmeshes it may output nonsense.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="graph">The graph to perform the search on</param>
		/// <param name="origin">Point to start from. This point should be on the navmesh. It will be snapped to the closest point on the navmesh otherwise.</param>
		/// <param name="end">Point to linecast to</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo</param>
		/// <param name="hint">If you already know the node which contains the origin point, you may pass it here for slighly improved performance. If null, a search for the closest node will be done.</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes along the line up until it hits an obstacle or reaches the end.</param>
		/// <param name="filter">If not null then the delegate will be called for each node and if it returns false the node will be treated as unwalkable and a hit will be returned.
		///               Note that unwalkable nodes are always treated as unwalkable regardless of what this filter returns.</param>
		public static bool Linecast (NavmeshBase graph, Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace, System.Func<GraphNode, bool> filter = null) {
			if (!graph.RecalculateNormals) {
				throw new System.InvalidOperationException("The graph is configured to not recalculate normals. This is typically used for spherical navmeshes or other non-planar ones. Linecasts cannot be done on such navmeshes. Enable 'Recalculate Normals' on the navmesh graph if you want to use linecasts.");
			}

			hit = new GraphHitInfo();

			if (float.IsNaN(origin.x + origin.y + origin.z)) throw new System.ArgumentException("origin is NaN");
			if (float.IsNaN(end.x + end.y + end.z)) throw new System.ArgumentException("end is NaN");

			var node = hint as TriangleMeshNode;
			if (node == null) {
				node = graph.GetNearest(origin, NNConstraintNoneXZ).node as TriangleMeshNode;

				if (node == null) {
					Debug.LogError("Could not find a valid node to start from");
					hit.origin = origin;
					hit.point = origin;
					return true;
				}
			}

			// Snap the origin to the navmesh
			var i3originInGraphSpace = node.ClosestPointOnNodeXZInGraphSpace(origin);
			hit.origin = graph.transform.Transform((Vector3)i3originInGraphSpace);

			if (!node.Walkable || (filter != null && !filter(node))) {
				hit.node = node;
				hit.point = hit.origin;
				hit.tangentOrigin = hit.origin;
				return true;
			}

			var endInGraphSpace = graph.transform.InverseTransform(end);
			var i3endInGraphSpace = (Int3)endInGraphSpace;

			// Fast early out check
			if (i3originInGraphSpace == i3endInGraphSpace) {
				hit.point = hit.origin;
				hit.node = node;
				if (trace != null) trace.Add(node);
				return false;
			}

			int counter = 0;
			while (true) {
				counter++;
				if (counter > 2000) {
					Debug.LogError("Linecast was stuck in infinite loop. Breaking.");
					return true;
				}

				if (trace != null) trace.Add(node);

				Int3 a0, a1, a2;
				node.GetVerticesInGraphSpace(out a0, out a1, out a2);
				int sideOfLine = (byte)VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, a0);
				sideOfLine |= (byte)VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, a1) << 2;
				sideOfLine |= (byte)VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, a2) << 4;
				// Use a lookup table to figure out which side of this triangle that the ray exits
				int shapeEdgeA = (int)LinecastShapeEdgeLookup[sideOfLine];
				// The edge consists of the vertex with index 'sharedEdgeA' and the next vertex after that (index '(sharedEdgeA+1)%3')

				var sideNodeExit = VectorMath.SideXZ(shapeEdgeA == 0 ? a0 : (shapeEdgeA == 1 ? a1 : a2), shapeEdgeA == 0 ? a1 : (shapeEdgeA == 1 ? a2 : a0), i3endInGraphSpace);
				if (sideNodeExit != Side.Left) {
					// Ray stops before it leaves the current node.
					// The endpoint must be inside the current node.

					hit.point = end;
					hit.node = node;

					var endNode = graph.GetNearest(end, NNConstraintNoneXZ).node as TriangleMeshNode;
					if (endNode == node || endNode == null) {
						// We ended up at the right node.
						// If endNode == null we also take this branch.
						// That case may happen if a linecast is made to a point, but the point way a very large distance straight up into the air.
						// The linecast may indeed reach the right point, but it's so far away up into the air that the GetNearest method will stop searching.
						return false;
					} else {
						// The closest node to the end point was not the node we ended up at.
						// This can happen if a linecast is done between two floors of a building.
						// The linecast may reach the right location when seen from above
						// but it will have ended up on the wrong floor of the building.
						// This indicates that the start and end points cannot be connected by a valid straight line on the navmesh.
						return true;
					}
				}

				if (shapeEdgeA == 0xFF) {
					// Line does not intersect node at all?
					// This may theoretically happen if the origin was not properly snapped to the inside of the triangle, but is instead a tiny distance outside the node.
					Debug.LogError("Line does not intersect node at all");
					hit.node = node;
					hit.point = hit.tangentOrigin = hit.origin;
					return true;
				} else {
					bool success = false;
					var nodeConnections = node.connections;

					// Check all node connetions to see which one is the next node along the ray's path
					for (int i = 0; i < nodeConnections.Length; i++) {
						if (nodeConnections[i].shapeEdge == shapeEdgeA) {
							// This might be the next node that we enter

							var neighbour = nodeConnections[i].node as TriangleMeshNode;
							if (neighbour == null || !neighbour.Walkable || (filter != null && !filter(neighbour))) continue;

							var neighbourConnections = neighbour.connections;
							int shapeEdgeB = -1;
							for (int j = 0; j < neighbourConnections.Length; j++) {
								if (neighbourConnections[j].node == node) {
									shapeEdgeB = neighbourConnections[j].shapeEdge;
									break;
								}
							}

							if (shapeEdgeB == -1) {
								// Connection was mono-directional!
								// This shouldn't normally happen on navmeshes (when the shapeEdge matches at least) unless a user has done something strange to the navmesh.
								continue;
							}

							var side1 = VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, neighbour.GetVertexInGraphSpace(shapeEdgeB));
							var side2 = VectorMath.SideXZ(i3originInGraphSpace, i3endInGraphSpace, neighbour.GetVertexInGraphSpace((shapeEdgeB+1) % 3));

							// Check if the line enters this edge
							success = (side1 == Side.Right || side1 == Side.Colinear) && (side2 == Side.Left || side2 == Side.Colinear);

							if (!success) continue;

							// Ray has entered the neighbouring node.
							// After the first node, it is possible to prove the loop invariant that shapeEdgeA will *never* end up as -1 (checked above)
							// Since side = Colinear acts essentially as a wildcard. side1 and side2 can be the most restricted if they are side1=right, side2=left.
							// Then when we get to the next node we know that the sideOfLine array is either [*, Right, Left], [Left, *, Right] or [Right, Left, *], where * is unknown.
							// We are looking for the sequence [Left, Right] (possibly including Colinear as wildcard). We will always find this sequence regardless of the value of *.
							node = neighbour;
							break;
						}
					}

					if (!success) {
						// Node did not enter any neighbours
						// It must have hit the border of the navmesh
						var hitEdgeStartInGraphSpace = (Vector3)(shapeEdgeA == 0 ? a0 : (shapeEdgeA == 1 ? a1 : a2));
						var hitEdgeEndInGraphSpace = (Vector3)(shapeEdgeA == 0 ? a1 : (shapeEdgeA == 1 ? a2 : a0));
						var intersectionInGraphSpace = VectorMath.LineIntersectionPointXZ(hitEdgeStartInGraphSpace, hitEdgeEndInGraphSpace, (Vector3)i3originInGraphSpace, (Vector3)i3endInGraphSpace);
						hit.point = graph.transform.Transform(intersectionInGraphSpace);
						hit.node = node;
						var hitEdgeStart = graph.transform.Transform(hitEdgeStartInGraphSpace);
						var hitEdgeEnd = graph.transform.Transform(hitEdgeEndInGraphSpace);
						hit.tangent = hitEdgeEnd - hitEdgeStart;
						hit.tangentOrigin = hitEdgeStart;
						return true;
					}
				}
			}
		}

		public override void OnDrawGizmos (DrawingData gizmos, bool drawNodes, RedrawScope redrawScope) {
			if (!drawNodes) {
				return;
			}

			using (var builder = gizmos.GetBuilder(redrawScope)) {
				var bounds = new Bounds();
				bounds.SetMinMax(Vector3.zero, forcedBoundsSize);
				// Draw a write cube using the latest transform
				// (this makes the bounds update immediately if some field is changed in the editor)
				using (builder.WithMatrix(CalculateTransform().matrix)) {
					builder.WireBox(bounds, Color.white);
				}
			}

			if (tiles != null && (showMeshSurface || showMeshOutline || showNodeConnections)) {
				var baseHasher = new NodeHasher(active);
				baseHasher.Add(showMeshOutline ? 1 : 0);
				baseHasher.Add(showMeshSurface ? 1 : 0);
				baseHasher.Add(showNodeConnections ? 1 : 0);

				int startTileIndex = 0;
				var hasher = baseHasher;
				var hashedNodes = 0;

				// Update navmesh vizualizations for
				// the tiles that have been changed
				for (int i = 0; i < tiles.Length; i++) {
					// This may happen if an exception has been thrown when the graph was scanned.
					// We don't want the gizmo code to start to throw exceptions as well then as
					// that would obscure the actual source of the error.
					if (tiles[i] == null) continue;

					// Calculate a hash of the tile
					var nodes = tiles[i].nodes;
					for (int j = 0; j < nodes.Length; j++) {
						hasher.HashNode(nodes[j]);
					}
					hashedNodes += nodes.Length;

					// Note: do not batch more than some large number of nodes at a time.
					// Also do not batch more than a single "row" of the graph at once
					// because otherwise a small change in one part of the graph could invalidate
					// the caches almost everywhere else.
					// When restricting the caches to row by row a change in a row
					// will never invalidate the cache in another row.
					if (hashedNodes > 1024 || (i % tileXCount) == tileXCount - 1 || i == tiles.Length - 1) {
						if (!gizmos.Draw(hasher, redrawScope)) {
							using (var helper = GraphGizmoHelper.GetGizmoHelper(gizmos, active, hasher, redrawScope)) {
								if (showMeshSurface || showMeshOutline) {
									CreateNavmeshSurfaceVisualization(tiles, startTileIndex, i + 1, helper);
									CreateNavmeshOutlineVisualization(tiles, startTileIndex, i + 1, helper);
								}

								if (showNodeConnections) {
									for (int ti = startTileIndex; ti <= i; ti++) {
										if (tiles[ti] == null) continue;

										var tileNodes = tiles[ti].nodes;
										for (int j = 0; j < tileNodes.Length; j++) {
											helper.DrawConnections(tileNodes[j]);
										}
									}
								}
							}
						}

						startTileIndex = i + 1;
						hasher = baseHasher;
						hashedNodes = 0;
					}
				}
			}

			if (active.showUnwalkableNodes) DrawUnwalkableNodes(gizmos, active.unwalkableNodeDebugSize, redrawScope);
		}

		/// <summary>Creates a mesh of the surfaces of the navmesh for use in OnDrawGizmos in the editor</summary>
		void CreateNavmeshSurfaceVisualization (NavmeshTile[] tiles, int startTile, int endTile, GraphGizmoHelper helper) {
			int numNodes = 0;

			for (int i = startTile; i < endTile; i++) if (tiles[i] != null) numNodes += tiles[i].nodes.Length;

			// Vertex array might be a bit larger than necessary, but that's ok
			var vertices = ArrayPool<Vector3>.Claim(numNodes*3);
			var colors = ArrayPool<Color>.Claim(numNodes*3);
			int offset = 0;
			for (int i = startTile; i < endTile; i++) {
				var tile = tiles[i];
				if (tile == null) continue;

				for (int j = 0; j < tile.nodes.Length; j++) {
					var node = tile.nodes[j];
					Int3 v0, v1, v2;
					node.GetVertices(out v0, out v1, out v2);
					int index = offset + j*3;
					vertices[index + 0] = (Vector3)v0;
					vertices[index + 1] = (Vector3)v1;
					vertices[index + 2] = (Vector3)v2;

					var color = helper.NodeColor(node);
					colors[index + 0] = colors[index + 1] = colors[index + 2] = color;
				}
				offset += tile.nodes.Length * 3;
			}

			if (showMeshSurface) helper.DrawTriangles(vertices, colors, numNodes);
			if (showMeshOutline) helper.DrawWireTriangles(vertices, colors, numNodes);

			// Return lists to the pool
			ArrayPool<Vector3>.Release(ref vertices);
			ArrayPool<Color>.Release(ref colors);
		}

		/// <summary>Creates an outline of the navmesh for use in OnDrawGizmos in the editor</summary>
		static void CreateNavmeshOutlineVisualization (NavmeshTile[] tiles, int startTile, int endTile, GraphGizmoHelper helper) {
			var sharedEdges = new bool[3];

			for (int i = startTile; i < endTile; i++) {
				var tile = tiles[i];
				if (tile == null) continue;

				for (int j = 0; j < tile.nodes.Length; j++) {
					sharedEdges[0] = sharedEdges[1] = sharedEdges[2] = false;

					var node = tile.nodes[j];
					for (int c = 0; c < node.connections.Length; c++) {
						var other = node.connections[c].node as TriangleMeshNode;

						// Loop through neighbours to figure out which edges are shared
						if (other != null && other.GraphIndex == node.GraphIndex) {
							for (int v = 0; v < 3; v++) {
								for (int v2 = 0; v2 < 3; v2++) {
									if (node.GetVertexIndex(v) == other.GetVertexIndex((v2+1)%3) && node.GetVertexIndex((v+1)%3) == other.GetVertexIndex(v2)) {
										// Found a shared edge with the other node
										sharedEdges[v] = true;
										v = 3;
										break;
									}
								}
							}
						}
					}

					var color = helper.NodeColor(node);
					for (int v = 0; v < 3; v++) {
						if (!sharedEdges[v]) {
							helper.builder.Line((Vector3)node.GetVertex(v), (Vector3)node.GetVertex((v+1)%3), color);
						}
					}
				}
			}
		}

		/// <summary>
		/// Serializes Node Info.
		/// Should serialize:
		/// - Base
		///    - Node Flags
		///    - Node Penalties
		///    - Node
		/// - Node Positions (if applicable)
		/// - Any other information necessary to load the graph in-game
		/// All settings marked with json attributes (e.g JsonMember) have already been
		/// saved as graph settings and do not need to be handled here.
		///
		/// It is not necessary for this implementation to be forward or backwards compatible.
		///
		/// See:
		/// </summary>
		protected override void SerializeExtraInfo (GraphSerializationContext ctx) {
			BinaryWriter writer = ctx.writer;

			if (tiles == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(tileXCount);
			writer.Write(tileZCount);

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					NavmeshTile tile = tiles[x + z*tileXCount];

					if (tile == null) {
						throw new System.Exception("NULL Tile");
						//writer.Write (-1);
						//continue;
					}

					writer.Write(tile.x);
					writer.Write(tile.z);

					if (tile.x != x || tile.z != z) continue;

					writer.Write(tile.w);
					writer.Write(tile.d);

					writer.Write(tile.tris.Length);

					for (int i = 0; i < tile.tris.Length; i++) writer.Write(tile.tris[i]);

					writer.Write(tile.verts.Length);
					for (int i = 0; i < tile.verts.Length; i++) {
						ctx.SerializeInt3(tile.verts[i]);
					}

					writer.Write(tile.vertsInGraphSpace.Length);
					for (int i = 0; i < tile.vertsInGraphSpace.Length; i++) {
						ctx.SerializeInt3(tile.vertsInGraphSpace[i]);
					}

					writer.Write(tile.nodes.Length);
					for (int i = 0; i < tile.nodes.Length; i++) {
						tile.nodes[i].SerializeNode(ctx);
					}
				}
			}
		}

		protected override void DeserializeExtraInfo (GraphSerializationContext ctx) {
			BinaryReader reader = ctx.reader;

			tileXCount = reader.ReadInt32();

			if (tileXCount < 0) return;

			tileZCount = reader.ReadInt32();
			transform = CalculateTransform();

			tiles = new NavmeshTile[tileXCount * tileZCount];

			//Make sure mesh nodes can reference this graph
			TriangleMeshNode.SetNavmeshHolder((int)ctx.graphIndex, this);

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					int tileIndex = x + z*tileXCount;
					int tx = reader.ReadInt32();
					if (tx < 0) throw new System.Exception("Invalid tile coordinates (x < 0)");

					int tz = reader.ReadInt32();
					if (tz < 0) throw new System.Exception("Invalid tile coordinates (z < 0)");

					// This is not the origin of a large tile. Refer back to that tile.
					if (tx != x || tz != z) {
						tiles[tileIndex] = tiles[tz*tileXCount + tx];
						continue;
					}

					var tile = tiles[tileIndex] = new NavmeshTile {
						x = tx,
						z = tz,
						w = reader.ReadInt32(),
						d = reader.ReadInt32(),
						bbTree = ObjectPool<BBTree>.Claim(),
						graph = this,
					};

					int trisCount = reader.ReadInt32();

					if (trisCount % 3 != 0) throw new System.Exception("Corrupt data. Triangle indices count must be divisable by 3. Read " + trisCount);

					tile.tris = new int[trisCount];
					for (int i = 0; i < tile.tris.Length; i++) tile.tris[i] = reader.ReadInt32();

					tile.verts = new Int3[reader.ReadInt32()];
					for (int i = 0; i < tile.verts.Length; i++) {
						tile.verts[i] = ctx.DeserializeInt3();
					}

					if (ctx.meta.version.Major >= 4) {
						tile.vertsInGraphSpace = new Int3[reader.ReadInt32()];
						if (tile.vertsInGraphSpace.Length != tile.verts.Length) throw new System.Exception("Corrupt data. Array lengths did not match");
						for (int i = 0; i < tile.verts.Length; i++) {
							tile.vertsInGraphSpace[i] = ctx.DeserializeInt3();
						}
					} else {
						// Compatibility
						tile.vertsInGraphSpace = new Int3[tile.verts.Length];
						tile.verts.CopyTo(tile.vertsInGraphSpace, 0);
						transform.InverseTransform(tile.vertsInGraphSpace);
					}

					int nodeCount = reader.ReadInt32();
					tile.nodes = new TriangleMeshNode[nodeCount];

					// Prepare for storing in vertex indices
					tileIndex <<= TileIndexOffset;

					for (int i = 0; i < tile.nodes.Length; i++) {
						var node = new TriangleMeshNode(active);
						tile.nodes[i] = node;

						node.DeserializeNode(ctx);

						node.v0 = tile.tris[i*3+0] | tileIndex;
						node.v1 = tile.tris[i*3+1] | tileIndex;
						node.v2 = tile.tris[i*3+2] | tileIndex;
						node.UpdatePositionFromVertices();
					}

					tile.bbTree.RebuildFrom(tile);
				}
			}
		}

		protected override void PostDeserialization (GraphSerializationContext ctx) {
			// Compatibility
			if (ctx.meta.version < AstarSerializer.V4_1_0 && tiles != null) {
				Dictionary<TriangleMeshNode, Connection[]> conns = tiles.SelectMany(s => s.nodes).ToDictionary(n => n, n => n.connections ?? new Connection[0]);
				// We need to recalculate all connections when upgrading data from earlier than 4.1.0
				// as the connections now need information about which edge was used.
				// This may remove connections for e.g off-mesh links.
				foreach (var tile in tiles) CreateNodeConnections(tile.nodes);
				foreach (var tile in tiles) ConnectTileWithNeighbours(tile);

				// Restore any connections that were contained in the serialized file but didn't get added by the method calls above
				GetNodes(node => {
					var triNode = node as TriangleMeshNode;
					foreach (var conn in conns[triNode].Where(conn => !triNode.ContainsConnection(conn.node)).ToList()) {
						triNode.AddConnection(conn.node, conn.cost, conn.shapeEdge);
					}
				});
			}

			// Make sure that the transform is up to date.
			// It is assumed that the current graph settings correspond to the correct
			// transform as it is not serialized itself.
			transform = CalculateTransform();
		}
	}
}
