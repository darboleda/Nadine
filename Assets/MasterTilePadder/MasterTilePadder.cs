//#define HAS_NGUI
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
public class MasterTilePadder:EditorWindow {
	[MenuItem("Window/MasterTilePadder")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(MasterTilePadder));
	}
	
	Texture2D _baseImage = null;
	int _tileSize = 32;
	int _paddingPixels = 4;
	bool _padEdge = false;
#if HAS_NGUI
	bool _createNGUIAtlas = false;
	Texture2D _paddedImage = null;
#endif
	
	void OnGUI() {
		GUILayout.Label ("Input Texture Settings", EditorStyles.boldLabel);
		_baseImage = (Texture2D)EditorGUILayout.ObjectField("Base Tile Image", _baseImage, typeof(Texture2D), true);
		
		_tileSize = EditorGUILayout.IntField("Tile Size", _tileSize);
		if (8>_tileSize) {
			_tileSize = 8;
		}
		_paddingPixels = EditorGUILayout.IntField("Padding", _paddingPixels);
		if (0>_paddingPixels) {
			_paddingPixels =0;
		}
		_padEdge = EditorGUILayout.Toggle("Pad Outer Edge", _padEdge);
#if HAS_NGUI
		_createNGUIAtlas = EditorGUILayout.Toggle("NGUI Atlas", _createNGUIAtlas);	
#endif
		if (GUILayout.Button("Generate Padded Texture")) {
			CreatePadded();
		}
	}
	
	void CreatePadded() {
		if (null!=_baseImage) {
			string baseImagePath = AssetDatabase.GetAssetPath(_baseImage);
			string directoryPath = Path.GetDirectoryName(baseImagePath);
			string resultFileName = string.Format("{0}/{1}_padded.png", directoryPath, _baseImage.name);
			bool goAhead = true;
			if (File.Exists(resultFileName)) {
				goAhead = EditorUtility.DisplayDialog("Warning", string.Format("A file named {0} already exists. Overwrite?", resultFileName), "Ok", "Cancel");
			}
			
			if (FileNotReadable(baseImagePath)) {
				goAhead = EditorUtility.DisplayDialog("Warning", string.Format("The texture you have selected is currently not readable from code. Change settings to be readable?"), "Ok", "Cancel");
				if (goAhead) {
					MakeTextureReadable(baseImagePath);
					AssetDatabase.Refresh();
				}
			}
			
			if (goAhead) {
				int tileWidth = _baseImage.width/_tileSize;
				int resultWidth = 2*_paddingPixels*(tileWidth-1)+tileWidth*_tileSize;
				int tileHeight = _baseImage.height/_tileSize;
				int resultHeight = 2*_paddingPixels*(tileHeight-1)+tileHeight*_tileSize;
				if (_padEdge) {
					resultWidth += 2*_paddingPixels;
					resultHeight += 2*_paddingPixels;
				}
				Texture2D newImage = new Texture2D(resultWidth, resultHeight, TextureFormat.ARGB32, false);
				for (int tileY=0; tileY<tileHeight; ++tileY) {
					for (int tileX=0; tileX<tileWidth; ++tileX) {
						CopyTile(tileX, tileY, tileWidth, tileHeight, _baseImage, newImage);
					}
				}
	        	byte[] bytes = newImage.EncodeToPNG();
	        	File.WriteAllBytes(resultFileName, bytes);
				AssetDatabase.Refresh();
				ConfigureTextureImportSettings(resultFileName);
				AssetDatabase.Refresh();
				EditorUtility.DisplayDialog("Success", string.Format("File \"{0}/{1}_padded.png\" was generated.", directoryPath, _baseImage.name), "Ok");
#if HAS_NGUI
				if (_createNGUIAtlas) {
					CreatePaddedNGUIAtlas(directoryPath, resultFileName);
					AssetDatabase.Refresh();
					EditorUtility.DisplayDialog("Success", string.Format("NGUI Atlas \"{0}/{1}_padded\" was also generated.", directoryPath, _baseImage.name), "Ok");
				}
#endif
			}
		}
		else {
			EditorUtility.DisplayDialog("Texture Unselected", "Select a tiled texture first.", "Ok");
		}
	}
	
	protected void CopyTile(int tileX, int tileY, int tileWidthNum, int tileHeightNum, Texture2D sourceTexture, Texture2D destTexture) {
		int offsetX = 2*_paddingPixels*(tileX);
		int offsetY = 2*_paddingPixels*(tileY);
		if (_padEdge) {
			offsetX += _paddingPixels;
			offsetY += _paddingPixels;
		}
		
		// The current destination tile's reference point
		int pivoxX = offsetX+_tileSize*tileX;
		int pivotY = offsetY+_tileSize*tileY;
		
		// The current source tile's reference point
		int srcPivotX = _tileSize*tileX;
		int srcPivotY = _tileSize*tileY;
		
		// Below
		if (_padEdge || 0<tileY) {
			for (int j=0; j<_tileSize; ++j) {
				for (int padding=0; padding>-_paddingPixels;--padding) {
					destTexture.SetPixel(pivoxX+j, pivotY+padding-1, sourceTexture.GetPixel(srcPivotX+j, srcPivotY));
				}
			}
		}
		// Above
		if (_padEdge || tileHeightNum-1>tileY) {
			for (int j=0; j<_tileSize; ++j) {
				for (int padding=0; padding<_paddingPixels;++padding) {
					destTexture.SetPixel(pivoxX+j, pivotY+_tileSize+padding, sourceTexture.GetPixel(srcPivotX+j, srcPivotY+_tileSize-1));
				}
			}
		}
		// Left
		if (_padEdge || 0<tileX) {
			for (int i=0; i<_tileSize; ++i) {
				for (int padding=0; padding>-_paddingPixels;--padding) {
					destTexture.SetPixel(pivoxX+padding-1, pivotY+i, sourceTexture.GetPixel(srcPivotX, srcPivotY+i));
				}
			}
		}
		// Right
		if (_padEdge || tileWidthNum-1>tileX) {
			for (int i=0; i<_tileSize; ++i) {
				for (int padding=0; padding<_paddingPixels;++padding) {
					destTexture.SetPixel(pivoxX+_tileSize+padding, pivotY+i, sourceTexture.GetPixel(srcPivotX+_tileSize-1, srcPivotY+i));
				}
			}
		}
		
		// Below Left
		if (_padEdge || (0<tileY && 0<tileX)) {
			for (int paddingY=0; paddingY>-_paddingPixels; --paddingY) {
				for (int paddingX=0; paddingX>-_paddingPixels; --paddingX) {
					destTexture.SetPixel(pivoxX+paddingX-1, pivotY+paddingY-1, sourceTexture.GetPixel(srcPivotX, srcPivotY));
				}
			}
		}
		
		
		// Below Right
		if (_padEdge || (0<tileY && tileWidthNum-1>tileX)) {
			for (int paddingY=0; paddingY>-_paddingPixels; --paddingY) {
				for (int paddingX=0; paddingX<_paddingPixels; ++paddingX) {
					destTexture.SetPixel(pivoxX+_tileSize+paddingX, pivotY+paddingY-1, sourceTexture.GetPixel(srcPivotX+_tileSize-1, srcPivotY));
				}
			}
		}
		
		// Bottom Left
		if (_padEdge || (tileHeightNum-1>tileY && 0<tileX)) {
			for (int paddingY=0; paddingY<_paddingPixels; ++paddingY) {
				for (int paddingX=0; paddingX>-_paddingPixels; --paddingX) {
					destTexture.SetPixel(pivoxX+paddingX-1, pivotY+_tileSize+paddingY, sourceTexture.GetPixel(srcPivotX, srcPivotY+_tileSize-1));
				}
			}
		}
		
		// Bottom Right
		if (_padEdge || (tileHeightNum-1>tileY && tileWidthNum-1>tileX)) {
			for (int paddingY=0; paddingY<_paddingPixels; ++paddingY) {
				for (int paddingX=0; paddingX<_paddingPixels; ++paddingX) {
					destTexture.SetPixel(pivoxX+_tileSize+paddingX, pivotY+_tileSize+paddingY, sourceTexture.GetPixel(srcPivotX+_tileSize-1, srcPivotY+_tileSize-1));
				}
			}
		}
		
		// Middle
		for (int i=0; i<_tileSize; ++i) {
			for (int j=0; j<_tileSize; ++j) {
				destTexture.SetPixel(pivoxX+j, pivotY+i,  sourceTexture.GetPixel(srcPivotX+j, srcPivotY+i));
			}
		}
	}
	
	bool FileNotReadable(string textureFileName) {
		if (string.IsNullOrEmpty(textureFileName)) {
			throw new System.Exception("Bad texture file name: "+textureFileName);
		}
		TextureImporter textureImporter = AssetImporter.GetAtPath(textureFileName) as TextureImporter;
		if (textureImporter == null) {
			throw new System.Exception("Failed to open file name: "+textureFileName);
		}

		TextureImporterSettings settings = new TextureImporterSettings();
		textureImporter.ReadTextureSettings(settings);
		
		return !settings.readable;
	}
	
	bool MakeTextureReadable(string textureFileName) {
		if (string.IsNullOrEmpty(textureFileName)) {
			throw new System.Exception("Bad texture file name: "+textureFileName);
		}
		
		TextureImporter textureImporter = AssetImporter.GetAtPath(textureFileName) as TextureImporter;
		if (textureImporter == null) {
			throw new System.Exception("Failed to open file name: "+textureFileName);
		}

		TextureImporterSettings settings = new TextureImporterSettings();
		textureImporter.ReadTextureSettings(settings);

		if (!settings.readable) {
			settings.readable = true;
			textureImporter.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(textureFileName, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			return true;
		}
		else {
			return false;
		}
	}
	
	bool ConfigureTextureImportSettings(string textureFileName) {
		if (string.IsNullOrEmpty(textureFileName)) {
			throw new System.Exception("Bad texture file name: "+textureFileName);
		}
		
		TextureImporter textureImporter = AssetImporter.GetAtPath(textureFileName) as TextureImporter;
		if (textureImporter == null) {
			throw new System.Exception("Failed to open file name: "+textureFileName);
		}

		TextureImporterSettings settings = new TextureImporterSettings();
		textureImporter.ReadTextureSettings(settings);

		if (!settings.readable
			|| !settings.mipmapEnabled
			|| settings.maxTextureSize >= 4096
			|| settings.filterMode != FilterMode.Trilinear
			|| settings.wrapMode != TextureWrapMode.Clamp
			|| settings.npotScale != TextureImporterNPOTScale.None
			|| settings.aniso != 3)
		{
			settings.readable = true;
			settings.ApplyTextureType(TextureImporterType.Advanced, true);
			settings.mipmapEnabled = true;
			settings.maxTextureSize = 4096;
			settings.textureFormat = TextureImporterFormat.ARGB32;
			settings.filterMode = FilterMode.Trilinear;
			settings.wrapMode = TextureWrapMode.Clamp;
			settings.npotScale = TextureImporterNPOTScale.None;
			settings.aniso = 3;
			
			textureImporter.SetTextureSettings(settings);
			AssetDatabase.ImportAsset(textureFileName, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			return true;
		}
		else {
			return false;
		}
	}
	
#if HAS_NGUI
	private void CreatePaddedNGUIAtlas(string directoryName, string textureFileName) {
		string materialPath = string.Format("{0}/{1}.mat", directoryName, Path.GetFileNameWithoutExtension(textureFileName));
		string prefabPath = string.Format("{0}/{1}.prefab", directoryName, Path.GetFileNameWithoutExtension(textureFileName));
		
		
		// Check if material exists. If not, create it.
		Material material = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
		if (null==material) {
			material = new Material(Shader.Find("Unlit/Transparent Colored"));

			// Save the material
			AssetDatabase.CreateAsset(material, materialPath);
			AssetDatabase.Refresh();

			// Load the material so it's usable
			material = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
		}
		
		// Set the texture of the new material and save it
		Texture2D tex = AssetDatabase.LoadAssetAtPath(textureFileName, typeof(Texture2D)) as Texture2D;
		material.mainTexture = tex;
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		// Create a prefab if not found
		GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(prefabPath);
		
		// Create a new game object for the atlas
		go = new GameObject("PaddedTexture");
		go.AddComponent<UIAtlas>().spriteMaterial = material;
		PrefabUtility.ReplacePrefab(go, prefab);
		DestroyImmediate(go);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		
		go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
		CreatePaddedTileSprites(go.GetComponent<UIAtlas>());
	}
	
	void CreatePaddedTileSprites(UIAtlas atlas) {
		Texture2D tex = atlas.spriteMaterial.mainTexture as Texture2D;
		int rowLength = 1+(tex.width-_tileSize)/(_tileSize+2*_paddingPixels);
		int columnLength = 1+(tex.height-_tileSize)/(_tileSize+2*_paddingPixels);
		int tileNum = rowLength*columnLength;
		List<UIAtlas.Sprite> oldSprites = atlas.spriteList;
		atlas.spriteList = new List<UIAtlas.Sprite>();
		
		for (int i=0; i<tileNum; ++i) {
			UIAtlas.Sprite newSprite = new UIAtlas.Sprite();
			newSprite.name = (i+1).ToString();
			
			int tileNumX = i%rowLength;
			int tileNumY = i/rowLength;
			int frameX = tileNumX*_tileSize+Mathf.Max(0, (tileNumX)*(2*_paddingPixels));
			int frameY = tileNumY*_tileSize+Mathf.Max(0, (tileNumY)*(2*_paddingPixels));

			// Not rotated
			newSprite.rotated = false;
			
			// Set coordinates
			newSprite.outer = new Rect(frameX, frameY, _tileSize, _tileSize);
			newSprite.inner = new Rect(frameX, frameY, _tileSize, _tileSize);


			// Add this new sprite
			atlas.spriteList.Add(newSprite);
		}

		// Unload the asset
		Resources.UnloadUnusedAssets();
		atlas.MarkAsDirty();
	}
#endif
}
