# Images
Images is a plugin and library that allows you to put images into SCPSL. You can put images into the intercom, hints, and broadcasts. If you want to put images into serverinfo, use [this converter](https://pint.cloud/img2txt).

## Installation
To install this plugin, download the [latest release](https://github.com/PintTheDragon/Images/releases/latest), then put Images.dll into your plugins folder and System.Drawing.dll into your dependencies folder (which is inside of your Plugins folder). You need to also have [Mono](https://www.mono-project.com/download/stable/) installed or else you will run into errors.

## Usage
To use Images, you have to first configure your images. Then, you can use them in any of the below commands, replacing the image name with the name of your image (the name you set it to in the config, not the file name).

## Limitations
Currently, the maximum uncompressed image size is 27x27 for hints and intercom, and around 7x7 for broadcasts. While images larger than 27x27 will be automatically compressed, images in broadcasts are more limited. If an image exceeds 27x27, it will be compressed so that it's small enough to function in hints and the intercom. This compression will cause it to lose quality, but will allow you to use larger images.

## Commands
Here are all of the commands that Images has:
* `iintercom <image name>` - `images.iintercom` - `Puts an image on the intercom`
* `ibc <duration> <image name>` - `images.ibc` - `Broadcasts an image to every player`
* `ihint <duration> <image name>` - `images.ihint` - `Shows a hint with an image to every player`

## Configuration
To add an image to Images, you need to first add it to the configuration. In your EXILED config file, find the `Images` section. To add images, you need to add your image under the `Images` config option. Here's what an image might look like:
```
- name: example2
  location: https://pint.cloud/img2txt/smallimg.png
  isURL: true
  scale: 26
  fps: auto
  precache: true
```
The first item (`name`) is the name you want the image to be set to. This will be the name that is used in your commands.

The second item (`location`) is where your image is located. This can be either a file path or a URL.

The third item (`isURL`) is whether or not your image's location is a URL. If it is, set this to `true`, otherwise `false`.

Optional: The fourth item (`scale`) is the scale of an image. You can set this manually and try to find what works for you, or you can set it to `auto` to have the plugin determine it for you.

Optional: The fifth item (`fps`) is the fps of the image. If your image is not animated, you don't need to worry about this.

Optional: The sixth item (`precache`) is whether or not to the image should be automatically converted and cached. Setting this to `true` can help increase performance for the first loop of an image (or if it is not animated, the load time).

Once you add your image to the config, you can use it in anywhere you can put an image into.