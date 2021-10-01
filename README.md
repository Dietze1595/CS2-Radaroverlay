# **CSGOOverlay**

## Introduction

Hey, I'm Marcel Dietz, the developer of this FaceIt stream plug-in.
The plug-in shows your live matchstats, as well as those of your teammates.
Additionally the last FaceIt statistics of the last 20 games are displayed every 10 seconds.
The plugins layout is designed to be placed over the radar, which protects you from streamsniping. (You can choose the resoltution of your circle between 16:9 and 4:3).
If you want to get an impression, watch this [Youtubevideo](https://youtu.be/pTHZYc1WQpM). 

I have two Overlays, one to cover the radar, and 2. a simple information about the Faceitmatch.
You can chose these two Radars over the branch on Github.
The Radaroverlay is called Master, the information over the faceitmatch is called FaceitOverlay

![Picture](https://raw.githubusercontent.com/Dietze1595/Faceitplugin/master/public/picture/overview.PNG) 




## Install

* Clone the [project (Radar)](https://github.com/Dietze1595/Faceitplugin) or click the [Download Zip (Radar)](https://github.com/Dietze1595/Faceitplugin/archive/master.zip) button and extract the file
* copy-paste the file gamestate_integration_overlay.cfg in your csgo cfg folder 
> steamapps/common/Counter-Strike Globaloffensive/csgo/cfg/
* Install Node.JS [Nodejs.org](https://nodejs.org/en/download/)
  * Check the box in the installation, **NPM is included**
* open the config.json file and add your Faceit Bearer token
  * Open the Link [developers.faceit.com](https://developers.faceit.com/apps) and create an app
  * Go to the tab API KEYS and create a new **Client Side** token
  * Copy the token and implement the token to the config.json file
* open your commandwindow, an type the following lines
  * cd \where\you\extracted\the\zip\Faceitplugin
  * npm install
* Go to the following path: 
> \where\you\extracted\the\zip\Faceitplugin\Desktop
* Create a link of the start.bat file to your desktop (move the file while holding your right mousebutton)
 
## Start

* Doubleclick the start.bat file on your Desktop and leave it in the background open
* Open the following link on your webbrowser or implement the URL in your OBS screen (width: 1920 height: 1080)
> http://127.0.0.1:3001/169 or http://127.0.0.1:3001/43 for the resolution of your circle

