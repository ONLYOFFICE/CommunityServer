/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ASC.Common.Threading.Progress
{
    [DataContract(Namespace = "")]
    public abstract class ProgressBase : IProgressItem
    {
        private double _percentage;

        protected int StepCount { get; set; }

        [DataMember]
        public object Id { get; set; }
        [DataMember]
        public object Status { get; set; }
        [DataMember]
        public object Error { get; set; }

        [DataMember]
        public double Percentage
        {
            get { return Math.Min(100.0, Math.Max(0, _percentage)); }
            set { _percentage = value; }
        }

        [DataMember]
        public virtual bool IsCompleted { get; set; }


        public void RunJob()
        {
            try
            {
                Percentage = 0;
                DoJob();
            }
            catch (Exception e)
            {
                Error = e;
            }
            finally
            {
                Percentage = 100;
                IsCompleted = true;
            }
        }

        public async Task RunJobAsync()
        {
            try
            {
                Percentage = 0;
                await DoJobAsync();
            }
            catch (Exception e)
            {
                Error = e;
            }
            finally
            {
                Percentage = 100;
                IsCompleted = true;
            }
        }

        protected ProgressBase()
        {
            Id = Guid.NewGuid(); // random id
        }

        protected void ProgressAdd(double value)
        {
            Percentage += value;
        }

        protected void StepDone()
        {
            if (StepCount > 0)
            {
                Percentage += 100.0 / StepCount;
            }
        }


        protected abstract void DoJob();

        protected virtual Task DoJobAsync()
        {
            return Task.Run(()=>{});
        }


        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }
}