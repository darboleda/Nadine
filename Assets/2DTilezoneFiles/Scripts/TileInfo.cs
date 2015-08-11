using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[AddComponentMenu("2D/Tile Info")]
public class TileInfo : MonoBehaviour {

	public enum CollisionType {
		None, Full, SlopeUpRight, SlopeUpLeft, Platform
	};

	public enum AutoTileEdgeMode {
		None, LinkToEdge, Wrap
	};

	public enum AutoTileType {
		Normal, IsometricWall
	};

	[HideInInspector]
	/// <summary>
	/// List of the tiles in a 1D array in the format [ local y * mapWidth * local x ].
	/// To get the index of a tile from a world position use WorldPointToMapIndex
	/// </summary>
	public Tile[] tiles;
	[HideInInspector]
	public int mapWidth;
	[HideInInspector]
	public int mapHeight;
	[HideInInspector]
	public int _numberOfTiles;

	public int numberOfTiles {
		get {
			return _numberOfTiles;
		}
	}

	//[HideInInspector]
	//public List<Vector2> tileIndex = new List<Vector2>();
	[HideInInspector]
	public int tileSize;
	[HideInInspector]
	public int spacing;
	[HideInInspector]
	public CollisionType[] collisions;
	//[HideInInspector]
	//public Vector3 positionAtLastEdit;
	[HideInInspector]
	public int numberOfAutotiles;
	[HideInInspector]
	public List<string> autoTileNames = new List<string>();
	[HideInInspector]
	public List<int> autoTileLinkMask = new List<int>();
	[HideInInspector]
	public List<Tile> autoTileData = new List<Tile>();
	[HideInInspector]
	public List<bool> showAutoTile = new List<bool>();
	[HideInInspector]
	public List<AutoTileEdgeMode> autoTileEdgeMode = new List<AutoTileEdgeMode>();
	[HideInInspector]
	public List<AutoTileType>autoTileType = new List<AutoTileType>();


	//settings
	[HideInInspector]
	public bool update3DWalls = false;

	[HideInInspector]
	public bool mapHasChanged = false;

	Vector2 _nonTransparentUV = new Vector2( -1, -1 );

	public Vector2 nonTransparentUV {
		get {

			if( _nonTransparentUV == new Vector2( -1, -1 ) ) {
				if( GetComponent<Renderer>() == null || GetComponent<Renderer>().sharedMaterial == null || GetComponent<Renderer>().sharedMaterial.mainTexture == null )
					return Vector2.zero;

				Texture2D tex = (Texture2D)GetComponent<Renderer>().sharedMaterial.mainTexture;
				for( int x = 0; x < tex.width; x++ ) {
					for( int y = 0; y < tex.height; y++ ) {
						if( tex.GetPixel( x, y ).a == 1 ) {
							_nonTransparentUV = new Vector2( (float)x / (float)tex.width, (float)y / (float)tex.height );
							return _nonTransparentUV;
						}
					}
				}
			}
			return _nonTransparentUV;
		}
	}

	PolygonCollider2D _mainCollider;

	public PolygonCollider2D mainCollider {
		get {
			if( _mainCollider == null ) {
				PolygonCollider2D[] allColliders = GetComponents<PolygonCollider2D>();
				foreach( PolygonCollider2D col in allColliders ) {
					if( !col.usedByEffector )
						_mainCollider = col;
				}
				if( _mainCollider == null )
					_mainCollider = gameObject.AddComponent<PolygonCollider2D>();
			}
			return _mainCollider;
		}
	}

	PolygonCollider2D _platformCollider;

	public PolygonCollider2D platformCollider {
		get {
			if( _platformCollider == null ) {
				PolygonCollider2D[] allColliders = GetComponents<PolygonCollider2D>();
				foreach( PolygonCollider2D col in allColliders ) {
					if( col.usedByEffector )
						_platformCollider = col;
				}
				if( _platformCollider == null ) {
					_platformCollider = gameObject.AddComponent<PolygonCollider2D>();
					_platformCollider.usedByEffector = true;
					if( GetComponent<PlatformEffector2D>() == null )
						gameObject.AddComponent<PlatformEffector2D>();
				}
			}
			return _platformCollider;
		}
	}

	public static TileInfo[] _allMaps;
	public static TileInfo[] allMaps {
		get {
			if( _allMaps == null )
				_allMaps = FindObjectsOfType<TileInfo>();
			return _allMaps;
		}
	}

	public static TileInfo GetMapAtWorldPos ( Vector2 worldPos ) {
		foreach( TileInfo map in allMaps ) {
			if( map.WorldPointToMapIndex( worldPos ) != -1 )
				return map;
		}
		return null;
	}

	public static TileInfo[] GetMapsAtWorldPos ( Vector2 worldPos ) {
		List<TileInfo> result = new List<TileInfo>();
		foreach( TileInfo map in allMaps ) {
			if( map.WorldPointToMapIndex( worldPos ) != -1 )
				result.Add( map );
		}
		return result.ToArray();
	}

	public static bool CollisionAtWorldPos ( Vector2 worldPos ) {
		TileInfo[] maps = GetMapsAtWorldPos( worldPos );
		foreach( TileInfo map in maps ) {
			if( map.tiles[map.WorldPointToMapIndex(worldPos)].GetCollisionType( map ) != CollisionType.None )
				return true;
		}
		return false;
	}

	public static CollisionType CollisionTypeAtWorldPos ( Vector2 worldPos ) {
		TileInfo[] maps = GetMapsAtWorldPos( worldPos );
		CollisionType result = CollisionType.None;
		foreach( TileInfo map in maps ) {
			CollisionType tempResult = map.tiles[map.WorldPointToMapIndex(worldPos)].GetCollisionType( map );
			if( tempResult != CollisionType.None && result != CollisionType.Full )
				result = tempResult;
		}
		return result;
	}

	void AddWall ( Vector2 p1, Vector2 p2, List<Vector3> vertices, List<Vector2> uv, List<int> triangles ) {
		vertices.Add( new Vector3( p1.x, p1.y, 10 ) );
		vertices.Add( new Vector3( p1.x, p1.y, 0 ) );
		vertices.Add( new Vector3( p2.x, p2.y, 0 ) );
		vertices.Add( new Vector3( p2.x, p2.y, 10 ) );

		uv.Add( nonTransparentUV );
		uv.Add( nonTransparentUV );
		uv.Add( nonTransparentUV );
		uv.Add( nonTransparentUV );

		triangles.Add( vertices.Count - 4 );
		triangles.Add( vertices.Count - 3 );
		triangles.Add( vertices.Count - 1 );
		triangles.Add( vertices.Count - 1 );
		triangles.Add( vertices.Count - 3 );
		triangles.Add( vertices.Count - 2 );
	}



	/// <summary>
	/// Updates the visual mesh.
	/// </summary>
	public void UpdateVisualMesh () {
		if( !mapHasChanged )
			return;
		mapHasChanged = false;
		Mesh m = new Mesh();
//		Vector3[] vertices = new Vector3[numberOfTiles * 4];
//		Vector2[] uv = new Vector2[numberOfTiles * 4];
//		int[] triangles = new int[numberOfTiles * 6];

		List<Vector3> vertices = new List<Vector3>();
		//vertices.AddRange( new Vector3[numberOfTiles * 4] );
		List<Vector2> uv = new List<Vector2>();
		//uv.AddRange( new Vector2[numberOfTiles * 4] );
		List<int> triangles = new List<int>();
		//triangles.AddRange( new int[numberOfTiles * 6] );

		//int i = 0;

		for( int x = 0; x < mapWidth; x++ ) {
			for( int y = 0; y < mapHeight; y++ ) {

				UpdateAutoTile( x, y );

				if( tiles[ y * mapWidth + x ] == Tile.empty )
					continue;

				int i = vertices.Count / 4;

				vertices.AddRange( new Vector3[4] );
				uv.AddRange( new Vector2[4] );
				triangles.AddRange( new int[6] );



				vertices[i*4 + tiles[y * mapWidth + x].rotation ] = new Vector3( x, y );
				vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4] = new Vector3( x + 1, y );
				vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4] = new Vector3( x + 1, y + 1 );
				vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4] = new Vector3( x, y + 1 );
				
				triangles[i*6] = i*4;
				triangles[i*6 + 1] = i*4 + 2;
				triangles[i*6 + 2] = i*4 + 1;
				
				triangles[i*6 + 3] = i*4;
				triangles[i*6 + 4] = i*4 + 3;
				triangles[i*6 + 5] = i*4 + 2;
				
				Material mat = GetComponent<Renderer>().sharedMaterial;
				//Vector2 uvBottomLeft = new Vector2( (float)(tiles[ y * mapWidth + x ].xIndex * (tileSize + spacing)) / mat.mainTexture.width, 1f - (float)((tiles[ y * mapWidth + x ].yIndex + 1)  * (float)(tileSize + spacing) - spacing) / (float)mat.mainTexture.height );
				Vector2 uvBottomLeft = tiles[ y * mapWidth + x ].GetBottomLeftUV( this );
				Vector2 uvRight = new Vector2( (float)tileSize / mat.mainTexture.width, 0 );
				Vector2 uvTop = new Vector2( 0, (float)tileSize / mat.mainTexture.height );

				if( tiles[y * mapWidth + x].flip ) {
					uv[i*4] = uvBottomLeft + uvRight;
					uv[i*4 + 1] = uvBottomLeft;
					uv[i*4 + 2] = uvBottomLeft + uvTop;
					uv[i*4 + 3] = uvBottomLeft + uvRight + uvTop;
				}
				else {
					uv[i*4] = uvBottomLeft;
					uv[i*4 + 1] = uvBottomLeft + uvRight;
					uv[i*4 + 2] = uvBottomLeft + uvRight + uvTop;
					uv[i*4 + 3] = uvBottomLeft + uvTop;
				}

				//i++;

				if( update3DWalls ) {
					Vector2 ti = (Vector2)tiles[y * mapWidth + x];
					int width = (gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture.width / (tileSize + spacing));
					Vector2[] points;
					Vector2[] pointsB;
					
					bool up = LocalPointToMapIndex(new Vector2( x, y + 1 )) != -1 && tiles[(y+1) * mapWidth + x] != Tile.empty && collisions[tiles[(y+1) * mapWidth + x].yIndex * width + tiles[(y+1) * mapWidth + x].xIndex] == CollisionType.Full;
					bool down = LocalPointToMapIndex(new Vector2( x, y - 1 )) != -1 && tiles[(y-1) * mapWidth + x] != Tile.empty && collisions[tiles[(y-1) * mapWidth + x].yIndex * width + tiles[(y-1) * mapWidth + x].xIndex] == CollisionType.Full;
					bool left = LocalPointToMapIndex(new Vector2( x - 1, y )) != -1 && tiles[y * mapWidth + (x-1)] != Tile.empty && collisions[tiles[y * mapWidth + (x-1)].yIndex * width + tiles[y * mapWidth + (x-1)].xIndex] == CollisionType.Full;
					bool right = LocalPointToMapIndex(new Vector2( x + 1, y )) != -1 && tiles[y * mapWidth + (x+1)] != Tile.empty && collisions[tiles[y * mapWidth + (x+1)].yIndex * width + tiles[y * mapWidth + (x+1)].xIndex] == CollisionType.Full;
					
					bool flip = tiles[ y * mapWidth + x ].flip;
					
					switch( collisions[ (int)ti.y * width + (int)ti.x ] ) {
					case CollisionType.Full:
					case CollisionType.Platform:
						
//						points = new Vector2[4];
//						points[0] = new Vector2( x, y );
//						points[1] = new Vector2( x+1, y );
//						points[2] = new Vector2( x+1, y+1 );
//						points[3] = new Vector2( x, y+1 );
						if( !up )
							AddWall( vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4], vertices, uv, triangles );
						
						if( !down )
							AddWall( vertices[i*4 + tiles[y * mapWidth + x].rotation], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4], vertices, uv, triangles );
						
						if( !left )
							AddWall( vertices[i*4 + (tiles[y * mapWidth + x].rotation + 3) % 4], vertices[i*4 + tiles[y * mapWidth + x].rotation], vertices, uv, triangles );
						
						if( !right )
							AddWall( vertices[i*4 + (tiles[y * mapWidth + x].rotation + 1) % 4], vertices[i*4 + (tiles[y * mapWidth + x].rotation + 2) % 4], vertices, uv, triangles );
						
						
						break;
						
					case CollisionType.SlopeUpRight:
						
						points = new Vector2[2];
						pointsB = new Vector2[4];
						pointsB[0] = new Vector2( x, y );
						pointsB[1] = new Vector2( x+1, y );
						pointsB[2] = new Vector2( x+1, y+1 );
						pointsB[3] = new Vector2( x, y+1 );
						if( tiles[y * mapWidth + x].flip ) {
							points[0] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
							points[1] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						}
						else
						{
							points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
							points[1] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						}
						
						if( flip )
							AddWall( points[0], points[1], vertices, uv, triangles );
						else
							AddWall( points[1], points[0], vertices, uv, triangles );
						
						break;
						
					case CollisionType.SlopeUpLeft:
						
						points = new Vector2[2];
						pointsB = new Vector2[4];
						pointsB[0] = new Vector2( x, y );
						pointsB[1] = new Vector2( x+1, y );
						pointsB[2] = new Vector2( x+1, y+1 );
						pointsB[3] = new Vector2( x, y+1 );
						if( tiles[y * mapWidth + x].flip ) {
							points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
							points[1] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						}
						else {
							points[0] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
							points[1] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						}
						
						if( flip )
							AddWall( points[1], points[0], vertices, uv, triangles );
						else
							AddWall( points[0], points[1], vertices, uv, triangles );
						
						break;
					}
				}
			}
		}
		
		m.vertices = vertices.ToArray();
		m.triangles = triangles.ToArray();
		m.uv = uv.ToArray();
		m.RecalculateNormals();
		
		GetComponent<MeshFilter>().sharedMesh = m;
	}

	bool AutoTileIsLinked( int otherTile, int thisTile ) {
		if( otherTile == -1 || thisTile == -1 )
			return false;
		return (autoTileLinkMask[thisTile] & (int)Mathf.Pow( 2, otherTile )) == (int)Mathf.Pow( 2, otherTile );
	}

	void UpdateAutoTile ( int x, int y ) {

		if( LocalPointToMapIndex( new Vector2( x, y ) ) == -1 )
			return;
		if( tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ].autoTileIndex == -1 )
			return;
		if( tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ].autoTileIndex >= numberOfAutotiles )
			return;

		bool up, down, left, right;
		bool upRight, upLeft, downRight, downLeft;

		int autoTileIndex = tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ].autoTileIndex;
		
		Tile selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );

		if( autoTileType[autoTileIndex] == AutoTileType.Normal ) {

			switch( autoTileEdgeMode[autoTileIndex] ) {
				
			case AutoTileEdgeMode.None:
			default:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );
				
				upRight = LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y +1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				downRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				downLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;
				
			case AutoTileEdgeMode.LinkToEdge:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );
				
				upRight = LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y +1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				downRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				downLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;
				
			case AutoTileEdgeMode.Wrap:
				up = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				down = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				left = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );
				right = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );
				
				upRight = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				upLeft = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				downRight = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				downLeft = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				break;
			}


			if( !up && !left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );
			
			if( !up && left && down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
			
			if( !up && left && down && !right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );
			
			if( !up && !left && down && !right  )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );
			
			if( !up && !left && down && right && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 4 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 4 ] );
			}
			
			if( !up && left && down && !right && !downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 5 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 5 ] );
			}
			
			if( up && !left && down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
			
			if( up && left && down && right && upRight && upLeft && downRight && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
			
			if( up && left && down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
			
			if( up && !left && down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );
			
			if( up && !left && !down && right && !upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 10 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 10 ] );
			}
			
			if( up && left && !down && !right && !upLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 11 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 11 ] );
			}
			
			if( up && !left && !down && right && upRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );
			
			if( up && left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );
			
			if( up && left && !down && !right && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );
			
			if( up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );
			
			if( up && left && down && right && !downRight && upLeft && upRight && downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 16 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 16 ] );
			}
			
			if( up && left && down && right && !downLeft && upLeft && upRight && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 17 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 17 ] );
			}
			
			if( !up && !left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 18 ] );
			
			if( !up && left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 19 ] );
			
			if( !up && left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 20 ] );
			
			if( !up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );
			
			if( up && left && down && right && ! upRight && upLeft && downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 22 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 22 ] );
			}
			
			if( up && left && down && right && !upLeft && upRight && downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 23 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 23 ] );
			}

			if( up && !left && down && right && !upRight && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 24 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 24 ] );
			}

			if( !up && left && down && right && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 25 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 25 ] );
			}

			if( up && left && down && right && upLeft && !upRight && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 26 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 26 ] );
			}

			if( up && left && down && right && upLeft && upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 27 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 27 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 28 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 28 ] );
			}

			if( up && left && down && right && upLeft && !upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 29 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 29 ] );
			}

			if( up && left && !down && right && !upLeft && !upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 30 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 30 ] );
			}

			if( up && left && down && !right && !upLeft && !downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 31 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 31 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 32 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 32 ] );
			}

			if( up && left && down && right && !upLeft && upRight && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 33 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 33 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 34 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 34 ] );
			}
			if( up && left && down && right && !upLeft && upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 35 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 35 ] );
			}

			if( up && left && down && right && !upLeft && !upRight && !downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 40 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 40 ] );
			}

			if( up && left && down && right && upLeft && !upRight && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 41 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 41 ] );
			}

			if( up && left && down && right && !upLeft && upRight && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 46 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 46 ] );
			}

			if( up && !left && down && right && upRight && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 36 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 36 ] );
			}

			if( !up && left && down && right && !downLeft && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 37 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 37 ] );
			}

			if( !up && left && down && right && downLeft && !downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 38 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 38 ] );
			}

			if( up && left && down && !right && upLeft && !downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 39 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 39 ] );
			}

			if( up && left && !down && right && upLeft && !upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 42 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 42 ] );
			}

			if( up && left && down && !right && !upLeft && downLeft ) {
				if( autoTileData[ autoTileIndex * 48 + 43 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 43 ] );
			}

			if( up && !left && down && right && !upRight && downRight ) {
				if( autoTileData[ autoTileIndex * 48 + 44 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 44 ] );
			}

			if( up && left && !down && right && !upLeft && upRight ) {
				if( autoTileData[ autoTileIndex * 48 + 45 ] == Tile.empty )
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );
				else
					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 45 ] );
			}
		}


		if( autoTileType[autoTileIndex] == AutoTileType.IsometricWall ) {

			switch( autoTileEdgeMode[autoTileIndex] ) {
				
			case AutoTileEdgeMode.None:
			default:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );

				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				upRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) != -1
					&& AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;
				
			case AutoTileEdgeMode.LinkToEdge:
				up = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y + 1 ) ) ].autoTileIndex, autoTileIndex );
				down = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				left = LocalPointToMapIndex( new Vector2( x - 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y ) ) ].autoTileIndex, autoTileIndex );
				right = LocalPointToMapIndex( new Vector2( x + 1, y ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y ) ) ].autoTileIndex, autoTileIndex );

				upLeft = LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x - 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				upRight = LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) == -1
					|| AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x + 1, y - 1 ) ) ].autoTileIndex, autoTileIndex );
				break;
				
			case AutoTileEdgeMode.Wrap:
				up = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y + 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				down = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( x, Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				left = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );
				right = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), y ) ) ].autoTileIndex, autoTileIndex );

				upLeft = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x - 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				upRight = AutoTileIsLinked( tiles[ LocalPointToMapIndex( new Vector2( Mod(x + 1, mapWidth), Mod(y - 1, mapHeight) ) ) ].autoTileIndex, autoTileIndex );
				break;
			}

			downRight = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
				&& down && (tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] ||
				            tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 14 ] || 
				            tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
				            tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 15 ] );

			downLeft = LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1
				&& down && (tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] ||
				            tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 12 ] ||
				            tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
				            tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 15 ] );

//			upRight = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
//				&& up && (tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] ||
//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 2 ] || 
//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 3 ] );
//			
//			upLeft = LocalPointToMapIndex( new Vector2( x, y + 1 ) ) != -1
//				&& up && (tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] ||
//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 0 ] ||
//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 9 ] || 
//				            tiles[LocalPointToMapIndex( new Vector2( x, y + 1 ) )] == autoTileData[ autoTileIndex * 48 + 3 ] );

			if( !up && !left && down && right && !downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );

			if( !up && !left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );
			
			if( !up && left && down && !right && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );

			if( !up && left && down && !right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );
			
			if( !up && !left && down && !right  )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );
			
			if( up && !left && down && right && !downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );

			if( up && !left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );
			
			if( up && left && down && !right && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );

			if( up && left && down && !right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );
			
			if( up && !left && down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 9 ] );
			
			if( up && !left && !down && right && !upRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );

			if( up && !left && !down && right && upRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );
			
			if( up && left && !down && right && !upRight && !upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 13 ] );

			if( up && left && !down && right && upRight && !upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );

			if( up && left && !down && right && !upRight && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 12 ] );

			if( up && left && !down && right && upRight && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );
			
			if( up && left && !down && !right && !upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 14 ] );

			if( up && left && !down && !right && upLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );
			
			if( up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 15 ] );
			
			if( !up && !left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 18 ] );
			
			if( !up && left && !down && right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 19 ] );
			
			if( !up && left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 20 ] );
			
			if( !up && !left && !down && !right )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );

			if( !up && left && down && right && !downRight && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );

			if( !up && left && down && right && downRight && !downLeft)
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );

			if( !up && left && down && right && !downRight && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );

			if( !up && left && down && right && downRight && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 3 ] );

			if( up && left && down && right && !downRight && !downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );

			if( up && left && down && right && downRight )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );

			if( up && left && down && right && downLeft )
				selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
//			if( !up && left && down && right ) {
//				if( LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1 ) {
//					if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] || 
//					   tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 14 ] ) {
//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 2 ] );
//						Debug.Log( "made side1 wall" );
//					}
//					else if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] || 
//					        tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 12 ] ) {
//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 0 ] );
//						Debug.Log( "made side2 wall" );
//					}
//					else {
//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
//						Debug.Log( "made middle wall" );
//					}
//				}
//				else {
//					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 1 ] );
//					Debug.Log( "made middle wall at bottom" );
//				}
//			}
//			
//			if( up && left && down && right ) {
//				if( LocalPointToMapIndex( new Vector2( x, y - 1 ) ) != -1 ) {
//					if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 8 ] || 
//					   tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 14 ] ) {
//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 8 ] );
//						Debug.Log( "made side1 wall" );
//					}
//					else if( tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 6 ] || 
//					        tiles[LocalPointToMapIndex( new Vector2( x, y - 1 ) )] == autoTileData[ autoTileIndex * 48 + 12 ] ) {
//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 6 ] );
//						Debug.Log( "made side2 wall" );
//					}
//					else {
//						selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
//						Debug.Log( "made middle wall" );
//					}
//				}
//				else {
//					selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 7 ] );
//					Debug.Log( "made middle wall at bottom" );
//				}
//			}
		}


		if( selectedTile == Tile.empty )
			selectedTile = new Tile( autoTileData[ autoTileIndex * 48 + 21 ] );

		if( selectedTile == Tile.empty )
			_numberOfTiles--;
		else
			selectedTile.autoTileIndex = autoTileIndex;
		tiles[ LocalPointToMapIndex( new Vector2( x, y ) ) ] = new Tile( selectedTile );
	}

	public static int Mod( int value, int length ) {
		if( length == 1 )
			return 0;
		while( value < 0 )
			value += length;
		return value % length;
	}

	public void FloodFill ( Vector2 worldPos, Tile[,] selectedTiles, Vector2 originalPos, Tile originalTile ) {
		if( WorldPointToMapIndex(originalPos) == -1 )
			return;
		if( WorldPointToMapIndex( worldPos ) == -1 )
			return;
		Tile replacementTile = selectedTiles[Mod((int)(worldPos.x - originalPos.x), selectedTiles.GetLength(0)), Mod((int)(originalPos.y - worldPos.y), selectedTiles.GetLength(1))];

		if( originalTile == replacementTile )
			return;

		if( tiles[WorldPointToMapIndex(worldPos)] != originalTile ) {
			//if( targetTile.autoTileIndex == -1 || targetTile.autoTileIndex != tiles[WorldPointToMapIndex(worldPos)].autoTileIndex )
				return;
		}

		AddTile( worldPos, replacementTile );
		FloodFill( new Vector2( worldPos.x + 1, worldPos.y ), selectedTiles, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x - 1, worldPos.y ), selectedTiles, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y + 1 ), selectedTiles, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y - 1 ), selectedTiles, originalPos, originalTile );
	}

	public void FloodFill ( Vector2 worldPos, int autoTileIndex, Vector2 originalPos, Tile originalTile ) {
		if( WorldPointToMapIndex(originalPos) == -1 )
			return;
		if( WorldPointToMapIndex( worldPos ) == -1 )
			return;
		Tile replacementTile = autoTileData[ autoTileIndex * 48 + 0 ];
		
		if( originalTile == replacementTile )
			return;
		
		if( tiles[WorldPointToMapIndex(worldPos)] != originalTile ) {
			//if( targetTile.autoTileIndex == -1 || targetTile.autoTileIndex != tiles[WorldPointToMapIndex(worldPos)].autoTileIndex )
			return;
		}
		
		AddTile( worldPos, new Tile( (Vector2)replacementTile, replacementTile.rotation, autoTileIndex ) );
		FloodFill( new Vector2( worldPos.x + 1, worldPos.y ), autoTileIndex, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x - 1, worldPos.y ), autoTileIndex, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y + 1 ), autoTileIndex, originalPos, originalTile );
		FloodFill( new Vector2( worldPos.x, worldPos.y - 1 ), autoTileIndex, originalPos, originalTile );
	}

	/// <summary>
	/// Adds a tile with default flip and rotation. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	/// <param name="selectedTile">Selected tile.</param>
	public void AddTile( Vector2 worldPos, Tile selectedTile ) {
		if( selectedTile == new Vector2( 0.5f, 0.5f ) )
			return;
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		if( tiles[WorldPointToMapIndex(worldPos)] != Tile.empty )
			RemoveTile( worldPos );

		tiles[WorldPointToMapIndex(worldPos)] = new Tile( selectedTile );
		_numberOfTiles++;
		mapHasChanged = true;
	}

	/// <summary>
	/// Adds an auto tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	/// <param name="autoTileIndex">Auto tile index.</param>
	public void AddTile( Vector2 worldPos, int autoTileIndex ) {
		if( autoTileData.Count == 0 )
			return;
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

//		if( autoTileData[ autoTileIndex * 48 + 21 ] == Tile.empty )
//			return;

		if( tiles[WorldPointToMapIndex(worldPos)] != Tile.empty )
			RemoveTile( worldPos );


		if( autoTileData[ autoTileIndex * 48 + 21 ] != Tile.empty ) {
			tiles[WorldPointToMapIndex(worldPos)] = new Tile( autoTileData[ autoTileIndex * 48 + 21 ].xIndex, autoTileData[ autoTileIndex * 48 + 21 ].yIndex, 0, autoTileIndex );
			_numberOfTiles++;
		}
		mapHasChanged = true;
	}

	/// <summary>
	/// Resizes the bounds to fit world position.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void ResizeBoundsToFitWorldPos ( Vector2 worldPos ) {
		const int maxSize = 64;
		if( WorldPointToMapIndex( worldPos ) != -1 )
			return;
		worldPos.x = Mathf.Floor( worldPos.x );
		worldPos.y = Mathf.Floor( worldPos.y );

		//resize to the right
		if( worldPos.x + 1 > transform.position.x + mapWidth ) {
			if( (int)(worldPos.x + 1 - transform.position.x) > maxSize )
				worldPos.x = transform.position.x + maxSize - 1;
			Tile[] newMap = new Tile[(int)(worldPos.x + 1 - transform.position.x) * mapHeight];
			for( int x = 0; x < (int)(worldPos.x + 1 - transform.position.x); x++ ) {
				for( int y = 0; y < mapHeight; y++ ) {
					if( x >= mapWidth ) {
						newMap[y*(int)(worldPos.x + 1 - transform.position.x)+x] = Tile.empty;
						continue;
					}
					newMap[y*(int)(worldPos.x + 1 - transform.position.x)+x] = tiles[y*mapWidth+x];
				}
			}
			mapWidth = (int)(worldPos.x + 1 - transform.position.x);
			tiles = newMap;
		}

		//resize to the left
		if( worldPos.x  < transform.position.x ) {
			if( ((int)(transform.position.x - worldPos.x) + mapWidth) > maxSize )
				worldPos.x = transform.position.x - (maxSize-mapWidth);
			Tile[] newMap = new Tile[((int)(transform.position.x - worldPos.x) + mapWidth) * mapHeight];
			for( int x = 0; x < ((int)(transform.position.x - worldPos.x) + mapWidth); x++ ) {
				for( int y = 0; y < mapHeight; y++ ) {
					if( x < (int)(transform.position.x - worldPos.x) ) {
						newMap[y*((int)(transform.position.x - worldPos.x) + mapWidth)+x] = Tile.empty;
						continue;
					}
					newMap[y*((int)(transform.position.x - worldPos.x) + mapWidth)+x] = tiles[y*mapWidth+(x-(int)(transform.position.x - worldPos.x))];
				}
			}
			mapWidth = ((int)(transform.position.x - worldPos.x) + mapWidth);
			tiles = newMap;
			Vector3 newPos = transform.position;
			newPos.x -= (transform.position.x - worldPos.x);
			transform.position = newPos;
			mapHasChanged = true;
			UpdateVisualMesh();
		}

		//resize to the up
		if( worldPos.y + 1 > transform.position.y + mapHeight ) {
			if( (int)(worldPos.y + 1 - transform.position.y) > maxSize )
				worldPos.y = transform.position.y + maxSize - 1;
			Tile[] newMap = new Tile[(int)(worldPos.y + 1 - transform.position.y) * mapWidth];
			for( int x = 0; x < mapWidth; x++ ) {
				for( int y = 0; y < (int)(worldPos.y + 1 - transform.position.y); y++ ) {
					if( y >= mapHeight ) {
						newMap[y*mapWidth+x] = Tile.empty;
						continue;
					}
					newMap[y*mapWidth+x] = tiles[y*mapWidth+x];
				}
			}
			mapHeight = (int)(worldPos.y + 1 - transform.position.y);
			tiles = newMap;
		}

		//resize to the down
		if( worldPos.y  < transform.position.y ) {
			if( ((int)(transform.position.y - worldPos.y) + mapHeight) > maxSize )
				worldPos.y = transform.position.y - (maxSize-mapHeight);
			Tile[] newMap = new Tile[((int)(transform.position.y - worldPos.y) + mapHeight) * mapWidth];
			for( int x = 0; x < mapWidth; x++ ) {
				for( int y = 0; y < ((int)(transform.position.y - worldPos.y) + mapHeight); y++ ) {
					if( y < (int)(transform.position.y - worldPos.y) ) {
						newMap[y*mapWidth+x] = Tile.empty;
						continue;
					}
					newMap[y*mapWidth+x] = tiles[(y-(int)(transform.position.y - worldPos.y))*mapWidth+x];
				}
			}
			mapHeight = ((int)(transform.position.y - worldPos.y) + mapHeight);
			tiles = newMap;
			Vector3 newPos = transform.position;
			newPos.y -= (transform.position.y - worldPos.y);
			transform.position = newPos;
			mapHasChanged = true;
			UpdateVisualMesh();
		}
	}

	/// <summary>
	/// Updates the colliders.
	/// </summary>
	public void UpdateColliders () {
//		PolygonCollider2D mainCollider;
//		if( GetComponent<PolygonCollider2D>() == null )
//			mainCollider = gameObject.AddComponent<PolygonCollider2D>();
//		else
//			mainCollider = gameObject.GetComponent<PolygonCollider2D>();
		mainCollider.pathCount = 0;
		if( _platformCollider != null )
			platformCollider.pathCount = 0;
		
		int i = 0;
		int p = 0;
		for( int x = 0; x < mapWidth; x++ ) {
			for( int y = 0; y < mapHeight; y++ ) {
				if( tiles[y * mapWidth + x] == Tile.empty )
					continue;
				
				Tile ti = tiles[y * mapWidth + x];
				int width = (gameObject.GetComponent<Renderer>().sharedMaterial.mainTexture.width / (tileSize + spacing));
				Vector2[] points;
				Vector2[] pointsB;

				bool up = LocalPointToMapIndex(new Vector2( x, y + 1 )) != -1 && tiles[(y+1) * mapWidth + x] != Tile.empty && collisions[tiles[(y+1) * mapWidth + x].yIndex * width + tiles[(y+1) * mapWidth + x].xIndex] == CollisionType.Full;
				bool down = LocalPointToMapIndex(new Vector2( x, y - 1 )) != -1 && tiles[(y-1) * mapWidth + x] != Tile.empty && collisions[tiles[(y-1) * mapWidth + x].yIndex * width + tiles[(y-1) * mapWidth + x].xIndex] == CollisionType.Full;
				bool left = LocalPointToMapIndex(new Vector2( x - 1, y )) != -1 && tiles[y * mapWidth + (x-1)] != Tile.empty && collisions[tiles[y * mapWidth + (x-1)].yIndex * width + tiles[y * mapWidth + (x-1)].xIndex] == CollisionType.Full;
				bool right = LocalPointToMapIndex(new Vector2( x + 1, y )) != -1 && tiles[y * mapWidth + (x+1)] != Tile.empty && collisions[tiles[y * mapWidth + (x+1)].yIndex * width + tiles[y * mapWidth + (x+1)].xIndex] == CollisionType.Full;

				switch( collisions[ ti.yIndex * width + ti.xIndex ] ) {
				case CollisionType.Full:

					if( up && down && left && right )
						break;
					points = new Vector2[4];
					points[0] = new Vector2( x, y );
					points[1] = new Vector2( x+1, y );
					points[2] = new Vector2( x+1, y+1 );
					points[3] = new Vector2( x, y+1 );
					mainCollider.pathCount = i + 1;
					mainCollider.SetPath( i, points );
					i++;

					break;

				case CollisionType.SlopeUpRight:

					points = new Vector2[3];
					pointsB = new Vector2[4];
					pointsB[0] = new Vector2( x, y );
					pointsB[1] = new Vector2( x+1, y );
					pointsB[2] = new Vector2( x+1, y+1 );
					pointsB[3] = new Vector2( x, y+1 );
					if( tiles[y * mapWidth + x].flip ) {
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					else
					{
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					mainCollider.pathCount = i + 1;
					mainCollider.SetPath( i, points );
					i++;
					
					break;

				case CollisionType.SlopeUpLeft:
					
					points = new Vector2[3];
					pointsB = new Vector2[4];
					pointsB[0] = new Vector2( x, y );
					pointsB[1] = new Vector2( x+1, y );
					pointsB[2] = new Vector2( x+1, y+1 );
					pointsB[3] = new Vector2( x, y+1 );
					if( tiles[y * mapWidth + x].flip ) {
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(2 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					else
					{
						points[0] = pointsB[(0 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[1] = pointsB[(1 + 4 - tiles[y * mapWidth + x].rotation) % 4];
						points[2] = pointsB[(3 + 4 - tiles[y * mapWidth + x].rotation) % 4];
					}
					mainCollider.pathCount = i + 1;
					mainCollider.SetPath( i, points );
					i++;
					
					break;

					//case for platforms
				case CollisionType.Platform:
					
					if( up && down && left && right )
						break;
					points = new Vector2[4];
					points[0] = new Vector2( x, y+0.5f );
					points[1] = new Vector2( x+1, y+0.5f );
					points[2] = new Vector2( x+1, y+1 );
					points[3] = new Vector2( x, y+1 );
					platformCollider.pathCount = p + 1;
					platformCollider.SetPath( p, points );
					p++;
					
					break;
				}
			}
//			if( Application.isEditor )
//				UnityEditor.EditorUtility.DisplayProgressBar( "Updating 2D Collisions", "Updating the PolygonCollider2D points...", (float)x / mapHeight );
		}
//		if( Application.isEditor )
//			UnityEditor.EditorUtility.ClearProgressBar();
	}

	/// <summary>
	/// Returns the tile index of the tile at the given world coordinates. Returns -1 if the position is out of the map bounds
	/// </summary>
	/// <returns>The point to map index.</returns>
	/// <param name="worldPos">World position.</param>
	public int WorldPointToMapIndex ( Vector2 worldPos ) {
		Vector2 localPos = worldPos - (Vector2)transform.position;
		localPos.x = Mathf.Floor( localPos.x );
		localPos.y = Mathf.Floor( localPos.y );
		if( localPos.x < 0 || localPos.x >= mapWidth || localPos.y < 0 || localPos.y >= mapHeight )
			return -1;
		return (int)localPos.y * mapWidth + (int)localPos.x;
	}

	/// <summary>
	/// Returns the tile index of the tile at the given world coordinates. Returns -1 if the position is out of the map bounds
	/// </summary>
	/// <returns>The point to map index.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public int WorldPointToMapIndex ( int x, int y ) {
		return WorldPointToMapIndex( new Vector2( x, y ) );
	}

	/// <summary>
	/// Returns the tile index of the tile at the given local coordinates. Returns -1 if the position is out of the map bounds
	/// </summary>
	/// <returns>The point to map index.</returns>
	/// <param name="localPos">Local position.</param>
	public int LocalPointToMapIndex ( Vector2 localPos ) {
		localPos.x = Mathf.Floor( localPos.x );
		localPos.y = Mathf.Floor( localPos.y );
		if( localPos.x < 0 || localPos.x >= mapWidth || localPos.y < 0 || localPos.y >= mapHeight )
			return -1;
		return (int)localPos.y * mapWidth + (int)localPos.x;
	}

	/// <summary>
	/// Removes the tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void RemoveTile ( Vector2 worldPos ) {
		//GameObject layerToEdit = GameObject.Find( layerName );
			//System.Collections.Generic.List<Vector2> tileList = layerToEdit.GetComponent<TileInfo>().tiles;
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		if( tiles[WorldPointToMapIndex( worldPos )] == Tile.empty )
			return;

		tiles[WorldPointToMapIndex( worldPos )] = Tile.empty;
		_numberOfTiles--;
		mapHasChanged = true;
	}

	/// <summary>
	/// Rotates the tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void RotateTile ( Vector2 worldPos ) {
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;

		if( tiles[WorldPointToMapIndex( worldPos )] == Tile.empty )
			return;

		tiles[WorldPointToMapIndex( worldPos )].rotation--;
		if( tiles[WorldPointToMapIndex( worldPos )].rotation < 0 )
			tiles[WorldPointToMapIndex( worldPos )].rotation += 4;
		mapHasChanged = true;
	}

	/// <summary>
	/// Flips the tile. UpdateVisualMesh() must be called after all changes have been made to see the result.
	/// </summary>
	/// <param name="worldPos">World position.</param>
	public void FlipTile ( Vector2 worldPos ) {
		if( WorldPointToMapIndex( worldPos ) < 0 )
			return;
		
		if( tiles[WorldPointToMapIndex( worldPos )] == Tile.empty )
			return;
		
		tiles[WorldPointToMapIndex( worldPos )].flip = !tiles[WorldPointToMapIndex( worldPos )].flip;
		mapHasChanged = true;
	}

}

[XmlRoot]
public class AutoTile {
	[XmlElement]
	public string autoTileName;
	[XmlElement]
	public List<Tile> autoTileData = new List<Tile>();
	[XmlElement]
	public int textureWidth;
	[XmlElement]
	public int textureHeight;
}

[XmlRoot]
public class CollisionData {
	[XmlElement]
	public TileInfo.CollisionType[] collisions;
}

/// <summary>
/// Tile class. 
/// </summary>
[System.Serializable]
public class Tile {
	
	public static Tile empty {
		get {
			return new Tile( -1, -1, -1 );

		}
	}

	public Tile () {
		this.xIndex = -1;
		this.yIndex = -1;
		this.rotation = -1;
		this.autoTileIndex = -1;
		this.flip = false;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="xIndex">X The x position of the tile image on the sprite sheet.</param>
	/// <param name="yIndex">Y The y position of the tile image on the sprite sheet.</param>
	/// <param name="meshIndex">Mesh The index of the mesh for referencing what vertices and triacgles belong to this tile..</param>
	public Tile ( int xIndex, int yIndex, int rotation = 0, int autoTileIndex = -1, bool flip = false ) {
		this.xIndex = xIndex;
		this.yIndex = yIndex;
		this.rotation = rotation;
		this.flip = flip;
		this.autoTileIndex = autoTileIndex;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="index">The position of the tile image on the sprite sheet starting at the top left most tile being x = 0 y = 0, and the next tile to the right being x = 1 y = 0..</param>
	/// <param name="meshIndex">Mesh index.</param>
	public Tile ( Vector2 index, int rotation = 0, int autoTileIndex = -1, bool flip = false ) {
		this.xIndex = (int)index.x;
		this.yIndex = (int)index.y;
		this.rotation = rotation;
		this.flip = flip;
		this.autoTileIndex = autoTileIndex;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="original">Original tile to clone.</param>
	public Tile ( Tile original ) {
		this.xIndex = original.xIndex;
		this.yIndex = original.yIndex;
		this.rotation = original.rotation;
		this.flip = original.flip;
		this.autoTileIndex = original.autoTileIndex;
	}
	
	/// <summary>
	/// The x position of the tile image on the sprite sheet
	/// starting with the left most tile being x = 0.
	/// </summary>
	public int xIndex;
	
	/// <summary>
	/// The y position of the tile image on the sprite sheet
	/// starting with the top most tile being y = 0.
	/// </summary>
	public int yIndex;

	/// <summary>
	/// Has the tile been filpped horizontally?
	/// </summary>
	public bool flip;

	/// <summary>
	/// The rotation of the tile. 0 = no rotation, 1 = 90 degree rotation ect
	/// </summary>
	public int rotation;

	/// <summary>
	/// The index of the auto tile, or -1 if this tile does not belong to an auto tile
	/// </summary>
	public int autoTileIndex;

	/// <summary>
	/// Gets the UV of the bottom left of the tile without taking in to account the rotation and flip
	/// </summary>
	/// <returns>The U.</returns>
	/// <param name="tileInfo">Tile info.</param>
	public Vector2 GetBottomLeftUV ( TileInfo tileInfo ) {
		Material mat = tileInfo.GetComponent<Renderer>().sharedMaterial;
		//Vector2 uvBottomLeft = new Vector2( (float)(tiles[ y * mapWidth + x ].xIndex * (tileSize + spacing)) / mat.mainTexture.width, 1f - (float)((tiles[ y * mapWidth + x ].yIndex + 1)  * (float)(tileSize + spacing) - spacing) / (float)mat.mainTexture.height );
		Vector2 result = new Vector2( (float)(xIndex * ( tileInfo.tileSize + tileInfo.spacing )) / (float)mat.mainTexture.width,
		                       1f - (float)((yIndex + 1)  * (float)(tileInfo.tileSize + tileInfo.spacing) - tileInfo.spacing) / (float)mat.mainTexture.height );

		return result;
	}

	/// <summary>
	/// Gets the type of the collision according to the tileInfo.
	/// </summary>
	/// <returns>The collision type.</returns>
	/// <param name="tileInfo">Tile info.</param>
	public TileInfo.CollisionType GetCollisionType( TileInfo tileInfo ) {
		if( xIndex < 0 )
			return TileInfo.CollisionType.None;
		if( tileInfo == null
		   || tileInfo.collisions == null
		   || tileInfo.GetComponent<Renderer>() == null 
		   || tileInfo.GetComponent<Renderer>().sharedMaterial == null
		   || tileInfo.GetComponent<Renderer>().sharedMaterial.mainTexture == null )
			return TileInfo.CollisionType.None;
		return tileInfo.collisions[yIndex * Mathf.RoundToInt((float)tileInfo.GetComponent<Renderer>().sharedMaterial.mainTexture.width / (tileInfo.tileSize + tileInfo.spacing)) + xIndex];
	}
	
	public static explicit operator Vector2(Tile t) {
		return new Vector2( t.xIndex, t.yIndex );
	}
	
	public override bool Equals(System.Object obj) {
		// If parameter is null return false.
		if (obj == null) {
			return false;
		}

		Tile t = obj as Tile;
		
		// If parameter cannot be cast to Tile return false.
		if ((System.Object)t == null) {
			return false;
		}
		
		// Return true if the fields match:
		return ((xIndex == t.xIndex) && (yIndex == t.yIndex) && (rotation == t.rotation) && (flip == t.flip)) || (autoTileIndex == t.autoTileIndex && autoTileIndex != -1);
	}
	
	public bool Equals(Tile t) {
		// If parameter is null return false:
		if ((object)t == null) {
			return false;
		}
		
		// Return true if the fields match:
		return ((xIndex == t.xIndex) && (yIndex == t.yIndex) && (rotation == t.rotation) && (flip == t.flip)) || (autoTileIndex == t.autoTileIndex && autoTileIndex != -1);
	}

	public bool Equals( Vector2 v ) {
		return( xIndex == v.x ) && ( yIndex == v.y );
	}
	
	public override int GetHashCode() {
		return xIndex ^ yIndex ^ rotation ^ flip.GetHashCode();
	}

	public static bool operator ==(Tile lhs, Tile rhs) {
		return Equals(lhs, rhs);
	}

	public static bool operator ==(Vector2 lhs, Tile rhs) {
		return Equals(lhs, rhs);
	}
	public static bool operator ==(Tile lhs, Vector2 rhs) {
		return Equals(lhs, rhs);
	}
	public static bool operator !=(Vector2 lhs, Tile rhs) {
		return !Equals(lhs, rhs);
	}
	public static bool operator !=(Tile lhs, Vector2 rhs) {
		return !Equals(lhs, rhs);
	}
	
	public static bool operator !=(Tile lhs, Tile rhs) {
		return !Equals(lhs, rhs);
	}
}
