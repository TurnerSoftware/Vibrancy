using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Diagnostics;
using TurnerSoftware.Vibrancy;

var a = new[]
{
	SwatchDefinition.DarkVibrant,
	SwatchDefinition.Vibrant,
	SwatchDefinition.LightVibrant,
	SwatchDefinition.DarkMuted,
	SwatchDefinition.Muted,
	SwatchDefinition.LightMuted
};


//var o = new PaletteOptions(a);
var o = new PaletteOptions
{
	Definitions = a
};


var palette = new Palette
{
	Options = o
};

var stopwatch = new Stopwatch();
stopwatch.Start();
foreach (var file in Directory.GetFiles("images").Take(1))
{
	Console.Write(file);
	var inputImage = await Image.LoadAsync<Rgb24>(file);
	//await inputImage.SaveAsPngAsync($"tmp-{Path.GetFileName(file)}");

	var swatches = palette.GetSwatches(inputImage);

	//var outputImage = new Image<Rgba32>(swatches.Count, swatches.Max(s => s.Count));
	//outputImage.Mutate(x =>
	//{
	//	x.BackgroundColor(Color.Transparent);
	//});
	//for (var x = 0; x < swatches.Count; x++)
	//{
	//	var swatch = swatches[x];
	//	var colours = swatch.GetColors()
	//		.ToArray();
	//	for (var y = 0; y < colours.Length; y++)
	//	{
	//		outputImage[x, y] = colours[y].Rgb;
	//	}
	//}
	//outputImage.Mutate(x =>
	//{
	//	x.Resize(swatches.Count * 20, 0, new NearestNeighborResampler());
	//});
	//await outputImage.SaveAsPngAsync($"output-{Path.GetFileName(file)}");
	Console.WriteLine($" ({stopwatch.Elapsed.TotalMilliseconds}ms)");
	stopwatch.Restart();
}