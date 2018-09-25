namespace OnTheFenceDevelopment.PersonalEncryptorCLI
{
    public class DecryptedData
    {
        public byte[] FileContents { get; set; }

        public string Filename { get; set; }

        public bool Version1Packet { get; set; }
    }
}
