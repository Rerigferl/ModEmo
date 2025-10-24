using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Numeira;

[StructLayout(LayoutKind.Explicit)]
internal struct GUIPosition
{
    public static float LineHeight =>
#if UNITY_EDITOR
        EditorGUIUtility.singleLineHeight;
#else
        18f;
#endif

    [FieldOffset(0)]
    public float X;

    [FieldOffset(sizeof(float))]
    public float Y;

    [FieldOffset(sizeof(float) * 2)]
    public float Width;

    [FieldOffset(sizeof(float) * 3)]
    public float Height;

    [FieldOffset(0)]
    public Vector2 Position;

    [FieldOffset(sizeof(float) * 2)]
    public Vector2 Size;

    [FieldOffset(sizeof(float) * 4)]
    private float requiredHeight;

    public GUIPosition(Rect position)
    {
        this = Unsafe.As<Rect, GUIPosition>(ref position);
        requiredHeight = 0;
    }

    private GUIPosition(float requiredHeight)
    {
        this = default;
        this.requiredHeight = requiredHeight;
    }

    public readonly GUIPosition NewLine(float? lineHeight = null)
    {
        lineHeight ??= LineHeight;
        if (IsEmpty)
        {
            return new(requiredHeight + lineHeight.Value);
        }
        else
        {
            return this with
            {
                Y = Y + Height,
                Height = lineHeight.Value,
                requiredHeight = requiredHeight + lineHeight.Value,
            };
        }
    }

    public readonly GUIPosition SingleLine(float? lineHeight = null)
    {
        return IsEmpty ? new(Math.Max(requiredHeight, lineHeight ?? LineHeight)) : this with { Height = (lineHeight ?? LineHeight) };
    }

    public readonly GUIPosition Space(float height)
        => IsEmpty ? new(requiredHeight + height) : this with { Y = Y + height, requiredHeight = requiredHeight + height };

    public readonly GUIPosition Padding(float padding = 0)
        => IsEmpty ? new(requiredHeight + padding) : Padding(padding, padding, padding, padding);

    public readonly GUIPosition Padding(float left = 0, float right = 0, float top = 0, float bottom = 0)
    {
        return IsEmpty ? new(requiredHeight + top + bottom) : this with
        {
            X = X + left,
            Y = Y + top,
            Width = Width - (left + right),
            Height = Height - (top + bottom),
            requiredHeight = requiredHeight + top + bottom,
        };
    }

    public readonly GUIPosition Center(Vector2 innerSize)
    {
        if (IsEmpty)
            return default;

        var margin = Size - innerSize;
        return this with 
        { 
            X = X + margin.x / 2,
            Y = Y + margin.y / 2,
            Width = innerSize.x, 
            Height = innerSize.y,
        };
    }

    public readonly (GUIPosition Left, GUIPosition Right) HorizontalSeparate(float? width = null, float? margin = null)
    {
        if (IsEmpty)
            return default;

        var m = margin ?? 0;
        var w = width ?? Width / 2; 
        var left = this;
        var right = this;
        left.Width = w;
        right.Width -= w + m;
        right.X += w + m;
        return (left, right);
    }

    public readonly (GUIPosition Top, GUIPosition Bottom) VerticalSeparate(float? height = null, float? margin = null)
    {
        if (IsEmpty)
            return default;

        var m = margin ?? 0;
        var h = height ?? Height / 2;
        var top = this;
        var bottom = this;
        top.Height = h;
        bottom.Height -= h + m;
        bottom.Y += h + m;
        return (top, bottom);
    }

    public readonly Span<GUIPosition> HorizontalSplit(Span<GUIPosition> buffer)
    {
        if (IsEmpty)
            return buffer;

        var width = Width / buffer.Length;

        for (int i = 0; i < buffer.Length; i++)
        {
            ref var item = ref buffer[i];

            item = this;
            item.Width = width;
            item.X += width * i;
        }

        return buffer;
    }

    private ref float GetPinnableReference() => ref MemoryMarshal.GetReference(MemoryMarshal.CreateSpan(ref X, 4));

    public readonly bool IsEmpty => (X, Y, Width, Height) is (0, 0, 0, 0);

    public ref Rect AsRect() => ref Unsafe.As<float, Rect>(ref GetPinnableReference());

    internal readonly float RequiredHeight => requiredHeight;

    public readonly void Deconstruct(out float X, out float Y) 
        => (X, Y) = (this.X, this.Y);

    public readonly void Deconstruct(out float X, out float Y, out float Width, out float Height) 
        => (X, Y, Width, Height) = (this.X, this.Y, this.Width, this.Height);

    public static implicit operator Rect(GUIPosition position) => Unsafe.As<GUIPosition, Rect>(ref position);
    public static implicit operator GUIPosition(Rect position) => new(position);
}


#if UNITY_EDITOR
internal static class GUIPositionExt
{
    public static void TextField(this GUIPosition position, string label, ref string contents, string? placeholder = null)
    {
        if (position.IsEmpty)
            return;

        GUIPosition left, right;
        if (!string.IsNullOrEmpty(label))
        {
            (left, right) = position.HorizontalSeparate(EditorGUIUtility.labelWidth, 2);
            EditorGUI.LabelField(left, label);
        }
        else
        {
            (left, right) = (default, position);
        }

        GUI.SetNextControlName("TextField");
        contents = EditorGUI.DelayedTextField(right, "", contents, PlaceholderTextAreaStyle);
        GUI.SetNextControlName("");
        if (string.IsNullOrEmpty(contents) && GUI.GetNameOfFocusedControl() != "TextField")
            EditorGUI.LabelField(right, placeholder, PlaceholderStyle);
    }

    public static void TextField(this GUIPosition position, string label, SerializedProperty? property, string? placeholder = null)
    {
        if (position.IsEmpty || property is null)
            return;

        EditorGUI.BeginProperty(position, new(label), property);
        string value = property.stringValue;
        EditorGUI.BeginChangeCheck();
        TextField(position, label, ref value, placeholder);
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = value;
        }

        EditorGUI.EndProperty();
    }

    public static void SearchField(this GUIPosition position, string label, ref string contents)
    {
        if (position.IsEmpty)
            return;

        GUIPosition left, right;
        if (!string.IsNullOrEmpty(label))
        {
            (left, right) = position.HorizontalSeparate(EditorGUIUtility.labelWidth, 2);
            EditorGUI.LabelField(left, label);
        }
        else
        {
            (left, right) = (default, position);
        }

        contents = EditorGUI.TextField(right, "", contents, SearchTextAreaStyle);
    }

    public static T? ObjectField<T>(this GUIPosition position, string label, T? obj, Type? objectType = null, bool allowSceneObjects = true, bool readOnly = false) where T : Object
    {
        ObjectField(position, label, ref obj, objectType, allowSceneObjects, readOnly);
        return obj;
    }

    public static void ObjectField<T>(this GUIPosition position, string label, ref T? obj, Type? objectType = null, bool allowSceneObjects = true, bool readOnly = false) where T : Object
    {
        GUIPosition left, right;
        if (!string.IsNullOrEmpty(label))
        {
            (left, right) = position.HorizontalSeparate(EditorGUIUtility.labelWidth, 2);
            EditorGUI.LabelField(left, label);
        }
        else
        {
            (left, right) = (default, position);
        }

        EditorGUI.BeginDisabledGroup(readOnly);
        var result = EditorGUI.ObjectField(right, obj, objectType ?? typeof(T), allowSceneObjects);
        EditorGUI.EndDisabledGroup();
        obj = result as T;
    }

    public static void Label(this GUIPosition position, string label)
    {
        EditorGUI.LabelField(position, label);
    }

    public static bool Foldout(this GUIPosition position, string label, bool open)
    {
        if (position.IsEmpty)
            return open;

        return EditorGUI.Foldout(position, open, label);
    }

    public static bool Foldout(this GUIPosition position, string label, SerializedProperty? serializedProperty)
    {
        if (serializedProperty is null)
            return false;

        if (position.IsEmpty)
            return serializedProperty.isExpanded;

        EditorGUI.BeginChangeCheck();
        var expanded = position.Foldout(label, serializedProperty.isExpanded);
        if (EditorGUI.EndChangeCheck())
        {
            serializedProperty.isExpanded = expanded;
        }

        return expanded;
    }

    public static void Separator(this GUIPosition position)
    {
        if (position.IsEmpty)
            return;

        position = position.Center(position.Size with { y = 0.5f });
        EditorGUI.DrawRect(position, Color.white with { a = 0.2f });
    }

    public static void Slider(this GUIPosition position, string label, SerializedProperty property, float min, float max, float minLimit = float.MinValue, float maxLimit = float.MaxValue)
    {
        var content = string.IsNullOrEmpty(label) ? GUIContent.none : new(label);
        var method = InternalSliderMethod ??= CreateInternalSliderMethodProxy();

        var value = property.floatValue;
        EditorGUI.BeginChangeCheck();
        value = method.Invoke(position, content, value, min, max, minLimit, maxLimit);
        if (EditorGUI.EndChangeCheck())
        {
            property.floatValue = value;
        }
    }

    public static float Slider(this GUIPosition position, string label, float value, float min, float max, float minLimit = float.MinValue, float maxLimit = float.MaxValue)
    {
        var content = string.IsNullOrEmpty(label) ? GUIContent.none : new(label);
        var method = InternalSliderMethod ??= CreateInternalSliderMethodProxy();

        return method.Invoke(position, content, value, min, max, minLimit, maxLimit);
    }

    public static void IntSlider(this GUIPosition position, string label, SerializedProperty property, int min, int max, int minLimit = int.MinValue, int maxLimit = int.MaxValue)
    {
        var content = string.IsNullOrEmpty(label) ? GUIContent.none : new(label);
        var method = InternalSliderMethod ??= CreateInternalSliderMethodProxy();

        var value = property.intValue;
        EditorGUI.BeginChangeCheck();
        value = Mathf.RoundToInt(method.Invoke(position, content, value, min, max, minLimit, maxLimit));
        if (EditorGUI.EndChangeCheck())
        {
            property.intValue = value;
        }
    }

    public static int IntSlider(this GUIPosition position, string label, int value, int min, int max, int minLimit = int.MinValue, int maxLimit = int.MaxValue)
    {
        var content = string.IsNullOrEmpty(label) ? GUIContent.none : new(label);
        var method = InternalSliderMethod ??= CreateInternalSliderMethodProxy();

        EditorGUI.BeginChangeCheck();
        return Mathf.RoundToInt(method.Invoke(position, content, value, min, max, minLimit, maxLimit));
    }

    private static InternalSliderMethodDelegate CreateInternalSliderMethodProxy()
    {
        var args = new[] { typeof(Rect), typeof(GUIContent), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), };
        var a = new DynamicMethod("Slider", typeof(float), args);

        var inner = typeof(EditorGUI).GetMethod("Slider", BindingFlags.Static | BindingFlags.NonPublic, null, args, null);

        var il = a.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Ldarg_S, 4);
        il.Emit(OpCodes.Ldarg_S, 5);
        il.Emit(OpCodes.Ldarg_S, 6);
        il.Emit(OpCodes.Call, inner);
        il.Emit(OpCodes.Ret);

        return (a.CreateDelegate(typeof(InternalSliderMethodDelegate)) as InternalSliderMethodDelegate)!;
    }

    private delegate float InternalSliderMethodDelegate(Rect position, GUIContent label, float value, float sliderMin, float sliderMax, float textFieldMin, float textFieldMax);

    private static InternalSliderMethodDelegate? InternalSliderMethod;
    
    private static GUIStyle PlaceholderStyle
    {
        get
        {
            if (placeholderStyle == null)
            {
                placeholderStyle = new(EditorStyles.label);
                placeholderStyle.normal.textColor = Color.gray;
                placeholderStyle.padding = new RectOffset(2, 2, 0, 0);
            }
            return placeholderStyle;
        }
    }
    private static GUIStyle? placeholderStyle;
    
    private static GUIStyle PlaceholderTextAreaStyle => placeholderTextAreaStyle ??= new(EditorStyles.textArea)
    {
        padding = new RectOffset(2, 2, 0, 0),
        alignment = TextAnchor.MiddleLeft,
    };
    private static GUIStyle SearchTextAreaStyle => searchTextAreaStyle ??= new(EditorStyles.toolbarSearchField)
    {
        padding = new RectOffset(16, 2, 0, 0),
        alignment = TextAnchor.MiddleLeft,
    };

    private static GUIStyle? placeholderTextAreaStyle;
    private static GUIStyle? searchTextAreaStyle;
}

#endif
