//-----------------------------------------------------------------------
// <copyright file="TwitterEntityCollection.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net)
// 
//  Copyright (c) 2010, Patrick "Ricky" Smith (ricky@digitally-born.com)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Twitterizer nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <author>Ricky Smith</author>
// <summary>The twitter entity collection class</summary>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace Twitterizer.Entities
{
    using System;
    using System.Linq;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
using System.Linq.Expressions;
    using System.Reflection;
    using System.Globalization;

    /// <summary>
    /// Represents multiple <see cref="Twitterizer.Entities.TwitterEntity"/> objects.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterEntityCollection : Collection<TwitterEntity>
    {
        /// <summary>
        /// The Json converter for <see cref="TwitterEntityCollection"/> data.
        /// </summary>
#if !SILVERLIGHT         
        internal class Converter : JsonConverter
#else
        public class Converter : JsonConverter
#endif
        {
            /// <summary>
            /// Determines whether this instance can convert the specified object type.
            /// </summary>
            /// <param name="objectType">Type of the object.</param>
            /// <returns>
            /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
            /// </returns>
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(TwitterEntityCollection);
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
            /// <param name="objectType">Type of the object.</param>
            /// <param name="existingValue">The existing value of object being read.</param>
            /// <param name="serializer">The calling serializer.</param>
            /// <returns>The object value.</returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                TwitterEntityCollection result = existingValue as TwitterEntityCollection;
                if (result == null)
                    result = new TwitterEntityCollection();

                int startDepth = reader.Depth;
                string entityType = string.Empty;
                TwitterEntity entity = null;
                try
                {
                    if (reader.TokenType == JsonToken.StartArray)
                        reader.Read();

                    while (reader.Read() && reader.Depth > startDepth)
                    {
                        if (reader.TokenType == JsonToken.PropertyName && reader.Depth == startDepth + 1)
                        {
                            entityType = (string)reader.Value;
                            continue;
                        }

                        switch (entityType)
                        {
                            case "urls":
                                if (reader.TokenType == JsonToken.StartObject)
                                    entity = new TwitterUrlEntity();

                                TwitterUrlEntity tue = entity as TwitterUrlEntity;
                                if (tue != null)
                                {
                                    ReadFieldValue(reader, "url", entity, () => tue.Url);
                                    ReadFieldValue(reader, "display_url", entity, () => tue.DisplayUrl);
                                    ReadFieldValue(reader, "expanded_url", entity, () => tue.ExpandedUrl);
                                }
                                break;

                            case "user_mentions":
                                if (reader.TokenType == JsonToken.StartObject)
                                    entity = new TwitterMentionEntity();

                                TwitterMentionEntity tme = entity as TwitterMentionEntity;
                                if (tme != null)
                                {
                                    ReadFieldValue(reader, "screen_name", entity, () => tme.ScreenName);
                                    ReadFieldValue(reader, "name", entity, () => tme.Name);
                                    ReadFieldValue(reader, "id", entity, () => tme.UserId);
                                }
                                break;

                            case "hashtags":
                                if (reader.TokenType == JsonToken.StartObject)
                                    entity = new TwitterHashTagEntity();

                                TwitterHashTagEntity the = entity as TwitterHashTagEntity;
                                if (the != null)
                                {
                                    ReadFieldValue(reader, "text", entity, () => the.Text);
                                }
                                break;

                            case "media":
                                // Move to object start and parse the entity
                                reader.Read();
                                entity = parseMediaEntity(reader);

                                break;
                        }

                        // Read the indicies (for all entities except Media)
                        if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "indices" && entity != null)
                        {
                            reader.Read();
                            reader.Read();
                            entity.StartIndex = Convert.ToInt32((long)reader.Value);
                            reader.Read();
                            entity.EndIndex = Convert.ToInt32((long)reader.Value);
                        }

                        if ((reader.TokenType == JsonToken.EndObject && entity != null) || entity is TwitterMediaEntity)
                        {
                            result.Add(entity);
                            entity = null;
                        }
                    }
                }
                catch { }

                return result;
            }

            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
            /// <param name="value">The value.</param>
            /// <param name="serializer">The calling serializer.</param>
            /// <remarks>This is a best attempt to recreate the structure created by the Twitter API.</remarks>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                TwitterEntityCollection entities = (TwitterEntityCollection)value;

                writer.WriteStartObject();
                {
                    WriteEntity(writer, entities.OfType<TwitterHashTagEntity>().ToList(), "hashtags", (w, e) =>
                        {
                            w.WritePropertyName("text");
                            w.WriteValue(e.Text);
                        });

                    WriteEntity(writer, entities.OfType<TwitterMentionEntity>().ToList(), "user_mentions", (w, e) =>
                        {
                            w.WritePropertyName("screen_name");
                            w.WriteValue(e.ScreenName);

                            w.WritePropertyName("name");
                            w.WriteValue(e.Name);

                            w.WritePropertyName("id");
                            w.WriteValue(e.UserId);
                        });

                    WriteEntity(writer, entities.OfType<TwitterUrlEntity>().ToList(), "urls", (w, e) =>
                        {
                            w.WritePropertyName("url");
                            w.WriteValue(e.Url);

                            w.WritePropertyName("display_url");
                            w.WriteValue(e.DisplayUrl);

                            w.WritePropertyName("expanded_url");
                            w.WriteValue(e.ExpandedUrl);
                        });

                    WriteEntity(writer, entities.OfType<TwitterMediaEntity>().ToList(), "media", WriteMediaEntity);

                    writer.WriteEndObject();
                }
            }

            /// <summary>
            /// Writes the media entity.
            /// </summary>
            /// <param name="w">The w.</param>
            /// <param name="e">The e.</param>
            private static void WriteMediaEntity(JsonWriter w, TwitterMediaEntity e)
            {
                w.WritePropertyName("type");
                switch (e.MediaType)
                {
                    case TwitterMediaEntity.MediaTypes.Unknown:
                        w.WriteNull();
                        break;
                    case TwitterMediaEntity.MediaTypes.Photo:
                        w.WriteValue("photo");
                        break;
                    default:
                        break;
                }

                w.WritePropertyName("sizes");
                w.WriteStartObject();
                {
                    foreach (var item in e.Sizes)
                    {
                        w.WritePropertyName(item.Size.ToString().ToLower());
                        w.WriteStartObject();
                        {
                            w.WritePropertyName("h");
                            w.WriteValue(item.Height);
                            
                            w.WritePropertyName("w");
                            w.WriteValue(item.Width);

                            w.WritePropertyName("resize");
                            w.WriteValue(item.Resize == TwitterMediaEntity.MediaSize.MediaSizeResizes.Fit ? "fit" : "crop");
                            w.WriteEndObject();
                        }
                    }
                  
                    w.WriteEndObject();
                }

                w.WritePropertyName("id");
                w.WriteValue(e.Id);
                
                w.WritePropertyName("id_str");
                w.WriteValue(e.IdString);

                w.WritePropertyName("media_url");
                w.WriteValue(e.MediaUrl);

                w.WritePropertyName("media_url_https");
                w.WriteValue(e.MediaUrlSecure);

                w.WritePropertyName("url");
                w.WriteValue(e.Url);

                w.WritePropertyName("display_url");
                w.WriteValue(e.DisplayUrl);

                w.WritePropertyName("expanded_url");
                w.WriteValue(e.ExpandedUrl);
            }

            /// <summary>
            /// Writes an entity.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="writer">The writer.</param>
            /// <param name="entities">The entities.</param>
            /// <param name="entityName">Name of the entity.</param>
            /// <param name="detailsAction">The details action.</param>
            private static void WriteEntity<T>(JsonWriter writer, IEnumerable<T> entities, string entityName, Action<JsonWriter, T> detailsAction)
                where T : TwitterEntity
            {
                // Note to people reading this code: Extra brackets exist to group code by json hierarchy. You're welcome.
                writer.WritePropertyName(entityName);
                writer.WriteStartArray();
                {
                    foreach (var item in entities)
                    {
                        writer.WriteStartObject();
                        {
                            writer.WritePropertyName("indices");
                            writer.WriteStartArray();
                            {
                                writer.WriteValue(item.StartIndex);
                                writer.WriteValue(item.EndIndex);
                                writer.WriteEndArray();
                            }

                            detailsAction(writer, item);

                            writer.WriteEndObject();
                        }
                    }

                    writer.WriteEndArray();
                }
            }

            /// <summary>
            /// Parses the media entity.
            /// </summary>
            /// <param name="reader">The reader.</param>
            /// <returns></returns>
            public TwitterMediaEntity parseMediaEntity(JsonReader reader)
            {
                try
                {
                    if (reader.TokenType != JsonToken.StartObject)
                        return null;

                    TwitterMediaEntity entity = new TwitterMediaEntity();

                    int startDepth = reader.Depth;

                    // Start looping through all of the child nodes
                    while (reader.Read() && reader.Depth >= startDepth)
                    {
                        // If the current node isn't a property, skip it
                        if (reader.TokenType != JsonToken.PropertyName)
                        {
                            continue;
                        }

                        string fieldName = reader.Value as string;
                        if (string.IsNullOrEmpty(fieldName))
                        {
                            continue;
                        }

                        switch (fieldName)
                        {
                            case "type":
                                entity.MediaType = string.IsNullOrEmpty((string)reader.Value) ?
                                    TwitterMediaEntity.MediaTypes.Unknown :
                                    TwitterMediaEntity.MediaTypes.Photo;
                                break;

                            case "sizes":
                                entity.Sizes = new List<TwitterMediaEntity.MediaSize>();
                                break;

                            case "large":
                            case "medium":
                            case "small":
                            case "thumb":
                                if (reader.TokenType != JsonToken.PropertyName)
                                {
                                    break;
                                }

                                TwitterMediaEntity.MediaSize newSize = new TwitterMediaEntity.MediaSize();

                                switch ((string)reader.Value)
                                {
                                    case "large":
                                        newSize.Size = TwitterMediaEntity.MediaSize.MediaSizes.Large;
                                        break;
                                    case "medium":
                                        newSize.Size = TwitterMediaEntity.MediaSize.MediaSizes.Medium;
                                        break;
                                    case "small":
                                        newSize.Size = TwitterMediaEntity.MediaSize.MediaSizes.Small;
                                        break;
                                    case "thumb":
                                        newSize.Size = TwitterMediaEntity.MediaSize.MediaSizes.Thumb;
                                        break;
                                    default:
                                        break;
                                }
                                
                                int sizeDepth = reader.Depth;
                                // Loop through all of the properties of the size and read their values
                                while (reader.Read() && sizeDepth < reader.Depth)
                                {
                                    if (reader.TokenType != JsonToken.PropertyName)
                                    {
                                        continue;
                                    }

                                    ReadFieldValue(reader, "h", newSize, () => newSize.Height);
                                    ReadFieldValue(reader, "w", newSize, () => newSize.Width);

                                    if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "resize")
                                    {
                                        reader.Read();
                                        newSize.Resize = string.IsNullOrEmpty((string)reader.Value) ?
                                            TwitterMediaEntity.MediaSize.MediaSizeResizes.Unknown : 
                                            ((string)reader.Value == "fit" ? TwitterMediaEntity.MediaSize.MediaSizeResizes.Fit : TwitterMediaEntity.MediaSize.MediaSizeResizes.Crop);
                                    }
                                }

                                entity.Sizes.Add(newSize);

                                break;
                            case "indices":
                                reader.Read();
                                reader.Read();
                                entity.StartIndex = Convert.ToInt32((long)reader.Value);
                                reader.Read();
                                entity.EndIndex = Convert.ToInt32((long)reader.Value);
                                break;
                            default:
                                break;
                        }

                        ReadFieldValue(reader, "id", entity, () => entity.Id);
                        ReadFieldValue(reader, "id_str", entity, () => entity.IdString);
                        ReadFieldValue(reader, "media_url", entity, () => entity.MediaUrl);
                        ReadFieldValue(reader, "media_url_https", entity, () => entity.MediaUrlSecure);
                        ReadFieldValue(reader, "url", entity, () => entity.Url);
                        ReadFieldValue(reader, "display_url", entity, () => entity.DisplayUrl);
                        ReadFieldValue(reader, "expanded_url", entity, () => entity.ExpandedUrl);
                    }
                    return entity;
                }
                catch
                {
                    return null;
                }
            }

            private bool ReadFieldValue<T>(JsonReader reader, string fieldName, ref T result)
            {
                try
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                        return false;

                    if ((string)reader.Value != fieldName)
                        return false;

                    reader.Read();

                    if (reader.ValueType == typeof(T))
                    {
                        result = (T)reader.Value;
                    }
                    else
                    {
#if !SILVERLIGHT
                        result = (T)Convert.ChangeType(reader.Value, typeof(T));
#endif
#if SILVERLIGHT
                        result = (T)Convert.ChangeType(reader.Value, typeof(T), CultureInfo.InvariantCulture);
#endif

                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private void ReadFieldValue<TSource, TProperty>(JsonReader reader, string fieldName, TSource source, Expression<Func<TProperty>> property)
                where TSource : class
            {
                try
                {
                    if (reader == null || source == null)
                    {
                        return /*false*/;
                    }

                    var expr = (MemberExpression)property.Body;
                    var prop = (PropertyInfo)expr.Member;

                    TProperty value = (TProperty)prop.GetValue(source, null);
                    if (ReadFieldValue(reader, fieldName, ref value))
                    {
                        prop.SetValue(source, value, null);
                        return /*true*/;
                    }

                    return /*false*/;
                }
                catch
                {
                    return /*false*/;
                }
            }
        }
    }
}
