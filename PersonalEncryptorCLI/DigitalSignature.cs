using System.IO;
using System.Security.Cryptography;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class DigitalSignature
    {
        public byte[] SignData(byte[] hashOfDataToSign, int keyLength, string pathToPrivateKey)
        {
            using (var rsa = new RSACryptoServiceProvider(keyLength))
            {
                rsa.PersistKeyInCsp = false;

                var privateKey = File.ReadAllText(pathToPrivateKey);

                rsa.FromXmlString(privateKey);
                
                var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);                
                rsaFormatter.SetHashAlgorithm("SHA256");

                return rsaFormatter.CreateSignature(hashOfDataToSign);
            }
        }

        public bool VerifySignature(byte[] hashOfDataToSign, byte[] signature, int keyLength, string pathToPublicKey)
        {
            using (var rsa = new RSACryptoServiceProvider(keyLength))
            {
                var publicKey = File.ReadAllText(pathToPublicKey);
                rsa.FromXmlString(publicKey);

                var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaDeformatter.SetHashAlgorithm("SHA256");

                return rsaDeformatter.VerifySignature(hashOfDataToSign, signature);
            }
        }   
    }
}