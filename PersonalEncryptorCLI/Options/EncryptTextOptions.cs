using CommandLine;

namespace OnTheFenceDevelopment.PersonalEncryptorCLI.Options
{
    [Verb("encrypttext", HelpText = "Encrypts specified Test into an Encrypted Packet")]
    public class EncryptTextOptions
    {
        [Option('t', "text", HelpText = "The Text to be encrypted", Required = true)]
        public string Text { get; set; }

        [Option('k', "keylength", HelpText = "The bit length of the encryption keys", Default = 2048)]
        public int KeyBitLength { get; set; }

        [Option('s', "senderkeypath", HelpText = "Full path to Senders PRIVATE key", Required = true)]
        public string SenderKeyPath { get; set; }

        [Option('r',"recipientkeypath", HelpText ="Full path to Recipients PUBLIC key", Required = true)]
        public string RecipientKeyPath { get; set; }

        [Option('o', "output", HelpText = "Output path for generated encrypted packet file")]
        public string OutputPath { get; set; }
    }
}
