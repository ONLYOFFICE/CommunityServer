using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MSBuild.Community.Tasks.Sandcastle
{
    /// <summary>
    /// A class representing the sandcastle enviroment.
    /// </summary>
    public class SandcastleEnviroment
    {
        internal static readonly string DefaultLocation;
        
        static SandcastleEnviroment()
        {
            DefaultLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Sandcastle");            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SandcastleEnviroment"/> class.
        /// </summary>
        public SandcastleEnviroment()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SandcastleEnviroment"/> class.
        /// </summary>
        /// <param name="sandcastleRoot">The sandcastle root.</param>
        public SandcastleEnviroment(string sandcastleRoot)
        {
            _sandcastleRoot = sandcastleRoot;
        }

        private string _sandcastleRoot;

        /// <summary>
        /// Gets or sets the sandcastle root.
        /// </summary>
        /// <value>The sandcastle root.</value>
        public string SandcastleRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_sandcastleRoot))
                {
                    _sandcastleRoot = Environment.GetEnvironmentVariable(
                        "DXROOT", EnvironmentVariableTarget.Machine);

                    if (string.IsNullOrEmpty(_sandcastleRoot))
                        return DefaultLocation;
                }

                return _sandcastleRoot;
            }
            set
            {
                _sandcastleRoot = value;
            }
        }

        private string _toolsDirectory;

        /// <summary>
        /// Gets or sets the tools directory.
        /// </summary>
        /// <value>The tools directory.</value>
        public string ToolsDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_toolsDirectory))
                    return Path.Combine(SandcastleRoot, "ProductionTools");

                return _toolsDirectory;
            }
            set
            {
                _toolsDirectory = value;
            }
        }

        private string _transformsDirectory;

        /// <summary>
        /// Gets or sets the transforms directory.
        /// </summary>
        /// <value>The transforms directory.</value>
        public string TransformsDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_transformsDirectory))
                    return Path.Combine(SandcastleRoot, "ProductionTransforms");

                return _transformsDirectory;
            }
            set
            {
                _transformsDirectory = value;
            }
        }

        private string _presentationDirectory;

        /// <summary>
        /// Gets or sets the presentation directory.
        /// </summary>
        /// <value>The presentation directory.</value>
        public string PresentationDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_presentationDirectory))
                    return Path.Combine(SandcastleRoot, "Presentation");

                return _presentationDirectory;
            }
            set
            {
                _presentationDirectory = value;
            }
        }

    }
}
