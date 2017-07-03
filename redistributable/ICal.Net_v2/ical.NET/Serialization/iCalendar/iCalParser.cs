// $ANTLR 2.7.6 (20061021): "iCal.g" -> "iCalParser.cs"$

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using antlr;
using antlr.collections;
using Ical.Net.ExtensionMethods;
using Ical.Net.General;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.General;
using Ical.Net.Interfaces.Serialization;
using Ical.Net.Interfaces.Serialization.Factory;
using Ical.Net.Serialization.iCalendar.Serializers;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.iCalendar
{
    // Generate the header common to all output files.

    public class iCalParser : LLkParser
    {
        public const int EOF = 1;
        public const int NULL_TREE_LOOKAHEAD = 3;
        public const int CRLF = 4;
        public const int BEGIN = 5;
        public const int COLON = 6;
        public const int VCALENDAR = 7;
        public const int END = 8;
        public const int IANA_TOKEN = 9;
        public const int X_NAME = 10;
        public const int SEMICOLON = 11;
        public const int EQUAL = 12;
        public const int COMMA = 13;
        public const int DQUOTE = 14;
        public const int CTL = 15;
        public const int BACKSLASH = 16;
        public const int NUMBER = 17;
        public const int DOT = 18;
        public const int CR = 19;
        public const int LF = 20;
        public const int ALPHA = 21;
        public const int DIGIT = 22;
        public const int DASH = 23;
        public const int UNDERSCORE = 24;
        public const int UNICODE = 25;
        public const int SPECIAL = 26;
        public const int SPACE = 27;
        public const int HTAB = 28;
        public const int SLASH = 29;
        public const int ESCAPED_CHAR = 30;
        public const int LINEFOLDER = 31;


        protected void initialize()
        {
            tokenNames = tokenNames_;
        }


        protected iCalParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
        {
            initialize();
        }

        public iCalParser(TokenBuffer tokenBuf) : this(tokenBuf, 3) {}

        protected iCalParser(TokenStream lexer, int k) : base(lexer, k)
        {
            initialize();
        }

        public iCalParser(TokenStream lexer) : this(lexer, 3) {}

        public iCalParser(ParserSharedInputState state) : base(state, 3)
        {
            initialize();
        }

        public IICalendarCollection icalendar(ISerializationContext ctx) //throws RecognitionException, TokenStreamException
        {
            IICalendarCollection iCalendars = new CalendarCollection();


            SerializationUtil.OnDeserializing(iCalendars);

            ICalendar cal = null;
            Type foo = typeof(ISerializationSettings);
            var settings = ctx.GetService<ISerializationSettings>();

            { // ( ... )*
                for (;;)
                {
                    if ((LA(1) == CRLF || LA(1) == BEGIN))
                    {
                        { // ( ... )*
                            for (;;)
                            {
                                if ((LA(1) == CRLF))
                                {
                                    match(CRLF);
                                }
                                else
                                {
                                    goto _loop4_breakloop;
                                }
                            }
                            _loop4_breakloop:
                            ;
                        } // ( ... )*
                        match(BEGIN);
                        match(COLON);
                        match(VCALENDAR);
                        { // ( ... )*
                            for (;;)
                            {
                                if ((LA(1) == CRLF))
                                {
                                    match(CRLF);
                                }
                                else
                                {
                                    goto _loop6_breakloop;
                                }
                            }
                            _loop6_breakloop:
                            ;
                        } // ( ... )*

                        var processor = ctx.GetService(typeof(ISerializationProcessor<ICalendar>)) as ISerializationProcessor<ICalendar>;

                        // Do some pre-processing on the calendar:
                        processor?.PreDeserialization(cal);

                        cal = new Calendar();
                        SerializationUtil.OnDeserializing(cal);

                        // Push the iCalendar onto the serialization context stack
                        ctx.Push(cal);

                        icalbody(ctx, cal);
                        match(END);
                        match(COLON);
                        match(VCALENDAR);
                        { // ( ... )*
                            for (;;)
                            {
                                if ((LA(1) == CRLF) && (LA(2) == EOF || LA(2) == CRLF || LA(2) == BEGIN) && (tokenSet_0_.member(LA(3))))
                                {
                                    match(CRLF);
                                }
                                else
                                {
                                    goto _loop8_breakloop;
                                }
                            }
                            _loop8_breakloop:
                            ;
                        } // ( ... )*

                        // Do some final processing on the calendar:
                        processor?.PostDeserialization(cal);

                        // Notify that the iCalendar has been loaded
                        cal.OnLoaded();
                        iCalendars.Add(cal);

                        SerializationUtil.OnDeserialized(cal);

                        // Pop the iCalendar off the serialization context stack
                        ctx.Pop();
                    }
                    else
                    {
                        goto _loop9_breakloop;
                    }
                }
                _loop9_breakloop:
                ;
            } // ( ... )*

            SerializationUtil.OnDeserialized(iCalendars);

            return iCalendars;
        }

        public void icalbody(ISerializationContext ctx, ICalendar cal) //throws RecognitionException, TokenStreamException
        {
            var sf = ctx.GetService(typeof(ISerializerFactory)) as ISerializerFactory;
            var cf = ctx.GetService(typeof(ICalendarComponentFactory)) as ICalendarComponentFactory;

            for (;;)
            {
                switch (LA(1))
                {
                    case IANA_TOKEN:
                    case X_NAME:
                    {
                        property(ctx, cal);
                        break;
                    }
                    case BEGIN:
                    {
                        component(ctx, sf, cf, cal);
                        break;
                    }
                    default:
                    {
                        goto _loop12_breakloop;
                    }
                }
            }
            _loop12_breakloop:
            ;
        }

        public ICalendarProperty property(ISerializationContext ctx, ICalendarPropertyListContainer c) //throws RecognitionException, TokenStreamException
        {
            ICalendarProperty p;
            {
                switch (LA(1))
                {
                    case IANA_TOKEN:
                    {
                        var n = LT(1);
                        match(IANA_TOKEN);

                        p = new CalendarProperty(n.getLine(), n.getColumn())
                        {
                            Name = n.getText().ToUpper()
                        };

                        break;
                    }
                    case X_NAME:
                    {
                        var m = LT(1);
                        match(X_NAME);

                        p = new CalendarProperty(m.getLine(), m.getColumn())
                        {
                            Name = m.getText().ToUpper()
                        };

                        break;
                    }
                    default:
                    {
                        throw new NoViableAltException(LT(1), getFilename());
                    }
                }
            }

            var processor = ctx.GetService(typeof(ISerializationProcessor<ICalendarProperty>)) as ISerializationProcessor<ICalendarProperty>;
            // Do some pre-processing on the property
            processor?.PreDeserialization(p);

            // Add the property to the container, as the parent object(s)
            // may be needed during deserialization.
            c?.Properties.Add(p);

            // Push the property onto the serialization context stack
            ctx.Push(p);
            IStringSerializer dataMapSerializer = new DataMapSerializer(ctx);

            { // ( ... )*
                for (;;)
                {
                    if ((LA(1) == SEMICOLON))
                    {
                        match(SEMICOLON);
                        parameter(ctx, p);
                    }
                    else
                    {
                        goto _loop24_breakloop;
                    }
                }
                _loop24_breakloop:
                ;
            } // ( ... )*
            match(COLON);
            var v = value();

            // Deserialize the value of the property
            // into a concrete iCalendar data type,
            // a list of concrete iCalendar data types,
            // or string value.
            var deserialized = dataMapSerializer.Deserialize(new StringReader(v));
            if (deserialized != null)
            {
                // Try to determine if this is was deserialized as a *list*
                // of concrete types.
                var targetType = dataMapSerializer.TargetType;
                var listOfTargetType = typeof(IList<>).MakeGenericType(targetType);
                if (listOfTargetType.IsInstanceOfType(deserialized))
                {
                    // We deserialized a list - add each value to the
                    // resulting object.
                    foreach (var item in (IEnumerable) deserialized)
                    {
                        p.AddValue(item);
                    }
                }
                else
                {
                    // We deserialized a single value - add it to the object.
                    p.AddValue(deserialized);
                }
            }

            { // ( ... )*
                for (;;)
                {
                    if ((LA(1) == CRLF))
                    {
                        match(CRLF);
                    }
                    else
                    {
                        goto _loop26_breakloop;
                    }
                }
                _loop26_breakloop:
                ;
            } // ( ... )*

            // Do some final processing on the property:
            processor?.PostDeserialization(p);

            // Notify that the property has been loaded
            p.OnLoaded();

            // Pop the property off the serialization context stack
            ctx.Pop();

            return p;
        }

        public ICalendarComponent component(ISerializationContext ctx, ISerializerFactory sf, ICalendarComponentFactory cf, ICalendarObject o)
            //throws RecognitionException, TokenStreamException
        {
            ICalendarComponent c;
            IToken n = null;

            match(BEGIN);
            match(COLON);
            {
                switch (LA(1))
                {
                    case IANA_TOKEN:
                    {
                        n = LT(1);
                        match(IANA_TOKEN);
                        c = cf.Build(n.getText());
                        break;
                    }
                    case X_NAME:
                    {
                        var m = LT(1);
                        match(X_NAME);
                        c = cf.Build(m.getText());
                        break;
                    }
                    default:
                    {
                        throw new NoViableAltException(LT(1), getFilename());
                    }
                }
            }

            var processor = ctx.GetService(typeof(ISerializationProcessor<ICalendarComponent>)) as ISerializationProcessor<ICalendarComponent>;
            // Do some pre-processing on the component
            processor?.PreDeserialization(c);

            SerializationUtil.OnDeserializing(c);

            // Push the component onto the serialization context stack
            ctx.Push(c);

            // Add the component as a child immediately, in case
            // embedded components need to access this component,
            // or the iCalendar itself.
            o?.AddChild(c);

            c.Line = n.getLine();
            c.Column = n.getColumn();

            { // ( ... )*
                for (;;)
                {
                    if ((LA(1) == CRLF))
                    {
                        match(CRLF);
                    }
                    else
                    {
                        goto _loop16_breakloop;
                    }
                }
                _loop16_breakloop:
                ;
            } // ( ... )*
            { // ( ... )*
                for (;;)
                {
                    switch (LA(1))
                    {
                        case IANA_TOKEN:
                        case X_NAME:
                        {
                            property(ctx, c);
                            break;
                        }
                        case BEGIN:
                        {
                            component(ctx, sf, cf, c);
                            break;
                        }
                        default:
                        {
                            goto _loop18_breakloop;
                        }
                    }
                }
                _loop18_breakloop:
                ;
            } // ( ... )*
            match(END);
            match(COLON);
            match(IANA_TOKEN);
            { // ( ... )*
                for (;;)
                {
                    if ((LA(1) == CRLF))
                    {
                        match(CRLF);
                    }
                    else
                    {
                        goto _loop20_breakloop;
                    }
                }
                _loop20_breakloop:
                ;
            } // ( ... )*

            // Do some final processing on the component
            processor?.PostDeserialization(c);

            // Notify that the component has been loaded
            c.OnLoaded();

            SerializationUtil.OnDeserialized(c);

            // Pop the component off the serialization context stack
            ctx.Pop();

            return c;
        }

        public CalendarParameter parameter(ISerializationContext ctx, ICalendarParameterCollectionContainer container)
        {
            CalendarParameter p;
            var values = new List<string>();

            switch (LA(1))
            {
                case IANA_TOKEN:
                {
                    var n = LT(1);
                    match(IANA_TOKEN);
                    p = new CalendarParameter(n.getText());
                    break;
                }
                case X_NAME:
                {
                    var m = LT(1);
                    match(X_NAME);
                    p = new CalendarParameter(m.getText());
                    break;
                }
                default:
                {
                    throw new NoViableAltException(LT(1), getFilename());
                }
            }

            // Push the parameter onto the serialization context stack
            ctx.Push(p);

            match(EQUAL);
            var v = param_value();
            values.Add(v);
            { // ( ... )*
                for (;;)
                {
                    if ((LA(1) == COMMA))
                    {
                        match(COMMA);
                        v = param_value();
                        values.Add(v);
                    }
                    else
                    {
                        goto _loop30_breakloop;
                    }
                }
                _loop30_breakloop:
                ;
            } // ( ... )*

            p.SetValue(values);

            container?.Parameters.Add(p);

            // Notify that the parameter has been loaded
            p.OnLoaded();

            // Pop the parameter off the serialization context stack
            ctx.Pop();

            return p;
        }

        public string value() //throws RecognitionException, TokenStreamException
        {
            var sb = new StringBuilder();

            { // ( ... )*
                for (;;)
                {
                    if ((tokenSet_1_.member(LA(1))) && (tokenSet_2_.member(LA(2))) && (tokenSet_2_.member(LA(3))))
                    {
                        var c = value_char();
                        sb.Append(c);
                    }
                    else
                    {
                        goto _loop37_breakloop;
                    }
                }
                _loop37_breakloop:
                ;
            } // ( ... )*
            var v = sb.ToString();
            return v;
        }

        public string param_value() //throws RecognitionException, TokenStreamException
        {
            string v;


            switch (LA(1))
            {
                case BEGIN:
                case COLON:
                case VCALENDAR:
                case END:
                case IANA_TOKEN:
                case X_NAME:
                case SEMICOLON:
                case EQUAL:
                case COMMA:
                case BACKSLASH:
                case NUMBER:
                case DOT:
                case CR:
                case LF:
                case ALPHA:
                case DIGIT:
                case DASH:
                case UNDERSCORE:
                case UNICODE:
                case SPECIAL:
                case SPACE:
                case HTAB:
                case SLASH:
                case ESCAPED_CHAR:
                case LINEFOLDER:
                {
                    v = paramtext();
                    break;
                }
                case DQUOTE:
                {
                    v = quoted_string();
                    break;
                }
                default:
                {
                    throw new NoViableAltException(LT(1), getFilename());
                }
            }
            return v;
        }

        public string paramtext() //throws RecognitionException, TokenStreamException
        {
            var sb = new StringBuilder();

            { // ( ... )*
                for (;;)
                {
                    if ((tokenSet_3_.member(LA(1))))
                    {
                        var c = safe_char();
                        sb.Append(c);
                    }
                    else
                    {
                        goto _loop34_breakloop;
                    }
                }
                _loop34_breakloop:
                ;
            } // ( ... )*
            var s = sb.ToString();
            return s;
        }

        public string quoted_string() //throws RecognitionException, TokenStreamException
        {
            var sb = new StringBuilder();

            match(DQUOTE);
            { // ( ... )*
                for (;;)
                {
                    if ((tokenSet_4_.member(LA(1))))
                    {
                        var c = qsafe_char();
                        sb.Append(c);
                    }
                    else
                    {
                        goto _loop40_breakloop;
                    }
                }
                _loop40_breakloop:
                ;
            } // ( ... )*
            match(DQUOTE);
            var s = sb.ToString();
            return s;
        }

        public string safe_char() //throws RecognitionException, TokenStreamException
        {
            var a = LT(1);
            match(tokenSet_3_);
            var c = a.getText();
            return c;
        }

        public string value_char() //throws RecognitionException, TokenStreamException
        {
            var a = LT(1);
            match(tokenSet_1_);
            var c = a.getText();
            return c;
        }

        public string qsafe_char() //throws RecognitionException, TokenStreamException
        {
            var a = LT(1);
            match(tokenSet_4_);
            var c = a.getText();
            return c;
        }

        public string tsafe_char() //throws RecognitionException, TokenStreamException
        {
            var a = LT(1);
            match(tokenSet_5_);
            var s = a.getText();
            return s;
        }

        public string text_char() //throws RecognitionException, TokenStreamException
        {
            var a = LT(1);
            match(tokenSet_6_);
            var s = a.getText();
            return s;
        }

        public string text() //throws RecognitionException, TokenStreamException
        {
			var sb = new StringBuilder();

            { // ( ... )*
                for (;;)
                {
                    if ((tokenSet_6_.member(LA(1))))
                    {
                        var t = text_char();
                        sb.Append(t);
                    }
                    else
                    {
                        goto _loop53_breakloop;
                    }
                }
                _loop53_breakloop:
                ;
            } // ( ... )*
            return sb.ToString();
        }

        public string number() //throws RecognitionException, TokenStreamException
        {
			var sb = new StringBuilder();
            var n1 = LT(1);
            match(NUMBER);
            sb.Append(n1.getText());
            {
                switch (LA(1))
                {
                    case DOT:
                    {
                        match(DOT);
                        sb.Append(".");
                        var n2 = LT(1);
                        match(NUMBER);
                        sb.Append(n2.getText());
                        break;
                    }
                    case EOF:
                    case SEMICOLON:
                    {
                        break;
                    }
                    default:
                    {
                        throw new NoViableAltException(LT(1), getFilename());
                    }
                }
            }
            return sb.ToString();
        }

        public string version_number() //throws RecognitionException, TokenStreamException
        {
			var sb = new StringBuilder();
            var t = number();
            sb.Append(t);
            {
                switch (LA(1))
                {
                    case SEMICOLON:
                    {
                        match(SEMICOLON);
                        sb.Append(";");
                        t = number();
                        sb.Append(t);
                        break;
                    }
                    case EOF:
                    {
                        break;
                    }
                    default:
                    {
                        throw new NoViableAltException(LT(1), getFilename());
                    }
                }
            }
            return sb.ToString();
        }

        public static readonly string[] tokenNames_ =
        {
            @"""<0>""", @"""EOF""", @"""<2>""", @"""NULL_TREE_LOOKAHEAD""", @"""CRLF""", @"""BEGIN""", @"""COLON""",
            @"""VCALENDAR""", @"""END""", @"""IANA_TOKEN""", @"""X_NAME""", @"""SEMICOLON""", @"""EQUAL""", @"""COMMA""", @"""DQUOTE""", @"""CTL""",
            @"""BACKSLASH""", @"""NUMBER""", @"""DOT""", @"""CR""", @"""LF""", @"""ALPHA""", @"""DIGIT""", @"""DASH""", @"""UNDERSCORE""", @"""UNICODE""",
            @"""SPECIAL""", @"""SPACE""", @"""HTAB""", @"""SLASH""", @"""ESCAPED_CHAR""", @"""LINEFOLDER"""
        };

        private static long[] mk_tokenSet_0_()
        {
            long[] data = {114L, 0L};
            return data;
        }

        public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());

        private static long[] mk_tokenSet_1_()
        {
            long[] data = {4294934496L, 0L, 0L, 0L};
            return data;
        }

        public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());

        private static long[] mk_tokenSet_2_()
        {
            long[] data = {4294934512L, 0L, 0L, 0L};
            return data;
        }

        public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());

        private static long[] mk_tokenSet_3_()
        {
            long[] data = {4294907808L, 0L, 0L, 0L};
            return data;
        }

        public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());

        private static long[] mk_tokenSet_4_()
        {
            long[] data = {4294918112L, 0L, 0L, 0L};
            return data;
        }

        public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());

        private static long[] mk_tokenSet_5_()
        {
            long[] data = {4294842272L, 0L, 0L, 0L};
            return data;
        }

        public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());

        private static long[] mk_tokenSet_6_()
        {
            long[] data = {4294868960L, 0L, 0L, 0L};
            return data;
        }

        public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
    }
}