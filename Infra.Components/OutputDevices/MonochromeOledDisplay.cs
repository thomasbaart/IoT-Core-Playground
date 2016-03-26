using Infra.Components.Platform.Gpio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Components.OutputDevices
{
    public class MonochromeOledDisplay : IMonochromeOledDisplay
    {
        
    }

    public class SSD1306DeviceConfiguration
    {
        private byte _vccState = SWITCHCAPVCC;

        public byte VccState
        {
            get
            {
                return _vccState;
            }

            private set
            {
                _vccState = value;
            }
        }

        #region command
        /* Uncomment for Raspberry Pi 2 */
        public static readonly string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        public static readonly uint SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
        public static readonly uint DATA_COMMAND_PIN = 22;          /* We use GPIO 22 since it's conveniently near the SPI pins */
        public static readonly uint RESET_PIN = 23;                 /* We use GPIO 23 since it's conveniently near the SPI pins */

        /* This sample is intended to be used with the following OLED display: http://www.adafruit.com/product/938 */
        public uint ScreenWidthPx { get; private set; }
        public uint ScreenHeigthPx { get; private set; }
        public uint ScreenHeigthPages { get; private set; }
        public byte[,] DisplayBuffer { get; set; }
        public byte[] SerializedDisplayBuffer { get; set; }

        /* Display commands. See the datasheet for details on commands: http://www.adafruit.com/datasheets/SSD1306.pdf                      */
        public static readonly byte[] CMD_DISPLAY_OFF = { 0xAE };              /* Turns the display off                                    */
        public static readonly byte[] CMD_DISPLAY_ON = { 0xAF };               /* Turns the display on                                     */
        public static readonly byte[] CMD_CHARGEPUMP_ON = { 0x8D };      /* Turn on internal charge pump to supply power to display  */

        public static readonly byte[] CMD_0x80 = { 0x80 };
        public static readonly byte[] CMD_SETDISPLAYCLOCKDIV = { 0xD5 };
        public static readonly byte[] CMD_SETMULTIPLEX = { 0xA8 };
        public static readonly byte[] CMD_0x1F = { 0x1F };
        public static readonly byte[] CMD_SETDISPLAYOFFSET = { 0xD3 };
        public static readonly byte[] CMD_0x00 = { 0x00 };
        public static readonly byte[] CMD_0x02 = { 0x02 };
        public static readonly byte[] CMD_0x8F = { 0x8F };
        public static readonly byte[] CMD_0xF1 = { 0xF1 };
        public static readonly byte[] CMD_0x40 = { 0x40 };
        public static readonly byte[] CMD_0x14 = { 0x14 };
        public static readonly byte[] CMD_0x10 = { 0x10 };
        public static readonly byte[] CMD_0x22 = { 0x22 };

        public static readonly byte[] CMD_0x12 = { 0x12 };
        public static readonly byte[] CMD_0x3F = { 0x3F };
        public static readonly byte[] CMD_0x9F = { 0x9F };
        public static readonly byte[] CMD_0xCF = { 0xCF };

        public static readonly byte[] CMD_SETSTARTLINE = { 0x40 };
        public static readonly byte[] CMD_CHARGEPUMP = { 0x8D };
        public static readonly byte[] CMD_MEMORYMODE = { 0x20 };
        public static readonly byte[] CMD_SEGREMAP = { 0xA0 }; /*other possible value is { 0xA1 }; *//* Remaps the segments, which has the effect of mirroring the display horizontally */

        public static readonly byte[] CMD_SETCOMPINS = { 0xDA };
        public static readonly byte[] CMD_SETCONTRAST = { 0x81 };
        public static readonly byte[] CMD_SETPRECHARGE = { 0xd9 };
        public static readonly byte[] CMD_SETVCOMDETECT = { 0xD8 };
        public static readonly byte[] CMD_DISPLAYALLON_RESUME = { 0xA4 };
        public static readonly byte[] CMD_NORMALDISPLAY = { 0xA6 };
        public static readonly byte[] CMD_INVERTDISPLAY = { 0xA7 };

        private const byte EXTERNALVCC = 0x1;
        private const byte SWITCHCAPVCC = 0x2;

        private static readonly byte[] CMD_MEMADDRMODE = { 0x20, 0x00 };        /* Horizontal memory mode                                   */

        public static readonly byte[] CMD_COMSCANDEC = { 0xC8 };               /* Set the COM scan direction to inverse, which flips the screen vertically        */
        public static readonly byte[] CMD_COMSCANIN = { 0xC0 };
        public static readonly byte[] CMD_RESETCOLADDR = { 0x21, 0x00, 0x7F }; /* Reset the column address pointer                         */
        public static readonly byte[] CMD_RESETPAGEADDR = { 0x22, 0x00, 0x07 };/* Reset the page address pointer     */

        #endregion command

        public SSD1306DeviceConfiguration(byte __vccState = SWITCHCAPVCC)
        {
            VccState = __vccState;

            ScreenWidthPx = 128;
            ScreenHeigthPx = 64;

            ScreenHeigthPages = ScreenHeigthPx / 8;
            DisplayBuffer = new byte[ScreenWidthPx, ScreenHeigthPages];
            SerializedDisplayBuffer = new byte[ScreenWidthPx * ScreenHeigthPages];
        }

        public byte[] InitCommand()
        {
            List<byte> returnArray = new List<byte>();

            returnArray.AddRange(CMD_DISPLAY_OFF);          // 0xAE Set the display off
            returnArray.AddRange(CMD_SETDISPLAYCLOCKDIV);   // 0xD5 Set the Clock Divide Ratio
            returnArray.AddRange(CMD_0x80);                 // 0x80 ...to a value of 8 (high nibble)
            returnArray.AddRange(CMD_SETMULTIPLEX);         // 0xA8 Set the Multiplex Ratio
            returnArray.AddRange(CMD_0x3F);                 // 0x3F ...to the default value of 63

            returnArray.AddRange(CMD_SETDISPLAYOFFSET);     // 0xD3 Set Display Offset
            returnArray.AddRange(CMD_0x00);                 // 0x00 ...to the default value of 0
            returnArray.AddRange(CMD_SETSTARTLINE);         // 0x40 Set the display start line to 0 (last six bits determine the value)
            
            returnArray.AddRange(CMD_CHARGEPUMP_ON);        // 0x8D Set the charge pump setting
            if (VccState == EXTERNALVCC)
            {
                returnArray.AddRange(CMD_0x10);             // 0x10 ...disable the charge pump because the external
            }                                               //         circuit supplies enough power
            else
            {
                returnArray.AddRange(CMD_0x14);             // 0x14 ...enable the charge pump because we take
            }                                               //         power from the Raspberry Pi

            returnArray.AddRange(CMD_MEMORYMODE);           // 0x20 Set the Memory Addressing Mode...
            returnArray.AddRange(CMD_0x00);                 // 0x00 ...to Horizontal Addressing Mode
            returnArray.AddRange(CMD_SEGREMAP);             // 0xA0 Set Segment Re-map to no horizontal mirroring
            returnArray.AddRange(CMD_COMSCANIN);            // 0xC0 Set COM Output Scan Direction to no vertical mirroring
            returnArray.AddRange(CMD_SETCOMPINS);           // 0xDA Set COM Pins Hardware Configuration
            returnArray.AddRange(CMD_0x12);                 // 0x12 ...Alternative COM pin configuration (A[4]=1, default)
                                                            //      ...Disable COM Left/Right remap (A[5]=0, default)
            returnArray.AddRange(CMD_SETCONTRAST);          // 0x81 Set Contrast Control
            if (VccState == EXTERNALVCC)
            {
                returnArray.AddRange(CMD_0x9F);             // 0x9F ...to 159, which is 62%
            }
            else
            {
                returnArray.AddRange(CMD_0xCF);             // 0xCF ...to 207, which is 81%
            }
            returnArray.AddRange(CMD_SETPRECHARGE);         // 0xD9 Set Pre-charge period
            if (VccState == EXTERNALVCC)
            {
                returnArray.AddRange(CMD_0x22);             // 0x22 ...to Phase 1 = 2 clocks and Phase 2 = 2 clocks (defaults)
            }                                               //      Phase 1 discharges pixels. Phase 2 charges them back up.
            else                                            //      This creates the display refresh cycle.
            {
                returnArray.AddRange(CMD_0xF1);             // 0xF1 ...to Phase 1 = 1 clocks and Phase 2 = 15 clocks.
            }                                               // ? Waarom afwijken van de defaults wanneer er geen externe power source is?

            returnArray.AddRange(CMD_SETVCOMDETECT);        // 0xD8 Set COM signal deselected voltage level.
            returnArray.AddRange(CMD_0x40);                 // 0x40 !! Staat niet vermeld in datasheet. Zou overeenkomen met ongeveer 1 x Vcc, mits geïmplementeerd.
            returnArray.AddRange(CMD_DISPLAYALLON_RESUME);  // 0xA4 Set Entire Display ON. Output renders from RAM.
            returnArray.AddRange(CMD_NORMALDISPLAY);        // 0xA6 Set Normal/Inverse display to Normal.
            returnArray.AddRange(CMD_DISPLAY_ON);           // 0xAF Set Display ON.

            return returnArray.ToArray();
        }
    }

    public class SSD1306
    {
        #region const

        /* Definitions for SPI and GPIO */
        //private SpiDevice SpiDisplay;
        private IGpioController IoController;
        private IGpioPin DataCommandPin;
        private IGpioPin ResetPin;
        private SSD1306DeviceConfiguration DeviceConfig;

        #endregion const

        public SSD1306(IGpioController controller)
        {
            IoController = controller;
        }

        public async Task InitAll()
        {
            DeviceConfig = new SSD1306DeviceConfiguration();
            InitGpio();             /* Initialize the GPIO controller and GPIO pins */
            await InitSpi();        /* Initialize the SPI controller                */
            await InitDisplay();    /* Initialize the display                       */
        }

        /// <summary>
        /// Initialize the GPIO.
        /// </summary>
        public void InitGpio()
        {
            /* Initialize a pin as output for the Data/Command line on the display  */
            DataCommandPin = IoController.OpenPin(SSD1306DeviceConfiguration.DATA_COMMAND_PIN);
            DataCommandPin.Write(GpioPinValue.High);
            DataCommandPin.SetDriveMode(GpioPinDriveMode.Output);

            /* Initialize a pin as output for the hardware Reset line on the display */
            ResetPin = IoController.OpenPin(SSD1306DeviceConfiguration.RESET_PIN);
            ResetPin.Write(GpioPinValue.High);
            ResetPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        /// <summary>
        /// Send SPI commands to power up and initialize the display
        /// </summary>
        public async Task InitDisplay()
        {
            /* Initialize the display */
            try
            {
                /* See the datasheet for more details on these commands: http://www.adafruit.com/datasheets/SSD1306.pdf             */
                await ResetDisplay();                   /* Perform a hardware reset on the display                                  */
                DisplaySendCommand(DeviceConfig.InitCommand());
            }
            catch (Exception ex)
            {
                throw new Exception("Display Initialization Failed", ex);
            }
        }

        /// <summary>
        /// Perform a hardware reset of the display
        /// </summary>
        public async Task ResetDisplay()
        {
            ResetPin.Write(GpioPinValue.Low);   /* Put display into reset                       */
            await Task.Delay(1);                /* Wait at least 3uS (We wait 1mS since that is the minimum delay we can specify for Task.Delay() */
            ResetPin.Write(GpioPinValue.High);  /* Bring display out of reset                   */
            await Task.Delay(100);              /* Wait at least 100mS before sending commands  */
        }

        /// <summary>
        /// Send commands to the screen.
        /// </summary>
        /// <param name="Command">The commands to send.</param>
        public void DisplaySendCommand(byte[] Command)
        {
            /* When the Data/Command pin is low, SPI data is treated as commands for the display controller */
            DataCommandPin.Write(GpioPinValue.Low);
            //SpiDisplay.Write(Command);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispose all dependencies.
        /// </summary>
        public void DisposeAll()
        {
            //SpiDisplay.Dispose();
            throw new NotImplementedException();
            ResetPin.Dispose();
            DataCommandPin.Dispose();
        }

        /// <summary>
        /// Initialize the SPI bus
        /// </summary>
        public async Task InitSpi()
        {
            try
            {
                //var settings = new SpiConnectionSettings(SSD1306DeviceConfiguration.SPI_CHIP_SELECT_LINE);
                //// Datasheet specifies maximum SPI clock frequency of 10MHz
                //settings.ClockFrequency = 10000000;
                //// The display expects an idle-high clock polarity
                //// we use Mode3 to set the clock polarity and phase to: CPOL = 1, CPHA = 1
                //settings.Mode = SpiMode.Mode3;

                //string spiAqs = SpiDevice.GetDeviceSelector(SSD1306DeviceConfiguration.SPI_CONTROLLER_NAME);
                //var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);
                //SpiDisplay = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);
                throw new NotImplementedException();
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        /// <summary>
        /// Sets all pixels in the screen buffer to 0
        /// </summary>
        public void ClearDisplayBuf()
        {
            Array.Clear(DeviceConfig.DisplayBuffer, 0, DeviceConfig.DisplayBuffer.Length);
        }

        /// <summary>
        /// Writes a string to the display screen buffer (DisplayUpdate() needs to be called subsequently 
        /// to output the buffer to the screen)
        /// </summary>
        /// <param name="Line">The string we want to render. In this sample, special characters like tabs and newlines are not supported.</param>
        /// <param name="Col">The horizontal column we want to start drawing at. This is equivalent to the 'X' axis pixel position.</param>
        /// <param name="Row">The vertical row we want to write to. The screen is divided up into 4 rows of 16 pixels each, so valid values for Row are 0,1,2,3.</param>
        /// <remarks>No return value. We simply return when we encounter characters that are out-of-bounds or aren't available in the font.</remarks>
        public void WriteLineDisplayBuf(string Line, uint Col, uint Row)
        {

            uint CharWidth = 0;
            foreach (char Character in Line)
            {
                CharWidth = WriteCharDisplayBuf(Character, Col, Row);
                Col += CharWidth;   /* Increment the column so we can track where to write the next character   */
                if (CharWidth == 0) /* Quit if we encounter a character that couldn't be printed                */
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Writes one character to the display screen buffer (DisplayUpdate() needs to be called 
        /// subsequently to output the buffer to the screen).
        /// </summary>
        /// <param name="Chr">The character we want to draw. In this sample, special characters like tabs and newlines are not supported.</param>
        /// <param name="Col">The horizontal column we want to start drawing at. This is equivalent to the 'X' axis pixel position.</param>
        /// <param name="Row">The vertical row we want to write to. The screen is divided up into 4 rows of 16 pixels each, so valid values for Row are 0,1,2,3.</param>
        /// <returns>We return the number of horizontal pixels used. This value is 0 if Row/Col are out-of-bounds, or if the character isn't available in the font.</returns>
        public uint WriteCharDisplayBuf(char Chr, uint Col, uint Row)
        {
            /* Check that we were able to find the font corresponding to our character */
            FontCharacterDescriptor CharDescriptor = DisplayFontTable.GetCharacterDescriptor(Chr);
            if (CharDescriptor == null)
            {
                return 0;
            }

            /* Make sure we're drawing within the boundaries of the screen buffer */
            uint MaxRowValue = (DeviceConfig.ScreenHeigthPages / DisplayFontTable.FontHeightBytes) - 1;
            uint MaxColValue = DeviceConfig.ScreenWidthPx;
            if (Row > MaxRowValue)
            {
                return 0;
            }
            if ((Col + CharDescriptor.CharacterWidthPx + DisplayFontTable.FontCharSpacing) > MaxColValue)
            {
                return 0;
            }

            uint CharDataIndex = 0;
            uint StartPage = Row * 2;
            uint EndPage = StartPage + CharDescriptor.CharacterHeightBytes;
            uint StartCol = Col;
            uint EndCol = StartCol + CharDescriptor.CharacterWidthPx;
            uint CurrentPage = 0;
            uint CurrentCol = 0;

            /* Copy the character image into the display buffer */
            for (CurrentPage = StartPage; CurrentPage < EndPage; CurrentPage++)
            {
                for (CurrentCol = StartCol; CurrentCol < EndCol; CurrentCol++)
                {
                    DeviceConfig.DisplayBuffer[CurrentCol, CurrentPage] = CharDescriptor.CharacterData[CharDataIndex];
                    CharDataIndex++;
                }
            }

            /* Pad blank spaces to the right of the character so there exists space between adjacent characters */
            for (CurrentPage = StartPage; CurrentPage < EndPage; CurrentPage++)
            {
                for (; CurrentCol < EndCol + DisplayFontTable.FontCharSpacing; CurrentCol++)
                {
                    DeviceConfig.DisplayBuffer[CurrentCol, CurrentPage] = 0x00;
                }
            }

            /* Return the number of horizontal pixels used by the character */
            return CurrentCol - StartCol;
        }

        /// <summary>
        /// Writes the Display Buffer out to the physical screen for display.
        /// </summary>
        public void DisplayUpdate()
        {

            int Index = 0;
            /* We convert our 2-dimensional array into a serialized string of bytes that will be sent out to the display */
            for (int PageY = 0; PageY < DeviceConfig.ScreenHeigthPages; PageY++)
            {
                for (int PixelX = 0; PixelX < DeviceConfig.ScreenWidthPx; PixelX++)
                {
                    DeviceConfig.SerializedDisplayBuffer[Index] = DeviceConfig.DisplayBuffer[PixelX, PageY];
                    Index++;
                }
            }

            /* Write the data out to the screen */
            DisplaySendCommand(SSD1306DeviceConfiguration.CMD_RESETCOLADDR);         /* Reset the column address pointer back to 0 */
            DisplaySendCommand(SSD1306DeviceConfiguration.CMD_RESETPAGEADDR);        /* Reset the page address pointer back to 0   */
            DisplaySendData(DeviceConfig.SerializedDisplayBuffer);     /* Send the data over SPI                     */
        }

        /// <summary>
        /// Send graphics data to the screen.
        /// </summary>
        public void DisplaySendData(byte[] Data)
        {
            /* When the Data/Command pin is high, SPI data is treated as graphics data  */
            DataCommandPin.Write(GpioPinValue.High);
            //SpiDisplay.Write(Data);
            throw new NotImplementedException();
        }

        public void InvertDisplay(bool _invert)
        {
            DisplaySendCommand(_invert ? SSD1306DeviceConfiguration.CMD_INVERTDISPLAY : SSD1306DeviceConfiguration.CMD_NORMALDISPLAY);
        }
    }

    public class FontCharacterDescriptor
    {
        public readonly char Character;
        public readonly uint CharacterWidthPx;
        public readonly uint CharacterHeightBytes;
        public readonly byte[] CharacterData;

        public FontCharacterDescriptor(char Chr, uint CharHeightBytes, byte[] CharData)
        {
            Character = Chr;
            CharacterWidthPx = (uint)CharData.Length / CharHeightBytes;
            CharacterHeightBytes = CharHeightBytes;
            CharacterData = CharData;
        }
    }

    /* This class contains the character data needed to output render text on the display */
    public static class DisplayFontTable
    {
        public static readonly uint FontHeightBytes = 2;  /* Height of the characters. A value of 2 would indicate a 16 (2*8) pixel tall character */
        public static readonly uint FontCharSpacing = 1;  /* Number of blank horizontal pixels to insert between adjacent characters               */

        /* Takes and returns the character descriptor for the corresponding Char if it exists */
        public static FontCharacterDescriptor GetCharacterDescriptor(char Chr)
        {
            foreach (FontCharacterDescriptor CharDescriptor in FontTable)
            {
                if (CharDescriptor.Character == Chr)
                {
                    return CharDescriptor;
                }
            }
            return null;
        }

        /* Table with all the character data */
        private static readonly FontCharacterDescriptor[] FontTable =
        {
            new FontCharacterDescriptor(' ' ,FontHeightBytes,new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('!' ,FontHeightBytes,new byte[]{0xFE,0x05}),
            new FontCharacterDescriptor('"' ,FontHeightBytes,new byte[]{0x1E,0x00,0x1E,0x00,0x00,0x00}),
            new FontCharacterDescriptor('#' ,FontHeightBytes,new byte[]{0x80,0x90,0xF0,0x9E,0xF0,0x9E,0x10,0x00,0x07,0x00,0x07,0x00,0x00,0x00}),
            new FontCharacterDescriptor('$' ,FontHeightBytes,new byte[]{0x38,0x44,0xFE,0x44,0x98,0x02,0x04,0x0F,0x04,0x03}),
            new FontCharacterDescriptor('%' ,FontHeightBytes,new byte[]{0x0C,0x12,0x12,0x8C,0x40,0x20,0x10,0x88,0x84,0x00,0x00,0x02,0x01,0x00,0x00,0x00,0x03,0x04,0x04,0x03}),
            new FontCharacterDescriptor('&' ,FontHeightBytes,new byte[]{0x80,0x5C,0x22,0x62,0x9C,0x00,0x00,0x03,0x04,0x04,0x04,0x05,0x02,0x05}),
            new FontCharacterDescriptor('\'',FontHeightBytes,new byte[]{0x1E,0x00}),
            new FontCharacterDescriptor('(' ,FontHeightBytes,new byte[]{0xF0,0x0C,0x02,0x07,0x18,0x20}),
            new FontCharacterDescriptor(')' ,FontHeightBytes,new byte[]{0x02,0x0C,0xF0,0x20,0x18,0x07}),
            new FontCharacterDescriptor('*' ,FontHeightBytes,new byte[]{0x14,0x18,0x0E,0x18,0x14,0x00,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('+' ,FontHeightBytes,new byte[]{0x40,0x40,0xF0,0x40,0x40,0x00,0x00,0x01,0x00,0x00}),
            new FontCharacterDescriptor(',' ,FontHeightBytes,new byte[]{0x00,0x00,0x08,0x04}),
            new FontCharacterDescriptor('-' ,FontHeightBytes,new byte[]{0x40,0x40,0x40,0x40,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('.' ,FontHeightBytes,new byte[]{0x00,0x04}),
            new FontCharacterDescriptor('/' ,FontHeightBytes,new byte[]{0x00,0x80,0x70,0x0E,0x1C,0x03,0x00,0x00}),
            new FontCharacterDescriptor('0' ,FontHeightBytes,new byte[]{0xFC,0x02,0x02,0x02,0xFC,0x03,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('1' ,FontHeightBytes,new byte[]{0x04,0x04,0xFE,0x00,0x00,0x07}),
            new FontCharacterDescriptor('2' ,FontHeightBytes,new byte[]{0x0C,0x82,0x42,0x22,0x1C,0x07,0x04,0x04,0x04,0x04}),
            new FontCharacterDescriptor('3' ,FontHeightBytes,new byte[]{0x04,0x02,0x22,0x22,0xDC,0x02,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('4' ,FontHeightBytes,new byte[]{0xC0,0xA0,0x98,0x84,0xFE,0x00,0x00,0x00,0x00,0x07}),
            new FontCharacterDescriptor('5' ,FontHeightBytes,new byte[]{0x7E,0x22,0x22,0x22,0xC2,0x02,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('6' ,FontHeightBytes,new byte[]{0xFC,0x42,0x22,0x22,0xC4,0x03,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('7' ,FontHeightBytes,new byte[]{0x02,0x02,0xC2,0x32,0x0E,0x00,0x07,0x00,0x00,0x00}),
            new FontCharacterDescriptor('8' ,FontHeightBytes,new byte[]{0xDC,0x22,0x22,0x22,0xDC,0x03,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('9' ,FontHeightBytes,new byte[]{0x3C,0x42,0x42,0x22,0xFC,0x02,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor(':' ,FontHeightBytes,new byte[]{0x10,0x04}),
            new FontCharacterDescriptor(';' ,FontHeightBytes,new byte[]{0x00,0x10,0x08,0x04}),
            new FontCharacterDescriptor('<' ,FontHeightBytes,new byte[]{0x40,0xE0,0xB0,0x18,0x08,0x00,0x00,0x01,0x03,0x02}),
            new FontCharacterDescriptor('=' ,FontHeightBytes,new byte[]{0xA0,0xA0,0xA0,0xA0,0xA0,0x00,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('>' ,FontHeightBytes,new byte[]{0x08,0x18,0xB0,0xE0,0x40,0x02,0x03,0x01,0x00,0x00}),
            new FontCharacterDescriptor('?' ,FontHeightBytes,new byte[]{0x0C,0x02,0xC2,0x22,0x1C,0x00,0x00,0x05,0x00,0x00}),
            new FontCharacterDescriptor('@' ,FontHeightBytes,new byte[]{0xF0,0x0C,0x02,0x02,0xE1,0x11,0x11,0x91,0x72,0x02,0x0C,0xF0,0x00,0x03,0x04,0x04,0x08,0x09,0x09,0x08,0x09,0x05,0x05,0x00}),
            new FontCharacterDescriptor('A' ,FontHeightBytes,new byte[]{0x00,0x80,0xE0,0x98,0x86,0x98,0xE0,0x80,0x00,0x06,0x01,0x00,0x00,0x00,0x00,0x00,0x01,0x06}),
            new FontCharacterDescriptor('B' ,FontHeightBytes,new byte[]{0xFE,0x22,0x22,0x22,0x22,0x22,0xDC,0x07,0x04,0x04,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('C' ,FontHeightBytes,new byte[]{0xF8,0x04,0x02,0x02,0x02,0x02,0x04,0x08,0x01,0x02,0x04,0x04,0x04,0x04,0x02,0x01}),
            new FontCharacterDescriptor('D' ,FontHeightBytes,new byte[]{0xFE,0x02,0x02,0x02,0x02,0x02,0x04,0xF8,0x07,0x04,0x04,0x04,0x04,0x04,0x02,0x01}),
            new FontCharacterDescriptor('E' ,FontHeightBytes,new byte[]{0xFE,0x22,0x22,0x22,0x22,0x22,0x02,0x07,0x04,0x04,0x04,0x04,0x04,0x04}),
            new FontCharacterDescriptor('F' ,FontHeightBytes,new byte[]{0xFE,0x22,0x22,0x22,0x22,0x22,0x02,0x07,0x00,0x00,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('G' ,FontHeightBytes,new byte[]{0xF8,0x04,0x02,0x02,0x02,0x42,0x44,0xC8,0x01,0x02,0x04,0x04,0x04,0x04,0x02,0x07}),
            new FontCharacterDescriptor('H' ,FontHeightBytes,new byte[]{0xFE,0x20,0x20,0x20,0x20,0x20,0x20,0xFE,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x07}),
            new FontCharacterDescriptor('I' ,FontHeightBytes,new byte[]{0xFE,0x07}),
            new FontCharacterDescriptor('J' ,FontHeightBytes,new byte[]{0x00,0x00,0x00,0x00,0xFE,0x03,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('K' ,FontHeightBytes,new byte[]{0xFE,0x20,0x50,0x88,0x04,0x02,0x00,0x07,0x00,0x00,0x00,0x01,0x02,0x04}),
            new FontCharacterDescriptor('L' ,FontHeightBytes,new byte[]{0xFE,0x00,0x00,0x00,0x00,0x00,0x07,0x04,0x04,0x04,0x04,0x04}),
            new FontCharacterDescriptor('M' ,FontHeightBytes,new byte[]{0xFE,0x18,0x60,0x80,0x00,0x80,0x60,0x18,0xFE,0x07,0x00,0x00,0x01,0x06,0x01,0x00,0x00,0x07}),
            new FontCharacterDescriptor('N' ,FontHeightBytes,new byte[]{0xFE,0x04,0x18,0x20,0x40,0x80,0x00,0xFE,0x07,0x00,0x00,0x00,0x00,0x01,0x02,0x07}),
            new FontCharacterDescriptor('O' ,FontHeightBytes,new byte[]{0xF8,0x04,0x02,0x02,0x02,0x02,0x04,0xF8,0x01,0x02,0x04,0x04,0x04,0x04,0x02,0x01}),
            new FontCharacterDescriptor('P' ,FontHeightBytes,new byte[]{0xFE,0x42,0x42,0x42,0x42,0x42,0x24,0x18,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('Q' ,FontHeightBytes,new byte[]{0xF8,0x04,0x02,0x02,0x02,0x02,0x04,0xF8,0x01,0x02,0x04,0x04,0x04,0x05,0x02,0x05}),
            new FontCharacterDescriptor('R' ,FontHeightBytes,new byte[]{0xFE,0x42,0x42,0x42,0x42,0x42,0x64,0x98,0x00,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x03,0x04}),
            new FontCharacterDescriptor('S' ,FontHeightBytes,new byte[]{0x1C,0x22,0x22,0x22,0x42,0x42,0x8C,0x03,0x04,0x04,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('T' ,FontHeightBytes,new byte[]{0x02,0x02,0x02,0x02,0xFE,0x02,0x02,0x02,0x02,0x00,0x00,0x00,0x00,0x07,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('U' ,FontHeightBytes,new byte[]{0xFE,0x00,0x00,0x00,0x00,0x00,0x00,0xFE,0x01,0x02,0x04,0x04,0x04,0x04,0x02,0x01}),
            new FontCharacterDescriptor('V' ,FontHeightBytes,new byte[]{0x06,0x18,0x60,0x80,0x00,0x80,0x60,0x18,0x06,0x00,0x00,0x00,0x01,0x06,0x01,0x00,0x00,0x00}),
            new FontCharacterDescriptor('W' ,FontHeightBytes,new byte[]{0x0E,0x30,0xC0,0x00,0xC0,0x30,0x0E,0x30,0xC0,0x00,0xC0,0x30,0x0E,0x00,0x00,0x01,0x06,0x01,0x00,0x00,0x00,0x01,0x06,0x01,0x00,0x00}),
            new FontCharacterDescriptor('X' ,FontHeightBytes,new byte[]{0x06,0x08,0x90,0x60,0x60,0x90,0x08,0x06,0x06,0x01,0x00,0x00,0x00,0x00,0x01,0x06}),
            new FontCharacterDescriptor('Y' ,FontHeightBytes,new byte[]{0x06,0x08,0x10,0x20,0xC0,0x20,0x10,0x08,0x06,0x00,0x00,0x00,0x00,0x07,0x00,0x00,0x00,0x00}),
            new FontCharacterDescriptor('Z' ,FontHeightBytes,new byte[]{0x02,0x82,0x42,0x22,0x1A,0x06,0x06,0x05,0x04,0x04,0x04,0x04}),
            new FontCharacterDescriptor('[' ,FontHeightBytes,new byte[]{0xFE,0x02,0x02,0x3F,0x20,0x20}),
            new FontCharacterDescriptor('\\',FontHeightBytes,new byte[]{0x0E,0x70,0x80,0x00,0x00,0x00,0x03,0x1C}),
            new FontCharacterDescriptor('^' ,FontHeightBytes,new byte[]{0x02,0x02,0xFE,0x20,0x20,0x3F}),
            new FontCharacterDescriptor('_' ,FontHeightBytes,new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x10,0x10,0x10,0x10,0x10,0x10,0x10}),
            new FontCharacterDescriptor('`' ,FontHeightBytes,new byte[]{0x02,0x04,0x00,0x00}),
            new FontCharacterDescriptor('a' ,FontHeightBytes,new byte[]{0xA0,0x50,0x50,0x50,0x50,0xE0,0x00,0x03,0x04,0x04,0x04,0x04,0x03,0x04}),
            new FontCharacterDescriptor('b' ,FontHeightBytes,new byte[]{0xFE,0x20,0x10,0x10,0x10,0xE0,0x07,0x02,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('c' ,FontHeightBytes,new byte[]{0xE0,0x10,0x10,0x10,0x10,0x20,0x03,0x04,0x04,0x04,0x04,0x02}),
            new FontCharacterDescriptor('d' ,FontHeightBytes,new byte[]{0xE0,0x10,0x10,0x10,0x20,0xFE,0x03,0x04,0x04,0x04,0x02,0x07}),
            new FontCharacterDescriptor('e' ,FontHeightBytes,new byte[]{0xE0,0x90,0x90,0x90,0x90,0xE0,0x03,0x04,0x04,0x04,0x04,0x02}),
            new FontCharacterDescriptor('f' ,FontHeightBytes,new byte[]{0x10,0xFC,0x12,0x00,0x07,0x00}),
            new FontCharacterDescriptor('g' ,FontHeightBytes,new byte[]{0xE0,0x10,0x10,0x10,0x20,0xF0,0x03,0x24,0x24,0x24,0x22,0x1F}),
            new FontCharacterDescriptor('h' ,FontHeightBytes,new byte[]{0xFE,0x20,0x10,0x10,0xE0,0x07,0x00,0x00,0x00,0x07}),
            new FontCharacterDescriptor('i' ,FontHeightBytes,new byte[]{0xF2,0x07}),
            new FontCharacterDescriptor('j' ,FontHeightBytes,new byte[]{0x00,0xF2,0x20,0x1F}),
            new FontCharacterDescriptor('k' ,FontHeightBytes,new byte[]{0xFE,0x80,0xC0,0x20,0x10,0x00,0x07,0x00,0x00,0x01,0x02,0x04}),
            new FontCharacterDescriptor('l' ,FontHeightBytes,new byte[]{0xFE,0x07}),
            new FontCharacterDescriptor('m' ,FontHeightBytes,new byte[]{0xF0,0x20,0x10,0x10,0xE0,0x20,0x10,0x10,0xE0,0x07,0x00,0x00,0x00,0x07,0x00,0x00,0x00,0x07}),
            new FontCharacterDescriptor('n' ,FontHeightBytes,new byte[]{0xF0,0x20,0x10,0x10,0xE0,0x07,0x00,0x00,0x00,0x07}),
            new FontCharacterDescriptor('o' ,FontHeightBytes,new byte[]{0xE0,0x10,0x10,0x10,0x10,0xE0,0x03,0x04,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('p' ,FontHeightBytes,new byte[]{0xF0,0x20,0x10,0x10,0x10,0xE0,0x3F,0x02,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('q' ,FontHeightBytes,new byte[]{0xE0,0x10,0x10,0x10,0x20,0xF0,0x03,0x04,0x04,0x04,0x02,0x3F}),
            new FontCharacterDescriptor('r' ,FontHeightBytes,new byte[]{0xF0,0x20,0x10,0x07,0x00,0x00}),
            new FontCharacterDescriptor('s' ,FontHeightBytes,new byte[]{0x60,0x90,0x90,0x90,0x20,0x02,0x04,0x04,0x04,0x03}),
            new FontCharacterDescriptor('t' ,FontHeightBytes,new byte[]{0x10,0xFC,0x10,0x00,0x03,0x04}),
            new FontCharacterDescriptor('u' ,FontHeightBytes,new byte[]{0xF0,0x00,0x00,0x00,0xF0,0x03,0x04,0x04,0x02,0x07}),
            new FontCharacterDescriptor('v' ,FontHeightBytes,new byte[]{0x30,0xC0,0x00,0x00,0x00,0xC0,0x30,0x00,0x00,0x03,0x04,0x03,0x00,0x00}),
            new FontCharacterDescriptor('w' ,FontHeightBytes,new byte[]{0x30,0xC0,0x00,0xC0,0x30,0xC0,0x00,0xC0,0x30,0x00,0x01,0x06,0x01,0x00,0x01,0x06,0x01,0x00}),
            new FontCharacterDescriptor('x' ,FontHeightBytes,new byte[]{0x10,0x20,0xC0,0xC0,0x20,0x10,0x04,0x02,0x01,0x01,0x02,0x04}),
            new FontCharacterDescriptor('y' ,FontHeightBytes,new byte[]{0x30,0xC0,0x00,0x00,0x00,0xC0,0x30,0x20,0x20,0x13,0x0C,0x03,0x00,0x00}),
            new FontCharacterDescriptor('z' ,FontHeightBytes,new byte[]{0x10,0x90,0x50,0x30,0x06,0x05,0x04,0x04}),
            new FontCharacterDescriptor('{' ,FontHeightBytes,new byte[]{0x80,0x80,0x7C,0x02,0x02,0x00,0x00,0x1F,0x20,0x20}),
            new FontCharacterDescriptor('|' ,FontHeightBytes,new byte[]{0xFE,0x3F}),
            new FontCharacterDescriptor('}' ,FontHeightBytes,new byte[]{0x02,0x02,0x7C,0x80,0x80,0x20,0x20,0x1F,0x00,0x00}),
            new FontCharacterDescriptor('~' ,FontHeightBytes,new byte[]{0x0C,0x02,0x02,0x04,0x08,0x08,0x06,0x00,0x00,0x00,0x00,0x00,0x00,0x00}),
        };
    }
}
