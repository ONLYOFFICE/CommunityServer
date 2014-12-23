/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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