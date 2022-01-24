using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace TurnerSoftware.Vibrancy
{
	public readonly record struct Palette(PaletteOptions Options)
	{
		/// <summary>
		/// Processes each pixel in the image to identify swatches that match the <see cref="PaletteOptions.Definitions"/>.
		/// </summary>
		/// <remarks>
		/// Due to the per-pixel processing, it is recommended to resize the image beforehand.
		/// </remarks>
		/// <param name="image"></param>
		/// <returns></returns>
		public IReadOnlyList<Swatch> GetSwatches(Image<Rgb24> image)
		{
			var definitions = Options.Definitions;
			var swatches = new List<Swatch>(definitions.Length);
			for (var i = 0; i < definitions.Length; i++)
			{
				swatches.Add(new Swatch(definitions[i]));
			}

			for (var y = 0; y < image.Height; y++)
			{
				for (var x = 0; x < image.Width; x++)
				{
					var color = new SwatchColor(image[x, y]);
					foreach (var swatch in swatches)
					{
						swatch.TryAddColor(color, Options.MinimumColorDelta);
					}
				}
			}
			return swatches;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="Definitions"></param>
	/// <param name="MinimumColorDelta"></param>
	/// <param name="MaxColors">
	/// The input image is run through a quantizer to restrict the number of colours
	/// Max value allowed is 256.
	/// Defaults to 64.
	/// </param>
	/// <param name="MaxSize">
	/// Maximum size of the image in an individual dimension before it is processed.
	/// The higher the value, the higher the quality colours detected and the longer it takes to process.
	/// When max size is greater than the source image, no resizing is applied.
	/// Defaults to 100.
	/// </param>
	public readonly record struct PaletteOptions(SwatchDefinition[] Definitions, float MinimumColorDelta = 25f);

	public record class Swatch(SwatchDefinition Definition)
	{
		private readonly List<(float targetFit, SwatchColor color)> Colors = new();

		public int Count => Colors.Count;

		public bool TryAddColor(SwatchColor color, float minimumColorDelta)
		{
			if (!Definition.FitsGroup(color))
			{
				return false;
			}

			var newTargetFitDistance = Definition.DistanceToTargetFit(color);
			for (var i = 0; i < Colors.Count; i++)
			{
				var (existingTargetFitDistance, existingColor) = Colors[i];
				if (existingColor.GetDeltaE(color) < minimumColorDelta)
				{
					if (newTargetFitDistance < existingTargetFitDistance)
					{
						//We want to preference the color that is the closest match to the target swatch definition.
						Colors[i] = (newTargetFitDistance, color);
					}
					//Whether or not we actually replace the color, we say we added it.
					//We do this because the colors should be close enough perceptionally that it doesn't matter.
					return true;
				}
			}
			Colors.Add((newTargetFitDistance, color));
			return true;
		}

		public IEnumerable<SwatchColor> GetColors() => Colors.OrderBy(c => c.targetFit).Select(c => c.color);
	}

	public readonly record struct SwatchDefinition(string Name, float MinSaturation = 0, float MaxSaturation = 1, float TargetSaturation = -1, float MinValue = 0, float MaxValue = 1, float TargetValue = -1)
	{
		public static readonly SwatchDefinition DarkVibrant = new(nameof(DarkVibrant), MinSaturation: 0.55f, TargetSaturation: 1, MinValue: 0.1f, TargetValue: 0.3f, MaxValue: 0.64f);
		public static readonly SwatchDefinition Vibrant = new(nameof(Vibrant), MinSaturation: 0.55f, TargetSaturation: 1, MinValue: 0.65f, TargetValue: 1);
		public static readonly SwatchDefinition LightVibrant = new(nameof(LightVibrant), MinSaturation: 0.4f, MaxSaturation: 0.69f, MinValue: 0.85f, TargetValue: 1);

		public static readonly SwatchDefinition DarkMuted = new(nameof(DarkMuted), MinSaturation: 0.1f, TargetSaturation: 0.15f, MaxSaturation: 0.45f, MinValue: 0.1f, TargetValue: 0.3f, MaxValue: 0.64f);
		public static readonly SwatchDefinition Muted = new(nameof(Muted), MinSaturation: 0.1f, TargetSaturation: 0.15f, MaxSaturation: 0.45f, MinValue: 0.65f, TargetValue: 0.70f, MaxValue: 0.75f);
		public static readonly SwatchDefinition LightMuted = new(nameof(LightMuted), MinSaturation: 0.1f, TargetSaturation: 0.15f, MaxSaturation: 0.45f, MinValue: 0.75f, TargetValue: 1);

		public float DistanceToTargetFit(SwatchColor color)
		{
			var fit = 0f;
			var hsv = color.Hsv;

			if (TargetSaturation > -1)
			{
				fit += Math.Abs(hsv.S - TargetSaturation) * 1.2f;
			}

			if (TargetValue > -1)
			{
				fit += Math.Abs(hsv.V - TargetValue) * 0.8f;
			}

			return fit;
		}

		public bool FitsGroup(SwatchColor color)
		{
			var hsv = color.Hsv;
			return hsv.S >= MinSaturation && hsv.S <= MaxSaturation && hsv.V >= MinValue && hsv.V <= MaxValue;
		}
	}

	public readonly struct SwatchColor
	{
		private static readonly ColorSpaceConverter Converter = new();

		public readonly Rgb Rgb;
		public readonly Hsv Hsv;
		public readonly CieLab CieLab;

		public SwatchColor(Rgb rgb)
		{
			Rgb = rgb;
			Hsv = Converter.ToHsv(rgb);
			CieLab = Converter.ToCieLab(rgb);
		}

		//public Hsv Hsv => Converter.ToHsv(Rgb);
		//public CieLab CieLab => Converter.ToCieLab(Rgb);

		public float GetDeltaE(SwatchColor other)
		{
			var x = CieLab;
			var y = other.CieLab;

			var l = y.L - x.L;
			var a = y.A - x.A;
			var b = y.B - x.B;
			return (float)Math.Sqrt(
				l * l +
				a * a +
				b * b
			);
		}
	}
}