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
[Manual of the Multi-Cut Plugin for Rhino](https://youtu.be/dQw4w9WgXcQ)

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

## Commands
Multi-Cut Plugin has two commands: ```mct``` and ```mcp```
### - mct
```mct``` stands for multi-cut.
### - mcp
```mcp``` stands for multi-cut preference. 

## Anatomy - MCP
By typing ```mcp``` in the commandline, the "Multi-Cut Preference" window will show up. 
The behavior of entire multi-cut operation could be changed through this window, including ***General, About, Assistant Line, Assistant Point, Prediction Line***.

<img align="left" alt="mcp" src="https://i0.wp.com/elderaven.com/wp-content/uploads/2022/10/mcp-setting.png?w=1158&ssl=1" width="600"> 

### - General
##### - Split When Possible / Keep Brep Joined
When the cutting polyline is closed in 3D, it is possible to split the Brep in two (or more) pieces.
By checking this, the Brep will be split if the cutting polyline is closed. 
However, if the cutting line/polyline is not closed, even checked will not split the Brep.

### - About
##### - Version
Show the current version of the Multi-Cut Plugin.
##### - Documentation
The link about this Readme file.
##### - Find More
The link of another staff which is actually a placeholder to align the hold UI panel...... 

### - Assistant Line

