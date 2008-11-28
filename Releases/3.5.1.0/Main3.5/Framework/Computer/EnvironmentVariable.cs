//-----------------------------------------------------------------------
// <copyright file="EnvironmentVariable.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Computer
{
    using System;
    using System.Globalization;
    using System.Management;
    using Microsoft.Build.Framework;

    /// <summary>
    /// <b>Valid TaskActions are:</b>
    /// <para><i>Get</i> (<b>Required: </b> Variable  <b>Output: </b> Value)</para>
    /// <para><i>Set</i> (<b>Required: </b> Variable, Value)</para>
    /// <para><b>Remote Execution Support:</b> For Get TaskAction only</para>
    /// </summary>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <Project ToolsVersion="3.5" DefaultTargets="Default" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    ///     <PropertyGroup>
    ///         <TPath>$(MSBuildProjectDirectory)\..\MSBuild.ExtensionPack.tasks</TPath>
    ///         <TPath Condition="Exists('$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks')">$(MSBuildProjectDirectory)\..\..\Common\MSBuild.ExtensionPack.tasks</TPath>
    ///     </PropertyGroup>
    ///     <Import Project="$(TPath)"/>
    ///     <Target Name="Default">
    ///         <!-- Set an environment variable. Note how special characters need to be escaped (http://msdn.microsoft.com/en-us/library/ms228186(VS.80).aspx) -->
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Set" Variable="PATH" Value="$(VCInstallDir)Common7\IDE%3B$(VCInstallDir)VC\BIN%3B$(VCInstallDir)Common7\Tools%3B$(VCInstallDir)Common7\Tools\bin%3B$(VCInstallDir)VC\PlatformSDK\bin%3B$(SDKInstallDir)bin%3B$(PATH)"/>
    ///         <!-- Set a new Environment Variable. The default target is Process -->
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Set" Variable="ANewEnvSample" Value="bddd"/>
    ///         <!-- Get the Environment Variable -->
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Get" Variable="ANewEnvSample">
    ///             <Output PropertyName="EnvValue" TaskParameter="Value"/>
    ///         </MSBuild.ExtensionPack.Computer.EnvironmentVariable>
    ///         <Message Text="Get: $(EnvValue)"/>
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Set" Variable="ANewEnvSample" Value="newddd"/>
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Get" Variable="ANewEnvSample">
    ///             <Output PropertyName="EnvValue" TaskParameter="Value"/>
    ///         </MSBuild.ExtensionPack.Computer.EnvironmentVariable>
    ///         <Message Text="Get: $(EnvValue)"/>
    ///         <!-- Set a new Environment Variable on a remote machine -->
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Set" Variable="ANewEnvSample" Value="bddd" MachineName="MediaHub"/>
    ///         <!-- Get an Environment Variable from a remote machine -->
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Get" Variable="INOCULAN" Target="Machine" MachineName="machinename" UserName="Administrator" UserPassword="passw">
    ///             <Output PropertyName="EnvValue" TaskParameter="Value"/>
    ///         </MSBuild.ExtensionPack.Computer.EnvironmentVariable>
    ///         <Message Text="INOCULAN Get: $(EnvValue)"/>
    ///         <MSBuild.ExtensionPack.Computer.EnvironmentVariable TaskAction="Get" Variable="FT" Target="User" MachineName="machinename" UserName="Administrator" UserPassword="passw">
    ///             <Output PropertyName="EnvValue" TaskParameter="Value"/>
    ///         </MSBuild.ExtensionPack.Computer.EnvironmentVariable>
    ///         <Message Text="FT Get: $(EnvValue)"/>
    ///     </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>
    public class EnvironmentVariable : BaseTask
    {
        private const string cGetTaskAction = "Get";
        private const string cSetTaskAction = "Set";
        
        private EnvironmentVariableTarget target = EnvironmentVariableTarget.Process;

        [DropdownValue(cGetTaskAction)]
        [DropdownValue(cSetTaskAction)]
        public override string TaskAction
        {
            get
            {
                return base.TaskAction;
            }
            set
            {
                base.TaskAction = value;
            }
        }

        /// <summary>
        /// Gets the value. May be a string array.
        /// </summary>
        [Output]
        [TaskAction(cGetTaskAction, false)]
        [TaskAction(cSetTaskAction, true)]
        public string[] Value { get; set; }

        /// <summary>
        /// The name of the Environment Variable
        /// </summary>
        [Required]
        [TaskAction(cGetTaskAction, true)]
        [TaskAction(cSetTaskAction, true)]
        public string Variable { set; get; }

        /// <summary>
        /// Machine, Process or User. Defaults to Process
        /// </summary>
        public string Target
        {
            get
            {
                return this.target.ToString();
            }

            set
            {
                if (Enum.IsDefined(typeof(EnvironmentVariableTarget), value))
                {
                    this.target = (EnvironmentVariableTarget) Enum.Parse(typeof(EnvironmentVariableTarget), value);
                }
                else
                {
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "The value '{0}' is not in the EnvironmentVariableTarget Enum. Use Process, User or Machine.", value));
                    return;
                }
            }
        }

        /// <summary>
        /// Performs the action of this task.
        /// </summary>
        protected override void InternalExecute()
        {
            switch (this.TaskAction)
            {
                case "Get":
                    this.Get();
                    break;
                case "Set":
                    this.Set();
                    break;
                default:
                    this.Log.LogError(string.Format(CultureInfo.CurrentCulture, "Invalid TaskAction passed: {0}", this.TaskAction));
                    return;
            }
        }

        /// <summary>
        /// Sets this instance.
        /// </summary>
        private void Set()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Setting Environment Variable: \"{0}\" for target \"{1}\" to \"{2}\".", this.Variable, this.target, this.Value[0]));
            Environment.SetEnvironmentVariable(this.Variable, this.Value[0], this.target);
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        private void Get()
        {
            this.LogTaskMessage(string.Format(CultureInfo.CurrentCulture, "Getting Environment Variable: {0} for target: {1} from: {2}", this.Variable, this.target, this.MachineName));

            if (this.MachineName == Environment.MachineName)
            {
                string temp = Environment.GetEnvironmentVariable(this.Variable, this.target);
                if (!string.IsNullOrEmpty(temp))
                {
                    this.Value = Environment.GetEnvironmentVariable(this.Variable, this.target).Split(';');
                }
                else
                {
                    this.LogTaskWarning(string.Format(CultureInfo.CurrentCulture, "The Environemnt Variable was not found: {0}", this.Variable));
                }
            }
            else
            {
                this.GetManagementScope(@"\root\cimv2");
                ObjectQuery query = new ObjectQuery(string.Format(CultureInfo.CurrentCulture, "SELECT * FROM Win32_Environment WHERE Name = '{0}'", this.Variable));
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(this.Scope, query);
                ManagementObjectCollection moc = searcher.Get();
                foreach (ManagementObject mo in moc)
                {
                    if (mo["VariableValue"] != null)
                    {
                        this.Value = mo["VariableValue"].ToString().Split(';');
                    }
                }
            }
        }
    }
}