using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace raumPlayer.Helpers
{
    public static class StringExtension
    {
        public static string ComputeMD5(this string str)
        {
            HashAlgorithmProvider hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            IBuffer hashedBuffer = hashAlgorithm.HashData(buffer);
            return CryptographicBuffer.EncodeToHexString(hashedBuffer);
        }
    }
}
