#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
#define UNITY_5_4_OR_LOWER
#else
#define UNITY_5_5_OR_HIGHER
#endif

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
#define UNITY_5_5_OR_LOWER
#else
#define UNITY_5_6_OR_HIGHER
#endif

using UnityEngine;
using UnityEditor;
#if !UNITY_4_7
using UnityEngine.Rendering;
#endif
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Collections;
using System.Linq;

public class pb_Preferences
{
	private static bool prefsLoaded = false;

	static Color pbDefaultFaceColor;
	static Color pbDefaultEdgeColor;
	static Color pbDefaultSelectedVertexColor;
	static Color pbDefaultVertexColor;

	static bool defaultOpenInDockableWindow;
	static Material pbDefaultMaterial;
	static Vector2 settingsScroll = Vector2.zero;
	static bool pbShowEditorNotifications;
	static bool pbForceConvex = false;
	static bool pbDragCheckLimit = false;
	static bool pbForceVertexPivot = true;
	static bool pbForceGridPivot = true;
	static bool pbPerimeterEdgeBridgeOnly;
	static bool pbPBOSelectionOnly;
	static bool pbCloseShapeWindow = false;
	static bool pbUVEditorFloating = true;
	static bool pbStripProBuilderOnBuild = true;
	static bool pbDisableAutoUV2Generation = false;
	static bool pbShowSceneInfo = false;
	static bool pbUniqueModeShortcuts = false;
	static bool pbIconGUI = false;
	static bool pbShiftOnlyTooltips = false;
	static bool pbDrawAxisLines = true;
	static bool pbMeshesAreAssets = false;
	static bool pbElementSelectIsHamFisted = false;
	static bool pbDragSelectWholeElement = false;
	static bool pbEnableExperimental = false;

	#if !UNITY_4_7
	static ShadowCastingMode pbShadowCastingMode = ShadowCastingMode.On;
	#endif

	static ColliderType defaultColliderType = ColliderType.BoxCollider;
	static SceneToolbarLocation pbToolbarLocation = SceneToolbarLocation.UpperCenter;
	static EntityType pbDefaultEntity = EntityType.Detail;

	static float pbUVGridSnapValue;
	static float pbVertexHandleSize;

	static pb_Shortcut[] defaultShortcuts;

	[PreferenceItem (pb_Constant.PRODUCT_NAME)]
	public static void PreferencesGUI ()
	{
		// Load the preferences
		if (!prefsLoaded) {
			LoadPrefs();
			prefsLoaded = true;
		}

		settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.MinHeight(180), GUILayout.MaxHeight(180));

		EditorGUI.BeginChangeCheck();

		if(GUILayout.Button("Reset All Preferences"))
			ResetToDefaults();

		/**
		 * GENERAL SETTINGS
		 */
		GUILayout.Label("General Settings", EditorStyles.boldLabel);

		pbStripProBuilderOnBuild = EditorGUILayout.Toggle(new GUIContent("Strip PB Scripts on Build", "If true, when building an executable all ProBuilder scripts will be stripped from your built product."), pbStripProBuilderOnBuild);
		pbDisableAutoUV2Generation = EditorGUILayout.Toggle(new GUIContent("Disable Auto UV2 Generation", "Disables automatic generation of UV2 channel.  If Unity is sluggish when working with large ProBuilder objects, disabling UV2 generation will improve performance.  Use `Actions/Generate UV2` or `Actions/Generate Scene UV2` to build lightmap UVs prior to baking."), pbDisableAutoUV2Generation);
		pbShowSceneInfo = EditorGUILayout.Toggle(new GUIContent("Show Scene Info", "Displays the selected object vertex and triangle counts in the scene view."), pbShowSceneInfo);
		pbShowEditorNotifications = EditorGUILayout.Toggle("Show Editor Notifications", pbShowEditorNotifications);

		/**
		 * TOOLBAR SETTINGS
		 */
		GUILayout.Label("Toolbar Settings", EditorStyles.boldLabel);

		pbIconGUI = EditorGUILayout.Toggle(new GUIContent("Use Icon GUI", "Toggles the ProBuilder window interface between text and icon versions."), pbIconGUI);
		pbShiftOnlyTooltips = EditorGUILayout.Toggle(new GUIContent("Shift Key Tooltips", "Tooltips will only show when the Shift key is held"), pbShiftOnlyTooltips);
		pbToolbarLocation = (SceneToolbarLocation) EditorGUILayout.EnumPopup("Toolbar Location", pbToolbarLocation);

		pbUniqueModeShortcuts = EditorGUILayout.Toggle(new GUIContent("Unique Mode Shortcuts", "When off, the G key toggles between Object and Element modes and H enumerates the element modes.  If on, G, H, J, and K are shortcuts to Object, Vertex, Edge, and Face modes respectively."), pbUniqueModeShortcuts);
		defaultOpenInDockableWindow = EditorGUILayout.Toggle("Open in Dockable Window", defaultOpenInDockableWindow);

		/**
		 * DEFAULT SETTINGS
		 */
		GUILayout.Label("Defaults", EditorStyles.boldLabel);

		pbDefaultMaterial = (Material) EditorGUILayout.ObjectField("Default Material", pbDefaultMaterial, typeof(Material), false);

		GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Default Entity");
			pbDefaultEntity = ((EntityType)EditorGUILayout.EnumPopup( (EntityType)pbDefaultEntity ));
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Default Collider");
			defaultColliderType = ((ColliderType)EditorGUILayout.EnumPopup( (ColliderType)defaultColliderType ));
		GUILayout.EndHorizontal();

		if((ColliderType)defaultColliderType == ColliderType.MeshCollider)
			pbForceConvex = EditorGUILayout.Toggle("Force Convex Mesh Collider", pbForceConvex);

		#if !UNITY_4_7
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Shadow Casting Mode");
		pbShadowCastingMode = (ShadowCastingMode) EditorGUILayout.EnumPopup(pbShadowCastingMode);
		GUILayout.EndHorizontal();
		#endif

		/**
		 * MISC. SETTINGS
		 */
		GUILayout.Label("Misc. Settings", EditorStyles.boldLabel);

		pbDragCheckLimit = EditorGUILayout.Toggle(new GUIContent("Limit Drag Check to Selection", "If true, when drag selecting faces, only currently selected pb-Objects will be tested for matching faces.  If false, all pb_Objects in the scene will be checked.  The latter may be slower in large scenes."), pbDragCheckLimit);
		pbPBOSelectionOnly = EditorGUILayout.Toggle(new GUIContent("Only PBO are Selectable", "If true, you will not be able to select non probuilder objects in Geometry and Texture mode"), pbPBOSelectionOnly);
		pbCloseShapeWindow = EditorGUILayout.Toggle(new GUIContent("Close shape window after building", "If true the shape window will close after hitting the build button"), pbCloseShapeWindow);
		pbDrawAxisLines = EditorGUILayout.Toggle(new GUIContent("Dimension Overlay Lines", "When the Dimensions Overlay is on, this toggle shows or hides the axis lines."), pbDrawAxisLines);

		GUILayout.Space(4);

		/**
		 * GEOMETRY EDITING SETTINGS
		 */
		GUILayout.Label("Geometry Editing Settings", EditorStyles.boldLabel);

		pbElementSelectIsHamFisted = !EditorGUILayout.Toggle(new GUIContent("Precise Element Selection", "When enabled you will be able to select object faces when in Vertex of Edge mode by clicking the center of a face.  When disabled, edge and vertex selection will always be restricted to the nearest element."), !pbElementSelectIsHamFisted);
		pbDragSelectWholeElement = EditorGUILayout.Toggle("Precise Drag Select", pbDragSelectWholeElement);
		pbDefaultFaceColor = EditorGUILayout.ColorField("Selected Face Color", pbDefaultFaceColor);
		pbDefaultEdgeColor = EditorGUILayout.ColorField("Edge Wireframe Color", pbDefaultEdgeColor);
		pbDefaultVertexColor = EditorGUILayout.ColorField("Vertex Color", pbDefaultVertexColor);
		pbDefaultSelectedVertexColor = EditorGUILayout.ColorField("Selected Vertex Color", pbDefaultSelectedVertexColor);
		pbVertexHandleSize = EditorGUILayout.Slider("Vertex Handle Size", pbVertexHandleSize, 0f, 3f);
		pbForceVertexPivot = EditorGUILayout.Toggle(new GUIContent("Force Pivot to Vertex Point", "If true, new objects will automatically have their pivot point set to a vertex instead of the center."), pbForceVertexPivot);
		pbForceGridPivot = EditorGUILayout.Toggle(new GUIContent("Force Pivot to Grid", "If true, newly instantiated pb_Objects will be snapped to the nearest point on grid.  If ProGrids is present, the snap value will be used, otherwise decimals are simply rounded to whole numbers."), pbForceGridPivot);
		pbPerimeterEdgeBridgeOnly = EditorGUILayout.Toggle(new GUIContent("Bridge Perimeter Edges Only", "If true, only edges on the perimeters of an object may be bridged.  If false, you may bridge any between any two edges you like."), pbPerimeterEdgeBridgeOnly);

		GUILayout.Space(4);

		GUILayout.Label("Experimental", EditorStyles.boldLabel);

		pbEnableExperimental = EditorGUILayout.Toggle(new GUIContent("Experimental Features", "Enables some experimental new features that we're trying out.  These may be incomplete or buggy, so please exercise caution when making use of this functionality!"), pbEnableExperimental);
		pbMeshesAreAssets = EditorGUILayout.Toggle(new GUIContent("Meshes Are Assets", "Experimental!  Instead of storing mesh data in the scene, this toggle creates a Mesh cache in the Project that ProBuilder will use."), pbMeshesAreAssets);

		GUILayout.Space(4);

		/**
		 * UV EDITOR SETTINGS
		 */
		GUILayout.Label("UV Editing Settings", EditorStyles.boldLabel);
		pbUVGridSnapValue = EditorGUILayout.FloatField("UV Snap Increment", pbUVGridSnapValue);
		pbUVGridSnapValue = Mathf.Clamp(pbUVGridSnapValue, .015625f, 2f);
		pbUVEditorFloating = EditorGUILayout.Toggle(new GUIContent("Editor window floating", "If true UV   Editor window will open as a floating window"), pbUVEditorFloating);

		EditorGUILayout.EndScrollView();

		GUILayout.Space(4);

		GUILayout.Label("Shortcut Settings", EditorStyles.boldLabel);

		ShortcutSelectPanel();
		ShortcutEditPanel();

		// Save the preferences
		if (EditorGUI.EndChangeCheck())
			SetPrefs();
	}

	public static void ResetToDefaults()
	{
		if(EditorUtility.DisplayDialog("Delete ProBuilder editor preferences?", "Are you sure you want to delete all existing ProBuilder preferences?\n\nThis action cannot be undone.", "Yes", "No"))
		{
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultFaceColor);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultEditLevel);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultSelectionMode);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbHandleAlignment);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbVertexColorTool);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbToolbarLocation);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultEntity);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultFaceColor);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultEdgeColor);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultSelectedVertexColor);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultVertexColor);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultOpenInDockableWindow);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbEditorPrefVersion);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbEditorShortcutsVersion);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultCollider);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbForceConvex);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbVertexColorPrefs);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowEditorNotifications);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDragCheckLimit);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbForceVertexPivot);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbForceGridPivot);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbManifoldEdgeExtrusion);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbPerimeterEdgeBridgeOnly);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbPBOSelectionOnly);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbCloseShapeWindow);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbUVEditorFloating);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbUVMaterialPreview);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowSceneToolbar);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbNormalizeUVsOnPlanarProjection);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbStripProBuilderOnBuild);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDisableAutoUV2Generation);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowSceneInfo);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbEnableBackfaceSelection);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbVertexPaletteDockable);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbExtrudeAsGroup);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbUniqueModeShortcuts);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbMaterialEditorFloating);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShapeWindowFloating);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbIconGUI);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShiftOnlyTooltips);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDrawAxisLines);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbCollapseVertexToFirst);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbMeshesAreAssets);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbElementSelectIsHamFisted);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDragSelectWholeElement);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbEnableExperimental);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbFillHoleSelectsEntirePath);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDetachToNewObject);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbPreserveFaces);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbVertexHandleSize);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbUVGridSnapValue);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbUVWeldDistance);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbWeldDistance);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbExtrudeDistance);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbBevelAmount);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbEdgeSubdivisions);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultShortcuts);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbDefaultMaterial);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbGrowSelectionUsingAngle);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbGrowSelectionAngle);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbGrowSelectionAngleIterative);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowDetail);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowOccluder);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowMover);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowCollider);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowTrigger);
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShowNoDraw);
			#if !UNITY_4_7
			pb_Preferences_Internal.DeleteKey(pb_Constant.pbShadowCastingMode);
			#endif
		}

		LoadPrefs();
	}

	static int shortcutIndex = 0;

#if UNITY_5_6_OR_HIGHER
	static Rect selectBox = new Rect(0, 214, 183, 156);
	static Rect shortcutEditRect = new Rect(190, 191, 178, 300);
#else
	static Rect selectBox = new Rect(130, 253, 183, 142);
	static Rect shortcutEditRect = new Rect(320, 228, 178, 300);
#endif

	static Vector2 shortcutScroll = Vector2.zero;
	static int CELL_HEIGHT = 20;

	static void ShortcutSelectPanel()
	{
		GUILayout.Space(4);
		GUI.contentColor = Color.white;
		GUI.Box(selectBox, "");

		GUIStyle labelStyle = GUIStyle.none;

		if(EditorGUIUtility.isProSkin)
			labelStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);

		labelStyle.alignment = TextAnchor.MiddleLeft;
		labelStyle.contentOffset = new Vector2(4f, 0f);

		shortcutScroll = EditorGUILayout.BeginScrollView(shortcutScroll, false, true, GUILayout.MaxWidth(183), GUILayout.MaxHeight(156));

		for(int n = 1; n < defaultShortcuts.Length; n++)
		{
			if(n == shortcutIndex)
			{
				GUI.backgroundColor = new Color(0.23f, .49f, .89f, 1f);
					labelStyle.normal.background = EditorGUIUtility.whiteTexture;
					Color oc = labelStyle.normal.textColor;
					labelStyle.normal.textColor = Color.white;
					GUILayout.Box(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT));
					labelStyle.normal.background = null;
					labelStyle.normal.textColor = oc;
				GUI.backgroundColor = Color.white;
			}
			else
			{

				if(GUILayout.Button(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT)))
				{
					shortcutIndex = n;
				}
			}
		}

		EditorGUILayout.EndScrollView();

	}

	static void ShortcutEditPanel()
	{
		GUILayout.BeginArea(shortcutEditRect);

			// descriptionTitleRect = EditorGUI.RectField(new Rect(240,150,200,50), descriptionTitleRect);
			GUILayout.Label("Key", EditorStyles.boldLabel);
			KeyCode key = defaultShortcuts[shortcutIndex].key;
			key = (KeyCode) EditorGUILayout.EnumPopup(key);
			defaultShortcuts[shortcutIndex].key = key;

			GUILayout.Label("Modifiers", EditorStyles.boldLabel);
			// EnumMaskField returns a bit-mask where the flags correspond to the indices of the enum, not the enum values,
			// so this isn't technically correct.
			EventModifiers em = (EventModifiers) (((int)defaultShortcuts[shortcutIndex].eventModifiers) * 2);
			em = (EventModifiers)EditorGUILayout.EnumMaskField(em);
			defaultShortcuts[shortcutIndex].eventModifiers = (EventModifiers) (((int)em) / 2);
			GUILayout.Label("Description", EditorStyles.boldLabel);

			GUILayout.Label(defaultShortcuts[shortcutIndex].description, EditorStyles.wordWrappedLabel);

		GUILayout.EndArea();
	}

	static void LoadPrefs()
	{
		pbStripProBuilderOnBuild 			= pb_Preferences_Internal.GetBool(pb_Constant.pbStripProBuilderOnBuild);
		pbDisableAutoUV2Generation 			= pb_Preferences_Internal.GetBool(pb_Constant.pbDisableAutoUV2Generation);
		pbShowSceneInfo 					= pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneInfo);
		defaultOpenInDockableWindow 		= pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
		pbDragCheckLimit 					= pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit);
		pbForceConvex 						= pb_Preferences_Internal.GetBool(pb_Constant.pbForceConvex);
		pbForceGridPivot 					= pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot);
		pbForceVertexPivot 					= pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot);
		pbPerimeterEdgeBridgeOnly 			= pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);
		pbPBOSelectionOnly 					= pb_Preferences_Internal.GetBool(pb_Constant.pbPBOSelectionOnly);
		pbCloseShapeWindow 					= pb_Preferences_Internal.GetBool(pb_Constant.pbCloseShapeWindow);
		pbUVEditorFloating 					= pb_Preferences_Internal.GetBool(pb_Constant.pbUVEditorFloating);
		// pbShowSceneToolbar 					= pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneToolbar);
		pbShowEditorNotifications 			= pb_Preferences_Internal.GetBool(pb_Constant.pbShowEditorNotifications);
		pbUniqueModeShortcuts 				= pb_Preferences_Internal.GetBool(pb_Constant.pbUniqueModeShortcuts);
		pbIconGUI 							= pb_Preferences_Internal.GetBool(pb_Constant.pbIconGUI);
		pbShiftOnlyTooltips 				= pb_Preferences_Internal.GetBool(pb_Constant.pbShiftOnlyTooltips);
		pbDrawAxisLines 					= pb_Preferences_Internal.GetBool(pb_Constant.pbDrawAxisLines);
		pbMeshesAreAssets 					= pb_Preferences_Internal.GetBool(pb_Constant.pbMeshesAreAssets);
		pbElementSelectIsHamFisted			= pb_Preferences_Internal.GetBool(pb_Constant.pbElementSelectIsHamFisted);
		pbDragSelectWholeElement			= pb_Preferences_Internal.GetBool(pb_Constant.pbDragSelectWholeElement);
		pbEnableExperimental				= pb_Preferences_Internal.GetBool(pb_Constant.pbEnableExperimental);


		pbDefaultFaceColor 					= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultFaceColor );
		pbDefaultEdgeColor 					= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultEdgeColor );
		pbDefaultSelectedVertexColor 		= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultSelectedVertexColor );
		pbDefaultVertexColor 				= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultVertexColor );

		pbUVGridSnapValue 					= pb_Preferences_Internal.GetFloat(pb_Constant.pbUVGridSnapValue);
		pbVertexHandleSize 					= pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);

		defaultColliderType 				= pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider);
		pbToolbarLocation	 				= pb_Preferences_Internal.GetEnum<SceneToolbarLocation>(pb_Constant.pbToolbarLocation);
		pbDefaultEntity	 					= pb_Preferences_Internal.GetEnum<EntityType>(pb_Constant.pbDefaultEntity);
		#if !UNITY_4_7
		pbShadowCastingMode					= pb_Preferences_Internal.GetEnum<ShadowCastingMode>(pb_Constant.pbShadowCastingMode);
		#endif

		pbDefaultMaterial 					= pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial);

		defaultShortcuts 					= pb_Preferences_Internal.GetShortcuts().ToArray();

	}

	public static void SetPrefs()
	{
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbStripProBuilderOnBuild, pbStripProBuilderOnBuild);
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbDisableAutoUV2Generation, pbDisableAutoUV2Generation);
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbShowSceneInfo, pbShowSceneInfo);

		pb_Preferences_Internal.SetInt		(pb_Constant.pbToolbarLocation, (int) pbToolbarLocation, pb_PreferenceLocation.Global);
		pb_Preferences_Internal.SetInt		(pb_Constant.pbDefaultEntity, (int)pbDefaultEntity);

		pb_Preferences_Internal.SetString	(pb_Constant.pbDefaultFaceColor, pbDefaultFaceColor.ToString(), pb_PreferenceLocation.Global);
		pb_Preferences_Internal.SetString	(pb_Constant.pbDefaultEdgeColor, pbDefaultEdgeColor.ToString());
		pb_Preferences_Internal.SetString	(pb_Constant.pbDefaultSelectedVertexColor, pbDefaultSelectedVertexColor.ToString());
		pb_Preferences_Internal.SetString	(pb_Constant.pbDefaultVertexColor, pbDefaultVertexColor.ToString());
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbDefaultOpenInDockableWindow, defaultOpenInDockableWindow);
		pb_Preferences_Internal.SetString	(pb_Constant.pbDefaultShortcuts, pb_Shortcut.ShortcutsToString(defaultShortcuts));

		string matPath = pbDefaultMaterial != null ? AssetDatabase.GetAssetPath(pbDefaultMaterial) : "";
		pb_Preferences_Internal.SetString	(pb_Constant.pbDefaultMaterial, matPath);

		pb_Preferences_Internal.SetInt 		(pb_Constant.pbDefaultCollider, (int) defaultColliderType);
		#if !UNITY_4_7
		pb_Preferences_Internal.SetInt 		(pb_Constant.pbShadowCastingMode, (int) pbShadowCastingMode);
		#endif
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbShowEditorNotifications, pbShowEditorNotifications);
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbForceConvex, pbForceConvex);
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbDragCheckLimit, pbDragCheckLimit);
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbForceVertexPivot, pbForceVertexPivot);
		pb_Preferences_Internal.SetBool  	(pb_Constant.pbForceGridPivot, pbForceGridPivot);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbPerimeterEdgeBridgeOnly, pbPerimeterEdgeBridgeOnly);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbPBOSelectionOnly, pbPBOSelectionOnly);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbCloseShapeWindow, pbCloseShapeWindow);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbUVEditorFloating, pbUVEditorFloating);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbUniqueModeShortcuts, pbUniqueModeShortcuts);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbIconGUI, pbIconGUI);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbShiftOnlyTooltips, pbShiftOnlyTooltips);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbDrawAxisLines, pbDrawAxisLines);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbMeshesAreAssets, pbMeshesAreAssets);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbElementSelectIsHamFisted, pbElementSelectIsHamFisted);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbDragSelectWholeElement, pbDragSelectWholeElement);
		pb_Preferences_Internal.SetBool		(pb_Constant.pbEnableExperimental, pbEnableExperimental);

		pb_Preferences_Internal.SetFloat	(pb_Constant.pbVertexHandleSize, pbVertexHandleSize);
		pb_Preferences_Internal.SetFloat 	(pb_Constant.pbUVGridSnapValue, pbUVGridSnapValue);

		if(pb_Editor.instance != null)
			pb_Editor.instance.OnEnable();

		SceneView.RepaintAll();
	}
}
