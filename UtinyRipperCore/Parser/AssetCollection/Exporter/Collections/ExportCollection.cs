﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters
{
	public abstract class ExportCollection : IExportCollection
	{
		static ExportCollection()
		{
			string invalidChars = new string(Path.GetInvalidFileNameChars());
			string escapedChars = Regex.Escape(invalidChars);
			FileNameRegex = new Regex($"[{escapedChars}]");
		}

		public static string GetMainExportID(Object asset)
		{
			return $"{(int)asset.ClassID}00000";
		}

		public abstract bool Export(ProjectAssetContainer container, string dirPath);
		public abstract bool IsContains(Object asset);
		public abstract string GetExportID(Object asset);
		public abstract ExportPointer CreateExportPointer(Object asset, bool isLocal);
		
		protected void ExportAsset(ProjectAssetContainer container, IAssetImporter importer, Object asset, string path, string name)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			string filePath = $"{Path.Combine(path, name)}.{asset.ExportExtension}";
			AssetExporter.Export(container, asset, filePath);
			Meta meta = new Meta(importer, asset.GUID);
			ExportMeta(container, meta, filePath);
		}

		protected void ExportMeta(ProjectAssetContainer container, Meta meta, string filePath)
		{
			string metaPath = $"{filePath}.meta";
			using (FileStream fileStream = File.Open(metaPath, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					YAMLDocument doc = meta.ExportYAMLDocument(container);
					writer.IsWriteDefaultTag = false;
					writer.IsWriteVersion = false;
					writer.AddDocument(doc);
					writer.Write(streamWriter);
				}
			}
		}
		
		protected string GetUniqueFileName(Object @object, string dirPath)
		{
			string fileName;
			switch (@object)
			{
				case NamedObject named:
					fileName = named.Name;
					break;
				case Prefab prefab:
					fileName = prefab.GetName();
					break;

				default:
					fileName = @object.GetType().Name;
					break;
			}
			fileName = FileNameRegex.Replace(fileName, string.Empty);

			fileName = DirectoryUtils.GetMaxIndexName(dirPath, fileName);
			fileName = $"{fileName}.{@object.ExportExtension}";
			return fileName;
		}

		public abstract IAssetExporter AssetExporter { get; }
		public abstract IEnumerable<Object> Objects { get; }
		public abstract string Name { get; }

		private static readonly Regex FileNameRegex;
	}
}
