=-- Ps2D -=-=-=-=->

:: Reunite Your Sprites ::

You design and draw with Photoshop.
You animate and code with Unity.
But in the middle of these two awesome things is a pile of un-awesome chores.

Ps2D helps by reassembling your Photoshop layers into Unity sprites!

* pixel perfect layouts
* preserve your names, nested layers, and sort order
* import a layer, group, or everything
* many flexible options like anchoring, trimming, colliders, and prefabs



=-- how does it work? -=-=-=-=->

* use the Ps2D Photoshop plugin to make a layer map (*.ps2dmap.json)
* import that map into Unity
* point Ps2D at your images (it can even auto-detect them)
* click "Assemble!"



=-- tweak the bajeezus out of it -=-=-=-=->

* reads Unity 4.5/5.x spritesheets

* supports images in an asset folder (great for prototyping, bad for releases)

* supports 2D Toolkit sprite collections

* full C# source code included for the Unity plugin (no stinky DLLs)

* full javascript source code included for the Photoshop plugin

* use a prefab as a template for creating sprites
	- wanna use a different color tint?
	- automatically attach your own scripts to the imported sprites

* preserve the Photoshop layer nesting
	- a great headstart for rigging animations

* customize the anchor point (default middle)
	- handy for characters that stand on the ground

* customize the sorting layer

* assign a sorting order (or make it count up per sprite)
	- a good way to cheat depth in 2.5D games without
	  burning up all your sorting layers

* change the z position in addition to sort order
	- great for integrating a little bit of 3d in your 2d
	- eg. setup your background layers parallax depth

* change the pixels-to-units ratio
	- match this up with your sprite import settings

* change the Photoshop scale
	- when your PSD is @2x, but your sprites are @1x (common for mobile)

* target a single layer, a group, or everything
	- a good way to pull in a subset (for building modular dungeons)

* add an optional parent to each sprite
	- good option when you want to add extra game objects that aren't
	  in your layers like particle effects

* trim to fit the sprites
	- adjusts the anchor to be based on the sprites, not the document
	- useful for documents with many side-by-side characters

* create a collider for total area of import shapes



=-- requirements -=-=-=-=->

Photoshop CC 14.2.1+
- Generator plugin enabled (comes with Photoshop but disabled)

Unity 4.3+ or 5.0+
- Works on both Personal/Free and Pro versions
- All platforms are supported
- This is an editor-only tool, nothing from Ps2D actually ships with your build.

Optionally 2D Toolkit 2.5+
- disabled by default, read the section below for enabling it



=-- what it doesn't do -=-=-=-=->

* doesn't make spritesheets (it reads them)

	Options for this part of the pipeline:

	* TexturePacker Pro is a great 3rd party app that can do this
	* Unity 4.x Pro has sprite packing features
	* SpritePacker on the AssetStore (there may be more)
	* 2D Toolkit has a sprite packer built-in

* doesn't update sprites

	The sprites Ps2D generates for you don't stay connected to the layers.
	You can always regenerate if make changes to your PSD.  Live connections
	would bust any animations anyway, so I'm not sure how useful this feature
	would be.  Lemme know if you need this.


=-- installing -=-=-=-=->

0.  Install Ps2D from the Unity Asset Store.  If you're reading this,
	excellent work champion!  Proceed to step 1.

1.  Install Photoshop CC

	You want the latest version.  2014.0 as of this writing.  They just changed their
	version numbers again.  But anything 14.2.1+ is good.

2.  Enable Generator

	Edit -> Preferences -> Plugins...
	Check on [X] Enable Generator

3.  Close Photoshop

4.  Install the Ps2D plugin for Photoshop

	***
	IMPORTANT:  Due to the way the Unity Asset Store works, I am unable to
	include the Photoshop plugin.  You can grab your copy off the web site at:

	http://stevekellock.com/ps2d/ps2d-photoshop-plugin.zip

	***

	Download and uncompress the plugin.  Copy the new Ps2D folder to the
	Photoshop "Plug-ins/Generator" folder.

	(Windows 64 bit)
	C:\Program Files\Adobe\Adobe Photoshop CC (64 Bit)\Plug-ins\Generator

	(Windows 32 bit)
	C:\Program Files\Adobe\Adobe Photoshop CC\Plug-ins\Generator

	(OS X)
	/Applications/Adobe Photoshop CC/Plug-ins/Generator
	or
	/Applications/Adobe Photoshop CC 2014/Plug-ins/Generator

	*** HEADS UP! ***
	You may have to create the Generator sub-folder.
	*** /HEADS UP! ***

5.  Verify Install

	* Open Photoshop CC and create a blank document. Don't save.
	* Click File -> Generate.
	* Do you see sub menus called Image Assets and Ps2D Map?
		If yes, you're ready.
		If no, read on...
	* Close the menu down completely by clicking on the blank document.
	* Draw a happy face.
		Basically anything to burn 10 seconds.
		Yes, I'm serious.  I've seen it take 10 seconds to start the plugins.
	* Look again under File -> Generate.
		Are they there?
		If no, double check the last 3 steps.
		Are they there now?
		No?
	* Contact me.  I should be able to help.  Chances are, it's your fault and
	  you're a bad person.  Shame on you.  Kidding.


=-- 2D Toolkit Support -=-=-=-=->

2D Toolkit support is optional.  It is off by default.  Not everyone uses it
and I didn't want to release this as another Unity asset or charge people more
money for the same thing.

To turn it on, you need to add a scripting define symbol.

From the main menu, select:

Edit -> Project Settings -> Player

Under your current platform, open the "Other Settings" section.
In the "Configuration" / "Scripting Define Symbols" field, type:

PS2D_TK2D

This will enable 2D Toolkit support.  Give Unity a moment while it recompiles.

That's it!



=-- quick start -=-=-=-=->


In Photoshop

* Open your Photoshop document that has layers
* Create a Ps2D map file by choosing File > Generate > Ps2D Map
* <YourName>.ps2dmap.json has been created in the same directory as your PSD file.
* when you make changes to your layers in photoshop, you'll need to refresh this map.

Meanwhile... in Unity

* Switch over to Unity and import this file.
* Open the Ps2D window by choosing Window > Ps2D
* Select the first drop down and choose your map.
* Ps2D will try to guess where your textures are by looking:
	* in the current folder for similarily named PSD or PNGs with multiple sprites
	* in a subfolder called <YourName>-assets (this is the Adobe Generator default naming)
* You can pick your own spritesheet or folder by dragging from the Project inspector.
* Double check that Unity will import your graphics as Sprites and not Textures in the inspector (thx Jim!)
* Click the Assemble! button! (yes, the exclamation is required!)



=-- tips -=-=-=-=->

* save your .psd and .ps2dmap files directly into a unity folder for ease
	(downside is your source control will need to pull a little more weight)

* set your sprite pivots before importing

* certain Photoshop layer styles do not contribute to bounds
	- outside strokes and drop shadows will not add to geometry
	- use layer masks to fix the geometry

* i've seen instances of Adobe Generator not providing the proper dimension
	- it had to do with shape-only layer groups
	- this bug has something to do with sub-pixel values
	- i'm still trying to track this down, however, the fix is to a layer mask
	  on the group


=-- randomly asked questions -=-=-=-=->

	Q. Does it support GIMP or older Photoshop versions?
	A. No.  It relies on Generator which ships with Photoshop 14.2 (Sep '14)



=-- wish list -=-=-=-=->

	* support layer masks just added in the last Generator release
	* support DaikonForge
	* support nGUI



=-- change log -=-=-=-=->

1.2.0 | Mar 29th, 2015

- Support for Unity 5.0


1.1.1 | Jun 23rd, 2014

- Fix for loading spritesheets whose name don't match the map file.
- Fix for the new serialization warnings introduced in Unity 4.5.x.


1.1.0 | Mar 22nd, 2014

- 2D Toolkit support


1.0.1 | Mar 12th, 2014

- editor settings survive entering and leaving play mode


1.0.0 | Mar 11th, 2014

- initial release
