﻿namespace fiskaltrust.Middleware.SCU.IT.Epson
{
    public class EpsonScuConfiguration
    {
        /// <summary>
        /// The URL or IP address of the RT Printer or Server, e.g. http://192.168.0.100
        /// </summary>
        public string DeviceUrl { get; set; } = "http://127.0.0.1:4321";

        /// <summary>
        /// The HTTP client timeout used when communicating with the RT Printer or Server
        /// </summary>
        public int ClientTimeoutMs { get; set; } = 15000;

        /// <summary>
        /// The server/printer timeout for executing commands
        /// </summary>
        public int ServerTimeoutMs { get; set; } = 10000;





        /// <summary>
        /// position defines the starting position from the left margin (range 0 to 511). Three special values 
        /// can also be used:
        /// o 900 = Left aligned
        /// o 901 = Centred
        ///  o 902 = Right aligned
        /// </summary>
        public int BarCodePosition { get; set; } = 10;

        /// <summary>
        /// Indicates the print dot width of each distinct bar (range 1 to 6). Please note that not all readers are able to read 
        /// barcodes with a 1 dot width
        /// </summary>
        public int BarCodeWidth { get; set; } = 2;

        /// <summary>
        ///  Indicates the height of the bar code measured in print dots (range 1 to 255).
        /// </summary>
        public int BarCodeHeight { get; set; } = 66;

        /// <summary>
        ///  selects one of three ways to print the alphanumeric representation of the barcode 
        /// or to disable it altogether.
        /// The options are as follows:
        /// o 0 = Disabled
        /// o 1 = Above the barcode
        /// o 2 = Below the barcode
        /// o 3 = Below and above the barcode
        /// </summary>
        public int BarCodeHRIPosition { get; set; } = 1;

        /// <summary>
        ///  Indicates the font to be used for the HRI string.The options are as follows:
        ///  o A
        ///  o B
        ///  o C
        /// </summary>
        public char BarCodeHRIFont { get; set; } = 'C';

        /// <summary>
        ///  Indicates the barcode or QR code standard.Choose from one of the following:
        ///  o UPC-A => 65
        ///  o UPC-E / 66
        ///  o EAN13 / 67
        ///  o EAN8 / 68
        ///  o CODE39 / 69
        ///  o ITF / 70
        ///  o CODABAR / 71
        ///  o CODE93 / 72
        ///  o CODE128 / 73
        ///  o 74 to 78 *
        ///  o QRCODE1 / 91
        ///  o QRCODE2 / 92
        /// </summary>
        public string CodeType { get; set; } = "CODE39";

        /// <summary>
        /// This command allows the real-time subtotal to be printed and/or shown on the display.
        /// • option sets the subtotal option:
        ///     o 0 = Print and show on the display
        ///     o 1 = Only print
        ///     o 2 = Only show on the display
        /// </summary>
        public int PrintRecSubtotal { get; set; }

        /// <summary>
        /// AdditionalMessageFont
        /// </summary>
        public int AdditionalMessageFont { get; set; }
    }
}