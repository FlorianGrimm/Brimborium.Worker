namespace Brimborium.Worker;

public class BWConvertToCode {
    public Dictionary<Type, Action<object, BWConvertToCode, StringBuilder>> Converters = new();

    public BWConvertToCode() {
    }

    public StringBuilder Convert<T>(T value) {
        StringBuilder result = new();
        this.Convert(value, result);
        return result;
    }

    public void Convert<T>(T value, StringBuilder output) {
        if (value is null) {
            output.Append("null");
            return;
        }
        // convert often used value types
        if (typeof(T).Equals(typeof(string))) {
            ConvertStringToCode(value as string, output);
            return;
        }
        if (typeof(T).Equals(typeof(int))) {
            if (value is int valueInt) {
                ConvertIntToCode(valueInt, output);
            }
            return;
        }
        if (typeof(T).Equals(typeof(Nullable<int>))) {
            ConvertIntQToCode(value as int?, output);
            return;
        }
        if (typeof(T).Equals(typeof(double))) {
            if (value is double valueDouble) {
                output.Append(valueDouble.ToString(System.Globalization.CultureInfo.InvariantCulture));
            } else {
                output.Append("null");
            }
            return;
        }
        if (this.Converters.TryGetValue(typeof(T), out var converter)) {
            converter((object)value, this, output);
            return;
        }
        // handle generic types with 1 parameter
        var typeT = typeof(T);
        if (typeT.IsGenericType) {
            var genericDef = typeT.GetGenericTypeDefinition();
            if (genericDef == typeof(List<>)) {
                var elementType = typeT.GetGenericArguments()[0];
                ConvertListTToCode(value as System.Collections.IList, elementType, this, output);
                return;
            }
        }
        // handle generic types with more parameters

        // TODO
    }

    public static void ConvertStringToCode(string? value, StringBuilder output) {
        if (value is string valueString) {
            valueString = valueString.Replace("\\", "\\\\");
            valueString = valueString.Replace("\"", "\\\"");
            valueString = valueString.Replace("\r", "\\r");
            valueString = valueString.Replace("\n", "\\n");
            output.Append('"').Append(valueString).Append('"');
        } else {
            output.Append("null");
        }
    }

    public static void ConvertIntToCode(int valueInt, StringBuilder output) {
        output.Append(valueInt);
    }

    public static void ConvertIntQToCode(int? valueIntQ, StringBuilder output) {
        if (valueIntQ is int valueInt) {
            output.Append(valueInt);
        } else {
            output.Append("null");
        }
    }

    public static void ConvertListTToCode(
            System.Collections.IList? list,
            Type elementType,
            BWConvertToCode converter,
            StringBuilder output) {
        if (list is null) {
            output.Append("null");
            return;
        }
        output.Append("new List<").Append(GetTypeName(elementType)).Append("> {");
        for (int i = 0; i < list.Count; i++) {
            output.Append(i == 0 ? " " : ", ");
            converter.ConvertObject(list[i], elementType, output);
        }
        output.Append(" }");
    }

    public void ConvertObject(object? value, Type type, StringBuilder output) {
        if (value is null) {
            output.Append("null");
            return;
        }
        if (type == typeof(string)) {
            ConvertStringToCode(value as string, output);
            return;
        }
        if (type == typeof(int)) {
            output.Append((int)value);
            return;
        }
        if (type == typeof(double)) {
            output.Append(((double)value).ToString(System.Globalization.CultureInfo.InvariantCulture));
            return;
        }
        if (this.Converters.TryGetValue(type, out var converter)) {
            converter(value, this, output);
            return;
        }
        if (type.IsGenericType) {
            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef == typeof(List<>)) {
                var elementType = type.GetGenericArguments()[0];
                ConvertListTToCode(value as System.Collections.IList, elementType, this, output);
                return;
            }
        }
        output.Append("null");
    }

    public static string GetTypeName(Type type) {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(double)) return "double";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(long)) return "long";
        if (type == typeof(float)) return "float";
        return type.Name;
    }
}