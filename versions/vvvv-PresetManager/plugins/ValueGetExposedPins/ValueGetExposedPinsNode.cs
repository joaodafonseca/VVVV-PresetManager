#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Text;
using VVVV.Core.Logging;
#endregion usings


namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "GetExposedPins", Category = "Value")]
	#endregion PluginInfo
	public class ValueGetExposedPinsNode : IPluginEvaluate
	{
		#region fields & pins
		
		[Input("Exposed Pins")]
		public ISpread<string> exposedPins;
		
		[Input("NodeSubType")]
		public ISpread<String> nodeSubType;
		
		[Input("Name")]
		public ISpread<String> name;
		
		[Input("ReceiveNewValue")]
		public ISpread<bool> receiveNewValue;
		
		
		
		
		[Output("Exposed Nodes")]
		public ISpread<String> exposedNodes;
		
//		/[Output("Update Exposed Nodes")]
		//bool updateNodes;
		
		[Import()]
		public ILogger FLogger;
		
		[Import()]
		IHDEHost FHDEHost;
		#endregion fields & pins
		
		
	
		
		
		
		public String RetrieveType(String subType)
		{
			
			String type = subType;
			String[] SplitString = type.Split(',');
			type = SplitString[0];
			
			if (SplitString[0] == "Endless") {
				
				String t = "int";
				if (SplitString[5].Contains("."))
				t = "float";
				type += ":" + t;
			}
			
			return type;
		}
		
		
		
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			exposedNodes.SliceCount = exposedPins.Count;
		
			
			for (int i = 0; i < exposedPins.Count; i++) {
				exposedNodes[i] ="";
			
				if(receiveNewValue[0]){
				
				var nodePath = exposedPins[i].Substring(0, exposedPins[i].LastIndexOf('/'));
				var node = FHDEHost.GetNodeFromPath(nodePath);
				
				
				
				//
			//	if (exposedPins[i]) {
					if (node != null) {
						
						var parts = exposedPins[i].Split('/');
						var pin = node.FindPin(parts[parts.Length - 1]);
						
               // FLogger.Log(LogType.Debug, oldValues.Equals(pin.Spread).ToString());
					//if(receiveNewValue[0]){
						
							
							//FValues[i] = pin.Spread;
							string nodeID = parts[parts.Length - 2];
							
							
							//string nodeType
							
							if (pin.Spread.Contains("|")) {
								string trimValue = pin.Spread.Substring(1, pin.Spread.Length - 2);
								
								//		trimValue.Remove(',');
								
								string[] nodeValues = trimValue.Split(new string[] { "|,|" }, StringSplitOptions.None);
								
								
								string values = "";
								
								
								for(int n = 0; n < nodeValues.Length; n++){
									
									values+=nodeValues[n];
									if(n<nodeValues.Length-1)values+=";";
								}
								
								
								
								//var.Add(new Variable(name[i], nodeID, RetrieveType(nodeSubType[i]), colorValues));
								
								exposedNodes[i] = name[i] + "/" + nodeID + "/" + RetrieveType(nodeSubType[i]) + "/" + values;
							} else {
								string[] nodeValues = pin.Spread.Split(',');
								
								string values = "";
								
								for(int n = 0; n < nodeValues.Length; n++){
									
									values+=nodeValues[n];
									if(n<nodeValues.Length-1)values+=";";
								}
								//var.Add(new Variable(name[i], nodeID, RetrieveType(nodeSubType[i]), nodeValues));
								exposedNodes[i] = name[i] + "/" + nodeID + "/" + RetrieveType(nodeSubType[i]) + "/" + values;
							}
							
						
							
						//}
						//oldValues = pin.Spread;
						
					}
				
				
				}
			}
			
		}
	}
}