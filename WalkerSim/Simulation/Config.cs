﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WalkerSim
{
	public class Config
	{
		public int UpdateInterval { get; private set; }
		public int SpinupTicks { get; private set; }
		public bool Persistent { get; private set; }
		public string ZombieGroup { get; private set; }
		public int WorldZoneDivider { get; private set; }
		public float POITravellerChance { get; private set; }
		public int PopulationDensity { get; private set; }
		public bool IncludeSleepers { get; private set; }
		public bool EnableViewServer { get; private set; }
		public int ViewServerPort { get; private set; }

		public Config()
		{
			UpdateInterval = 60;
			SpinupTicks = 10000;
			ZombieGroup = "ZombiesAll";
			WorldZoneDivider = 32;
			POITravellerChance = 0.25f;
			PopulationDensity = 25;
			IncludeSleepers = true;
			Persistent = true;
#if DEBUG
			EnableViewServer = true;
#else
			EnableViewServer = false;
#endif
			ViewServerPort = 13632;
		}

		public bool Load(string configFile)
		{
			try
			{
				Log.Out("[WalkerSim] Loading configuration...");

				XmlDocument doc = new XmlDocument();
				doc.Load(configFile);

				XmlNode nodeConfig = doc.DocumentElement;
				if (nodeConfig == null || nodeConfig.Name != "WalkerSim")
				{
					Log.Error("Invalid xml configuration format, unable to load config.");
					return false;
				}
				foreach (XmlNode node in nodeConfig.ChildNodes)
				{
					if (node.Name == "#comment")
						continue;
					ProcessNode(node);
				}
			}
			catch (Exception ex)
			{
				Log.Error("[WalkerSim] Unable to load {0} configuration", configFile);
				Log.Exception(ex);
				return false;
			}
			return true;
		}

		private void ProcessNode(XmlNode node)
		{
			switch (node.Name)
			{
				case "UpdateInterval":
					UpdateInterval = int.Parse(node.InnerText);
					Log.Out("[WalkerSim] {0} = {1}", "UpdateInterval", UpdateInterval);
					break;
				case "SpinupTicks":
					SpinupTicks = int.Parse(node.InnerText);
					Log.Out("[WalkerSim] {0} = {1}", "SpinupTicks", SpinupTicks);
					break;
				case "Persistent":
					Persistent = node.InnerText.ToLower() == "true";
					Log.Out("[WalkerSim] {0} = {1}", "Persistent", Persistent);
					break;
				case "WorldZoneDivider":
					WorldZoneDivider = int.Parse(node.InnerText);
					Log.Out("[WalkerSim] {0} = {1}", "WorldZoneDivider", WorldZoneDivider);
					break;
				case "POITravellerChance":
					POITravellerChance = float.Parse(node.InnerText);
					Log.Out("[WalkerSim] {0} = {1}", "POITravellerChance", POITravellerChance);
					break;
				case "PopulationDensity":
					PopulationDensity = int.Parse(node.InnerText);
					Log.Out("[WalkerSim] {0} = {1}", "PopulationDensity", PopulationDensity);
					break;
				case "IncludeSleepers":
					IncludeSleepers = node.InnerText.ToLower() == "true";
					Log.Out("[WalkerSim] {0} = {1}", "IncludeSleepers", IncludeSleepers);
					break;
#if !DEBUG
				case "ViewServer":
					EnableViewServer = node.InnerText.ToLower() == "true";
					Log.Out("[WalkerSim] {0} = {1}", "ViewServer", EnableViewServer);
					break;
#endif
				case "ViewServerPort":
					ViewServerPort = int.Parse(node.InnerText);
					Log.Out("[WalkerSim] {0} = {1}", "ViewServerPort", ViewServerPort);
					break;
			}
		}
	}
}
