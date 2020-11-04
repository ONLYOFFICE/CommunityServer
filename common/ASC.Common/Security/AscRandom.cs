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

namespace ASC.Common.Security
{
    public class AscRandom : Random
    {
        private int inext;
        private int inextp;
        private const int MBIG = int.MaxValue;
        private const int MSEED = 161803398;
        private const int MZ = 0;
        private int[] seeds;


        public AscRandom() : this(Environment.TickCount)
        {
        }

        public AscRandom(int seed)
        {
            seeds = new int[56];
            var num4 = (seed == int.MinValue) ? int.MaxValue : Math.Abs(seed);
            var num2 = 161803398 - num4;
            seeds[seeds.Length - 1] = num2;
            var num3 = 1;
            for (int i = 1; i < seeds.Length - 1; i++)
            {
                var index = (21 * i) % (seeds.Length - 1);
                seeds[index] = num3;
                num3 = num2 - num3;
                if (num3 < 0)
                {
                    num3 += int.MaxValue;
                }
                num2 = seeds[index];
            }
            for (int j = 1; j < 5; j++)
            {
                for (int k = 1; k < seeds.Length; k++)
                {
                    seeds[k] -= seeds[1 + ((k + 30) % (seeds.Length - 1))];
                    if (seeds[k] < 0)
                    {
                        seeds[k] += int.MaxValue;
                    }
                }
            }
            inext = 0;
            inextp = 21;
            seed = 1;
        }

        public override int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue");
            }
            return (int)(InternalSample() * 4.6566128752457969E-10 * maxValue);
        }

        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(InternalSample() % (Byte.MaxValue + 1));
            }
        }

        private int InternalSample()
        {
            var inext = this.inext;
            var inextp = this.inextp;
            if (++inext >= seeds.Length - 1)
            {
                inext = 1;
            }
            if (++inextp >= seeds.Length - 1)
            {
                inextp = 1;
            }
            int num = seeds[inext] - seeds[inextp];
            if (num == int.MaxValue)
            {
                num--;
            }
            if (num < 0)
            {
                num += int.MaxValue;
            }
            seeds[inext] = num;
            this.inext = inext;
            this.inextp = inextp;
            return num;
        }
    }
}