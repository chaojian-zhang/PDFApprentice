# PDF Apprentice - Motivation

Uses **Windows 10 APIs** to render a PDF Document as **image** in WPF, planned to support rudimentary **text annotation** and **entity management**. This project is directly inspired by [Lander Verhack](https://blogs.u2u.be/lander/)'s post [Creating a PDF Viewer in WPF using Windows 10 APIs](https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs). The goal is to provide a simple way to handle **knowledge management** *from within a PDF context* - partly due to the modern "PDF culture" - if you know what I mean.

This project is in schedule.

# Task List

**Current**

1. Allow adding text annotations as an **entity**, each entity for now just need to contain *content*, along side optional *title* and *tags* string. Entities contain **location information** and it's relative to **owner page**. Pages are identified by **page ID**, starting from *0*, for a particular document.
	* (Implementation) Entities are shown only on page, it's edited in seperate window.

**Future Features**

1. Allow arbitary taggable page-level Ink Canvas to be associated with each page (for **highlight**, sketch and draft purpose, not for annotation purpose per-se, thus the whole thing is managed as a single entity).

# Terminology

1. Entity: All annotations, be it texts for inks, are defines as an **entity**. Each entity can contain some meta data besides its main content. All entities are confined to a particular **page**.
2. Page: Each PDF document consists of multiple pages; Page is the smallest container for information.

# Repository Setup

Due to difference in Windows SDK installation paths new checkouts need to setup corresponding paths to two assemblies:

1. *C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.18362.0\Windows.winmd* (Needs to "Show All Files" when "Add Reference...")
2. *C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll*

Additional details is available in Lander Verhack's [post](https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs).
