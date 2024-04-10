//
// Copyright (c) 2023, Vinod Chavva
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Globalization;
using System.Text;
using DiscUtils.Streams;

namespace DiscUtils.BootConfig
{
    internal class CompositeDeviceRecord : DeviceRecord
    {
        // supported Composite format verison.
        public const UInt32 CompatibleVersion = 0;

        public Guid Signature;
        public UInt32 Version;
        public UInt32 PrimaryDeviceOffset;
        public UInt32 SecondaryDeviceOffset;

        private const int _guidSize = 16 /* size of Guid */;
        private const int _size = _guidSize +
                                  (sizeof(UInt32) * 3) /* Other Uint fields */ +
                                  (sizeof(int) * 2) /* Header fields */ +
                                  0x8 /* Header offset */;

        public override int Size => _size;
 
        public override void GetBytes(byte[] data, int offset)
        {
            WriteHeader(data, offset);

            if (Version == CompatibleVersion)
            {
                int localOffset = offset + 0x10;
                EndianUtilities.WriteBytesLittleEndian(Signature, data, localOffset);
                localOffset += _guidSize; // guid bytes
                EndianUtilities.WriteBytesLittleEndian(Version, data, localOffset);
                localOffset += sizeof(UInt32);
                EndianUtilities.WriteBytesLittleEndian(PrimaryDeviceOffset, data, localOffset);
                localOffset += sizeof(UInt32);
                EndianUtilities.WriteBytesLittleEndian(SecondaryDeviceOffset, data, localOffset);
            }
        }

        public override string ToString()
        {
            var compositeInfo = new StringBuilder("Composite - (");
            if (Version == CompatibleVersion)
            {
                compositeInfo.AppendFormat(
                    CultureInfo.InvariantCulture,
                    $"version:{Version}, signature:{Signature}, primaryDeviceOffset:{PrimaryDeviceOffset}, " +
                    $"secondaryDeviceOffset:{SecondaryDeviceOffset}");
            }
            else
            {
                compositeInfo.AppendFormat(CultureInfo.InvariantCulture, $"version:{Version}");
            }

            compositeInfo.Append(")");
            return compositeInfo.ToString();
        }

        protected override void DoParse(byte[] data, int offset)
        {
            base.DoParse(data, offset);

            int localOffset = offset + 0x10;
            Signature = EndianUtilities.ToGuidBigEndian(data, localOffset);
            localOffset += _guidSize; // guid bytes
            Version = EndianUtilities.ToUInt32LittleEndian(data, localOffset);

            if (Version != CompatibleVersion)
            {
                throw new NotImplementedException($"Unknown Composite version: {Version}");
            }

            localOffset += sizeof(UInt32);
            PrimaryDeviceOffset = EndianUtilities.ToUInt32LittleEndian(data, localOffset);
            localOffset += sizeof(UInt32);
            SecondaryDeviceOffset = EndianUtilities.ToUInt32LittleEndian(data, localOffset);
        }
    }
}
