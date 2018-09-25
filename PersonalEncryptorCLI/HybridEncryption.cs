using System.Security.Cryptography;
using System.Text;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class HybridEncryption
    {
        private readonly AesEncryption _aes = new AesEncryption();

        public EncryptedPacket EncryptData(byte[] original, int keyLength, RSAEncryption rsaParams, DigitalSignature digitalSignature, string pathToRecipientPublicKey, string pathToSenderPrivateKey, string filename)
        {
            var sessionKey = _aes.GenerateRandomNumber(32);

            var encryptedPacket = new EncryptedPacket { Iv = _aes.GenerateRandomNumber(16) };

            encryptedPacket.EncryptedData = _aes.Encrypt(original, sessionKey, encryptedPacket.Iv);

            encryptedPacket.Filename = _aes.Encrypt(Encoding.ASCII.GetBytes(filename), sessionKey, encryptedPacket.Iv);

            encryptedPacket.EncryptedSessionKey = rsaParams.EncryptData(sessionKey, keyLength, pathToRecipientPublicKey);

            using (var hmac = new HMACSHA256(sessionKey))
            {
                encryptedPacket.Hmac = hmac.ComputeHash(encryptedPacket.EncryptedData);
            }

            encryptedPacket.Signature = digitalSignature.SignData(encryptedPacket.Hmac, keyLength, pathToSenderPrivateKey);

            return encryptedPacket;
        }

        public DecryptedData DecryptData(EncryptedPacket encryptedPacket, int keyLength, RSAEncryption rsaParams, DigitalSignature digitalSignature, string pathToRecipientPrivateKey, string pathToSenderPublicKey)
        {
            var decryptedSessionKey = rsaParams.DecryptData(encryptedPacket.EncryptedSessionKey, keyLength, pathToRecipientPrivateKey);

            using (var hmac = new HMACSHA256(decryptedSessionKey))
            {
                var hmacToCheck = hmac.ComputeHash(encryptedPacket.EncryptedData);

                if (!Compare(encryptedPacket.Hmac, hmacToCheck))
                {
                    throw new CryptographicException("HMAC for decryption does not match encrypted packet.");
                }

                if (!digitalSignature.VerifySignature(encryptedPacket.Hmac, encryptedPacket.Signature, keyLength, pathToSenderPublicKey))
                {
                    throw new CryptographicException("Digital Signature can not be verified.");
                }
            }

            var decryptedData = _aes.Decrypt(encryptedPacket.EncryptedData, decryptedSessionKey, encryptedPacket.Iv);
            var originalFilename = _aes.Decrypt(encryptedPacket.Filename, decryptedSessionKey, encryptedPacket.Iv);

            return new DecryptedData { FileContents = decryptedData, Filename = Encoding.ASCII.GetString(originalFilename) };
        }

        private static bool Compare(byte[] array1, byte[] array2)
        {
            var result = array1.Length == array2.Length;

            for (var i = 0; i < array1.Length && i < array2.Length; ++i)
            {
                result &= array1[i] == array2[i];
            }

            return result;
        }
    }
}
