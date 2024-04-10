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
    internal class CimfsDeviceRecord : DeviceRecord
    {
        // supported Cimfs version - in the future this needs to be a list of supported values
        public const UInt32 CompatibleVersion2 = 2;

        public UInt32 Version;
        public UInt32 ParentOffset;
        public UInt32 RootPathOffset;
        public UInt32 RootPathLength;
        public Guid TargetMountVolume;

        private const int _size = 16 /* size of Guid */ +
                                  (sizeof(UInt32) * 4) /* remaining fields */ +
                                  (sizeof(int) * 2) /* Header fields */ +
                                  0x8 /* Header offset */;

        public override int Size => _size;

        public override void GetBytes(byte[] data, int offset)
        {
            WriteHeader(data, offset);

            int localOffset = offset + 0x10;
            EndianUtilities.WriteBytesLittleEndian(Version, data, localOffset);
            if (Version == CompatibleVersion2)
            {
                localOffset += sizeof(UInt32);
                EndianUtilities.WriteBytesLittleEndian(ParentOffset, data, localOffset);
                localOffset += sizeof(UInt32);
                EndianUtilities.WriteBytesLittleEndian(RootPathOffset, data, localOffset);
                localOffset += sizeof(UInt32);
                EndianUtilities.WriteBytesLittleEndian(RootPathLength, data, localOffset);
                localOffset += sizeof(UInt32);
                EndianUtilities.WriteBytesLittleEndian(TargetMountVolume, data, localOffset);
            }
        }

        public override string ToString()
        {
            var cimfsInfo = new StringBuilder("Cimfs - (");
            if (Version == CompatibleVersion2)
            {
                cimfsInfo.AppendFormat(
                    CultureInfo.InvariantCulture,
                    $"version:{Version}, parentOffset:{ParentOffset}, rootPathOffset:{RootPathOffset}, " +
                    $"rootPathLength:{RootPathLength}, targetMountVolume:{TargetMountVolume}");
            }
            else
            {
                cimfsInfo.AppendFormat(CultureInfo.InvariantCulture, $"(version:{Version}");
            }

            cimfsInfo.Append(")");
            return cimfsInfo.ToString();
        }

        protected override void DoParse(byte[] data, int offset)
        {
            base.DoParse(data, offset);

            int localOffset = offset + 0x10;
            Version = EndianUtilities.ToUInt32LittleEndian(data, localOffset);
            if (Version == CompatibleVersion2)
            {
                localOffset += sizeof(UInt32);
                ParentOffset = EndianUtilities.ToUInt32LittleEndian(data, localOffset);
                localOffset += sizeof(UInt32);
                RootPathOffset = EndianUtilities.ToUInt32LittleEndian(data, localOffset);
                localOffset += sizeof(UInt32);
                RootPathLength = EndianUtilities.ToUInt32LittleEndian(data, localOffset);
                localOffset += sizeof(UInt32);
                TargetMountVolume = EndianUtilities.ToGuidLittleEndian(data, localOffset);
            }
            else
            {
                throw new NotImplementedException($"Unknown Cimfs version: {Version}");
            }
        }
    }
}
