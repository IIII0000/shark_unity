using UnityEngine;
using System.IO;
using System;
using UnityEditor;

public class ExportPngFile: MonoBehaviour
{
    [MenuItem("Tools/转换asset图片为png")]
    public static void ExportImageFile()
    {
        try
        {
            string assetpath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Path.GetExtension(assetpath) == ".asset")
            {
                ExportPng(assetpath);
            }
            else
            {
                foreach (string path in Directory.GetFiles(assetpath))
                {
                    if (Path.GetExtension(path) == ".asset")
                    {
                        ExportPng(path);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + e.StackTrace);
        }
    }

    private static void ExportPng(string srcPath)
    {
        // Debug.Log(srcPath);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(srcPath);
        if (sprite == null)
        {
            return;
        }

        Texture2D texture = sprite.texture;
        string assetPath = AssetDatabase.GetAssetPath(texture);
        var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.textureType = TextureImporterType.Default;
            tImporter.isReadable = true;
            AssetDatabase.ImportAsset(assetPath);
            tImporter.SaveAndReimport();
            AssetDatabase.Refresh();
        }

        string parentPath = Directory.GetParent(srcPath).FullName;
        string exportedPath = parentPath + "/exported/";
        if (!Directory.Exists(exportedPath))
        {
            Directory.CreateDirectory(exportedPath);
        }

        string fileName = exportedPath + "/" + sprite.name + ".png";
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        byte[] bytes = ToPNG(sprite);
        File.WriteAllBytes(fileName, bytes);
    }

    private static byte[] ToPNG(Sprite sprite)
    {
        Texture2D pSource = sprite.texture;
        Rect r = sprite.textureRect;
        int left = (int)r.x, top = (int)r.y, width = (int)r.width, height = (int)r.height;
        if (left < 0)
        {
            width += left;
            left = 0;
        }

        if (top < 0)
        {
            height += top;
            top = 0;
        }

        if (left + width > pSource.width)
        {
            width = pSource.width - left;
        }

        if (top + height > pSource.height)
        {
            height = pSource.height - top;
        }

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        Color[] aSourceColor = pSource.GetPixels(0);

        //*** Make New
        Texture2D subtex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        //*** Make destination array
        int xLength = width * height;
        Color[] aColor = new Color[xLength];

        int i = 0;
        for (int y = 0; y < height; y++)
        {
            int sourceIndex = (y + top) * pSource.width + left;
            for (int x = 0; x < width; x++)
            {
                aColor[i++] = aSourceColor[sourceIndex++];
            }
        }

        //*** Set Pixels
        subtex.SetPixels(aColor);
        subtex.Apply();
        byte[] bytes = subtex.EncodeToPNG();
        return bytes;
    }
}