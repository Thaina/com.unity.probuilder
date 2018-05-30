using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEditor.ProBuilder.UI;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class NewPolyShape : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/NewPolyShape", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Poly Shape"; } }
		public override int toolbarPriority { get { return 1; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"New Polygon Shape",
			"Creates a new shape by clicking around a perimeter and extruding."
		);

		public override bool IsHidden()
		{
			return false;
		}

		public override bool IsEnabled()
		{
			return true;
		}

		public override ActionResult DoAction()
		{
			GameObject go = new GameObject();
			PolyShape poly = go.AddComponent<PolyShape>();
			ProBuilderMesh pb = poly.gameObject.AddComponent<ProBuilderMesh>();
			pb.CreateShapeFromPolygon(poly.m_Points, poly.extrude, poly.flipNormals);
			var mat = PreferencesInternal.GetMaterial(PreferenceKeys.pbDefaultMaterial);
			foreach (var face in pb.facesInternal)
				face.material = mat;
			EditorUtility.InitObject(pb);
			MeshSelection.SetSelection(go);
			UndoUtility.RegisterCreatedObjectUndo(go, "Create Poly Shape");
			poly.polyEditMode = PolyShape.PolyEditMode.Path;

			Vector3 pivot;

			if(ProGridsInterface.GetPivot(out pivot))
				go.transform.position = pivot;

			return new ActionResult(ActionResult.Status.Success, "Create Poly Shape");
		}
	}
}
