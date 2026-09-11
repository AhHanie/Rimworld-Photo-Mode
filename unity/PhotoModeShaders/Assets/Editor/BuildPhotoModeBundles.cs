using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildPhotoModeBundles
{
    private static readonly string[] ShaderAssetPaths =
    {
        "Assets/Data/sk.photomode/Materials/PhotoMode/OnePass.shader",
        "Assets/Data/sk.photomode/Materials/PhotoMode/Bloom.shader",
        "Assets/Data/sk.photomode/Materials/PhotoMode/SelectiveBlur.shader"
    };

    private const string BundleName = "photomode";
    private const string OutputRootDirectory = "Build";

    public static void Build()
    {
        AssetDatabase.Refresh();

        for (int i = 0; i < ShaderAssetPaths.Length; i++)
        {
            string shaderAssetPath = ShaderAssetPaths[i];
            AssetImporter importer = AssetImporter.GetAtPath(shaderAssetPath);
            if (importer == null)
            {
                Debug.LogError("BuildPhotoModeBundles: no asset found at " + shaderAssetPath);
                return;
            }

            importer.SetAssetBundleNameAndVariant(BundleName, string.Empty);
            importer.SaveAndReimport();
        }

        BuildForTarget(BuildTarget.StandaloneWindows64, "win");
        BuildForTarget(BuildTarget.StandaloneOSX, "mac");
        BuildForTarget(BuildTarget.StandaloneLinux64, "linux");
    }

    private static void BuildForTarget(BuildTarget target, string osSuffix)
    {
        string outputDirectory = Path.Combine(OutputRootDirectory, target.ToString());

        try
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputDirectory, BuildAssetBundleOptions.None, target);
            if (manifest == null)
            {
                Debug.LogError("BuildPhotoModeBundles: build for " + target + " returned no manifest (target support likely missing).");
                return;
            }

            string builtBundlePath = Path.Combine(outputDirectory, BundleName);
            string finalPath = Path.Combine(OutputRootDirectory, BundleName + "_" + osSuffix);
            File.Copy(builtBundlePath, finalPath, overwrite: true);

            Debug.Log("BuildPhotoModeBundles: built " + finalPath + " for " + target + ".");
        }
        catch (Exception exception)
        {
            Debug.LogError("BuildPhotoModeBundles: failed to build for " + target + ": " + exception);
        }
    }
}
