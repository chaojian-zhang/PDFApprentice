# PDF Apprentice - Motivation

Uses **Windows 10 APIs** to render a PDF Document as **image** in WPF, planned to support rudimentary **text annotation** and **entity management**. This project is directly inspired by [Lander Verhack](https://blogs.u2u.be/lander/)'s post [Creating a PDF Viewer in WPF using Windows 10 APIs](https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs). The goal is to provide a simple way to handle **knowledge management** *from within a PDF context* - partly due to the modern "PDF culture" - if you know what I mean.

This project is in schedule.

# Repository Setup

Due to difference in Windows SDK installation paths new checkouts need to setup corresponding paths to two assemblies:

1. *C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.18362.0\Windows.winmd* (Needs to "Show All Files" when "Add Reference...")
2. *C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll*

Additional details is available in Lander Verhack's [post](https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs).
