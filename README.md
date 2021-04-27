# **CSGOOverlay**

## Introduction

Hey, I'm Marcel Dietz, the developer of this FaceIt stream plug-in.
The plug-in shows your live matchstats, as well as those of your teammates.
Additionally the last FaceIt statistics of the last 20 games are displayed every 10 seconds.
The plugins layout is designed to be placed over the radar, which protects you from streamsniping. (You can choose the resoltution of your circle between 16:9 and 4:3).
If you want to get an impression, watch this [Youtubevideo](https://youtu.be/pTHZYc1WQpM). 

The plugin runs on your system, but can also be run on my server, to find no excuse that you can't hit anything with the plugin.
If you are interested, please contact me over [Steam](https://steamcommunity.com/id/dietze_). 

If you want to support me, you can send me something via [Paypal](http://paypal.me/DietzMarcel) or a 
[Tradeoffers](http://steamcommunity.com/tradeoffer/new/?partner=296799755&token=W4Bv5kSS) via Steam

I have two Overlays, one to cover the radar, and 2. a simple information about the Faceitmatch.
You can chose these two Radars over the branch on Github.
The Radaroverlay is called Master, the information over the faceitmatch is called FaceitOverlay

![Picture](https://raw.githubusercontent.com/Dietze1595/Faceitplugin/master/public/picture/overview.PNG) 
![Picture](https://raw.githubusercontent.com/Dietze1595/Faceitplugin/FaceitOverlay/public/picture/picture.PNG)




## Install

* Clone the [project (Radar)](https://github.com/Dietze1595/Faceitplugin), [FaceitOverlay](https://github.com/Dietze1595/Faceitplugin/tree/FaceitOverlay) , otherwise click the [Download Zip (Radar)](https://github.com/Dietze1595/Faceitplugin/archive/master.zip), [FaceitOverlayy](https://github.com/Dietze1595/Faceitplugin/archive/FaceitOverlay.zip) button and extract the file
* copy-paste the file gamestate_integration_overlay.cfg in your csgo cfg folder 
> steamapps/common/Counter-Strike Globaloffensive/csgo/cfg/
* Install Node.JS [Nodejs.org](https://nodejs.org/en/download/)
  * Check the box in the installation, **NPM is included**
* open the start.js file and add your Faceit Bearer token
  * Open the Link [developers.faceit.com](https://developers.faceit.com/apps) and create an app
  * Go to the tab API KEYS and create a new **Client Side** token
  * Copy the token and implement the token to the start.js file under line 1
  * It should look like this
> var Bearertoken="12345678-abcdefghi-9876-54321",
* open your commandwindow, an type the following lines
  * cd \where\you\extracted\the\zip\Faceitplugin
  * npm install axios
  * npm install express
  * npm install socket.io
* Go to the following path: 
> \where\you\extracted\the\zip\Faceitplugin\Desktop
* Make a shortcut of the start.bat file to your desktop
 
## Start

* Doubleclick the start.bat file on your Desktop and leave it in the background open
* Open the following link on your webbrowser or implement the URL in your OBS screen
> http://127.0.0.1:3001/169 or http://127.0.0.1:3001/43 for the resolution of your circle

