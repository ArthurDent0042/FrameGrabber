using CommandLine;
using System.Collections.Generic;

namespace FrameGrabber
{
	/// <summary>
	/// Define the commandline options that are available to the user
	/// </summary>
	public class Options
	{
		[Option('f', "file", Required = true, HelpText = "Source file to obtain screenshots from. Can specify multiple files with a comma separated list.")]
		public IEnumerable<string> Files { get; set; }

		[Option(Group = "Timestamp", HelpText = "Set the number of screenshots to generate at pseudo-random times.", Default = 10)]
		public int Auto { get; set; }

		[Option(Group = "Timestamp", HelpText = "Specify list of timestamps from which to generate screenshots. Times can be entered as seconds or at a specific time. Ex 1:03:22")]
		public IEnumerable<string> Time { get; set; }

		[Option(Group = "Timestamp", HelpText = "Specify a list of frames from which to generate screenshots.")]
		public IEnumerable<string> Frames { get; set; }

		[Option('o', "overlay", Required = false, HelpText = "Overlay the frame number the screenshot was taken from in the upper left corner of image.", Default = false)]
		public bool Overlay { get; set; }

		[Option('l', "label", Required = false, HelpText = "Text to display underneath the frame value. Used in conjunction with the --overlay flag")]
		public IEnumerable<string> Labels { get; set; }

		[Option("label2", Required = false, HelpText = "Text to display underneath the first label. Used in conjunction with the --overlay flag")]
		public IEnumerable<string> Labels2 { get; set; }

		[Option("ts", Required = false, HelpText = "Display the timestamp in the overlay")]
		public bool DisplayTimeStamp { get; set; }

		[Option("ft", Required = false, HelpText = "Display the frame type in the overlay")]
		public bool DisplayFrameType { get; set; }

		[Option("fc", Required = false, HelpText = "Specify the font color to be used for the overlay text.  Hex values are also valid. Ex: 28BDCF", Default = "White")]
		public string FontColor { get; set; }

		[Option("fs", Required = false, HelpText = "Specify the font size to be used for the overlay text.", Default = "15")]
		public string FontSize { get; set; }

		[Option('b', "box", Required = false, HelpText = "Used to specify the box color and opacity that is behind the text being written. Ex: --box black, 0.6 Hex values are also valid. Ex: 5BD6CB, 0.6")]
		public IEnumerable<string> BoxOptions { get; set; }

		[Option('x', "xoffset", Required = false, HelpText = "Specify the offset in pixels on the x-axis when writing an overlay")]
		public int xOffset { get; set; }

		[Option('y', "yoffset", Required = false, HelpText = "Specify the offset in pixels on the y-axis when writing an overlay")]
		public int yOffset { get; set; }

		//[Option('d', "debug", Required = false, HelpText = "Display the ffmpeg command used to generate an image")]
		//public bool debug { get; set; }
	}
}