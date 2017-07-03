using System.Collections.Generic;
using System.Text;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization
{
    public class EncodingStack : IEncodingStack
    {
        private readonly Stack<Encoding> _mStack;

        public EncodingStack()
        {
            _mStack = new Stack<Encoding>();
        }

        public Encoding Current
        {
            get
            {
                return _mStack.Count > 0
                    ? _mStack.Peek()
                    : Encoding.UTF8;
            }
        }

        public void Push(Encoding encoding)
        {
            if (encoding != null)
            {
                _mStack.Push(encoding);
            }
        }

        public Encoding Pop()
        {
            return _mStack.Count > 0
                ? _mStack.Pop()
                : null;
        }
    }
}