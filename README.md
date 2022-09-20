<a name="readme-top"></a>
<br />
<h3 align="center">FrameGrabber</h3>

  <p align="center">
    A tool used to extract screenshots from a media file.
    <br />
<div align="center">
  <p align="center">
    <br />
    <a href="https://github.com/ArthurDent0042/FrameGrabber"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/ArthurDent0042/FrameGrabber/issues">Report Bug</a>
    ·
    <a href="https://github.com/ArthurDent0042/FrameGrabber/issues">Request Feature</a>
  </p>
</div>

### Getting Started
There are several options that can be given to the application.  The most basic use of the program is as follows:

```FrameGrabber.exe --file "c:\videos\video1.mkv" --time 189 309 779```

This will generate 3 .png files from video1.mkv at time intervals (in seconds) 189, 309, and 779. The resulting files will
be named 
* video1 189.png
* video1 309.png
* video1 779.png

The screenshots are saved in the same directory as the video they were taken from.

### Some more in-depth features
However, having to input specific timestamps is tedious, and not intuitive as you may not know the length of the video in seconds. 
This makes guessing which timestamps to use difficult.  For this situation, you can use the ```--auto option```.
```--auto 6``` would generate 6 screenshots at semi-random intervals within the video.  To do this, the video is broken up into
6 equal sections, and a screenshot is taken from a random spot within each section.  For the first screenshot, the minimum time
at which a screenshot can be taken from is 61 seconds, as the first 60 seconds is skipped.  ```--auto``` has a default value of 10

```FrameGrabber.exe --file "c:\videos\video1.mkv" --auto 6```

You can also queue up several videos:

```FrameGrabber.exe --file "c:\videos\video1.mkv","c:\videos\video2.mkv" --auto 6```

This will generate 6 screenshots from each video, using the same timestamps for each.  This can be useful if you are wanting 
to see how different encodes look when compared to each other.

```--frames```
The ```--frames``` option tells the application to generate screenshots at the exact frames you specify.  This option cannot
be used in conjunction with ```--auto``` or ```--time```

```FrameGrabber.exe --file "c:\videos\video1.mkv" --frames 500, 8302, 99356```

```--overlay```
The ```--overlay``` option tells the application to log the frame the screenshot was taken from to the upper left-hand corner of the
image.  It will look like: 

```Frame 9352 of 882351```

```--ft```
Display the Frame Type in the overlay

```--ts```
Display the Timestamp in the overlay

```--label```
The --label option is used to label the screenshot so that you can tell where it came from. This is useful when doing
compares between different encodes.  Note that if you have multiple files defined in the --file option, you must have
the same number of values in the --label option.

```FrameGrabber.exe --file "c:\videos\video1.mkv" --auto 6 -overlay --label "Encode Test #1"```

or

```FrameGrabber.exe --file "c:\videos\video1.mkv","c:\videos\video2.mkv" --auto 6 --overlay --label "Encode Test #1","Encode Test #2"```

```--label2```
Very similar to ```--label```. Used to print a second line of text that will appear below the first label

```FrameGrabber.exe --file "c:\videos\video1.mkv","c:\videos\video2.mkv" --auto 6 --overlay --label "Encode Test #1","Encode Test #2" --label2 "Super", "Awesome"```

```--fc```
The --fc option is used to specify the font color of the text that is being overlaid by the --overlay command.
Acceptable values for this are color names (black/white/blue,etc) or Hex Codes of colors (5BD6CB).
The default value for this is White.

```FrameGrabber.exe --file "c:\videos\video1.mkv" --auto 6 --overlay --fc 5BD6CB```

```--fs```
The --fs option is used to specify the font size of the text that is being overlaid by the --overlay command
The default value for this is 15
```FrameGrabber.exe --file "c:\videos\video1.mkv" --auto 6 --overlay --fs 22```

```--box```
The --box option is used to draw a box of color x with an opacity of y behind the text being written by the ```--overlay``` command
This is sometimes necesary because the color of the screenshot may be the same color as the text, making it very 
hard if not impossible to read.
This option takes 2 values, separated by a comma.  These values describe the color and the opacity of the box being drawn.
Once again, the color can be a normal color name, or a Hex color code
Values for opacity should be between 0.0 (completely transparant) and 1.0 (completely solid)

```FrameGrabber.exe --file "c:\videos\video1.mkv" --auto 6 --overlay --box black,0.6```

```-x``` Offset the position on the x-axis by a number of pixels when writing an overlay to the screenshot

```-y``` Offset the position on the y-axis by a number of pixels when writing an overlay to the screenshot

```FrameGrabber.exe -f "c:\videos\video1.mkv" --frames 23256, 37234, 103624, 132731, 178285 --box black, 0.6 --label "Test" -x 30 -y 33```

Some commands have a short version that can be used. Note that if the short version is used, only one dash is used.

```-f``` --filename

```-o``` --overlay

```-l``` --label

```-b``` --box

When processing multiple files, you can use a comma separated list, or separate by spaces
The use of the "=" sign is also optional.

```FrameGrabber.exe -f "C:\videos\video1.mkv" "C:\videos\video2.mkv"```

```FrameGrabber.exe -f="C:\videos\video1.mkv", "C:\videos\video2.mkv"```

Both of the above produce the same results

### Making it semi-automated
You may also make use of the ```appSettings.json``` file instead of sending all values via the commandline. This may be useful 
for setting custom font colors, or background color/opacity without needing to input them everytime on the commnandline.

To use this feature, set the "UseConfigInsteadOfCommandLine" to a value of True.
Note that you will still need to use the commandline to supply the files to be processed, but all other values will
be taken from the appSettings.json.  

Also note that if you are submitting multiple files to be processed, and are also
using the ```--overlay option```, if you choose to have a label displayed as part of the overlay, the number of labels
must be equal to the number of files being processed.  This is true whether you are using the appSettings.json or
doing everything from the commandline.

Please report any bugs or suggestions to @ArthurDent0042 on GitHub
