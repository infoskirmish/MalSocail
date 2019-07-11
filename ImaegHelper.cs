using System;
using System.Drawing;

namespace MalSocail
{
    public enum Channel
    {
        R,
        B,
        G,
        A,
        QT
    }


    /*
        The human eye is fairly good at seeing small differences in brightness over a relatively large area, but not so good at distinguishing the exact strength of a high frequency 
        (rapidly varying) brightness variation. This fact allows one to reduce the amount of information required by ignoring the high frequency components. This is done by simply 
        dividing each component in the frequency domain by a constant for that component, and then rounding to the nearest integer. This is the main lossy operation in the whole process. 
        As a result of this, it is typically the case that many of the higher frequency components are rounded to zero, and many of the rest become small positive or negative numbers.
        
        As human vision is also more sensitive to luminance than chrominance, further compression can be obtained by working in a non-RGB color space which separates the two (e.g., YCbCr), 
        and quantizing the channels separately.

        Wikipedia (https://en.wikipedia.org/wiki/Quantization_(image_processing)
     
     */

    public static class ImaegHelper
    {
        //A quantization matrix
        public static double[,] QT = new double[8, 8] {
                                        { 06, 04, 04, 06, 10, 16, 20, 24 },
                                        { 05, 05, 06, 08, 10, 23, 24, 22 },
                                        { 06, 05, 06, 10, 16, 23, 28, 22 },
                                        { 06, 07, 09, 12, 20, 35, 32, 25 },
                                        { 07, 09, 15, 22, 27, 44, 41, 31 },
                                        { 10, 14, 22, 26, 32, 42, 45, 37 },
                                        { 20, 26, 31, 35, 41, 48, 48, 40 },
                                        { 29, 37, 38, 39, 45, 40, 41, 40 }
                                    };

        //This method takes two 8x8 quantized matrices and prints a side by side comparison. Differnces are highlighted in red.
        public static void PrintGridComparison(double[,] grid1, double[,] grid2, string name1, string name2, Channel channel)
        {
            if (grid1.Length != grid2.Length)
                return;

            int width =  (Math.Sqrt(grid1.Length) % 1) == 0 ? (int)Math.Sqrt(grid1.Length) : 0;
            int height = (Math.Sqrt(grid1.Length) % 1) == 0 ? (int)Math.Sqrt(grid1.Length) : 0;

            string terminal = "\t " + new String('-', width*height + 9) + "\n";
            Console.Write(terminal);
            if (channel == Channel.R)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (channel == Channel.B)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (channel == Channel.G)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.Write("\t |Channel:  " + channel.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   " + name1 + "\t  |\t|Channel:  " + channel.ToString()+ " " + name2 + "\t |\n");
            Console.Write("\t |");
            for (int y = 0; y < height; y++)
                Console.Write(string.Format("{0,3} ", y));
            Console.Write("|\t|");
            for (int y = 0; y < height; y++)
                Console.Write(string.Format("{0,3} ", y));

            Console.Write("|\n");
            Console.Write(terminal);
            for (int y = 0; y < height; y++)
            {
                Console.Write("\t" + y + "|");
                for (int x = 0; x < width; x++)
                {
                    if(grid1[x,y] != grid2[x,y])
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write(string.Format("{0,3} ", grid1[x, y]));

                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Write("|\t|");
                for (int x = 0; x < width; x++)
                {
                    if (grid1[x, y] != grid2[x, y])
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.Write(string.Format("{0,3} ", grid2[x, y]));

                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Write("|" + y + "\n");
            }
            Console.Write(terminal);
        }

        // A reuseable method to highlight different values in red
        public static void ReadString(Bitmap image)
        {
           Color c = image.GetPixel(0, 0);
           Color c2 = image.GetPixel(1, 0);
            
            string str = Convert.ToChar(c.R).ToString() + Convert.ToChar(c.G).ToString() + Convert.ToChar(c.B).ToString() + Convert.ToChar(c2.R).ToString() + Convert.ToChar(c2.G).ToString() + Convert.ToChar(c2.B).ToString();
            Console.WriteLine(str);
        }

        //Extract hidden text from image
        public static string ExtractString(Bitmap image, int x=0,int y = 0)
        {
            string extractedText = "";

            int terminating = 0;
            int counter = 0;

            for (int i = y; i < image.Height; i++)
            {
                for (int j = x; j < image.Width; j++)
                {
                    if (counter % 15 == 0)
                    {
                        Color pixel = image.GetPixel(j, i);
                        int RValue = pixel.R;
                        int GValue = pixel.G;
                        int BValue = pixel.B;
                        if (RValue + GValue + BValue == 0)
                        {
                            terminating++;
                            if (terminating == 8)
                            {
                                return extractedText;
                            }
                        }
                        else
                        {

                            char r = (char)RValue;
                            char g = (char)GValue;
                            char b = (char)BValue;

                            extractedText += r.ToString();
                            extractedText += g.ToString();
                            extractedText += b.ToString();

                            terminating = 0;
                        }
                    }
                }
            }
            return "";
        }

        // Insert string into image
        public static Bitmap ImplantString(Bitmap image, string str, int x=0, int y=0)
        {
            int length = str.Length;
            int index = 0;
            int terminating = 0;
            int R = 0, G = 0, B = 0;
            int counter = 0;

            for (int i = y; i < image.Height; i++)
            {
                for (int j = x; j < image.Width; j++)
                {
                    if (index < length)
                    {
                        if (counter % 15 == 0)
                        {
                            Color pixel = image.GetPixel(j, i);

                            R = pixel.R;
                            G = pixel.G;
                            B = pixel.B;

                            R = index < length ? R = str[index++] : 0;
                            G = index < length ? G = str[index++] : 0;
                            B = index < length ? B = str[index++] : 0;

                            image.SetPixel(j, i, Color.FromArgb(R, G, B));
                        }
                    }

                    else
                    {
                        if (terminating == 8)
                        {
                            return image;

                        }else
                        {
                            image.SetPixel(j, i, Color.FromArgb(0, 0, 0));
                            terminating++;
                        }
                    }
                }
            }
            return image;
        }


        // Prints a single 8x8 Matrix
        public static void PrintChannel(double[,] grid, Channel channel)
        {
            int width = (Math.Sqrt(grid.Length) % 1) == 0 ? (int)Math.Sqrt(grid.Length) : 0;
            int height = (Math.Sqrt(grid.Length) % 1) == 0 ? (int)Math.Sqrt(grid.Length) : 0;

            string[] lines = new string[height];

            for (int y = 0; y < height; y++)
            {
                object[] obj = new object[width];
                for (int x = 0; x < width; x++)
                {
                    obj[x] = grid[x, y];
                }
                string line = String.Format("\t| [{0,3} {1,3} {2,3} {3,3} {4,3} {5,3} {6,3} {7,3}] |\n",obj);
                lines[y] = line;
            }

            string terminal = "\t" + new String('-', lines[0].Length -2) + "\n";
            Console.Write(terminal);
            if(channel == Channel.R)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (channel == Channel.B)
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (channel == Channel.G)
                Console.ForegroundColor = ConsoleColor.Green;

            Console.Write("\t| Channel: " + channel.ToString() + "\t\t\t    |\n");
            Console.ForegroundColor = ConsoleColor.White;

            foreach (string line in lines)
                Console.Write(line);
            Console.Write(terminal);
        }

        //Quantize a 8x8 grid
        public static double[,] Quantiz(double[,] grid)
        {
            int width = (Math.Sqrt(grid.Length) % 1) == 0 ? (int)Math.Sqrt(grid.Length) : 0;
            int height = (Math.Sqrt(grid.Length) % 1) == 0 ? (int)Math.Sqrt(grid.Length) : 0;

            double[,] block = new double[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    block[x, y] = Math.Round(grid[x, y] / QT[x, y]);
                }
            }
            return block;
        }

        // Calculate the discrete cosine transform (DCT) of a quantized 8x8 block
        public static double[,] GetIDCTBlock(double[,] grid)
        {
            int width = (Math.Sqrt(grid.Length) % 1) == 0 ? (int) Math.Sqrt(grid.Length) : 0;
            int height = (Math.Sqrt(grid.Length) % 1) == 0 ? (int)Math.Sqrt(grid.Length) : 0;

            double[,] block = new double[width,height];
            int cell = 0;
            double[] flatten = new double[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flatten[cell] = grid[x,y];
                    cell++;
                }
            }

            double[] vector = InverseTransform(flatten);
            cell = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    block[x,y] = flatten[cell];
                    cell++;
                }
            }
            return block;
        }

        // Get 8x8 block from image
        public static double[,] Get8by8(Bitmap image,Channel channel, int x = 0, int y = 0, int height = 8, int width = 8)
        {
            double[,] block = new double[width,height];

            for (int yi = 0; yi < height; yi++)
            {
                for (int xi = 0; xi < width; xi++)
                {
                    Color pixel = image.GetPixel((x + xi), (y + yi));
                    switch (channel)
                    {
                        case Channel.A:
                            block[xi, yi] = pixel.A;
                            break;
                        case Channel.B:
                            block[xi, yi] = pixel.B;
                            break;
                        case Channel.G:
                            block[xi, yi] = pixel.G;
                            break;
                        case Channel.R:
                            block[xi, yi] = pixel.R;
                            break;
                    }
                }
            }
            return block;
        }

        // Get the image's first 8x8 block
        public static void PrintFirstRowRBGValues(Bitmap image)
        {
            int width = 0;
            int height = 0;

            for (height = 0; height < 8; height++)
            {
                for (width = 0; width < 8; width++)
                {
                    Color pixel = image.GetPixel(width, height);
                   
                    Console.Write( String.Format("[{0,3} {1,3} {2,3} {3,3}]", pixel.R, pixel.B, pixel.G, pixel.A));
                }
                Console.Write("\n");
            }
            Console.WriteLine("\n\t"+image.PixelFormat.ToString());
            Console.WriteLine("\n\tWidth = " + width);
            Console.WriteLine("\tHeight = " + height);
        }

        // Preform and inverse transform
        public static double[] InverseTransform(double[] vector)
        {
            if (vector == null)
                throw new NullReferenceException();
            double[] result = new double[vector.Length];
            double factor = Math.PI / vector.Length;
            for (int i = 0; i < vector.Length; i++)
            {
                double sum = vector[0] / 2;
                for (int j = 1; j < vector.Length; j++)
                    sum += vector[j] * Math.Cos(j * (i + 0.5) * factor);
                result[i] = sum;
            }
            return result;
        }
        public static Bitmap CreateNonIndexedImage(Image src)
        {
            Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }

            return newBmp;
        }

        public static int reverseBits(int n)
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
