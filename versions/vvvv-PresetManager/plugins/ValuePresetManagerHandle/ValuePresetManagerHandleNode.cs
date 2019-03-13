#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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
	[PluginInfo(Name = "PresetManagerHandle", Category = "Value")]
	#endregion PluginInfo
	public class ValuePresetManagerHandleNode : IPluginEvaluate
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
		
		[Input("Load")]
		public ISpread<bool> load;
		
		[Input("Preset")]
		public ISpread<String> preset;
		
		[Output("NodeName")]
		public ISpread<String> nodeName;
		
		[Output("NodeID")]
		public ISpread<String> nodeID;
		
		[Output("NodeType")]
		public ISpread<String> nodeType;
		
		[Output("NodeBinSize")]
		public ISpread<String> nodeBinSize;
		
		[Output("NodeValue")]
		public ISpread<String> nodeValue;
		
		
		[Import()]
		public ILogger FLogger;
		
		[Import()]
		IHDEHost FHDEHost;
		#endregion fields & pins
		
		
		List<Variable> var = new List<Variable>();
		
		
		public void getExposedPinSettings(){
			var.Clear();
			FLogger.Log(LogType.Debug,"ADDING EXPOSED PINS");
			
			for (int i = 0; i < exposedPins.Count; i++) {
				var nodePath = exposedPins[i].Substring(0, exposedPins[i].LastIndexOf('/'));
				var node = FHDEHost.GetNodeFromPath(nodePath);
				
				
				
				//	if (exposedPins[i]) {
					if (node != null) {
						
						var parts = exposedPins[i].Split('/');
						var pin = node.FindPin(parts[parts.Length - 1]);
						
						//FValues[i] = pin.Spread;
						string nodeID = parts[parts.Length - 2];
						
						FLogger.Log(LogType.Debug,parts[parts.Length - 2]);
						//string nodeType
						
						if(pin.Spread.Contains("|")){
							string trimValue = pin.Spread.Substring(1, pin.Spread.Length - 2);
							
							//		trimValue.Remove(',');
							
							string[] nodeValues = trimValue.Split('|');
							
						 	
							List<String> colorValues = new List<String>();
							
							for(int v = 0; v < nodeValues.Length; v++){
								
								if(nodeValues[v]!=",")colorValues.Add(nodeValues[v]);
							}
							
							var.Add(new Variable(name[i],nodeID, RetrieveType(nodeSubType[i]), colorValues));
						}else{
							string[] nodeValues = pin.Spread.Split(',');
							var.Add(new Variable(name[i],nodeID, RetrieveType(nodeSubType[i]), nodeValues));
						}
						
						
					}
				//}
			}
			
		}
		
		
		public void loadPreset(){
			
			
			FLogger.Log(LogType.Debug,"Loading Preset");
			
			for(int i = 0; i < var.Count; i++){
				
				for(int p = 0; p < preset.Count; p++){
					
					String[] pre = preset[p].Split(':');
					String nodeID = pre[4];

					if(nodeID == var[i].ID){

						String[] pv = pre[3].Split(';');
						var[i].updateValue( pv);	
					}
				}
			}
		}
		
		public String RetrieveType(String subType){
			
			String type = subType;
			String[] SplitString = type.Split(',');
			type = SplitString[0];
			
			if(SplitString[0]=="Endless"){
				
				String t = "int";
				if(SplitString[5].Contains("."))t="float";
				type+=":"+t;
			}
			
			return type;
		}
		
		
		
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(var.Count != exposedPins.Count || receiveNewValue[0])getExposedPinSettings();
			if(load[0])loadPreset();
			
			nodeName.SliceCount = var.Count;
			nodeBinSize.SliceCount = var.Count;
			nodeValue.SliceCount = var.Count;
			nodeID.SliceCount = var.Count;
			nodeType.SliceCount = var.Count;
			
			for (int i = 0; i < var.Count; i++){
				
				nodeName[i] = var[i].name;
				nodeID[i] = var[i].ID;
				nodeBinSize[i] = var[i].binSize.ToString();
				nodeType[i] = var[i].type;
				string val = "";
				
				for(int v = 0; v < var[i].binSize; v++){
					
					val += string.Join(Environment.NewLine, var[i].value[v]);
					if(var[i].binSize > 1 && v < var[i].binSize-1)val += ";";
				}
				
				nodeValue[i] = val;
			}
		}
	}
	
	
	public class Variable : IPluginEvaluate
	{
		
		
		public String name;
		public String ID;
		//	public int IDint;
		public String type;
		public int binSize;
		public List<String> value = new List<String>();
		
		
		
		public Variable(){
			
		}
		
		public Variable(String _name, String _ID, String _type, String[] _value){
			
			name = _name;
			ID = _ID;
			//IDint = Int32.Parse(ID);
			type = _type;
			binSize = _value.Length;
			
			//value = new string[binSize];
			value.Clear();
			
			for(int i = 0; i < binSize; i++){
				value.Add(_value[i]);
			}
			
		}
		
		public Variable(String _name, String _ID, String _type, List<String> _value){
			
			name = _name;
			ID = _ID;
			//IDint = Int32.Parse(ID);
			type = _type;
			binSize = _value.Count;
			
			//value = new string[binSize];
			value.Clear();
			
			for(int i = 0; i < binSize; i++){
				value.Add(_value[i]);
			}
			
		}
		
		public void updateValue(String[] _value){
			
			value.Clear();
			for(int i = 0; i < binSize; i++){
				if(i<_value.Length)value.Add( _value[i]);
				else value.Add("0");
			}
			binSize = value.Count;
		}
		
		public void Evaluate(int SpreadMax)
		{
		}
	}
}
