                     Magic Lightmap Switcher

  What is it?
  -----------

  Magic Lightmap Switcher is a Unity editor extension that will allow you 
  to store baked lightmap data and then switch between its instantly or interpolate smoothly. 
  
  The following data is involved in switching and blending:
    * Lightmaps
    * Lightprobes
    * Reflection probes
    * Light sources settings
    * Skybox texture
    * Scene fog settings
    * Any custom data such as post-processing settings, Volume component settings in SRP, 
      or variable values of any of your script
  
  System features:
    * Switching and blending all available baked data in the scene at runtime
    * Using shaders and multithreading for system operation, which allows you to maintain high performance
    * Support for multiple lighting scenarios for one scene
    * Automatic baking and storing process of lightmaps in turn order

  The Latest Version
  ------------------
    
  The latest version is always available in the asset store. Interim updates 
  not published in the asset store can be obtained by contacting the developer. 
  
  Documentation
  -------------
    
  Up-to-date documentation is located at
  https://motiongamesstudio.gitbook.io/magic-lightmap-switcher/

  Additional Information
  -------------

  About error "Failed to reserve memory for scene-based lightmaps"
  https://forum.unity.com/threads/lightmaps-always-bake-but-unity-says-its-out-of-memory-to-store-them.644632/

  CHANGELOG
  
  v 1.1.1
  -------------
        
  Bug Fixes:   
    - Core: Shadow mask does not work in URP and HDRP    
    - Custom Blendable: When changing the order of the blending queue, the skybox textures are displayed incorrectly     
  
  v 1.1.0
  -------------
      
  Bug Fixes:   
      - Preset Manager: Fixed an error due to which the "_Tint" 
        property of the skybox could not be assigned correctly.
      - Preset Manager: Fixed a bug where the Shadows Type 
        property of the light source was reset to None. 
      
  Improvements:
      - Core: Added support for Texture2DArrays and TextureCubeArrays, 
        which significantly saves FPS.
      - Lighting Scenario: Added options to disable mixing cycle modules.
      - Core: Added support for Bakery Volumes 
        (operation mode switches between Lightprobes and Bakery Volumes).
  
  v 1.0.0
  -------------
    
  Bug Fixes:   
    - Lighting Scenario: Fixed Events serialization errors.
    
  Improvements:
     - Core: Bakery RNM + SH for SRP added.
     - Core: Added support for Bakery Shader Tweaks.
     - Core: Added support for Deferred mode in HDRP.
  
  v 0.94.5b
  -------------
  
  Bug Fixes:   
     - Core: Lighting Scenario events are reset after editor restart.
  
  Improvements:
     - Lightmap Data Storing: Data is now saved in a folder based on 
       the location of the scene or in a specified custom folder.

  v 0.94.4b
  -------------

  Improvements:
    - SRP 12 support added. (URP + HDRP)

  v 0.94.3b
  -------------

  Bug Fixes:   
    - Custom Blendable: Minor fixes.
    - Preset Manager: Game Objects settings reset during baking with Bakery.
    - Lightmap Data Storing: Fixed a bug due to which Terrain data was saved with errors
    - Lighting Scenario: Fixed a bug that caused Events to work incorrectly

  Improvements:
    - Preset Manager: Added support for saving Environment Lighting settings.
    - Preset Manager: Added options to save light source shadow type (None / Hard / Soft) and Baked Shadow Angle.

  v 0.94.2b
  -------------

  Bug Fixes:   
    - Fixed a bug due to which, immediately after importing a package, an error 
      occurred when determining the SRP version.
    - Fixed a bug due to which presets for Bakery Light Mesh worked incorrectly.

  v 0.94.1b
  -------------

  Bug Fixes:   
    - Fixed a bug due to which it was not possible to correctly create a System Properties asset.
    - Fixed a bug due to which the building of the project failed.

  v 0.94b
  -------------

  Bug Fixes:   
    - Fixed a bug due to which preset data was reset during baking with Bakery.
    - Fixed a bug due to which the instance of the main component was not deleted 
      after calling the "Clear All Data" option, which led to many errors.

  Improvements:
    - Added support for callbacks during blending.
    - Now the plugin can work in the lightmap switching only mode without the need for a shader patch.

  v 0.93.1b
  -------------

  Bug Fixes:   
    - Fixed a bug in the shader code that led to the overexposure of the scene in HDRP.
    - Fixed a bug that occurred when duplicating objects and repeated baking.

  Improvements:
    - Enviro support.

  v 0.93b
  -------------

  Bug Fixes:   
    - Dynamic and static component on the same object caused "Null Reference Exeption" error
    - Deleted Custom Blendable objects cause errors in the preset manager
    - Terrain was incorrectly accounted for by the system
    - Fog settings are overwritten for all presets
    - Fixed other bugs in the preset manager
    - Fixed a bug when working with a few lighting scenarios
    - Missing light probes in the scene causes an error in UpdateOperationalData
    - An error occurred while rewriting a scenario asset if the previous option did not contain Reflection Probes
    - Fixed bug when working with meshes with multiple materials

  Improvements:
    - New Stable Shader Patch System for SRP

  v 0.92b
  -------------

  Bug Fixes:
    - Preset parameters break if baking starts with the preset manager window opened.
    - Rotation values ​​for light sources are sometimes incorrectly assigned.
    - The SRP patch does not execute correctly.
    - Fixed bugs in test scenes.

  Improvements:
    - Access to settings and presets is now locked during baking.
    - New versions of the plugin can be downloaded immediately after release through the "About MLS..." window.

  v 0.91b
  -------------

  Bug Fixes:
    - Fixed a rare bug that caused the lightmap data asset to lose all settings.
    - Fixed a bug due to which objects were sometimes assigned different UIDs when changing 
      presets, resulting in an error when mixing.
    - Fixed a bug that occurred if the Terrain was not marked as static.
    - Fixed bugs in test scenes.

  Improvements:
    - The ability to save transforms of game objects to presets and, as a result, 
      synchronize them with the Global Blend value.
    - Blending of the tint color for the skybox shader is now also supported.
  
  v 0.9b
  -------------

  First release.

  Contacts
  -------------

  e-mail: evb45@bk.ru
  telegram: https://t.me/EVB45
  forum: https://forum.unity.com/threads/magic-lightmap-switcher-storing-switching-and-blending-lightmaps-in-real-time.966461/
  discord channel: https://discord.gg/p94azzE