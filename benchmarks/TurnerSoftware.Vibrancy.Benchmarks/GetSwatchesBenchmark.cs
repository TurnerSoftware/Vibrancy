using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TurnerSoftware.Vibrancy.Benchmarks;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class GetSwatchesBenchmark
{
	public Image<Rgb24>? TestImage = Image.Load<Rgb24>("resources/1.jpg");

	//public IEnumerable<Image<Rgb24>> TestImages => Directory.GetFiles("resources").Select(path => Image.Load<Rgb24>(path)).ToArray();

	public readonly Palette Palette = new(new PaletteOptions(new[]
	{
		SwatchDefinition.DarkVibrant,
		SwatchDefinition.Vibrant,
		SwatchDefinition.LightVibrant,
		SwatchDefinition.DarkMuted,
		SwatchDefinition.Muted,
		SwatchDefinition.LightMuted
	}));

	[Benchmark]
	public IReadOnlyList<Swatch> GetSwatches() => Palette.GetSwatches(TestImage!);
}
