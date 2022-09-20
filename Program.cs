using CommandLine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace FrameGrabber
{
	class Program
	{
		public static IConfigurationRoot config = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile(path: "appSettings.json", optional: false, reloadOnChange: true)
		.Build();

		public static List<string> filenames = new List<string>();
		public static bool displayFrameNumber = false;
		public static List<string> labels = new List<string>();
		public static List<string> labels2 = new List<string>();
		public static double frameRate;
		public static int totalFrames;
		public static string fontSize;
		public static string fontColor;
		public static bool box;
		public static bool displayOverlay = false;
		public static string overlayText = string.Empty;
		public static string boxColor;
		public static string boxOpacity;
		public static bool useFrames;
		public static bool displayFrameType;
		public static bool displayTimeStamp;
		public static string displayType;
		public static int xOffset;
		public static int yOffset;
		//public static bool debug = false;

		public static List<string> files = new List<string>();
		public static int AutoGenerate { get; set; }
		public static List<double> timestamps = new List<double>();
		public static List<string> frames = new List<string>();

		static async Task Main(string[] args)
		{
			if (Convert.ToBoolean(config.GetSection("UseConfigInsteadOfCommandLine").Value))
			{
				// Read from config
				AutoGenerate = Convert.ToInt32(config.GetSection("autoGenerate").Value);
				displayOverlay = Convert.ToBoolean(config.GetSection("DisplayOverlay").Value);
				displayFrameNumber = Convert.ToBoolean(config.GetSection("DisplayFrameNumber").Value);
				frames = config.GetSection("CaptureSpecificFrames").Value.Split(',').ToList();
				fontSize = config.GetSection("fontSize").Value;
				fontColor = config.GetSection("fontColor").Value;
				boxColor = config.GetSection("boxColor").Value;
				boxOpacity = config.GetSection("boxOpacity").Value;
				displayTimeStamp = Convert.ToBoolean(config.GetSection("DisplayTimeStamp").Value);
				labels = config.GetSection("labels").Value.Split(',').ToList();
				labels2 = config.GetSection("labelsRow2").Value.Split(',').ToList();
				displayFrameType = Convert.ToBoolean(config.GetSection("DisplayFrameType").Value);
				xOffset = Convert.ToInt32(config.GetSection("x-Offset").Value);
				yOffset = Convert.ToInt32(config.GetSection("y-Offset").Value);
				//debug = Convert.ToBoolean(config.GetSection("Show_ffmpeg_command").Value);

				// Parse commandline for filename only
				Parser.Default.ParseArguments<Options>(args)
					.WithParsed(FileOnlyRunOptions)
					.WithNotParsed(HandleParserError);

				// If user specifies to grab frames, then ignore the AutoGenerate value
				if (frames[0].Length > 0 && frames.Count() > 0)
				{
					useFrames = true;
				}
			}
			else
			{
				// Parse commandline for all options
				Parser.Default.ParseArguments<Options>(args)
					.WithParsed(RunOptions)
					.WithNotParsed(HandleParserError);
			}

			// let the app know where ffmpeg lives
			FFmpeg.SetExecutablesPath(AppDomain.CurrentDomain.BaseDirectory + @"\ffmpeg\");

			int fileIndex = 0;
			foreach (string file in files)
			{
				try
				{
					if (AutoGenerate > 0 && timestamps.Count() == 0)
					{
						timestamps = CreateTimestamps(file);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error when attempting to generate timestamps. {ex}");
				}

				if (File.Exists(file))
				{
					Console.WriteLine($"Processing {file}");

					// if we are logging the frame & source of the screenshot as an overlay onto the image...
					//if (displayFrameNumber)
					//{
						// Get some MediaInfo
						Task<IMediaInfo> mediaInfo = FFmpeg.GetMediaInfo(file);
						IMediaInfo mi = mediaInfo.Result;
						frameRate = mi.VideoStreams.ToList()[0].Framerate;
						totalFrames = (int)((double)mi.VideoStreams.ToList()[0].Duration.TotalSeconds * frameRate);
					//}

					if (useFrames)
					{
						// If we are using frames, then disregard any default values for timestamps
						timestamps.Clear();
						foreach (string frame in frames)
						{
							timestamps.Add(Convert.ToDouble(1 / frameRate * Convert.ToInt32(frame) + 1 / frameRate));
						}
					}
					foreach (double timestamp in timestamps)
					{
						TimeSpan length = FFmpeg.GetMediaInfo(file).Result.Duration;

						if (timestamp <= length.TotalSeconds)
						{
							await GenerateScreenshot(file, timestamp, fileIndex);
						}
						else
						{
							Console.WriteLine($"Timestamp {TimeSpan.FromSeconds((int)timestamp)} is beyond the length of the media ({TimeSpan.FromSeconds(length.TotalSeconds)}). Skipping.");
						}
					}
				}
				else
				{
					Console.WriteLine($"Cannot find file {file}.");
				}
				fileIndex += 1;
			}
			Console.WriteLine("Done!");
		}

		/// <summary>
		/// Captures a screenshot from the specified file at the given timestamp
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="timestamp"></param>
		/// <param name="index"> Index of the label list to display on the overlay </param>
		/// <returns></returns>
		public static async Task GenerateScreenshot(string filename, double timestamp, int index)
		{
			try
			{
				string screenshotFilename = $"{Path.ChangeExtension(filename, null)} {(int)timestamp}.png";

				TimeSpan captureTime = TimeSpan.FromSeconds(timestamp);

				// check to see if file already exists. if it does, then smite it
				if (File.Exists(screenshotFilename))
				{
					File.Delete(screenshotFilename);
				}
				// create the screenshot at the specified timestamp
				IConversion conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(filename, screenshotFilename, captureTime);

				if (displayFrameType)
				{
					displayType = GetPictureType(filename, timestamp);
				}

				await conversion.Start();

				// add the Frame info to image
				if (File.Exists(screenshotFilename))
				{
					string tmpFilename = Path.GetDirectoryName(screenshotFilename) + @"\output.png";

					if (displayFrameNumber)
					{
						overlayText = $@"{Environment.NewLine}Frame {(int)((double)captureTime.TotalSeconds * frameRate)} of {totalFrames}";
					}

					if (displayFrameType)
					{
						overlayText += $"{Environment.NewLine}Frame Type {displayType}";
					}

					if (displayTimeStamp)
					{
						overlayText += $@"{Environment.NewLine}Captured at {captureTime.ToString().Replace(":", ".")}";
					}

					if (labels.Count() > 0 && labels[0].Length > 0)
					{
						overlayText += $"{Environment.NewLine}{labels[index].Trim()}";
					}

					if (labels2.Count() > 0 && labels2[0].Length > 0)
					{
						overlayText += $"{Environment.NewLine}{labels2[index].Trim()}";
					}

					string arguments;
					if (displayOverlay)
					{
						if (box)
						{
							arguments = $@"-i ""{screenshotFilename}"" -vf drawtext=text=""{overlayText}"":fontcolor={fontColor}:fontsize={fontSize}:x=10+{xOffset}:y=8+{yOffset}:box=1:boxcolor={boxColor}@{boxOpacity} ""{tmpFilename}""";
						}
						else
						{
							arguments = $@"-i ""{screenshotFilename}"" -vf drawtext=text=""{overlayText}"":fontcolor={fontColor}:fontsize={fontSize}:x=10+{xOffset}:y=8+{yOffset}: ""{tmpFilename}""";
						}
					}
					else
					{
						arguments = $@"-i ""{screenshotFilename}"" ""{tmpFilename}""";
					}
					//if (debug)
					//{
					//	Console.WriteLine("ffmpeg.exe " + arguments);
					//}
					await FFmpeg.Conversions.New().Start(arguments);

					File.Delete(screenshotFilename);
					File.Move($@"{tmpFilename}", screenshotFilename);
				}

				Console.WriteLine($"Screenshot at timestamp {captureTime} generated.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Encountered an error when generating screenshot: {ex}");
			}
		}

		/// <summary>
		/// Generate pseudo random timestamps based on the number of screenshots requested
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static List<double> CreateTimestamps(string filename)
		{
			try
			{
				// get the length of the media
				TimeSpan length = FFmpeg.GetMediaInfo(filename).Result.Duration;

				// divide length by the number of frames we want to grab
				int chunk = (int)length.TotalSeconds / AutoGenerate;

				for (int i = 0; i < AutoGenerate; i++)
				{
					int rInt;
					// get a random frame within each chunk
					Random r = new Random();
					if (i == 0 && chunk > 60)
					{
						// for the first frame grab, we're gonna skip at least 60 seconds in
						rInt = r.Next(60, chunk);
					}
					else
					{
						rInt = r.Next(chunk * i, chunk * (i + 1));
					}
					timestamps.Add(rInt);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Encountered an error when determining timestamps to grab screenshots. {ex}");
			}
			return timestamps;
		}

		/// <summary>
		/// If the config file is being used, only read the files to be processed from the commandline
		/// </summary>
		/// <param name="options"></param>
		static void FileOnlyRunOptions(Options options)
		{
			if (options.Files.Count() > 0)
			{
				foreach (string file in options.Files)
				{
					files.Add(file.Replace(",", " ").Replace("=", " "));
				}
			}
		}

		/// <summary>
		/// Process the commandline arguments
		/// </summary>
		/// <param name="options"></param>
		static void RunOptions(Options options)
		{
			if (options.Files.Count() > 0)
			{
				foreach (string file in options.Files)
				{
					if (File.Exists(file.Replace(",", " ")))
					{
						files.Add(file.Replace(",", " "));
					}
					else
					{
						Console.WriteLine($"Specified file {file.Replace(",", " ")} does not exist. Exiting");
					}
				}
			}

			if (options.Auto > 0 && options.Time.Count() == 0 && options.Frames.Count() == 0)
			{
				AutoGenerate = options.Auto;
			}

			//options.Time
			if (options.Time.Count() > 0)
			{
				if (options.Time.Count() == 1)
				{
					options.Time = options.Time.ToList()[0].Split(',').ToList();
				}
				foreach (string time in options.Time)
				{
					if (time.Contains(":"))
					{
						timestamps.Add(Convert.ToInt32(TimeSpan.Parse(time.Replace("=", " ").Replace(",","")).TotalSeconds));
					}
					else
					{
						timestamps.Add(Convert.ToInt32(time.Replace("=", " ")));
					}
				}
			}

			if (options.Frames.Count() > 0)
			{
				if (options.Frames.Count() == 1)
				{
					options.Frames = options.Frames.ToList()[0].Split(',').ToList();
				}
				foreach (string frame in options.Frames)
				{
					frames.Add(frame.Replace(",", ""));
				}
				useFrames = true;
			}

			if (options.Overlay)
			{
				displayFrameNumber = true;
				displayOverlay = true;

				if (options.FontColor.Length > 0)
				{
					fontColor = options.FontColor;
				}

				if (options.FontSize.Length > 0)
				{
					fontSize = options.FontSize;
				}

				if (options.BoxOptions.Count() > 0)
				{
					box = true;

					boxColor = options.BoxOptions.ToList()[0].Replace(",", " ");
					boxOpacity = options.BoxOptions.ToList()[1];
				}

				if (options.Labels.Count() > 0)
				{
					foreach (string item in options.Labels)
					{
						labels.Add(item.Replace(",", " "));
					}
				}
				if (options.Labels2.Count() > 0)
				{
					foreach (string item in options.Labels2)
					{
						labels2.Add(item.Replace(",", " "));
					}
				}
			}

			if (options.xOffset != 0)
			{
				xOffset = options.xOffset;
			}
			if (options.yOffset != 0)
			{
				yOffset = options.yOffset;
			}

			if (options.DisplayFrameType)
			{
				displayFrameType = true;
			}

			if (options.DisplayTimeStamp)
			{
				displayTimeStamp = true;
			}

			//if (options.debug)
			//{
			//	debug = true;
			//}
		}

		public static string GetPictureType(string input, double timestamp)
		{
			string ptype;
			string arguments = @$"-noaccurate_seek -ss {timestamp} -hide_banner -i {input} -filter:v ""select = eq('n,pict_type'),showinfo"" -vframes 1 -f null -";
			using (Process p = new Process())
			{
				p.StartInfo.FileName = Directory.GetCurrentDirectory() + @"\ffmpeg\ffmpeg.exe";
				p.StartInfo.Arguments = arguments;
				p.StartInfo.RedirectStandardError = true;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.UseShellExecute = false;
				p.Start();

				StreamReader ffmpegOutput = p.StandardError;
				p.WaitForExit(300);
				//if (p.HasExited)
				//{
				string hOutput = ffmpegOutput.ReadToEnd();
				int location = hOutput.IndexOf("type:");
				ptype = hOutput.Substring(location + 5, 2).Trim();
				return ptype;
			}
			//var conversionResult = await FFmpeg.Conversions.New().Start(arguments);
		}

		static void HandleParserError(IEnumerable<Error> errors)
		{
			foreach (Error error in errors)
			{
				Console.WriteLine(error.ToString());
			}
		}
	}
}