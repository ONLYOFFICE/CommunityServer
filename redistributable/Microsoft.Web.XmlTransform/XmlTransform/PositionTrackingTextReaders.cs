using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Microsoft.Web.XmlTransform
{
    internal class PositionTrackingTextReader : TextReader
    {
        private TextReader internalReader;

        private int lineNumber = 1;
        private int linePosition = 1;
        private int characterPosition = 1;

        private const int newlineCharacter = '\n';

        public PositionTrackingTextReader(TextReader textReader) {
            this.internalReader = textReader;
        }

        public override int Read() {
            int read = internalReader.Read();

            UpdatePosition(read);

            return read;
        }

        public override int Peek() {
            return internalReader.Peek();
        }

        public bool ReadToPosition(int lineNumber, int linePosition) {
            while (this.lineNumber < lineNumber && Peek() != -1) {
                ReadLine();
            }

            while (this.linePosition < linePosition && Peek() != -1) {
                Read();
            }

            return this.lineNumber == lineNumber && this.linePosition == linePosition;
        }

        public bool ReadToPosition(int characterPosition) {
            while (this.characterPosition < characterPosition && Peek() != -1) {
                Read();
            }

            return this.characterPosition == characterPosition;
        }

        private void UpdatePosition(int character) {
            if (character == newlineCharacter) {
                lineNumber++;
                linePosition = 1;
            }
            else {
                linePosition++;
            }
            characterPosition++;
        }
    }

    internal class WhitespaceTrackingTextReader : PositionTrackingTextReader
    {
        private StringBuilder precedingWhitespace = new StringBuilder();

        public WhitespaceTrackingTextReader(TextReader reader)
            : base(reader) {
        }

        public override int Read() {
            int read = base.Read();

            UpdateWhitespaceTracking(read);

            return read;
        }

        public string PrecedingWhitespace {
            get {
                return precedingWhitespace.ToString();
            }
        }

        private void UpdateWhitespaceTracking(int character) {
            if (Char.IsWhiteSpace((char)character)) {
                AppendWhitespaceCharacter(character);
            }
            else {
                ResetWhitespaceString();
            }
        }

        private void AppendWhitespaceCharacter(int character) {
            precedingWhitespace.Append((char)character);
        }

        private void ResetWhitespaceString() {
            precedingWhitespace = new StringBuilder();
        }
    }
}
