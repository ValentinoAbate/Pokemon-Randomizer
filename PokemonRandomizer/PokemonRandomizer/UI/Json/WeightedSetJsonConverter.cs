using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PokemonRandomizer.Backend.Randomization;
using System.Reflection;

namespace PokemonRandomizer.UI.Json
{
    public class WeightedSetJsonConverter : JsonConverterFactory
    {
        private const string itemProperty = "Item";
        private const string weightProperty = "Weight";
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }
            return typeToConvert.GetGenericTypeDefinition() == typeof(WeightedSet<>);
            //return typeToConvert.GetGenericArguments()[0];
        }

        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            Type itemType = type.GetGenericArguments()[0];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(WeightedSetConverter<>).MakeGenericType(
                    new Type[] { itemType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;
        }

        private class WeightedSetConverter<T> : JsonConverter<WeightedSet<T>>
        {
            private readonly JsonConverter<T> _valueConverter;
            private readonly JsonConverter<float> floatConverter;
            private readonly Type _valueType;

            public WeightedSetConverter(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                _valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));
                floatConverter = (JsonConverter<float>)options.GetConverter(typeof(float));

                // Cache the value type.
                _valueType = typeof(T);
            }

            public override WeightedSet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }
                // Get the value.
                var set = new WeightedSet<T>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return set;
                    }
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }
                    reader.Read();
                    // Get the key.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }
                    reader.Read();
                    T item;
                    if (_valueConverter != null)
                    {
                        item = _valueConverter.Read(ref reader, _valueType, options);
                    }
                    else
                    {
                        item = JsonSerializer.Deserialize<T>(ref reader, options);
                    }
                    reader.Read();
                    // Get the key.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }
                    reader.Read();

                    set.Add(item, (float)reader.GetDouble());
                    reader.Read();

                    if (reader.TokenType != JsonTokenType.EndObject)
                    {
                        throw new JsonException();
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, WeightedSet<T> set, JsonSerializerOptions options)
            {

                foreach (var kvp in set)
                {
                    writer.WriteStartObject();
                    if (_valueConverter != null)
                    {
                        _valueConverter.Write(writer, kvp.Key, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, kvp.Key, options);
                    }
                    writer.WritePropertyName(PropertyText(weightProperty, options));
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                    writer.WriteEndObject();
                }
            }

            private static string PropertyText(string propertyName, JsonSerializerOptions options)
            {
                return options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
            }
        }
    }
}
