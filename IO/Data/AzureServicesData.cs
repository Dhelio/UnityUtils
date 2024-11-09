using Castrimaris.Core;

namespace Castrimaris.IO.AzureDataStructures {

    #region Enums
    /// <summary>
    /// List of italian voices, with the associated id
    /// </summary>
    public enum ItalianVoices {
        [StringValue("it-IT-ElsaNeural")] Elsa,
        [StringValue("it-IT-IsabellaNeural")] Isabella,
        [StringValue("it-IT-DiegoNeural")] Diego,
        [StringValue("it-IT-BenignoNeural")] Benigno,
        [StringValue("it-IT-CalimeroNeural")] Calimero,
        [StringValue("it-IT-CataldoNeural")] Cataldo,
        [StringValue("it-IT-FabiolaNeural")] Fabiola,
        [StringValue("it-IT-FiammaNeural")] Fiamma,
        [StringValue("it-IT-GianniNeural")] Gianni,
        [StringValue("it-IT-ImeldaNeural")] Imelda,
        [StringValue("it-IT-IrmaNeural")] Irma,
        [StringValue("it-IT-LisandroNeural")] Lisandro,
        [StringValue("it-IT-PalmiraNeural")] Palmira,
        [StringValue("it-IT-PierinaNeural")] Pierina,
        [StringValue("it-IT-RinaldoNeural")] Rinaldo,
        [StringValue("it-IT-GiuseppeNeural")] Giuseppe
    }

    /// <summary>
    /// List of US voices, with the associated id
    /// </summary>
    public enum AmericanVoices {
        [StringValue("en-US-AvaNeural")] Ava,
        [StringValue("en-US-AndrewNeural")] Andrew,
        [StringValue("en-US-EmmaNeural")] Emma,
        [StringValue("en-US-BrianNeural")] Brian,
        [StringValue("en-US-JennyNeural")] Jenny,
        [StringValue("en-US-GuyNeural")] Guy,
        [StringValue("en-US-AriaNeural")] Aria,
        [StringValue("en-US-DavisNeural")] Davis,
        [StringValue("en-US-JaneNeural")] Jane,
        [StringValue("en-US-JasonNeural")] Jason,
        [StringValue("en-US-SaraNeural")] Sara,
        [StringValue("en-US-TonyNeural")] Tony,
        [StringValue("en-US-NancyNeural")] Nancy,
        [StringValue("en-US-AmberNeural")] Amber,
        [StringValue("en-US-AnaNeuralChild)")] AnaChild,
        [StringValue("en-US-AshleyNeural")] Ashley,
        [StringValue("en-US-BrandonNeural")] Brandon,
        [StringValue("en-US-ChristopherNeural")] Christopher,
        [StringValue("en-US-CoraNeural")] Cora,
        [StringValue("en-US-ElizabethNeural")] Elizabeth,
        [StringValue("en-US-EricNeural")] Eric,
        [StringValue("en-US-JacobNeural")] Jacob,
        [StringValue("en-US-JennyMultilingualNeural3")] JennyMultilingual3,
        [StringValue("en-US-JennyMultilingualV2Neural3")] JennyMultilingualV23,
        [StringValue("en-US-MichelleNeural")] Michelle,
        [StringValue("en-US-MonicaNeural")] Monica,
        [StringValue("en-US-RogerNeural")] Roger,
        [StringValue("en-US-RyanMultilingualNeural3")] RyanMultilingual3,
        [StringValue("en-US-SteffanNeural")] Steffan,
        [StringValue("en-US-AIGenerate1Neural1")] AIGenerate11,
        [StringValue("en-US-AIGenerate2Neural1")] AIGenerate21,
        [StringValue("en-US-AndrewMultilingualNeural1Male)")] AndrewMultilingual1Male,
        [StringValue("en-US-AvaMultilingualNeural1Female)")] AvaMultilingual1Female,
        [StringValue("en-US-BlueNeural1")] Blue1,
        [StringValue("en-US-BrianMultilingualNeural1Male)")] BrianMultilingual1Male,
        [StringValue("en-US-EmmaMultilingualNeural1Female)")] EmmaMultilingual1Female,
        [StringValue("en-US-AlloyMultilingualNeural4")] AlloyMultilingual4,
        [StringValue("en-US-EchoMultilingualNeural4")] EchoMultilingual4,
        [StringValue("en-US-FableMultilingualNeural4")] FableMultilingual4,
        [StringValue("en-US-OnyxMultilingualNeural4")] OnyxMultilingual4,
        [StringValue("en-US-NovaMultilingualNeural4")] NovaMultilingual4,
        [StringValue("en-US-ShimmerMultilingualNeural4")] ShimmerMultilingual4,
        [StringValue("en-US-AlloyMultilingualNeuralHD4")] AlloyMultilingualHD4,
        [StringValue("en-US-EchoMultilingualNeuralHD4")] EchoMultilingualHD4,
        [StringValue("en-US-FableMultilingualNeuralHD4")] FableMultilingualHD4,
        [StringValue("en-US-OnyxMultilingualNeuralHD4")] OnyxMultilingualHD4,
        [StringValue("en-US-NovaMultilingualNeuralHD4")] NovaMultilingualHD4,
        [StringValue("en-US-ShimmerMultilingualNeuralHD4")] ShimmerMultilingualHD4
    }

    /// <summary>
    /// Supported languages, with the associated id
    /// </summary>
    public enum Languages {
        [StringValue("it-IT")] Italian,
        [StringValue("en-US")] American
    }

    /// <summary>
    /// Supported server regions, with the associated id
    /// </summary>
    public enum Regions {
        [StringValue("westeurope")] West_Europe
    }
    #endregion

}