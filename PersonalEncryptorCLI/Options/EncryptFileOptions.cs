using CommandLine;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI.Options
{
    [Verb("encryptfile", HelpText = "Encrypts specified File into an Encrypted Packet")]
    public class EncryptFileOptions
    {
        [Option('f', "filepath", HelpText = "The path to the file to be encrypted (including filename)", Required = true)]
        public string FilePath { get; set; }

        [Option('k', "keylength", HelpText = "The bit length of the encryption keys", Default = 2048)]
        public int KeyBitLength { get; set; }

        [Option('s', "senderkeypath", HelpText = "Full path to Senders PRIVATE key (including filename)", Required = true)]
        public string SenderKeyPath { get; set; }

        [Option('r',"recipientkeypath", HelpText = "Full path to Recipients PUBLIC key (including filename)", Required = true)]
        public string RecipientKeyPath { get; set; }

        [Option('o', "output", HelpText = "Output path for generated encrypted packet file (including filename)")]
        public string OutputPath { get; set; }
    }
}
