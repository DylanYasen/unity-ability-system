using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;

public class AbilityEditor : EditorWindow
{
	[MenuItem("Warlock War/Ability/Create New")]
	public static void CreateNewAbility()
	{
		AbilityEditor editor = GetWindow<AbilityEditor>();
		editor.Show();
		AssetUtility.CreateAsset<DataDrivenAbility>("Abilities");
	}

	[OnOpenAsset(0)]
	public static bool LoadAbility(int instanceId, int line)
	{
		var obj = EditorUtility.InstanceIDToObject(instanceId);
		if (obj is DataDrivenAbility)
		{
			AbilityEditor editor = GetWindow<AbilityEditor>();
			editor.Show();
			return true;
		}
		return false;
	}

	GUIStyle _foldout = new GUIStyle(EditorStyles.foldout);
	void Awake()
	{
		_foldout.fontStyle = FontStyle.Bold;
	}

	private Vector2 scrollPos;
	void OnGUI()
	{
		DataDrivenAbility ability = Selection.activeObject as DataDrivenAbility;
		if(ability == null) return;

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		GUILayout.Label("Name", EditorStyles.boldLabel);
		ability.abilityData.Name = EditorGUILayout.TextField(ability.abilityData.Name);

		GUILayout.Label("Description", EditorStyles.boldLabel);
		ability.abilityData.Description = EditorGUILayout.TextArea(ability.abilityData.Description, GUILayout.Height(60));

		DrawAttributes(ability);

		DrawEventList(ability.events);

		if (GUILayout.Button("Save"))
		{
			// rename based on the ability's name
			var oldPath = AssetDatabase.GetAssetPath(ability);
			var fileName = Path.GetFileName(oldPath);
			var newPath = oldPath.Replace(fileName, ability.abilityData.Name + ".asset");
			AssetDatabase.MoveAsset(oldPath, newPath);
			EditorUtility.SetDirty(ability);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		EditorGUILayout.EndScrollView();
	}

	private void DrawAttributes(DataDrivenAbility ability)
	{
		GUILayout.Label("Attributres", EditorStyles.boldLabel);

		ability.abilityData.Icon = EditorGUILayout.ObjectField("Icon", ability.abilityData.Icon, typeof(Sprite), false) as Sprite;
		ability.abilityData.Price = EditorGUILayout.IntField("Price", ability.abilityData.Price);
		ability.abilityData.BehaviorFlags = (DataDrivenAbility.BehaviorFlag)EditorGUILayout.EnumFlagsField("Behavior Flags", ability.abilityData.BehaviorFlags);
		ability.abilityData.TargetFlags = (DataDrivenAbility.TargetFlag)EditorGUILayout.EnumFlagsField("Can Target", ability.abilityData.TargetFlags);
		ability.abilityData.Cooldown = EditorGUILayout.FloatField("Cooldown", ability.abilityData.Cooldown);
		ability.abilityData.Animation = EditorGUILayout.TextField("Animation", ability.abilityData.Animation);
		ability.abilityData.AnimCastPoint = EditorGUILayout.FloatField("AnimCastPoint", ability.abilityData.AnimCastPoint);

		if ((ability.abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.CHANNELLED) != 0)
		{
			ability.abilityData.Duration = EditorGUILayout.FloatField("Duration", ability.abilityData.Duration);
		}

		DrawAoeAttributes(ability.abilityData);
	}

	private void DrawAoeAttributes(AbilityData abilityData)
	{
		if ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.AOE) == 0)
		{
			return;
		}
		GUILayout.Label("AOE Attributres", EditorStyles.boldLabel);
		abilityData.AoeData.Center = (AoeData.CenterType)EditorGUILayout.EnumPopup("Center", abilityData.AoeData.Center);
		abilityData.AoeData.Shape = (AoeData.ShapeType)EditorGUILayout.EnumPopup("Shape", abilityData.AoeData.Shape);

		if (abilityData.AoeData.Shape == AoeData.ShapeType.CIRCLE)
		{
			abilityData.AoeData.Radius = EditorGUILayout.IntField("Radius", abilityData.AoeData.Radius);
		}
		else if (abilityData.AoeData.Shape == AoeData.ShapeType.RECT)
		{
			abilityData.AoeData.Width = EditorGUILayout.IntField("Width", abilityData.AoeData.Width);
            abilityData.AoeData.Distance = EditorGUILayout.IntField("Distance", abilityData.AoeData.Distance);
        }
    }

	private void DrawCustomAttributes(AbilityActionData actionData)
	{
		actionData.showCustomAttributes = EditorGUILayout.Foldout(actionData.showCustomAttributes, "Custom Attributes");
		if (actionData.showCustomAttributes)
		{
			DrawIntFieldList(actionData.IntFields);
			DrawFloatField(actionData.FloatFields);
			DrawVec3Field(actionData.Vec3Fields);
			DrawVec2Field(actionData.Vec2Fields);
			DrawStrField(actionData.StrFields);
			DrawBoolField(actionData.BoolFields);
		}
	}

	private void DrawIntFieldList(List<IntField> intFields)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Integer Fields", EditorStyles.boldLabel);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			intFields.Add(new IntField());
		}
		GUILayout.Space(280); // hack!
		GUILayout.EndHorizontal();
		for (int i = 0; i < intFields.Count; i++)
		{
			GUILayout.BeginHorizontal();
			intFields[i].Name = EditorGUILayout.TextField(intFields[i].Name);
			intFields[i].Value = EditorGUILayout.IntField(intFields[i].Value);
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				intFields[i].tobeRemoved = true;
			}
			GUILayout.EndHorizontal();
		}
		intFields.RemoveAll((a) => a.tobeRemoved);
	}

	void DrawFloatField(List<FloatField> floatFields)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Float Fields", EditorStyles.boldLabel);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			floatFields.Add(new FloatField());
		}
		GUILayout.Space(280); // hack!
		GUILayout.EndHorizontal();
		for (int i = 0; i < floatFields.Count; i++)
		{
			GUILayout.BeginHorizontal();
			floatFields[i].Name = EditorGUILayout.TextField(floatFields[i].Name);
			floatFields[i].Value = EditorGUILayout.FloatField(floatFields[i].Value);
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				floatFields[i].tobeRemoved = true;
			}
			GUILayout.EndHorizontal();
		}
		floatFields.RemoveAll((a) => a.tobeRemoved);
	}

	void DrawVec3Field(List<Vec3Field> vec3Fields)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Vector3 Fields", EditorStyles.boldLabel);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			vec3Fields.Add(new Vec3Field());
		}
		GUILayout.Space(280); // hack!
		GUILayout.EndHorizontal();
		for (int i = 0; i < vec3Fields.Count; i++)
		{
			GUILayout.BeginHorizontal();
			vec3Fields[i].Name = EditorGUILayout.TextField(vec3Fields[i].Name);
			vec3Fields[i].Value = EditorGUILayout.Vector3Field("", vec3Fields[i].Value);
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				vec3Fields[i].tobeRemoved = true;
			}
			GUILayout.EndHorizontal();
		}
		vec3Fields.RemoveAll((a) => a.tobeRemoved);
	}

	void DrawVec2Field(List<Vec2Field> vec2Fields)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Vector2 Fields", EditorStyles.boldLabel);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			vec2Fields.Add(new Vec2Field());
		}
		GUILayout.Space(280); // hack!
		GUILayout.EndHorizontal();
		for (int i = 0; i < vec2Fields.Count; i++)
		{
			GUILayout.BeginHorizontal();
			vec2Fields[i].Name = EditorGUILayout.TextField(vec2Fields[i].Name);
			vec2Fields[i].Value = EditorGUILayout.Vector2Field("", vec2Fields[i].Value);
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				vec2Fields[i].tobeRemoved = true;
			}
			GUILayout.EndHorizontal();
		}
		vec2Fields.RemoveAll((a) => a.tobeRemoved);
	}

	void DrawStrField(List<StrField> strFields)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("String Fields", EditorStyles.boldLabel);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			strFields.Add(new StrField());
		}
		GUILayout.Space(280); // hack!
		GUILayout.EndHorizontal();
		for (int i = 0; i < strFields.Count; i++)
		{
			GUILayout.BeginHorizontal();
			strFields[i].Name = EditorGUILayout.TextField(strFields[i].Name);
			strFields[i].Value = EditorGUILayout.TextField(strFields[i].Value);
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				strFields[i].tobeRemoved = true;
			}
			GUILayout.EndHorizontal();
		}
		strFields.RemoveAll((a) => a.tobeRemoved);
	}
	void DrawBoolField(List<BoolField> boolFields)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Bool Fields", EditorStyles.boldLabel);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			boolFields.Add(new BoolField());
		}
		GUILayout.Space(280); // hack!
		GUILayout.EndHorizontal();
		for (int i = 0; i < boolFields.Count; i++)
		{
			GUILayout.BeginHorizontal();
			boolFields[i].Name = EditorGUILayout.TextField(boolFields[i].Name);
			boolFields[i].Value = EditorGUILayout.Toggle(boolFields[i].Value);
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				boolFields[i].tobeRemoved = true;
			}
			GUILayout.EndHorizontal();
		}
		boolFields.RemoveAll((a) => a.tobeRemoved);
	}

	#region Events
	void DrawEventList(List<AbilityEventData> abilityEvents)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Events", EditorStyles.boldLabel);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			abilityEvents.Add(new AbilityEventData());
		}
		GUILayout.Space(310); // hack!
		GUILayout.EndHorizontal();

		for (int i = 0; i < abilityEvents.Count; i++)
		{
			GUILayout.BeginHorizontal();
			abilityEvents[i].showInEditor = EditorGUILayout.Foldout(abilityEvents[i].showInEditor, abilityEvents[i].Type.ToString());
			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				abilityEvents[i].tobeRemoved = true;
			}
			GUILayout.EndHorizontal();
			DrawEvent(abilityEvents[i]);
		}
		abilityEvents.RemoveAll((a) => a.tobeRemoved);
	}
	void DrawEvent(AbilityEventData abilityEvent)
	{
		if (abilityEvent.showInEditor)
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			abilityEvent.Type = (AbilityEventData.Event)EditorGUILayout.EnumPopup(new GUIContent("Type", abilityEvent.Type.ToDescription()), abilityEvent.Type);
			EditorGUILayout.EndHorizontal();

			DrawActionList(abilityEvent);
			DrawModiferList(abilityEvent);

			EditorGUILayout.EndVertical();
		}
	}
	#endregion

	#region Actions
	void DrawActionList(AbilityEventData abilityEvent)
	{
		var eventActions = abilityEvent.Actions;

		GUILayout.BeginHorizontal();
		abilityEvent.showActionsInEditor = EditorGUILayout.Foldout(abilityEvent.showActionsInEditor, "Actions", _foldout);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			eventActions.Add(new AbilityActionData());
		}
		GUILayout.Space(310); // hack!
		GUILayout.EndHorizontal();

		if (abilityEvent.showActionsInEditor)
		{
			for (int i = 0; i < eventActions.Count; i++)
			{
				GUILayout.BeginHorizontal();
				eventActions[i].showInEditor = EditorGUILayout.Foldout(eventActions[i].showInEditor, eventActions[i].Type.ToString());

				if (GUILayout.Button("-", GUILayout.Width(20)))
				{
					eventActions[i].tobeRemoved = true;
				}
				GUILayout.EndHorizontal();

				AbilityActionData action = eventActions[i];

				DrawAction(action);
			}
			eventActions.RemoveAll((a) => a.tobeRemoved);
		}
	}

	void DrawAction(AbilityActionData eventAction)
	{
		if (eventAction.showInEditor)
		{
			eventAction.Type = (AbilityActionData.Action)EditorGUILayout.EnumPopup(new GUIContent("Type", eventAction.Type.ToDescription()), eventAction.Type);
			eventAction.Target = (DataDrivenAbility.Target)EditorGUILayout.EnumPopup("Target", eventAction.Target);
			eventAction.MultipleTargets = EditorGUILayout.Toggle("Multiple Targets", eventAction.MultipleTargets);
			DrawCustomAttributes(eventAction);
		}
	}
	#endregion

	#region Modifiers
	void DrawModiferList(AbilityEventData abilityEvent)
	{
		var eventModifiers = abilityEvent.Modifiers;

		GUILayout.BeginHorizontal();
		abilityEvent.showModifiersInEditor = EditorGUILayout.Foldout(abilityEvent.showModifiersInEditor, "Modifier", _foldout);
		if (GUILayout.Button("+", GUILayout.Width(20)))
		{
			eventModifiers.Add(new AbilityModifierData());
		}
		GUILayout.Space(310); // hack!
		GUILayout.EndHorizontal();

		if (abilityEvent.showModifiersInEditor)
		{
			for (int i = 0; i < eventModifiers.Count; i++)
			{
				GUILayout.BeginHorizontal();
				eventModifiers[i].showInEditor = EditorGUILayout.Foldout(eventModifiers[i].showInEditor, eventModifiers[i].Type.ToString());

				if (GUILayout.Button("-", GUILayout.Width(20)))
				{
					eventModifiers[i].tobeRemoved = true;
				}
				GUILayout.EndHorizontal();
				DrawModifier(eventModifiers[i]);
			}
			eventModifiers.RemoveAll((a) => a.tobeRemoved);
		}
	}
	void DrawModifier(AbilityModifierData eventModifier)
	{
		if (eventModifier.showInEditor)
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Type");
			eventModifier.Type = (AbilityModifierData.Modifier)EditorGUILayout.EnumPopup(eventModifier.Type);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
		}
	}

	#endregion

}