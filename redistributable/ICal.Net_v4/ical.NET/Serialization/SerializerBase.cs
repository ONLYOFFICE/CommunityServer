using System;
using System.IO;
using System.Text;

namespace Ical.Net.Serialization
{
    public abstract class SerializerBase : IStringSerializer
    {
        private SerializationContext _mSerializationContext;

        protected SerializerBase()
        {
            _mSerializationContext = SerializationContext.Default;
        }

        protected SerializerBase(SerializationContext ctx)
        {
            _mSerializationContext = ctx;
        }

        public virtual SerializationContext SerializationContext
        {
            get => _mSerializationContext;
            set => _mSerializationContext = value;
        }

        public abstract Type TargetType { get; }
        public abstract string SerializeToString(object obj);
        public abstract object Deserialize(TextReader tr);

        public object Deserialize(Stream stream, Encoding encoding)
        {
            object obj;
            using (var sr = new StreamReader(stream, encoding))
            {
                var encodingStack = GetService<EncodingStack>();
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

            const int defaultBuffer = 1024;     //This is StreamWriter's built-in default buffer size
            using (var sw = new StreamWriter(stream, encoding, defaultBuffer, leaveOpen: true))
            {
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
            }
        }

        public virtual object GetService(Type serviceType) => SerializationContext?.GetService(serviceType);

        public virtual object GetService(string name) => SerializationContext?.GetService(name);

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