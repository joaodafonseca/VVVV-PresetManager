#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
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
		
		[Input("NodeSubType")]
		public ISpread<String> nodeSubType;
		
		[Input("Name")]
		public ISpread<String> name;
		
		[Input("ID")]
		public ISpread<String> ID;
		
		[Input("Value")]
		public ISpread<String> value;
		
		[Input("VariablePosition")]
		public ISpread<int> variablePosition;
		
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
		#endregion fields & pins
		
	
		List<Variable> var = new List<Variable>(); 
		
		
		public void loadPreset(){
		
			for(int i = 0; i < var.Count; i++){
				
				for(int p = 0; p < preset.Count; p++){
				
					String[] pre = preset[p].Split(':');
					String nodeID = pre[4];
					//	FLogger.Log(LogType.Debug, preset[p]);
					if(nodeID == var[i].ID){
						
					
						
						String[] pv = pre[3].Split(';');
					
					
						var[i].updateValue( pv);
					}
				}
				
			
				
			}
			
		}
			

		public int RetrieveBinSize(String subType){
			
			int binSize = 0;
			String type = subType; 
			String[] SplitString = type.Split(',');
					
			if(SplitString[0]=="Toggle" || SplitString[0]=="Bang")binSize = 1;
			
			if(SplitString[0]=="Endless"){
					
				binSize =  Int32.Parse(SplitString[1]);
			}
				
			if(SplitString[0]=="HSVAField"){
				
				binSize = 1;
			}
			
			return binSize;
			
		}
		
		
		public String RetrieveType(String subType){
			
			 
			String type = subType; 
			String[] SplitString = type.Split(',');
			
			type = SplitString[0];
					
			
			
			return type;
			
		}
		
		
		public void KeepTrackOfExposedNodes(){
		
			//	FLogger.Log(LogType.Debug,"hello");
			if(var.Count==0  || load[0]){
			
			var.Clear();
			
				for(int i = 0; i < nodeSubType.Count; i++){
				
					int binSize = RetrieveBinSize(nodeSubType[i]);	
					var.Add(new Variable(name[i],ID[i],RetrieveType(nodeSubType[i]), binSize));	
				}	
				
					if(preset.Count>=1)loadPreset();
			
			}
			
			
			if(ID.Count > var.Count){
		FLogger.Log(LogType.Debug,"ADDING NODE");
			int newNodePos=-1;
			for(int n = ID.Count; n > 0; n--){//	for(int n = 0; n < ID.Count; n++){
				
					bool nodeFound = false;
				
					for(int v = 0; v < var.Count; v++){
						
						if( ID[n] == var[v].ID)nodeFound=true;
						
					}
				
				
				if(!nodeFound)newNodePos=n;
				
				}
					if(newNodePos>=0){
						//if(newNodePos== ID.Count-1)newNodePos-=1;
						FLogger.Log(LogType.Debug,"name: " + name[newNodePos] + " | id: "+ID[newNodePos] + " | var size: "+var.Count.ToString() + " | at pos: "+newNodePos.ToString());
						var.Insert(newNodePos,new Variable(name[newNodePos],ID[newNodePos], RetrieveType(nodeSubType[newNodePos]), RetrieveBinSize(nodeSubType[newNodePos])));
				
						FLogger.Log(LogType.Debug,"ADDED");
				}
				
			}
			
			if(ID.Count < var.Count){
				FLogger.Log(LogType.Debug,"REMOVING NODE");
					for(int v = 0; v < var.Count; v++){
					
						bool nodeFound = false;
					
						for(int n = 0; n < ID.Count; n++){
							if( var[v].ID == ID[n])nodeFound=true;
						
						}
							if(!nodeFound){
						
								var.RemoveAt(v);
							}
					}
			}
			
		}
		
		
		public void updateVariable(){
			if(receiveNewValue[0]){
				var[variablePosition[0]].updateValue(value);
				FLogger.Log(LogType.Debug, value[0]);
			}
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			KeepTrackOfExposedNodes();
			updateVariable();
			
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
				//FLogger.Log(LogType.Debug, "");
			}
			//
		}  
	}
	
	
	public class Variable : IPluginEvaluate
	{
			
		
	    public String name;
		public String ID;
		public int IDint;
		public String type;
		public int binSize;
	 	public List<String> value = new List<String>(); 
		
		
	   
	    public Variable(){
	    	
	    }
		
		public Variable(String _name, String _ID, String _type, int _binSize){
			
			name = _name;
			ID = _ID;
			IDint = Int32.Parse(ID);
			type = _type;
			binSize = _binSize;
			
		//	value = new string[binSize];
			value.Clear();
			for(int i = 0; i < binSize; i++){
				value.Add("0");
			}
				
		}
		
	
		
		public void updateValue(ISpread<String> _value){
			value.Clear();
			
			
			
			if(type == "HSVAField"){
			
				for(int i = 0; i < binSize; i++){
				
				//	value.Add( _value[0].Split(',')[i]);
					
					value.Add( _value[i]);
					
				//	FLogger.Log(LogType.Debug, _value[0].Split(',')[i]);
				}
				
				
				
			}else{
			
				for(int i = 0; i < binSize; i++){
				
					value.Add( _value[i]);
				}
				
			}
		}
		
			
		public void updateValue(String[] _value){
			for(int i = 0; i < binSize; i++){
				value[i] = _value[i];
			}
		}
		
	    public void Evaluate(int SpreadMax)
	    {
	    }
	}
}
