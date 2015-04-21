#region Copyright © 2005 Peter G Jones. All rights reserved.
/*
Copyright © 2005 Peter G Jones. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Data.SqlClient;
using System.IO;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Executes a SQL command.
    /// </summary>
    /// <remarks>
    /// Execute a SQL command against a database.  Target attributes to set:
    /// ConnectionString (required), Command (required, the SQL to execute),
    /// SelectMode (NonQuery, Scalar, or ScalarXml, default is NonQuery),
    /// OutputFile (required when SelectMode is Scalar or ScalarXml).
    /// 
    /// Note: ScalarXml was created because of the 2033 byte limit on the sql return. 
    /// See http://aspnetresources.com/blog/executescalar_truncates_xml.aspx for details.
    /// </remarks>
    /// <example>
    /// Example of returning a count of items in a table.  Uses the default SelectMode of NonQuery.
    /// <code><![CDATA[
    ///     <SqlExecute ConnectionString="server=MyServer;Database=MyDatabase;Trusted_Connection=yes;"
    ///         Command="create database MyDatabase" />
    /// ]]></code>
    /// 
    /// Example of returning the items of a table in an xml format.
    /// <code><![CDATA[
    ///     <SqlExecute ConnectionString="server=MyServer;Database=MyDatabase;Trusted_Connection=yes;"
    ///			Command="select * from SomeTable for xml auto"
    ///			SelectMode="ScalarXml"
    ///			OutputFile="SomeTable.xml" />
    /// ]]></code>
    /// </example>
    public class SqlExecute : Task
    {

        private const string NONQUERY = "NonQuery";
        private const string SCALAR = "Scalar";
        private const string SCALARXML = "ScalarXml";

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            SqlConnection con = null;
            SqlCommand cmd = null;
            _result = -1;

            try
            {
                con = new SqlConnection(ConnectionString);
                cmd = new SqlCommand(Command, con);
                cmd.CommandTimeout = CommandTimeout;
                con.Open();

                switch (SelectMode)
                {
                    case SCALAR:
                        if (!IsOutputFileSpecified(SCALAR))
                        {
                            return false;
                        }
                        object scalar = cmd.ExecuteScalar();
                        Log.LogMessage("Successfully executed SQL command.");
                        File.WriteAllText(this.OutputFile, scalar.ToString());
                        break;
                    case SCALARXML:
                        if (!IsOutputFileSpecified(SCALARXML))
                        {
                            return false;
                        }

                        System.Xml.XmlReader rdr = cmd.ExecuteXmlReader();
                        using (TextWriter tw = new StreamWriter(OutputFile))
                        {
                            while (rdr.Read())
                            {
                                tw.Write(rdr.ReadOuterXml());
                            }
                            tw.Close();
                        }
                        break;
                    case NONQUERY:
                        _result = cmd.ExecuteNonQuery();
                        Log.LogMessage("Successfully executed SQL command with result = : " + _result.ToString());
                        break;
                    default:
                        Log.LogError("Unrecognized SelectMode: " + SelectMode);
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError("Error executing SQL command: {0}\n{1}", Command, ex.Message);
                return false;
            }
            finally
            {
                if (con != null)
                    con.Close();
            }
        }

        #region private decls
        private string _conStr;
        private string _cmd;
        private string _mode;
        private int _result;
        private string _output;
        private int _commandTimeout;
        #endregion

        /// <summary>
        /// The connection string
        /// </summary>
        [Required]
        public string ConnectionString
        {
            get { return _conStr; }
            set { _conStr = value; }
        }

        /// <summary>
        /// The command to execute
        /// </summary>
        [Required]
        public string Command
        {
            get { return _cmd; }
            set { _cmd = value; }
        }

        /// <summary>
        /// Command Timeout
        /// </summary>
        /// <remarks>Defaults to 30 seconds. Set to 0 for an infinite timeout period.</remarks>
        [DefaultValue(30)]
        public int CommandTimeout
        {
            get { return _commandTimeout; }
            set { _commandTimeout = value; }
        }

        /// <summary>
        /// The SQL Selection Mode.  Set to NonQuery, Scalar, or ScalarXml.  Default is NonQuery.
        /// </summary>
        public string SelectMode
        {
            get
            {
                if (_mode == null)
                {
                    return NONQUERY;
                }
                else
                {
                    return _mode;
                }
            }
            set { _mode = value; }
        }

        /// <summary>
        /// The file name to write to
        /// </summary>
        public string OutputFile
        {
            get { return _output; }
            set { _output = value; }
        }

        /// <summary>
        /// Determines if an output file was specified.
        /// </summary>
        private bool IsOutputFileSpecified(string selectionMode)
        {
            if (this.OutputFile == null || this.OutputFile == string.Empty)
            {
                Log.LogError("When using SelectMode=\"{0}\" you must specify an OutputFile.", selectionMode);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Output the return count/value
        /// </summary>
        [Output]
        public int Result
        {
            get { return _result; }
        }
    }
}
