using System.IO;
using System.Security.Cryptography;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class RSAWithRSAParameterKey
    {
        //private RSAParameters _publicKey;
        //private RSAParameters _privateKey;

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

        //public byte[] EncryptData(byte[] dataToEncrypt)
        //{
        //    byte[] cipherbytes;

        //    using (var rsa = new RSACryptoServiceProvider(2048))
        //    {
        //        rsa.PersistKeyInCsp = false;                
        //        rsa.ImportParameters(_publicKey);


        //        cipherbytes = rsa.Encrypt(dataToEncrypt, false);
        //    }

        //    return cipherbytes;
        //}

        //public byte[] DecryptData(byte[] dataToEncrypt)
        //{
        //    byte[] plain;

        //    using (var rsa = new RSACryptoServiceProvider(2048))
        //    {
        //        rsa.PersistKeyInCsp = false;
                                
        //        rsa.ImportParameters(_privateKey);
        //        plain = rsa.Decrypt(dataToEncrypt, false);
        //    }

        //    return plain;
        //}
    }
}