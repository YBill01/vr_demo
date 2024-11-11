
using System.Collections.Generic;
using UnityEngine;

public class MiniConcole : MonoBehaviour{
	static MiniConcole(){
		Camera.main.gameObject.AddComponent<MiniConcole> ();
	}
	public class Message{
		public string msg;
		public Color msgColor;
		public MsgLevel ThisMsgLevel;
		public Message(string _msg){
			msg = _msg;
			msgColor = Color.white;
		}
		public Message(string _msg,Color _color){
			msg = _msg;
			msgColor = _color;
			ThisMsgLevel = getMsgLevel(_color);
		}
		public Message(string _msg, MsgLevel _type){
			msg = _msg;
			msgColor = getColor(_type);
			ThisMsgLevel = _type;
		}
		public static Color getColor(MsgLevel _type){
			switch (_type) {
			case MsgLevel.Log: return Color.white;
			case MsgLevel.Warning: return Color.yellow;
			case MsgLevel.Eror: return Color.red;
			}
			return Color.white;
		}
		public static MsgLevel getMsgLevel(Color _msgColor){
			if (_msgColor == Color.white) return MsgLevel.Log;
			if (_msgColor == Color.yellow) return MsgLevel.Warning;
			if (_msgColor == Color.red) return MsgLevel.Eror;
			return MsgLevel.IoMmo;
		}
	}
	public enum MsgLevel{Log,Warning,Eror,IoMmo};

	private KeyCode ConsoleActivateKey = KeyCode.Tab;
	private bool showConcole = true;//false;

	private bool ShowLog = true;//false;
	private bool ShowOnlyLog = false;
	private bool ShowOnlyWarining = false;
	private bool ShowOnlyEror = false;
	private bool ShowIoMmo = false;
	private bool ShowLogBur = true;//false;
	private Vector2 ScrollPoss = Vector2.zero;

	private static List<Message> allMsg = new List<Message>();

	public static void Log(string _msg){
		allMsg.Add (new Message (_msg));
	}
	public static void Log(string _msg,Color _msgColor){
		allMsg.Add (new Message (_msg,_msgColor));
	}
	public static void Log(string _msg,MsgLevel _msgLvl){
		allMsg.Add (new Message (_msg,_msgLvl));
	}
	public static void LogWarning(string _msg){
		allMsg.Add (new Message (_msg,Color.yellow));
	}
	public static void LogEror(string _msg){
		allMsg.Add (new Message (_msg,Color.red));
	}

	private MsgLevel GetLvlToVisable{get{
		if (ShowOnlyLog)
			return MsgLevel.Log;
		if (ShowOnlyWarining)
			return MsgLevel.Warning;
		if (ShowOnlyEror)
			return MsgLevel.Eror;
		if (ShowIoMmo)
			return MsgLevel.IoMmo;
		return MsgLevel.Log;
		}
	}

	void LateUpdate(){
		if (Input.GetKeyDown (ConsoleActivateKey))
			showConcole = !showConcole;
	}
	void OnGUI(){
		if (!showConcole)
			return;
		/*
		if (ShowLog) GUI.color = Color.green; else GUI.color = Color.white;
		if (GUI.Button (new Rect (0, 0, Screen.width / 20, Screen.height / 35), "All")) {ShowLog = true; ShowOnlyLog = ShowOnlyWarining = ShowOnlyEror = ShowIoMmo = false;}
		if (ShowOnlyLog) GUI.color = Color.green; else GUI.color = Color.white;
		if (GUI.Button (new Rect (Screen.width / 20, 0, Screen.width / 20, Screen.height / 35), "Log")) {ShowOnlyLog = true; ShowLog = ShowOnlyWarining = ShowOnlyEror = ShowIoMmo = false;}
		if (ShowOnlyWarining) GUI.color = Color.green; else GUI.color = Color.white;
		if (GUI.Button (new Rect (Screen.width / 20 * 2, 0, Screen.width / 20, Screen.height / 35), "Warning")) {ShowOnlyWarining = true; ShowLog = ShowOnlyLog = ShowOnlyEror = ShowIoMmo = false;}
		if (ShowOnlyEror) GUI.color = Color.green; else GUI.color = Color.white;
		if (GUI.Button (new Rect (Screen.width / 20 * 3, 0, Screen.width / 20, Screen.height / 35), "Eror")) {ShowOnlyEror = true; ShowLog = ShowOnlyLog = ShowOnlyWarining = ShowIoMmo = false;}
		if (ShowIoMmo) GUI.color = Color.green; else GUI.color = Color.white;
		if (GUI.Button (new Rect (Screen.width / 20 * 4, 0, Screen.width / 20, Screen.height / 35), "IoMmo")) {ShowIoMmo = true; ShowLog = ShowOnlyLog = ShowOnlyWarining = ShowOnlyEror = false;}
		GUI.color = Color.white;	
		if (GUI.Button (new Rect (Screen.width / 20 * 6, 0, Screen.width / 20, Screen.height / 35), "LogBur")) {ShowLogBur = !ShowLogBur;}
		if (GUI.Button (new Rect (Screen.width / 20 * 7, 0, Screen.width / 20, Screen.height / 35), "Close")) {showConcole = !showConcole;}
		*/
		if (!ShowLogBur)
			return;
		if (ShowLog) {
			GUI.Box (new Rect (0, Screen.height / 35, Screen.width / 2, Screen.height / 2), "");
			GUILayout.BeginArea (new Rect(0, Screen.height / 35, Screen.width / 2, Screen.height / 2));
			ScrollPoss = GUILayout.BeginScrollView(ScrollPoss);
			GUILayout.BeginVertical ();
			for (int i = 0; i < allMsg.Count; i++) {
				GUI.color = allMsg [i].msgColor;
				GUILayout.Label (allMsg [i].msg);
				GUI.color = Color.white;
			}
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
			GUILayout.EndArea ();
		} else {
			MsgLevel lvlToVisable = GetLvlToVisable;
			GUI.Box (new Rect (0, Screen.height / 35, Screen.width / 2, Screen.height / 2), "");
			GUILayout.BeginArea (new Rect(0, Screen.height / 35, Screen.width / 2, Screen.height / 2));
			ScrollPoss = GUILayout.BeginScrollView (ScrollPoss);
			GUILayout.BeginVertical ();
			for (int i = 0; i < allMsg.Count; i++) {
				if (allMsg [i].ThisMsgLevel == lvlToVisable) {
					GUI.color = allMsg [i].msgColor;
					GUILayout.Label (allMsg [i].msg);
					GUI.color = Color.white;
				}
			}
			GUILayout.EndVertical ();
			GUILayout.EndScrollView ();
			GUILayout.EndArea ();
		}
	}
}
