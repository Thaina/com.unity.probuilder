﻿using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Mesh editing helper functions that are only available in the Editor.
	/// </summary>
	public static class EditorMeshUtility
	{
		const string k_MeshCacheDirectoryName = "ProBuilderMeshCache";
		static string k_MeshCacheDirectory = "Assets/ProBuilder Data/ProBuilderMeshCache";

		/// <value>
		/// This callback is raised after a ProBuilderMesh has been successfully optimized.
		/// </value>
		/// <seealso cref="Optimize"/>
		public static event Action<ProBuilderMesh, Mesh> meshOptimized = null;

		/// <summary>
		/// Optmizes the mesh geometry, and generates a UV2 channel (if automatic lightmap generation is enabled).
		/// </summary>
		/// <remarks>This is only applicable to meshes with triangle topology. Quad meshes are not affected by this function.</remarks>
		/// <param name="mesh">The ProBuilder mesh component to be optimized.</param>
		/// <param name="forceRebuildUV2">If the Auto UV2 preference is disabled this parameter can be used to force UV2s to be built.</param>
		public static void Optimize(this ProBuilderMesh mesh, bool forceRebuildUV2 = false)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			Mesh umesh = mesh.mesh;

			if(umesh == null || umesh.vertexCount < 1)
				return;

			bool skipMeshProcessing = false;

			// @todo Support mesh compression for topologies other than Triangles.
			for(int i = 0; !skipMeshProcessing && i < umesh.subMeshCount; i++)
				if(umesh.GetTopology(i) != MeshTopology.Triangles)
					skipMeshProcessing = true;

			bool hasUv2 = false;

			if(!skipMeshProcessing)
			{
				// if generating UV2, the process is to manually split the mesh into individual triangles,
				// generate uv2, then re-assemble with vertex collapsing where possible.
				// if not generating uv2, just collapse vertexes.
				if(!PreferencesInternal.GetBool(PreferenceKeys.pbDisableAutoUV2Generation) || forceRebuildUV2)
				{
					Vertex[] vertexes = UnityEngine.ProBuilder.MeshUtility.GeneratePerTriangleMesh(umesh);

					float time = Time.realtimeSinceStartup;

					UnwrapParam unwrap = Lightmapping.GetUnwrapParam(mesh.unwrapParameters);

					Vector2[] uv2 = Unwrapping.GeneratePerTriangleUV(umesh, unwrap);

					// If GenerateUV2() takes longer than 3 seconds (!), show a warning prompting user to disable auto-uv2 generation.
					if( (Time.realtimeSinceStartup - time) > 3f )
						Log.Warning(string.Format("Generate UV2 for \"{0}\" took {1} seconds! You may want to consider disabling Auto-UV2 generation in the `Preferences > ProBuilder` tab.", mesh.name, (Time.realtimeSinceStartup - time).ToString("F2")));

					if(uv2.Length == vertexes.Length)
					{
						for(int i = 0; i < uv2.Length; i++)
							vertexes[i].uv2 = uv2[i];

						hasUv2 = true;
					}
					else
					{
						Log.Warning("Generate UV2 failed - the returned size of UV2 array != mesh.vertexCount");
					}

					UnityEngine.ProBuilder.MeshUtility.CollapseSharedVertexes(umesh, vertexes);
				}
				else
				{
					UnityEngine.ProBuilder.MeshUtility.CollapseSharedVertexes(umesh);
				}
			}

			if(PreferencesInternal.GetBool(PreferenceKeys.pbManageLightmappingStaticFlag, false))
				Lightmapping.SetLightmapStaticFlagEnabled(mesh, hasUv2);

			if(meshOptimized != null)
				meshOptimized(mesh, umesh);

			if(PreferencesInternal.GetBool(PreferenceKeys.pbMeshesAreAssets))
				TryCacheMesh(mesh);

			UnityEditor.EditorUtility.SetDirty(mesh);
		}

		internal static void TryCacheMesh(ProBuilderMesh pb)
		{
			Mesh mesh = pb.mesh;

			// check for an existing mesh in the mesh cache and update or create a new one so
			// as not to clutter the scene yaml.
			string meshAssetPath = AssetDatabase.GetAssetPath(mesh);

			// if mesh is already an asset any changes will already have been applied since
			// pb_Object is directly modifying the mesh asset
			if(string.IsNullOrEmpty(meshAssetPath))
			{
				// at the moment the asset_guid is only used to name the mesh something unique
				string guid = pb.assetGuid;

				if(string.IsNullOrEmpty(guid))
				{
					guid = Guid.NewGuid().ToString("N");
					pb.assetGuid = guid;
				}

				string meshCacheDirectory = GetMeshCacheDirectory(true);

				string path = string.Format("{0}/{1}.asset", meshCacheDirectory, guid);

				Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);

				// a mesh already exists in the cache for this pb_Object
				if(m != null)
				{
					if(mesh != m)
					{
						// prefab instances should always point to the same mesh
						if(EditorUtility.IsPrefabInstance(pb.gameObject) || EditorUtility.IsPrefabAsset(pb.gameObject))
						{
							// Debug.Log("reconnect prefab to mesh");

							// use the most recent mesh iteration (when undoing for example)
							UnityEngine.ProBuilder.MeshUtility.CopyTo(mesh, m);

							UnityEngine.Object.DestroyImmediate(mesh);
							pb.gameObject.GetComponent<MeshFilter>().sharedMesh = m;

							// also set the MeshCollider if it exists
							MeshCollider mc = pb.gameObject.GetComponent<MeshCollider>();
							if(mc != null) mc.sharedMesh = m;
							return;
						}
						else
						{
							// duplicate mesh
							// Debug.Log("create new mesh in cache from disconnect");
							pb.assetGuid = Guid.NewGuid().ToString("N");
							path = string.Format("{0}/{1}.asset", meshCacheDirectory, pb.assetGuid);
						}
					}
					else
					{
						Debug.LogWarning("Mesh found in cache and scene mesh references match, but pb.asset_guid doesn't point to asset.  Please report the circumstances leading to this event to Karl.");
					}
				}

				AssetDatabase.CreateAsset(mesh, path);
			}
		}

		internal static bool GetCachedMesh(ProBuilderMesh pb, out string path, out Mesh mesh)
		{
			if (pb.mesh != null)
			{
				string meshPath = AssetDatabase.GetAssetPath(pb.mesh);

				if (!string.IsNullOrEmpty(meshPath))
				{
					path = meshPath;
					mesh = pb.mesh;

					return true;
				}
			}

			string meshCacheDirectory = GetMeshCacheDirectory(false);
			string guid = pb.assetGuid;

			path = string.Format("{0}/{1}.asset", meshCacheDirectory, guid);
			mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);

			return mesh != null;
		}

		static string GetMeshCacheDirectory(bool initializeIfMissing = false)
		{
			if (Directory.Exists(k_MeshCacheDirectory))
				return k_MeshCacheDirectory;

			string[] results = Directory.GetDirectories("Assets", k_MeshCacheDirectoryName, SearchOption.AllDirectories);

			if (results.Length < 1)
			{
				if (initializeIfMissing)
				{
					k_MeshCacheDirectory = FileUtility.GetLocalDataDirectory() + "/" + k_MeshCacheDirectoryName;
					Directory.CreateDirectory(k_MeshCacheDirectory);
				}
				else
				{
					k_MeshCacheDirectory = null;
				}
			}
			else
			{
				k_MeshCacheDirectory = results.First();
			}

			return k_MeshCacheDirectory;
		}
	}
}
