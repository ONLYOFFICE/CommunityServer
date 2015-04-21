using System;
using System.Collections.Generic;

namespace Jurassic.Library
{

    /// <summary>
    /// Represents a container for property names and attributes.
    /// </summary>
    [Serializable]
    internal class HiddenClassSchema
    {
        // Properties
        private Dictionary<string, SchemaProperty> properties;

        // Transitions
        [Serializable]
        private struct TransitionInfo
        {
            public string Name;
            public PropertyAttributes Attributes;
        }
        private Dictionary<TransitionInfo, HiddenClassSchema> addTransitions;
        private Dictionary<string, HiddenClassSchema> deleteTransitions;
        private Dictionary<TransitionInfo, HiddenClassSchema> modifyTransitions;

        // The index of the next value.
        private int nextValueIndex;

        // Used to recreate the properties dictionary if properties == null.
        private HiddenClassSchema parent;
        private TransitionInfo addPropertyTransitionInfo;

        /// <summary>
        /// Creates a new HiddenClassSchema instance from a modify or delete operation.
        /// </summary>
        private HiddenClassSchema(Dictionary<string, SchemaProperty> properties, int nextValueIndex)
        {
            this.properties = properties;
            this.addTransitions = null;
            this.deleteTransitions = null;
            this.modifyTransitions = null;
            this.nextValueIndex = nextValueIndex;
        }

        /// <summary>
        /// Creates a new HiddenClassSchema instance from an add operation.
        /// </summary>
        private HiddenClassSchema(Dictionary<string, SchemaProperty> properties, int nextValueIndex, HiddenClassSchema parent, TransitionInfo addPropertyTransitionInfo)
            : this(properties, nextValueIndex)
        {
            this.parent = parent;
            this.addPropertyTransitionInfo = addPropertyTransitionInfo;
        }

        /// <summary>
        /// Creates a hidden class schema with no properties.
        /// </summary>
        /// <returns> A hidden class schema with no properties. </returns>
        public static HiddenClassSchema CreateEmptySchema()
        {
            return new HiddenClassSchema(new Dictionary<string, SchemaProperty>(), 0);
        }

        /// <summary>
        /// Gets the number of properties defined in this schema.
        /// </summary>
        public int PropertyCount
        {
            get
            {
                if (this.properties == null)
                    this.properties = CreatePropertiesDictionary();
                return this.properties.Count;
            }
        }

        /// <summary>
        /// Gets the index into the Values array of the next added property.
        /// </summary>
        public int NextValueIndex
        {
            get { return this.nextValueIndex; }
        }

        /// <summary>
        /// Enumerates the property names and values for this schema.
        /// </summary>
        /// <param name="values"> The array containing the property values. </param>
        /// <returns> An enumerable collection of property names and values. </returns>
        public IEnumerable<PropertyNameAndValue> EnumeratePropertyNamesAndValues(object[] values)
        {
            if (this.properties == null)
                this.properties = CreatePropertiesDictionary();
            this.parent = null;     // Prevents the properties dictionary from being stolen while an enumeration is in progress.
            foreach (var pair in this.properties)
                yield return new PropertyNameAndValue(pair.Key, new PropertyDescriptor(values[pair.Value.Index], pair.Value.Attributes));
        }

        /// <summary>
        /// Gets the zero-based index of the property with the given name.
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The zero-based index of the property, or <c>-1</c> if a property with the
        /// given name does not exist. </returns>
        public int GetPropertyIndex(string name)
        {
            return GetPropertyIndexAndAttributes(name).Index;
        }

        /// <summary>
        /// Gets the zero-based index of the property with the given name and the attributes
        /// associated with the property.
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> A structure containing the zero-based index of the property, or <c>-1</c> if a property with the
        /// given name does not exist. </returns>
        public SchemaProperty GetPropertyIndexAndAttributes(string name)
        {
            if (this.properties == null)
                this.properties = CreatePropertiesDictionary();
            SchemaProperty propertyInfo;
            if (this.properties.TryGetValue(name, out propertyInfo) == false)
                return SchemaProperty.Undefined;
            return propertyInfo;
        }

        /// <summary>
        /// Adds a property to the schema.
        /// </summary>
        /// <param name="name"> The name of the property to add. </param>
        /// <param name="attributes"> The property attributes. </param>
        /// <returns> A new schema with the extra property. </returns>
        public HiddenClassSchema AddProperty(string name, PropertyAttributes attributes = PropertyAttributes.FullAccess)
        {
            // Package the name and attributes into a struct.
            var transitionInfo = new TransitionInfo() { Name = name, Attributes = attributes };

            // Check if there is a transition to the schema already.
            HiddenClassSchema newSchema = null;
            if (this.addTransitions != null)
                this.addTransitions.TryGetValue(transitionInfo, out newSchema);

            if (newSchema == null)
            {
                if (this.parent == null)
                {
                    // Create a new schema based on this one.  A complete copy must be made of the properties hashtable.
                    var properties = new Dictionary<string, SchemaProperty>(this.properties);
                    properties.Add(name, new SchemaProperty(this.NextValueIndex, attributes));
                    newSchema = new HiddenClassSchema(properties, this.NextValueIndex + 1, this, transitionInfo);
                }
                else
                {
                    // Create a new schema based on this one.  The properties hashtable is "given
                    // away" so a copy does not have to be made.
                    if (this.properties == null)
                        this.properties = CreatePropertiesDictionary();
                    this.properties.Add(name, new SchemaProperty(this.NextValueIndex, attributes));
                    newSchema = new HiddenClassSchema(this.properties, this.NextValueIndex + 1, this, transitionInfo);
                    this.properties = null;
                }
                

                // Add a transition to the new schema.
                if (this.addTransitions == null)
                    this.addTransitions = new Dictionary<TransitionInfo, HiddenClassSchema>(1);
                this.addTransitions.Add(transitionInfo, newSchema);
            }

            return newSchema;
        }

        /// <summary>
        /// Deletes a property from the schema.
        /// </summary>
        /// <param name="name"> The name of the property to delete. </param>
        /// <returns> A new schema without the property. </returns>
        public HiddenClassSchema DeleteProperty(string name)
        {
            // Check if there is a transition to the schema already.
            HiddenClassSchema newSchema = null;
            if (this.deleteTransitions != null)
                this.deleteTransitions.TryGetValue(name, out newSchema);

            if (newSchema == null)
            {
                // Create a new schema based on this one.
                var properties = this.properties == null ? CreatePropertiesDictionary() : new Dictionary<string, SchemaProperty>(this.properties);
                if (properties.Remove(name) == false)
                    throw new InvalidOperationException(string.Format("The property '{0}' does not exist.", name));
                newSchema = new HiddenClassSchema(properties, this.NextValueIndex);

                // Add a transition to the new schema.
                if (this.deleteTransitions == null)
                    this.deleteTransitions = new Dictionary<string, HiddenClassSchema>(1);
                this.deleteTransitions.Add(name, newSchema);
            }

            return newSchema;
        }

        /// <summary>
        /// Modifies the attributes for a property in the schema.
        /// </summary>
        /// <param name="name"> The name of the property to modify. </param>
        /// <param name="attributes"> The new attributes. </param>
        /// <returns> A new schema with the modified property. </returns>
        public HiddenClassSchema SetPropertyAttributes(string name, PropertyAttributes attributes)
        {
            // Package the name and attributes into a struct.
            var transitionInfo = new TransitionInfo() { Name = name, Attributes = attributes };

            // Check if there is a transition to the schema already.
            HiddenClassSchema newSchema = null;
            if (this.modifyTransitions != null)
                this.modifyTransitions.TryGetValue(transitionInfo, out newSchema);

            if (newSchema == null)
            {
                // Create the properties dictionary if it hasn't already been created.
                if (this.properties == null)
                    this.properties = CreatePropertiesDictionary();

                // Check the attributes differ from the existing attributes.
                SchemaProperty propertyInfo;
                if (this.properties.TryGetValue(name, out propertyInfo) == false)
                    throw new InvalidOperationException(string.Format("The property '{0}' does not exist.", name));
                if (attributes == propertyInfo.Attributes)
                    return this;

                // Create a new schema based on this one.
                var properties = new Dictionary<string, SchemaProperty>(this.properties);
                properties[name] = new SchemaProperty(propertyInfo.Index, attributes);
                newSchema = new HiddenClassSchema(properties, this.NextValueIndex);

                // Add a transition to the new schema.
                if (this.modifyTransitions == null)
                    this.modifyTransitions = new Dictionary<TransitionInfo, HiddenClassSchema>(1);
                this.modifyTransitions.Add(transitionInfo, newSchema);
            }

            return newSchema;
        }

        /// <summary>
        /// Creates the properties dictionary.
        /// </summary>
        private Dictionary<string, SchemaProperty> CreatePropertiesDictionary()
        {
            // Search up the tree until a schema is found with a populated properties hashtable, 
            // while keeping a list of the transitions.

            var addTransitions = new Stack<KeyValuePair<string, SchemaProperty>>();
            var node = this;
            while (node != null)
            {
                if (node.properties == null)
                {
                    // The schema is the same as the parent schema except with the addition of a single
                    // property.
                    addTransitions.Push(new KeyValuePair<string, SchemaProperty>(
                        node.addPropertyTransitionInfo.Name,
                        new SchemaProperty(node.NextValueIndex - 1, node.addPropertyTransitionInfo.Attributes)));
                }
                else
                {
                    // The schema has a populated properties hashtable - we can stop here.
                    break;
                }
                node = node.parent;
            }
            if (node == null)
                throw new InvalidOperationException("Internal error: no route to a populated schema was found.");

            // Add the properties to the hashtable in order.
            var result = new Dictionary<string, SchemaProperty>(node.properties);
            while (addTransitions.Count > 0)
            {
                var keyValuePair = addTransitions.Pop();
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return result;
        }
    }

}
