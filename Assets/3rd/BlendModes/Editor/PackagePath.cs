// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BlendModes
{
    public static class PackagePath
    {
        public static string PackageRootPath => GetPackageRootPath();
        public static string PackageMarkerPath => Path.Combine(cachedPackageRootPath, markerSearchPattern);
        public static string ResourcesPath => Path.Combine(PackageRootPath, "Resources");
        public static string EditorResourcesPath => Path.Combine(PackageRootPath, "EditorResources");
        public static string ShaderExtensionsPath => Path.Combine(PackageRootPath, "Shaders/Generated");
        public static string ShaderResourcesAssetPath => Path.Combine(ResourcesPath, "BlendModes/ShaderResources.asset");
        public static string ExtensionSourcesAssetPath => Path.Combine(EditorResourcesPath, "ExtensionSources.asset");

        private const string markerSearchPattern = "PackageMarker.com-elringus-blendmodes";
        private static string cachedPackageRootPath;

        public static string ToAssetsPath (string absolutePath)
        {
            absolutePath = absolutePath.Replace("\\", "/");
            return "Assets" + absolutePath.Replace(Application.dataPath, string.Empty);
        }

        public static void CreateDirectoryAsset (string fullDirectoryPath)
        {
            var assetPath = ToAssetsPath(fullDirectoryPath);
            EnsureFolderIsCreatedRecursively(assetPath);
        }

        private static string GetPackageRootPath ()
        {
            if (string.IsNullOrEmpty(cachedPackageRootPath) || !File.Exists(PackageMarkerPath))
            {
                var marker = Directory.GetFiles(Application.dataPath, markerSearchPattern, SearchOption.AllDirectories).FirstOrDefault();
                if (marker == null) { Debug.LogError("Can't find package marker file."); return null; }
                cachedPackageRootPath = Directory.GetParent(marker).Parent.FullName;
            }
            return cachedPackageRootPath;
        }

        private static void EnsureFolderIsCreatedRecursively (string targetFolder)
        {
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                EnsureFolderIsCreatedRecursively(Path.GetDirectoryName(targetFolder));
                AssetDatabase.CreateFolder(Path.GetDirectoryName(targetFolder), Path.GetFileName(targetFolder));
            }
        }
    }
}
