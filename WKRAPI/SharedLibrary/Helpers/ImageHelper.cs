using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SharedLibrary.Models;

namespace SharedLibrary.Helpers;

#pragma warning disable CA1416 // Validate platform compatibility
public static class ImageHelper
{
    public static Bitmap ResizeToResolutionLimit(this Bitmap source, int maxSize) {
        int width, height;

        if(source.Width > source.Height) {
            width = maxSize;
            height = Convert.ToInt32(maxSize * ((float)source.Height / (float)source.Width));
        }
        else {
            width = Convert.ToInt32(maxSize * ((float)source.Width / (float)source.Height));
            height = maxSize;
        }

        Rectangle destRect = new Rectangle(0, 0, width, height);
        Bitmap result = new Bitmap(width, height);

        //WARNING Mutation
        result.SetResolution(source.HorizontalResolution, source.VerticalResolution);
        using(var graphics = Graphics.FromImage(result)) {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using(var wrapMode = new ImageAttributes()) {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return result;
    }

    public static void ReziseToResolutionLimit(this SixLabors.ImageSharp.Image source, int maxSize) {
        int width, height;

        if(source.Width > source.Height) {
            width = maxSize;
            height = Convert.ToInt32(maxSize * ((float)source.Height / (float)source.Width));
        }
        else {
            width = Convert.ToInt32(maxSize * ((float)source.Width / (float)source.Height));
            height = maxSize;
        }

        //WARNING Mutation
        source.Mutate(a => a.Resize(width, height));
    }

    public static byte[] ToByteArray(this Bitmap source) {
        byte[] result;
        using(var stream = new MemoryStream()) {
            source.Save(stream, ImageFormat.Jpeg);
            result = stream.ToArray();
        }
        return result;
    }

    private const int maxShorterSide = 2560;
    private const int medShorterSide = 1920;
    private const int lowShorterSide = 1280;

    public static bool IsLargeEnoughForCompression(FileCorrectionModel source) {
        var shorterSide = Math.Min(source.Height, source.Width);

        if(source.BytesPer100Pixel >= 40)
            return true;
        if(shorterSide > maxShorterSide)
            return true;

        if(maxShorterSide >= shorterSide && shorterSide >= lowShorterSide) {
            var minBp100 = DetermineMinBp100(shorterSide);

            if(source.BytesPer100Pixel >= minBp100)
                return true;
        }

        return false;
    }

    private static int DetermineMinBp100(int value) {
        double outputMin = 20;
        double outputMax = 40;

        // Scale the input value to the range of the output value using a linear equation
        double outputValue = outputMin + (outputMax - outputMin) * (maxShorterSide - value) / (maxShorterSide - lowShorterSide);

        return (int)outputValue;
    }

    private static int DetermineQuality(int shorterSide, bool isPng) {
        return isPng 
            ? DetermineQualityPng(shorterSide) 
            : DetermineQualityNonPng(shorterSide);
    }

    private static int DetermineQualityPng(int shorterSide) {
        // Linear interpolation between maxShorterSide (70) and lowShorterSide (90)
        double outputMin = 70;
        double outputMax = 90;

        // Scale the input value to the range of the output value using a linear equation
        double linearResult = outputMin + (outputMax - outputMin) * (ImageHelper.maxShorterSide - shorterSide) / (ImageHelper.maxShorterSide - ImageHelper.lowShorterSide);

        return Math.Clamp((int)linearResult, 70, 90);
    }

    private static int DetermineQualityNonPng(int shorterSide) {
        // Linear interpolation between medShorterSide (90) and lowShorterSide (95)
        double outputMin = 90;
        double outputMax = 95;

        // Scale the input value to the range of the output value using a linear equation
        double linearResult = outputMin + (outputMax - outputMin) * (ImageHelper.medShorterSide - shorterSide) / (ImageHelper.medShorterSide - ImageHelper.lowShorterSide);

        return Math.Clamp((int)linearResult, 90, 95);
    }

    private static (int, int) ClampHeightWidth(int height, int width, int target) {
        var shorterSide = Math.Min(height, width);
        var longerSide = Math.Max(height, width);

        float pct = (float)target / shorterSide;
        var newLongerSide = (int)(longerSide * pct);

        return (
            height < width ? target : newLongerSide,
            width < height ? target : newLongerSide
        );
    }

    public static CompressionCondition DetermineCompressionCondition(int height, int width, bool isPng, int? targetShorterSide = null) {
        var shorterSide = Math.Min(height, width);
        var longerSide = Math.Max(height, width);

        if(targetShorterSide.HasValue) {
            (int newHeight, int newWidth) = ClampHeightWidth(height, width, targetShorterSide.Value);

            return new CompressionCondition {
                Height = newHeight,
                Width = newWidth,
                Quality = DetermineQuality(targetShorterSide.Value, isPng),
            };
        }

        if(shorterSide > ImageHelper.maxShorterSide) {
            (int newHeight, int newWidth) = ClampHeightWidth(height, width, ImageHelper.maxShorterSide);

            return new CompressionCondition {
                Height = newHeight,
                Width = newWidth,
                Quality = DetermineQuality(shorterSide, isPng)
            };
        }
        else if(shorterSide >= ImageHelper.medShorterSide) {
            return new CompressionCondition {
                Height = height,
                Width = width,
                Quality = DetermineQuality(shorterSide, isPng)
            };
        }
        else {
            return new CompressionCondition {
                Height = height,
                Width = width,
                Quality = DetermineQuality(shorterSide, isPng)
            };
        }
    }

    public static float? DetermineUpscaleMultiplier(FileCorrectionModel source, int upscaleTarget) {
        var shorterSide = Math.Min(source.Height, source.Width);
        if(shorterSide >= upscaleTarget)
            return null;

        float multiplier = 1.5f;
        while(shorterSide * multiplier < upscaleTarget) {
            multiplier += 0.5f;
        }
        return Math.Min(4, multiplier);
    }
}