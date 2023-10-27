using Microsoft.Maui.ApplicationModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.Helpers
{
    public static class SteganographyHelper
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        public static void EmbedText(string imagePath, string outputPath, string textToEmbed)
        {
            using (SKBitmap bitmap = SKBitmap.Decode(imagePath))
            {

                // Initially, we'll be hiding characters in the image
                State state = State.Hiding;

                // Holds the index of the character that is being hidden
                int charIndex = 0;

                // Holds the value of the character converted to integer
                int charValue = 0;

                // Holds the index of the color element (R or G or B) that is currently being processed
                long pixelElementIndex = 0;

                // Holds the number of trailing zeros that have been added when finishing the process
                int zeros = 0;

                // Hold pixel elements
                int R = 0, G = 0, B = 0;

                // Pass through the rows
                for (int i = 0; i < bitmap.Height; i++)
                {
                    // Pass through each row
                    for (int j = 0; j < bitmap.Width; j++)
                    {
                        // Holds the pixel that is currently being processed
                        SKColor pixel = bitmap.GetPixel(j, i);

                        // Now, clear the least significant bit (LSB) from each pixel element
                        R = pixel.Red - pixel.Red % 2;
                        G = pixel.Green - pixel.Green % 2;
                        B = pixel.Blue - pixel.Blue % 2;

                        // For each pixel, pass through its elements (RGB)
                        for (int n = 0; n < 3; n++)
                        {
                            // Check if new 8 bits have been processed
                            if (pixelElementIndex % 8 == 0)
                            {
                                // Check if the whole process has finished
                                // We can say that it's finished when 8 zeros are added
                                if (state == State.Filling_With_Zeros && zeros == 8)
                                {
                                    // Apply the last pixel on the image
                                    // Even if only a part of its elements have been affected
                                    if ((pixelElementIndex - 1) % 3 < 2)
                                    {
                                        bitmap.SetPixel(j, i, new SKColor((byte)R, (byte)G, (byte)B));
                                    }

                                    using (FileStream stream = File.OpenWrite(outputPath))
                                    {
                                        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                                    }
                                    return;
                                }

                                // Check if all characters have been hidden
                                if (charIndex >= textToEmbed.Length)
                                {
                                    // Start adding zeros to mark the end of the text
                                    state = State.Filling_With_Zeros;
                                }
                                else
                                {
                                    // Move to the next character and process again
                                    charValue = textToEmbed[charIndex++];
                                }
                            }

                            // Check which pixel element has the turn to hide a bit in its LSB
                            switch (pixelElementIndex % 3)
                            {
                                case 0:
                                    {
                                        if (state == State.Hiding)
                                        {
                                            // The rightmost bit in the character will be (charValue % 2)
                                            // To put this value instead of the LSB of the pixel element
                                            // Just add it to it
                                            // Recall that the LSB of the pixel element had been cleared
                                            // Before this operation
                                            R += charValue % 2;

                                            // Removes the added rightmost bit of the character
                                            // Such that next time we can reach the next one
                                            charValue /= 2;
                                        }
                                    }
                                    break;
                                case 1:
                                    {
                                        if (state == State.Hiding)
                                        {
                                            G += charValue % 2;

                                            charValue /= 2;
                                        }
                                    }
                                    break;
                                case 2:
                                    {
                                        if (state == State.Hiding)
                                        {
                                            B += charValue % 2;

                                            charValue /= 2;
                                        }

                                        bitmap.SetPixel(j, i, new SKColor((byte)R, (byte)G, (byte)B));
                                    }
                                    break;
                            }

                            pixelElementIndex++;

                            if (state == State.Filling_With_Zeros)
                            {
                                // Increment the value of zeros until it is 8
                                zeros++;
                            }
                        }
                    }
                }

                using (FileStream stream = File.OpenWrite(outputPath))
                {
                    bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
                }
            }
        }

        public static string ExtractText(string imagePath)
        {
            using (SKBitmap bitmap = SKBitmap.Decode(imagePath))
            {
                int colorUnitIndex = 0;
                int charValue = 0;

                // Holds the text that will be extracted from the image
                string extractedText = String.Empty;

                // Pass through the rows
                for (int i = 0; i < bitmap.Height; i++)
                {
                    // Pass through each row
                    for (int j = 0; j < bitmap.Width; j++)
                    {
                        SKColor pixel = bitmap.GetPixel(j, i);

                        // For each pixel, pass through its elements (RGB)
                        for (int n = 0; n < 3; n++)
                        {
                            switch (colorUnitIndex % 3)
                            {
                                case 0:
                                    {
                                        // Get the LSB from the pixel element (will be pixel.Red % 2)
                                        // Then add one bit to the right of the current character
                                        // This can be done by (charValue = charValue * 2)
                                        // Replace the added bit (which value is by default 0) with
                                        // The LSB of the pixel element, simply by addition
                                        charValue = charValue * 2 + pixel.Red % 2;
                                    }
                                    break;
                                case 1:
                                    {
                                        charValue = charValue * 2 + pixel.Green % 2;
                                    }
                                    break;
                                case 2:
                                    {
                                        charValue = charValue * 2 + pixel.Blue % 2;
                                    }
                                    break;
                            }

                            colorUnitIndex++;

                            // If 8 bits have been added,
                            // Then add the current character to the result text
                            if (colorUnitIndex % 8 == 0)
                            {
                                // Reverse? Of course, since each time the process occurs
                                // On the right (for simplicity)
                                charValue = ReverseBits(charValue);

                                // Can only be 0 if it is the stop character (the 8 zeros)
                                if (charValue == 0)
                                {
                                    return extractedText;
                                }

                                // Convert the character value from int to char
                                char c = (char)charValue;

                                // Add the current character to the result text
                                extractedText += c.ToString();
                            }
                        }
                    }
                }

                return extractedText;
            }
        }

        public static int ReverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;
                n /= 2;
            }

            return result;
        }
    }
}