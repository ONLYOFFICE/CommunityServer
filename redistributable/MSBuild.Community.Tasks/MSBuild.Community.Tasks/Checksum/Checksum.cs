using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MSBuild.Community.Tasks
{
  /// <summary>
  /// Calculates checksum of a file set. 
  /// 
  /// It is based on ANT's checksum task. The output is stored in a file in the same folder
  /// as the input file with the name [input_file].[algorithm_type]. Supported algorithms: MD5, SH1. 
  /// </summary>
  /// <example>
  /// Creates MD5 checksum files for all TXT files in root folder.
  /// <code><![CDATA[
  ///     <Checksum Files="Root\*.txt" />
  /// ]]></code>
  /// </example>
  public class Checksum : Task
  {
    enum SupportedAlgorithm
    {
      md5, 
      sha1
    }
    
    
    #region Fields

    private string algorithm = SupportedAlgorithm.md5.ToString();

    #endregion

    #region Input Parameters
    /// <summary>
    /// Files uses to calculate the checksum
    /// </summary>
    [Required]
    public ITaskItem[] Files { get; set; }


    /// <summary>
    /// Algorithm to be used. Defaults to MD5. 
    /// </summary>
    public string Algorithm { get; set; }
    #endregion

    #region Task Overrides

    public override bool Execute()
    {
      //No files provided
      if ( null == Files ||this.Files.Length == 0 )
      {
        Log.LogError( Properties.Resources.ParameterRequired, "Checksum", "Files" );
        return false;
      }

      SupportedAlgorithm alg;
      if( !Enum.TryParse( this.Algorithm, true, out alg ) )
      {
        Log.LogError( "Unsupported algorithm type: {0}", this.Algorithm );
        return false;
      }

      try
      {
        HashAlgorithm hashAlgorithm = HashAlgorithm.Create(this.Algorithm);

        foreach( var file in Files )
        {
          string inputFile = file.GetMetadata( "FullPath" );

          //Compute file hash
          byte[] hashValue;
          using( FileStream stream = File.OpenRead( inputFile ) )
          {
            hashValue =  hashAlgorithm.ComputeHash( stream );
          }

          //Write hash to output file
          string outputFile = string.Format("{0}.{1}", inputFile, this.Algorithm);
          File.WriteAllText( outputFile, ByteArrayToHexString(hashValue) );
        }
      }
      catch( Exception ex )
      {
        Log.LogErrorFromException( ex );
        return false;
      }

      return true;
    } 
    #endregion

    string ByteArrayToHexString(byte[] array)
    {
      StringBuilder result = new StringBuilder();
      foreach( byte b in array )
      {
        result.AppendFormat( "{0:x2}", b );
      }
      return result.ToString();
    }
  }
}
