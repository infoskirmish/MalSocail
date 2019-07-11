using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace MalSocail
{
    class Program
    {
        private static readonly string KEY                      = "password";                                          //  Password to encrypt data with.
        private static readonly string PathOriginalImage        = @"C:\Users\Public\clean.gif";                        //  Location of clean image with no encoded data.
        private static readonly string TweetImageURL            = "https://pbs.twimg.com/media/D_M5VvWVAAAYIEx.png";   //  The URL of the tweet /w image
        private static readonly string text                     = "I am a string hidden, and I came from an image";    //  The string to encrypt and hide inside an dimage.
        private static readonly string TweetImageExfiledFile    = @"C:\Users\Public\secret_bannercopy.docx";           //  The file to save the extracted binary data.
        private static readonly string TweetImageFile           = @"C:\Users\Public\banner_stuffed.gif";               //  The filename to save the download Twitter image to.
        private static readonly string ImageWithHiddenString    = @"C:\Users\Public\string_stuffed.gif";               //  Image which contains a hidden encrypted string.
        private static readonly string BinaryDataToHide         = @"C:\Users\Public\secret.docx";                      //  The binary file we wish to encrypt and stuff into an image.
        private static readonly string ExfiledBinaryData        = @"C:\Users\Public\secret_copy.docx";                 //  The file/location where we will store the decrypted binary data
        private static readonly string ImageWithDatafile        = @"C:\Users\Public\data_stuffed.gif";                 //  The image we will create which contains the encrypted binary data

    static void Main(string[] args)
        {


            Console.WriteLine("------------------[  End Testing Text String Exfil  ]------------------\n");

            Bitmap image = GetImage(@"C:\Users\Public\clean.gif");                          //  Get clean image
            string ciphertext = EncryptString(text, KEY);                                   //  Encrypt string/message we want to hide
            Bitmap stuffed = ImaegHelper.ImplantString(image, ciphertext);                  //  Encode encrypted string into the image
            SaveImage(stuffed, ImageWithHiddenString);                                      //  Save the newly created image with hidden string
            Bitmap image_withHiddenString = GetImage(ImageWithHiddenString);                //  Get a fresh copy of the image we just saved
            string extractedciphertext = ImaegHelper.ExtractString(image_withHiddenString); //  Extract string from encoded image file
            Compare(ciphertext, extractedciphertext, "Encrypted strings");                  //  Compare the extracted encrypted string with the string
                                                                                            //  we encrypted above.
            string cleartext = DecryptString(extractedciphertext);                          //  Decypt encrypted string
            Compare(text, cleartext, "Clear text strings");                                 //  Compare the decrypted string with the orginal string

            Console.WriteLine("------------------[  End Testing Text String Exfil  ]------------------\n");



            Console.WriteLine("------------------[ Start Create Image with Encoded Binary File ]------------------\n");

            Bitmap file_image      = GetImage(PathOriginalImage);                           //  Get clean image 
            string file_data       = ReadDataFile(BinaryDataToHide);                        //  Get data file to hide
            string file_ciphertext = EncryptString(file_data, KEY);                         //  Encrypt file data and return a base64encoded string
            Bitmap file_stuffed    = ImaegHelper.ImplantString(file_image, file_ciphertext);//  Insert encrypted data into the clean image
            SaveImage(file_stuffed, ImageWithDatafile);                                     //  Save the newly created image
            Bitmap image_withHiddenData = GetImage(ImageWithDatafile);                      //  Get a fresh copy of the image we just saved
            string fileimage_ciphertext = ImaegHelper.ExtractString(image_withHiddenData);  //  Extract hidden file data from image
            Compare(file_ciphertext, fileimage_ciphertext, "Encrypted strings");            //  For testing, compare the extracted text downloaded from Twitter to 
                                                                                            //  what is expected based on the data string we encrypted above.
            string file_exfildata = DecryptString(fileimage_ciphertext);                    //  Decrypt the exfiled binary data.
            Compare(file_data, file_exfildata, "Base64Encoded strings");                    //  For testing, compare the decrypted base64 exfiled data string to the base64
                                                                                            //  we encoded above.
            WriteDataFile(file_exfildata, ExfiledBinaryData);                               //  Save the decrypted, exfiled binary data to a file.

            Console.WriteLine("------------------[  End Binary File Exfil From Image   ]------------------\n");



            Console.WriteLine("------------------[ Start Get Banner From Twitter and Decode Data ]------------------\n");

            Bitmap banner_image = GetBannerImage(TweetImageURL);                        //  Download the image from Twitter
            SaveImage(banner_image, TweetImageFile);                                    //  Save the downloaded image
            Bitmap image_fromTwitter = GetImage(TweetImageFile);                        //  Get a fresh copy of the image we just saved
            string banner_ciphertext = ImaegHelper.ExtractString(image_fromTwitter);    //  Extract the encrypted data
            string banner_exfildata = DecryptString(banner_ciphertext);                 //  Decrypt the extracted data
            Compare(file_data, banner_exfildata, "Base64Encoded strings");              //  Compare the decrypted data with the orginal data from above
            WriteDataFile(banner_exfildata, TweetImageExfiledFile);                     //  Save the decrypte data to a file.
            Print8by8ComparisonBlock(PathOriginalImage, TweetImageFile);                //  Print some pixel by pixel diagnostic data.

            Console.WriteLine("\n------------------[  End Get Banner From Twitter and Decode Data  ]------------------\n");

        }
        private static void Compare(byte[] str1, byte[] str2, string title)
        {
            Compare(Convert.ToBase64String(str1), Convert.ToBase64String(str2), title);
        }

        private static void Compare(string str1, string str2, string title)
        {
            if (str1 == str2)
            {
                Console.WriteLine("\t" + title + "\tMATCH!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t" + title + "\tDO NOT MATCH!");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("String 1 = \t" + str1.Substring(0, Math.Min(str1.Length, 100)) + " \nString 2 = \t" + str2.Substring(0, Math.Min(str2.Length, 100)));
            }
        }

        private static void Print8by8ComparisonBlock(string image1path, string image2path)
        {
            Bitmap image1 = GetImage(image1path);
            Bitmap image2 = GetImage(image2path);

            ImaegHelper.PrintFirstRowRBGValues(image1);
            ImaegHelper.PrintFirstRowRBGValues(image2);

            var image1_Rblock = ImaegHelper.Get8by8(image1, Channel.R, 8, 8);
            var image1_Rdct   = ImaegHelper.GetIDCTBlock(image1_Rblock);
            var image1_RQ     = ImaegHelper.Quantiz(image1_Rdct);

            var image1_Bblock = ImaegHelper.Get8by8(image1, Channel.B, 8, 8);
            var image1_Bdct   = ImaegHelper.GetIDCTBlock(image1_Bblock);
            var image1_BQ     = ImaegHelper.Quantiz(image1_Bdct);

            var image1_Gblock = ImaegHelper.Get8by8(image1, Channel.G, 8, 8);
            var image1_Gdct   = ImaegHelper.GetIDCTBlock(image1_Gblock);
            var image1_GQ     = ImaegHelper.Quantiz(image1_Gdct);

            var image2_Rblock = ImaegHelper.Get8by8(image2, Channel.R, 8, 8);
            var image2_Rdct   = ImaegHelper.GetIDCTBlock(image2_Rblock);
            var image2_RQ     = ImaegHelper.Quantiz(image2_Rdct);

            var image2_Bblock = ImaegHelper.Get8by8(image2, Channel.B, 8, 8);
            var image2_Bdct   = ImaegHelper.GetIDCTBlock(image2_Bblock);
            var image2_BQ     = ImaegHelper.Quantiz(image2_Bdct);

            var image2_Gblock = ImaegHelper.Get8by8(image2, Channel.G, 8, 8);
            var image2_Gdct   = ImaegHelper.GetIDCTBlock(image2_Gblock);
            var image2_GQ     = ImaegHelper.Quantiz(image2_Gdct);

            ImaegHelper.PrintGridComparison(image1_Rblock, image2_Rblock, "Image 1R",     "Image 2R",     Channel.R);
            ImaegHelper.PrintGridComparison(image1_Rdct, image2_Rdct,     "Image1 RDCT",  "Image2 RDCT",  Channel.R);
            ImaegHelper.PrintGridComparison(image1_RQ, image2_RQ,         "Image1 RQ",    "Image2 RQ",    Channel.R);
            ImaegHelper.PrintGridComparison(image1_Bblock, image2_Bblock, "Image 1B",     "Image 2B",     Channel.B);
            ImaegHelper.PrintGridComparison(image1_Bdct, image2_Bdct,     "Image1 BDCT",  "Image2 BDCT",  Channel.B);
            ImaegHelper.PrintGridComparison(image1_BQ, image2_BQ,         "Image1 BQ",    "Image2 BQ",    Channel.B);
            ImaegHelper.PrintGridComparison(image1_Gblock, image2_Gblock, "Image 1G",     "Image 2G",     Channel.G);
            ImaegHelper.PrintGridComparison(image1_Gdct, image2_Gdct,     "Image1 GDCT",  "Image2 GDCT",  Channel.G);
            ImaegHelper.PrintGridComparison(image1_GQ, image2_GQ,         "Image1 GQ",    "Image2 GQ",    Channel.G);
        }

        private static void WriteDataFile(byte[] encodedcontents, string topath)
        {
            File.WriteAllBytes(topath, encodedcontents);
        }

        private static void WriteDataFile(string encodedcontents,string topath)
        {
            byte[] bytes = Convert.FromBase64String(encodedcontents);
            File.WriteAllBytes(topath, bytes);
        }

        private static string ReadDataFile(string filepath)
        {
            byte[] bytes = File.ReadAllBytes(filepath);
            string bytesencoded = Convert.ToBase64String(bytes);
            return bytesencoded;
        }

        private static void SaveImage(Bitmap image, string path)
        {
            image.Save(path);
        }

        private static Bitmap GetImage(string path)
        {
            Bitmap originalImage = ImaegHelper.CreateNonIndexedImage(Image.FromFile(path));
            return originalImage;
        }

        private static string EncryptString(string plaintext, string key)
        {
            string encryptedData = Cipher.Encrypt(plaintext, key);
            return encryptedData;
        }

        private static string DecryptString(string ciphertext)
        {
            string decryptedData = Cipher.Decrypt(ciphertext, KEY);
            return decryptedData;
        }

        public static Bitmap GetBannerImage(string url)
        {
            using (WebClient client = new WebClient())
            {
                using (Stream stream = client.OpenRead(url))
                {
                    Bitmap bitmap; bitmap = new Bitmap(stream);

                    if (bitmap != null)
                        return bitmap;
                    return null;
                }
            }
        }
    }
}
