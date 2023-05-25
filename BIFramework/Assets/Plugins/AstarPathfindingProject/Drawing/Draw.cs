// This file is automatically generated by a script based on the CommandBuilder API
using Unity.Burst;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Pathfinding.Drawing {
	/// <summary>
	/// Methods for easily drawing things in the editor and in standalone games.
	///
	/// See: getstarted (view in online documentation for working links)
	/// </summary>
	public static class Draw {
		internal static CommandBuilder builder;
		internal static CommandBuilder ingame_builder;


		/// <summary>\copydocref{CommandBuilder.xy}</summary>
		public static CommandBuilder2D xy {
			get {
				DrawingManager.Init();
				return new CommandBuilder2D(builder, true);
			}
		}

		/// <summary>\copydocref{CommandBuilder.xz}</summary>
		public static CommandBuilder2D xz {
			get {
				DrawingManager.Init();
				return new CommandBuilder2D(builder, false);
			}
		}


#if UNITY_EDITOR
		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WithMatrix(Matrix4x4)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static CommandBuilder.ScopeMatrix WithMatrix (Matrix4x4 matrix) {
			DrawingManager.Init();
			return builder.WithMatrix(matrix);
		}
#else
		[BurstDiscard]
		public static CommandBuilder.ScopeEmpty WithMatrix (Matrix4x4 matrix) {
			// Do nothing in standlone builds
			return new CommandBuilder.ScopeEmpty();
		}
#endif


#if UNITY_EDITOR
		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WithColor(Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static CommandBuilder.ScopeColor WithColor (Color color) {
			DrawingManager.Init();
			return builder.WithColor(color);
		}
#else
		[BurstDiscard]
		public static CommandBuilder.ScopeEmpty WithColor (Color color) {
			// Do nothing in standlone builds
			return new CommandBuilder.ScopeEmpty();
		}
#endif


#if UNITY_EDITOR
#else
#endif


#if UNITY_EDITOR
#else
#endif


#if UNITY_EDITOR
#else
#endif


#if UNITY_EDITOR
#else
#endif




























		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (float3 a, float3 b) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Line(a, b);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(Vector3,Vector3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (Vector3 a, Vector3 b) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Line(a, b);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(Vector3,Vector3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (Vector3 a, Vector3 b, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Line(a, b, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (float3 origin, float3 direction) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Ray(origin, direction);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(Ray,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (Ray ray, float length) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Ray(ray, length);
#endif
		}



		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXZ(float3,float,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public static void CircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CircleXZ(center, radius, startAngle, endAngle);
#endif
		}



		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Circle(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Circle (float3 center, float3 normal, float radius) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Circle(center, normal, radius);
#endif
		}











		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 bottom, float3 top, float radius) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCylinder(bottom, top, radius);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 position, float3 up, float height, float radius) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCylinder(position, up, height, radius);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 start, float3 end, float radius) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCapsule(start, end, radius);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 position, float3 direction, float length, float radius) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCapsule(position, direction, length, radius);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireSphere(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireSphere (float3 position, float radius) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireSphere(position, radius);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(List&lt;Vector3&gt;,bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (List<Vector3> points, bool cycle = false) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(Vector3[],bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (Vector3[] points, bool cycle = false) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(float3[],bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (float3[] points, bool cycle = false) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(NativeArray&lt;float3&gt;,bool)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (NativeArray<float3> points, bool cycle = false) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, float3 size) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireBox(center, size);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,quaternion,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, quaternion rotation, float3 size) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireBox(center, rotation, size);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(Bounds)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (Bounds bounds) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireBox(bounds);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireMesh(Mesh)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireMesh (Mesh mesh) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireMesh(mesh);
#endif
		}







		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Cross(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Cross (float3 position, float size = 1) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Cross(position, size);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXZ(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public static void CrossXZ (float3 position, float size = 1) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CrossXZ(position, size);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXY(float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public static void CrossXY (float3 position, float size = 1) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CrossXY(position, size);
#endif
		}







		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Arrow(from, to);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to, float3 up, float headSize) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Arrow(from, to, up, headSize);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::ArrowRelativeSizeHead(float3,float3,float3,float)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.ArrowRelativeSizeHead(from, to, up, headFraction);
#endif
		}







		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireGrid(float3,quaternion,int2,float2)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireGrid (float3 center, quaternion rotation, int2 cells, float2 totalSize) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireGrid(center, rotation, cells, totalSize);
#endif
		}































		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, float3 size) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.SolidBox(center, size);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(Bounds)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (Bounds bounds) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.SolidBox(bounds);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,quaternion,float3)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, quaternion rotation, float3 size) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.SolidBox(center, rotation, size);
#endif
		}









































		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Line(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Line (float3 a, float3 b, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Line(a, b, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (float3 origin, float3 direction, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Ray(origin, direction, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Ray(Ray,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Ray (Ray ray, float length, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Ray(ray, length, color);
#endif
		}



		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXZ(float3,float,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public static void CircleXZ (float3 center, float radius, float startAngle, float endAngle, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CircleXZ(center, radius, startAngle, endAngle, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CircleXZ(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Circle instead")]
		public static void CircleXZ (float3 center, float radius, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CircleXZ(center, radius, color);
#endif
		}





		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Circle(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Circle (float3 center, float3 normal, float radius, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Circle(center, normal, radius, color);
#endif
		}















		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 bottom, float3 top, float radius, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCylinder(bottom, top, radius, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCylinder(float3,float3,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCylinder (float3 position, float3 up, float height, float radius, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCylinder(position, up, height, radius, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 start, float3 end, float radius, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCapsule(start, end, radius, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireCapsule(float3,float3,float,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireCapsule (float3 position, float3 direction, float length, float radius, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireCapsule(position, direction, length, radius, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireSphere(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireSphere (float3 position, float radius, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireSphere(position, radius, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(List&lt;Vector3&gt;,bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (List<Vector3> points, bool cycle, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(List&lt;Vector3&gt;,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (List<Vector3> points, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(Vector3[],bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (Vector3[] points, bool cycle, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(Vector3[],Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (Vector3[] points, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(float3[],bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (float3[] points, bool cycle, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(float3[],Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (float3[] points, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(NativeArray&lt;float3&gt;,bool,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (NativeArray<float3> points, bool cycle, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, cycle, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Polyline(NativeArray&lt;float3&gt;,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Polyline (NativeArray<float3> points, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Polyline(points, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, float3 size, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireBox(center, size, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(float3,quaternion,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (float3 center, quaternion rotation, float3 size, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireBox(center, rotation, size, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireBox(Bounds,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireBox (Bounds bounds, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireBox(bounds, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireMesh(Mesh,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireMesh (Mesh mesh, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireMesh(mesh, color);
#endif
		}



		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Cross(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Cross (float3 position, float size, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Cross(position, size, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Cross(float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Cross (float3 position, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Cross(position, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXZ(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public static void CrossXZ (float3 position, float size, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CrossXZ(position, size, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXZ(float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xz.Cross instead")]
		public static void CrossXZ (float3 position, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CrossXZ(position, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXY(float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public static void CrossXY (float3 position, float size, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CrossXY(position, size, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::CrossXY(float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		[System.Obsolete("Use Draw.xy.Cross instead")]
		public static void CrossXY (float3 position, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.CrossXY(position, color);
#endif
		}







		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Arrow(from, to, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::Arrow(float3,float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void Arrow (float3 from, float3 to, float3 up, float headSize, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.Arrow(from, to, up, headSize, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::ArrowRelativeSizeHead(float3,float3,float3,float,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void ArrowRelativeSizeHead (float3 from, float3 to, float3 up, float headFraction, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.ArrowRelativeSizeHead(from, to, up, headFraction, color);
#endif
		}









		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::WireGrid(float3,quaternion,int2,float2,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void WireGrid (float3 center, quaternion rotation, int2 cells, float2 totalSize, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.WireGrid(center, rotation, cells, totalSize, color);
#endif
		}































		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, float3 size, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.SolidBox(center, size, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(Bounds,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (Bounds bounds, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.SolidBox(bounds, color);
#endif
		}

		/// <summary>
		/// \copydocref{Drawing::CommandBuilder::SolidBox(float3,quaternion,float3,Color)}
		/// Warning: This method cannot be used inside of Burst jobs. See job-system (view in online documentation for working links) instead.
		/// </summary>
		[BurstDiscard]
		public static void SolidBox (float3 center, quaternion rotation, float3 size, Color color) {
#if UNITY_EDITOR
			DrawingManager.Init();
			builder.SolidBox(center, rotation, size, color);
#endif
		}
	}
}
