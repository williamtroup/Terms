using System;
using System.Security.Cryptography;
using System.Text;

namespace Terms.Tools.Actions
{
    public static class Cypher
    {
        #region Private Statics

        private static readonly byte[] RgbKey = { 8, 6, 1, 4, 3, 7, 2, 5 };
        private static readonly byte[] RgbIv = { 8, 6, 1, 4, 3, 7, 2, 5 };

        #endregion

        public static string Encrypt(string text)
        {
            string updatedString = text;

            if (!string.IsNullOrEmpty(text))
            {
                using (SymmetricAlgorithm symmetricAlgorithm = DES.Create())
                using (ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateEncryptor(RgbKey, RgbIv))
                {
                    byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
                    byte[] outputBuffer = cryptoTransform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);

                    updatedString = Convert.ToBase64String(outputBuffer);
                }
            }

            return updatedString;
        }

        public static string Decrypt(string text)
        {
            string updatedString = text;

            if (!string.IsNullOrEmpty(text))
            {
                using (SymmetricAlgorithm symmetricAlgorithm = DES.Create())
                using (ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateDecryptor(RgbKey, RgbIv))
                {
                    byte[] inputbuffer = Convert.FromBase64String(text);
                    byte[] outputBuffer = cryptoTransform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);

                    updatedString = Encoding.Unicode.GetString(outputBuffer);
                }
            }

            return updatedString;
        }
    }
}