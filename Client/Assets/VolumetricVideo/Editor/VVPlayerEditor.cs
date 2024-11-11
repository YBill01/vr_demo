using System;
using System.Collections;
using System.Collections.Generic;
using unity4dv;
using UnityEditor;
using UnityEngine;

namespace AEstAR.VolumetricVideo
{
	[CustomEditor(typeof(VVPlayer))]
	public class VVPlayerEditor : Editor
	{
		private VVPlayer _vvpTarget;

		private void OnEnable()
		{
			_vvpTarget = (VVPlayer)target;
		}

		public override void OnInspectorGUI()
		{
			Undo.RecordObject(_vvpTarget, "Inspector");

			serializedObject.Update();

			//base.OnInspectorGUI();


			_vvpTarget.settings = (VVSOSettings)EditorGUILayout.ObjectField("Settings", _vvpTarget.settings, typeof(VVSOSettings), false);

			
			if (_vvpTarget.settings != null)
			{
				

				int index = Array.IndexOf(_vvpTarget.settings.hosts, _vvpTarget.hostInfo);
				if (index == -1)
				{
					index = 0;
				}
				string[] labels = new string[_vvpTarget.settings.hosts.Length];
				for (int i = 0; i < _vvpTarget.settings.hosts.Length; i++)
				{
					labels[i] = _vvpTarget.settings.hosts[i].name;
				}

				//string[] labels = { "1", "2", "3" };
				index = EditorGUILayout.Popup("Host", index, labels);

				_vvpTarget.hostInfo = _vvpTarget.settings.hosts[index];

				using (new GUILayout.VerticalScope("box"))
				{
					GUILayout.Label(_vvpTarget.hostInfo.url);
					//GUILayout.TextArea(_vvpTarget.hostInfo.description);
					//GUILayout.TextArea(_vvpTarget.hostInfo.description, EditorStyles.textField, GUILayout.ExpandHeight(true));

				}
			}
			

			_vvpTarget.plugin4DS = (Plugin4DS)EditorGUILayout.ObjectField("Plugin4DS 1", _vvpTarget.plugin4DS, typeof(Plugin4DS), true);
			_vvpTarget.plugin4DS2 = (Plugin4DS)EditorGUILayout.ObjectField("Plugin4DS 2", _vvpTarget.plugin4DS2, typeof(Plugin4DS), true);

			_vvpTarget.sequenceJSONInfo = EditorGUILayout.TextField(new GUIContent("Info JSON"), _vvpTarget.sequenceJSONInfo).ToString();







			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}
		}

	}
}