using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PokemonRandomizer.Backend.Randomization;
using System.Reflection;

namespace PokemonRandomizer.UI.Json
{
    public class WeightedSetJsonConverter : JsonConverterFactory
    {
        private const string itemsProperty = "Items";
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
            private readonly Type _valueType;

            public WeightedSetConverter(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                _valueConverter = (JsonConverter<T>)options.GetConverter(typeof(T));

                // Cache the value type.
                _valueType = typeof(T);
            }

            public override WeightedSet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var set = new WeightedSet<T>();
                bool standAlone = reader.TokenType == JsonTokenType.StartObject;
                if (standAlone)
                {
                    AssertTokenType(ref reader, JsonTokenType.StartObject);
                    AssertTokenType(ref reader, JsonTokenType.PropertyName);
                }
                AssertTokenType(ref reader, JsonTokenType.StartArray);
                while (true)
                {
                    if (reader.TokenType == JsonTokenType.EndArray)// || reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (standAlone)
                        {
                            reader.Read();
                        }
                        return set;
                    }
                    ReadWeight(set, ref reader, options);
                }
                throw new JsonException();
            }

            private void ReadWeight(WeightedSet<T> set, ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                AssertTokenType(ref reader, JsonTokenType.StartObject);
                AssertTokenType(ref reader, JsonTokenType.PropertyName);
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
                AssertTokenType(ref reader, JsonTokenType.PropertyName);

                set.Add(item, (float)reader.GetDouble());
                reader.Read();

                AssertTokenType(ref reader, JsonTokenType.EndObject);
            }

            public override void Write(Utf8JsonWriter writer, WeightedSet<T> set, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(PropertyText(itemsProperty, options));
                writer.WriteStartArray();
                foreach (var kvp in set)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(PropertyText(itemProperty, options));
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
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            private static string PropertyText(string propertyName, JsonSerializerOptions options)
            {
                return options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
            }

            private static void AssertTokenType(ref Utf8JsonReader reader, JsonTokenType type)
            {
                if(reader.TokenType != type)
                {
                    throw new JsonException();
                }
                reader.Read();
            }
        }
    }
}
