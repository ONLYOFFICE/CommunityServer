

using System;
using System.Text;

namespace MSBuild.Community.Tasks.Math
{
    /// <summary>
    /// Performs the modulo operation on numbers.
    /// </summary>
    /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="Math.Modulo"]/*'/>
    public class Modulo : MathBase
    {
        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            decimal[] numbers = StringArrayToDecimalArray(this.Numbers);
            decimal? total = null;

            StringBuilder logger = new StringBuilder();
            logger.Append("Modulo numbers: ");

            foreach (decimal number in numbers)
            {
                if (total.HasValue)
                {
                    logger.AppendFormat(" modulo {0}", number);
                    total %= number;
                }
                else
                {
                    logger.Append(number);
                    total = number;
                }
            }

            decimal actualTotal = total ?? 0;

            logger.AppendFormat(" = {0}", actualTotal);
            base.Log.LogMessage(logger.ToString());

            this.Result = actualTotal.ToString(this.NumericFormat ?? string.Empty);
            return true;
        }
    }
}
