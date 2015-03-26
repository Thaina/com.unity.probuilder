﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using System.Linq;

namespace ProBuilder2.EditorCommon
{

	/**
	 * When building the project, remove all references to pb_Objects.
	 */
	public class pb_ScenePostProcessor
	{
		[PostProcessScene]
		public static void OnPostprocessScene()
		{ 
			Material invisibleFaceMaterial = (Material)Resources.Load("Materials/InvisibleFace");

			/**
			 * Hide nodraw faces if present.
			 */
			foreach(pb_Object pb in GameObject.FindObjectsOfType(typeof(pb_Object)))
			{
				if( pb.GetComponent<MeshRenderer>().sharedMaterials.Any(x => x.name.Contains("NoDraw")) )
				{
					Material[] mats = pb.GetComponent<MeshRenderer>().sharedMaterials;

					for(int i = 0; i < mats.Length; i++)
					{
						if(mats[i].name.Contains("NoDraw"))	
							mats[i] = invisibleFaceMaterial;
					}

					pb.GetComponent<MeshRenderer>().sharedMaterials = mats;
				}
			}

			if(EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			foreach(pb_Object pb in GameObject.FindObjectsOfType(typeof(pb_Object)))
			{
				GameObject go = pb.gameObject;

				pb_Entity entity = pb.gameObject.GetComponent<pb_Entity>();

				if(entity.entityType == EntityType.Collider || entity.entityType == EntityType.Trigger)	
					go.GetComponent<MeshRenderer>().enabled = false;

				if(!pb_Preferences_Internal.GetBool(pb_Constant.pbStripProBuilderOnBuild))
				   return;

				pb.dontDestroyMeshOnDelete = true;
				
				GameObject.DestroyImmediate( pb );
				GameObject.DestroyImmediate( go.GetComponent<pb_Entity>() );

			}
		}
	}
}