using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PokemonRandomizer.UI.Json
{
    using System.Reflection;
    using Utilities;
    public class BoxJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }
            return typeToConvert.GetGenericTypeDefinition() != typeof(Box<>);
            //return typeToConvert.GetGenericArguments()[0];
        }

        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type valueType = type.GetGenericArguments()[0];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(BoxConverterInner<>).MakeGenericType(
                    new Type[] { valueType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;
        }

        private class BoxConverterInner<T> : JsonConverter<Box<T>> where T : struct, Enum
        {
            private readonly JsonConverter<T> _valueConverter;
            private readonly Type _valueType;

            public BoxConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                _valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));

                // Cache the value type.
                _valueType = typeof(T);
            }

            public override Box<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }
                reader.Read();
                // Get the value.
                T value;
                if (_valueConverter != null)
                {
                    reader.Read();
                    value = _valueConverter.Read(ref reader, _valueType, options);
                }
                else
                {
                    value = JsonSerializer.Deserialize<T>(ref reader, options);
                }
                reader.Read();
                return new Box<T>(value);
            }

            public override void Write(Utf8JsonWriter writer, Box<T> box, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                if (_valueConverter != null)
                {
                    _valueConverter.Write(writer, box.Value, options);
                }
                else
                {
                    JsonSerializer.Serialize(writer, box.Value, options);
                }
                writer.WriteEndObject();
            }
        }
    }
}
