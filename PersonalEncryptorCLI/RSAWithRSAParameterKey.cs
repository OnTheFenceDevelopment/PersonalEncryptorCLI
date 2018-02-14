using System.IO;
using System.Security.Cryptography;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class RSAWithRSAParameterKey
    {
        public void GenerateKeyPair(int keyLength, string keyPrefix, string outputLocation)
        {
            using (var rsa = new RSACryptoServiceProvider(keyLength))
            {                
                rsa.PersistKeyInCsp = false;

                var publicKey = rsa.ToXmlString(false);
                File.WriteAllText(Path.Combine(outputLocation, $"{keyPrefix}PublicKey.xml"), publicKey);

                var privateKey = rsa.ToXmlString(true);
                File.WriteAllText(Path.Combine(outputLocation, $"{keyPrefix}PrivateKey.xml"), privateKey);
            }
        }

        public byte[] EncryptData(byte[] dataToEncrypt, int keyLength, string pathToPublicKey)
        {
            byte[] cipherbytes;

            using (var rsa = new RSACryptoServiceProvider(keyLength))
            {
                rsa.PersistKeyInCsp = false;

                var publicKey = File.ReadAllText(pathToPublicKey);

                rsa.FromXmlString(publicKey);

                cipherbytes = rsa.Encrypt(dataToEncrypt, false);
            }

            return cipherbytes;
        }

        public byte[] DecryptData(byte[] dataToEncrypt, int keyLength, string pathToPrivateKey)
        {
            byte[] plain;

            using (var rsa = new RSACryptoServiceProvider(keyLength))
            {
                rsa.PersistKeyInCsp = false;

                var privateKey = File.ReadAllText(pathToPrivateKey);
                rsa.FromXmlString(privateKey);
                plain = rsa.Decrypt(dataToEncrypt, false);
            }

            return plain;
        }
    }
}