namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class EncryptedPacket
    {
        public byte[] EncryptedSessionKey;
        public byte[] EncryptedData;
        public byte[] Filename;
        public byte[] Iv;
        public byte[] Hmac;
        public byte[] Signature;
    }
}
