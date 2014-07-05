using System;

using ActiveUp.Net.Mail;
using NUnit.Framework;

namespace ActiveUp.Net.Tests
{
    /// <summary>
    /// ActiveUp.Net.Common library related tests.
    /// </summary>
    [TestFixture(Description = "ActiveUp.Net.Common library related tests")]
    public class CommonTests
    {
        #region Fields

        /// <summary>
        /// Headers to be decoded
        /// </summary>
        private static string[] encodedHeaders = new string[]
        {
            "From: =?US-ASCII?Q?Keith_Moore?= <moore@cs.utk.edu>",
            "To: =?ISO-8859-1?Q?Keld_J=F8rn_Simonsen?= <keld@dkuug.dk>",
            "CC: =?ISO-8859-1?Q?Andr=E9?= Pirard <PIRARD@vm1.ulg.ac.be>",
            "Subject: =?ISO-8859-1?B?SWYgeW91IGNhbiByZWFkIHRoaXMgeW8=?=\n=?ISO-8859-2?B?dSB1bmRlcnN0YW5kIHRoZSBleGFtcGxlLg==?=",
            "From: =?ISO-8859-1?Q?Olle_J=E4rnefors?= <ojarnef@admin.kth.se>",
            "To: ietf-822@dimacs.rutgers.edu, ojarnef@admin.kth.se",
            "Subject: Time for ISO 10646?",
            "To: Dave Crocker <dcrocker@mordor.stanford.edu>",
            "Cc: ietf-822@dimacs.rutgers.edu, paf@comsol.se",
            "From: =?ISO-8859-1?Q?Patrik_F=E4ltstr=F6m?= <paf@nada.kth.se>",
            "Subject: Re: RFC-HDR care and feeding",
            "To: Greg Vaudreuil <gvaudre@NRI.Reston.VA.US>, Ned Freed\n <ned@innosoft.com>, Keith Moore <moore@cs.utk.edu>",
            "Subject: Test of new header generator",
            "MIME-Version: 1.0",
            "Content-type: text/plain; charset=ISO-8859-1",
            "(=?ISO-8859-1?Q?a?=)",
            "(=?ISO-8859-1?Q?a?= b)",
               "(=?ISO-8859-1?Q?a?= =?ISO-8859-1?Q?b?=)",
            "(=?ISO-8859-1?Q?a?=  =?ISO-8859-1?Q?b?=)",
            "(=?ISO-8859-1?Q?a?=\n       =?ISO-8859-1?Q?b?=)",
            "(=?ISO-8859-1?Q?a_b?=)",
            "(=?ISO-8859-1?Q?a?= =?ISO-8859-2?Q?_b?=)"
        };

        /// <summary>
        /// Decoded headers
        /// </summary>
        private static string[] decodedHeaders = new string[] 
        { 
            "From: Keith Moore <moore@cs.utk.edu>",
            "To: Keld Jørn Simonsen <keld@dkuug.dk>",
            "CC: André Pirard <PIRARD@vm1.ulg.ac.be>",
            "Subject: If you can read this you understand the example.",
            "From: Olle Järnefors <ojarnef@admin.kth.se>",
            "To: ietf-822@dimacs.rutgers.edu, ojarnef@admin.kth.se",
            "Subject: Time for ISO 10646?",
            "To: Dave Crocker <dcrocker@mordor.stanford.edu>",
            "Cc: ietf-822@dimacs.rutgers.edu, paf@comsol.se",
            "From: Patrik Fältström <paf@nada.kth.se>",
            "Subject: Re: RFC-HDR care and feeding",
            "To: Greg Vaudreuil <gvaudre@NRI.Reston.VA.US>, Ned Freed\n <ned@innosoft.com>, Keith Moore <moore@cs.utk.edu>",
            "Subject: Test of new header generator",
            "MIME-Version: 1.0",
            "Content-type: text/plain; charset=ISO-8859-1",
            "(a)",
            "(a b)",
            "(ab)",
            "(ab)",
            "(ab)",
            "(a b)",
            "(a b)"
        };

        #endregion Fields

        /// <summary>
        /// This test is used to verify that Codec.RFC2047Decode correctly retrieve RFC2047 encoded words
        /// </summary>
        [Test(Description = "This test is used to verify that Codec.RFC2047Decode correctly retrieve RFC2047 encoded words")]
        public void CodecRFC2047DecodeTests()
        {
            // First check example directly taken from RFC2047
            for (int i = 0; i < encodedHeaders.GetLength(0); i++)
            {
                Assert.AreEqual(decodedHeaders[i], Codec.RFC2047Decode(encodedHeaders[i]), string.Format("error in decoding : {0}", encodedHeaders[i]));
            }

            // Second, ensure that an encoded sentence from Codec.RFC2047Encode is correctly decoded
            string decodedText = "Je suis Liégeois et je suis prêt à rencontrer Asger Jørnow";
            string encodedText = Codec.RFC2047Encode(decodedText, "iso-8859-1");
            Assert.AreEqual(decodedText, Codec.RFC2047Decode(encodedText));
        }
    }
}
