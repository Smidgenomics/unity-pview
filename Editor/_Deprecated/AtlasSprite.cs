// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;

	/// <summary>
	/// IMGUI sprite - texture + position + offset
	/// </summary>
	[Serializable]
	internal struct AtlasSprite
	{
		public Texture Texture => _texture.asset;
		public Vector2 Size => _size;
		public Vector2 Offset => _offset;

		public void Draw(in Rect pos)
		{
			var c = Mathf.Approximately(_tint.a, 0f)
			? Color.white
			: _tint;
			IMGUI.DrawSprite(pos, Texture, new Rect(Offset, Size), c);
		}

		public static readonly AtlasSprite fill = new()
		{
			_size = Vector2.one,
			_offset = Vector2.zero,
			_tint = Color.white
		};

		public AtlasSprite(Texture tex, Vector2 size, Vector2 offset)
		{
			_size = size;
			_offset = offset;
			_texture = new LazyLoadReference<Texture>(tex);
			_tint = Color.white;
		}

		[SerializeField] internal Vector2 _size;
		[SerializeField] internal Vector2 _offset;
		[SerializeField] internal LazyLoadReference<Texture> _texture;
		[SerializeField] internal Color _tint;
	}

}

// this IF is a bit moot - it's all editor code really
#if UNITY_EDITOR

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomPropertyDrawer(typeof(AtlasSprite))]
	internal sealed class _AtlasIcon : PropertyDrawer
	{
		public const byte ROWS = 4;
		public static readonly float LINE_HEIGHT = EditorGUIUtility.singleLineHeight;

		private static readonly (string, int, int)[] _OFFSET_BTNS = {
			("-x", -1, 0),
			("+x", 1, 0),
			("-y", 0, -1),
			("+y", 0, 1)
		};

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return
			ROWS * LINE_HEIGHT
			+ (ROWS - 1) * EditorGUIUtility.standardVerticalSpacing;
		}

		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent l)
		{
			var ctx = new DrawerContext
			{
				texture = prop.FindPropertyRelative(nameof(AtlasSprite._texture)),
				offset = prop.FindPropertyRelative(nameof(AtlasSprite._offset)),
				size = prop.FindPropertyRelative(nameof(AtlasSprite._size)),
				tint = prop.FindPropertyRelative(nameof(AtlasSprite._tint)),
			};

			EditorGUI.BeginProperty(pos, l, prop);
			{
				if (!fieldInfo.FieldType.IsArray)
				{
					pos = EditorGUI.PrefixLabel(pos, l);
				}

				var icoRect = pos.SliceLeft(pos.height * 0.75f);
				pos.SliceLeft(EditorGUIUtility.standardVerticalSpacing);

				var texRect = LineRect(ref pos);
				
				var colorWidth = Mathf.Max(texRect.width * 0.25f, EditorGUIUtility.singleLineHeight * 2);
				var tintRect = texRect.SliceRight(colorWidth);
				texRect.SliceRight(EditorGUIUtility.standardVerticalSpacing);
				
				EditorGUI.PropertyField(texRect, ctx.texture, GUIContent.none);
				EditorGUI.PropertyField(tintRect, ctx.tint, GUIContent.none);
				EditorGUI.PropertyField(LineRect(ref pos), ctx.size, GUIContent.none);
				EditorGUI.PropertyField(LineRect(ref pos), ctx.offset, GUIContent.none);
				Buttons(ref pos, ctx);
				
				PreviewIcon(icoRect, ctx);
				
			}
			EditorGUI.EndProperty();
		}

		private ref struct DrawerContext
		{
			public SerializedProperty texture, size, offset, tint;
		}

		private static void Buttons(ref Rect pos, in DrawerContext ctx)
		{
			var area = LineRect(ref pos);
			var barea = area.SliceRight(100f);
			var bw = barea.width / _OFFSET_BTNS.Length;
			for (var i = 0; i < _OFFSET_BTNS.Length; i++)
			{
				OffsetButton(barea.SliceLeft(bw), i, ctx);
			}
		}

		private static void OffsetButton(in Rect pos, in int i, in DrawerContext ctx)
		{
			var (l, x, y) = _OFFSET_BTNS[i];
			var s = EditorStyles.miniButtonMid;
			if (i == 0) { s = EditorStyles.miniButtonLeft; }
			if (i == _OFFSET_BTNS.Length - 1) { s = EditorStyles.miniButtonRight; }
			if (GUI.Button(pos, l, s))
			{
				AddOffset(ctx, x, y);
			}
		}

		private static void AddOffset(in DrawerContext ctx, in float x, in float y)
		{
			var size = ctx.size.vector2Value;
			var offset = ctx.offset.vector2Value;
			offset.x += size.x * x;
			offset.y += size.y * y;

			if (offset.x < 0f || offset.y < 0f)
			{
				return;
			}

			if (offset.x >= 1f || offset.y >= 1f)
			{
				return;
			}
			ctx.offset.vector2Value = offset;
		}

		private static Rect LineRect(ref Rect pos)
		{
			var area = pos.SliceTop(LINE_HEIGHT);
			pos.SliceTop(EditorGUIUtility.standardVerticalSpacing);
			return area;
		}

		private static void PreviewIcon(Rect rect, in DrawerContext ctx)
		{
			rect.height = rect.width;
			var tex = ctx.texture.objectReferenceValue as Texture;
			GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
			if (!tex)
			{
				return;
			}
			var ir = rect;
			ir.Resize(-ir.width * 0.1f);

			var tint = ctx.tint.colorValue;
			if (Mathf.Approximately(tint.a, 0f))
			{
				tint = Color.white;
			}
			IMGUI.DrawSprite(ir, tex, new Rect(ctx.offset.vector2Value, ctx.size.vector2Value), tint);
		}

	}
}

#endif