﻿using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct KeyframeTpl<T> : IAssetReadable, IYAMLExportable
		where T : struct, IAssetReadable, IYAMLExportable
	{
		public KeyframeTpl(float time, T value, T weight)
		{
			Time = time;
			Value = value;
			InSlope = default;
			OutSlope = default;
			WeightedMode = WeightedMode.None;
			InWeight = weight;
			OutWeight = weight;
		}

		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public bool IsReadWeight(Version version)
		{
			return version.IsGreaterEqual(2018);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				//return 3;
				return 2;
			}

			/*if(version.IsGreaterEqual(2018))
			{
				return 3;
			}*/
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();
			Value.Read(stream);
			InSlope.Read(stream);
			OutSlope.Read(stream);
			if(IsReadWeight(stream.Version))
			{
				WeightedMode = (WeightedMode)stream.ReadInt32();
				InWeight.Read(stream);
				OutWeight.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("time", Time);
			node.Add("value", Value.ExportYAML(container));
			node.Add("inSlope", InSlope.ExportYAML(container));
			node.Add("outSlope", OutSlope.ExportYAML(container));
			if (GetSerializedVersion(container.Version) >= 2)
			{
#warning TODO: value?
				node.Add("tangentMode", 0);
			}
			if (GetSerializedVersion(container.Version) >= 3)
			{
				node.Add("weightedMode", (int)WeightedMode);
				node.Add("inWeight", InWeight.ExportYAML(container));
				node.Add("outWeight", OutWeight.ExportYAML(container));
			}
			return node;
		}

		public float Time { get; private set; }
		public WeightedMode WeightedMode { get; private set; }

		public T Value;
		public T InSlope;
		public T OutSlope;
		public T InWeight;
		public T OutWeight;
	}
}
