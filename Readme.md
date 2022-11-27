# Multi-Cut
## General
### - Introduction
Multi-Cut is a Rhino Plugin that mimics the behavior of the Multi-Cut Function in Autodesk Maya,
providing basically all core features of the original Maya version.
### - System Requirement
Rhino 7.0 and later, Windows 10/11. <br/>
Support MacOS partly*.<br/>
### - Tutorial
Here is a youtube video about how to use this plugin if you don't want to read the heavy text below.<br/>
[《 Manual of the Multi-Cut Plugin for Rhino 》](https://youtu.be/dQw4w9WgXcQ)

## Instillation
### - via Package Manager
- Open *Rhino*
- In the Command Window, type the command ```PackageManager```
- In the popped up *Package Manager* window, search ***Multi-Cut***
- Click Install
- Restart Rhino

### - via Food4Rhino
Click the link below to download via food4rhino:
<br/>
[https://www.food4rhino.com/en/app/deviant-inspector](https://www.food4rhino.com/en/app/deviant-inspector)
<br/>
<br/>

## Terminology
Several concepts, definitions, terminologies are used to form the whole Multi-Cut Plugin logic. These terminologies also help understanding of how the plugin works when modelling in Rhino.

<img alt ="mct" src="https://i0.wp.com/elderaven.com/wp-content/uploads/2022/11/mct-operation.png?w=1686&ssl=1" width="800"> 

### - Cut Line/Point
**Cut Line** is the most fundamental concept of the multi-cut operation. 
In Multi-Cut Plugin, Breps are split by the curves **ON** the Brep face of it, these curves are called "Cut Line".
Cut Line is generated by the Start Point and the End Point, both on the edges of the Brep faces, these points are called "Cut Point".
In general, by this logic, Brep is split by picking **Cut Point**s on its edges.
### - Assistant Point
**Assistant Point** is the preset points on the edges of the Brep, equally divided by the lenght of current hovered edge. 
Division number could be changed via both two embedded commands of the Plugin.
### - Assistant Line
**Assistant Line** is a bundle of pre-drawn **Cut Line**s which are common used in modelling, including iso-curves(both directions) of the surface,
the work plane (aka CPlane) intersections, and the world plane (aka world coordinates) intersections.
When enabled, pre-drawn lines will show up on the surface which is going to be cut,
with a paired end point that could be picked as a selection of its paired line.
### - Prediction Line
Not like **Assistant Line** which is pre-drawn by Plugin, **Prediction Line** is the line generated by a plane and the selected Brep, which means,
the **Prediction Line** is an intersection of a special plane and the selected Brep.
In different cut operation phase, the plane will adjust according to the cut point,
and up to 3 points ( a plane formed by 3 points ). Following scenarios show how the plane forms by different numbers of the Cut Points
<br/>
<br/>
***Cut Point = 1:***
<br/>
The plane will be defined by Origin: current cut point, Z-Axis: the tangent line on the current edge at the current point.

***Cut Point = 2:***
<br/>
The plane will be defined by Origin: current cut point, Vector 1: (current point - last point), Vector 2: World Axis-X


***Cut Point = 3:***
<br/>
The plane will be defined by 3 points that just picked from the cut operation beginning.



## Commands
Multi-Cut Plugin has two commands: ```mct``` and ```mcp```
### - command ```mct```
```mct``` stands for multi-cut which deals with the Rhino Geometry. By Typing ```mct``` in commandline, the Multi-Cut operation will begin.
### - command ```mcp```

<img align="left" alt="mcp" src="https://i0.wp.com/elderaven.com/wp-content/uploads/2022/10/mcp-setting-1.png?w=858&ssl=1" width="400"> 

```mcp``` stands for multi-cut preference which deals with the default settings used in the ```mct``` operation.
By typing ```mcp``` in the commandline, the "Multi-Cut Preference" window will show up.
The behavior of entire multi-cut operation could be changed through this window, including **General, About, Assistant Line, Assistant Point, Prediction Line**.

#### - General -
Settings placed here are the settings don't belong to any of the specific sub-category.
###### Split When Possible /  Keep Brep Joined

When the cutting polyline is closed in 3D, it is possible to split the Brep in two (or more) pieces.
By checking this, the Brep will be split if the cutting polyline is closed. 
However, if the cutting line/polyline is not closed, even checked will not split the Brep.

#### - About -
The labels and info shown here serve as placeholder to align the hole UI panel.
###### Version
Show the current version of the Multi-Cut Plugin.
###### Documentation
The link about this Readme file.
###### Find More
The link of another staff which is actually a placeholder to align the hold UI panel......

#### - Customization -
Multi-Cut Plugin uses specific colors to highlight the current operation and show what kind operation is executing, 
as well as the specific point size and line width. 
If not designated, the default color is set by the Plugin's built in settings, and the point size and line width is following the user's current display mode setting.

## Epilogue
About macOS compatibility, the Plugin never tested on macOS, but the ```mct``` command should work smoothly, and the ```mcp``` command will not work, so called "works on macOS partly".
<br/>
And one more thing...
<br/>
Indeed, when texts are somehow not easy to understand, here is a video tutorial of this Plugin. Take it easy, you won't get rick rolled this time.
<br/>
[{Manual of the Multi-Cut Plugin for Rhino}](https://youtu.be/dQw4w9WgXcQ)
<br/>
Enjoy!








