using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public enum PhotoLutStatus
    {
        None,
        Loaded,
        Missing,
        Invalid
    }

    public readonly struct PhotoLutOption
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly string FilePath;
        public readonly bool IsCustom;

        public PhotoLutOption(string id, string displayName, string filePath, bool isCustom)
        {
            Id = id;
            DisplayName = displayName;
            FilePath = filePath;
            IsCustom = isCustom;
        }
    }

    public static class PhotoLutLibrary
    {
        private const string IncludedContentPath = "LUTs";
        public const string IncludedPrefix = "Included/";
        public const string CustomPrefix = "Custom/";

        private const int MinLutSize = 2;
        private const int MaxLutSize = 64;

        public static string CustomLutsFolderPath => Path.Combine(GenFilePaths.ConfigFolderPath, "PhotoModeLuts");

        public static List<PhotoLutOption> ListIncluded()
        {
            List<PhotoLutOption> options = new List<PhotoLutOption>();

            ModContentPack content = LoadedModManager.GetMod<Mod>()?.Content;
            if (content == null)
            {
                return options;
            }

            Dictionary<string, FileInfo> files = ModContentPack.GetAllFilesForMod(content, IncludedContentPath, IsPngExtension);
            foreach (KeyValuePair<string, FileInfo> entry in files)
            {
                string name = Path.GetFileNameWithoutExtension(entry.Value.Name);
                options.Add(new PhotoLutOption(IncludedPrefix + name, name, entry.Value.FullName, isCustom: false));
            }

            options.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
            return options;
        }

        public static List<PhotoLutOption> ListCustom()
        {
            List<PhotoLutOption> options = new List<PhotoLutOption>();

            string folder = CustomLutsFolderPath;
            if (!Directory.Exists(folder))
            {
                return options;
            }

            string[] files = Directory.GetFiles(folder, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileNameWithoutExtension(files[i]);
                options.Add(new PhotoLutOption(CustomPrefix + name, name, files[i], isCustom: true));
            }

            options.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
            return options;
        }

        public static List<PhotoLutOption> ListAll()
        {
            List<PhotoLutOption> options = ListIncluded();
            options.AddRange(ListCustom());
            return options;
        }

        public static bool Exists(string id)
        {
            return TryFind(id, out _);
        }

        public static bool TryFind(string id, out PhotoLutOption option)
        {
            option = default;

            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            List<PhotoLutOption> candidates = id.StartsWith(CustomPrefix, StringComparison.Ordinal) ? ListCustom() : ListIncluded();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].Id == id)
                {
                    option = candidates[i];
                    return true;
                }
            }

            return false;
        }

        public static PhotoLutStatus TryLoad(string id, out Texture2D texture)
        {
            texture = null;

            if (!TryFind(id, out PhotoLutOption option) || !File.Exists(option.FilePath))
            {
                return PhotoLutStatus.Missing;
            }

            byte[] bytes = File.ReadAllBytes(option.FilePath);
            Texture2D candidate = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            bool decoded = candidate.LoadImage(bytes, markNonReadable: true);
            if (!decoded)
            {
                UnityEngine.Object.Destroy(candidate);
                return PhotoLutStatus.Invalid;
            }

            int size = candidate.height;
            if (size < MinLutSize || size > MaxLutSize || candidate.width != size * size)
            {
                UnityEngine.Object.Destroy(candidate);
                return PhotoLutStatus.Invalid;
            }

            candidate.filterMode = FilterMode.Bilinear;
            candidate.wrapMode = TextureWrapMode.Clamp;

            texture = candidate;
            return PhotoLutStatus.Loaded;
        }

        public static void OpenCustomFolder()
        {
            Directory.CreateDirectory(CustomLutsFolderPath);
            string fullPath = Path.GetFullPath(CustomLutsFolderPath);

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                Application.OpenURL(fullPath);
                return;
            }

            Find.WindowStack.Add(new Dialog_MessageBox(fullPath));
        }

        private static bool IsPngExtension(string extension)
        {
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase);
        }
    }
}
