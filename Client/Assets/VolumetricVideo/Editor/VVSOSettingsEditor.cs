using UnityEditor;
using UnityEngine;

namespace AEstAR.VolumetricVideo
{
	[CustomEditor(typeof(VVSOSettings))]
	public class VVSOSettingsEditor : Editor
	{
		private VVSOSettings _vvsTarget;

		private void OnEnable()
		{
			_vvsTarget = (VVSOSettings)target;
		}

		public override void OnInspectorGUI()
		{
			Undo.RecordObject(_vvsTarget, "Inspector");

			serializedObject.Update();

			SerializedProperty hostsProperty = serializedObject.FindProperty("hosts");
			EditorGUILayout.PropertyField(hostsProperty, includeChildren: true);
			if (hostsProperty.hasChildren)
			{
				serializedObject.ApplyModifiedProperties();
			}

			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				_vvsTarget.loadingMethod = (VVSOSettings.LoadingMethodType)EditorGUILayout.EnumPopup(new GUIContent("Loading Method", "Choose the source of the data between local file and over http network"), _vvsTarget.loadingMethod);

				using (new GUILayout.VerticalScope("box"))
				{
					switch (_vvsTarget.loadingMethod)
					{
						case VVSOSettings.LoadingMethodType.Streaming:

							GUILayout.Label("Streaming", EditorStyles.boldLabel);

							break;

						case VVSOSettings.LoadingMethodType.Sequencing:

							GUILayout.Label("Successively", EditorStyles.boldLabel);

							break;
					}
				}
			}


			

			

			

			/*if (_vvsTarget.loadingMethod == VolumetricVideoSOSettings.LoadingMethodType.Streaming)
			{
				EditorGUILayout.Toggle(new GUIContent("In Streaming Assets", "checked if the 4ds file is located in the StreamingAssets folder (necessary to deploy it with the app)"), true);
			}
			else if (_vvsTarget.loadingMethod == VolumetricVideoSOSettings.LoadingMethodType.Preloading)
			{
				EditorGUILayout.Toggle(new GUIContent("In Samples Assets", "qweqwe"), false);
			}*/



			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}
		}


	}
}