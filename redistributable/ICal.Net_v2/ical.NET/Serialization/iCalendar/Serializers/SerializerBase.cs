using System;
using System.IO;
using System.Text;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Serializers
{
    public abstract class SerializerBase : IStringSerializer
    {
        private ISerializationContext _mSerializationContext;

        protected SerializerBase()
        {
            _mSerializationContext = Serialization.SerializationContext.Default;
        }

        protected SerializerBase(ISerializationContext ctx)
        {
            _mSerializationContext = ctx;
        }

        public virtual ISerializationContext SerializationContext
        {
            get { return _mSerializationContext; }
            set { _mSerializationContext = value; }
        }

        public abstract Type TargetType { get; }
        public abstract string SerializeToString(object obj);
        public abstract object Deserialize(TextReader tr);

        public object Deserialize(Stream stream, Encoding encoding)
        {
            object obj;
            using (var sr = new StreamReader(stream, encoding))
            {
                var encodingStack = GetService<IEncodingStack>();
                encodingStack.Push(encoding);
                obj = Deserialize(sr);
                encodingStack.Pop();
            }
            return obj;
        }

        public void Serialize(object obj, Stream stream, Encoding encoding)
        {
            // NOTE: we don't use a 'using' statement here because
            // we don't want the stream to be closed by this serialization.
            // Fixes bug #3177278 - Serialize closes stream

            var sw = new StreamWriter(stream, encoding);
            // Push the current object onto the serialization stack
            SerializationContext.Push(obj);

            // Push the current encoding on the stack
            var encodingStack = GetService<EncodingStack>();
            encodingStack.Push(encoding);

            sw.Write(SerializeToString(obj));

            // Pop the current encoding off the serialization stack
            encodingStack.Pop();

            // Pop the current object off the serialization stack
            SerializationContext.Pop();
            sw.Flush();
        }

        public virtual object GetService(Type serviceType)
        {
            return SerializationContext?.GetService(serviceType);
        }

        public virtual object GetService(string name)
        {
            return SerializationContext?.GetService(name);
        }

        public virtual T GetService<T>()
        {
            if (SerializationContext != null)
            {
                return SerializationContext.GetService<T>();
            }
            return default(T);
        }

        public virtual T GetService<T>(string name)
        {
            if (SerializationContext != null)
            {
                return SerializationContext.GetService<T>(name);
            }
            return default(T);
        }

        public void SetService(string name, object obj)
        {
            SerializationContext?.SetService(name, obj);
        }

        public void SetService(object obj)
        {
            SerializationContext?.SetService(obj);
        }

        public void RemoveService(Type type)
        {
            SerializationContext?.RemoveService(type);
        }

        public void RemoveService(string name)
        {
            SerializationContext?.RemoveService(name);
        }
    }
}