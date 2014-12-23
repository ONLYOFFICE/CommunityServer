using System;
using System.Collections.Generic;
using Jurassic.Library;
using System.Security.Permissions;
using System.Security;

namespace Jurassic
{
    /// <summary>
    /// Represents the JavaScript script engine.  This is the first object that needs to be
    /// instantiated in order to execute javascript code.
    /// </summary>
    [Serializable]
    public sealed class ScriptEngine
#if !SILVERLIGHT
        : System.Runtime.Serialization.ISerializable
#endif
    {
        // Compatibility mode.
        private CompatibilityMode compatibilityMode;

        // The initial hidden class schema.
        private HiddenClassSchema emptySchema;

        // The built-in objects.
        private GlobalObject globalObject;
        private ArrayConstructor arrayConstructor;
        private BooleanConstructor booleanConstructor;
        private DateConstructor dateConstructor;
        private FunctionConstructor functionConstructor;
        private JSONObject jsonObject;
        private MathObject mathObject;
        private NumberConstructor numberConstructor;
        private ObjectConstructor objectConstructor;
        private RegExpConstructor regExpConstructor;
        private StringConstructor stringConstructor;

        // The built-in error objects.
        private ErrorConstructor errorConstructor;
        private ErrorConstructor rangeErrorConstructor;
        private ErrorConstructor typeErrorConstructor;
        private ErrorConstructor syntaxErrorConstructor;
        private ErrorConstructor uriErrorConstructor;
        private ErrorConstructor evalErrorConstructor;
        private ErrorConstructor referenceErrorConstructor;


        public ScriptEngine()
        {
            // Create the initial hidden class schema.  This must be done first.
            this.emptySchema = HiddenClassSchema.CreateEmptySchema();

            // Create the base of the prototype chain.
            var baseObject = ObjectInstance.CreateRootObject(this);

            // Create the global object.
            this.globalObject = new GlobalObject(baseObject);

            // Create the function object that second to last in the prototype chain.
            var baseFunction = UserDefinedFunction.CreateEmptyFunction(baseObject);

            // Object must be created first, then function.
            this.objectConstructor = new ObjectConstructor(baseFunction, baseObject);
            this.functionConstructor = new FunctionConstructor(baseFunction, baseFunction);

            // Create all the built-in objects.
            this.mathObject = new MathObject(baseObject);
            this.jsonObject = new JSONObject(baseObject);

            // Create all the built-in functions.
            this.arrayConstructor = new ArrayConstructor(baseFunction);
            this.booleanConstructor = new BooleanConstructor(baseFunction);
            this.dateConstructor = new DateConstructor(baseFunction);
            this.numberConstructor = new NumberConstructor(baseFunction);
            this.regExpConstructor = new RegExpConstructor(baseFunction);
            this.stringConstructor = new StringConstructor(baseFunction);

            // Create the error functions.
            this.errorConstructor = new ErrorConstructor(baseFunction, "Error");
            this.rangeErrorConstructor = new ErrorConstructor(baseFunction, "RangeError");
            this.typeErrorConstructor = new ErrorConstructor(baseFunction, "TypeError");
            this.syntaxErrorConstructor = new ErrorConstructor(baseFunction, "SyntaxError");
            this.uriErrorConstructor = new ErrorConstructor(baseFunction, "URIError");
            this.evalErrorConstructor = new ErrorConstructor(baseFunction, "EvalError");
            this.referenceErrorConstructor = new ErrorConstructor(baseFunction, "ReferenceError");

            // Populate the instance prototypes (TODO: optimize this, currently takes about 15ms).
            this.globalObject.PopulateFunctions();
            this.objectConstructor.PopulateFunctions();
            this.objectConstructor.InstancePrototype.PopulateFunctions();
            this.functionConstructor.InstancePrototype.PopulateFunctions(typeof(FunctionInstance));
            this.mathObject.PopulateFunctions();
            this.mathObject.PopulateFields();
            this.jsonObject.PopulateFunctions();
            this.arrayConstructor.PopulateFunctions();
            this.arrayConstructor.InstancePrototype.PopulateFunctions();
            this.booleanConstructor.InstancePrototype.PopulateFunctions();
            this.dateConstructor.PopulateFunctions();
            this.dateConstructor.InstancePrototype.PopulateFunctions();
            this.numberConstructor.InstancePrototype.PopulateFunctions();
            this.numberConstructor.PopulateFields();
            this.regExpConstructor.InstancePrototype.PopulateFunctions();
            this.stringConstructor.PopulateFunctions();
            this.stringConstructor.InstancePrototype.PopulateFunctions();
            this.errorConstructor.InstancePrototype.PopulateFunctions();

            // Add them as JavaScript-accessible properties of the global instance.
            this.globalObject.FastSetProperty("Array", this.arrayConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("Boolean", this.booleanConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("Date", this.dateConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("Function", this.functionConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("JSON", this.jsonObject, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("Math", this.mathObject, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("Number", this.numberConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("Object", this.objectConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("RegExp", this.regExpConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("String", this.stringConstructor, PropertyAttributes.NonEnumerable);

            // And the errors.
            this.globalObject.FastSetProperty("Error", this.errorConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("RangeError", this.rangeErrorConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("TypeError", this.typeErrorConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("SyntaxError", this.syntaxErrorConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("URIError", this.uriErrorConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("EvalError", this.evalErrorConstructor, PropertyAttributes.NonEnumerable);
            this.globalObject.FastSetProperty("ReferenceError", this.referenceErrorConstructor, PropertyAttributes.NonEnumerable);
        }



        //     SERIALIZATION
        //_________________________________________________________________________________________

#if !SILVERLIGHT

        /// <summary>
        /// Initializes a new instance of the ObjectInstance class with serialized data.
        /// </summary>
        /// <param name="info"> The SerializationInfo that holds the serialized object data about
        /// the exception being thrown. </param>
        /// <param name="context"> The StreamingContext that contains contextual information about
        /// the source or destination. </param>
        private ScriptEngine(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            // Set the DeserializationEnvironment to this script engine.
            ScriptEngine.DeserializationEnvironment = this;

            // Create the initial hidden class schema.  This must be done first.
            this.emptySchema = HiddenClassSchema.CreateEmptySchema();

            // Deserialize the compatibility mode.
            this.compatibilityMode = (CompatibilityMode)info.GetInt32("compatibilityMode");

            // Deserialize the ForceStrictMode flag.
            this.ForceStrictMode = info.GetBoolean("forceStrictMode");

            // Deserialize the built-in objects.
            this.globalObject = (GlobalObject)info.GetValue("globalObject", typeof(GlobalObject));
            this.arrayConstructor = (ArrayConstructor)info.GetValue("arrayConstructor", typeof(ArrayConstructor));
            this.booleanConstructor = (BooleanConstructor)info.GetValue("booleanConstructor", typeof(BooleanConstructor));
            this.dateConstructor = (DateConstructor)info.GetValue("dateConstructor", typeof(DateConstructor));
            this.functionConstructor = (FunctionConstructor)info.GetValue("functionConstructor", typeof(FunctionConstructor));
            this.jsonObject = (JSONObject)info.GetValue("jsonObject", typeof(JSONObject));
            this.mathObject = (MathObject)info.GetValue("mathObject", typeof(MathObject));
            this.numberConstructor = (NumberConstructor)info.GetValue("numberConstructor", typeof(NumberConstructor));
            this.objectConstructor = (ObjectConstructor)info.GetValue("objectConstructor", typeof(ObjectConstructor));
            this.regExpConstructor = (RegExpConstructor)info.GetValue("regExpConstructor", typeof(RegExpConstructor));
            this.stringConstructor = (StringConstructor)info.GetValue("stringConstructor", typeof(StringConstructor));

            // Deserialize the built-in error objects.
            this.errorConstructor = (ErrorConstructor)info.GetValue("errorConstructor", typeof(ErrorConstructor));
            this.rangeErrorConstructor = (ErrorConstructor)info.GetValue("rangeErrorConstructor", typeof(ErrorConstructor));
            this.typeErrorConstructor = (ErrorConstructor)info.GetValue("typeErrorConstructor", typeof(ErrorConstructor));
            this.syntaxErrorConstructor = (ErrorConstructor)info.GetValue("syntaxErrorConstructor", typeof(ErrorConstructor));
            this.uriErrorConstructor = (ErrorConstructor)info.GetValue("uriErrorConstructor", typeof(ErrorConstructor));
            this.evalErrorConstructor = (ErrorConstructor)info.GetValue("evalErrorConstructor", typeof(ErrorConstructor));
            this.referenceErrorConstructor = (ErrorConstructor)info.GetValue("referenceErrorConstructor", typeof(ErrorConstructor));
        }

        /// <summary>
        /// Sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info"> The SerializationInfo that holds the serialized object data about
        /// the exception being thrown. </param>
        /// <param name="context"> The StreamingContext that contains contextual information about
        /// the source or destination. </param>
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            // Serialize the compatibility mode.
            info.AddValue("compatibilityMode", (int)this.compatibilityMode);

            // Serialize the ForceStrictMode flag.
            info.AddValue("forceStrictMode", this.ForceStrictMode);

            // Serialize the built-in objects.
            info.AddValue("globalObject", this.globalObject);
            info.AddValue("arrayConstructor", this.arrayConstructor);
            info.AddValue("booleanConstructor", this.booleanConstructor);
            info.AddValue("dateConstructor", this.dateConstructor);
            info.AddValue("functionConstructor", this.functionConstructor);
            info.AddValue("jsonObject", this.jsonObject);
            info.AddValue("mathObject", this.mathObject);
            info.AddValue("numberConstructor", this.numberConstructor);
            info.AddValue("objectConstructor", this.objectConstructor);
            info.AddValue("regExpConstructor", this.regExpConstructor);
            info.AddValue("stringConstructor", this.stringConstructor);

            // Serialize the built-in error objects.
            info.AddValue("errorConstructor", this.errorConstructor);
            info.AddValue("rangeErrorConstructor", this.rangeErrorConstructor);
            info.AddValue("typeErrorConstructor", this.typeErrorConstructor);
            info.AddValue("syntaxErrorConstructor", this.syntaxErrorConstructor);
            info.AddValue("uriErrorConstructor", this.uriErrorConstructor);
            info.AddValue("evalErrorConstructor", this.evalErrorConstructor);
            info.AddValue("referenceErrorConstructor", this.referenceErrorConstructor);
        }

#endif



        //     PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets or sets a value that indicates whether to force ECMAScript 5 strict mode, even if
        /// the code does not contain a strict mode directive ("use strict").  The default is
        /// <c>false</c>.
        /// </summary>
        public bool ForceStrictMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the script engine should run in
        /// compatibility mode.
        /// </summary>
        public CompatibilityMode CompatibilityMode
        {
            get { return this.compatibilityMode; }
            set
            {
                this.compatibilityMode = value;

                // Infinity, NaN and undefined are writable in ECMAScript 3 and read-only in ECMAScript 5.
                var attributes = PropertyAttributes.Sealed;
                if (this.CompatibilityMode == CompatibilityMode.ECMAScript3)
                    attributes = PropertyAttributes.Writable;
                this.Global.FastSetProperty("Infinity", double.PositiveInfinity, attributes, overwriteAttributes: true);
                this.Global.FastSetProperty("NaN", double.NaN, attributes, overwriteAttributes: true);
                this.Global.FastSetProperty("undefined", Undefined.Value, attributes, overwriteAttributes: true);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to disassemble any generated IL and store it
        /// in the associated function.
        /// </summary>
        public bool EnableILAnalysis
        {
            get;
            set;
        }

#if !SILVERLIGHT

        private static object lowPrivilegeEnvironmentLock = new object();
        private static bool lowPrivilegeEnvironmentTested = false;
        private static bool lowPrivilegeEnvironment = false;

        /// <summary>
        /// Gets a value that indicates whether the script engine must run in a low privilege environment.
        /// </summary>
        internal static bool LowPrivilegeEnvironment
        {
            get
            {
                lock (lowPrivilegeEnvironmentLock)
                {
                    if (lowPrivilegeEnvironmentTested == false)
                    {
                        var permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                        var permissionSet = new PermissionSet(PermissionState.None);
                        permissionSet.AddPermission(permission);
                        lowPrivilegeEnvironment = AppDomain.CurrentDomain.PermissionSet.IsSubsetOf(permissionSet);
                        lowPrivilegeEnvironmentTested = true;
                    }
                }
                return lowPrivilegeEnvironment;
            }
        }

#else

        /// <summary>
        /// Gets a value that indicates whether the script engine must run in a low privilege environment.
        /// </summary>
        internal static bool LowPrivilegeEnvironment
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates that the current AppDomain is a low privilege environment.
        /// </summary>
        internal static void SetLowPrivilegeEnvironment()
        {
        }

#endif

        [ThreadStatic]
        private static ScriptEngine deserializationEnvironment;

        /// <summary>
        /// Gets or sets the script engine to use when deserializing objects.  This property is a
        /// per-thread setting; it must be set on the thread that is doing the deserialization.
        /// </summary>
        public static ScriptEngine DeserializationEnvironment
        {
            get { return deserializationEnvironment; }
            set { deserializationEnvironment = value; }
        }



        //     GLOBAL BUILT-IN OBJECTS
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the built-in global object.  This object is implicitly accessed when creating
        /// global variables and functions.
        /// </summary>
        public GlobalObject Global
        {
            get { return this.globalObject; }
        }

        /// <summary>
        /// Gets the built-in Array object.
        /// </summary>
        public ArrayConstructor Array
        {
            get { return this.arrayConstructor; }
        }

        /// <summary>
        /// Gets the built-in Boolean object.
        /// </summary>
        public BooleanConstructor Boolean
        {
            get { return this.booleanConstructor; }
        }

        /// <summary>
        /// Gets the built-in Date object.
        /// </summary>
        public DateConstructor Date
        {
            get { return this.dateConstructor; }
        }

        /// <summary>
        /// Gets the built-in Function object.
        /// </summary>
        public FunctionConstructor Function
        {
            get { return this.functionConstructor; }
        }

        /// <summary>
        /// Gets the built-in Math object.
        /// </summary>
        public MathObject Math
        {
            get { return this.mathObject; }
        }

        /// <summary>
        /// Gets the built-in Number object.
        /// </summary>
        public NumberConstructor Number
        {
            get { return this.numberConstructor; }
        }

        /// <summary>
        /// Gets the built-in Object object.
        /// </summary>
        public ObjectConstructor Object
        {
            get { return this.objectConstructor; }
        }

        /// <summary>
        /// Gets the built-in RegExp object.
        /// </summary>
        public RegExpConstructor RegExp
        {
            get { return this.regExpConstructor; }
        }

        /// <summary>
        /// Gets the built-in String object.
        /// </summary>
        public StringConstructor String
        {
            get { return this.stringConstructor; }
        }


        /// <summary>
        /// Gets the built-in Error object.
        /// </summary>
        public ErrorConstructor Error
        {
            get { return this.errorConstructor; }
        }

        /// <summary>
        /// Gets the built-in RangeError object.
        /// </summary>
        public ErrorConstructor RangeError
        {
            get { return this.rangeErrorConstructor; }
        }

        /// <summary>
        /// Gets the built-in TypeError object.
        /// </summary>
        public ErrorConstructor TypeError
        {
            get { return this.typeErrorConstructor; }
        }

        /// <summary>
        /// Gets the built-in SyntaxError object.
        /// </summary>
        public ErrorConstructor SyntaxError
        {
            get { return this.syntaxErrorConstructor; }
        }

        /// <summary>
        /// Gets the built-in URIError object.
        /// </summary>
        public ErrorConstructor URIError
        {
            get { return this.uriErrorConstructor; }
        }

        /// <summary>
        /// Gets the built-in EvalError object.
        /// </summary>
        public ErrorConstructor EvalError
        {
            get { return this.evalErrorConstructor; }
        }

        /// <summary>
        /// Gets the built-in ReferenceError object.
        /// </summary>
        public ErrorConstructor ReferenceError
        {
            get { return this.referenceErrorConstructor; }
        }



        //     DEBUGGING SUPPORT
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets or sets a value which indicates whether debug information should be generated.  If
        /// this is set to <c>true</c> performance and memory usage are negatively impacted.
        /// </summary>
        public bool EnableDebugging
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether CLR types can be exposed directly to the script engine.  If this is set to 
        /// <c>false</c>, attempting to instantiate CLR types from script may result in exceptions being
        /// thrown in script.
        /// </summary>
        /// <remarks>
        /// <para>This property is intended to prevent script developers from accessing the entire CLR
        /// type system, for security purposes.  When this property is set to <c>false</c>, it should prevent
        /// new instances of CLR types from being exposed to the script engine, even if you have already 
        /// exposed CLR types to the script engine.</para>
        /// </remarks>
        public bool EnableExposedClrTypes
        {
            get;
            set;
        }

#if !WINDOWS_PHONE
        internal class ReflectionEmitModuleInfo
        {
            public System.Reflection.Emit.AssemblyBuilder AssemblyBuilder;
            public System.Reflection.Emit.ModuleBuilder ModuleBuilder;
            public int TypeCount;
        }

        /// <summary>
        /// Gets or sets information needed by Reflection.Emit.
        /// </summary>
        [NonSerialized]
        internal ReflectionEmitModuleInfo ReflectionEmitInfo;
#endif //!WINDOWS_PHONE



        //     EXECUTION
        //_________________________________________________________________________________________

        /// <summary>
        /// Executes the given source code.  Execution is bound to the global scope.
        /// </summary>
        /// <param name="code"> The javascript source code to execute. </param>
        /// <returns> The result of executing the source code. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="code"/> is a <c>null</c> reference. </exception>
        public object Evaluate(string code)
        {
            return Evaluate(new StringScriptSource(code));
        }

        /// <summary>
        /// Executes the given source code.  Execution is bound to the global scope.
        /// </summary>
        /// <typeparam name="T"> The type to convert the result to. </typeparam>
        /// <param name="code"> The javascript source code to execute. </param>
        /// <returns> The result of executing the source code. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="code"/> is a <c>null</c> reference. </exception>
        public T Evaluate<T>(string code)
        {
            return TypeConverter.ConvertTo<T>(this, Evaluate(code));
        }

        /// <summary>
        /// Executes the given source code.  Execution is bound to the global scope.
        /// </summary>
        /// <param name="source"> The javascript source code to execute. </param>
        /// <returns> The result of executing the source code. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="code"/> is a <c>null</c> reference. </exception>
        public object Evaluate(ScriptSource source)
        {
            var methodGen = new Jurassic.Compiler.EvalMethodGenerator(
                this,                               // The script engine
                this.CreateGlobalScope(),           // The variable scope.
                source,                             // The source code.
                CreateOptions(),                    // The compiler options.
                this.Global);                       // The value of the "this" keyword.
            
            // Parse
            if (this.ParsingStarted != null)
                this.ParsingStarted(this, EventArgs.Empty);
            methodGen.Parse();

            // Optimize
            if (this.OptimizationStarted != null)
                this.OptimizationStarted(this, EventArgs.Empty);
            methodGen.Optimize();

            // Generate code
            if (this.CodeGenerationStarted != null)
                this.CodeGenerationStarted(this, EventArgs.Empty);
            methodGen.GenerateCode();
            VerifyGeneratedCode();

            // Execute
            if (this.ExecutionStarted != null)
                this.ExecutionStarted(this, EventArgs.Empty);
            var result = methodGen.Execute();

            // Normalize the result (convert null to Undefined, double to int, etc).
            return TypeUtilities.NormalizeValue(result);
        }

        /// <summary>
        /// Executes the given source code.  Execution is bound to the global scope.
        /// </summary>
        /// <typeparam name="T"> The type to convert the result to. </typeparam>
        /// <param name="source"> The javascript source code to execute. </param>
        /// <returns> The result of executing the source code. </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="code"/> is a <c>null</c> reference. </exception>
        public T Evaluate<T>(ScriptSource source)
        {
            return TypeConverter.ConvertTo<T>(this, Evaluate(source));
        }

        /// <summary>
        /// Executes the given source code.  Execution is bound to the global scope.
        /// </summary>
        /// <param name="code"> The javascript source code to execute. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="path"/> is a <c>null</c> reference. </exception>
        public void Execute(string code)
        {
            Execute(new StringScriptSource(code));
        }

        /// <summary>
        /// Executes the given file.  If the file does not have a BOM then it is assumed to be UTF8.
        /// Execution is bound to the global scope.
        /// </summary>
        /// <param name="path"> The path to a javascript file.  This can be a local file path or a
        /// UNC path. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="path"/> is a <c>null</c> reference. </exception>
        public void ExecuteFile(string path)
        {
            ExecuteFile(path, null);
        }

        /// <summary>
        /// Executes the given file.  Execution is bound to the global scope.
        /// </summary>
        /// <param name="path"> The path to a javascript file.  This can be a local file path or a
        /// UNC path. </param>
        /// <param name="encoding"> The character encoding to use if the file lacks a byte order
        /// mark (BOM). </param>
        /// <exception cref="ArgumentNullException"> <paramref name="path"/> is a <c>null</c> reference. </exception>
        public void ExecuteFile(string path, System.Text.Encoding encoding)
        {
            Execute(new FileScriptSource(path, encoding));
        }

        /// <summary>
        /// Executes the given source code.  Execution is bound to the global scope.
        /// </summary>
        /// <param name="source"> The javascript source code to execute. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="code"/> is a <c>null</c> reference. </exception>
        public void Execute(ScriptSource source)
        {
            var methodGen = new Jurassic.Compiler.GlobalMethodGenerator(
                this,                               // The script engine
                source,                             // The source code.
                CreateOptions());                   // The compiler options.

            // Parse
            if (this.ParsingStarted != null)
                this.ParsingStarted(this, EventArgs.Empty);
            methodGen.Parse();

            // Optimize
            if (this.OptimizationStarted != null)
                this.OptimizationStarted(this, EventArgs.Empty);
            methodGen.Optimize();

            // Generate code
            if (this.CodeGenerationStarted != null)
                this.CodeGenerationStarted(this, EventArgs.Empty);
            methodGen.GenerateCode();
            VerifyGeneratedCode();

            // Execute
            if (this.ExecutionStarted != null)
                this.ExecutionStarted(this, EventArgs.Empty);
            methodGen.Execute();
        }

        /// <summary>
        /// Verifies the generated byte code.
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        private void VerifyGeneratedCode()
        {
#if false
            if (this.EnableDebugging == false)
                return;

            var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "JurassicDebug.dll");

            // set the entry point for the application and save it
            this.ReflectionEmitInfo.AssemblyBuilder.Save(System.IO.Path.GetFileName(filePath));

            // Copy this DLL there as well.
            var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.IO.File.Copy(assemblyPath, System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileName(assemblyPath)), true);

            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools\x64\PEVerify.exe";
            startInfo.Arguments = string.Format("\"{0}\" /nologo /verbose /unique", filePath);
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            var verifyProcess = System.Diagnostics.Process.Start(startInfo);
            string output = verifyProcess.StandardOutput.ReadToEnd();

            if (verifyProcess.ExitCode != 0)
            {
                throw new InvalidOperationException(output);
            }
            //else
            //{
            //    System.Diagnostics.Process.Start(@"C:\Program Files\Reflector\Reflector.exe", string.Format("\"{0}\" /select:JavaScriptClass", filePath));
            //    Environment.Exit(0);
            //}

            // The assembly can no longer be modified - so don't use it again.
            this.ReflectionEmitInfo = null;
#endif
        }

        /// <summary>
        /// Creates a CompilerOptions instance using the script engine properties.
        /// </summary>
        /// <returns> A populated CompilerOptions instance. </returns>
        private Compiler.CompilerOptions CreateOptions()
        {
            return new Compiler.CompilerOptions() { ForceStrictMode = this.ForceStrictMode, EnableDebugging = this.EnableDebugging };
        }



        //     GLOBAL HELPERS
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets a value that indicates whether the global variable with the given name is defined.
        /// </summary>
        /// <param name="variableName"> The name of the variable to check. </param>
        /// <returns> <c>true</c> if the given variable has a value; <c>false</c> otherwise. </returns>
        /// <remarks> Note that a variable that has been set to <c>undefined</c> is still
        /// considered to have a value. </remarks>
        public bool HasGlobalValue(string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");
            return this.Global.HasProperty(variableName);
        }

        /// <summary>
        /// Gets the value of the global variable with the given name.
        /// </summary>
        /// <param name="variableName"> The name of the variable to retrieve the value for. </param>
        /// <returns> The value of the global variable, or <c>null</c> otherwise. </returns>
        public object GetGlobalValue(string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");
            return TypeUtilities.NormalizeValue(this.Global.GetPropertyValue(variableName));
        }

        /// <summary>
        /// Gets the value of the global variable with the given name and coerces it to the given
        /// type.
        /// </summary>
        /// <typeparam name="T"> The type to coerce the value to. </typeparam>
        /// <param name="variableName"> The name of the variable to retrieve the value for. </param>
        /// <returns> The value of the global variable, or <c>null</c> otherwise. </returns>
        /// <remarks> Note that <c>null</c> is coerced to the following values: <c>false</c> (if
        /// <typeparamref name="T"/> is <c>bool</c>), 0 (if <typeparamref name="T"/> is <c>int</c>
        /// or <c>double</c>), string.Empty (if <typeparamref name="T"/> is <c>string</c>). </remarks>
        public T GetGlobalValue<T>(string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");
            return TypeConverter.ConvertTo<T>(this, TypeUtilities.NormalizeValue(this.Global.GetPropertyValue(variableName)));
        }

        /// <summary>
        /// Sets the value of the global variable with the given name.  If the property does not
        /// exist, it will be created.
        /// </summary>
        /// <param name="variableName"> The name of the variable to set. </param>
        /// <param name="value"> The desired value of the variable.  This must be of a supported
        /// type (bool, int, double, string, Null, Undefined or a ObjectInstance-derived type). </param>
        /// <exception cref="JavaScriptException"> The property is read-only or the property does
        /// not exist and the object is not extensible. </exception>
        public void SetGlobalValue(string variableName, object value)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");
            if (value == null)
                throw new ArgumentNullException("value");

            if (value == null)
                value = Null.Value;
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        break;
                    case TypeCode.Byte:
                        value = (int)(byte)value;
                        break;
                    case TypeCode.Char:
                        value = new string((char)value, 1);
                        break;
                    case TypeCode.Decimal:
                        value = decimal.ToDouble((decimal)value);
                        break;
                    case TypeCode.Double:
                        break;
                    case TypeCode.Int16:
                        value = (int)(short)value;
                        break;
                    case TypeCode.Int32:
                        break;
                    case TypeCode.Int64:
                        value = (double)(long)value;
                        break;
                    case TypeCode.Object:
                        if (value is Type)
                            value = ClrStaticTypeWrapper.FromCache(this, (Type)value);
                        else if ((value is ObjectInstance) == false)
                            value = new ClrInstanceWrapper(this, value);
                        break;
                    case TypeCode.SByte:
                        value = (int)(sbyte)value;
                        break;
                    case TypeCode.Single:
                        value = (double)(float)value;
                        break;
                    case TypeCode.String:
                        break;
                    case TypeCode.UInt16:
                        value = (int)(ushort)value;
                        break;
                    case TypeCode.UInt32:
                        break;
                    case TypeCode.UInt64:
                        value = (double)(ulong)value;
                        break;
                    default:
                        throw new ArgumentException(string.Format("Cannot store value of type {0}.", value.GetType()), "value");
                }
            }
            this.Global.SetPropertyValue(variableName, value, true);
        }

        /// <summary>
        /// Calls a global function and returns the result.
        /// </summary>
        /// <param name="functionName"> The name of the function to call. </param>
        /// <param name="argumentValues"> The argument values to pass to the function. </param>
        /// <returns> The return value from the function. </returns>
        public object CallGlobalFunction(string functionName, params object[] argumentValues)
        {
            if (functionName == null)
                throw new ArgumentNullException("functionName");
            if (argumentValues == null)
                throw new ArgumentNullException("argumentValues");
            var value = this.Global.GetPropertyValue(functionName);
            if ((value is FunctionInstance) == false)
                throw new InvalidOperationException(string.Format("'{0}' is not a function.", functionName));
            return ((FunctionInstance)value).CallLateBound(null, argumentValues);
        }

        /// <summary>
        /// Calls a global function and returns the result.
        /// </summary>
        /// <typeparam name="T"> The type to coerce the value to. </typeparam>
        /// <param name="functionName"> The name of the function to call. </param>
        /// <param name="argumentValues"> The argument values to pass to the function. </param>
        /// <returns> The return value from the function, coerced to the given type. </returns>
        public T CallGlobalFunction<T>(string functionName, params object[] argumentValues)
        {
            if (functionName == null)
                throw new ArgumentNullException("functionName");
            if (argumentValues == null)
                throw new ArgumentNullException("argumentValues");
            return TypeConverter.ConvertTo<T>(this, CallGlobalFunction(functionName, argumentValues));
        }

        /// <summary>
        /// Sets the global variable with the given name to a function implemented by the provided
        /// delegate.
        /// </summary>
        /// <param name="functionName"> The name of the global variable to set. </param>
        /// <param name="functionDelegate"> The delegate that will implement the function. </param>
        public void SetGlobalFunction(string functionName, Delegate functionDelegate)
        {
            if (functionName == null)
                throw new ArgumentNullException("functionName");
            if (functionDelegate == null)
                throw new ArgumentNullException("functionDelegate");
            SetGlobalValue(functionName, new ClrFunction(this.Function.InstancePrototype, functionDelegate, functionName));
        }




        //     TIMING EVENTS
        //_________________________________________________________________________________________

        /// <summary>
        /// Fires when the compiler starts parsing javascript source code.
        /// </summary>
        public event EventHandler ParsingStarted;

        /// <summary>
        /// Fires when the compiler starts optimizing.
        /// </summary>
        public event EventHandler OptimizationStarted;

        /// <summary>
        /// Fires when the compiler starts generating byte code.
        /// </summary>
        public event EventHandler CodeGenerationStarted;

        /// <summary>
        /// Fires when the compiler starts running javascript code.
        /// </summary>
        public event EventHandler ExecutionStarted;



        //     SCOPE
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new global scope.
        /// </summary>
        /// <returns> A new global scope, with no declared variables. </returns>
        internal Compiler.ObjectScope CreateGlobalScope()
        {
            return Compiler.ObjectScope.CreateGlobalScope(this.Global);
        }



        //     HIDDEN CLASS INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets an empty schema.
        /// </summary>
        internal HiddenClassSchema EmptySchema
        {
            get { return this.emptySchema; }
        }



        //     EVAL SUPPORT
        //_________________________________________________________________________________________

        //private class EvalCacheKey
        //{
        //    public string Code;
        //    public Compiler.Scope Scope;
        //    public bool StrictMode;

        //    public override int GetHashCode()
        //    {
        //        int bitValue = 1;
        //        int hashCode = this.Code.GetHashCode();
        //        var scope = this.Scope;
        //        do
        //        {
        //            if (scope is Compiler.DeclarativeScope)
        //                hashCode ^= bitValue;
        //            scope = scope.ParentScope;
        //            bitValue *= 2;
        //        } while (scope != null);
        //        if (this.StrictMode == true)
        //            hashCode ^= bitValue;
        //        return hashCode;
        //    }

        //    public override bool Equals(object obj)
        //    {
        //        if ((obj is EvalCacheKey) == false)
        //            return false;
        //        var other = (EvalCacheKey) obj;
        //        if (this.Code != other.Code ||
        //            this.StrictMode != other.StrictMode)
        //            return false;
        //        var scope1 = this.Scope;
        //        var scope2 = other.Scope;
        //        do
        //        {
        //            if (scope1.GetType() != scope2.GetType())
        //                return false;
        //            scope1 = scope1.ParentScope;
        //            scope2 = scope2.ParentScope;
        //            if (scope1 == null && scope2 != null)
        //                return false;
        //            if (scope1 != null && scope2 == null)
        //                return false;
        //        } while (scope1 != null);
        //        return true;
        //    }
        //}

        //private Dictionary<EvalCacheKey, WeakReference> evalCache =
        //    new Dictionary<EvalCacheKey, WeakReference>();

        /// <summary>
        /// Evaluates the given javascript source code and returns the result.
        /// </summary>
        /// <param name="code"> The source code to evaluate. </param>
        /// <param name="scope"> The containing scope. </param>
        /// <param name="thisObject"> The value of the "this" keyword in the containing scope. </param>
        /// <param name="strictMode"> Indicates whether the eval statement is being called from
        /// strict mode code. </param>
        /// <returns> The value of the last statement that was executed, or <c>undefined</c> if
        /// there were no executed statements. </returns>
        internal object Eval(string code, Compiler.Scope scope, object thisObject, bool strictMode)
        {
            // Check if the cache contains the eval already.
            //var key = new EvalCacheKey() { Code = code, Scope = scope, StrictMode = strictMode };
            //WeakReference cachedEvalGenRef;
            //if (evalCache.TryGetValue(key, out cachedEvalGenRef) == true)
            //{
            //    var cachedEvalGen = (Compiler.EvalMethodGenerator)cachedEvalGenRef.Target;
            //    if (cachedEvalGen != null)
            //    {
            //        // Replace the "this object" before running.
            //        cachedEvalGen.ThisObject = thisObject;

            //        // Execute the cached code.
            //        return ((Compiler.EvalMethodGenerator)cachedEvalGen).Execute();
            //    }
            //}

            // Parse the eval string into an AST.
            var options = new Compiler.CompilerOptions() { ForceStrictMode = strictMode };
            var evalGen = new Jurassic.Compiler.EvalMethodGenerator(
                this,                                                   // The script engine.
                scope,                                                  // The scope to run the code in.
                new StringScriptSource(code, "eval"),                   // The source code to execute.
                options,                                                // Options.
                thisObject);                                            // The value of the "this" keyword.

            // Make sure the eval cache doesn't get too big.  TODO: add some sort of LRU strategy?
            //if (evalCache.Count > 100)
            //    evalCache.Clear();

            //// Add the eval method generator to the cache.
            //evalCache[key] = new WeakReference(evalGen);

            // Compile and run the eval code.
            return evalGen.Execute();
        }



        //     STACK TRACE SUPPORT
        //_________________________________________________________________________________________

        private class StackFrame
        {
            public string Path;
            public string Function;
            public int Line;
        }

        private Stack<StackFrame> stackFrames = new Stack<StackFrame>();

        /// <summary>
        /// Creates a stack trace.
        /// </summary>
        /// <param name="errorName"> The name of the error (e.g. "ReferenceError"). </param>
        /// <param name="message"> The error message. </param>
        /// <param name="path"> The path of the javascript source file that is currently executing. </param>
        /// <param name="function"> The name of the currently executing function. </param>
        /// <param name="line"> The line number of the statement that is currently executing. </param>
        internal string FormatStackTrace(string errorName, string message, string path, string function, int line)
        {
            var result = new System.Text.StringBuilder(errorName);
            if (string.IsNullOrEmpty(message) == false)
            {
                result.Append(": ");
                result.Append(message);
            }
            if (path != null || function != null || line != 0)
                AppendStackFrame(result, path, function, line);
            foreach (var frame in stackFrames)
                AppendStackFrame(result, frame.Path, frame.Function, frame.Line);
            return result.ToString();
        }

        /// <summary>
        /// Appends a stack frame to the end of the given StringBuilder instance.
        /// </summary>
        /// <param name="result"> The StringBuilder to append to. </param>
        /// <param name="path"> The path of the javascript source file. </param>
        /// <param name="function"> The name of the function. </param>
        /// <param name="line"> The line number of the statement. </param>
        private void AppendStackFrame(System.Text.StringBuilder result, string path, string function, int line)
        {
            result.AppendLine();
            result.Append("    ");
            result.Append("at ");
            if (string.IsNullOrEmpty(function) == false)
            {
                result.Append(function);
                result.Append(" (");
            }
            result.Append(path ?? "unknown");
            if (line > 0)
            {
                result.Append(":");
                result.Append(line);
            }
            if (string.IsNullOrEmpty(function) == false)
                result.Append(")");
        }

        /// <summary>
        /// Pushes a frame to the javascript stack.
        /// </summary>
        /// <param name="path"> The path of the javascript source file that contains the function. </param>
        /// <param name="function"> The name of the function that is calling another function. </param>
        /// <param name="line"> The line number of the function call. </param>
        internal void PushStackFrame(string path, string function, int line)
        {
            this.stackFrames.Push(new StackFrame() { Path = path, Function = function, Line = line });
        }

        /// <summary>
        /// Pops a frame from the javascript stack.
        /// </summary>
        internal void PopStackFrame()
        {
            this.stackFrames.Pop();
        }


        //     CLRTYPEWRAPPER CACHE
        //_________________________________________________________________________________________

        private Dictionary<Type, ClrInstanceTypeWrapper> instanceTypeWrapperCache;
        private Dictionary<Type, ClrStaticTypeWrapper> staticTypeWrapperCache;

        /// <summary>
        /// Gets a dictionary that can be used to cache ClrInstanceTypeWrapper instances.
        /// </summary>
        internal Dictionary<Type, ClrInstanceTypeWrapper> InstanceTypeWrapperCache
        {
            get
            {
                if (this.instanceTypeWrapperCache == null)
                    this.instanceTypeWrapperCache = new Dictionary<Type, ClrInstanceTypeWrapper>();
                return this.instanceTypeWrapperCache;
            }
        }

        /// <summary>
        /// Gets a dictionary that can be used to cache ClrStaticTypeWrapper instances.
        /// </summary>
        internal Dictionary<Type, ClrStaticTypeWrapper> StaticTypeWrapperCache
        {
            get
            {
                if (this.staticTypeWrapperCache == null)
                    this.staticTypeWrapperCache = new Dictionary<Type, ClrStaticTypeWrapper>();
                return this.staticTypeWrapperCache;
            }
        }
    }
}
