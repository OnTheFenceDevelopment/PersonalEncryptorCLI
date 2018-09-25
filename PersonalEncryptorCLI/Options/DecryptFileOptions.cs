using CommandLine;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI.Options
{
    [Verb("decryptfile", HelpText = "Decrypts specified Encrypted Packet into an Plain Text File")]
    public class DecryptFileOptions
    {
        [Option('p',"pathtopacket", HelpText = "Full path to Encrypted Packet (including filename)")]
        public string EncryptedPacketPath { get; set; }

        [Option('k', "keylength", HelpText = "The bit length of the encryption keys", Default = 2048)]
        public int KeyBitLength { get; set; }

        [Option('s', "senderkeypath", HelpText = "Full path to Senders PUBLIC key (including filename)", Required = true)]
        public string SenderKeyPath { get; set; }

        [Option('r', "recipientkeypath", HelpText = "Full path to Recipients PRIVATE key (including filename)", Required = true)]
        public string RecipientKeyPath { get; set; }

        [Option('o', "output", HelpText = "Output path for generated decrypted file (excluding filename)")]
        public string OutputPath { get; set; }
    }
}
