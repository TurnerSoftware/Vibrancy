using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TurnerSoftware.Vibrancy.Benchmarks;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
public class CtorVsInitBenchmark
{
	public Image<Rgb24>? TestImage = Image.Load<Rgb24>(@"resources/1-small.jpg");

	public readonly Palette Palette = new(new PaletteOptions(new[]
	{
		SwatchDefinition.DarkVibrant,
		SwatchDefinition.Vibrant,
		SwatchDefinition.LightVibrant,
		SwatchDefinition.DarkMuted,
		SwatchDefinition.Muted,
		SwatchDefinition.LightMuted
	}));
	public readonly Palette Palette2 = new(new PaletteOptions()
	{
		Definitions = new[]
		{
			SwatchDefinition.DarkVibrant,
			SwatchDefinition.Vibrant,
			SwatchDefinition.LightVibrant,
			SwatchDefinition.DarkMuted,
			SwatchDefinition.Muted,
			SwatchDefinition.LightMuted
		}
	});

	[Benchmark]
	public IReadOnlyList<Swatch> Fast() => Palette.GetSwatches(TestImage!);
	[Benchmark]
	public IReadOnlyList<Swatch> Slow() => Palette2.GetSwatches(TestImage!);
}
