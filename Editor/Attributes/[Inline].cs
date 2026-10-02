// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Draw struct/class fields on one line
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class InlineAttribute : PropertyAttribute
	{
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Set size of specific inlined field
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class InlineWidthAttribute : Attribute
	{
		public InlineWidthAttribute(float w)
		{
			width = Mathf.Max(w, 0f);
		}

		/// <summary>
		/// Specify width of inner field
		/// </summary>
		public InlineWidthAttribute(string field, float w) : this(w)
		{
			this.field = field;
		}
		internal float width { get; }
		internal string field { get; }
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;

	/// <summary>
	/// Hide field from being inlined
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class InlineHiddenAttribute : Attribute
	{
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;
	using System;
	using SP = UnityEditor.SerializedProperty;

	[CustomPropertyDrawer(typeof(InlineAttribute))]
	internal sealed class _InlineAttribute : PropertyDrawer
	{
		private void InitFields(Type type)
		{
			List<(FieldInfo, float)> fields = new();
			
			// loop through all potential fields
			foreach (var f in type.FindInspectorFields<object>())
			{
				// skip non inlineable field
				if (f.IsDefined(typeof(InlineHiddenAttribute)))
				{
					continue;
				}
				var wAttr = f.GetCustomAttribute<InlineWidthAttribute>();
				
				if (wAttr == null)
				{
					wAttr = GetFieldOverride(f.Name);
				}
				var w = wAttr?.width ?? 0f;

				if (Mathf.Approximately(w, 0f))
				{
					_flexFields++;
				}

				fields.Add((f, w));
			}
			_fields = fields;
			_currentWidths = new float[_fields.Count];
		}

		private bool _init;
		private static readonly float _PAD = EditorGUIUtility.standardVerticalSpacing;

		public override float GetPropertyHeight(SP prop, GUIContent label)
		{
			if (!_init)
			{
				_init = true;
				InitFields(fieldInfo.FieldType.GetInnermostType());
			}
			
			var max = EditorGUIUtility.singleLineHeight;
			foreach (var (f, _) in _fields)
			{
				var p = prop.FindPropertyRelative(f.Name);
				var h = EditorGUI.GetPropertyHeight(p, GUIContent.none);
				if (h > max)
				{
					max = h;
				}
			}
			return max;
		}

		private Dictionary<string, InlineWidthAttribute> _outerFieldOverrides;

		private InlineWidthAttribute GetFieldOverride(string name)
		{
			if (_outerFieldOverrides == null)
			{
				_outerFieldOverrides = new();

				foreach (var attr in fieldInfo.GetCustomAttributes<InlineWidthAttribute>())
				{
					if (!string.IsNullOrEmpty(attr.field))
					{
						_outerFieldOverrides[attr.field] = attr;
					}
				}
			}
			return _outerFieldOverrides.GetValueOrDefault(name, null);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (_fields == null)
			{
				return;
			}

			var ti = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var pos = position;
			var usableWidth = pos.width - Mathf.Max(0f, _fields.Count - 1) * _PAD;
			var remainingWidth = usableWidth;

			for(int i = 0; i < _currentWidths.Length; i++)
			{
				var width = _fields[i].Item2;
				var fWidth = width > 1f ? width : width * usableWidth;
				_currentWidths[i] = fWidth;
				remainingWidth -= fWidth;
			}

			var flexWidth = _flexFields > 0 ? remainingWidth / _flexFields : 0f;

			for(int i = 0; i < _currentWidths.Length; i++)
			{
				var w = Mathf.Approximately(_currentWidths[i], 0f)
				? flexWidth
				: _currentWidths[i];

				var fRect = pos.SliceLeft(w);
				var field = _fields[i].Item1;

				var prop = property.FindPropertyRelative(field.Name);

				var height = EditorGUI.GetPropertyHeight(prop, GUIContent.none);
				fRect.height = height;

				EditorGUI.PropertyField(fRect, prop, GUIContent.none);

				if (i != _fields.Count - 1)
				{
					pos.SliceLeft(_PAD);
				}
			}
			EditorGUI.indentLevel = ti;
		}

		private int _flexFields;
		private IReadOnlyList<(FieldInfo, float)> _fields;
		private float[] _currentWidths;
	}

}

#endif