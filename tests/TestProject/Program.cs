using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Diagnostics;
using TurnerSoftware.Vibrancy;

var palette = new Palette(new PaletteOptions(new[]
{
	SwatchDefinition.DarkVibrant,
	SwatchDefinition.Vibrant,
	SwatchDefinition.LightVibrant,
	SwatchDefinition.DarkMuted,
	SwatchDefinition.Muted,
	SwatchDefinition.LightMuted
}));

var stopwatch = new Stopwatch();
foreach (var file in Directory.GetFiles("images"))
{
	Console.Write(file);
	var inputImage = await Image.LoadAsync<Rgb24>(file);
	await inputImage.SaveAsPngAsync($"tmp-{Path.GetFileName(file)}");

	stopwatch.Restart();
	var swatches = palette.GetSwatches(inputImage);
	Console.WriteLine($" ({stopwatch.Elapsed.TotalMilliseconds}ms)");

	var outputImage = new Image<Rgba32>(swatches.Count, swatches.Max(s => s.Count));
	outputImage.Mutate(x =>
	{
		x.BackgroundColor(Color.Transparent);
	});
	for (var x = 0; x < swatches.Count; x++)
	{
		var swatch = swatches[x];
		var colours = swatch.GetColors()
			.OrderByDescending(c => (int)(c.Hsv.V * 3))
			.ThenByDescending(c => (int)(c.Hsv.S * 3))
			.ThenBy(c => c.Hsv.H)
			.ToArray();
		for (var y = 0; y < colours.Length; y++)
		{
			outputImage[x, y] = colours[y].Rgb;
		}
	}
	outputImage.Mutate(x =>
	{
		x.Resize(swatches.Count * 20, 0, new NearestNeighborResampler());
	});
	await outputImage.SaveAsPngAsync($"output-{Path.GetFileName(file)}");
}