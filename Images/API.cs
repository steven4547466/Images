﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Exiled.API.Features;
using MEC;

namespace Images
{
    public static class API
    {
        private static Image GetBitmapFromURL(string url)
        {
            var ms = new MemoryStream();
            
            var stream = WebRequest.Create(url)?.GetResponse()?.GetResponseStream();
            if (stream == null) return null;
                
            stream.CopyTo(ms);
            ms.Position = 0;

            Image image = Image.FromStream(ms);
                
            stream.Flush();
            stream.Dispose();
                
            return image;
            }

        private static Image GetBitmapFromFile(string path)
        {
            if (!File.Exists(path)) return null;

            return Image.FromFile(path);
        }

        private static CoroutineHandle FileToText(string path, Action<string> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, float threshold = 0f)
        {
            var file = GetBitmapFromFile(path);
            if (file == null) return new CoroutineHandle();

            return BitmapToText(file, handle, scale, shapeCorrection, waitTime, threshold);
        }

        private static CoroutineHandle URLToText(string url, Action<string> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, float threshold = 0f)
        {
            var file = GetBitmapFromURL(url);
            if (file == null) return new CoroutineHandle();

            return BitmapToText(file, handle, scale, shapeCorrection, waitTime, threshold);
        }

        /// <summary>
        /// Converts an image from a File Path or URL to some text.
        /// </summary>
        /// <param name="loc">The File Path/URL of the image.</param>
        /// <param name="handle">An <see cref="Action<string>"/> that will be ran for each frame of the image.</param>
        /// <param name="isURL">Whether the location is a URL.</param>
        /// <param name="scale">The <see cref="float"/> that determines the scale. Leave at default to automatically calculate scale.</param>
        /// <param name="shapeCorrection">Whether the shape of the image should be automatically corrected.</param>
        /// <param name="waitTime">How long should be waited after every frame in an image.</param>
        /// <param name="threshold">Threshold for how similar images need to be combined. Set to 0 to disable compression.</param>
        public static CoroutineHandle LocationToText(string loc, Action<string> handle, bool isURL = false, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, float threshold = 0f)
        {
            if (isURL)
            {
                return URLToText(loc, handle, scale, shapeCorrection, waitTime, threshold);
            }
            else
            {
                return FileToText(loc, handle, scale, shapeCorrection, waitTime, threshold);
            }
        }

        /// <summary>
        /// Converts an image to some text.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> that will be converted to text.</param>
        /// <param name="handle">An <see cref="Action<string>"/> that will be ran for each frame of the image.</param>
        /// <param name="scale">The <see cref="float"/> that determines the scale. Leave at default to automatically calculate scale.</param>
        /// <param name="shapeCorrection">Whether or not the shape of the image should be automatically corrected.</param>
        /// <param name="waitTime">How long should be waited after every frame in an image.</param>
        /// <param name="threshold">Threshold for how similar images need to be combined. Set to 0 to disable compression.</param>
        public static CoroutineHandle BitmapToText(Image bitmap, Action<string> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, float threshold = 0f)
        {
            return Timing.RunCoroutine(_BitmapToText(bitmap, handle, scale, shapeCorrection, waitTime, threshold));
        }
        
        private static IEnumerator<float> _BitmapToText(Image image, Action<string> handle, float scale = 0f, bool shapeCorrection = true, float waitTime = .1f, float threshold = 0f)
        {
            if (image == null) yield break;

            if(threshold != 0f) threshold /= 10f;
            
            var size = 0f;

            var dim = new FrameDimension(image.FrameDimensionsList[0]);

            for (var index = 0; index < image.GetFrameCount(dim); index++)
            {
                image.SelectActiveFrame(dim, index);

                if (size == 0f)
                {
                    if(image.Size.Height * image.Size.Width > 3000) throw new Exception("The image was too large. Please use an image with less that 3,000 pixels.");
                    size = Convert.ToInt32(scale == 0f ? Math.Floor((-.47*(((image.Size.Width+image.Size.Height)/2 > 60 ? 45 : (image.Width+image.Height)/2)))+28.72) : scale);
                }

                Bitmap bitmap;
                if(shapeCorrection) bitmap = new Bitmap(image, new Size(Convert.ToInt32(image.Size.Width*(1+.03*size)), image.Size.Height));
                else bitmap = new Bitmap(image);
                
                var text = "<size=" + size + "%>";

                Color pastPixel = new Color();

                //I need to figure out how to use bitmap data, but GetPixel is fine for now
                for (var i = 0; i < bitmap.Height; i++)
                {
                    for (var j = 0; j < bitmap.Width; j++)
                    {
                        Color pixel = bitmap.GetPixel(j, i);

                        var colorString = "#" + pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2") + pixel.A.ToString("X2");
                        
                        if (!pixel.Equals(pastPixel))
                        {
                            if(threshold == 0f || (i == 0 && j == 0)) text += ((i == 0 && j == 0) ? "" : "</color>") + "<color=" + colorString + ">█";
                            else
                            {
                                var d = Math.Abs(pixel.GetHue() - pastPixel.GetHue());
                                var diff = d > 180 ? 360 - d : d;

                                if (diff > threshold) text += ((i == 0 && j == 0) ? "" : "</color>") + "<color=" + colorString + ">█";
                                else
                                {
                                    bitmap.SetPixel(j, i, pastPixel);
                                    pixel = pastPixel;
                                    
                                    text += "█";
                                }
                            }
                        }
                        else
                        {
                            text += "█";
                        }

                        pastPixel = pixel;

                        if (j == bitmap.Width - 1) text += "\\n";
                    }
                }

                if (!text.EndsWith("</color>\\n") && !text.EndsWith("</color>")) text += "</color>";
                
                text+="</size>";

                if (text.Length > 32768) throw new Exception("Output text is too large. Please use a smaller image.");

                handle(text);

                yield return Timing.WaitForSeconds(waitTime);
            }

            image.Dispose();
        }
    }
}