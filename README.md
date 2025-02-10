![gif](https://github.com/mistwisp/C-Animated-Gif-WPF/blob/main/example.gif)

# C-Animated-Gif-WPF
Class to use animated gifs, of any dimensions and sizes, on a C# WPF application

# Memory usage at idle
![memory](https://i.imgur.com/jg3tsvu.png)

(in this example, the .gif file has 90+MB in size)

# How to implement
1. Include the AnimatedGif.cs file in your project
2. inside the ` <window>` tag of any .xaml file you want to display GIFs, you should add:
```xmlns:local="clr-namespace:AnimatedGifWPF"```
3. to display a GIF, inside the grid of your xaml, you put:
```<local:AnimatedGif x:Name="image" />```
where you want your GIF to be, it supports every propriety of a regular `<image>` tag
4. Inside the .xaml.cs of any window you want to display your GIFs, you set the source at the constructor, or inside any method you see fit, with:
```image.SetGifSource(new Uri("pack://application:,,,/MyApplication;component/Images/image.gif"));```
5. Don't forget to use
```using AnimatedGifWPF```
