using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

public class TilesetEditor : EditorWindow {
	public enum TileTool {
		None,
		Draw,
		Erase,
		Box,
		Rotate90,
		Flip,
		FloodFill,
		Collisions
	};

	static int sheetWidth {
		get {
			return Mathf.RoundToInt( (float)mat.mainTexture.width / (tileSize + spacing) );
		}
	}

	static int sheetHeight {
		get {
			return Mathf.RoundToInt( (float)mat.mainTexture.height / (tileSize + spacing) );
		}
	}

	static int tileSize = 16;
	static int spacing = 0;
	[SerializeField]
	static Material mat;
	static Vector2 fullSheetScroll;
	static Vector2 sideControlsScroll;
	Vector2 mousePos;
	bool mouseDownScene = false;
	Vector2 lastPosScene;
	Vector2 lastPosWindow;
	public static Tile[,] selectedTiles = {{ new Tile( 0, 0 ) }};
	static GameObject layerToEdit;
	static TileInfo layerTileInfo;
	public static TileTool toolSelected;
	static float zoomScale = 1f;
	public static int autoTileSelected = -1;
	public static Vector2 posToCreateMesh;

	static Texture _autoTileGuide;
	List<int> autoTileY = new List<int>();
	
	static public Texture autoTileGuide {
		get {
			if( _autoTileGuide == null )
				_autoTileGuide = Resources.Load<Texture>( "Tileguide" );
			return _autoTileGuide;
		}
	}

	static Texture _gridIcon;
	static public Texture gridIcon {
		get {
			if( _gridIcon == null )
				_gridIcon = Resources.Load<Texture>( "grid" );
			return _gridIcon;
		}
	}
	static GUIContent[] _toolIcons;

	public static GUIContent[] toolIcons {
		get {
			if( _toolIcons == null ) {
				_toolIcons = new GUIContent[]
				{
					new GUIContent("X", "None"),
					EditorGUIUtility.IconContent ("TerrainInspector.TerrainToolSplat", "Draw Tile" ),
					new GUIContent(Resources.Load<Texture>( "Eraser" ), "Erase Tile" ),
					new GUIContent(Resources.Load<Texture>( "Box" ), "Box Tool" ),
					EditorGUIUtility.IconContent ("RotateTool", "Rotate Tile"),
					new GUIContent(Resources.Load<Texture>( "Flip" ), "Flip Tile" ),
					new GUIContent(Resources.Load<Texture>( "floodfill" ), "Fill Area" ),
					new GUIContent(Resources.Load<Texture>( "Collisions" ), "Set up collisions" ),
				};
			}
			return _toolIcons;
		}
	}	

	[MenuItem ("Window/Tileset Editor")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		TilesetEditor window = (TilesetEditor)EditorWindow.GetWindow (typeof (TilesetEditor));
		window.Show();




	}

	void OnUndoRedo () {
		if( layerTileInfo != null ) {
			layerTileInfo.mapHasChanged = true;
			layerTileInfo.UpdateVisualMesh();
		}
		Repaint();
	}
	
	void OnFocus () {
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		
		Undo.undoRedoPerformed -= this.OnUndoRedo;
		Undo.undoRedoPerformed += this.OnUndoRedo;
	}
	
	void OnDestroy () {
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		Undo.undoRedoPerformed -= this.OnUndoRedo;
	}

	void OnEnable() {
		PropertyInfo propertyInfo = typeof(EditorWindow).GetProperty("cachedTitleContent", BindingFlags.Instance | BindingFlags.NonPublic );
		if (propertyInfo == null) return;
		
		GUIContent content = propertyInfo.GetValue( this, null ) as GUIContent;
		
		if (content != null) {
			
			if (content.image != gridIcon) content.image = gridIcon;
			if (content.text != "Tileset Editor" ) content.text = "Tileset Editor";
		}

		tileSize = EditorPrefs.GetInt( "2DTilezoneTileSize", 16 );
		spacing = EditorPrefs.GetInt( "2DTilezoneSpacing", 0 );
		toolSelected = (TileTool)EditorPrefs.GetInt( "2DTilezoneToolSelected", 0 );
		layerToEdit = GameObject.Find( EditorPrefs.GetString( "2DTilezoneLayerName" ) );
		mat = AssetDatabase.LoadAssetAtPath<Material>( EditorPrefs.GetString( "2DTilzoneMatPath" ) );
		if( layerToEdit == null )
			return;
		layerTileInfo = layerToEdit.GetComponent<TileInfo>();
		if( layerTileInfo == null )
			return;
		if( mat == null || mat.mainTexture == null && layerToEdit.GetComponent<Renderer>() != null )
			mat = layerToEdit.GetComponent<Renderer>().sharedMaterial;
		EditorUtility.SetSelectedWireframeHidden( layerToEdit.GetComponent<Renderer>(), true );

	}

	void OnDisable() {
		EditorPrefs.SetInt( "2DTilezoneTileSize", tileSize );
		EditorPrefs.SetInt( "2DTilezoneSpacing", spacing );
		EditorPrefs.SetInt( "2DTilezoneToolSelected", (int)toolSelected );
		if(layerToEdit != null )
			EditorPrefs.SetString( "2DTilezoneLayerName", layerToEdit.name );
		else
			EditorPrefs.SetString( "2DTilezoneLayerName", "" );

		if(mat != null )
			EditorPrefs.SetString( "2DTilzoneMatPath", AssetDatabase.GetAssetPath( mat ) );
		else
			EditorPrefs.SetString( "2DTilzoneMatPath", "" );
	}

	void OnSelectionChange () {
		if( Selection.activeGameObject != null ) {
			TileInfo ti = Selection.activeGameObject.GetComponent<TileInfo>();
			if( ti != null ) {
				layerToEdit = ti.gameObject;
				layerTileInfo = ti;
				tileSize = ti.tileSize;
				spacing = ti.spacing;
				mat = ti.GetComponent<Renderer>().sharedMaterial;
				EditorUtility.SetSelectedWireframeHidden( layerToEdit.GetComponent<Renderer>(), true );
				Repaint();
			}
		}
	}
	
	public static void CreateMesh ( Rect pos, string goName, bool addRandomDungeon ) {
		GameObject result;

		result = new GameObject( goName );
		result.transform.position = new Vector3( pos.x - pos.width / 2, pos.y - pos.height / 2, 0 );
		result.AddComponent<MeshFilter>().sharedMesh = new Mesh();
		result.AddComponent<MeshRenderer>().material = mat;
		TileInfo ti = result.AddComponent<TileInfo>();
		ti.collisions = new TileInfo.CollisionType[(sheetWidth) * (sheetHeight)];
		for( int i = 0; i < (sheetWidth) * (sheetHeight); i++ ) {
			ti.collisions[i] = TileInfo.CollisionType.Full;
		}
		ti.tileSize = tileSize;
		ti.spacing = spacing;
		//result.GetComponent<TileInfo>().positionAtLastEdit = pos;
		ti.mapWidth = (int)pos.width;
		ti.mapHeight = (int)pos.height;
		ti.tiles = new Tile[(int)pos.width*(int)pos.height];
		for( int i = 0; i < ti.tiles.Length; i++ ) {
			ti.tiles[i] = Tile.empty;
		}

		if( layerTileInfo != null ) {
			ti.collisions = new TileInfo.CollisionType[layerTileInfo.collisions.Length];
			for( int i = 0; i < layerTileInfo.collisions.Length; i++ ) {
				ti.collisions[i] = layerTileInfo.collisions[i];
			}
		}

		if( addRandomDungeon ) {
			RandomDungeon rd = result.AddComponent<RandomDungeon>();
			rd.width = (int)pos.width;
			rd.height = (int)pos.height;
		}
		Undo.RegisterCreatedObjectUndo( result, "Create Object" );
		layerToEdit = result;
		layerTileInfo = ti;
		EditorUtility.SetSelectedWireframeHidden( layerToEdit.GetComponent<Renderer>(), true );
		//return ti;
	}
	
	public void DrawBox( Vector2 position ) {
		Handles.DrawLine( new Vector3( position.x, position.y, 0 ), new Vector3( position.x + 1, position.y, 0 ) );
		Handles.DrawLine( new Vector3( position.x + 1, position.y, 0 ), new Vector3( position.x + 1, position.y + 1, 0 ) );
		Handles.DrawLine( new Vector3( position.x + 1, position.y + 1, 0 ), new Vector3( position.x, position.y + 1, 0 ) );
		Handles.DrawLine( new Vector3( position.x, position.y + 1, 0 ), new Vector3( position.x, position.y, 0 ) );
	}
	
	public void DrawBox( Vector2 bottomLeft, Vector2 topRight ) {
		Handles.DrawLine( new Vector3( bottomLeft.x, bottomLeft.y, 0 ), new Vector3( topRight.x, bottomLeft.y, 0 ) );
		Handles.DrawLine( new Vector3( topRight.x, bottomLeft.y, 0 ), new Vector3( topRight.x, topRight.y, 0 ) );
		Handles.DrawLine( new Vector3( topRight.x, topRight.y, 0 ), new Vector3( bottomLeft.x, topRight.y, 0 ) );
		Handles.DrawLine( new Vector3( bottomLeft.x, topRight.y, 0 ), new Vector3( bottomLeft.x, bottomLeft.y, 0 ) );
	}

	public void DrawTriangle( Rect p, Color c, bool right ) {
		for( int y = 0; y < p.height; y++ ) {
			if( right )
				EditorGUI.DrawRect( new Rect( p.x + p.width - y, y + p.y, y, 1 ), c );
			else
				EditorGUI.DrawRect( new Rect( p.x, y + p.y, y, 1 ), c );
		}
	}

	void DrawGrid () {
		for( float x = tileSize * zoomScale; x < mat.mainTexture.width * zoomScale; x += (tileSize + spacing) * zoomScale ) {
			EditorGUI.DrawRect( new Rect( x, 0, Mathf.Max( spacing, 1 ), mat.mainTexture.height * zoomScale ), Color.white );
		}
		
		for( float y = tileSize * zoomScale; y < mat.mainTexture.height * zoomScale; y += (tileSize + spacing) * zoomScale ) {
			EditorGUI.DrawRect( new Rect( 0, y, mat.mainTexture.width * zoomScale, Mathf.Max( spacing, 1 ) ), Color.white );
		}
	}

	void DrawSceneGrid () {
		if( layerTileInfo == null )
			return;

		Handles.color = new Color( 1, 1, 1, 0.25f );

		for( int x = 0; x <= layerTileInfo.mapWidth; x++ ) {
			Vector3 p1 = layerTileInfo.transform.position;
			p1.x += x;

			Vector3 p2 = p1;
			p2.y += layerTileInfo.mapHeight;

			Handles.DrawLine( p1, p2 );
		}

		for( int y = 0; y <= layerTileInfo.mapHeight; y++ ) {
			Vector3 p1 = layerTileInfo.transform.position;
			p1.y += y;
			
			Vector3 p2 = p1;
			p2.x += layerTileInfo.mapWidth;
			
			Handles.DrawLine( p1, p2 );
		}

		Handles.color = new Color( 1, 1, 1, 0.75f );
	}
	
	bool CheckForGridSnap () {
		if( layerTileInfo == null )
			return true;

		if( layerTileInfo.tileSize != tileSize || layerTileInfo.spacing != spacing ) {
			if( EditorUtility.DisplayDialog( "Create new layer?",
			                                "The tile size selected or spacing is different than what is on the selected layer. To continue with the selected tile size create a new layer.",
			                                "Create New Layer",
			                                "Cancel" ) ) {
				posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
				CreateTileLayer.Init();
			}
			return false;
		}

		if(
			layerToEdit.transform.position.x == Mathf.FloorToInt( layerToEdit.transform.position.x )
			&& layerToEdit.transform.position.y == Mathf.FloorToInt( layerToEdit.transform.position.y )
			&& layerToEdit.transform.rotation == Quaternion.identity 
			) {

			return true;
		}
		string title = "Align " + layerToEdit + " to grid?";
		string message = "To continue editing " + layerToEdit + " needs to be snapped to the grid.";
		if( EditorUtility.DisplayDialog( title, message, "Snap to grid", "Cancel" ) ) {
			Undo.RecordObject( layerToEdit.transform, "Snap to grid" );
			Vector3 snappedPosition = layerToEdit.transform.position;
			snappedPosition.x = Mathf.Round( snappedPosition.x );
			snappedPosition.y = Mathf.Round( snappedPosition.y );
			layerToEdit.transform.position = snappedPosition;
			layerToEdit.transform.rotation = Quaternion.identity;
		}
		mouseDownScene = false;
		return false;

	}
	
	void SetCursor ( Rect pos, bool shift = false ) {
		switch( toolSelected ) {
		case TileTool.Box:
		case TileTool.Draw:
			if( shift )
				EditorGUIUtility.AddCursorRect( pos, MouseCursor.Text );
			else
				EditorGUIUtility.AddCursorRect( pos, MouseCursor.ArrowPlus );
			break;
		case TileTool.Erase:
			EditorGUIUtility.AddCursorRect( pos, MouseCursor.ArrowMinus );
			break;
		case TileTool.Rotate90:
			EditorGUIUtility.AddCursorRect( pos, MouseCursor.RotateArrow );
			break;
		case TileTool.Flip:
			EditorGUIUtility.AddCursorRect( pos, MouseCursor.SlideArrow );
			break;
		case TileTool.FloodFill:
			EditorGUIUtility.AddCursorRect( pos, MouseCursor.MoveArrow );
			break;
		}
	}

	void AddTile( Vector2 pos, Tile selectedTile ) {
		int index = layerTileInfo.WorldPointToMapIndex( pos );
		if( index == -1 )
			return;
		if( layerTileInfo.tiles[index] == selectedTile && layerTileInfo.tiles[index].autoTileIndex == autoTileSelected )
			return;

		if( selectedTile.autoTileIndex >= layerTileInfo.numberOfAutotiles )
			selectedTile.autoTileIndex = -1;

		Undo.RecordObject( layerTileInfo, "Add Tile" );
		if( autoTileSelected == -1 )
			layerTileInfo.AddTile( pos, selectedTile );
		else
			layerTileInfo.AddTile( pos, autoTileSelected );
	}

	void FloodFill ( Vector2 pos ) {
		if( layerTileInfo.WorldPointToMapIndex(pos) == -1 )
			return;
		Undo.RecordObject( layerTileInfo, "Flood Fill" );
		if( autoTileSelected == -1 )
			layerTileInfo.FloodFill( pos, selectedTiles, pos, new Tile( layerTileInfo.tiles[layerTileInfo.WorldPointToMapIndex(pos)] ) );
		else
			layerTileInfo.FloodFill( pos, autoTileSelected, pos, new Tile( layerTileInfo.tiles[layerTileInfo.WorldPointToMapIndex(pos)] ) );
	}

	void RemoveTile ( Vector2 pos ) {
		Undo.RecordObject( layerTileInfo, "Remove Tile" );
		layerTileInfo.RemoveTile( pos );
	}

	void RotateTile ( Vector2 pos ) {
		Undo.RecordObject( layerTileInfo, "Rotate Tile" );
		layerTileInfo.RotateTile( pos );
	}

	void FlipTile ( Vector2 pos ) {
		Undo.RecordObject( layerTileInfo, "Flip Tile" );
		layerTileInfo.FlipTile( pos );
	}
	
	public void OnSceneGUI ( SceneView sceneView ) {

		Event current = Event.current;
		Vector2 mousePos = Event.current.mousePosition;
		
		SetCursor(sceneView.position, current.shift);
		
		if( toolSelected != TileTool.None && toolSelected != TileTool.Collisions ) {

			DrawSceneGrid();

			// This prevents us from selecting other objects in the scene
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(controlID);
			
			mousePos.y = sceneView.camera.pixelHeight - mousePos.y;
			mousePos = (Vector2)sceneView.camera.ScreenPointToRay(mousePos).origin;
			mousePos.x = Mathf.Floor( mousePos.x );
			mousePos.y = Mathf.Floor( mousePos.y );
			
			if( mouseDownScene && lastPosScene != mousePos && (toolSelected != TileTool.Draw || current.shift) ) {
				Vector2 bottomLeft = lastPosScene;
				Vector2 topRight = mousePos;
				if( mousePos.x < lastPosScene.x ) {
					bottomLeft.x = mousePos.x;
					topRight.x = lastPosScene.x;
				}
				if( mousePos.y < lastPosScene.y ) {
					bottomLeft.y = mousePos.y;
					topRight.y = lastPosScene.y;
				}
				for( int x = (int)bottomLeft.x; x <= (int)topRight.x; x++ ) {
					for( int y = (int)bottomLeft.y; y <= (int)topRight.y; y++ ) {
						DrawBox( new Vector2(x,y) );
					}
				}
			}
			else if( selectedTiles.Length > 0 ) {
				for( int x = 0; x < selectedTiles.GetLength( 0 ); x++ ) {
					for( int y = 0; y < selectedTiles.GetLength( 1 ); y++ ) {
						DrawBox( new Vector2( mousePos.x + x, mousePos.y - y ) );
						if( (toolSelected != TileTool.Box && toolSelected != TileTool.FloodFill && toolSelected != TileTool.Draw ) || autoTileSelected != -1 )
							break;
					}
					if( (toolSelected != TileTool.Box && toolSelected != TileTool.FloodFill && toolSelected != TileTool.Draw ) || autoTileSelected != -1  )
						break;
				}
			}
			else {
				Debug.Log( "Scene Error" );
				selectedTiles = new Tile[1,1]{{ new Tile( 0, 0 ) }};
			}
			
		}

		if( mouseDownScene && toolSelected == TileTool.Draw && !current.shift && CheckForGridSnap() ) {
			if( layerToEdit == null || layerTileInfo == null ) {
				if( EditorUtility.DisplayDialog( "Create new layer?", "There is no layer selected. Would you like to create a new one?", "Create new layer", "Cancel" ) ) {
					posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
					//layerTileInfo = layerToEdit.GetComponent<TileInfo>();
					CreateTileLayer.Init();
				}
				mouseDownScene = false;
				return;
			}
			else {
				if( autoTileSelected == -1 ) {
					for( int x = 0; x < selectedTiles.GetLength(0); x++ ) {
						for( int y = 0; y < selectedTiles.GetLength(1); y++ ) {
							int xx = TileInfo.Mod( x + (int)mousePos.x - (int)lastPosScene.x, selectedTiles.GetLength(0) );
							int yy = TileInfo.Mod( y + (int)lastPosScene.y - (int)mousePos.y, selectedTiles.GetLength(1) );
							AddTile( new Vector2( x + mousePos.x, mousePos.y - y ), selectedTiles[xx, yy] );
						}
					}
				}
				else
					AddTile( mousePos, layerTileInfo.autoTileData[48 * autoTileSelected] );
			}
			layerTileInfo.UpdateVisualMesh();
		}
		
		if( toolSelected != TileTool.None && toolSelected != TileTool.Collisions && current.isMouse && current.button == 0 ) {
			
			if( current.type == EventType.MouseDown ) {
				if( layerTileInfo != null && autoTileSelected >= layerTileInfo.numberOfAutotiles )
					autoTileSelected = -1;

				mouseDownScene = true;
				lastPosScene = mousePos;
			}
			
			else if( current.type == EventType.MouseUp && mouseDownScene ) {
				mouseDownScene = false;
				if( CheckForGridSnap () ) {
					if( (toolSelected == TileTool.Box || toolSelected == TileTool.FloodFill || toolSelected == TileTool.Draw) && layerToEdit == null ) {
						if( mat != null && mat.mainTexture != null ) {
							if( EditorUtility.DisplayDialog( "Create new layer?", "There is no layer selected. Would you like to create a new one?", "Create new layer", "Cancel" ) ) {
								posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
								//layerTileInfo = layerToEdit.GetComponent<TileInfo>();
								CreateTileLayer.Init();
							}
							else {
								return;
							}
						}
					}
					if( layerToEdit == null )
						return;

					if( (toolSelected != TileTool.None && toolSelected != TileTool.Collisions && toolSelected != TileTool.Erase) && ( layerTileInfo.WorldPointToMapIndex( mousePos ) == -1 ||
					   layerTileInfo.WorldPointToMapIndex( lastPosScene ) == -1 ) ) {
						if( EditorUtility.DisplayDialog( "Resize map bounds?",
						                                "Resize the map bounds to include the tile clicked?",
						                                "Resize Map Bounds",
						                                "Cancel" ) ) {
							Undo.RecordObjects( new UnityEngine.Object[2]{ layerTileInfo, layerTileInfo.transform }, "Resize Bounds" );
							layerTileInfo.ResizeBoundsToFitWorldPos( mousePos );
						}
					}

					Vector2 originalTilePos = lastPosScene;
					if( mousePos.x < lastPosScene.x ) {
						lastPosScene.x = mousePos.x;
						mousePos.x = originalTilePos.x;
					}
					if( mousePos.y < lastPosScene.y ) {
						lastPosScene.y = mousePos.y;
						mousePos.y = originalTilePos.y;
					}
					if( current.shift ) {
						selectedTiles = new Tile[(int)(mousePos.x - lastPosScene.x) + 1, (int)(mousePos.y - lastPosScene.y) + 1];
						autoTileSelected = -1;
					}
					for( int x = (int)lastPosScene.x; x <= (int)mousePos.x; x++ ) {
						for( int y = (int)lastPosScene.y; y <= (int)mousePos.y; y++ ) {
							switch( toolSelected ) {
							case TileTool.Box:
							case TileTool.FloodFill:
							case TileTool.Draw:
								if( current.shift ) {
									selectedTiles[x - (int)lastPosScene.x, (int)mousePos.y - y] = layerTileInfo.tiles[layerTileInfo.WorldPointToMapIndex(x,y)];
									break;
								}
								//if no drag was performed stamp the whole selection
								if( lastPosScene == mousePos && selectedTiles.Length > 1 && autoTileSelected == -1 && toolSelected == TileTool.Box ) {
									for( int xx = 0; xx < selectedTiles.GetLength(0); xx++ ) {
										for( int yy = 0; yy < selectedTiles.GetLength(1); yy++ ) {
											AddTile( new Vector2( x + xx, y - yy ), selectedTiles[xx, yy % selectedTiles.GetLength(1)] );
										}
									}
								}
								else {
									if( toolSelected == TileTool.Box )
										AddTile( new Vector2( x, y ), selectedTiles[(x - (int)lastPosScene.x) % selectedTiles.GetLength(0),((int)mousePos.y - y) % selectedTiles.GetLength(1)] );
									else if( toolSelected == TileTool.FloodFill )
										FloodFill( new Vector2( x, y ) );
								}
								
								break;
							case TileTool.Erase:
								RemoveTile( new Vector2( x, y ) );
								break;
							case TileTool.Rotate90:
								RotateTile ( new Vector2( x, y ) );
								break;
							case TileTool.Flip:
								FlipTile( new Vector2( x, y ) );
								break;
							}
						}
					}
					layerTileInfo.UpdateVisualMesh();
				}
			}
			
			current.Use();
			Repaint();
		}
	}
	
	public void OnGUI() {
		GUILayout.BeginHorizontal();
		DrawTileSheet();
		DrawSideControls();
		GUILayout.EndHorizontal();
	}

	void DrawTileSheet () {
		fullSheetScroll = GUILayout.BeginScrollView( fullSheetScroll );
		
		if (mat != null && mat.mainTexture != null)
		{
			GUILayout.Label(
				string.Empty, GUILayout.Width(mat.mainTexture.width * zoomScale), GUILayout.Height(mat.mainTexture.height * zoomScale));
			
			GUI.DrawTexture(
				new Rect(0, 0, mat.mainTexture.width * zoomScale, mat.mainTexture.height * zoomScale),
				mat.mainTexture,
				ScaleMode.StretchToFill,
				true,
				1);
			DrawGrid();
			HandleSelection();
			
		}
		
		GUILayout.EndScrollView();
	}
	
	void DrawSelectedTiles () {
		if( mat == null || mat.mainTexture == null )
			return;
		
		GUILayout.Label(
			string.Empty, GUILayout.Height(100));

		if( autoTileSelected == -1 ) {
			float displaySize = 150 / Mathf.Max( selectedTiles.GetLength(0), selectedTiles.GetLength(1) );
			
			for( int x = 0; x < selectedTiles.GetLength(0); x++ ) {
				for( int y = 0; y < selectedTiles.GetLength(1); y++ ) {
					Rect texCoords = new Rect( (float)selectedTiles[x,y].xIndex * (tileSize + spacing) / (float)mat.mainTexture.width,
					                          1 - (((float)selectedTiles[x,y].yIndex + 1)  * (tileSize + spacing) - spacing) / mat.mainTexture.height,
					                          (tileSize + spacing) / (float)mat.mainTexture.width,
					                          (tileSize + spacing) / (float)mat.mainTexture.height );
					Vector2 pivot = new Vector2( 50 + (displaySize/2) + (x*displaySize), 100 + (displaySize/2) + (y*displaySize) );
					if( selectedTiles[x,y].flip ) {
						texCoords.x += texCoords.width;
						texCoords.width *= -1;
					}
					EditorGUIUtility.RotateAroundPivot(selectedTiles[x,y].rotation * 90 , pivot );
					GUI.DrawTextureWithTexCoords(
						new Rect( 50 + x * displaySize, 100 + y * displaySize, displaySize, displaySize ),
						mat.mainTexture,
						texCoords
						);
					GUI.matrix = Matrix4x4.identity;
				}
			}
		}
		else
			GUI.Label( new Rect( 50, 250, 150, 50 ), "Auto Tile Selected" );
	}

	void SaveCollisionData () {
		if( mat == null || mat.mainTexture == null )
			return;
		string path = EditorUtility.SaveFilePanel( "Save collision data", "Assets/2DTilezoneFiles/CollisionData", mat.mainTexture.name + "_collision_data", "cd" );
		if( path != string.Empty ) {
			XmlSerializer xmls = new XmlSerializer(typeof(CollisionData));
			CollisionData collisionData = new CollisionData();
			collisionData.collisions = layerTileInfo.collisions;

			StringWriter sw = new StringWriter();
			xmls.Serialize( sw, (object)collisionData );
			string xml = sw.ToString();
			File.WriteAllText( path, xml );
		}
	}

	void LoadCollisionData () {
		if( mat == null || mat.mainTexture == null )
			return;

		string path = EditorUtility.OpenFilePanel( "Load collision data", "Assets/2DTilezoneFiles/CollisionData", "cd" );

		if( path != string.Empty ) {
			XmlSerializer xmls = new XmlSerializer(typeof(CollisionData));
			CollisionData collisionData = xmls.Deserialize( new StringReader(File.ReadAllText( path ) ) ) as CollisionData;
			
			if( collisionData.collisions.Length == layerTileInfo.collisions.Length ) {
				layerTileInfo.collisions = collisionData.collisions;
			}
			else
				EditorUtility.DisplayDialog( "Incorect Texture Size",
				                            "Can not load as the collision data you attempted to load is from a texture of a different size.",
				                            "OK" );
		}
	}

	void SaveAutoTile ( int index ) {
		if( mat == null || mat.mainTexture == null )
			return;

		string path = EditorUtility.SaveFilePanel( "Save auto tile data", "Assets/2DTilezoneFiles/AutoTileFiles", mat.mainTexture.name + "_" + layerTileInfo.autoTileNames[index], "atd" );
		if( path != string.Empty ) {
			XmlSerializer xmls = new XmlSerializer(typeof(AutoTile));
			AutoTile autoTile = new AutoTile();
			for( int i = 48 * index; i < 48 * index + 48; i++ ) {
				autoTile.autoTileData.Add( layerTileInfo.autoTileData[i] );
			}
			autoTile.autoTileName = layerTileInfo.autoTileNames[index];
			autoTile.textureWidth = mat.mainTexture.width;
			autoTile.textureHeight = mat.mainTexture.height;
			
			StringWriter sw = new StringWriter();
			xmls.Serialize(sw, (object)autoTile);
			string xml = sw.ToString();
			File.WriteAllText( path, xml );
		}

	}

	void LoadAutoTile ( int index ) {
		if( mat == null || mat.mainTexture == null )
			return;
		string path = EditorUtility.OpenFilePanel( "Load auto tile data", "Assets/2DTilezoneFiles/AutoTileFiles", "atd" );
		if( path != string.Empty ) {
			XmlSerializer xmls = new XmlSerializer(typeof(AutoTile));
			AutoTile autoTile = xmls.Deserialize( new StringReader(File.ReadAllText( path ) ) ) as AutoTile;

			if( mat.mainTexture.width == autoTile.textureWidth && mat.mainTexture.height == autoTile.textureHeight ) {
				for( int i = 0; i < 48; i++ ) {
					layerTileInfo.autoTileData[i + 48 * index] = autoTile.autoTileData[i];
				}
				layerTileInfo.autoTileNames[index] = autoTile.autoTileName;
			}
			else
				EditorUtility.DisplayDialog( "Incorect Texture Size",
				                            "Can not load as the auto tile you attempted to load is from a texture of a different size.",
				                            "OK" );
		}


	}
	
	void DrawAutoTiles() {
		if( layerTileInfo == null )
			return;

		if( mat == null )
			return;

		if( mat.mainTexture == null )
			return;
		EditorGUI.BeginChangeCheck();
		layerTileInfo.numberOfAutotiles = EditorGUILayout.IntField( "Number of Auto Tiles:", layerTileInfo.numberOfAutotiles );
		if( EditorGUI.EndChangeCheck() ) {
			//add empty tile data if it doesnt exsist
			while( layerTileInfo.autoTileData.Count < 48 * layerTileInfo.numberOfAutotiles ) {
				layerTileInfo.autoTileData.Add( Tile.empty );
			}
			while( layerTileInfo.autoTileNames.Count < layerTileInfo.numberOfAutotiles ) {
				layerTileInfo.autoTileNames.Add( "Auto Tile " + (layerTileInfo.autoTileNames.Count + 1).ToString() );
				layerTileInfo.autoTileLinkMask.Add( 1 << (int)Mathf.Pow( 2, layerTileInfo.autoTileLinkMask.Count-1 ) );
				layerTileInfo.showAutoTile.Add( true );
				layerTileInfo.autoTileEdgeMode.Add(TileInfo.AutoTileEdgeMode.None);
			}
		}
		//kept seperate for version compatability
		while( layerTileInfo.autoTileType.Count < layerTileInfo.numberOfAutotiles )
			layerTileInfo.autoTileType.Add( TileInfo.AutoTileType.Normal );

		
		for( int i = 0; i < layerTileInfo.numberOfAutotiles; i++ ) {
			layerTileInfo.showAutoTile[i] = EditorGUILayout.Foldout( layerTileInfo.showAutoTile[i], layerTileInfo.autoTileNames[i] );
			if( layerTileInfo.showAutoTile[i] ) {
				while( autoTileY.Count <= i )
					autoTileY.Add( 0 );
				autoTileY[i] = (int)EditorGUILayout.BeginVertical().y;
				layerTileInfo.autoTileNames[i] = EditorGUILayout.TextField( layerTileInfo.autoTileNames[i] );
				GUILayout.Label( string.Empty, GUILayout.Height(250) );
				SetCursor( new Rect( 50, autoTileY[i] + 30, 150, 200 ) );
				layerTileInfo.autoTileLinkMask[i] = EditorGUILayout.MaskField( "Link With", layerTileInfo.autoTileLinkMask[i], layerTileInfo.autoTileNames.ToArray() );
				layerTileInfo.autoTileEdgeMode[i] = (TileInfo.AutoTileEdgeMode)EditorGUILayout.EnumPopup( "Map edge link mode", layerTileInfo.autoTileEdgeMode[i] );
				layerTileInfo.autoTileType[i] = (TileInfo.AutoTileType)EditorGUILayout.EnumPopup( "Auto Tile Type", layerTileInfo.autoTileType[i] );
				EditorGUILayout.BeginHorizontal();
				if( GUILayout.Button( "Save" ) ) { SaveAutoTile(i); }
				if( GUILayout.Button( "Load" ) ) { LoadAutoTile(i); }
				EditorGUILayout.EndHorizontal();
				GUI.DrawTexture( new Rect( 50, autoTileY[i] + 30, 150, 200 ), autoTileGuide, ScaleMode.StretchToFill );

				bool isSelected = (autoTileSelected == i);
				isSelected = EditorGUI.Toggle( new Rect( 20, autoTileY[i] + 100, 20, 20 ), isSelected );
				if( isSelected )
					autoTileSelected = i;

				if( !GUI.RepeatButton( new Rect( 0, autoTileY[i] + 130, 40, 20 ), "Hide" ) ) {

					for( int a = 0; a < 48; a++ ) {
						if( layerTileInfo.autoTileData.Count > a + i * 48 && layerTileInfo.autoTileData[a + i * 48] != Tile.empty ) {
							Rect texCoords = new Rect( (float)layerTileInfo.autoTileData[a + i * 48].xIndex * (tileSize + spacing) / (float)mat.mainTexture.width,
							                          1 - (((float)layerTileInfo.autoTileData[a + i * 48].yIndex + 1)  * (tileSize + spacing) - spacing) / mat.mainTexture.height,
							                          tileSize / (float)mat.mainTexture.width,
							                          tileSize / (float)mat.mainTexture.height );
							//flip tile
							if( layerTileInfo.autoTileData[a+i*48].flip ) {
								texCoords.x += texCoords.width;
								texCoords.width *= -1;
							}
							
							Vector2 pivot = new Vector2( 50 + (a % 6) * 25, autoTileY[i] + 30 + (a / 6) * 25 );
							pivot.x += 12.5f;
							pivot.y += 12.5f;
							
							EditorGUIUtility.RotateAroundPivot(layerTileInfo.autoTileData[a + i * 48].rotation * 90 , pivot );
							GUI.DrawTextureWithTexCoords(
								new Rect( 50 + (a % 6) * 25, autoTileY[i] + 30 + (a / 6) * 25  , 25, 25 ),
								mat.mainTexture,
								texCoords
								);
							GUI.matrix = Matrix4x4.identity;
						}
					} 
				}
				EditorGUILayout.EndVertical();
			}
		}
		EditorGUILayout.Space(  );
	}
	
	void DrawSideControls () {
		GUILayout.BeginVertical(GUILayout.Width(250));
		//toolSelected = (TileTool)GUILayout.SelectionGrid( (int)toolSelected,  Enum.GetNames( typeof(TileTool) ), 3, EditorStyles.radioButton );
		EditorGUI.BeginChangeCheck();
		toolSelected = (TileTool)GUILayout.Toolbar( (int)toolSelected, toolIcons, new GUILayoutOption[0] );
		if( EditorGUI.EndChangeCheck() ) {
			Repaint();
			SceneView.lastActiveSceneView.Repaint();
		}
		string s = " Hold the shift key to select tiles in the Scene View.";
		switch( toolSelected ) {
		case TileTool.None:
			EditorGUILayout.HelpBox( "No tool is selected. To start editing select a tool.", MessageType.Info );
			break;
		case TileTool.Box:
			EditorGUILayout.HelpBox( "Box Tool. Click and drag to draw a box." + s, MessageType.Info );
			break;
		case TileTool.Erase:
			EditorGUILayout.HelpBox( "Erase Tool. Click and drag to erase.", MessageType.Info );
			break;
		case TileTool.Collisions:
			EditorGUILayout.HelpBox( "Collisions. Edit the collision data in the window to the left.", MessageType.Info );
			break;
		case TileTool.Rotate90:
			EditorGUILayout.HelpBox( "Rotate Tool. Click on a tile to rotate it.", MessageType.Info );
			break;
		case TileTool.Flip:
			EditorGUILayout.HelpBox( "Flip Tool. Click on a tile to flip it", MessageType.Info );
			break;
		case TileTool.FloodFill:
			EditorGUILayout.HelpBox( "Fill Tool. Click on a tile to fill in an area" + s, MessageType.Info );
			break;
		case TileTool.Draw:
			EditorGUILayout.HelpBox( "Paint Brush Tool. Click on a tile to paint a tile" + s, MessageType.Info );
			break;
		}

		//EditorGUILayout.LabelField( "Layer to edit." );
		EditorGUI.BeginChangeCheck();
		layerTileInfo = EditorGUILayout.ObjectField( "Layer to edit.", layerTileInfo, typeof(TileInfo), true ) as TileInfo;
		if( EditorGUI.EndChangeCheck() ) {
			if( layerTileInfo != null )
				layerToEdit = layerTileInfo.gameObject;
			else
				layerToEdit = null;
			//GameObject layerToEdit = GameObject.Find( layerName );
			if( layerToEdit != null && layerToEdit.GetComponent<TileInfo>() != null ) {
				TileInfo tile = layerToEdit.GetComponent<TileInfo>();
				tileSize = tile.tileSize;
				spacing = tile.spacing;
				mat = tile.GetComponent<Renderer>().sharedMaterial;
				layerTileInfo = layerToEdit.GetComponent<TileInfo>();
				EditorUtility.SetSelectedWireframeHidden( layerToEdit.GetComponent<Renderer>(), true );
				Repaint();
				SceneView.lastActiveSceneView.Repaint();
			}
		}
		
		if( GUILayout.Button( "Create New Layer" ) ) {
			if( mat != null && mat.mainTexture != null ) {
				posToCreateMesh = (Vector2)SceneView.lastActiveSceneView.camera.transform.position;
				CreateTileLayer.Init();
			}
		}

		if( toolSelected == TileTool.Collisions ) {
			EditorGUILayout.Space();
			if( GUILayout.Button( "Update Colliders" ) ) {
				if( layerTileInfo != null )
					layerTileInfo.UpdateColliders();
			}

			if( GUILayout.Button( "Save Collision Data" ) ) {
				if( layerTileInfo != null )
					SaveCollisionData();
			}

			if( GUILayout.Button( "Load Collision Data" ) ) {
				if( layerTileInfo != null )
					LoadCollisionData();
			}
		}
		else {
			sideControlsScroll = GUILayout.BeginScrollView( sideControlsScroll );
			tileSize = EditorGUILayout.IntField( "Tile Size", tileSize );
			tileSize = Mathf.Max( 4, tileSize );
			spacing = EditorGUILayout.IntField( "Spacing", spacing );
			spacing = Math.Max( 0, spacing );
			mat = (Material)EditorGUILayout.ObjectField("Material", mat, typeof(Material), true );
			EditorGUILayout.Space();


			EditorGUILayout.Space();
			
			if( GUILayout.Button( "Update Colliders" ) ) {
				if( layerTileInfo != null )
					layerTileInfo.UpdateColliders();
			}

			GUILayout.Label( "Zoom" );
			zoomScale = GUILayout.VerticalSlider( zoomScale, 5f, 0.5f, GUILayout.MaxHeight(200) );

			DrawSelectedTiles();

			if( layerTileInfo != null ) {
				DrawAutoTiles();

				HandleAutoTileSelection();

				EditorGUILayout.Space();
				EditorGUILayout.HelpBox( "The below features require the textures read write flag to be enabled in the texture import settings.", MessageType.Info );
				if( GUILayout.Button( "Export to PNG" ) ) {
					ExportToPng();
				}
				layerTileInfo.update3DWalls = EditorGUILayout.Toggle( "Update 3D walls", layerTileInfo.update3DWalls );
			}
			
			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();
	}
	
	void HandleAutoTileSelection () {
		Event current = Event.current;
		if( current.isMouse && current.button == 0 && current.type == EventType.MouseUp ) {
			if( layerToEdit == null )
				return;

			Undo.RecordObject( layerTileInfo, "Change AutoTile" );

			mousePos = current.mousePosition;
			int autoTileClicked = -1;
			for( int i = 0; i < layerTileInfo.numberOfAutotiles; i++ ) {
				if( !layerTileInfo.showAutoTile[i] )
					continue;

				Vector2 tileClicked = current.mousePosition;
				tileClicked.x -= 50;
				tileClicked.y -= autoTileY[i] + 30;
				tileClicked /= 25;
				tileClicked.x = Mathf.Floor( tileClicked.x );
				tileClicked.y = Mathf.Floor( tileClicked.y );
				if( tileClicked.x >= 0 && tileClicked.x < 6 && tileClicked.y >= 0 && tileClicked.y < 8 ) {
					autoTileClicked = (int)tileClicked.x + 6 * (int)tileClicked.y;
					autoTileClicked += 48 * i;

					switch( toolSelected ) {
					case TileTool.Flip:
						//flip
						if( layerTileInfo.autoTileData[autoTileClicked] != Tile.empty )
							layerTileInfo.autoTileData[autoTileClicked].flip = !layerTileInfo.autoTileData[autoTileClicked].flip;
						break;
					case TileTool.Rotate90:
						//rotate
						if( layerTileInfo.autoTileData[autoTileClicked] != Tile.empty ) {
							layerTileInfo.autoTileData[autoTileClicked].rotation--;
							if( layerTileInfo.autoTileData[autoTileClicked].rotation < 0 )
								layerTileInfo.autoTileData[autoTileClicked].rotation += 4;
						}
						break;
					case TileTool.Erase:
						layerTileInfo.autoTileData[autoTileClicked] = Tile.empty;
						break;
					default:
						for( int x= 0; x < selectedTiles.GetLength(0); x++ ) {
							for( int y= 0; y < selectedTiles.GetLength(1); y++ ) {

								if( ((autoTileClicked - 48*i) % 6) + x >= 6 )
									break;
								if( ((autoTileClicked - 48*i) / 6) + y >= 8 )
									break;

								int t = (((autoTileClicked - 48*i) / 6) + y) * 6 + (((autoTileClicked - 48*i) % 6) + x);
								t += i * 48;
								if( layerTileInfo.autoTileData[t] == selectedTiles[x,y] && selectedTiles.Length == 1 )
									layerTileInfo.autoTileData[t] = Tile.empty;
								else
									layerTileInfo.autoTileData[t] = new Tile( selectedTiles[x,y] );
							}
						}
						
						break;
					}
				}
			}
			Repaint();
		}
	}
	
	void HandleSelection () {
		Event current = Event.current;
		if( current.isMouse ) {
			mousePos = current.mousePosition;
			
			mousePos.x = Mathf.Floor( mousePos.x / (( tileSize + spacing ) * zoomScale) );
			mousePos.y = Mathf.Floor( mousePos.y / (( tileSize + spacing ) * zoomScale) );
			
			if( current.type == EventType.MouseDown ) {
				lastPosWindow = mousePos;
			}
			if( current.type == EventType.MouseUp ) {
				mouseDownScene = false;
				Vector2 bottomLeft = lastPosWindow;
				bottomLeft.y = mousePos.y;
				Vector2 topRight = mousePos;
				topRight.y = lastPosWindow.y;
				if( mousePos.x < lastPosWindow.x ) {
					bottomLeft.x = mousePos.x;
					topRight.x = lastPosWindow.x;
				}
				if( mousePos.y < lastPosWindow.y ) {
					bottomLeft.y = lastPosWindow.y;
					topRight.y = mousePos.y;
				}


				if( toolSelected != TileTool.Collisions ) {
					selectedTiles = new Tile[( (int)topRight.x - (int)bottomLeft.x ) + 1, ( (int)bottomLeft.y - (int)topRight.y ) + 1];
					autoTileSelected = -1;
				}
				for( int x = 0; x <= topRight.x - bottomLeft.x; x++ ) {
					for( int y = 0; y <= bottomLeft.y - topRight.y; y++ ) {

						if( bottomLeft .x + x >= sheetWidth 
						   || topRight.y + y >= sheetHeight )
							continue;
						
						if( toolSelected == TileTool.Collisions ) {
							if( layerToEdit != null && layerToEdit.GetComponent<TileInfo>() != null ) {
								Undo.RecordObject( layerTileInfo, "Change collision data" );
								if( layerToEdit.GetComponent<TileInfo>().collisions.Length == (sheetWidth) * (sheetHeight) ) {
									layerToEdit.GetComponent<TileInfo>().collisions[((int)topRight.y+y) * (sheetWidth) + (int)bottomLeft.x + x]++;
									if( (int)layerToEdit.GetComponent<TileInfo>().collisions[((int)topRight.y+y) * (sheetWidth) + (int)bottomLeft.x + x] > 4 )
										layerToEdit.GetComponent<TileInfo>().collisions[((int)topRight.y+y) * (sheetWidth) + (int)bottomLeft.x + x] = TileInfo.CollisionType.None;
								}
								else {
									layerToEdit.GetComponent<TileInfo>().collisions = new TileInfo.CollisionType[(sheetWidth) * (sheetHeight)];
									for( int i = 0; i < (sheetWidth) * (sheetHeight); i++ ) {
										layerToEdit.GetComponent<TileInfo>().collisions[i] = TileInfo.CollisionType.Full;
									}
								}
							}
						}
						else {
							selectedTiles[x, y] = new Tile( (int)bottomLeft.x + x, (int)topRight.y + y );
						}
					}
				}
				this.Repaint();
			}
			
			
		}
		
		if( toolSelected == TileTool.Collisions ) {
			if( layerToEdit != null && layerToEdit.GetComponent<TileInfo>() != null 
			   && layerToEdit.GetComponent<TileInfo>().collisions.Length == (sheetWidth) * (sheetHeight) ) {
				for( int x = 0; x < sheetWidth; x++ ) {
					for( int y = 0; y < sheetHeight; y++ ) {
						if( layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.Full )
							EditorGUI.DrawRect( new Rect( x * (tileSize + spacing) * zoomScale, y * (tileSize + spacing) * zoomScale, tileSize * zoomScale, tileSize * zoomScale ), new Color( 0, 0, 1, 0.3f ) );
						if( layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.SlopeUpRight )
							DrawTriangle( new Rect( x * (tileSize + spacing) * zoomScale, y * (tileSize + spacing) * zoomScale, tileSize * zoomScale, tileSize * zoomScale ), new Color( 0, 0, 1, 0.3f ), true );
						if( layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.SlopeUpLeft )
							DrawTriangle( new Rect( x * (tileSize + spacing) * zoomScale, y * (tileSize + spacing) * zoomScale, tileSize * zoomScale, tileSize * zoomScale ), new Color( 0, 0, 1, 0.3f ), false );
						if( layerToEdit.GetComponent<TileInfo>().collisions[y * (sheetWidth) + x] == TileInfo.CollisionType.Platform )
							EditorGUI.DrawRect( new Rect( x * (tileSize + spacing) * zoomScale, y * (tileSize + spacing) * zoomScale, tileSize * zoomScale, (tileSize * zoomScale)/2 ), new Color( 0, 0, 1, 0.3f ) );
					}
				}
			}
		}
		else if( selectedTiles.Length > 0 && selectedTiles[0,0] != null ) {
			if( autoTileSelected == -1 ) {
				for( int x = 0; x < selectedTiles.GetLength( 0 ); x++ ) {
					for( int y = 0; y < selectedTiles.GetLength( 1 ); y++ ) {
						if( selectedTiles[x,y] != Tile.empty && selectedTiles[x,y] != null )
							EditorGUI.DrawRect( new Rect( selectedTiles[x,y].xIndex * (tileSize + spacing) * zoomScale, selectedTiles[x,y].yIndex * (tileSize + spacing) * zoomScale, tileSize * zoomScale, tileSize * zoomScale ), new Color( 1, 0, 0, 0.3f ) );
					}
				}
			}
		}
		else {
			selectedTiles = new Tile[1,1]{{ new Tile( 0, 0 ) }};
		}
		
	}

	void ExportToPng () {
		if( layerTileInfo == null || layerTileInfo.numberOfTiles == 0 ) {
			EditorUtility.DisplayDialog( "Export To Sprite", "Either you have no layer selected or the selected layer has no tiles.", "OK" );
			return;
		}
		tileSize = layerTileInfo.tileSize;
		spacing = layerTileInfo.spacing;
		
		Texture2D mapSprite = new Texture2D( (tileSize + spacing) * layerTileInfo.mapWidth, (tileSize + spacing) * layerTileInfo.mapHeight );
		Color[] allPixels = mapSprite.GetPixels();
		for( int i = 0; i < allPixels.Length; i++ ) {
			allPixels[i] = Color.clear;
		}
		mapSprite.SetPixels( allPixels );
		Texture2D tileTex = (Texture2D)layerToEdit.GetComponent<Renderer>().sharedMaterial.mainTexture;
		for( int x = 0; x < layerTileInfo.mapWidth; x++ ) {
			for( int y = 0; y < layerTileInfo.mapHeight; y++ ) {

				int i = y * layerTileInfo.mapWidth + x;
				if( layerTileInfo.tiles[i] == Tile.empty )
					continue;
				Color[] thisTile = tileTex.GetPixels( layerTileInfo.tiles[i].xIndex * (tileSize + spacing),
				                                     tileTex.height - (((int)layerTileInfo.tiles[i].yIndex + 1) * (tileSize + spacing)) + spacing,
				                                     tileSize, tileSize );
				Color[] modTile = new Color[thisTile.Length];

				//flip and rotate the color array
				if( layerTileInfo.tiles[i].flip ) {
					for( int xx = 0; xx < tileSize; xx++ ) {
						for( int yy = 0; yy < tileSize; yy++ ) {
							modTile[ yy * tileSize + xx ] = thisTile[ yy * tileSize + (tileSize - 1 - xx) ];
						}
					}
					modTile.CopyTo( thisTile, 0 );
				}

				if( layerTileInfo.tiles[i].rotation == 1 || layerTileInfo.tiles[i].rotation == 3 ) {
					for( int xx = 0; xx < tileSize; xx++ ) {
						for( int yy = 0; yy < tileSize; yy++ ) {
							if ( layerTileInfo.tiles[i].rotation == 3 )
								modTile[ yy * tileSize + xx ] = thisTile[ (tileSize-1-xx) * tileSize + yy ];
							if ( layerTileInfo.tiles[i].rotation == 1 )
								modTile[ yy * tileSize + xx ] = thisTile[ xx * tileSize + (tileSize-1-yy) ];
						}
					}
					thisTile = modTile;
				}

				if( layerTileInfo.tiles[i].rotation == 2 ) {
					for( int ii = 0; ii < thisTile.Length; ii++ ) {
						modTile[ii] = thisTile[thisTile.Length - 1 - ii];
					}
					thisTile = modTile;
				}

				mapSprite.SetPixels( x * tileSize, y * tileSize, tileSize, tileSize, thisTile );
			}
		}

		mapSprite.Apply();
		
		string path = EditorUtility.SaveFilePanelInProject( "Save as PNG",
		                                                   layerTileInfo.name + ".png",
		                                                   "png",
		                                                   "Please enter a file name to save the map to" );

		if( path != string.Empty ) {
			byte[] pngData = mapSprite.EncodeToPNG();
			if( pngData != null ) {

				File.WriteAllBytes( path, pngData );

				AssetDatabase.Refresh();
				TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath( path );
				textureImporter.spritePixelsPerUnit = tileSize;
				AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate );
			}
		}
	}
	
//	void UpdateColliders () {
//		//GameObject layerToEdit = GameObject.Find( layerName );
//		if( layerToEdit != null && layerToEdit.GetComponent<TileInfo>() != null ) {
//			GameObject tempObj = new GameObject();
//			SpriteRenderer ren = tempObj.AddComponent<SpriteRenderer>();
//			Vector2 size = (Vector2)layerToEdit.GetComponent<MeshFilter>().sharedMesh.bounds.size;
//			
//			Texture2D colliderImage = new Texture2D( (int)size.x * tileSize, (int)size.y * tileSize, TextureFormat.Alpha8, false );
//			Color[] allPixels = colliderImage.GetPixels();
//			for( int i = 0; i < allPixels.Length; i++ ) {
//				allPixels[i] = Color.clear;
//			}
//			colliderImage.SetPixels( allPixels );
//			
//			Texture2D sourceImage = mat.mainTexture as Texture2D;
//			Vector2 bottomLeft = layerToEdit.GetComponent<MeshFilter>().sharedMesh.bounds.min;
//			bottomLeft += (Vector2)layerToEdit.transform.position;
//			for( int i = 0; i < layerTileInfo.tiles.Length; i++ ) {
//				if( layerTileInfo.collisions[ (int)layerTileInfo.tiles[i].yInxex * sheetWidth + (int)layerTileInfo.tiles[i].xIndex ] ) {
//					Color[] pix = sourceImage.GetPixels(
//						(int)layerTileInfo.tiles[i].xIndex * (tileSize + spacing),
//						sourceImage.height - (((int)layerTileInfo.tiles[i].yInxex + 1) * (tileSize + spacing)),
//						tileSize,
//						tileSize
//						);
//					colliderImage.SetPixels(
//						(int)(layerTileInfo.tiles[i].x - bottomLeft.x) * tileSize,
//						(int)(layerTileInfo.tiles[i].y - bottomLeft.y) * tileSize,
//						tileSize,
//						tileSize,
//						pix
//						);
//				}
//				
//			}
//			colliderImage.Apply();
//			
//			Vector2 min = layerToEdit.GetComponent<MeshFilter>().sharedMesh.bounds.min;
//			ren.sprite = Sprite.Create(
//				colliderImage,
//				new Rect( 0, 0, colliderImage.width, colliderImage.height ),
//				new Vector2( -min.x / size.x, -min.y / size.y ),
//				tileSize
//				);
//			ren.sprite.name = "temp_sprite";
//			ren.sharedMaterial.mainTexture = colliderImage as Texture;
//			ren.sharedMaterial.shader = Shader.Find ("Sprites/Default");
//			tempObj.AddComponent<PolygonCollider2D>();
//			if( layerToEdit.GetComponent<PolygonCollider2D>() == null )
//				layerToEdit.AddComponent<PolygonCollider2D>();
//			EditorUtility.CopySerialized( tempObj.GetComponent<PolygonCollider2D>(), layerToEdit.GetComponent<PolygonCollider2D>() );
//			DestroyImmediate( tempObj );
//		}
//	}

}

public class CreateTileLayer : EditorWindow {
	int mapWidth;
	int mapHeight;
	string goName ;
	bool createRandomDungeon;

	public static void Init () {
		CreateTileLayer window = (CreateTileLayer)EditorWindow.GetWindow (typeof (CreateTileLayer));
		window.ShowPopup();

		window.mapWidth = 16;
		window.mapHeight = 16;
		window.goName = "Layer" + (GameObject.FindObjectsOfType<TileInfo>().Length + 1);
	}
	public void OnGUI () {

		mapWidth = EditorGUILayout.IntField( "Map Width", mapWidth );
		mapHeight = EditorGUILayout.IntField( "Map Height", mapHeight );

		mapWidth = Mathf.Max( mapWidth, 1 );
		mapWidth = Mathf.Min( mapWidth, 64 );

		mapHeight = Mathf.Max( mapHeight, 1 );
		mapHeight = Mathf.Min( mapHeight, 64 );

		goName = EditorGUILayout.TextField( "Layer name: ", goName );
		createRandomDungeon = EditorGUILayout.Toggle( "Create Random Dungeon", createRandomDungeon );
		if( GUILayout.Button( "Create Layer" ) ) {
			Rect mapRect = new Rect();
			mapRect.x = (int)TilesetEditor.posToCreateMesh.x;
			mapRect.y = (int)TilesetEditor.posToCreateMesh.y;
			mapRect.width = mapWidth;
			mapRect.height = mapHeight;
			TilesetEditor.CreateMesh( mapRect, goName, createRandomDungeon );
			TilesetEditor.autoTileSelected = -1;
			this.Close();
		}
	}
}

