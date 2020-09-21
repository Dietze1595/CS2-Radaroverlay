# **CSGOOverlay**

## Introduction

Hey, I'm Marcel Dietz, the developer of this FaceIt stream plug-in.
The plug-in shows your live matchstats, as well as those of your teammates.

The plugin runs on your system.
If you are interested, please contact me over [Steam](https://steamcommunity.com/id/dietze_). 

If you want to support me, you can send me something via [Paypal](http://paypal.me/DietzMarcel) or a 
[Tradeoffers](http://steamcommunity.com/tradeoffer/new/?partner=296799755&token=W4Bv5kSS) via Steam

![Picture](https://raw.githubusercontent.com/Dietze1595/Faceitplugin/FaceitOverlay/public/picture/picture.PNG)




## Install

* Clone the [project](https://github.com/Dietze1595/Faceitplugin), otherwise click the [Download Zip](https://github.com/Dietze1595/Faceitplugin/archive/FaceitOverlay.zip) button and extract the file
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
  * npm install
  * npm install express
  * npm install axios
  * npm install socket.io
* Go to the following path: 
> \where\you\extracted\the\zip\Faceitplugin\Desktop
* Make a shortcut of the start.bat file to your desktop
 
## Start

* Doubleclick the start.bat file on your Desktop and leave it in the background open
* Open the following link on your webbrowser or implement the URL in your OBS screen
> http://127.0.0.1:3001/Overlay for the resolution of your circle

